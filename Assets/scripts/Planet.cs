using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct NoiseLayer
{
    public enum FilterType { Simple, Rigid };
    public FilterType filterType;
    
    [Header("noise Settings")]
    [Range(0.0f, 10.0f)]
    public float roghness;
    [Range(0.0f, 10.0f)]
    public float baseRoghness;
    [Range(0.0f, 10.0f)]
    public float strength;
    public Vector3 center;
    [Range(1, 8)]
    public int nbLayers;
    [Range(0.0f, 10.0f)]
    public float persistence;
    public float minValue;

    public bool enabled;
    public bool useFirstLayerAsMask;

}

public class Planet : MonoBehaviour
{
    [SerializeField, HideInInspector]
    MeshFilter meshFilter;

    [SerializeField, Range(1.0f, 100.0f)]
    private float radius = 1;

    private MeshDataStructure meshDS;

    //public bool subdivide;
    public bool changingValues;

    public NoiseLayer[] noiseLayers;

    [Header("Color Settings")]
    [SerializeField]
    private Material planetMaterial;
    public BiomeColorSettings biomeColorSettings;
    public Gradient oceanColor;
    private Texture2D texture;
    private const int textureResolution = 50;

    // Start is called before the first frame update

    private void Start()
    {  
        Initialize();
        radius = 1;
        Vector3[] initialVertices = {
            new Vector3(-1, -1, 1),
            new Vector3(-1, -1, -1),
            new Vector3(-1, 1, -1),
            new Vector3(-1, 1, 1),
            new Vector3(1, -1, 1),
            new Vector3(1f, -1, -1),
            new Vector3(1, 1, -1),
            new Vector3(1, 1, 1)
        };

        int[] initialQuads = {
            3, 2, 1, 0,
            2, 3, 7, 6,
            6, 7, 4, 5,
            5, 4, 0, 1,
            7, 3, 0, 4,
            2, 6, 5, 1
        };


        meshDS = new MeshDataStructure(initialVertices, initialQuads);

        meshDS.subdivide(5);

        Vector3[] vertices = meshDS.getVertices(radius, noiseLayers, biomeColorSettings);
        int[] triangles = meshDS.getTriangles();
        Vector2[] uv = meshDS.getUVs();

        UpdateShader();
        Mesh mesh = meshFilter.sharedMesh;
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.uv = uv;
    }

    private void OnValidate()
    {
        if (changingValues)
            GenerateMesh();
    }

    void Initialize()
    {
        texture = new Texture2D(textureResolution * 2, biomeColorSettings.biomes.Length, TextureFormat.RGBA32, false);

        if (meshFilter == null)
        {
            GameObject meshObj = new GameObject("mesh");
            meshObj.transform.parent = transform;
            meshObj.transform.localPosition = Vector3.zero;
            meshObj.transform.localRotation = Quaternion.identity;

            meshObj.AddComponent<MeshRenderer>().sharedMaterial = planetMaterial;
            meshFilter = meshObj.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = new Mesh();
        }
        
    }

    void GenerateMesh()
    {
        Vector3[] vertices = meshDS.getVertices(radius, noiseLayers, biomeColorSettings);
        int[] triangles = meshDS.getTriangles();
        Vector2[] uv = meshDS.getUVs();
        
        UpdateShader();

        Mesh mesh = meshFilter.sharedMesh;
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.uv = uv;

    }

    void UpdateShader()
    {
        planetMaterial.SetVector("_elevationMinMax", new Vector4(meshDS.elevationMinMax.Min, meshDS.elevationMinMax.Max));
        if (texture == null || (texture.height != biomeColorSettings.biomes.Length))
            texture = new Texture2D(textureResolution * 2, biomeColorSettings.biomes.Length, TextureFormat.RGBA32, false);

        Color[] colors = new Color[texture.width * texture.height];
        int colorIndex = 0;
        
        foreach (var biome in biomeColorSettings.biomes)
        {
            for (int i = 0; i < textureResolution * 2; i++)
            {
                Color gradientColor;
                if (i < textureResolution)
                {
                    gradientColor = oceanColor.Evaluate(i / (textureResolution - 1f));

                }
                else
                {
                    gradientColor = biome.gradient.Evaluate((i - textureResolution) / (textureResolution - 1f));
                }
                Color tintColor = biome.tint;
                colors[colorIndex] = gradientColor * (1 - biome.tintPercent) + tintColor * biome.tintPercent;
                colorIndex++;
            }
        }
       
        texture.SetPixels(colors);
        texture.Apply();
        planetMaterial.SetTexture("_gradient", texture);
    }
}

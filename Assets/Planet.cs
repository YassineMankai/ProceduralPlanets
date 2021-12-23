using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct NoiseLayer
{
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

    [SerializeField]
    private Material customMaterial;
    
    [SerializeField, Range(1.0f, 10.0f)]
    private float radius = 1;

    private MeshDataStructure meshDS;

    public bool subdivide;
    public bool changingValues;

    public NoiseLayer[] noiseLayers;


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

        Vector3[] vertices = meshDS.getVertices(radius, noiseLayers);
        int[] triangles = meshDS.getTriangles();

        Mesh mesh = meshFilter.sharedMesh;
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }


    private void Update()
    {
        if (subdivide)
        {
            meshDS.subdivide(6);
            GenerateMesh();
            subdivide = false;
        }
    }

    private void OnValidate()
    {
        if (changingValues)
            GenerateMesh();
    }

    void Initialize()
    {
        if (meshFilter == null)
        {
            GameObject meshObj = new GameObject("mesh");
            meshObj.transform.parent = transform;
            meshObj.transform.localPosition = Vector3.zero;
            meshObj.transform.localRotation = Quaternion.identity;

            meshObj.AddComponent<MeshRenderer>().sharedMaterial = customMaterial;
            meshFilter = meshObj.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = new Mesh();
        }
        
    }

    void GenerateMesh()
    {
        Vector3[] vertices = meshDS.getVertices(radius, noiseLayers);
        int[] triangles = meshDS.getTriangles();

        Mesh mesh = meshFilter.sharedMesh;
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

    }
}

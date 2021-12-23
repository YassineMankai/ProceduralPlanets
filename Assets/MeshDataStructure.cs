using System.Collections.Generic;
using UnityEngine;
using System;

namespace UnityEngine
{
    public struct Vector4Int
    {
        private int m_X;
        private int m_Y;
        private int m_Z;
        private int m_W;


        public int x { get { return m_X; } set { m_X = value; } }
        public int y { get { return m_Y; } set { m_Y = value; } }
        public int z { get { return m_Z; } set { m_Z = value; } }
        public int w { get { return m_W; } set { m_W = value; } }



        public Vector4Int(int _x, int _y, int _z, int _w)
        {
            m_X = _x;
            m_Y = _y;
            m_Z = _z;
            m_W = _w;
        }

        public int this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0: return x;
                    case 1: return y;
                    case 2: return z;
                    case 3: return w;
                    default:
                        throw new IndexOutOfRangeException($"Invalid Vector4Int index addressed: {index}!");
                }
            }

            set
            {
                switch (index)
                {
                    case 0: x = value; break;
                    case 1: y = value; break;
                    case 2: z = value; break;
                    case 3: w = value; break;

                    default:
                        throw new IndexOutOfRangeException($"Invalid Vector4Int index addressed: {index}!");
                }
            }
        }

        public static Vector4Int operator +(Vector4Int a, Vector4Int b)
        {
            return new Vector4Int(a.x + b.x, a.y + b.y, a.z + b.z, a.w + b.w);
        }

        public static Vector4Int operator -(Vector4Int a, Vector4Int b)
        {
            return new Vector4Int(a.x - b.x, a.y - b.y, a.z - b.z, a.w - b.w);
        }

        public static Vector4Int operator -(Vector4Int a)
        {
            return new Vector4Int(-a.x, -a.y, -a.z, -a.w);
        }

        public static bool operator ==(Vector4Int lhs, Vector4Int rhs)
        {
            return lhs.x == rhs.x && lhs.y == rhs.y && lhs.z == rhs.z && lhs.w == rhs.w;
        }

        public static bool operator !=(Vector4Int lhs, Vector4Int rhs)
        {
            return !(lhs == rhs);
        }

        public override bool Equals(object other)
        {
            if (!(other is Vector4Int)) return false;

            return Equals((Vector4Int)other);
        }

        public bool Equals(Vector4Int other)
        {
            return this == other;
        }

        public override int GetHashCode()
        {
            var yHash = y.GetHashCode();
            var zHash = z.GetHashCode();
            var wHash = w.GetHashCode();
            return x.GetHashCode() ^ (yHash << 4) ^ (yHash >> 28) ^ (zHash >> 4) ^ (zHash << 28) ^ (wHash >> 4) ^ (wHash << 28);
        }

        public override string ToString()
        {
            return $"{x} {y} {z} {w}";
        }

    }

}
public class MeshDataStructure
{
    public SimpleNoiseFilter noiseFilter;

    private List<Vector3> verticesDS;
    private List<Vector4Int> quadsDS;
    private List<HashSet<int>> vertexToFaces;
    private List<HashSet<Vector2Int>> vertexToEdge;

    public MeshDataStructure(Vector3[] initialVertices, int[] initialQuads)
    {
        noiseFilter = new SimpleNoiseFilter();
        verticesDS = new List<Vector3>();
        quadsDS = new List<Vector4Int>();
        vertexToFaces = new List<HashSet<int>>();
        vertexToEdge = new List<HashSet<Vector2Int>>();

        for (int i = 0; i < initialVertices.Length; i++)
        {
            verticesDS.Add(initialVertices[i]);
            vertexToFaces.Add(new HashSet<int>());
            vertexToEdge.Add(new HashSet<Vector2Int>());
        }

        for (int i = 0; i < initialQuads.Length; i += 4)
        {
            Vector4Int face = new Vector4Int(initialQuads[i], initialQuads[i + 1], initialQuads[i + 2], initialQuads[i + 3]);

            for (int v = 0; v < 4; v++)
            {
                vertexToFaces[face[v]].Add(quadsDS.Count);

                Vector2Int edgeD = new Vector2Int(face[v], face[(v + 1) % 4]);
                Vector2Int edgeI = new Vector2Int(face[(v + 1) % 4], face[v]);

                vertexToEdge[face[v]].Add(edgeD);
                vertexToEdge[face[v]].Add(edgeI);

                vertexToEdge[face[(v + 1) % 4]].Add(edgeD);
                vertexToEdge[face[(v + 1) % 4]].Add(edgeI);

            }
            quadsDS.Add(face);
        }
    }

    public void updtateCurrentMaps()
    {
        vertexToFaces = new List<HashSet<int>>();
        vertexToEdge = new List<HashSet<Vector2Int>>();

        for (int i = 0; i < verticesDS.Count; i++)
        {
            vertexToFaces.Add(new HashSet<int>());
            vertexToEdge.Add(new HashSet<Vector2Int>());
        }

        for (int i = 0; i < quadsDS.Count; i ++)
        {
            Vector4Int face = quadsDS[i];

            for (int v = 0; v < 4; v++)
            {
                vertexToFaces[face[v]].Add(i);

                Vector2Int edgeD = new Vector2Int(face[v], face[(v + 1) % 4]);
                Vector2Int edgeI = new Vector2Int(face[(v + 1) % 4], face[v]);

                vertexToEdge[face[v]].Add(edgeD);
                vertexToEdge[face[v]].Add(edgeI);

                vertexToEdge[face[(v + 1) % 4]].Add(edgeD);
                vertexToEdge[face[(v + 1) % 4]].Add(edgeI);
            }
        }
    }

    public void subdivide()
    {
        updtateCurrentMaps();


        int nbOriginal = verticesDS.Count;
        int[,] edgeToEdgePoint = new int[verticesDS.Count, verticesDS.Count];
        int[] faceToFacePoint = new int[quadsDS.Count];

        // add facePoint  && edgmepoint
        for (int faceIndex = 0; faceIndex < quadsDS.Count; faceIndex++)
        {
            // add facePoint  
            Vector4Int face = quadsDS[faceIndex];
            Vector3 avg = (verticesDS[face.x] + verticesDS[face.y]
                           + verticesDS[face.z] + verticesDS[face.w]) / 4;
            faceToFacePoint[faceIndex] = verticesDS.Count;

            verticesDS.Add(avg);
            vertexToFaces.Add(new HashSet<int>());
            vertexToEdge.Add(new HashSet<Vector2Int>());

            // update edgePoint
            for (int v = 0; v < 4; v++)
            {
                Vector2Int edge = new Vector2Int(face[v], face[(v + 1) % 4]);
                if (edgeToEdgePoint[edge.x, edge.y] == 0)
                {
                    edgeToEdgePoint[edge.x, edge.y] = verticesDS.Count;
                    edgeToEdgePoint[edge.y, edge.x] = verticesDS.Count;
                    verticesDS.Add(avg);
                    vertexToFaces.Add(new HashSet<int>());
                    vertexToEdge.Add(new HashSet<Vector2Int>());
                }
                else
                {
                    verticesDS[edgeToEdgePoint[edge.x, edge.y]] += avg + verticesDS[edge.x] + verticesDS[edge.y];
                    verticesDS[edgeToEdgePoint[edge.x, edge.y]] /= 4;
                }
            }
        }


        // update og points
        for (int vOriginal = 0; vOriginal < nbOriginal; vOriginal++)
        {
            Vector3 avgFacePoints = Vector3.zero;
            int nbFaces = vertexToFaces[vOriginal].Count;

            foreach (int face in vertexToFaces[vOriginal])
            {
                avgFacePoints += verticesDS[faceToFacePoint[face]];
            }
            avgFacePoints /= nbFaces;

            Vector3 avgEdgeMidpoints = Vector3.zero;

            foreach (Vector2Int edge in vertexToEdge[vOriginal])
            {
                Vector3 midPoint = (verticesDS[edge.x] + verticesDS[edge.y]) / 2;
                // take into consideration duplicates
                avgEdgeMidpoints += midPoint / 2;
            }
            avgEdgeMidpoints /= nbFaces;
            verticesDS[vOriginal] = avgFacePoints + 2 * avgEdgeMidpoints + (nbFaces - 3) * verticesDS[vOriginal];
            verticesDS[vOriginal] /= nbFaces;
        }


        // connect
        List<Vector4Int> newquadsDS = new List<Vector4Int>();
        for (int faceindex = 0; faceindex < quadsDS.Count; faceindex++)
        {
            Vector4Int quad = quadsDS[faceindex];
            int facePoint = faceToFacePoint[faceindex];

            int edgePoint1 = edgeToEdgePoint[quad.x, quad.y];

            int edgePoint2 = edgeToEdgePoint[quad.y, quad.z];
            int edgePoint3 = edgeToEdgePoint[quad.z, quad.w];
            int edgePoint4 = edgeToEdgePoint[quad.w, quad.x];

            newquadsDS.Add(new Vector4Int(quad.x, edgePoint1, facePoint, edgePoint4));
            newquadsDS.Add(new Vector4Int(edgePoint1, quad.y, edgePoint2, facePoint));
            newquadsDS.Add(new Vector4Int(facePoint, edgePoint2, quad.z, edgePoint3));
            newquadsDS.Add(new Vector4Int(edgePoint4, facePoint, edgePoint3, quad.w));
        }
        quadsDS = newquadsDS;

    }

    public void subdivide(int level)
    {
        while (level > 0)
        {
            level--;
            subdivide();
        }
    }

    public int[] getTriangles()
    {
        int[] triangles = new int[quadsDS.Count * 2 * 3];
        int index = 0;
        foreach (Vector4Int quad in quadsDS)
        {
            triangles[index] = quad.x;
            triangles[index + 1] = quad.y;
            triangles[index + 2] = quad.z;

            triangles[index + 3] = quad.z;
            triangles[index + 4] = quad.w;
            triangles[index + 5] = quad.x;

            index += 6;
        }


        return triangles;
    }
    public Vector3[] getVertices(float radius, NoiseLayer[] noiseLayers)
    {
        Vector3[] vertices = new Vector3[verticesDS.Count];
        for (int i = 0; i < verticesDS.Count; i++)
        {
            Vector3 PointOnUnitSphere = verticesDS[i].normalized;

            float firstLayerValue = 0;
            float elevation = 0;

            if (noiseLayers.Length > 0)
            {
                firstLayerValue = noiseFilter.Evaluate(PointOnUnitSphere, noiseLayers[0]);
                if (noiseLayers[0].enabled)
                {
                    elevation += firstLayerValue;
                }
            }

            for (int layerIndex = 1; layerIndex < noiseLayers.Length; layerIndex++)
            {
                NoiseLayer noiseLayer = noiseLayers[layerIndex];
                if (noiseLayer.enabled)
                {
                    float mask = noiseLayer.useFirstLayerAsMask ? firstLayerValue : 1;
                    elevation += noiseFilter.Evaluate(PointOnUnitSphere, noiseLayer) * mask;
                }
            }

            vertices[i] = PointOnUnitSphere * radius * (1+elevation);
        }
        return vertices;
    }
}

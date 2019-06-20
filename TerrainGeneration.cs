using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGeneration : MonoBehaviour {
    [Header("Scale")]
    public float scale = 10;
    public float heightMultiplier = 4;
    [Header("Materials")]
    public Material primaryMaterial;
    [Header("Details")]
    public GameObject[] details;// Detail objects

    [Header("Texture Maps")]
    public Texture2D heightMap;// Black-White heightMap
    public Texture2D detailMap;// Black-White detail map
    
    private Vector3 vertexOffset = Vector3.zero;// Is used to match the center of the mesh and the transform pivot

    /*
     * This is the main function
     * Creates a terrain mesh
     * Adds details
     * 
     * Sets up GameObjects
     */
    public void GenerateTerrain()
    {
        int width = heightMap.width - 1;  // for convenience
        int height = heightMap.height - 1;
        vertexOffset = new Vector3(-width / 2f, 0, -height / 2f);

        for (int I = 0; I < heightMap.width / 64; ++I)
        {
            for (int J = 0; J < heightMap.height / 64; ++J)
            {
                List<Vector3> grassVerts = new List<Vector3>();
                List<int> grassTris = new List<int>();

                GameObject terrainChunk = new GameObject("TerrainChunk");
                terrainChunk.transform.parent = transform;
                
                for (int i = 0; i < 63; ++i)
                {
                    for (int j = 0; j < 63; ++j)
                    {
                        // First Triangle
                        VertexSetup(grassVerts, grassTris, i + I * 63, j + 1 + J * 63);
                        VertexSetup(grassVerts, grassTris, i + 1 + I * 63, j + 1 + J * 63);
                        VertexSetup(grassVerts, grassTris, i + I * 63, j + J * 63);

                        // Second Triangle
                        VertexSetup(grassVerts, grassTris, i + I * 63, j + J * 63);
                        VertexSetup(grassVerts, grassTris, i + 1 + I * 63, j + 1 + J * 63);
                        VertexSetup(grassVerts, grassTris, i + 1 + I * 63, j + J * 63);

                        // Details
                        if (detailMap != null && details.Length > 0 && detailMap.GetPixel(i + I * 63, j + J * 63).r > 0.7f)
                        {
                            DetailSetup(i + I * 63, j + J * 63, terrainChunk.transform);
                        }
                    }
                }

                GameObjectSetup(terrainChunk, primaryMaterial, grassVerts, grassTris);
            }
        }
    }

    // Gets the position of [i,j] point of the heightMap
    Vector3 TerrainPoint(int i, int j)
    {
        Vector3 result = new Vector3(i, 0, j) + 
                            (Vector3.up * heightMap.GetPixel(i, j).r * heightMultiplier) + vertexOffset;
        result *= scale;
        return result;
    }

    // Adds a new Vertex
    void VertexSetup(List<Vector3> verts, List<int> tris, int i, int j)
    {
        verts.Add(TerrainPoint(i,j));
        tris.Add(tris.Count);
    }

    // Adds a detail onto the terrain
    void DetailSetup(int i, int j,Transform parent)
    {
        Vector2 offsetBase = new Vector2(Random.value, Random.value) * 0.75f;
        Vector3 x;
        Vector3 y;
        if (offsetBase.x > offsetBase.y)
        {
            x = Vector3.Lerp(TerrainPoint(i, j), TerrainPoint(i + 1, j), offsetBase.x);
            y = Vector3.Lerp(TerrainPoint(i + 1, j), TerrainPoint(i + 1, j + 1), offsetBase.y);
        }
        else
        {
            x = Vector3.Lerp(TerrainPoint(i, j + 1), TerrainPoint(i + 1, j + 1), offsetBase.x);
            y = Vector3.Lerp(TerrainPoint(i, j), TerrainPoint(i, j + 1), offsetBase.y);
        }
        Vector3 finalPosition = Vector3.Lerp(x, y, 0.5f);
        GameObject tree = Instantiate(details[Random.Range(0, 1000) % details.Length], finalPosition + Vector3.down * 0.1f + transform.position.y * Vector3.up, Quaternion.Euler(0, Random.Range(0, 359), 0),transform);
        tree.transform.parent = parent;
    }

    // Adds MeshFilter(with the mesh itself),MeshRenderer and MeshCollider onto a GameObject
    void GameObjectSetup(GameObject targetGameObject, Material material, List<Vector3> verts, List<int> tris)
    {
        Mesh mesh = new Mesh
        {
            vertices = verts.ToArray(),
            triangles = tris.ToArray()
        };

        mesh.RecalculateNormals();
        mesh.RecalculateTangents();


        MeshFilter meshFilter = targetGameObject.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = targetGameObject.AddComponent<MeshRenderer>();
        MeshCollider meshCollider = targetGameObject.AddComponent<MeshCollider>();

        meshFilter.sharedMesh = mesh;
        meshRenderer.material = material;
        meshCollider.sharedMesh = mesh;
    }
    
    //Destroys all Mesh-related Components and all children
    public void ClearTerrain()
    {
        if (gameObject.GetComponent<MeshFilter>() && gameObject.GetComponent<MeshRenderer>())
        {
            DestroyImmediate(gameObject.GetComponent<MeshFilter>().sharedMesh);
            DestroyImmediate(gameObject.GetComponent<MeshRenderer>());
            DestroyImmediate(gameObject.GetComponent<MeshFilter>());
            DestroyImmediate(gameObject.GetComponent<MeshCollider>());
        }
        do
        {
            for (int i = 0; i < transform.childCount; ++i)
            {
                DestroyImmediate(transform.GetChild(i).gameObject);
            }
        }
        while (transform.childCount > 0);
    }
}

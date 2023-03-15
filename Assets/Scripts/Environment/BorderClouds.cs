using Sirenix.OdinInspector;
using Sirenix.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[ExecuteAlways]
public class BorderClouds : MonoBehaviour
{
    public World world;
    public int borderOffset;
    public float borderHeight = 20;
    [Header("Debug")]
    [ReadOnly] public int xSize;
    [ReadOnly] public int zSize;
    [ReadOnly] public int bSize;

    private Mesh mesh;
    private Vector3[] vertices;
    private int[] triangles;
    private Vector2[] uvs;
    private Color[] colors;

    private void Start()
    {
        MaterialPropertyBlock mpb = new MaterialPropertyBlock();
        mpb.SetVector("_Size", new Vector2(xSize, zSize));
        GetComponent<Renderer>().SetPropertyBlock(mpb);
    }

    [Button]
    private void GenerateClouds()
    {
        if (!world)
            throw new System.Exception("No world reference is assigned!");

        xSize = world.chunkSize * world.mapSizeInChunks;
        zSize = world.chunkSize * world.mapSizeInChunks;
        bSize = world.border - borderOffset;

        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        // Calculate mesh vertices
        vertices = new Vector3[((xSize + 1) * (zSize + 1)) - ((zSize + 1 - 2 * (bSize + 1)) * (xSize + 1 - 2 * (bSize + 1)))];

        for (int i = 0, z = 0; z <= zSize; z++)
        {
            for (int x = 0; x <= xSize; x++)
            {
                if (z > bSize
                    && z < zSize - bSize
                    && x > bSize
                    && x < xSize - bSize)
                    continue;
                vertices[i] = new Vector3(x, borderHeight, z);
                i++;
            }
        }
        
        // Calculate mesh triangles
        triangles = new int[((xSize * zSize) - ((zSize - 2 * bSize) * (xSize - 2 * bSize))) * 6];

        int vert = 0;
        int tris = 0;
        
        // Bottom section
        for (int z = 0; z < bSize; z++)
        {
            for (int x = 0; x < xSize; x++)
            {
                triangles[tris + 0] = vert + 0;
                triangles[tris + 1] = vert + xSize + 1;
                triangles[tris + 2] = vert + 1;
                triangles[tris + 3] = vert + 1;
                triangles[tris + 4] = vert + xSize + 1;
                triangles[tris + 5] = vert + xSize + 2;

                vert++;
                tris += 6;
            }
            vert++;
        }

        // Side sections
        int offset = 0;

        for (int z = 0; z < zSize - 2 * bSize; z++)
        {
            for (int x = 0; x < bSize * 2; x++)
            {
                if (z == 0) // First row is different
                {
                    triangles[tris + 0] = vert + 0;
                    triangles[tris + 1] = vert + xSize + offset + 1;
                    triangles[tris + 2] = vert + 1;
                    triangles[tris + 3] = vert + 1;
                    triangles[tris + 4] = vert + xSize + offset + 1;
                    triangles[tris + 5] = vert + xSize + offset + 2;

                    vert++;
                    tris += 6;

                    if ((vert - ((bSize) * (xSize + 1) - 1)) % (bSize + 1) == 0 && x < bSize)
                    {
                        offset = -(xSize - 2 * bSize - 1);
                        vert += xSize - 2 * bSize;
                    }
                }
                else
                {
                    triangles[tris + 0] = vert + 0;
                    triangles[tris + 1] = vert + xSize + offset + 1;
                    triangles[tris + 2] = vert + 1;
                    triangles[tris + 3] = vert + 1;
                    triangles[tris + 4] = vert + xSize + offset + 1;
                    triangles[tris + 5] = vert + xSize + offset + 2;

                    vert++;
                    tris += 6;

                    if ((vert - ((bSize + 1) * (xSize + 1) - 1)) % (bSize + 1) == 0 && x < bSize)
                    {
                        if (z == zSize - 2 * bSize - 1)
                            offset = 0;
                        vert++;
                    }
                }
            }
            vert++;
        }

        // Top section
        for (int z = 0; z < bSize; z++)
        {
            for (int x = 0; x < xSize; x++)
            {
                triangles[tris + 0] = vert + 0;
                triangles[tris + 1] = vert + xSize + 1;
                triangles[tris + 2] = vert + 1;
                triangles[tris + 3] = vert + 1;
                triangles[tris + 4] = vert + xSize + 1;
                triangles[tris + 5] = vert + xSize + 2;

                vert++;
                tris += 6;
            }
            vert++;
        }

        // Calculate UVs
        uvs = new Vector2[vertices.Length];

        for (int i = 0, z = 0; z <= zSize; z++)
        {
            for (int x = 0; x <= xSize; x++)
            {
                if (z > bSize
                    && z < zSize - bSize
                    && x > bSize
                    && x < xSize - bSize)
                    continue;

                uvs[i] = new Vector2((float)x / xSize, (float)z / zSize);
                i++;
            }
        }

        // Calculate Vertex Color
        colors = new Color[vertices.Length];

        for (int i = 0, z = 0; z <= zSize; z++)
        {
            for (int x = 0; x <= xSize; x++)
            {
                if (z > bSize
                    && z < zSize - bSize
                    && x > bSize
                    && x < xSize - bSize)
                    continue;

                // Set color to black if on the border, otherwise it will be white
                colors[i] = (((x == bSize || x == xSize - bSize) && (z >= bSize && z <= zSize - bSize))
                            || ((z == bSize || z == zSize - bSize) && (x >= bSize && x <= xSize - bSize))) 
                            ? Color.black : Color.white;
                i++;
            }
        }

        // Finalize mesh
        mesh.Clear();

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.colors = colors;

        mesh.RecalculateNormals();

        MaterialPropertyBlock mpb = new MaterialPropertyBlock();
        mpb.SetVector("_Size", new Vector2(xSize, zSize));
        GetComponent<Renderer>().SetPropertyBlock(mpb);
    }

    /*private void OnDrawGizmos()
    {
        if (vertices != null)
            vertices.ForEach(v => Gizmos.DrawSphere(v, .1f));
    }*/
}

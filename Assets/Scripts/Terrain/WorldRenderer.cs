using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class WorldRenderer : MonoBehaviour
{
    public World worldReference;
    public GameObject chunkPrefab;
    public Queue<ChunkRenderer> chunkPool = new Queue<ChunkRenderer>();

    public void Clear(WorldData worldData)
    {
        foreach (var item in worldReference.chunkDictionary.Values)
        {
            #if UNITY_EDITOR
            DestroyImmediate(item.gameObject);
            #else
            Destroy(item.gameObject);
            #endif

        }
        chunkPool.Clear();
    }

    public void DeleteRenderers()
    {
        if (chunkPool == null || chunkPool.Count() <= 0)
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                #if UNITY_EDITOR
                DestroyImmediate(transform.GetChild(i).gameObject);
                #else
                Destroy(transform.GetChild(i).gameObject);
                #endif
            }
    }

    internal ChunkRenderer RenderChunk(World world, WorldData worldData, Vector3Int position, MeshData meshData)
    {
        ChunkRenderer newChunk = null;

        GameObject chunkObject = Instantiate(chunkPrefab, position, Quaternion.identity, transform);
        chunkObject.name = "Chunk" + position;
        newChunk = chunkObject.GetComponent<ChunkRenderer>();

        newChunk.InitializeChunk(worldData.chunkDataDictionary[position]);
        newChunk.worldReference = world;
        newChunk.UpdateChunk(meshData);
        newChunk.gameObject.SetActive(true);
        return newChunk;
    }

    public void RemoveChunk(ChunkRenderer chunk)
    {
        chunk.gameObject.SetActive(false);
        chunkPool.Enqueue(chunk);
    }
}
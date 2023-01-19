using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class WorldDataHelper
{
    public static Vector3Int ChunkPositionFromBlockCoords(World world, Vector3Int worldBlockPosition)
    {
        return new Vector3Int
        {
            x = Mathf.FloorToInt(worldBlockPosition.x / (float)world.chunkSize) * world.chunkSize,
            y = Mathf.FloorToInt(worldBlockPosition.y / (float)world.chunkHeight) * world.chunkHeight,
            z = Mathf.FloorToInt(worldBlockPosition.z / (float)world.chunkSize) * world.chunkSize
        };
    }

    internal static List<Vector3Int> GetChunkPositionsAroundOrigin(World world)
    {
        int startX = 0;
        int startZ = 0;
        int endX = (world.mapSizeInChunks - 1) * world.chunkSize;
        int endZ = (world.mapSizeInChunks - 1) * world.chunkSize;

        List<Vector3Int> chunkPositionsToCreate = new List<Vector3Int>();
        for (int x = startX; x <= endX; x += world.chunkSize)
        {
            for (int z = startZ; z <= endZ; z += world.chunkSize)
            {
                Vector3Int chunkPos = ChunkPositionFromBlockCoords(world, new Vector3Int(x, 0, z));
                chunkPositionsToCreate.Add(chunkPos);
            }
        }

        return chunkPositionsToCreate;
    }

    internal static void RemoveChunkData(World world, Vector3Int pos)
    {
        world.worldData.chunkDataDictionary.Remove(pos);
    }

    internal static void RemoveChunk(World world, Vector3Int pos)
    {
        ChunkRenderer chunk = null;
        if (world.chunkDictionary.TryGetValue(pos, out chunk))
        {
            world.worldRenderer.RemoveChunk(chunk);
            world.chunkDictionary.Remove(pos);
        }
    }

    internal static List<Vector3Int> GetDataPositionsAroundOrigin(World world)
    {
        int startX = 0;
        int startZ = 0;
        int endX = (world.mapSizeInChunks - 1) * world.chunkSize;
        int endZ = (world.mapSizeInChunks - 1) * world.chunkSize;

        List<Vector3Int> chunkDataPositionsToCreate = new List<Vector3Int>();
        for (int x = startX; x <= endX; x += world.chunkSize)
        {
            for (int z = startZ; z <= endZ; z += world.chunkSize)
            {
                Vector3Int chunkPos = ChunkPositionFromBlockCoords(world, new Vector3Int(x, 0, z));
                chunkDataPositionsToCreate.Add(chunkPos);
            }
        }

        return chunkDataPositionsToCreate;
    }

    internal static ChunkRenderer GetChunk(World worldReference, Vector3Int worldPosition)
    {
        if (worldReference.chunkDictionary.ContainsKey(worldPosition))
            return worldReference.chunkDictionary[worldPosition];
        return null;
    }

    internal static BlockType GetBlock(World worldReference, Vector3Int worldBlockPosition)
    {
        ChunkData chunkData = GetChunkData(worldReference, worldBlockPosition);
        if (chunkData != null)
        {
            Vector3Int localPosition = Chunk.GetBlockInChunkCoordinates(chunkData, worldBlockPosition);
            return Chunk.GetBlockFromChunkCoordinates(worldReference, chunkData, localPosition);
        }
        return BlockType.Nothing;
    }

    internal static void SetBlock(World worldReference, Vector3Int worldBlockPosition, BlockType blockType)
    {
        ChunkData chunkData = GetChunkData(worldReference, worldBlockPosition);
        if (chunkData != null)
        {
            Vector3Int localPosition = Chunk.GetBlockInChunkCoordinates(chunkData, worldBlockPosition);
            Chunk.SetBlock(worldReference, chunkData, localPosition, blockType);
        }
    }

    public static ChunkData GetChunkData(World worldReference, Vector3Int worldBlockPosition)
    {
        Vector3Int chunkPosition = ChunkPositionFromBlockCoords(worldReference, worldBlockPosition);

        ChunkData containerChunk = null;

        worldReference.worldData.chunkDataDictionary.TryGetValue(chunkPosition, out containerChunk);

        return containerChunk;
    }

    internal static List<Vector3Int> GetUnnededData(WorldData worldData, List<Vector3Int> allChunkDataPositionsNeeded)
    {
        return worldData.chunkDataDictionary.Keys
    .Where(pos => allChunkDataPositionsNeeded.Contains(pos) == false && worldData.chunkDataDictionary[pos].modifiedByThePlayer == false)
    .ToList();

    }

    internal static List<Vector3Int> GetUnnededChunks(World worldReference, WorldData worldData, List<Vector3Int> allChunkPositionsNeeded)
    {
        List<Vector3Int> positionToRemove = new List<Vector3Int>();
        foreach (var pos in worldReference.chunkDictionary.Keys
            .Where(pos => allChunkPositionsNeeded.Contains(pos) == false))
        {
            if (worldReference.chunkDictionary.ContainsKey(pos))
            {
                positionToRemove.Add(pos);

            }
        }

        return positionToRemove;
    }

    internal static List<Vector3Int> SelectPositonsToCreate(World worldReference, WorldData worldData, List<Vector3Int> allChunkPositionsNeeded, Vector3Int center)
    {
        return allChunkPositionsNeeded
            .Where(pos => worldReference.chunkDictionary.ContainsKey(pos) == false)
            .OrderBy(pos => Vector3.Distance(center, pos))
            .ToList();
    }

    internal static List<Vector3Int> SelectDataPositonsToCreate(WorldData worldData, List<Vector3Int> allChunkDataPositionsNeeded, Vector3Int center)
    {
        return allChunkDataPositionsNeeded
            .Where(pos => worldData.chunkDataDictionary.ContainsKey(pos) == false)
            .OrderBy(pos => Vector3.Distance(center, pos))
            .ToList();
    }
}

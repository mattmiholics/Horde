using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BiomeGenerator : MonoBehaviour
{
    public int waterThreshold = 50;

    public NoiseSettings biomeNoiseSettings;

    public DomainWarping domainWarping;

    public bool useDomainWarping = true;

    public BlockLayerHandler startLayerHandler;

    public List<BlockLayerHandler> additionalLayerHandlers;

    public ChunkData ProcessChunkColumn(World worldReference, ChunkData data, int x, int z, Vector2Int mapSeedOffset, int? terrainHeightNoise)
    {
        biomeNoiseSettings.worldOffset = mapSeedOffset;

        int groundPosition;
        if (terrainHeightNoise.HasValue == false)
            groundPosition = GetSurfaceHeightNoise(data.worldPosition.x + x, data.worldPosition.z + z, data.chunkHeight);
        else
            groundPosition = terrainHeightNoise.Value;

        for (int y = data.worldPosition.y; y < data.worldPosition.y + data.chunkHeight; y++)
        {
            startLayerHandler.Handle(worldReference, data, x, y, z, groundPosition, mapSeedOffset);
        }

        foreach (var layer in additionalLayerHandlers)
        {
            layer.Handle(worldReference, data, x, data.worldPosition.y, z, groundPosition, mapSeedOffset);
        }
        return data;
    }

    public int GetSurfaceHeightNoise(int x, int z, int chunkHeight)
    {
        float terrainHeight;
        if (useDomainWarping == false)
        {
            terrainHeight = MyNoise.OctavePerlin(x, z, biomeNoiseSettings);
        }
        else
        {
            terrainHeight = domainWarping.GenerateDomainNoise(x, z, biomeNoiseSettings);
        }
        terrainHeight = MyNoise.Redistribution(terrainHeight, biomeNoiseSettings);
        int surfaceHeight = MyNoise.RemapValue01ToInt(terrainHeight, 0, chunkHeight - 1); //subtract the offset
        surfaceHeight += 1; //add offset back
        return surfaceHeight;
    }
}

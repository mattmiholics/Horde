using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BlockLayerHandler : MonoBehaviour
{
    [SerializeField]
    private BlockLayerHandler Next;

    public bool Handle(World worldReference, ChunkData chunkData, int x, int y, int z, int surfaceHeightNoise, Vector2Int mapSeedOffset)
    {
        if (TryHandling(worldReference, chunkData, x, y, z, surfaceHeightNoise, mapSeedOffset))
            return true;
        if (Next != null)
            return Next.Handle(worldReference, chunkData, x, y, z, surfaceHeightNoise, mapSeedOffset);
        return false;
    }

    protected abstract bool TryHandling(World worldReference, ChunkData chunkData, int x, int y, int z, int surfaceHeightNoise, Vector2Int mapSeedOffset);
}

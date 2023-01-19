using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[Serializable]
public class ChunkData
{
    public BlockType[] blocks;
    public List<Vector2Int> unmodifiableColumns;
    public int chunkSize = 16;
    public int chunkHeight = 100;
    public Vector3Int worldPosition;

    public bool modifiedByThePlayer = false;

    public ChunkData(int chunkSize, int chunkHeight, Vector3Int worldPosition)
    {
        this.chunkHeight = chunkHeight;
        this.chunkSize = chunkSize;
        this.worldPosition = worldPosition;
        blocks = new BlockType[chunkSize * chunkHeight * chunkSize];
        unmodifiableColumns = new List<Vector2Int>();
    }
}

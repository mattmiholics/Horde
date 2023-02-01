using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.AI.Navigation;

public class NavMeshUpdate : MonoBehaviour
{
    public NavMeshSurface worldSurface;
    
    void OnEnable()
    {
        World.ChunkUpdated += Test;
        World.WorldCreated += Test;
    }

    void OnDisable()
    {
        World.ChunkUpdated -= Test;
        World.WorldCreated -= Test;
    }

    void Test()
    {
        Test(new HashSet<ChunkRenderer>());
    }

    void Test(HashSet<ChunkRenderer> chunksUpdated = default)
    {
        Debug.Log("chunk updated");
        worldSurface.BuildNavMesh();
    }
}

using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class StaticAgent : SerializedMonoBehaviour
{
    public event Action<List<PathPoint>> PathUpdated;

    public int maxNodes;
    public Transform startPoint;
    public Transform debugTarget;
    public SPathFinderType type;

    [ReadOnly] public List<PathPoint> pathPoints;
    [ReadOnly] public Vector3 currentTarget;

    private void OnEnable()
    {
        World.ChunkUpdated += ChunksUpdatePath;
    }

    private void OnDisable()
    {
        World.ChunkUpdated -= ChunksUpdatePath;
    }

    private void ChunksUpdatePath(HashSet<ChunkRenderer> updatedChunks)
    {
        SetTarget(currentTarget, maxNodes);
    }

    // Start is called before the first frame update
    void Start()
    {
        SetTarget(debugTarget.position, maxNodes);
    }

    public bool SetTarget(Vector3 target, int maxNodes = 1000)
    {
        if (PathSearch.GetPathToTarget(startPoint.position, target, World.Instance, type, out List<PathPoint> pathPoints, maxNodes) && pathPoints.Count > 0)
        {
            currentTarget = Vector3Int.RoundToInt(target);
            this.pathPoints = pathPoints;
            PathUpdated?.Invoke(pathPoints);
            return true;
        }
        else
        {
            Debug.LogWarning("No path found!");
            return false;
        }
    }
}

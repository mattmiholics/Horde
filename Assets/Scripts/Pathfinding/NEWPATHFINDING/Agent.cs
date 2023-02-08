using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class Agent : SerializedMonoBehaviour
{
    public Rigidbody rigidbody;
    public SPathFinderType type;
    public Transform debugTarget;

    private Coroutine followPathCoroutine;

    private void Start()
    {
        StartCoroutine(SearchPath());
    }

    private IEnumerator SearchPath()
    {
        for (; ; )
        {
            yield return new WaitForSeconds(0.5f);

            SetTarget(debugTarget.position, 35);
        }
    }

    /// <summary>
    /// Sets a target position for the agent to pathfind to, and starts following the path it calculates.
    /// </summary>
    public void SetTarget(Vector3 target, int maxNodes = 1000)
    {
        if (PathSearch.GetPathToTarget(rigidbody.position, target, World.Instance, type, out List<PathPoint> pathPoints, maxNodes))
        {
            if (followPathCoroutine != null)
                StopCoroutine(followPathCoroutine);
            followPathCoroutine = StartCoroutine(FollowPath(pathPoints));
        }
        else
        {
            if (followPathCoroutine != null)
                StopCoroutine(followPathCoroutine);

            Debug.LogWarning("No path found!");
        }
    }

    /// <summary>
    /// Input a list of already calculated path points to follow.
    /// </summary>
    public void SetTarget(List<PathPoint> calculatedPathPoints)
    {
        if (followPathCoroutine != null)
            StopCoroutine(followPathCoroutine);
        followPathCoroutine = StartCoroutine(FollowPath(calculatedPathPoints));
    }

    private IEnumerator FollowPath(List<PathPoint> pathPoints)
    {
        yield return null;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Linq;
using System;

[RequireComponent(typeof(Rigidbody))]
public class Agent : SerializedMonoBehaviour
{
    public event Action<List<PathPoint>> PathUpdated;   

    new public Rigidbody rigidbody;
    public LayerMask groundLayer;
    public SPathFinderType type;
    // Debug
    /*public int debugMaxNodes;
    public Transform debugTarget;
    [Button]
    private void SearchPath() // This is to debug
    {
    SetTarget(debugTarget.position, debugMaxNodes);
    }*/

    [ReadOnly] public int remainingNodes;

    public float jumpHeightMultiplier = 2.1f;
    public float jumpHeightOffset = 5.2f;
    private float gravity = 20;
    private float jumpCooldown = 0.6f;
    private bool jumpReady;

    private Vector3 currentTarget;
    private int latestMaxNodes;

    private Coroutine followPathCoroutine;

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
        SetTarget(currentTarget, latestMaxNodes);
    }

    private void Start()
    {
        jumpReady = true;

        rigidbody.useGravity = false;
        rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
    }

    private void FixedUpdate()
    {
        rigidbody.AddForce(Vector3.down * rigidbody.mass * gravity);
    }

    /// <summary>
    /// Sets a target position for the agent to pathfind to, and starts following the path it calculates.
    /// </summary>
    public bool SetTarget(Vector3 target, int maxNodes = 1000)
    {
        latestMaxNodes = maxNodes;

        if (PathSearch.GetPathToTarget(rigidbody.position, target, World.Instance, type, out List<PathPoint> pathPoints, maxNodes) && pathPoints.Count > 0)
        {
            if (pathPoints.Count <= 1 && currentTarget == Vector3Int.RoundToInt(target))
                return true;

            currentTarget = Vector3Int.RoundToInt(target);
            PathUpdated?.Invoke(pathPoints);

            if (followPathCoroutine != null)
                StopCoroutine(followPathCoroutine);
            followPathCoroutine = StartCoroutine(FollowPath(pathPoints));

            return true;
        }
        else
        {
            if (followPathCoroutine != null)
                StopCoroutine(followPathCoroutine);

            rigidbody.velocity = new Vector3(0, rigidbody.velocity.y, 0);

            Debug.LogWarning("No path found!");

            return false;
        }
    }

    /// <summary>
    /// Input a list of already calculated path points to follow.
    /// </summary>
    public void SetTarget(List<PathPoint> calculatedPathPoints)
    {
        // Check for nearest path point instead of the first
        /*int nearestIndex = calculatedPathPoints.Select((p, ix) => new { Point = p, Index = ix })
                                               .OrderBy(x => Vector3.Distance(x.Point.point, rigidbody.position))
                                               .First()
                                               .Index;*/

        if (followPathCoroutine != null)
            StopCoroutine(followPathCoroutine);
        followPathCoroutine = StartCoroutine(FollowPath(calculatedPathPoints));

        currentTarget = calculatedPathPoints.LastOrDefault().point;
        PathUpdated?.Invoke(calculatedPathPoints);
    }

    private IEnumerator FollowPath(List<PathPoint> pathPoints)
    {
        pathPoints.Reverse();

        if (pathPoints.Count <= 0)
            yield break;

        for (; ; )
        {
            if (Vector3.Distance(new Vector3(rigidbody.position.x, pathPoints.Last().point.y, rigidbody.position.z), pathPoints.Last().point) < 0.05f)
            {
                if (pathPoints.Count <= 1)
                    break;
                else
                    pathPoints.RemoveAt(pathPoints.Count - 1);
            }

            Vector3 direction = (pathPoints.Last().point - rigidbody.position);
            direction = new Vector3(direction.x, 0, direction.z).normalized;

            if (jumpReady) // Check for jump
            {
                int blockHeightChange = Mathf.RoundToInt(pathPoints.Last().point.y - rigidbody.position.y);

                if (blockHeightChange <= type.jumpHeight && blockHeightChange > 0 && Physics.Raycast(rigidbody.position, Vector3.down, 0.51f, groundLayer))
                {
                    rigidbody.velocity = new Vector3(rigidbody.velocity.x, (blockHeightChange * jumpHeightMultiplier) + jumpHeightOffset, rigidbody.velocity.z);

                    StartCoroutine(JumpCooldown(jumpCooldown));
                }
            }

            rigidbody.velocity = new Vector3(type.speed * direction.x, rigidbody.velocity.y, type.speed * direction.z);

            remainingNodes = pathPoints.Count;
            //Debug.DrawLine(rigidbody.position, pathPoints.Last().point, Color.magenta);

            yield return null;
        }

        rigidbody.velocity = new Vector3(0, rigidbody.velocity.y, 0);

        // Just to make sure the position is exact lerp to the center of the block
        Vector3 currentPosition = transform.position;
        float currentTime = 0;
        float lerpTime = 0.5f/type.speed;

        while (currentTime < lerpTime)
        {
            rigidbody.position = Vector3.Slerp(currentPosition, pathPoints.Last().point, currentTime/lerpTime);

            currentTime += Time.deltaTime;

            yield return null;
        }

        remainingNodes = 0;
        rigidbody.position = pathPoints.Last().point;
        yield return null;
    }

    private IEnumerator JumpCooldown(float time)
    {
        jumpReady = false;

        yield return new WaitForSeconds(time);

        jumpReady = true;
    }
}
public struct SPathFinderType
{
    [MinValue(0)]
    public float speed;
    [MinValue(0)]
    public int maxFallDistance, jumpHeight;
    [MinValue(0)]
    public int characterHeight;
    public static SPathFinderType normal()
    {
        SPathFinderType n = new SPathFinderType();
        n.speed = 1;
        n.maxFallDistance = 1;
        n.jumpHeight = 1;
        n.characterHeight = 2;
        return n;
    }
}

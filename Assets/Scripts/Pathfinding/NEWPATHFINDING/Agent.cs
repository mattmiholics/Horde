using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Linq;
using System;
using UnityEngine.Events;

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

    [ReadOnly] public float movementMultiplier = 1;
    [ReadOnly] public int remainingNodes;
    [ReadOnly] public bool isMoving;

    [FoldoutGroup("Events")]
    public UnityEvent startMoving;
    [FoldoutGroup("Events")]
    public UnityEvent stopMoving;
    [FoldoutGroup("Events")]
    public UnityEvent jump;

    private const float jumpHeightMultiplier = 1.75f;
    private const float jumpHeightOffset = 6f;
    private const float rotateSpeed = 3f;
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
        // SetTarget(currentTarget, latestMaxNodes);
        Debug.Log("Current Target: " + currentTarget);
        SetTarget(currentTarget);
    }

    private void Start()
    {
        jumpReady = true;
        movementMultiplier = 1;
        rigidbody.useGravity = false;
        rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
    }

    private void FixedUpdate()
    {
        rigidbody.AddForce(Vector3.down * rigidbody.mass * gravity);

        if (!isMoving)
            rigidbody.velocity = new Vector3(rigidbody.velocity.x * 0.95f, rigidbody.velocity.y, rigidbody.velocity.z * 0.87f);
    }

    /// <summary>
    /// Sets a target position for the agent to pathfind to, and starts following the path it calculates.
    /// </summary>
    public bool SetTarget(Vector3 target, int maxNodes = 1000)
    {
        latestMaxNodes = maxNodes;

        if (PathSearch.GetPathToTarget(transform.position + Vector3.up * 0.5f, target, World.Instance, type, out List<PathPoint> pathPoints, maxNodes) && pathPoints.Count > 0)
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

            stopMoving?.Invoke();
            rigidbody.velocity = new Vector3(0, rigidbody.velocity.y, 0);
            remainingNodes = 0;
            isMoving = false;

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

        currentTarget = calculatedPathPoints.LastOrDefault().point;
        
        if (followPathCoroutine != null)
            StopCoroutine(followPathCoroutine);
        followPathCoroutine = StartCoroutine(FollowPath(calculatedPathPoints));

        PathUpdated?.Invoke(calculatedPathPoints);
    }

    private IEnumerator FollowPath(List<PathPoint> pathPoints)
    {
        pathPoints.Reverse();

        if (pathPoints.Count <= 0)
            yield break;

        isMoving = true;
        startMoving?.Invoke();

        for (; ; )
        {
            // Check if close enough to a node to either remove or if at final node
            if (Vector3.Distance(new Vector3(transform.position.x, pathPoints.Last().point.y, transform.position.z), pathPoints.Last().point) < 0.05f)
            {
                if (pathPoints.Count <= 1 && Vector3.Distance(transform.position + Vector3.up * 0.5f, pathPoints.Last().point) < 0.05f)
                    break;
                else if (pathPoints.Count > 1)
                    pathPoints.RemoveAt(pathPoints.Count - 1);
            }

            // Calculate direction
            Vector3 direction = (pathPoints.Last().point - (transform.position + Vector3.up * 0.5f));
            direction = new Vector3(direction.x, 0, direction.z).normalized;

            // Calculate rotation
            rigidbody.rotation = Quaternion.Slerp(rigidbody.rotation, Quaternion.LookRotation(direction), rotateSpeed * Time.deltaTime);

            // Check if jumping is necessary
            if (jumpReady)
            {
                int blockHeightChange = Mathf.RoundToInt(pathPoints.Last().point.y - (transform.position.y + 0.5f));

                if (blockHeightChange <= type.jumpHeight && blockHeightChange > 0 && Physics.Raycast(transform.position + Vector3.up * 0.5f, Vector3.down, 0.51f, groundLayer))
                {
                    rigidbody.velocity = new Vector3(rigidbody.velocity.x, (blockHeightChange * jumpHeightMultiplier) + jumpHeightOffset, rigidbody.velocity.z);

                    StartCoroutine(JumpCooldown(jumpCooldown));

                    jump?.Invoke();
                }
            }

            rigidbody.velocity = new Vector3(type.speed * movementMultiplier * direction.x, rigidbody.velocity.y, type.speed * movementMultiplier * direction.z);

            remainingNodes = pathPoints.Count;
            Debug.DrawLine(rigidbody.position, pathPoints.Last().point, Color.magenta);

            yield return null;
        }

        rigidbody.velocity = new Vector3(0, rigidbody.velocity.y, 0);

        // Just to make sure the position is exact lerp to the center of the block
        Vector3 currentPosition = transform.position;
        float currentTime = 0;
        float lerpTime = 0.5f/(type.speed*movementMultiplier);

        while (currentTime < lerpTime)
        {
            transform.position = Vector3.Slerp(currentPosition, pathPoints.Last().point + Vector3.down * 0.5f, currentTime/lerpTime);

            currentTime += Time.deltaTime;

            yield return null;
        }

        stopMoving?.Invoke();
        
        isMoving = false;
        remainingNodes = 0;
        transform.position = pathPoints.Last().point + Vector3.down * 0.5f;
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

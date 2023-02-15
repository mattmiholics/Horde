using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Linq;

[RequireComponent(typeof(Rigidbody))]
public class Agent : SerializedMonoBehaviour
{
    public Rigidbody rigidbody;
    public LayerMask groundLayer;
    public SPathFinderType type;
    public Transform debugTarget;

    public float jumpHeightMultiplier = 7;
    public float jumpHeightOffset;
    private float gravity = 20;
    private float jumpCooldown = 0.6f;
    private bool jumpReady;

    private Vector3 currentTarget;

    private Coroutine followPathCoroutine;

    private void Start()
    {
        jumpReady = true;

        rigidbody.useGravity = false;

        StartCoroutine(SearchPath());
    }

    private void FixedUpdate()
    {
        rigidbody.AddForce(Vector3.down * rigidbody.mass * gravity);
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
            if (pathPoints.Count > 0)
            {
                if (pathPoints.Count <= 1 && currentTarget == Vector3Int.RoundToInt(target))
                    return;

                currentTarget = Vector3Int.RoundToInt(target);

                if (followPathCoroutine != null)
                    StopCoroutine(followPathCoroutine);
                followPathCoroutine = StartCoroutine(FollowPath(pathPoints));
            }
        }
        else
        {
            if (followPathCoroutine != null)
                StopCoroutine(followPathCoroutine);

            rigidbody.velocity = new Vector3(0, rigidbody.velocity.y, 0);

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
        Debug.Log("new follow");

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

            Debug.DrawLine(rigidbody.position, pathPoints.Last().point, Color.magenta);

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

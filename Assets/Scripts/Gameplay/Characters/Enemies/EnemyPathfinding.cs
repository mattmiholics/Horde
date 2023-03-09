using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;

public class EnemyPathfinding : MonoBehaviour
{
    [Header("Navigation Pathfinding")]
    public Agent agent;
    public LayerMask whatIsPlayer;
    // public Transform ultimateDestination;

    [Header("Navigation Set Target")]
    public float sightRange;
    public float attackRange;
    public bool playerInSightRange, playerInAttackRange;

    [Header("Animation")]
    public Animator animator;

    Vector3 target;

    private void Awake()
    {
        agent = GetComponent<Agent>();
        // StartCoroutine(CheckPlayerInSight());
    }

    private void Update()
    {
        int dist = agent.remainingNodes;
        if(dist <= 2)
        {
            Destroy(gameObject);
            PlayerStats.Instance.lives--;
        }
        // else if (agent.SetTarget(target, 50))
        // {
        //     //DrawPath();
        // }
    }

    private void MoveToTarget()
    {
        // agent.SetTarget(ultimateDestination.position, 50);
        // Debug.Log("Ultimate Destination: " + ultimateDestination.position);
    }

    // Use overlapsphere instead
    private IEnumerator CheckPlayerInSight()
    {
        for (; ;) 
        {
            playerInSightRange = Physics.CheckSphere(transform.position, sightRange, whatIsPlayer);

            if (playerInSightRange) playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, whatIsPlayer);
            else playerInAttackRange = false;

            if (playerInSightRange && !playerInAttackRange) ChasePlayer(); // Prioritize chase player
            else if (playerInSightRange && playerInAttackRange) AttackPlayer(GetNearestPlayer()); // Then attacking player (probably need to prioritize this later on)
            else if (!playerInSightRange && !playerInAttackRange) MoveToTarget(); // Then move to target

            yield return new WaitForSeconds(0.5f);
        }
    }

    private void ChasePlayer()
    {
        agent.SetTarget(GetNearestPlayer().position, 50);
    }

    // Use overlapsphere instead
    private Transform GetNearestPlayer()
    {
        Collider[] playerUnits = Physics.OverlapSphere(transform.position, sightRange, whatIsPlayer);
        Transform nearestPlayerPosition = playerUnits.OrderBy(x => Vector3.Distance(x.transform.position, transform.position)).FirstOrDefault().transform;
        return nearestPlayerPosition;
    }

    private void AttackPlayer(Transform playerObject)
    {
        agent.SetTarget(transform.position, 50);

        transform.LookAt(playerObject);
        Debug.Log("Attack Player 1");

        this.gameObject.GetComponent<EnemyData>().Attack();
    }
}

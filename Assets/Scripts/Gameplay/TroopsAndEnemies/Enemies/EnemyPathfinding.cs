using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;
using Sirenix.OdinInspector;

public class EnemyPathfinding : MonoBehaviour
{
    // public Transform[] destinations;
    // public Agent agent;

    // private LineRenderer myLineRenderer;

    // //public Transform player;
    // public Transform targetObject;
    // //[SerializeField] private GameObject targetMarkerPrefab;
    // [PropertySpace(0, 10)]
    // [SerializeField] private Transform visualObjectsParent;
    // public LayerMask whatIsGround, whatIsPlayer;

    // //Patroling
    // public Vector3 walkPoint;
    // Vector3 target;
    // bool walkPointSet;
    // public float walkPointRange;

    // //States
    // public float sightRange, attackRange;
    // public bool playerInSightRange, playerInAttackRange;

    // private void Awake()
    // {
    //     // THIS NEEDS TO BE OPTIMIZED
    //     targetObject = TowerEditor.Instance.permanentTowerParent.GetComponentInChildren<MainHall>().target;
    //     // agent = GetComponent<NavMeshAgent>();
    //     myLineRenderer = GetComponent<LineRenderer>();

    //     myLineRenderer.startWidth = 0.15f;
    //     myLineRenderer.endWidth = 0.15f;
    //     myLineRenderer.positionCount = 0;

    //     StartCoroutine(CheckPlayerInSight());
    // }

    // private void Update()
    // {
    //     float dist = Vector3.Distance(transform.position, targetObject.transform.position);
    //     if(dist <= 2)
    //     {
    //         Destroy(gameObject);
    //         PlayerStats.Instance.lives--;
    //     }
    //     else if (agent.SetTarget(target))
    //     {
    //         //DrawPath();
    //     }
    // }

    // private IEnumerator CheckPlayerInSight()
    // {
    //     for (; ;) 
    //     {
    //         playerInSightRange = Physics.CheckSphere(transform.position, sightRange, whatIsPlayer);

    //         if (playerInSightRange) playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, whatIsPlayer);
    //         else playerInAttackRange = false;

    //         if (playerInSightRange && !playerInAttackRange) ChasePlayer(); // Prioritize chase player
    //         else if (playerInSightRange && playerInAttackRange) AttackPlayer(GetNearestPlayer()); // Then attacking player (probably need to prioritize this later on)
    //         else if (!playerInSightRange && !playerInAttackRange) MoveToTarget(); // Then move to target

    //         yield return new WaitForSeconds(0.5f);
    //     }
    // }

    // private void MoveToTarget()
    // {
    //     //targetMarkerPrefab.transform.SetParent(visualObjectsParent);
    //     //targetMarkerPrefab.SetActive(true);
    //     //targetMarkerPrefab.transform.position = targetObject.transform.position;
    //     agent.SetTarget(targetObject.position);
    // }

    // private void Patroling()
    // {
    //     if (!walkPointSet) SearchWalkPoint();

    //     if (walkPointSet)
    //         agent.SetTarget(walkPoint);

    //     Vector3 distanceToWalkPoint = transform.position - walkPoint;

    //     if (distanceToWalkPoint.magnitude < 1f)
    //     {
    //         walkPointSet = false;
    //     }
    // }

    // private void SearchWalkPoint()
    // {
    //     float randomZ = Random.Range(-walkPointRange, walkPointRange);
    //     float randomX = Random.Range(-walkPointRange, walkPointRange);

    //     walkPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

    //     if (Physics.Raycast(walkPoint, -transform.up, 2f, whatIsGround))
    //     {
    //         walkPointSet = true;
    //     }
    // }

    // private void ChasePlayer()
    // {
    //     agent.SetTarget(GetNearestPlayer().position);
    // }

    // private Transform GetNearestPlayer()
    // {
    //     Collider[] playerUnits = Physics.OverlapSphere(transform.position, sightRange, whatIsPlayer);
    //     Transform nearestPlayerPosition = playerUnits.OrderBy(x => Vector3.Distance(x.transform.position, transform.position)).FirstOrDefault().transform;
    //     return nearestPlayerPosition;
    // }

    // // void DrawPath()
    // // {
    // //     myLineRenderer.positionCount = agent.path.corners.Length;
    // //     myLineRenderer.SetPosition(0, transform.position);

    // //     if (agent.path.corners.Length < 2)
    // //     {
    // //         return;
    // //     }

    // //     for (int i = 1; i < agent.path.corners.Length; i++)
    // //     {
    // //         Vector3 pointPosition = new Vector3(agent.path.corners[i].x, agent.path.corners[i].y, agent.path.corners[i].z);
    // //         myLineRenderer.SetPosition(i, pointPosition);
    // //     }
    // // }
}

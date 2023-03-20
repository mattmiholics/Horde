using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;
using Sirenix.OdinInspector;

public class EnemyPathfinding : MonoBehaviour
{
    [Header("Navigation Pathfinding")]
    public Agent agent;
    public LayerMask troopLayer;
    // public Transform ultimateDestination;

    [Header("Navigation Set Target")]
    public float sightRange;
    public float attackRange;
    public bool troopInSightRange, troopInAttackRange;

    Transform target;
    [ReadOnly] public bool hasDeviatedFromMainPath;
    EnemyData enemyData;

    private void Start()
    {
        target = TowerEditor.Instance.permanentTowerParent.GetComponentInChildren<MainHall>().target;
        enemyData = GetComponent<EnemyData>();
        hasDeviatedFromMainPath = false;
        StartCoroutine(CheckTroopInSight());
    }

    private void Awake()
    {
        agent = GetComponent<Agent>();
    }

    private void CheckDistance()
    {
        if (Vector3.Distance(target.position, transform.position) <= 1f)
        {
            Destroy(gameObject);
            PlayerStats.Instance.lives--;
        }
    }

    private void OnEnable() 
    {
        agent.stopMovingEvent += CheckDistance;
    }

    private void OnDisable()
    {
        agent.stopMovingEvent -= CheckDistance;
    }

    private void MoveToTarget()
    {
        agent.SetTarget(target.position, 500);
        hasDeviatedFromMainPath = false;
    }

    // Use overlapsphere instead
    private IEnumerator CheckTroopInSight()
    {
        for (; ;) 
        {
            troopInSightRange = Physics.CheckSphere(transform.position, sightRange, troopLayer);
            if (troopInSightRange) 
            {
                Debug.Log("Check troops");
                troopInAttackRange = Physics.CheckSphere(transform.position, attackRange, troopLayer);
                hasDeviatedFromMainPath = true;
            }
            else troopInAttackRange = false;
            if (hasDeviatedFromMainPath && PathfindNearestTroop())
            {
                if (troopInSightRange && troopInAttackRange && enemyData.canAttack) 
                    enemyData.Attack(GetNearestTroop()); // Then attacking troop (probably need to prioritize this later on)
            } 
            else if (hasDeviatedFromMainPath)
                MoveToTarget();
            yield return new WaitForSeconds(0.2f);
        }
    }

    private bool PathfindNearestTroop()
    {
        Collider[] troopUnits = Physics.OverlapSphere(transform.position, sightRange, troopLayer).OrderBy(x => Vector3.Distance(x.transform.position, transform.position)).ToArray();
        foreach (Collider unit in troopUnits)
        {
            if (agent.SetTarget(unit.transform.position, (int)(sightRange * 3f)))
                // Debug.Log("Pathfind nearest troop: " + true);
                return true;
        }
        // Debug.Log("Pathfind nearest troop: " + false);
        return false;
    }
    private TroopData GetNearestTroop()
    {
        Collider[] troopUnits = Physics.OverlapSphere(transform.position, sightRange, troopLayer);
        Transform nearestTroopPosition = troopUnits.OrderBy(x => Vector3.Distance(x.transform.position, transform.position)).FirstOrDefault().transform;
        return nearestTroopPosition.GetComponentInParent<TroopData>();
    }
}

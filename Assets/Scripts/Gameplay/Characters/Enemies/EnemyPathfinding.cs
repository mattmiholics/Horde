using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;


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

    [Header("Animation")]
    public Animator animator;

    Transform target;
    bool hasDeviatedFromMainPath;
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
        if(Vector3.Distance(target.position, transform.position) <= 0.5f)
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
                // Debug.Log("Check troops");
                troopInAttackRange = Physics.CheckSphere(transform.position, attackRange, troopLayer);
                hasDeviatedFromMainPath = true;
            }
            else troopInAttackRange = false;
            if (hasDeviatedFromMainPath)
            {
                if (troopInSightRange && !troopInAttackRange) ChaseTroop(); // Prioritize chase troop
                else if (troopInSightRange && troopInAttackRange && enemyData.canAttack) enemyData.Attack(GetNearestTroop()); // Then attacking troop (probably need to prioritize this later on)
                else if (!troopInSightRange && !troopInAttackRange) MoveToTarget(); // Then move to target
            }
            yield return new WaitForSeconds(0.2f);
        }
    }

    private void ChaseTroop()
    {
        agent.SetTarget(GetNearestTroop().transform.position, (int)sightRange*3);
    }

    // Use overlapsphere instead
    private TroopData GetNearestTroop()
    {
        Collider[] troopUnits = Physics.OverlapSphere(transform.position, sightRange, troopLayer);
        Transform nearestTroopPosition = troopUnits.OrderBy(x => Vector3.Distance(x.transform.position, transform.position)).FirstOrDefault().transform;
        return nearestTroopPosition.GetComponentInParent<TroopData>();
    }
}

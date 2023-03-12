using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using System.Linq;

public class TroopData : UnitData
{
    protected Agent agent;
    protected TroopPathfinding troopPathfinding;

    [Header("Unity Setup Fields")]
    public LayerMask enemyLayer;

    // Start is called before the first frame update
    protected override void Start()
    {
        startHealth = health;
        canAttack = true;
        agent = this.gameObject.GetComponent<Agent>();
        troopPathfinding = this.gameObject.GetComponent<TroopPathfinding>();
        StartCoroutine(UpdateTarget());
    }

    private IEnumerator UpdateTarget()
    {
        for (;;)
        {
            Collider enemy = Physics.OverlapSphere(transform.position, range, enemyLayer).OrderBy(x => Vector3.Distance(x.transform.position, transform.position)).FirstOrDefault();
            if (enemy && canAttack)
                Attack(enemy.GetComponentInParent<EnemyData>());
            yield return new WaitForSeconds(0.2f);
        }
    }
}

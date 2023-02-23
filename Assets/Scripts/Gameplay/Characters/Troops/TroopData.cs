using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TroopData : UnitData
{
    protected Transform target;
    protected Agent agent;
    protected TroopPathfinding troopPathfinding;
    
    [Header("Attributes")]
    public float fireRate = 1f;
    public float fireReload = 0f;
    public float range = 15f;
    public float damage = 25f;

    [Header("Unity Setup Fields")]

    public string enemyTag = "Enemy";
    public Transform partToRotate;
    public float rotationSpeed = 7f;

    public GameObject bullet;
    public Transform firePoint;

    [Header("Animation")]
    public Animator animator;

    // Start is called before the first frame update
    protected override void Start()
    {
        startHealth = health;
        agent = this.gameObject.GetComponent<Agent>();
        troopPathfinding = this.gameObject.GetComponent<TroopPathfinding>();
        InvokeRepeating("UpdateTarget", 0f, .5f);
    }

    protected virtual void UpdateTarget()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag(enemyTag);
        float shortestDistance = Mathf.Infinity;
        GameObject nearestEnemy = null;

        foreach(GameObject enemy in enemies)
        {
            float distToEnemy = Vector3.Distance(transform.position, enemy.transform.position);
            if(distToEnemy < shortestDistance)
            {
                shortestDistance = distToEnemy;
                nearestEnemy = enemy;
            }
        }

        if(nearestEnemy != null && shortestDistance <= range)
        {
            target = nearestEnemy.transform;
        }
        else
        {
            //this was throwing an error in other prefabs
            target = null;
        }
    }

    protected virtual void Update()
    {
        MovementAnimation();
        
        if (target == null)
        {
            return;
        }
        //Target Locking
        Vector3 dir = target.transform.position - transform.position;
        Quaternion lookRotation = Quaternion.LookRotation(dir);
        Vector3 rotation = Quaternion.Lerp(partToRotate.rotation,lookRotation,Time.deltaTime * rotationSpeed).eulerAngles;
        partToRotate.rotation = Quaternion.Euler(0f,rotation.y, 0f);

        if(fireReload <= 0)
        {
            Attack();
            fireReload = 1 / fireRate;
        }

        fireReload -= Time.deltaTime;
        
    }

    protected virtual void Attack()
    {
        GameObject bulltObj = (GameObject)Instantiate(bullet, firePoint.position, firePoint.rotation);
        //If a new bullet script is created, update it here
        Bullet bulletS = bulltObj.GetComponent<Bullet>();

        if(bulletS != null)
        {
            bulletS.Seek(target, damage);
        }
    }

    protected virtual void MovementAnimation()
    {
        if (agent.remainingNodes <= 1f)
            animator.SetBool("IsRunning", false);
        else
            animator.SetBool("IsRunning", true);
    }

    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}

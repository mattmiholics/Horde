using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightningTower : MonoBehaviour
{
    private Transform target;

    [Header("Attributes")]

    public float fireRate = 1f;
    public float fireReload = 0f;
    public float range = 15f;
    public int damage = 50;
    public int chainAmount = 0;
    public int chainDamage = 0;

    [Header("Unity Setup Fields")]

    public string enemyTag = "Enemy";
    public Transform partToRotate;
    public float rotationSpeed = 7f;

    public GameObject bullet;
    public Transform firePoint;

    [Header("Laser Settings")]
    public LineRenderer lr;
    public bool useLaser = false;
    public GameObject laserStart;

    private List<GameObject> enemiesHit = new List<GameObject>();


    // Start is called before the first frame update
    void Start()
    {
        InvokeRepeating("UpdateTarget", 0f, .5f);
    }

    void UpdateTarget()
    {

        GameObject[] enemies = GameObject.FindGameObjectsWithTag(enemyTag);
        float shortestDistance = Mathf.Infinity;
        GameObject nearestEnemy = null;

        foreach (GameObject enemy in enemies)
        {
            float distToEnemy = Vector3.Distance(transform.position, enemy.transform.position);
            if (distToEnemy < shortestDistance)
            {
                shortestDistance = distToEnemy;
                nearestEnemy = enemy;
            }
        }

        if (nearestEnemy != null && shortestDistance <= range)
        {
            target = nearestEnemy.transform;
        }
        else
        {
            //this was throwing an error in other prefab
            target = null;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (target == null)
        {
            if (useLaser)
            {
                lr.enabled = false;
            }
            return;
        }
        //Target Locking
        Vector3 dir = target.transform.position - transform.position;
        Quaternion lookRotation = Quaternion.LookRotation(dir);
        Vector3 rotation = Quaternion.Lerp(partToRotate.rotation, lookRotation, Time.deltaTime * rotationSpeed).eulerAngles;
        partToRotate.rotation = Quaternion.Euler(0f, rotation.y, 0f);

        if (fireReload <= 0)
        {
            Shoot();
            if (useLaser)
            {
                FireLaser();
            }
            fireReload = 1 / fireRate;
        }

        fireReload -= Time.deltaTime;

    }

    void Shoot()
    {
        GameObject bulltObj = (GameObject)Instantiate(bullet, firePoint.position, firePoint.rotation);
        //If a new bullet script is created, update it here
        Bullet bulletS = bulltObj.GetComponent<Bullet>();
        CannonBullet cBullet = bulltObj.GetComponent<CannonBullet>();
        LBullet lBullet = bulltObj.GetComponent<LBullet>();

        if (bulletS != null)
        {
            bulletS.Seek(target, damage);
        }
        else if (cBullet != null)
        {
            cBullet.Seek(target, damage);
        }
        else if (lBullet != null)
        {
            lBullet.Seek(target, damage, chainAmount, chainDamage);
        }
    }

    private void FireLaser()
    {
        enemiesHit.Add(target.gameObject);
        HitEnemiesWithLaser(1);
        lr.enabled = true;
        lr.positionCount = 1 + enemiesHit.ToArray().Length;
        lr.SetPosition(0, laserStart.transform.position);
        int count = 1;
        foreach (GameObject pos in enemiesHit.ToArray())
        {
            if (pos != null)
            {
                lr.SetPosition(count, pos.transform.position);
                count++;
            }
        }
        //lr.enabled = false;
        enemiesHit.Clear();
        
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, range);
    }

    private void HitEnemiesWithLaser(int count)
    {
        /*
         *  Step 1: Find closeest enemy to the previousTarget
         *  Step 2: Deal damage and find next closest enemy
         *  Step 3: Repeat step 2 until chainAmount
         */
        GameObject prevTarget = target.gameObject;
        while (count <= chainAmount)
        {
            float relativeDist = float.MaxValue;
            GameObject currentTarget = null;
            Collider[] targetsHit = Physics.OverlapSphere(prevTarget.transform.position, 10);

            foreach (Collider hitCollider in targetsHit)
            {
                float storeVal = Vector3.Distance(prevTarget.transform.position, hitCollider.gameObject.transform.position);
                if (hitCollider.gameObject.tag == "Enemy" && storeVal < relativeDist && hitCollider.gameObject != prevTarget && !enemiesHit.Contains(hitCollider.gameObject))
                {
                    relativeDist = storeVal;
                    currentTarget = hitCollider.gameObject;
                }
            }
            if (currentTarget != null)
            {
                currentTarget.GetComponent<EnemyMovement>().TakeDamage(chainDamage);
                enemiesHit.Add(currentTarget);
                prevTarget= currentTarget;
            }
            count++;
        }

        /*if (count <= chainAmount)
        {
            float relativeDist = float.MaxValue;
            GameObject currentTarget = null;
            Collider[] targetsHit = Physics.OverlapSphere(previousTarget.position, 10);

            foreach (Collider hitCollider in targetsHit)
            {
                float storeVal = Vector3.Distance(previousTarget.position, hitCollider.gameObject.transform.position);
                int counter = 0;
                if(hitCollider.gameObject.tag == "Enemy" && storeVal < relativeDist && hitCollider.transform != previousTarget && !enemyAlreadyHit(hitCollider.transform))
                {
                    relativeDist = storeVal;
                    currentTarget = hitCollider.gameObject;
                    counter++;
                    Debug.Log("Count: " + counter);
                }

             
            }

            if(currentTarget != null)
            {
                currentTarget.GetComponent<EnemyMovement>().TakeDamage(chainDamage);
                HitEnemiesWithLaser(count++, currentTarget.transform);
            }
        }
        */
    }
    
}
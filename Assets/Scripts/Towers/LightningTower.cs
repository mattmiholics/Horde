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

    private Transform[] enemiesHit = new Transform[10];


    // Start is called before the first frame update
    void Start()
    {
        InvokeRepeating("UpdateTarget", 0f, .5f);
    }

    void UpdateTarget()
    {
        if (useLaser == true)
        {
            lr.enabled = false;
        }
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
            //this was throwing an error in other prefabs
            if (useLaser == true)
            {
                lr.enabled = false;
            }
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
        enemiesHit[0] = target;
        HitEnemiesWithLaser(1, target);
        lr.enabled = true;
        lr.SetPosition(0, laserStart.transform.position);
        int count = 1;
        foreach (Transform pos in enemiesHit)
        {
            if (pos != null)
            {
                lr.SetPosition(count, pos.position);
                count++;
            }
        }
        resetEnemiesHit();
        
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, range);
    }

    private void HitEnemiesWithLaser(int count, Transform previousTarget)
    {    
        if (count <= chainAmount)
        {
            float relativeDist = float.MaxValue;
            GameObject currentTarget = null;
            Collider[] targetsHit = Physics.OverlapSphere(previousTarget.position, 10);

            foreach (Collider hitCollider in targetsHit)
            {
                float storeVal = Vector3.Distance(previousTarget.position, hitCollider.gameObject.transform.position);

                if(hitCollider.gameObject.tag == "Enemy" && storeVal < relativeDist && hitCollider.transform != previousTarget && !enemyAlreadyHit(hitCollider.transform))
                {
                    relativeDist = storeVal;
                    currentTarget = hitCollider.gameObject;
                }
            }

            if(currentTarget != null)
            {
                currentTarget.GetComponent<EnemyMovement>().TakeDamage(chainDamage);
                HitEnemiesWithLaser(count--, currentTarget.transform);
            }
        }
    }

    private bool enemyAlreadyHit(Transform checkEnemy)
    {
        bool flag = false;
        for(int i = 0; i < enemiesHit.Length; i++)
        {
            if(checkEnemy == enemiesHit[i])
            {
                flag = true;
            }
        }
        return flag;
    }

    private void resetEnemiesHit()
    {
        for(int i = 0; i < enemiesHit.Length; i++)
        {
            enemiesHit[i] = null;
        }
    }
}
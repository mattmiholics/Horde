using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TroopData : UnitData
{
    protected Transform target;
    
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

    // Start is called before the first frame update
    protected override void Start()
    {
        startHealth = health;
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

    protected virtual void Shoot()
    {
        GameObject bulltObj = (GameObject)Instantiate(bullet, firePoint.position, firePoint.rotation);
        //If a new bullet script is created, update it here
        Bullet bulletS = bulltObj.GetComponent<Bullet>();

        if(bulletS != null)
        {
            bulletS.Seek(target, damage);
        }
    }

    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}

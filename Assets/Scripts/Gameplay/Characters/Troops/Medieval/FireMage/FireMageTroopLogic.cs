using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FireMageTroopLogic : TroopData
{
    protected SphereCollider sphereCollider;

    // private Transform target;

    [Header("Area Damage")]
    public float areaDamageRadius = 5f;
    public float areaDamageDuration = 1.0f;
    public int areaDamagePerSecond = 5;

    // [Header("Attributes")]
    // public float fireRate = 1f;
    // public float fireReload = 0f;
    // public float range = 15f;
    // public new int damage = 50;

    [Header("Unity Setup Fields")]

    public LayerMask enemyLayerMask;
    // public Transform partToRotate;
    // public float rotationSpeed = 7f;

    // public GameObject bullet;
    // public Transform firePoint;

    [Header("Animation")]
    public Animator animator;


    // // Start is called before the first frame update
    // void Start()
    // {
    //     InvokeRepeating("UpdateTarget", 0f, .5f);
    // }

    protected override void UpdateTarget()
    {
        Collider[] nearestEnemyCollider = Physics.OverlapSphere(transform.position, range, enemyLayerMask);
        
        if (nearestEnemyCollider == null || nearestEnemyCollider.Length <= 0)
            return;
        Collider nearestEnemy = nearestEnemyCollider.OrderBy(x => Vector3.Distance(x.transform.position, transform.position)).First();

        if(nearestEnemy != null && Vector3.Distance(nearestEnemy.transform.position, transform.position) <= range)
        {
            target = nearestEnemy.transform;
        }
        else
        {
            //this was throwing an error in other prefabs
            target = null;
        }
    }

    // // Update is called once per frame
    // void Update()
    // {
    //     if (target == null)
    //     {
    //         return;
    //     }
    //     //Target Locking
    //     Vector3 dir = target.transform.position - transform.position;
    //     Quaternion lookRotation = Quaternion.LookRotation(dir);
    //     Vector3 rotation = Quaternion.Lerp(partToRotate.rotation,lookRotation,Time.deltaTime * rotationSpeed).eulerAngles;
    //     partToRotate.rotation = Quaternion.Euler(0f,rotation.y, 0f);

    //     if(fireReload <= 0)
    //     {
    //         Shoot();
    //         fireReload = 1 / fireRate;
    //     }

    //     fireReload -= Time.deltaTime;

    // }

    protected override void Attack()
    {
        GameObject bulletParent = GameObject.Find("World/BulletParent");
        GameObject bulletObj = (GameObject)Instantiate(bullet, firePoint.position, firePoint.rotation, bulletParent.transform);
        //If a new bullet script is created, update it here
        FireMageBullet bulletS = bulletObj.GetComponent<FireMageBullet>();

        if(bulletS != null)
        {
            // bulletS.sphereCollider.radius = areaDamageRadius;
            bulletS.Seek(target, damage);
        }
    }
    
    public void DebugFireMage(string STR)
    {
        Debug.Log(STR);
    }
}


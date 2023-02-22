using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArcherTroopLogic : TroopData
{
    // private Transform target;

    // [Header("Attributes")]

    // public float fireRate = 1f;
    // public float fireReload = 1f;
    // public float range = 15f;
    // public new int damage = 80;

    // [Header("Unity Setup Fields")]

    // public string enemyTag = "Enemy";
    // public Transform partToRotate;
    // public float rotationSpeed = 7f;

    // public GameObject bullet;
    // public Transform firePoint;
    
    // Update is called once per frame
    void Update()
    {
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
            Shoot();
            fireReload = 1 / fireRate;
        }

        fireReload -= Time.deltaTime;

    }

    void Shoot()
    {
        GameObject bulltObj = (GameObject)Instantiate(bullet, firePoint.position, firePoint.rotation);
        //If a new bullet script is created, update it here
        Bullet bulletS = bulltObj.GetComponent<Bullet>();

        if(bulletS != null)
        {
            bulletS.Seek(target, damage);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}
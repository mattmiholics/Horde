using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireMageBullet : Bullet
{
    protected SphereCollider sphereCollider;

    void Start()
    {

    }
    
    void HitTarget()
    {
        EnemyData e = target.GetComponent<EnemyData>();
        TroopData p = target.GetComponent<TroopData>();

        if(e != null)
        {
            e.TakeDamage(damage);
        }
        
        if(p != null)
        {
            p.TakeDamage(damage);
        }

        GameObject effectInst = (GameObject)Instantiate(impactEffect, transform.position, transform.rotation, WaveSpawner.Instance.effectParent);
        Destroy(effectInst, 2f);
        Destroy(gameObject.gameObject);
        //Destroy(target.gameObject);
    }
}

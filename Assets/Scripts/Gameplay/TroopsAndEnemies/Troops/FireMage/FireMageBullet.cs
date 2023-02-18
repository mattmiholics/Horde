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
        EnemyMovement e = target.GetComponent<EnemyMovement>();
        PlayerController p = target.GetComponent<PlayerController>();

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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireMageBullet : Bullet
{
    protected SphereCollider sphereCollider;
    [Header("Area Damage")]
    public GameObject damageFieldPrefab;

    void Start()
    {

    }
    
    void HitTarget()
    {
        EnemyData e = target.GetComponentInParent<EnemyData>();
        if(e != null)
        {
            Debug.Log("Hit Enemy and prepare to create damage field");
            CreateDamageField();
        }
        Debug.Log("Hit Target!");
        GameObject effectInst = (GameObject)Instantiate(impactEffect, transform.position, transform.rotation, WaveSpawner.Instance.effectParent);
        Destroy(effectInst, 2f);
        Destroy(gameObject.gameObject);
        //Destroy(target.gameObject);
    }

    void CreateDamageField()
    {
        Debug.Log("Creating Damage Field");
        GameObject damageField = (GameObject)Instantiate(damageFieldPrefab, gameObject.transform);
    }
}

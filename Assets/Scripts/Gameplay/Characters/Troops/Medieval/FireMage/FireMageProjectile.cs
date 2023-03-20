using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class FireMageProjectile : Projectile
{
    protected SphereCollider sphereCollider;
    public LayerMask enemyLayer;
    [Header("Area Damage")]
    // public GameObject damageFieldPrefab;
    public float areaDamageRadius = 5f;
    // public float areaDamageDuration = 1.0f;
    // public int areaDamagePerSecond = 5;
    public float areaDamage = 10;

    void Start()
    {

    }
    
    protected override void HitTarget()
    {
        EnemyData e = target.GetComponentInParent<EnemyData>();
        if(e != null)
        {
            //Debug.Log("Hit Enemy and prepare to create damage field");
            AreaDamage();
        }
        //Debug.Log("Hit Target!");
        if (projectileEffectPool)
            projectileEffectPool.Create(transform.position, transform.rotation, 2f);
        projectilePool.Destroy(gameObject);
        //Destroy(target.gameObject);
    }

    protected virtual void AreaDamage()
    {
        // Debug.Log("Creating Damage Field");
        // GameObject damageField = (GameObject)Instantiate(damageFieldPrefab, gameObject.transform);
        Collider[] enemyUnits = Physics.OverlapSphere(transform.position, areaDamageRadius, enemyLayer);
        foreach (Collider enemy in enemyUnits)
            enemy.GetComponentInParent<EnemyData>().TakeDamage(areaDamage);
    }
}

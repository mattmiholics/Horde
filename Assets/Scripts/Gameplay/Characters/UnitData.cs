using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using Sirenix.OdinInspector;

public class UnitData : MonoBehaviour
{
    public bool debug = false;
    public bool drawGizmos = false;
    public static event Action AttackReady;
    protected float startHealth;
    [HideInInspector]
    public bool canAttack;
    
    [Header("Unit Variables")]
    public float health = 100f;
    public float attackCooldown = 1f;
    public float range = 15f;
    public int damage = 25;
    public bool isRanged = true;

    [Header("Ranged References")]
    [ShowIf("$isRanged")]
    public GameObject projectilePrefab;
    [HideInInspector]
    protected ObjectPooler projectilePool;

    [ShowIf("$isRanged")]
    public Transform firePoint;

    [Header("Graphic Info")]
    public Image healthBar;
    public Gradient damageNumberGradient;
    [HideInInspector]
    protected ObjectPooler damageNumberPool;
    public GameObject damageNumberPrefab;
    [Header("Unity Events")]
    public UnityEvent Attacking;
    public UnityEvent Hit;
    public UnityEvent Death;

    protected virtual void Awake()
    {
        if (damageNumberPrefab)
        {
            GameObject damageNumberTemp = GameObject.Find(damageNumberPrefab.name + "Parent");
            if (!damageNumberTemp)
            {
                damageNumberPool = new GameObject(damageNumberPrefab.name + "Parent").AddComponent<ObjectPooler>();
                damageNumberPool.prefab = damageNumberPrefab;
                SceneManager.MoveGameObjectToScene(damageNumberPool.gameObject, gameObject.scene);
            }
            else
            {
                damageNumberPool = damageNumberTemp.GetComponent<ObjectPooler>();
            }
        }

        if (projectilePrefab)
        {
            GameObject projectileTemp = GameObject.Find(projectilePrefab.name + "Parent");
            if (!projectileTemp)
            {
                projectilePool = new GameObject(projectilePrefab.name + "Parent").AddComponent<ObjectPooler>();
                projectilePool.prefab = projectilePrefab;
                SceneManager.MoveGameObjectToScene(projectilePool.gameObject, gameObject.scene);
            }
            else
            {
                projectilePool = projectileTemp.GetComponent<ObjectPooler>();
            }
        }
    }

    protected virtual void Start()
    {
        startHealth = health;
        canAttack = true;
        // Debug.Log("Start Health: " + startHealth);
    }

    protected IEnumerator AttackCooldown(float cooldown)
    {
        canAttack = false;
        if (debug)
            Debug.Log("Start");
        yield return new WaitForSeconds(cooldown);
        canAttack = true;
        if (debug)
            Debug.Log("End");
        AttackReady?.Invoke();
    }

    public virtual void Attack(UnitData unitData)
    {
        if (isRanged)
        {
            // GameObject projectileParent = GameObject.Find("World/BulletParent");
            if (projectilePool)
            {
                GameObject projectileObj = projectilePool.Create(firePoint.position, firePoint.rotation);
                Projectile projectileS = projectileObj.GetComponent<Projectile>();
                projectileS.projectilePool = projectilePool;
                
                if(projectileS != null)
                {
                    projectileS.Seek(unitData.transform, damage);
                }
            }
        }
        else
        {
            unitData.TakeDamage(damage);
        }
        Attacking.Invoke();
        StartCoroutine(AttackCooldown(attackCooldown));
    }

    public virtual void TakeDamage(float incomingDamage)
    {
        // Debug.Log("Original health: " + health);
        health -= incomingDamage;
        Hit.Invoke();

        // Damage number
        
        if (damageNumberPool)
        {
            GameObject damageNumber = damageNumberPool.Create(healthBar.transform.position, Quaternion.identity, 2f);
            damageNumber.GetComponent<DamageNumber>().UpdateNumber(incomingDamage, damageNumberGradient.Evaluate(Mathf.Clamp01(incomingDamage/200)), Mathf.Clamp01(incomingDamage / 400 + 0.5f)); // 300 is max sized and furthest color
        }
        
        healthBar.fillAmount = health/startHealth;
        // Debug.Log("Deal damage: " + incomingDamage);
        // Debug.Log("Health after damage: " + health);
        DeathCheck();
    }
        
    public virtual void DeathCheck()
    {
        if (health <= 0f)
        {
            Death.Invoke();
            Destroy(this.gameObject);
        }
    }

    protected virtual void OnDrawGizmosSelected()
    {
        if (drawGizmos)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, range);
        }
    }
}


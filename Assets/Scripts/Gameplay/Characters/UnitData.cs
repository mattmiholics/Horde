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
    public GameObject bullet;
    [ShowIf("$isRanged")]
    public Transform firePoint;

    [Header("Graphic Info")]
    public Image healthBar;
    public Gradient damageNumberGradient;
    private Transform damageNumberParent;
    public GameObject damageNumberPrefab;
    [Header("Unity Events")]
    public UnityEvent Attacking;
    public UnityEvent Hit;
    public UnityEvent Death;

    protected virtual void Start()
    {
        startHealth = health;
        canAttack = true;
        damageNumberParent = GameObject.Find("DamageNumberParent").transform;
        if (!damageNumberParent)
        {
            damageNumberParent = new GameObject("DamageNumberParent").transform;
            SceneManager.MoveGameObjectToScene(damageNumberParent.gameObject, gameObject.scene);
            Debug.Log("couldnt find paretn");
        }
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
            GameObject bulletParent = GameObject.Find("World/BulletParent");
            GameObject bulletObj = (GameObject)Instantiate(bullet, firePoint.position, firePoint.rotation, bulletParent.transform);
            Bullet bulletS = bulletObj.GetComponent<Bullet>();
            
            if(bulletS != null)
            {
                bulletS.Seek(unitData.transform, damage);
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
        GameObject damageNumber = Instantiate(damageNumberPrefab, healthBar.transform.position, Quaternion.identity, damageNumberParent);
        damageNumber.GetComponent<DamageNumber>().UpdateNumber(incomingDamage, damageNumberGradient.Evaluate(Mathf.Clamp01(incomingDamage/200)), Mathf.Clamp01(incomingDamage / 400 + 0.5f)); // 300 is max sized and furthest color
        Destroy(damageNumber, 2f);
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


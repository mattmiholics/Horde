using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.Events;

public class EnemyData : UnitData
{
    protected Transform target;
    [Header("Attributes")]
    public float fireRate = 1f;
    public float fireReload = 0f;
    public float range = 15f;
    public int damage = 25;
    
    [Header("Value")]
    public int deathValue = 75;

    [Header("SFX")]
    public GameObject deathEffect;
    public AudioSource aSource;
    public AudioClip clip;

    [Header("Unity Setup Fields")]

    public string enemyTag = "Enemy";
    public Transform partToRotate;
    public float rotationSpeed = 7f;

    public GameObject bullet;
    public Transform firePoint;

    [Header("Animation")]
    public Animator animator;

    public override void TakeDamage (float incomingDamage)
    {
        // Debug.Log("Original health: " + health);
        health -= incomingDamage;
        healthBar.fillAmount = health/startHealth;
        // Debug.Log("Deal damage: " + incomingDamage);
        // Debug.Log("Health after damage: " + health);
        DeathCheck();
        aSource.PlayOneShot(clip);
    }

    public override void DeathCheck()
    {
        if (health <= 0)
        {
            PlayerStats.Instance.money += deathValue;
            Debug.Log("Enemy killed!");
            GameObject effect = (GameObject)Instantiate(deathEffect, transform.position, Quaternion.identity, WaveSpawner.Instance.effectParent);
            Destroy(effect, 5f);

            death.Invoke();

            Destroy(this.gameObject);
        }
    }

    public virtual void Attack()
    {
        //Debug.Log("Attack Player 2");

        GameObject bulletObj = (GameObject)Instantiate(bullet, firePoint.position, firePoint.rotation);
        Bullet bulletS = bulletObj.GetComponent<Bullet>();
        
        if(bulletS != null)
        {
            //Debug.Log("Attack Player 3");
            attack.Invoke();
            bulletS.Seek(target, 50);
        }
    }

    public virtual void AttackAnimation()
    {
        animator.SetBool("Attack", true);
    }

    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}

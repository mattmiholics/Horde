using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyData : UnitData
{
    [Header("Attributes")]
    public int damage = 25;
    
    [Header("Value")]
    public int deathValue = 75;

    [Header("SFX")]
    public GameObject deathEffect;
    public AudioSource aSource;
    public AudioClip clip;

    public override void TakeDamage (int incomingDamage)
    {
        health -= incomingDamage;
        healthBar.fillAmount = health/startHealth;
        DeathCheck();
        aSource.PlayOneShot(clip);
    }

    public override void DeathCheck()
    {
        if (health <= 0)
        {
            PlayerStats.Instance.money += deathValue;

            GameObject effect = (GameObject)Instantiate(deathEffect, transform.position, Quaternion.identity, WaveSpawner.Instance.effectParent);
            Destroy(effect, 5f);

            Destroy(this.gameObject);
        }
    }
}

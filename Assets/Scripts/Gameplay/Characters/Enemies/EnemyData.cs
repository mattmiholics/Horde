using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class EnemyData : UnitData
{
    protected Transform target;
    
    [Header("Value")]
    public int deathValue = 75;

    [Header("SFX")]
    public GameObject deathEffect;
    public AudioSource aSource;
    public AudioClip clip;

    [Header("Animation")]
    public Animator animator;

    protected override void Start() 
    {
        startHealth = health;
        canAttack = true;
    }

    public override void DeathCheck()
    {
        if (health <= 0)
        {
            PlayerStats.Instance.money += deathValue;
            Debug.Log("Enemy killed!");
            GameObject effect = (GameObject)Instantiate(deathEffect, transform.position, Quaternion.identity, WaveSpawner.Instance.effectParent);
            Destroy(effect, 5f);

            Death.Invoke();

            Destroy(this.gameObject);
        }
    }

}

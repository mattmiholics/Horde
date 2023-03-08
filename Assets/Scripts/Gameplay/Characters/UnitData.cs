using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class UnitData : MonoBehaviour
{
    protected float startHealth;
    
    [Header("Unit stats")]
    public float health = 100f;

    [Header("Graphic Info")]
    public Image healthBar;
    [Header("Unity Events")]
    public UnityEvent attack;
    public UnityEvent death;

    protected virtual void Start()
    {
        startHealth = health;
        // Debug.Log("Start Health: " + startHealth);
    }

    public virtual void TakeDamage(float incomingDamage)
    {
        health -= incomingDamage;
        // Debug.Log("Damage: " + incomingDamage);
        // Debug.Log("Damage taken, Health: " + health);
        healthBar.fillAmount = health/startHealth;
        DeathCheck();
    }
        
    public virtual void DeathCheck()
    {
        if (health <= 0f)
        {
            death.Invoke();
            Destroy(this.gameObject);
        }
    }
}


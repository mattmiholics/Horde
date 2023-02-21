using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitData : MonoBehaviour
{
    [Header("Unit stats")]
    public int health = 100;

    public virtual void TakeDamage(int incomingDamage)
    {
        health -= incomingDamage;
        DeathCheck();
    }
        
    public virtual void DeathCheck()
    {
        if (health <= 0)
            Destroy(this.gameObject);
    }
}


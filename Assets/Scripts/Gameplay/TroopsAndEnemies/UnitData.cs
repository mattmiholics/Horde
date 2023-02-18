using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitData : MonoBehaviour
{
    [Header("Unit stats")]
    public int health = 100;
    public int damage = 25;

    public void TakeDamage(int incomingDamage)
    {
        health -= incomingDamage;
    }
        
    public void DeathCheck()
    {
        if (health <= 0)
            Destroy(this.gameObject);
    }
}


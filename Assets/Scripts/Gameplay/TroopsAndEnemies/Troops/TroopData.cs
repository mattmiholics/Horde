using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TroopData : UnitData
{
    [Header("Attributes")]
    public float fireRate = 1f;
    public float fireReload = 0f;
    public float range = 15f;
    public int damage = 25;
}

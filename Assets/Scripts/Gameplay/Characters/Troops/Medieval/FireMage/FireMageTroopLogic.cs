using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FireMageTroopLogic : TroopData
{
    private PurchaseTroops purchaseTroopsInstance;
    
    // public override void Attack(UnitData unitData)
    // {
    //     if (isRanged)
    //     {
    //         GameObject bulletParent = GameObject.Find("World/BulletParent");
    //         GameObject bulletObj = (GameObject)Instantiate(bullet, firePoint.position, firePoint.rotation, bulletParent.transform);
    //         FireMageBullet bulletS = bulletObj.GetComponent<FireMageBullet>();

    //         if(bulletS != null)
    //         {
    //             bulletS.Seek(unitData.transform, damage);
    //         }
    //     }
    //     else
    //     {
    //         unitData.TakeDamage(damage);
    //     }
    //     Attacking.Invoke();
    //     StartCoroutine(AttackCooldown(attackCooldown));
    // }
}


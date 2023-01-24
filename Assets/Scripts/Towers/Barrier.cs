using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Barrier : MonoBehaviour
{
    /* 
        Barrier should have following function
        1. Should stop the enemy from advancing
        2. Its own health drops when enemy enters its "area"
        3. Destroy itself when health is 0
    */
    [SerializeField]
    private GameObject BarrierTower;
    [SerializeField]
    private float health;
    [SerializeField]
    private string enemyTag = "Enemy";

    void Start()
    {
        InvokeRepeating("CheckHealth", 0f, .1f);
    }

    private void CheckHealth()
    {
        if (this.health <= 0)
        {
            Destroy(BarrierTower);
        }
    }
    private void OnTriggerStay(Collider other) 
    {
        if (other.tag == enemyTag)
        {
            if (other.TryGetComponent<NavMeshAgent>(out NavMeshAgent nma))
            {
                nma.velocity = Vector3.zero;
            }
            TakeDamage();
        }
        Debug.Log("Hit, Health is " + health);
    }

    void TakeDamage()
    {
        health--;
    }
}

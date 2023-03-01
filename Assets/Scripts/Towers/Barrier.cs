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
    private GameObject barrierTower;
    [SerializeField]
    private float health;
    [SerializeField]
    private string enemyTag = "Enemy";

    GameObject enemyObj;
    float enemyOriginalSpeed;
    private List<Agent> agentList;

    void Start()
    {
        agentList = new List<Agent>();
        InvokeRepeating("CheckHealth", 0f, .1f);
    }

    private void CheckHealth()
    {
        if (this.health <= 0)
        {
            TowerHelper.RemoveTower(TowerEditor.Instance, barrierTower.GetComponent<TowerData>());
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == enemyTag)
        {
            Agent agent = other.GetComponentInParent<Agent>();
            agent.movementMultiplier = 0f;
            agentList.Add(agent);
        }
    }

    private void OnDestroy() 
    {
        agentList.ForEach(a => a.movementMultiplier = 1f);
    }
    private void OnTriggerStay(Collider other) 
    {
        if (other.tag == enemyTag)
        {
            TakeDamage();
        }
    }

    void TakeDamage()
    {
        health--;
    }
}

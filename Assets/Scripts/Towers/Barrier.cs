using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using UnityEngine.Events;
using Sirenix.OdinInspector;

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
    [FoldoutGroup("Events")]
    public UnityEvent takeDamage;
    [Header("Graphic Info")]
    public Image healthBar;
    public GameObject damageNumberPrefab;

    GameObject enemyObj;
    float enemyOriginalSpeed;
    float delayTime;
    float currentTime;
    private List<Agent> agentList;
    bool enemyTriggered;

    void Start()
    {
        delayTime = 1f;
        currentTime = delayTime;
        enemyTriggered = false;
        agentList = new List<Agent>();
        InvokeRepeating("CheckHealth", 0f, 0.1f);
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
        if (agentList != null && agentList.Count > 0)
            agentList.ForEach(a => a.movementMultiplier = 1f);
    }
    private void OnTriggerStay(Collider other) 
    {
        currentTime -= Time.deltaTime;
        if (other.tag == enemyTag && currentTime <= 0)
        {
            // TakeDamage();
            takeDamage.Invoke();
            currentTime = delayTime;
        }
    }

    public void TakeDamage()
    {
        health--;
        // Debug.Log("Health: " + health);
    }
}

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
    private float hitPoints;
    [SerializeField]
    private LayerMask enemyLayer;
    [FoldoutGroup("Events")]
    public UnityEvent Hit;
    [Header("Graphic Info")]
    public Image healthBar;

    GameObject enemyObj;
    float enemyOriginalSpeed;
    public float delayTime;
    [ReadOnly]
    public float currentTime;
    private List<Agent> agentList;

    private float startHitPoints;

    void Start()
    {
        //delayTime = 1f;
        currentTime = delayTime;
        agentList = new List<Agent>();
        startHitPoints = hitPoints;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (enemyLayer == (enemyLayer | (1 << other.gameObject.layer)))
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
        if (currentTime <= 0 && enemyLayer == (enemyLayer | (1 << other.gameObject.layer)))
        {
            TakeDamage();
            Hit.Invoke();
            currentTime = delayTime;
        }
    }

    public void TakeDamage()
    {
        hitPoints--;

        healthBar.fillAmount = hitPoints / startHitPoints;

        if (this.hitPoints <= 0)
        {
            TowerHelper.RemoveTower(TowerEditor.Instance, barrierTower.GetComponent<TowerData>());
        }
        // Debug.Log("Health: " + health);
    }
}

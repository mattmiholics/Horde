using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireMageDamageField : MonoBehaviour
{
    float currentTime;
    float currentDamageDelayTime;

    [Header("Attributes")]
    public float countDownTime = 3f;
    public float damageDelayTime = 1f;
    public float damage = 10f;
    public LayerMask enemyLayer;
    
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Instantiated damage field");
        currentTime = countDownTime;
        currentDamageDelayTime = damageDelayTime;
    }

    // Update is called once per frame
    void Update()
    {
        currentTime -= Time.deltaTime;
        // currentDamageDelayTime -= Time.deltaTime;
        // if (currentDamageDelayTime <= 0)
        // {
        //     foreach (GameObject troop in )
        // }
        if (currentTime <= 0)
        {
            Destroy(this.gameObject);
        }
    }

    private void OnTriggerStay(Collider other) 
    {
        currentDamageDelayTime -= Time.deltaTime;
        if (other.gameObject.layer == enemyLayer && currentTime <= 0)
        {
            // TakeDamage();
            Debug.Log("Dealing area damage to enemy");
            other.gameObject.GetComponentInParent<EnemyData>().TakeDamage(20f);
            currentDamageDelayTime = damageDelayTime;
        }
    }
}

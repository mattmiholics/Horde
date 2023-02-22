using UnityEngine;

public class Bullet : MonoBehaviour
{
    protected Transform target;

    public float speed = 70f;
    public GameObject impactEffect;
    public float damage = 50f;

    public void Seek(Transform _target, float damage)
    {
        Debug.Log("Attack Player Seek");
        this.damage = damage;
        target = _target;
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log("Attack Player Update 1");
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 dir = target.position - transform.position;
        float distanceThisFrame = speed * Time.deltaTime;

        Debug.Log("Attack Player Update 2");

        if(dir.magnitude <= distanceThisFrame)
        {
            Debug.Log("Attack Player Hit Target 1");
            HitTarget();
            return;
        }

        transform.Translate(dir.normalized * distanceThisFrame, Space.World);
    }

    void HitTarget()
    {
        EnemyData e = target.GetComponent<EnemyData>();
        TroopData p = target.transform.parent.gameObject.transform.parent.gameObject.GetComponent<TroopData>();
        // Debug.Log("Attack Player Hit Target 2: " + p.name);

        if(e != null)
        {
            e.TakeDamage(damage);
        }
        
        if(p != null)
        {
            Debug.Log("Attack Player Take Damage: " + damage);
            p.TakeDamage(damage);
        }

        GameObject effectInst = (GameObject)Instantiate(impactEffect, transform.position, transform.rotation, WaveSpawner.Instance.effectParent);
        Destroy(effectInst, 2f);
        Destroy(gameObject.gameObject);
        //Destroy(target.gameObject);
    }
}

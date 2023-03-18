using UnityEngine;
using UnityEngine.SceneManagement;

public class Projectile : MonoBehaviour
{
    [HideInInspector]
    public ObjectPooler projectilePool;
    [HideInInspector]
    public ObjectPooler projectileEffectPool;
    protected Transform target;
    public float speed = 70f;
    public GameObject projectileEffectPrefab;
    public float damage = 50f;

    protected void Awake()
    {
        if (projectileEffectPrefab)
        {
            GameObject projectileEffectTemp = GameObject.Find(projectileEffectPrefab.name + "Parent");
            if (!projectileEffectTemp)
            {
                projectileEffectPool = new GameObject(projectileEffectPrefab.name + "Parent").AddComponent<ObjectPooler>();
                projectileEffectPool.prefab = projectileEffectPrefab;
                SceneManager.MoveGameObjectToScene(projectileEffectPool.gameObject, gameObject.scene);
            }
            else
            {
                projectileEffectPool = projectileEffectTemp.GetComponent<ObjectPooler>();
            }
        }
    }
    public void Seek(Transform _target, float damage)
    {
        gameObject.transform.LookAt(_target);
        //Debug.Log("Attack Player Seek");
        this.damage = damage;
        target = _target;
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        //Debug.Log("Attack Player Update 1");
        if (target == null)
        {
            projectilePool.Destroy(gameObject);
            return;
        }

        Vector3 dir = target.position - transform.position;
        float distanceThisFrame = speed * Time.deltaTime;

        //Debug.Log("Attack Player Update 2");

        if(dir.magnitude <= distanceThisFrame)
        {
            //Debug.Log("Attack Player Hit Target 1");
            HitTarget();
            return;
        }

        transform.Translate(dir.normalized * distanceThisFrame, Space.World);
    }

    protected virtual void HitTarget()
    {
        EnemyData e = target.GetComponentInParent<EnemyData>();
        TroopData p = target.transform.parent.gameObject.transform.parent.gameObject.GetComponent<TroopData>();
        // Debug.Log("Attack Player Hit Target 2: " + p.name);
        

        if(e != null)
        {
            e.TakeDamage(damage);
        }
        
        if(p != null)
        {
            //Debug.Log("Attack Player Take Damage: " + damage);
            p.TakeDamage(damage);
        }

        // GameObject effectInst = (GameObject)Instantiate(impactEffect, transform.position, transform.rotation, WaveSpawner.Instance.effectParent);
        if (projectileEffectPool)
            projectileEffectPool.Create(transform.position, transform.rotation, 2f);
        projectilePool.Destroy(gameObject);
        
    }
}

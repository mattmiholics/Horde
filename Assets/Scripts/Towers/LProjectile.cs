using UnityEngine;
using UnityEngine.SceneManagement;

public class LProjectile : MonoBehaviour
{
    [HideInInspector]
    public ObjectPooler projectilePool;
    [HideInInspector]
    public ObjectPooler projectileEffectPool;
    private Transform target;

    public float speed = 70f;
    public GameObject projectileEffectPrefab;
    private int damage = 25;
    private int chain = 0;
    private int ChainDamage = 0;

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

    public void Seek(Transform nTarget, int damage, int chain, int chainDamage)
    {
        this.damage = damage;
        target = nTarget;
        this.chain = chain;
        this.ChainDamage = chainDamage;
        gameObject.transform.LookAt(nTarget);
    }




    // Update is called once per frame
    void Update()
    {
        if (target == null)
        {
            projectilePool.Destroy(gameObject);
            return;
        }

        Vector3 dir = target.position - transform.position;
        float distanceThisFrame = speed * Time.deltaTime;

        if (dir.magnitude <= distanceThisFrame)
        {
            HitTarget();
            return;
        }

        transform.Translate(dir.normalized * distanceThisFrame, Space.World);
    }

    void HitTarget()
    {
        EnemyData e = target.GetComponentInParent<EnemyData>();
        TroopData p = target.GetComponent<TroopData>();

        if (e != null)
        {
            e.TakeDamage(damage);
        }

        if (p != null)
        {
            p.TakeDamage(damage);
        }

        // GameObject effectInst = (GameObject)Instantiate(impactEffect, transform.position, transform.rotation, WaveSpawner.Instance.effectParent);
        if (projectileEffectPool)
            projectileEffectPool.Create(transform.position, transform.rotation, 2f);
        projectilePool.Destroy(gameObject);
        //Destroy(target.gameObject);
    }

}

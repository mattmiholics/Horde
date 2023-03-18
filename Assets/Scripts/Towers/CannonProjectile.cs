using UnityEngine;
using UnityEngine.SceneManagement;

public class CannonProjectile : MonoBehaviour
{
    [HideInInspector]
    public ObjectPooler projectilePool;
    [HideInInspector]
    public ObjectPooler projectileEffectPool;
    
    private Transform target;

    public float speed = 70f;
    public GameObject projectileEffectPrefab;
    public string enemyTag = "Enemy";
    public float splashDamageRange = 10f;
    public int damage = 75;
    public int splashDamage = 50;
    // public AudioClip zombieHurt;

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

    public void Seek(Transform _target, int damage)
    {
        target = _target;
        this.damage = damage;
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
        GameObject[] enemies = GameObject.FindGameObjectsWithTag(enemyTag);

        EnemyData e = target.GetComponentInParent<EnemyData>();

        if (e != null)
        {
            e.TakeDamage(damage);
            
        }

        if (projectileEffectPool)
            projectileEffectPool.Create(transform.position, transform.rotation, 2f);
        projectilePool.Destroy(gameObject);

        foreach (GameObject enemy in enemies)
        {
            float dist = Vector3.Distance(target.transform.position, enemy.transform.position);
            if(dist <= splashDamageRange && enemy != target)
            {
                // GameObject effectNst = (GameObject)Instantiate(impactEffect, enemy.transform.position, enemy.transform.rotation);
                if (projectileEffectPool)
                    projectileEffectPool.Create(enemy.transform.position, enemy.transform.rotation, 2f);
                enemy.GetComponent<EnemyData>().TakeDamage(splashDamage);
            }
        }
    }
}

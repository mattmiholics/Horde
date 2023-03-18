using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class Turret : MonoBehaviour
{
    private Transform target;

    public UnityEvent fire;

    [Header("Attributes")]

    public float fireRate = 1f;
    public float fireReload = 0f;
    public float range = 15f;
    public int damage = 50;

    [Header("Unity Setup Fields")]

    public string enemyTag = "Enemy";
    public Transform partToRotate;
    public float rotationSpeed = 7f;

    public GameObject projectilePrefab;
    protected ObjectPooler projectilePool;
    public Transform firePoint;

    protected void Awake()
    {
        if (projectilePrefab)
        {
            GameObject projectileTemp = GameObject.Find(projectilePrefab.name + "Parent");
            if (!projectileTemp)
            {
                projectilePool = new GameObject(projectilePrefab.name + "Parent").AddComponent<ObjectPooler>();
                projectilePool.prefab = projectilePrefab;
                SceneManager.MoveGameObjectToScene(projectilePool.gameObject, gameObject.scene);
            }
            else
            {
                projectilePool = projectileTemp.GetComponent<ObjectPooler>();
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        InvokeRepeating("UpdateTarget", 0f, .5f);
    }

    void UpdateTarget()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag(enemyTag);
        float shortestDistance = Mathf.Infinity;
        GameObject nearestEnemy = null;

        foreach(GameObject enemy in enemies)
        {
            float distToEnemy = Vector3.Distance(transform.position, enemy.transform.position);
            if(distToEnemy < shortestDistance)
            {
                shortestDistance = distToEnemy;
                nearestEnemy = enemy;
            }
        }

        if(nearestEnemy != null && shortestDistance <= range)
        {
            target = nearestEnemy.transform;
        }
        else
        {
            //this was throwing an error in other prefabs
            target = null;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (target == null)
        {
            return;
        }
        //Target Locking
        Vector3 dir = target.transform.position - transform.position;
        Quaternion lookRotation = Quaternion.LookRotation(dir);
        Vector3 rotation = Quaternion.Lerp(partToRotate.rotation,lookRotation,Time.deltaTime * rotationSpeed).eulerAngles;
        partToRotate.rotation = Quaternion.Euler(0f,rotation.y, 0f);

        if(fireReload <= 0)
        {
            fire.Invoke();
            Shoot();
            fireReload = 1 / fireRate;
        }

        fireReload -= Time.deltaTime;

    }

    void Shoot()
    {
        if (projectilePool)
        {
            GameObject projectileObj = projectilePool.Create(firePoint.position, firePoint.rotation);
            Projectile projectileS = projectileObj.GetComponent<Projectile>();
            CannonProjectile cProjectile = projectileObj.GetComponent<CannonProjectile>();
            LProjectile lProjectile = projectileObj.GetComponent<LProjectile>();

            projectileS.projectilePool = projectilePool;

            if(projectileS != null)
            {
                projectileS.Seek(target, damage);
            }
            else if(cProjectile != null)
            {
                cProjectile.Seek(target, damage);
            }
        }
        
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}

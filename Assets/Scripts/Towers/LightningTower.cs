using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class LightningTower : MonoBehaviour
{
    private Transform target;
    public UnityEvent fire;

    [Header("Attributes")]

    public float fireRate = 1f;
    public float fireReload = 0f;
    public float range = 15f;
    public int damage = 50;
    public int chainAmount = 0;
    public int chainDamage = 0;

    [Header("Unity Setup Fields")]

    public string enemyTag = "Enemy";
    public Transform partToRotate;
    public float rotationSpeed = 7f;

    public GameObject projectilePrefab;
    protected ObjectPooler projectilePool;
    public Transform firePoint;

    [Header("Laser Settings")]
    public LineRenderer lr;
    public bool useLaser = false;
    public GameObject laserStart;

    private List<GameObject> enemiesHit = new List<GameObject>();
    private List<Vector3> halfwayPoints = new List<Vector3>();

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

        foreach (GameObject enemy in enemies)
        {
            float distToEnemy = Vector3.Distance(transform.position, enemy.transform.position);
            if (distToEnemy < shortestDistance)
            {
                shortestDistance = distToEnemy;
                nearestEnemy = enemy;
            }
        }

        if (nearestEnemy != null && shortestDistance <= range)
        {
            target = nearestEnemy.transform;
        }
        else
        {
            //this was throwing an error in other prefab
            target = null;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (target == null)
        {
            if (useLaser)
            {
                lr.enabled = false;
            }
            return;
        }
        //Target Locking
        Vector3 dir = target.transform.position - transform.position;
        Quaternion lookRotation = Quaternion.LookRotation(dir);
        Vector3 rotation = Quaternion.Lerp(partToRotate.rotation, lookRotation, Time.deltaTime * rotationSpeed).eulerAngles;
        partToRotate.rotation = Quaternion.Euler(0f, rotation.y, 0f);

        if (fireReload <= 0)
        {
            fire.Invoke();
            Shoot();
            if (useLaser)
            {
                FireLaser();
            }
            fireReload = 1 / fireRate;
        }

        fireReload -= Time.deltaTime;

    }

    void Shoot()
    {
        if (projectilePool)
        {
            GameObject projectileObj = projectilePool.Create(firePoint.position, firePoint.rotation);
            LProjectile lProjectile = projectileObj.GetComponent<LProjectile>();
            lProjectile.projectilePool = projectilePool;
            lProjectile.Seek(target, damage, chainAmount, chainDamage);
        }
    }

    private void FireLaser()
    {
        enemiesHit.Add(target.gameObject);

        //Gets the travel distance of laser in each direction and divides it by 2 for the halfway distance
        float xCord = (System.Math.Abs(gameObject.transform.position.x - target.transform.position.x) / 2);
        float yCord = (System.Math.Abs(gameObject.transform.position.y - target.transform.position.y) / 2);
        float zCord = (System.Math.Abs(gameObject.transform.position.z - target.transform.position.z) / 2);

        //Adds the halfway distance to the samller to the coords to find the halfway point in that direction, then adds a random distance to make a 'lightning' effect
        if(gameObject.transform.position.x <= target.transform.position.x)
        {
            xCord = xCord + gameObject.transform.position.x + Random.Range(-.6f,.6f);
        }
        else
        {
            xCord = xCord + target.transform.position.x + Random.Range(-.6f, .6f);
        }
        if (gameObject.transform.position.y <= target.transform.position.y)
        {
            yCord = yCord + gameObject.transform.position.y + Random.Range(-.2f, .2f);
        }
        else
        {
            yCord = yCord + target.transform.position.y + Random.Range(-.2f, .2f);
        }
        if (gameObject.transform.position.z <= target.transform.position.z)
        {
            zCord = zCord + gameObject.transform.position.z + Random.Range(-.6f, .6f);
        }
        else
        {
            zCord = zCord + target.transform.position.z + Random.Range(-.6f, .6f);
        }

        Vector3 coord = new Vector3(xCord, yCord, zCord);
        halfwayPoints.Add(coord);



        HitEnemiesWithLaser(1);
        lr.enabled = true;
        lr.positionCount = 1 + enemiesHit.ToArray().Length;
        lr.SetPosition(0, laserStart.transform.position);
        int count = 1;
        //Debug.Log(enemiesHit.ToArray().Length);
        foreach (GameObject pos in enemiesHit.ToArray())
        {
            if (pos != null)
            {
                Vector3 newPos = new Vector3(pos.transform.position.x,pos.transform.position.y + 0.5f, pos.transform.position.z);
                lr.SetPosition(count, newPos);
                count++;
            }
        }
        //lr.enabled = false;
        enemiesHit.Clear();
        halfwayPoints.Clear();
        
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, range);
    }

    private void HitEnemiesWithLaser(int count)
    {
        /*
         *  Step 1: Find closeest enemy to the previousTarget
         *  Step 2: Deal damage and find next closest enemy
         *  Step 3: Repeat step 2 until chainAmount
         */
        GameObject prevTarget = target.gameObject;
        while (count <= chainAmount)
        {
            float relativeDist = float.MaxValue;
            GameObject currentTarget = null;
            //Collider[] targetsHit = Physics.OverlapSphere(prevTarget.transform.position, 10);
            GameObject[] enemies = GameObject.FindGameObjectsWithTag(enemyTag);

            foreach (GameObject hitEnemy in enemies)
            {
                float storeVal = Vector3.Distance(prevTarget.transform.position, hitEnemy.gameObject.transform.position);
                if (hitEnemy.gameObject.tag == "Enemy" && storeVal < relativeDist && hitEnemy.gameObject != prevTarget && !enemiesHit.Contains(hitEnemy.gameObject) && storeVal <= 15)
                {
                    relativeDist = storeVal;
                    currentTarget = hitEnemy.gameObject;
                }
            }
            if (currentTarget != null)
            {
                currentTarget.GetComponentInParent<EnemyData>().TakeDamage(chainDamage);
                enemiesHit.Add(currentTarget);
                prevTarget= currentTarget;
            }
            count++;
        }

        /*if (count <= chainAmount)
        {
            float relativeDist = float.MaxValue;
            GameObject currentTarget = null;
            Collider[] targetsHit = Physics.OverlapSphere(previousTarget.position, 10);

            foreach (Collider hitCollider in targetsHit)
            {
                float storeVal = Vector3.Distance(previousTarget.position, hitCollider.gameObject.transform.position);
                int counter = 0;
                if(hitCollider.gameObject.tag == "Enemy" && storeVal < relativeDist && hitCollider.transform != previousTarget && !enemyAlreadyHit(hitCollider.transform))
                {
                    relativeDist = storeVal;
                    currentTarget = hitCollider.gameObject;
                    counter++;
                    Debug.Log("Count: " + counter);
                }

             
            }

            if(currentTarget != null)
            {
                currentTarget.GetComponent<EnemyMovement>().TakeDamage(chainDamage);
                HitEnemiesWithLaser(count++, currentTarget.transform);
            }
        }
        */
    }
    
}
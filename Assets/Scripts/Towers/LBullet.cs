using UnityEngine;

public class LBullet : MonoBehaviour
{
    private Transform target;

    public float speed = 70f;
    public GameObject impactEffect;
    private int damage = 50;
    private int chain = 0;
    private int ChainDamage = 0;



    public void Seek(Transform nTarget, int damage, int chain, int chainDamage)
    {
        this.damage = damage;
        target = nTarget;
        this.chain = chain;
        this.ChainDamage = chainDamage;
    }




    // Update is called once per frame
    void Update()
    {
        if (target == null)
        {
            Destroy(gameObject);
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
        EnemyMovement e = target.GetComponent<EnemyMovement>();
        MovePlayer p = target.GetComponent<MovePlayer>();

        if (e != null)
        {
            e.TakeDamage(damage);
        }

        if (p != null)
        {
            p.TakeDamage(damage);
        }

        ChainTarget(this.chain, this.ChainDamage, target.gameObject, null);
        GameObject effectInst = (GameObject)Instantiate(impactEffect, transform.position, transform.rotation, WaveSpawner.Instance.effectParent);
        Destroy(effectInst, 2f);
        Destroy(gameObject.gameObject);
        //Destroy(target.gameObject);
    }

    private void ChainTarget(int chainAmount, int chainDamage, GameObject currentPos, GameObject prieviousPos)
    {
        float relativeDist = float.MaxValue;
        GameObject currentTarget = null;
        Collider[] targetsHit = Physics.OverlapSphere(target.transform.position, 10);
        foreach (Collider hitCollider in targetsHit)
        {
            float storeVal = Vector3.Distance(target.position, hitCollider.gameObject.transform.position);
            if (hitCollider.gameObject.tag == "Enemy" && storeVal < relativeDist && hitCollider.gameObject != currentPos && hitCollider.gameObject != prieviousPos)
            {
                relativeDist = storeVal;
                currentTarget = hitCollider.gameObject;
            }
        }
        chainAmount--;
        if(currentTarget != null)
        {
            currentTarget.GetComponent<EnemyMovement>().TakeDamage(chainDamage);
            if (chainAmount > 0)
            {
                ChainTarget(chainAmount, chainDamage, currentTarget, currentPos);
            }
            else
            {
                return;
            }
        }
    }
}

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
        PlayerController p = target.GetComponent<PlayerController>();

        if (e != null)
        {
            e.TakeDamage(damage);
        }

        if (p != null)
        {
            p.TakeDamage(damage);
        }

        GameObject effectInst = (GameObject)Instantiate(impactEffect, transform.position, transform.rotation, WaveSpawner.Instance.effectParent);
        Destroy(effectInst, 2f);
        Destroy(gameObject.gameObject);
        //Destroy(target.gameObject);
    }

}

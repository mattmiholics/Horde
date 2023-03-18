using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class ObjectPooler : MonoBehaviour
{
    public GameObject prefab;
    private ObjectPool<GameObject> pool;

    // Start is called before the first frame update
    void Start()
    {
        pool = new ObjectPool<GameObject>(
            () => { GameObject gameObject = Instantiate(prefab); gameObject.transform.SetParent(transform); return gameObject; },
            gameObject => { gameObject.SetActive(true); },
            gameObject => { gameObject.SetActive(false); },
            gameObject => { Destroy(gameObject); },
            false,
            30 // Default pool size (will expand if needed)
            );
    }

    
    public GameObject Create(Vector3 position, Quaternion rotation, float timeTillDestroy = 0)
    {
        GameObject gameObject = pool.Get();
        gameObject.transform.position = position;
        gameObject.transform.rotation = rotation;
        if (timeTillDestroy > 0)
            StartCoroutine(DestroyAfter(gameObject, timeTillDestroy));
        return gameObject;
    }

    public void Destroy(GameObject gameObject, float time = 0)
    {
        if (time > 0)
            StartCoroutine(DestroyAfter(gameObject, time));
        else
            pool.Release(gameObject);
    }

    private IEnumerator DestroyAfter(GameObject gameObject, float time)
    {
        yield return new WaitForSeconds(time);
        pool.Release(gameObject);
    }
}

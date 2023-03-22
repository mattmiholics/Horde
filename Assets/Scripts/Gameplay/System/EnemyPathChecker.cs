using Sirenix.OdinInspector;
using Sirenix.Utilities;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyPathChecker : MonoBehaviour
{
    [ReadOnly, SerializeField] Transform target;
    [SerializeField] int maxNodes = 1500;

    private static EnemyPathChecker _instance;
    public static EnemyPathChecker Instance { get { return _instance; } }
    private void Awake()
    {
        // If an instance of this already exists and it isn't this one
        if (_instance != null && _instance != this)
        {
            // We destroy this instance
            Destroy(this.gameObject);
        }
        else
        {
            // Make this the instance
            _instance = this;
        }
    }

    private void Start()
    {
        target = TowerEditor.Instance.permanentTowerParent.GetComponentInChildren<MainHall>().target;

        WaveSpawner.Instance.spawns.ForEach(a => { StaticAgent sa = a.GetComponent<StaticAgent>(); sa.startPoint = a.spawnPoint; sa.SetTarget(target.position, sa.maxNodes); });
    }

    public bool PathExists()
    {
        List<List<PathPoint>> oldPathPoints = WaveSpawner.Instance.spawns.Select(a => a.GetComponent<StaticAgent>().pathPoints).ToList();
        bool pathExists = true;

        foreach(StaticAgent staticAgent in WaveSpawner.Instance.spawns.Select(a => a.GetComponent<StaticAgent>()).ToList())
        {
            if (!staticAgent.SetTarget(staticAgent.currentTarget, maxNodes))
                pathExists = false;
        }

        if (!pathExists)
            WaveSpawner.Instance.spawns.ForEach((a, index) => a.GetComponent<StaticAgent>().pathPoints = oldPathPoints.ElementAtOrDefault(index));

        return pathExists;
    }
}

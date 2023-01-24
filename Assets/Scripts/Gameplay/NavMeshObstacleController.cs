using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NavMeshObstacleController : MonoBehaviour
{
    private void Start()
    {
        StartCoroutine(GetAllUnitsWithSameTag());
    }

    private IEnumerator GetAllUnitsWithSameTag()
    {
        for (; ;)
        {
            EnableNavMeshObstacleIfTowerIsPlaced();
            
            yield return new WaitForSeconds(0.1f);
        }
    }

    private void EnableNavMeshObstacleIfTowerIsPlaced()
    {
        foreach (TowerData towerData in TowerEditor.Instance.tdList)
        {
            if (towerData.TryGetComponent<NavMeshObstacle>(out NavMeshObstacle nmo))
            {
                if (!towerData.proxy.activeSelf)
                    nmo.enabled = true;
                else
                    nmo.enabled = false;
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NavMeshObstacleController : MonoBehaviour
{
    private void Awake()
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
            if (!towerData.transform.Find("Proxy").gameObject.activeSelf)
            {
                towerData.GetComponent<NavMeshObstacle>().enabled = true;
            }
            else
            {
                towerData.GetComponent<NavMeshObstacle>().enabled = false;
            }
        }
    }
}

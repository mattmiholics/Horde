using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarketBuildingManager : MonoBehaviour
{

    public void payPlayer()
    {
        gameObject.GetComponentInChildren<MarketBuilding>().PayPlayer();
    }
}

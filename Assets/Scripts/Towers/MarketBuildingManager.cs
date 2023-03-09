using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarketBuildingManager : MonoBehaviour
{

    public void payPlayer()
    {
        gameObject.GetComponentInChildren<MarketBuilding>().PayPlayer();
    }

    public void BeginPayout()
    {
        gameObject.GetComponentInChildren<MarketBuilding>().StartPayOut();
    }

    public void StopPayout()
    {
        gameObject.GetComponentInChildren<MarketBuilding>().StopPayOut();
    }
}

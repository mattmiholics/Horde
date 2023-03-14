using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarketBuildingManager : MonoBehaviour
{
    [ReadOnly]
    public int tempCurrentAmountPayed;
    [ReadOnly]
    public bool tempStartPayingOut;

    private void OnEnable()
    {
        WaveSpawner.Instance.WaveStarted += BeginPayout;
        WaveSpawner.Instance.WaveEnded += PayPlayer;
        WaveSpawner.Instance.WaveEnded += StopPayout;
    }

    private void OnDisable()
    {
        WaveSpawner.Instance.WaveStarted -= BeginPayout;
        WaveSpawner.Instance.WaveEnded -= PayPlayer;
        WaveSpawner.Instance.WaveEnded -= StopPayout;
    }

    public void PayPlayer()
    {
        gameObject.GetComponentInChildren<MarketBuilding>(false).PayPlayer();
    }

    public void BeginPayout()
    {
        gameObject.GetComponentInChildren<MarketBuilding>(false).StartPayOut();
    }

    public void StopPayout()
    {
        gameObject.GetComponentInChildren<MarketBuilding>(false).StopPayOut();
        tempStartPayingOut = false;
        tempCurrentAmountPayed = 0;
    }
}

using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarketBuilding : MonoBehaviour
{
    public MarketBuildingNumber payoutNumber;
    [ReadOnly]
    public int currentAmountPayed = 0;
    public int payAmountPerSecond = 10;
    public int moneyCap = 1000;
    private bool startPayingOut = false;

    private float timer = 0;

    public void PayPlayer()
    {
        PlayerStats.Instance.money += (moneyCap - currentAmountPayed);
        currentAmountPayed = 0;
        timer = 0;
    }

    public void StartPayOut()
    {
        startPayingOut = true;
    }

    public void StopPayOut()
    {
        startPayingOut = false;
    }

    private void Update()
    {
        if(startPayingOut == true)
        {
            timer += Time.deltaTime;
            if (timer >= 1 && currentAmountPayed < moneyCap)
            {
                PlayerStats.Instance.money += payAmountPerSecond;
                currentAmountPayed += payAmountPerSecond;
                payoutNumber.BeginAnimation(payAmountPerSecond);        
                timer = 0;
            }
        }
    }

    // This makes it so that if a tower is upgraded mid round it doesn't reset its currentAmountPayed
    private void OnEnable()
    {
        MarketBuildingManager mbm = GetComponentInParent<MarketBuildingManager>();

        if (mbm.tempStartPayingOut)
        {
            startPayingOut = true;
            currentAmountPayed = mbm.tempCurrentAmountPayed;
        }
    }

    private void OnDisable()
    {
        if (startPayingOut)
        {
            MarketBuildingManager mbm = GetComponentInParent<MarketBuildingManager>();
            if (mbm)
            {
                mbm.tempStartPayingOut = true;
                mbm.tempCurrentAmountPayed = currentAmountPayed;
            }
        }
    }
}

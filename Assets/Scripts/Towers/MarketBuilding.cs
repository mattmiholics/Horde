using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarketBuilding : MonoBehaviour
{
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

    private void Update()
    {
        if(startPayingOut == true)
        {
            timer += Time.deltaTime;
            if (timer >= 1 && currentAmountPayed < moneyCap)
            {
                PlayerStats.Instance.money += payAmountPerSecond;
                currentAmountPayed += payAmountPerSecond;
                timer = 0;
            }
        }
    }

    public void StartPayOut()
    {
        startPayingOut = true;
    }

    public void StopPayOut()
    {
        startPayingOut = false;
    }

}

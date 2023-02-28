using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarketBuilding : MonoBehaviour
{
    public int payAmount;

    public void PayPlayer()
    {
            PlayerStats.Instance.money += payAmount;

    }
}

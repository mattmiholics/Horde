using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TrainingManager : MonoBehaviour
{
    public GameObject[] slotIcons;

    private int currentIcon = -1;

    public int slotPos;


    private PurchaseTroops troopQueue;

    private void Start()
    {
        troopQueue = PurchaseTroops.Instance;
    }

    private void Update()
    {
        if (troopQueue.troopQueue.ToArray().Length >= slotPos+1)
        {
            if (troopQueue.troopQueue[slotPos]-1 != currentIcon)
            {
                if (currentIcon != -1)
                {
                    slotIcons[currentIcon].SetActive(false);
                }
                currentIcon = troopQueue.troopQueue[slotPos]-1;
                slotIcons[currentIcon].SetActive(true);
            }
        }
        else
        {
            if (currentIcon != -1)
                slotIcons[currentIcon].SetActive(false);
            currentIcon = -1;
        }
    }
}

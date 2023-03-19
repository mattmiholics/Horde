using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TrainingBarManager : MonoBehaviour
{
    public GameObject trainingBar;
    public GameObject parent;
    private PurchaseTroops pt;

    private void Start()
    {
        pt = PurchaseTroops.Instance;
    }

    private void Update()
    {
        if (pt.trainingTroop)
        {
            parent.SetActive(true);
            trainingBar.SetActive(true);
            trainingBar.GetComponent<Image>().fillAmount = pt.troopSpawnTimer / 6f;
        }
        else
        {
            trainingBar.SetActive(false);
            parent.SetActive(false);
        }
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UpgradeUI : MonoBehaviour
{
    public GameObject towerData;
    public GameObject costT;
    public GameObject levelT;
    public GameObject descT;

    private string upgradeInfo;
    private int cost;
    private int lvl;

    private void Start()
    {
        lvl = towerData.GetComponent<TowerData>().level;
        cost = towerData.GetComponent<TowerData>().costToLvl;
        upgradeInfo = towerData.GetComponent<TowerData>().description;
    }

    public void Update()
    {
        costT.GetComponent<TextMeshProUGUI>().text = "$" + cost;
        levelT.GetComponent<TextMeshProUGUI>().text = lvl.ToString();
        if(upgradeInfo != "None")
        {
            descT.GetComponent<TextMeshProUGUI>().text = upgradeInfo;
        }

        lvl = towerData.GetComponent<TowerData>().level;
        cost = towerData.GetComponent<TowerData>().costToLvl;
        upgradeInfo = towerData.GetComponent<TowerData>().description;
    }
}

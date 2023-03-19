using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class UpgradeBarracks : MonoBehaviour
{
    public GameObject upgradeText;
    public TowerData barracksData;
    // Update is called once per frame
    void Update()
    {

        if (barracksData.level == 3)
        {
            gameObject.SetActive(false);
        }
        else
        {
            upgradeText.GetComponent<TMPro.TextMeshProUGUI>().text = "$" + barracksData.level * 1000;
        }
    }

    public void UpgradeBarracksButton()
    {
        if (PlayerStats.Instance.money >= barracksData.level * 1000 && barracksData.level < 3) {
            PlayerStats.Instance.money -= barracksData.level * 1000;
            barracksData.level++;
        }
    }
}

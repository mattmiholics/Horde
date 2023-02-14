using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using System;

public class TowerData : MonoBehaviour
{
    [PropertyTooltip("This will be automatically assigned at runtime based on the Tower Data Manager")]
    [ReadOnly]
    public int id;

    [Space]
    public bool editable;
    public bool placeBarriers = true;

    public GameObject upgradeUI;
    public GameObject infoText;
    //private GameObject cancelButton = upgradeUI.transform.GetChild(2);

    public int cost;
    public int costToLvl = 350;
    public int lvl = 0;
    [Space]
    public string type;
    [Space]
    [MinValue(1)]
    public Vector3Int size;
    [PropertyTooltip("The checker size will only evaluate the bounds volume with its values, but still place barriers using the regular size")]
    public bool useChecker;
    [ShowIf("useChecker")]
    public bool showCheckerGizmo;
    [MinValue(1)]
    [ShowIf("useChecker")]
    public Vector3Int checkerSize;
    [ShowIf("useChecker")]
    public Vector3 checkerOffset;
    [Space]
    [ReadOnly]
    public int rotation;
    public GameObject main;
    public GameObject proxy;
    [Space]
    public GameObject lvl2Main;
    public GameObject lvl2Proxy;
    [Space]
    public GameObject lvl3Main;
    public GameObject lvl3Proxy;


    [Space]
    public bool showGizmo;

    private static TowerData _instance;
    public static TowerData Instance { get { return _instance; } }

    [SerializeField]
    private List<TowerUpgradeData> upgradeDataList;


    [Serializable]
    private class TowerUpgradeData
    {
        public GameObject main;
        public GameObject proxy;
        [MinValue(0)]
        public int costToLvl;
    }



#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (showGizmo)
        {
            Gizmos.color = new Color(0, 1, 0, 0.4f);
                
            Gizmos.DrawCube(transform.position + new Vector3(0, size.y / 2f, 0), size);
        }

        if (showCheckerGizmo)
        {
            Gizmos.color = new Color(1, 0, 1, 0.4f);

            Gizmos.DrawCube(transform.position + checkerOffset + new Vector3(0, checkerSize.y / 2f, 0), checkerSize);
        }
    }
#endif

    public void BeginUpgrade()
    {
        if(lvl+1 < upgradeDataList.ToArray().Length)
        {
            this.upgradeUI.SetActive(true);
            UpgradeManager.Instance.GetComponent<UpgradeManager>().GetInfo(upgradeDataList[lvl].costToLvl, gameObject, lvl, type, this.upgradeUI, this.infoText);

        }
        /*
        if (lvl < 3)
        {
            this.upgradeUI.SetActive(true);
            UpgradeManager.Instance.GetComponent<UpgradeManager>().GetInfo(costToLvl, gameObject, lvl, type, this.upgradeUI, this.infoText);
        }
        */
    }


    public void Upgrade()
    {
        upgradeDataList[lvl].main.SetActive(false);
        upgradeDataList[lvl + 1].main.SetActive(true);
        lvl++;

        /*
        if (lvl == 1)
        {
            main.SetActive(false);
            lvl2Main.SetActive(true);
            main = lvl2Main;
            proxy = lvl2Proxy;
            lvl++;
            costToLvl *= 2;
        }else if(lvl == 2)
        {
            main.SetActive(false);
            lvl3Main.SetActive(true);
            main = lvl3Main;
            proxy = lvl3Proxy;
            lvl++;
        }
        */
    }

    public void upgradeTest()
    {
        Debug.Log("test");
    }
}

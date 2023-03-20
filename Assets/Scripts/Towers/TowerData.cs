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
    public bool selectable = false;
    public bool placeBarriers = true;

    public GameObject upgradeUI;
    //private GameObject cancelButton = upgradeUI.transform.GetChild(2);

    public int cost;
    [MinValue(1)]
    //[ReadOnly] 
    public int level = 1;
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
    [HideInInspector]
    public GameObject Main => upgradeDataList.ElementAtOrDefault(level - 1).main;
    [HideInInspector]
    public GameObject Proxy => upgradeDataList.ElementAtOrDefault(level - 1).proxy;

    [Space]
    public bool showGizmo;

    public bool isMaxLevel = false;
    public bool isBarracks = false;

    public GameObject rangeSphere;

    private static TowerData _instance;
    public static TowerData Instance { get { return _instance; } }

    [ValidateInput("@upgradeDataList.Count > 0")] // Requires there to be at least one level
    [SerializeField]
    private List<TowerUpgradeData> upgradeDataList;
    [Header("Unity Events")]
    public UnityEvent upgrade;
    [Serializable]
    private class TowerUpgradeData
    {
        [Required]
        public GameObject main;
        [Required]
        public GameObject proxy;
        [MinValue(0)]
        public int costToLvl;
    }

    public string description = "None";
    public int costToLvl { get { return upgradeDataList[level] != null ? upgradeDataList[level].costToLvl : 0; } }

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
        if (level < upgradeDataList.ToArray().Length || isBarracks)
        {
            this.upgradeUI.SetActive(true);
        }
        else if (upgradeUI != null)
        {
            this.upgradeUI.SetActive(false);
            isMaxLevel = true;
        }
        if(level == upgradeDataList.ToArray().Length)
        {
            isMaxLevel = true;
        }
    }


    public void Upgrade()
    {
        upgrade.Invoke();
        Main.SetActive(false);
        level++;
        Main.SetActive(true);
        if (Main.TryGetComponent(out Outline outline))
        {
            outline.OutlineColor = Color.yellow;
            outline.enabled = true;
        }
        BeginUpgrade();

    }

    public void UpgradeTurret()
    {
        if (PlayerStats.Instance.money >= upgradeDataList[level].costToLvl)
        {
            
            PlayerStats.Instance.money -= upgradeDataList[level].costToLvl;
            Main.SetActive(false);
            level++;
            Main.SetActive(true);
            upgrade.Invoke();
            
        }
        if(level == upgradeDataList.ToArray().Length)
        {
            this.upgradeUI.SetActive(false);
            isMaxLevel = true;
        }
    }

    public void cancel()
    {
        if (Main.TryGetComponent(out Outline outline))
        {
            outline.OutlineColor = Color.blue;
            outline.enabled = false;
        }
        if (rangeSphere != null)
            rangeSphere.SetActive(false);
        UpgradeManager.Instance.GetComponent<UpgradeManager>().towerDataSelected = null;
        upgradeUI.SetActive(false);
    }
}

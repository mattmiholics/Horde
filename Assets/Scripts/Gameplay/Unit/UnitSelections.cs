using System.Collections.Generic;
using UnityEngine;

public class UnitSelections : MonoBehaviour
{
    public List<GameObject> unitList = new List<GameObject>();
    public List<GameObject> unitsSelected = new List<GameObject>();

    public Transform troopParent;
    public Transform troopSelectionParent;

    private static UnitSelections _instance;
    public static UnitSelections Instance { get { return _instance; } }

    void Awake()
    {
        // If an instance of this already exists and it isn't this one
        if (_instance != null && _instance != this)
        {
            // We destroy this instance
            Destroy(this.gameObject);
        }
        else
        {
            // Make this the instance
            _instance = this;
        }
    }

    void Start()
    {
        InvokeRepeating("CheckTroopSelection", 0f, .1f);
    }

    void CheckTroopSelection()
    {
        foreach (var unit in unitsSelected)
        {
            if (LayerMask.LayerToName(unit.gameObject.layer) == "Troop")
            {
                unit.transform.SetParent(troopSelectionParent);
                unit.transform.Find("Offset/IsSelected").gameObject.SetActive(true);
            }
        }
    }

    public void ClickSelect(GameObject unitToAdd)
    {
        Debug.Log("Clicked " + unitToAdd.name);
        DeselectAll();
        unitsSelected.Add(unitToAdd);
        //unitToAdd.transform.GetChild(0).gameObject.SetActive(true);
        if (LayerMask.LayerToName(unitToAdd.gameObject.layer) == "Troop")
        {
            unitToAdd.transform.SetParent(troopSelectionParent);
            unitToAdd.transform.Find("Offset/IsSelected").gameObject.SetActive(true);
        }
    }

    public void ShiftClickSelect(GameObject unitToAdd)
    {
        if (!unitsSelected.Contains(unitToAdd))
        {
            unitsSelected.Add(unitToAdd);
            //unitToAdd.transform.GetChild(0).gameObject.SetActive(true);
            if (LayerMask.LayerToName(unitToAdd.gameObject.layer) == "Troop")
            {
                unitToAdd.transform.SetParent(troopSelectionParent);
                unitToAdd.transform.Find("Offset/IsSelected").gameObject.SetActive(true);
            }
        }
        else
        {
            if (LayerMask.LayerToName(unitToAdd.gameObject.layer) == "Troop")
            {
                unitToAdd.transform.SetParent(troopParent);
                unitToAdd.transform.Find("Offset/IsSelected").gameObject.SetActive(false); 
            }
            //unitToAdd.transform.GetChild(0).gameObject.SetActive(false);
            unitsSelected.Remove(unitToAdd);
        }
    }

    public void DragSelect(GameObject unitToAdd)
    {
        if (!unitsSelected.Contains(unitToAdd))
        {
            unitsSelected.Add(unitToAdd);
            //unitToAdd.transform.GetChild(0).gameObject.SetActive(true);
            // foreach (var unit in unitsSelected)
            // {
            //     if (LayerMask.LayerToName(unit.gameObject.layer) == "Troop")
            //     {
            //         unit.transform.SetParent(troopSelectionParent);
            //     }
            // }
        }
    }

    public void DeselectAll()
    {
        if (unitsSelected != null && unitsSelected.Count > 0)
        {
            foreach (var unit in unitsSelected)
            {
                if (LayerMask.LayerToName(unit.gameObject.layer) == "Troop")
                {
                    unit.transform.SetParent(troopParent);
                    unit.transform.Find("Offset/IsSelected").gameObject.SetActive(false);
                }
                //unit.transform.GetChild(0).gameObject.SetActive(false);
            }

            unitsSelected.Clear();
        }
    }
}

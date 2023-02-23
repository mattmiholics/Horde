using System.Collections.Generic;
using UnityEngine;

public class UnitSelections : MonoBehaviour
{
    public List<GameObject> unitList = new List<GameObject>();
    public List<GameObject> unitsSelected = new List<GameObject>();
    [Space]
    public Transform troopParent;
    public Transform troopSelectionParent;
    [Space]
    public LayerMask troopLayer;

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
            if (troopLayer == (troopLayer | (1 << unit.gameObject.layer)))
            {
                unit.transform.SetParent(troopSelectionParent);
                unit.GetComponent<TroopPathfinding>().isSelected.SetActive(true);
            }
        }
    }

    public void ClickSelect(GameObject unitToAdd)
    {
        //Debug.Log("Clicked " + unitToAdd.name);
        DeselectAll();
        unitsSelected.Add(unitToAdd);

        if (troopLayer == (troopLayer | (1 << unitToAdd.gameObject.layer)))
        {
            unitToAdd.transform.SetParent(troopSelectionParent);
            unitToAdd.GetComponent<TroopPathfinding>().isSelected.SetActive(true);
        }
    }

    public void ShiftClickSelect(GameObject unitToAdd)
    {
        if (!unitsSelected.Contains(unitToAdd))
        {
            unitsSelected.Add(unitToAdd);
            if (troopLayer == (troopLayer | (1 << unitToAdd.gameObject.layer)))
            {
                unitToAdd.transform.SetParent(troopSelectionParent);
                unitToAdd.GetComponent<TroopPathfinding>().isSelected.SetActive(true);
            }
        }
        else
        {
            if (troopLayer == (troopLayer | (1 << unitToAdd.gameObject.layer)))
            {
                unitToAdd.transform.SetParent(troopParent);
                unitToAdd.GetComponent<TroopPathfinding>().isSelected.SetActive(false); 
            }
            unitsSelected.Remove(unitToAdd);
        }
    }

    public void DragSelect(GameObject unitToAdd)
    {
        if (!unitsSelected.Contains(unitToAdd))
        {
            unitsSelected.Add(unitToAdd);
            foreach (var unit in unitsSelected)
            {
                 if (troopLayer == (troopLayer | (1 << unit.gameObject.layer)))
                 {
                    unit.transform.SetParent(troopSelectionParent);
                    unit.GetComponent<TroopPathfinding>().isSelected.SetActive(true);
                }
            }
        }
    }

    public void DeselectAll()
    {
        if (unitsSelected != null && unitsSelected.Count > 0)
        {
            foreach (var unit in unitsSelected)
            {
                if (troopLayer == (troopLayer | (1 << unit.gameObject.layer)))
                {
                    unit.transform.SetParent(troopParent);
                    unit.GetComponent<TroopPathfinding>().isSelected.SetActive(false);
                }
            }

            unitsSelected.Clear();
        }
    }
}

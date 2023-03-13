using System.Collections.Generic;
using System.Windows.Forms;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.UI.CanvasScaler;

public class UnitSelections : MonoBehaviour
{
    public List<GameObject> unitList = new List<GameObject>();
    public List<GameObject> unitsSelected = new List<GameObject>();
    [Space]
    public Transform troopParent;
    public Transform troopSelectionParent;
    [Space]
    public LayerMask troopLayer;

    //[HideInInspector]
    public List<Vector3Int> finalPositions;

    private static UnitSelections _instance;
    public static UnitSelections Instance { get { return _instance; } }

    public GameObject unitHovered;

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
        finalPositions = new List<Vector3Int>();
        InvokeRepeating("CheckTroopSelection", 0f, .1f);
    }

    private void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit hit;

        if (!CanvasHitDetector.Instance.IsPointerOverUI() && Physics.Raycast(ray, out hit, Mathf.Infinity, troopLayer))
        {
            GameObject ud = hit.transform.gameObject;
            if (ud != unitHovered)
            {
                if (unitHovered && !unitsSelected.Contains(unitHovered))
                {
                    unitHovered.GetComponent<TroopPathfinding>().modelToHighlight.GetComponent<Outline>().enabled = false;
                }

                unitHovered = ud;

                unitHovered.GetComponent<TroopPathfinding>().modelToHighlight.GetComponent<Outline>().OutlineColor = Color.blue;
                unitHovered.GetComponent<TroopPathfinding>().modelToHighlight.GetComponent<Outline>().enabled = true;
                    
            }
            
        }
        else if (unitHovered && !unitsSelected.Contains(unitHovered))
        {
            unitHovered.GetComponent<TroopPathfinding>().modelToHighlight.GetComponent<Outline>().enabled = false;
           
            unitHovered = null;
        }
    }

    void CheckTroopSelection()
    {
        foreach (var unit in unitsSelected)
        {
            if (unit && troopLayer == (troopLayer | (1 << unit.gameObject.layer)))
            {
                unit.transform.SetParent(troopSelectionParent);
                unit.GetComponent<TroopPathfinding>().modelToHighlight.GetComponent<Outline>().OutlineColor = Color.yellow;
                unit.GetComponent<TroopPathfinding>().modelToHighlight.GetComponent<Outline>().enabled = true;
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
            unitToAdd.GetComponent<TroopPathfinding>().modelToHighlight.GetComponent<Outline>().OutlineColor = Color.yellow;
            unitToAdd.GetComponent<TroopPathfinding>().modelToHighlight.GetComponent<Outline>().enabled = true;
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
                unitToAdd.GetComponent<TroopPathfinding>().modelToHighlight.GetComponent<Outline>().OutlineColor = Color.yellow;
                unitToAdd.GetComponent<TroopPathfinding>().modelToHighlight.GetComponent<Outline>().enabled = true;
            }
        }
        else
        {
            if (troopLayer == (troopLayer | (1 << unitToAdd.gameObject.layer)))
            {
                unitToAdd.transform.SetParent(troopParent);
                unitToAdd.GetComponent<TroopPathfinding>().modelToHighlight.GetComponent<Outline>().OutlineColor = Color.blue;
                unitToAdd.GetComponent<TroopPathfinding>().modelToHighlight.GetComponent<Outline>().enabled = false;
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
                 if (unit && troopLayer == (troopLayer | (1 << unit.gameObject.layer)))
                 {
                    unit.transform.SetParent(troopSelectionParent);
                    unit.GetComponent<TroopPathfinding>().modelToHighlight.GetComponent<Outline>().OutlineColor = Color.yellow;
                    unit.GetComponent<TroopPathfinding>().modelToHighlight.GetComponent<Outline>().enabled = true;
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
                if (unit && troopLayer == (troopLayer | (1 << unit.gameObject.layer)))
                {
                    unit.transform.SetParent(troopParent);
                    unit.GetComponent<TroopPathfinding>().modelToHighlight.GetComponent<Outline>().OutlineColor = Color.blue;
                    unit.GetComponent<TroopPathfinding>().modelToHighlight.GetComponent<Outline>().enabled = false;
                }
            }

            unitsSelected.Clear();
        }
    }

    public Vector3 FindNearestAvailiablePosition(Agent agent, Vector3 targetPosition)
    {
        Vector3Int currentPositionInt = Vector3Int.RoundToInt(agent.currentTarget);
        Vector3Int targetPositionInt = Vector3Int.RoundToInt(targetPosition);

        if (currentPositionInt == targetPositionInt)
            return currentPositionInt;

        if (!finalPositions.Contains(targetPositionInt))
            agent.SetTarget(targetPositionInt, 150);

        int index = 1;
        int size = 3;
        int borderSize = (int)(Mathf.Pow(size, 2) - Mathf.Pow(size - 2, 2)); // 8
        Vector3Int origionalTarget = targetPositionInt;

        while (finalPositions.Contains(targetPositionInt) || !agent.SetTarget(targetPositionInt, 150))
        {
            Vector3Int offset = new Vector3Int();
            int distance = (int)Mathf.Floor(size / 2f);

            if (index <= borderSize / 4f) // Less than 2
            {
                offset = new Vector3Int(distance, 0, distance - index);
            }
            else if (index <= borderSize / 2f) // Less than 4
            {
                offset = new Vector3Int(distance - (index - (int)(borderSize / 4f)), 0, -distance);
            }
            else if (index <= borderSize * 0.75f) // Less than 6
            {
                offset = new Vector3Int(-distance, 0, -distance + (index - (int)(borderSize / 2f)));
            }
            else
            {
                offset = new Vector3Int(-distance + (index - (int)(borderSize * 0.75f)), 0, distance);
            }

            targetPositionInt = World.Instance.GetSurfaceHeightPosition(origionalTarget + offset, true, true);

            index++;

            if (index > borderSize)
            {
                index = 1;
                size += 2;
                borderSize = (int)(Mathf.Pow(size, 2) - Mathf.Pow(size - 2, 2));
            }
        }

        finalPositions.Add(targetPositionInt);
        finalPositions.RemoveAll(p => p == currentPositionInt);
        return targetPositionInt;
    }
}

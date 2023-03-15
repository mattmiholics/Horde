using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class CanvasHitDetector : MonoBehaviour
{
    private GraphicRaycaster _graphicRaycaster;

    private static CanvasHitDetector _instance;
    private static List<CanvasHitDetector> _allInstances;
    public static CanvasHitDetector Instance { get { return _instance; } }
    public static List<CanvasHitDetector> AllInstances { get { return _allInstances; } }

    private void Awake()
    {
        // If an instance of this already exists and it isn't this one
        if (_instance != null && _instance != this)
        {
            _allInstances.Add(this);
        }
        else
        {
            // Make this the instance
            _instance = this;
            _allInstances = new List<CanvasHitDetector>();
            _allInstances.Add(this);
        }
    }

    private void Start()
    {
        // This instance is needed to compare between UI interactions and
        // game interactions with the mouse.
        _graphicRaycaster = GetComponent<GraphicRaycaster>();
    }

    public bool IsPointerOverUI()
    {
        // Obtain the current mouse position.
        var mousePosition = Mouse.current.position.ReadValue();

        // Create a pointer event data structure with the current mouse position.
        var pointerEventData = new PointerEventData(EventSystem.current);
        pointerEventData.position = mousePosition;

        // Use the GraphicRaycaster instance to determine how many UI items
        // the pointer event hits.  If this value is greater-than zero, skip
        // further processing.
        var results = new List<RaycastResult>();

        _allInstances = _allInstances.Where(chd => chd._graphicRaycaster != null).ToList();

        foreach (GraphicRaycaster graphicRaycaster in _allInstances.Select(chd => chd._graphicRaycaster))
        {
            graphicRaycaster.Raycast(pointerEventData, results);
            if (results.Count > 0)
                return true;
        }
        return false;
    }
}

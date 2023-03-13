using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using Sirenix.OdinInspector;
using System.Collections;

public class UnitSelectionLogic : MonoBehaviour
{
    // Graphical
    [SerializeField]
    private RectTransform boxVisual;
    public GameObject groundMarker;

    public LayerMask troopLayer;
    public LayerMask ground;
    [Space]
    public bool mapStartEnabled = true;

    [Header("Controls")]
    private PlayerInput _playerInput;
    [StringInList(typeof(PropertyDrawersHelper), "AllActionMaps")] public string unitActionMap;
    [Space]
    [StringInList(typeof(PropertyDrawersHelper), "AllPlayerInputs")] public string destinationControl;
    private InputAction _destination;
    [StringInList(typeof(PropertyDrawersHelper), "AllPlayerInputs")] public string selectControl;
    private InputAction _select;
    [StringInList(typeof(PropertyDrawersHelper), "AllPlayerInputs")] public string unionSelectControl;
    private InputAction _unionSelect;

    [FoldoutGroup("Events")]
    public UnityEvent unitSelected;
    [FoldoutGroup("Events")]
    public UnityEvent unitDeselected;
    [FoldoutGroup("Events")]
    public UnityEvent markerPlaced;

    // Logical
    private Rect selectionBox;

    private Canvas _canvas;

    private Vector2 startPosition;
    private Vector2 endPosition;

    private Coroutine dragging;

    // Awake is called when the script instance is being loaded
    private void Awake()
    {
        _playerInput = FindObjectOfType<PlayerInput>();

        _destination = _playerInput.actions[destinationControl];
        _select = _playerInput.actions[selectControl];
        _unionSelect = _playerInput.actions[unionSelectControl];

        if (mapStartEnabled)
            EnableMap();
    }

    // Start is called before the first frame update
    void Start()
    {
        _canvas = Root.Instance.GetComponent<Canvas>();

        /*GameObject UI = Root.Instance.UIGroups.Where(obj => obj.name == selectionUI).SingleOrDefault();
        if (UI != null)
            boxVisual = UI.transform.GetChild(0).GetComponent<RectTransform>();*/

        startPosition = Vector2.zero;
        endPosition = Vector2.zero;
        DrawVisual();
    }

    public void EnableMap()
    {
        _playerInput.actions.FindActionMap(unitActionMap, true).Enable();
    }

    public void DisableMap()
    {
        _playerInput.actions.FindActionMap(unitActionMap, true).Disable();
    }

    private void OnEnable()
    {
        _destination.performed += OnRightClick;
        _select.performed += OnClick;
        _select.canceled += OnRelease;
    }

    private void OnDisable()
    {
        _destination.performed -= OnRightClick;
        _select.performed -= OnClick;
        _select.canceled -= OnRelease;
    }

    private void OnClick(InputAction.CallbackContext context)
    {
        if (UnitSelections.Instance == null)
            return;

        // Dragging logic
        startPosition = Mouse.current.position.ReadValue();
        selectionBox = new Rect();
        dragging = StartCoroutine(OnDrag());

        // Single click logic
        groundMarker.SetActive(false);
        RaycastHit hit = new RaycastHit();
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, troopLayer))
        {
            GameObject clickedUnit = hit.collider.GetComponentInParent<Agent>().gameObject;

            // If we hit a clickable object
            if (_unionSelect.IsPressed())
            {
                // Shift clicked
                UnitSelections.Instance.ShiftClickSelect(clickedUnit);
            }
            else
            {
                // Normal Clicked
                UnitSelections.Instance.ClickSelect(clickedUnit);
            }
        }
        else
        {
            // If we didn't and we're not shift clicking
            if (!_unionSelect.IsPressed())
            {
                if (UnitSelections.Instance.unitsSelected.Count > 0)
                    unitDeselected?.Invoke();
                UnitSelections.Instance.DeselectAll();
            }
        }
    }

    private void OnRightClick(InputAction.CallbackContext context)
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, ground) && World.Instance != null)
        {
            groundMarker.transform.position = World.Instance.GetSurfaceHeightPosition(World.Instance.GetBlockPos(hit, true), true) - new Vector3(0, 0.5f, 0);
            groundMarker.SetActive(true);
            markerPlaced?.Invoke();
        }
    }

    private IEnumerator OnDrag()
    {
        for (; ; )
        {
            endPosition = Mouse.current.position.ReadValue();

            DrawVisual();
            DrawSelection(endPosition);
            foreach (var unit in UnitSelections.Instance.unitList)
            {
                // If unit is within the bounds of the selection rect
                if (selectionBox.Contains(Camera.main.WorldToScreenPoint(unit.transform.position)))
                {
                    // If any unit is within the selection add them to selection
                    UnitSelections.Instance.DragHover(unit);
                }
                else
                {
                    UnitSelections.Instance.DragUnhover(unit);
                }
            }
            
            yield return null;
        }
    }

    private void OnRelease(InputAction.CallbackContext context)
    {
        SelectUnits();
        startPosition = Vector2.zero;
        endPosition = Vector2.zero;
        DrawVisual();
        if (dragging != null)
            StopCoroutine(dragging);
    }

    void DrawVisual()
    {
        Vector2 boxStart = startPosition;
        Vector2 boxEnd = endPosition;

        //Debug.Log("DrawVisual startPosition" + boxStart);
        //Debug.Log("DrawVisual endPosition" + boxStart);

        Vector2 boxCenter = (boxStart + boxEnd) / 2;
        boxVisual.position = boxCenter;

        Vector2 boxSize = new Vector2(Mathf.Abs(boxStart.x - boxEnd.x), Mathf.Abs(boxStart.y - boxEnd.y)) / _canvas.scaleFactor;

        boxVisual.sizeDelta = boxSize;
    }

    void DrawSelection(Vector2 mousePos)
    {
        // Do X calculations
        if (mousePos.x < startPosition.x)
        {
            // Dragging left
            selectionBox.xMin = mousePos.x;
            selectionBox.xMax = startPosition.x;
        }
        else
        {
            // Dragging right
            selectionBox.xMin = startPosition.x;
            selectionBox.xMax = mousePos.x;
        }

        // Do Y calculations
        if (mousePos.y < startPosition.y)
        {
            // Dragging down
            selectionBox.yMin = mousePos.y;
            selectionBox.yMax = startPosition.y;
        }
        else
        {
            // Dragging up
            selectionBox.yMin = startPosition.y;
            selectionBox.yMax = mousePos.y;
        }
    }

    void SelectUnits()
    {
        if (UnitSelections.Instance == null)
            return;
        
        // Loop though all the units
        foreach (var unit in UnitSelections.Instance.unitList)
        {
            // If unit is within the bounds of the selection rect
            if (selectionBox.Contains(Camera.main.WorldToScreenPoint(unit.transform.position)))
            {
                // If any unit is within the selection add them to selection
                UnitSelections.Instance.DragSelect(unit);
            }
        }

        if (UnitSelections.Instance.unitsSelected.Count > 0)
            unitSelected?.Invoke();
    }
}

using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class UnitDrag : MonoBehaviour
{
    // Graphical
    [SerializeField]
    private RectTransform boxVisual;
    private string selectionUI;

    [Header("Controls")]
    private PlayerInput _playerInput;
    [StringInList(typeof(PropertyDrawersHelper), "AllPlayerInputs")] public string selectionControl;
    private InputAction _selection;

    // Logical
    private Rect selectionBox;

    private Canvas _canvas;

    private Vector2 startPosition;
    private Vector2 endPosition;

    private Coroutine dragging;

    private void Awake()
    {
        _playerInput = FindObjectOfType<PlayerInput>();
        _selection = _playerInput.actions[selectionControl];
    }

    // Start is called before the first frame update
    void Start()
    {
        _canvas = Root.Instance.GetComponent<Canvas>();

        GameObject UI = Root.Instance.UIGroups.Where(obj => obj.name == selectionUI).SingleOrDefault();
        if (UI != null)
            boxVisual = UI.transform.GetChild(0).GetComponent<RectTransform>();

        startPosition = Vector2.zero;
        endPosition = Vector2.zero;
        DrawVisual();
    }

    private void OnEnable()
    {
        _selection.performed += OnClick;
        _selection.canceled += OnRelease;
    }

    private void OnDisable()
    {
        _selection.performed -= OnClick;
        _selection.canceled -= OnRelease;
    }

    private void OnClick(InputAction.CallbackContext context)
    {
        startPosition = Mouse.current.position.ReadValue();
        selectionBox = new Rect();
        dragging = StartCoroutine(OnDrag());
    }

    private IEnumerator OnDrag()
    {
        for(; ; )
        {
            endPosition = Mouse.current.position.ReadValue();

            DrawVisual();
            DrawSelection(endPosition);
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
        foreach ( var unit in UnitSelections.Instance.unitList)
        {
            // If unit is within the bounds of the selection rect
            if (selectionBox.Contains(Camera.main.WorldToScreenPoint(unit.transform.position)))
            {
                // If any unit is within the selection add them to selection
                UnitSelections.Instance.DragSelect(unit);
                PlayerUnitArmyNew.AddUnitToSelectedParent(unit);
            }
        }
    }
}

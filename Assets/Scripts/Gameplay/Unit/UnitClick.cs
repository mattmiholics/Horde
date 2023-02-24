using UnityEngine;
using UnityEngine.InputSystem;

public class UnitClick : MonoBehaviour
{
    public GameObject groundMarker;
    public Animator arrowAnimator;

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
    
    // Start is called before the first frame update
    private void Awake()
    {
        _playerInput = FindObjectOfType<PlayerInput>();

        _destination = _playerInput.actions[destinationControl];
        _select = _playerInput.actions[selectControl];
        _unionSelect = _playerInput.actions[unionSelectControl];

        if (mapStartEnabled)
            EnableMap();
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
        _select.performed += OnLeftClick;
    }

    private void OnDisable()
    {
        _destination.performed -= OnRightClick;
        _select.performed -= OnLeftClick;
    }

    private void OnLeftClick(InputAction.CallbackContext context)
    {
        if (UnitSelections.Instance == null)
            return;

        groundMarker.SetActive(false);
        RaycastHit hit = new RaycastHit();
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, troopLayer))
        {
            GameObject clickedUnit = hit.collider.GetComponentInParent<Agent>().gameObject;
            // If we hit a clickable object
            if (_unionSelect.WasPressedThisFrame())
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
            if (!_unionSelect.WasPressedThisFrame())
            {
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
            arrowAnimator.SetTrigger("Spinning");
        }
    }
}

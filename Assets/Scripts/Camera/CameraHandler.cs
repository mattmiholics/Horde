using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using Cinemachine;

public class CameraHandler : MonoBehaviour
{
    public static event Action SingletonInstanced;

    public Camera cineCamera;
    public CinemachineVirtualCamera cineVC;
    public Transform cameraParent;
    public Transform cameraYRotate;
    public Transform cameraXRotate;
    public Transform cameraZoom;
    [Space]
    public bool lockControls = false;
    public bool lockZoomCollision = false;
    [Space]
    public bool lockMouseWhileRotating = true;
    public bool lockLookY = true;
    [Tooltip("Only necessary if lock look X is false.")]
    public Vector2 minMaxLookY;
    [Space]
    public bool screenEdgeMoving = true;
    public float screenEdgeRatio = 0.05f;
    [Space]
    public bool lockToWorldBorder = true;
    [Space]
    [Range(1, 5)]
    public float movementSensetivity = 2.5f;
    [Range(1, 50)]
    public float movementDrag = 16f;
    [Range(1, 5)]
    public float rotationSensetivity = 2.5f;
    [Range(1, 50)]
    public float rotationDrag = 10f;
    [Range(1, 5)]
    public float zoomSensetivity = 2.5f;
    [Range(1, 50)]
    public float zoomDrag = 10f;
    public Vector2 zoomMinMax = new Vector2(2, 20);
    [Space]
    public LayerMask clippingCollision;

    [PropertySpace(10, 10)]
    [Required] // Majority of the script needs the player input component
    [ValidateInput("ValidatePlayerInput")] // Player input needs to be set up correctly, see method below
    public PlayerInput playerInput;

    private bool ValidatePlayerInput(PlayerInput playerInput, ref string message, ref InfoMessageType? messageType)
    {
        if (playerInput == null) // Player Input doesn't exist
        {
            message = "";
            messageType = InfoMessageType.None;
            return false;
        }
        else if (playerInput.actions == null) // If input actions asset doesn't exist
        {
            message = "Player Input is missing an Input Actions component!";
            messageType = InfoMessageType.Error;
            return false;
        }
        else if (playerInput.actions.Count() <= 0) // There are no actions
        {
            message = "Input Actions component contains 0 actions.";
            messageType = InfoMessageType.Warning;
            return false;
        }

        return true;
    }

    [Space]
    [Title("Controls")]
    [SerializeField]
    [BoxGroup("Input", ShowLabel = false, VisibleIf = "@playerInput != null && playerInput.actions != null")]
    [LabelText("Input Action Maps")]
    [TableList(ShowIndexLabels = true)]
    [OnInspectorInit("InitializeActionMaps"), OnValueChanged("InitializeActionMaps")] // The ActionMapToggle type requires a player input to be assigned via a method
    private List<ActionMapToggle> actionMapToggles = new List<ActionMapToggle>() { new ActionMapToggle() };

    [HideInInspector]
    public InputActionMap[] actionMaps;

    [SerializeField]
    [BoxGroup("Input", ShowLabel = false, VisibleIf = "@playerInput != null && playerInput.actions != null")]
    [LabelText("Input Action Blacklist Maps")]
    [TableList(ShowIndexLabels = true)]
    [OnInspectorInit("InitializeActionMaps"), OnValueChanged("InitializeActionMaps")] // The ActionMapToggle type requires a player input to be assigned via a method
    private List<ActionMapToggle> actionMapBlacklistToggles = new List<ActionMapToggle>() { new ActionMapToggle() };

    [HideInInspector]
    public InputActionMap[] actionMapsBlacklist;

    private void InitializeActionMaps() { actionMapToggles.ForEach(am => am.playerInput = playerInput); actionMapBlacklistToggles.ForEach(am => am.playerInput = playerInput); }

    private bool ContainsActionMap()  // Used to check if action map is valid
    {
        if (playerInput != null && actionMapToggles.Count > 0)
        {
            bool aMapIsValid = false;
            actionMapToggles.ForEach(amt => { aMapIsValid = (amt.actionMapString != null && playerInput.actions.FindActionMap(amt.actionMapString) != null) ? true : aMapIsValid; });

            return aMapIsValid;
        }
        return false;
    }

    [SerializeField]
    [FoldoutGroup("Input/Actions", VisibleIf = "ContainsActionMap")]
    [ValueDropdown("GetInputActions", HideChildProperties = true, NumberOfItemsBeforeEnablingSearch = 0, ExpandAllMenuItems = false, DoubleClickToConfirm = true, OnlyChangeValueOnConfirm = true)]
    private string moveControl;
    private InputAction _move;

    [SerializeField]
    [FoldoutGroup("Input/Actions")]
    [ValueDropdown("GetInputActions", HideChildProperties = true, NumberOfItemsBeforeEnablingSearch = 0, ExpandAllMenuItems = false, DoubleClickToConfirm = true, OnlyChangeValueOnConfirm = true)]
    private string lookControl;
    private InputAction _look;

    [SerializeField]
    [FoldoutGroup("Input/Actions")]
    [ValueDropdown("GetInputActions", HideChildProperties = true, NumberOfItemsBeforeEnablingSearch = 0, ExpandAllMenuItems = false, DoubleClickToConfirm = true, OnlyChangeValueOnConfirm = true)]
    private string lookAltControl;
    private InputAction _lookAlt;

    [SerializeField]
    [FoldoutGroup("Input/Actions")]
    [ValueDropdown("GetInputActions", HideChildProperties = true, NumberOfItemsBeforeEnablingSearch = 0, ExpandAllMenuItems = false, DoubleClickToConfirm = true, OnlyChangeValueOnConfirm = true)]
    private string rotateControl;
    private InputAction _rotate;

    [SerializeField]
    [FoldoutGroup("Input/Actions")]
    [ValueDropdown("GetInputActions", HideChildProperties = true, NumberOfItemsBeforeEnablingSearch = 0, ExpandAllMenuItems = false, DoubleClickToConfirm = true, OnlyChangeValueOnConfirm = true)]
    private string rotateAltControl;
    private InputAction _rotateAlt;

    [SerializeField]
    [FoldoutGroup("Input/Actions")]
    [ValueDropdown("GetInputActions", HideChildProperties = true, NumberOfItemsBeforeEnablingSearch = 0, ExpandAllMenuItems = false, DoubleClickToConfirm = true, OnlyChangeValueOnConfirm = true)]
    private string zoomControl;
    private InputAction _zoom;

    [SerializeField]
    [FoldoutGroup("Input/Actions")]
    [ValueDropdown("GetInputActions", HideChildProperties = true, NumberOfItemsBeforeEnablingSearch = 0, ExpandAllMenuItems = false, DoubleClickToConfirm = true, OnlyChangeValueOnConfirm = true)]
    private string zoomAltControl;
    private InputAction _zoomAlt;

    [SerializeField]
    [FoldoutGroup("Input/Actions")]
    [ValueDropdown("GetInputActions", HideChildProperties = true, NumberOfItemsBeforeEnablingSearch = 0, ExpandAllMenuItems = false, DoubleClickToConfirm = true, OnlyChangeValueOnConfirm = true)]
    private string activateControl;
    private InputAction _activate;

    [SerializeField]
    [FoldoutGroup("Input/Actions")]
    [ValueDropdown("GetInputActions", HideChildProperties = true, NumberOfItemsBeforeEnablingSearch = 0, ExpandAllMenuItems = false, DoubleClickToConfirm = true, OnlyChangeValueOnConfirm = true)]
    private string deactivateControl;
    private InputAction _deactivate;

    [Serializable]
    private class ActionMapToggle
    {
        [HideInInspector]
        public PlayerInput playerInput; // Required to get availiable input maps

        [VerticalGroup("Action Map")]
        [HideLabel]
        [ValueDropdown("GetInputActionMaps", HideChildProperties = true, NumberOfItemsBeforeEnablingSearch = 0, CopyValues = false, OnlyChangeValueOnConfirm = true)]
        public string actionMapString;

        private IEnumerable GetInputActionMaps()
        {
            return playerInput.actions.actionMaps.Select(x => new ValueDropdownItem(x.name, x.name)); // Just pull the string name because otherwise if we grab a copy it won't work
        }
    }

    private IEnumerable GetInputActions()
    {
        return playerInput.actions.Where(x => actionMapToggles.Any(amt => x.actionMap.name == amt.actionMapString)) // Only get actions from selected action maps
                                  .Select(x => new ValueDropdownItem($"{x.actionMap.name}/{x.name}", x.name)); // Also only get the string name of action 
    }

    private Rigidbody _rigidbodyParent;
    private Rigidbody _rigidbodyYRotate;
    private Vector2 warpPosition;
    [HideInInspector]
    public float zoomPosZ;
    [HideInInspector]
    public float zoomTarget;
    private float zoomTime;
    [Space]
    [Header("Debug")]
    #pragma warning disable 0414
    [ReadOnly] public bool cameraAltActive;
    #pragma warning restore 0414
    [ReadOnly] [SerializeField] private Vector2 move;
    [ReadOnly] [SerializeField] private Vector2 look;
    [ReadOnly] [SerializeField] private float lookAlt;
    [ReadOnly] [SerializeField] private float zoom;

    [HideInInspector]
    public InputActionMap[] disabledActionMaps;

    float defaultDrag = 0.95f;

    bool IsMouseOverGameWindow { get { return !(0 > Mouse.current.position.ReadValue().x || 0 > Mouse.current.position.ReadValue().y || Screen.width < Mouse.current.position.ReadValue().x || Screen.height < Mouse.current.position.ReadValue().y); } }

    //might be better to separate the player input component from the camera handler object in the future
    private static CameraHandler _instance;
    public static CameraHandler Instance { get { return _instance; } }

    void Awake()
    {
        Physics.queriesHitBackfaces = true;

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

        actionMapsBlacklist = actionMapBlacklistToggles.Select(amt =>
                                                        {
                                                            InputActionMap am = playerInput.actions.FindActionMap(amt.actionMapString);
                                                            if (am == null) throw new Exception("Invalid action map selected");
                                                            return am;
                                                        })
                                                       .ToArray();

        actionMaps = actionMapToggles.Select(amt =>
                                      {
                                          InputActionMap am = playerInput.actions.FindActionMap(amt.actionMapString);
                                          if (am == null) throw new Exception("Invalid action map selected");
                                          return am;
                                      })
                                     .Where(am => !actionMapsBlacklist.Contains(am))
                                     .ToArray();

        //set variables
        if (!cameraParent.TryGetComponent<Rigidbody>(out _rigidbodyParent))
            throw new Exception("Camera parent missing parent rigidbody.");
        if (!cameraYRotate.TryGetComponent<Rigidbody>(out _rigidbodyYRotate))
            throw new Exception("Camera rotate missing y rotate rigidbody.");

        cameraAltActive = false;
        zoomPosZ = cameraZoom.localPosition.z;
        zoomTarget = zoomPosZ;

        //initialize inputs
        _move = playerInput.actions[moveControl];
        _look = playerInput.actions[lookControl];
        _lookAlt = playerInput.actions[lookAltControl];
        _rotate = playerInput.actions[rotateControl];
        _rotateAlt = playerInput.actions[rotateAltControl];
        _zoom = playerInput.actions[zoomControl];
        _zoomAlt = playerInput.actions[zoomAltControl];
        _activate = playerInput.actions[activateControl];
        _deactivate = playerInput.actions[deactivateControl];
        ControlChange(null);

        //other stuff
        _rigidbodyYRotate.maxAngularVelocity = 100;

        SingletonInstanced?.Invoke();
    }

    private void OnEnable()
    {
        _activate.started += EnableCameraControls;

        _deactivate.performed += DisableCameraControls;

        playerInput.onControlsChanged += ControlChange;
    }

    private void OnDisable()
    {
        _activate.started -= EnableCameraControls;

        _deactivate.performed -= DisableCameraControls;

        playerInput.onControlsChanged -= ControlChange;
    }

    private void ControlChange(PlayerInput input)
    {
        //Debug.Log("Controls Changed");

        foreach (var b in _activate.bindings)
        {
            int id = _activate.bindings.IndexOf(x => x == b);

            if (_deactivate.bindings.ElementAtOrDefault(id) != null)
            {
                //Debug.Log("added");
                _deactivate.AddBinding(b);
            }
            else
            {
                //Debug.Log("changed");
                _deactivate.ChangeBinding(id).To(b);
            }
        }
    }

    private void EnableCameraControls(InputAction.CallbackContext context)
    {
        disabledActionMaps = playerInput.actions.actionMaps.Where(am => (am.enabled && !actionMapsBlacklist.Contains(am))).ToArray(); //get all currently active maps
        //enable action map
        actionMaps.ForEach(am => am.Enable());
        //disable other action maps
        disabledActionMaps.ForEach(am => am.Disable());

        cameraAltActive = true;
    }

    private void DisableCameraControls(InputAction.CallbackContext context)
    {
        //disable action map
        actionMaps.ForEach(am => am.Disable());
        //enable previously disabled action maps
        disabledActionMaps.ForEach(am => am.Enable());

        cameraAltActive = false;
    }

    private void Update()
    {
        if (!cameraAltActive && _activate.IsPressed())
            EnableCameraControls(new InputAction.CallbackContext());

        if (!lockControls)
        {
            //screen edge moving happens first so that it is overwritten if move keys are used
            Vector2 screenPosition = Mouse.current.position.ReadValue();
            move = Vector2.zero;

            if (screenEdgeMoving && Application.isFocused && IsMouseOverGameWindow)
            {
                if (screenPosition.y / Screen.height < screenEdgeRatio) //bottom
                    move = new Vector2(move.x, -1);
                else if (screenPosition.y / Screen.height > 1 - screenEdgeRatio) //top
                    move = new Vector2(move.x, 1);
                if (screenPosition.x / Screen.width < screenEdgeRatio)  //left
                    move = new Vector2(-1, move.y);
                else if (screenPosition.x / Screen.width > 1 - screenEdgeRatio) //right
                    move = new Vector2(1, move.y);
            }

            if (_rotate.WasPressedThisFrame() || _rotateAlt.WasPressedThisFrame() || _zoomAlt.WasPressedThisFrame())
                warpPosition = screenPosition;

            Vector2 keyMove = _move.ReadValue<Vector2>();
            move = keyMove.magnitude > 0 || move.magnitude == 0 ? keyMove : move; //overwrite if move keys pressed
            look = _look.ReadValue<Vector2>();
            lookAlt = _lookAlt.ReadValue<float>();
            zoom = _zoom.ReadValue<Vector2>().normalized.y;

            //check if moving past world border
            if (lockToWorldBorder && World.Instance != null)
            {
                Vector2 borderMinMax = new Vector2(World.Instance.worldData.border - 0.5f, World.Instance.worldData.chunkSize * World.Instance.worldData.mapSizeInChunks - World.Instance.worldData.border - 0.5f);
                if (transform.position.x < borderMinMax.x)
                {
                    transform.position = new Vector3(borderMinMax.x, transform.position.y, transform.position.z);
                    _rigidbodyParent.velocity = new Vector3(Mathf.Abs(_rigidbodyParent.velocity.x) * 0.7f, _rigidbodyParent.velocity.y, _rigidbodyParent.velocity.z);
                }
                else if (transform.position.x > borderMinMax.y)
                {
                    transform.position = new Vector3(borderMinMax.y, transform.position.y, transform.position.z);
                    _rigidbodyParent.velocity = new Vector3(-Mathf.Abs(_rigidbodyParent.velocity.x) * 0.7f, _rigidbodyParent.velocity.y, _rigidbodyParent.velocity.z);
                }
                if (transform.position.z < borderMinMax.x)
                {
                    transform.position = new Vector3(transform.position.x, transform.position.y, borderMinMax.x);
                    _rigidbodyParent.velocity = new Vector3(_rigidbodyParent.velocity.x, _rigidbodyParent.velocity.y, Mathf.Abs(_rigidbodyParent.velocity.z) * 0.7f);
                }
                else if (transform.position.z > borderMinMax.y)
                {
                    transform.position = new Vector3(transform.position.x, transform.position.y, borderMinMax.y);
                    _rigidbodyParent.velocity = new Vector3(_rigidbodyParent.velocity.x, _rigidbodyParent.velocity.y, -Mathf.Abs(_rigidbodyParent.velocity.z) * 0.7f);
                }
            }

            //look X (optional)
            if (!lockLookY && (_rotateAlt.IsPressed() || _rotate.IsPressed()))
            {
                cameraXRotate.localEulerAngles += new Vector3(10f * look.y * rotationSensetivity * Time.unscaledDeltaTime, 0, 0);

                //check if camera is near x limits
                if (cameraXRotate.localEulerAngles.x > minMaxLookY.y && cameraXRotate.localEulerAngles.x < minMaxLookY.x)
                {
                    float target = Mathf.Abs(minMaxLookY.y - cameraXRotate.localEulerAngles.x) > Mathf.Abs(minMaxLookY.x - cameraXRotate.localEulerAngles.x) ? minMaxLookY.x : minMaxLookY.y;
                    cameraXRotate.localEulerAngles = new Vector3(target, 0, 0);
                }
            }

            //camera zoom alt
            if (_zoomAlt.IsPressed() && !_rotateAlt.IsPressed()) //if zoom is pressed and rotate isn't so that both aren't active at once
            {
                Cursor.visible = false;
                if (lockMouseWhileRotating)
                    Mouse.current.WarpCursorPosition(warpPosition);

                zoomTime = 0;
                zoomPosZ = cameraZoom.localPosition.z;
                Vector2 lookMinMax = new Vector2(look.x > look.y ? look.y : look.x, look.x > look.y ? look.x : look.y);
                zoomTarget = Mathf.Clamp(zoomTarget + Mathf.Clamp(look.x + look.y, lookMinMax.x, lookMinMax.y) * -zoomSensetivity * 2f * Time.unscaledDeltaTime, zoomMinMax.x, zoomMinMax.y);
            }
            //camera zoom
            else if (!_zoomAlt.IsPressed() && zoom != 0)
            {
                zoomTime = 0;
                zoomPosZ = cameraZoom.localPosition.z;
                float valueAdjustment = playerInput.currentControlScheme == "Controller" ? 10f : 120f;
                zoomTarget = Mathf.Clamp(zoomTarget + zoom * -zoomSensetivity * valueAdjustment * Time.unscaledDeltaTime, zoomMinMax.x, zoomMinMax.y);
            }
        }

        //check for zoom world clipping
        Vector3 furthestPoint = GetFurthestPoint(transform.position, cameraXRotate.forward, zoomMinMax.y) + cameraXRotate.forward; //the float value is the offset from the ground
        float dist = Vector3.Distance(furthestPoint, transform.position);

        Vector3 GetFurthestPoint(Vector3 start, Vector3 dir, float distance)
        {
            if (Physics.Raycast(start, dir, out RaycastHit hit, distance, clippingCollision) && !lockZoomCollision)
            {
                Vector3 point = hit.point + (dir.normalized * 0.01f);
                return GetFurthestPoint(point, dir, distance - Vector3.Distance(start, point));
            }
            else
                return start;
        }

        if (dist > Mathf.Abs(zoomTarget))
        {
            zoomTarget = dist;
        }

        if (zoomTime < (5 / zoomDrag))
        {
            zoomTime += Time.unscaledDeltaTime;
        }
        else
        {
            zoomTime = (5 / zoomDrag);
        }

        cameraZoom.localPosition = new Vector3(0, 0, Mathf.Lerp(zoomPosZ, zoomTarget, Mathf.SmoothStep(0f, 1f, Mathf.Pow(zoomTime / (5 / zoomDrag), 0.4f))));
    }

    private void FixedUpdate()
    {
        //camera movement
        Vector2 p_move = -move.normalized; //inverted because of the direction of the zoom parent
        Vector3 rotateOffset = cameraYRotate.TransformDirection(new Vector3(p_move.x, 0, p_move.y));
        p_move = new Vector2(rotateOffset.x, rotateOffset.z).normalized;

        if (!lockControls && p_move.magnitude > 0)
        {
            _rigidbodyParent.AddForce(movementSensetivity * 6f * new Vector3(p_move.x, 0, p_move.y), ForceMode.Impulse);
            _rigidbodyParent.velocity *= defaultDrag;
        }
        else
        {
            _rigidbodyParent.velocity *= (1f - (movementDrag / 100));
        }

        //camera rotation
        if (!lockControls && (_rotateAlt.IsPressed() || _rotate.IsPressed()))
        {
            Cursor.visible = false;
            if (lockMouseWhileRotating)
                Mouse.current.WarpCursorPosition(warpPosition);

            if (look.magnitude > 0)
            {
                _rigidbodyYRotate.angularVelocity = new Vector3(0, 0.21f * look.x * (_rotateAlt.IsPressed() ? 0.21f : 1) * rotationSensetivity, 0); //this is main horizontal rotate
            }
            else
            {
                _rigidbodyYRotate.angularVelocity *= (1f - (rotationDrag / 100));
            }
        }
        else if (!lockControls && lookAlt != 0)
        {
            _rigidbodyYRotate.AddRelativeTorque(new Vector3(0, 0.06f * lookAlt * rotationSensetivity, 0), ForceMode.Impulse);
            _rigidbodyYRotate.angularVelocity *= defaultDrag;
        }
        //rotation drag
        else
        {
            if (!_zoomAlt.IsPressed())
                Cursor.visible = true;
           
            _rigidbodyYRotate.angularVelocity *= (1f - (rotationDrag / 100));
        }
    }

    public void UpdateMovementSense(System.Single newSense)
    {
        movementSensetivity = newSense;
    }

    public void UpdateRotationSense(System.Single newSense)
    {
        rotationSensetivity = newSense;
    }

    public void UpdateZoomSense(System.Single newSense)
    {
        zoomSensetivity = newSense;
    }

    public void UpdateBorderMovement(bool canMove)
    {
        screenEdgeMoving = canMove;
    }

    public void UpdateLockMouseRotate(bool isLocked)
    {
        lockMouseWhileRotating = isLocked;
    }
}

using Sirenix.OdinInspector;
using Sirenix.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class SceneInitialize : MonoBehaviour
{
    [Space]
    public Vector3 cameraStartPosition;
    [Tooltip("this should be 0")]
    public Vector3 cameraStartEulerRotation;
    public float cameraZoom = 18f;
    [Min(0)]
    public float cameraFOV = 60;
    public bool cameraLockControls;
    public bool cameraLockZoomCollision;

    [Space]
    public List<string> activeUI;
    [Space]
    [Title("Initial Input Action Maps")]
    [SerializeField]
    InputActionAsset inputActionAsset;
    [SerializeField]
    [ValidateInput("@inputActionAsset != null")]
    [LabelText("Input Action Maps")]
    [HideLabel]
    [ValueDropdown("GetInputActionMaps", HideChildProperties = true, NumberOfItemsBeforeEnablingSearch = 0, CopyValues = false, OnlyChangeValueOnConfirm = true)]
    public List<string> actionMapString;

    private IEnumerable GetInputActionMaps()
    {
        if (inputActionAsset)
            return inputActionAsset.actionMaps.Select(x => new ValueDropdownItem(x.name, x.name)); // Just pull the string name because otherwise if we grab a copy it won't work
        return null;
    }

    [HideInInspector]
    public InputActionMap[] actionMaps;

    private static SceneInitialize _instance;
    public static SceneInitialize Instance { get { return _instance; } }

    private void Awake()
    {
        // If an instance of this already exists and it isn't this one
        if (_instance != null && _instance != this)
        {
            // We redo the instance
            Destroy(_instance.gameObject);
            _instance = this;
        }
        else
        {
            // Make this the instance
            _instance = this;
        }

        actionMaps = actionMapString.Select(amt =>
                                      {
                                          InputActionMap am = CameraHandler.Instance.playerInput.actions.FindActionMap(amt);
                                          if (am == null) throw new Exception("Invalid action map selected");
                                          return am;
                                      })
                                     .ToArray();
    }

    private void Start()
    {
        //camera
        CameraHandler cameraHandler = CameraHandler.Instance;

        cameraHandler.lockZoomCollision = cameraLockZoomCollision;
        cameraHandler.transform.position = cameraStartPosition;
        cameraHandler.cameraYRotate.localEulerAngles = new Vector3(0, cameraStartEulerRotation.y, 0);
        cameraHandler.cameraXRotate.localEulerAngles = new Vector3(cameraStartEulerRotation.x, 0, 0);
        cameraHandler.cameraZoom.transform.localEulerAngles = new Vector3(0, 180, cameraStartEulerRotation.z);
        cameraHandler.cameraZoom.transform.localPosition = new Vector3(0, 0, cameraZoom);
        cameraHandler.zoomPosZ = cameraZoom;
        cameraHandler.zoomTarget = cameraZoom;
        cameraHandler.cineVC.m_Lens.FieldOfView = cameraFOV;
        cameraHandler.lockControls = cameraLockControls;

        //input
        ResetMaps();

        //ui
        foreach (GameObject UI in Root.Instance.UIGroups) //disable all
            UI.SetActive(false);

        foreach (string UIName in activeUI) //enable only wanted
        {
            GameObject UI = Root.Instance.UIGroups.Where(obj => obj.name == UIName).SingleOrDefault();
            if (UI != null)
                UI.SetActive(true);
            else
                Debug.Log("invalid UI name");
        }
    }

    public void ResetMaps()
    {
        CameraHandler.Instance.cameraAltActive = false;

        foreach (InputActionMap map in CameraHandler.Instance.playerInput.actions.actionMaps)
        {
            if (actionMaps.Contains(map))
            {
                map.Enable();
                Debug.Log(map.name);
            }
            else
                map.Disable();
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SceneInitialize : MonoBehaviour
{
    [Space]
    public Vector3 cameraStartPosition;
    [Tooltip("this should be 0")]
    public Vector3 cameraStartEulerRotation;
    public float cameraZoom = -18f;
    public bool cameraLockControls;

    [Space]
    public List<string> activeUI;

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

        if (CameraHandler.Instance != null)
            DelayedStart();
    }

    private void OnEnable()
    {
        if (CameraHandler.Instance == null)
            CameraHandler.SingletonInstanced += DelayedStart;
    }

    private void OnDisable()
    {
        CameraHandler.SingletonInstanced -= DelayedStart;
    }
    // Start is called before the first frame update
    void DelayedStart()
    {
        //camera
        CameraHandler cameraHandler = CameraHandler.Instance;
        cameraHandler.transform.position = cameraStartPosition;
        cameraHandler.cameraYRotate.localEulerAngles = new Vector3(0, cameraStartEulerRotation.y, 0);
        cameraHandler.cameraXRotate.localEulerAngles = new Vector3(cameraStartEulerRotation.x, 0, 0);
        cameraHandler.cameraZoom.transform.position = new Vector3(0, 0, cameraZoom);
        cameraHandler.lockControls = cameraLockControls;

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
}

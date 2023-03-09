using UnityEngine;
using UnityEngine.UI;

public class ControlOptions : MonoBehaviour
{
    [SerializeField] Slider moveSense;
    [SerializeField] Slider rotationSense;
    [SerializeField] Slider zoomSense;
    [SerializeField] Toggle borderMovment;
    [SerializeField] GameObject camera;

    const string MOVE_SENSE = "MoveSense";
    const string ROTATION_SENSE = "RotationSense";
    const string ZOOM_SENSE = "ZoomSense";
    const string BORDER_MOVEMENT = "BorderMovement";
    CameraHandler cameraHandler;


    void Start()
    {
        cameraHandler = camera.GetComponent<CameraHandler>();
        LoadSettings();
    }

    public void UpdateMoveSense(float newSense)
    {
        cameraHandler.UpdateMovementSense(newSense);
        PlayerPrefs.SetFloat(MOVE_SENSE, moveSense.value);
    }
    public void UpdateRotationSense(float newSense)
    {
        cameraHandler.UpdateRotationSense(newSense);
        PlayerPrefs.SetFloat(ROTATION_SENSE, rotationSense.value);
    }

    public void UpdateZoomSense(float newSense)
    {
        cameraHandler.UpdateZoomSense(newSense);
        PlayerPrefs.SetFloat(ZOOM_SENSE, zoomSense.value);
    }

    public void ToggleBorderMovement(bool doesBorderMove)
    {
        cameraHandler.UpdateBorderMovement(doesBorderMove);
        PlayerPrefs.SetInt(BORDER_MOVEMENT, borderMovment.isOn ? 1 : 0);
    }

    public void SaveSettings()
    {
        PlayerPrefs.SetFloat(MOVE_SENSE, moveSense.value);
        PlayerPrefs.SetFloat(ROTATION_SENSE, rotationSense.value);
        PlayerPrefs.SetFloat(ZOOM_SENSE, zoomSense.value);
        PlayerPrefs.SetInt(BORDER_MOVEMENT, borderMovment.isOn ? 1 : 0);
    }

    public void LoadSettings()
    {
        if (PlayerPrefs.HasKey(MOVE_SENSE))
            moveSense.value = PlayerPrefs.GetFloat(MOVE_SENSE);
        else
            moveSense.value = 2.5f;
        UpdateMoveSense(moveSense.value);

        if (PlayerPrefs.HasKey(ROTATION_SENSE))
            rotationSense.value = PlayerPrefs.GetFloat(ROTATION_SENSE);
        else
            rotationSense.value = 2.5f;
        UpdateRotationSense(rotationSense.value);

        if (PlayerPrefs.HasKey(ZOOM_SENSE))
            zoomSense.value = PlayerPrefs.GetFloat(ZOOM_SENSE);
        else
            zoomSense.value = 2.5f;
        UpdateZoomSense(zoomSense.value);

        if (PlayerPrefs.HasKey(BORDER_MOVEMENT))
            if (PlayerPrefs.GetInt(BORDER_MOVEMENT) == 1)
            {
                borderMovment.SetIsOnWithoutNotify(true);
            }
            else 
            { 
                borderMovment.SetIsOnWithoutNotify(false);
            }
        else
            borderMovment.SetIsOnWithoutNotify(false);
        ToggleBorderMovement(borderMovment.isOn);
    }
}

    

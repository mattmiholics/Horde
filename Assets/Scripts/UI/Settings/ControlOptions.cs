using UnityEngine;
using UnityEngine.UI;

public class ControlOptions : MonoBehaviour
{
    [SerializeField] Slider moveSense;
    [SerializeField] Slider rotationSense;
    [SerializeField] Slider zoomSense;
    [SerializeField] Toggle borderMovment;
    [SerializeField] Toggle lockMouse;

    const string MOVE_SENSE = "MoveSense";
    const string ROTATION_SENSE = "RotationSense";
    const string ZOOM_SENSE = "ZoomSense";
    const string BORDER_MOVEMENT = "BorderMovement";
    const string LOCK_MOUSE = "LockMouse";

    void Start()
    {
        LoadSettings();
    }

    public void SaveSettings()
    {
        PlayerPrefs.SetFloat(MOVE_SENSE, moveSense.value);
        PlayerPrefs.SetFloat(ROTATION_SENSE, rotationSense.value);
        PlayerPrefs.SetFloat(ZOOM_SENSE, zoomSense.value);
        PlayerPrefs.SetInt(BORDER_MOVEMENT, borderMovment.isOn ? 1 : 0);
        PlayerPrefs.SetInt(LOCK_MOUSE, lockMouse.isOn ? 1 : 0);

    }

    public void LoadSettings()
    {
        if (PlayerPrefs.HasKey(MOVE_SENSE))
            moveSense.value = PlayerPrefs.GetFloat(MOVE_SENSE);
        else
            moveSense.value = 2.5f;

        if (PlayerPrefs.HasKey(ROTATION_SENSE))
            rotationSense.value = PlayerPrefs.GetFloat(ROTATION_SENSE);
        else
            rotationSense.value = 2.5f;

        if (PlayerPrefs.HasKey(ZOOM_SENSE))
            zoomSense.value = PlayerPrefs.GetFloat(ZOOM_SENSE);
        else
            zoomSense.value = 2.5f;

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

        if (PlayerPrefs.HasKey(LOCK_MOUSE))
            if (PlayerPrefs.GetInt(LOCK_MOUSE) == 1)
            {
                lockMouse.SetIsOnWithoutNotify(true);
            }
            else
            {
                lockMouse.SetIsOnWithoutNotify(false);
            }
        else
            lockMouse.SetIsOnWithoutNotify(true);
    }
}

    

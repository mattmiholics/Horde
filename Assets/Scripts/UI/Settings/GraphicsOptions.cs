using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class GraphicsOptions : MonoBehaviour
{
    [SerializeField] Slider resolution;
    [SerializeField] Slider shadowDistance;
    [SerializeField] Toggle motionBlur;
    [SerializeField] Toggle tiltShift;
    [SerializeField] Toggle lensDistortion;
    [SerializeField] Toggle vignette;
    [SerializeField] Toggle ambientOcclusion;

    const string RESOLUTION = "Resolution";
    const string SHADOW_DISTANCE = "ShadowDistance";
    const string MOTION_BLUR = "MotionBlur";
    const string TILT_SHIFT = "TiltShift";
    const string LENS_DISTORTION = "LensDistortion";
    const string VIGNETTE = "Vignette";
    const string AMBIENT_OCCLUSION = "AmbientOcclusion";

    void Start()
    {
        LoadSettings();
    }

    public void SaveSettings()
    {
        PlayerPrefs.SetFloat(RESOLUTION, resolution.value);
        PlayerPrefs.SetFloat(SHADOW_DISTANCE, shadowDistance.value);

        if (motionBlur.isOn){ PlayerPrefs.SetFloat(MOTION_BLUR, 1f); }
        else { PlayerPrefs.SetFloat(MOTION_BLUR, 0f); }
        if (tiltShift.isOn) { PlayerPrefs.SetFloat(TILT_SHIFT, 1f); }
        else { PlayerPrefs.SetFloat(TILT_SHIFT, 0f); }
        if (lensDistortion.isOn) { PlayerPrefs.SetFloat(LENS_DISTORTION, 1f); }
        else { PlayerPrefs.SetFloat(LENS_DISTORTION, 0f); }
        if (vignette.isOn) { PlayerPrefs.SetFloat(VIGNETTE, 1f); }
        else { PlayerPrefs.SetFloat(VIGNETTE, 0f); }
        if (ambientOcclusion.isOn) { PlayerPrefs.SetFloat(AMBIENT_OCCLUSION, 1f); }
        else { PlayerPrefs.SetFloat(AMBIENT_OCCLUSION, 0f); }
    }

    public void LoadSettings()
    {
        if (PlayerPrefs.HasKey(RESOLUTION))
            resolution.value = PlayerPrefs.GetFloat(RESOLUTION);
        else
            resolution.value = 1f;
        if (PlayerPrefs.HasKey(SHADOW_DISTANCE))
            shadowDistance.value = PlayerPrefs.GetFloat(SHADOW_DISTANCE);
        else
            shadowDistance.value = 50f;

        if (PlayerPrefs.HasKey(TILT_SHIFT))
        { 
            if (PlayerPrefs.GetFloat(TILT_SHIFT) == 0f) 
                tiltShift.isOn = false; 
            else
                tiltShift.isOn = true; 
        }
        else
            tiltShift.isOn = true;
        
        if (PlayerPrefs.HasKey(MOTION_BLUR))
        {
            if (PlayerPrefs.GetFloat(MOTION_BLUR) == 0f)
                motionBlur.isOn = false;
            else
                motionBlur.isOn = true;
        }
        else
            motionBlur.isOn = true;
        
        if (PlayerPrefs.HasKey(LENS_DISTORTION))
        {
            if (PlayerPrefs.GetFloat(LENS_DISTORTION) == 0f)
                lensDistortion.isOn = false;
            else
                lensDistortion.isOn = true;
        }
        else
            lensDistortion.isOn = true;
        if (PlayerPrefs.HasKey(VIGNETTE))
        {
            if (PlayerPrefs.GetFloat(VIGNETTE) == 0f)
                vignette.isOn = false;
            else
                vignette.isOn = true;
        }
        else
            vignette.isOn = true;
        
        if (PlayerPrefs.HasKey(AMBIENT_OCCLUSION))
        {
            if (PlayerPrefs.GetFloat(AMBIENT_OCCLUSION) == 0f)
                ambientOcclusion.isOn = false;
            else
                ambientOcclusion.isOn = true;
        }
        else
            ambientOcclusion.isOn = true;
    }
}

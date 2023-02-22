using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;
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
    [SerializeField] Button msaa0;
    [SerializeField] Button msaa1;
    [SerializeField] Button msaa2;
    [SerializeField] Button msaa3;

    [SerializeField] ConstantPostProcessUpdate constantPostProcess;
    int msaa;

    const string RESOLUTION = "Resolution";
    const string SHADOW_DISTANCE = "ShadowDistance";
    const string MOTION_BLUR = "MotionBlur";
    const string TILT_SHIFT = "TiltShift";
    const string LENS_DISTORTION = "LensDistortion";
    const string VIGNETTE = "Vignette";
    const string AMBIENT_OCCLUSION = "AmbientOcclusion";
    const string MSAA = "msaa";

    void Start()
    {
        LoadSettings();
    }

    public void SetMSAA(int msaaIndex)
    {
        msaa = msaaIndex;
        msaa0.interactable = true;
        msaa1.interactable = true;
        msaa2.interactable = true;
        msaa3.interactable = true;
        switch (msaaIndex)
        {
            case 0:
                msaa0.interactable = false; break;
            case 1:
                msaa1.interactable = false; break;
            case 2:
                msaa2.interactable = false; break;
            case 3:
                msaa3.interactable = false; break;
        }
        PlayerPrefs.SetInt(MSAA, msaaIndex);
    }

    public void UpdateRenderScale(float scale)
    {
        UniversalRenderPipeline.asset.renderScale = scale;
        PlayerPrefs.SetFloat(RESOLUTION, scale);
    }

    public void UpdateShadowDistance(float distance)
    {
        UniversalRenderPipeline.asset.shadowDistance = distance;
        PlayerPrefs.SetFloat(SHADOW_DISTANCE, distance);
    }

    public void UpdateMotionBlur(bool doesBlur)
    {
        
        constantPostProcess.UpdateMotionBlur(doesBlur);
        PlayerPrefs.SetInt(MOTION_BLUR, Convert.ToInt32(motionBlur.isOn));
    }

    public void UpdateTiltShift(bool doesTiltShift)
    {
        constantPostProcess.UpdateTiltShift(doesTiltShift);
        PlayerPrefs.SetInt(TILT_SHIFT, Convert.ToInt32(tiltShift.isOn));
    }

    public void UpdateLensDistortion(bool doesLensDistortion)
    {
        constantPostProcess.UpdateLensDistortion(doesLensDistortion);
        PlayerPrefs.SetInt(LENS_DISTORTION, Convert.ToInt32(lensDistortion.isOn));
    }

    public void UpdateVignette(bool doesVignette)
    {
        constantPostProcess.UpdateVignette(doesVignette);
        PlayerPrefs.SetInt(VIGNETTE, Convert.ToInt32(vignette.isOn));
    }

    public void UpdateAmbientOcclusion(bool doesAmbientOcclusion)
    {
        PlayerPrefs.SetInt(AMBIENT_OCCLUSION, Convert.ToInt32(ambientOcclusion.isOn));
    }

    public void SaveSettings()
    {
        PlayerPrefs.SetFloat(RESOLUTION, resolution.value);
        PlayerPrefs.SetFloat(SHADOW_DISTANCE, shadowDistance.value);
        PlayerPrefs.SetInt(MSAA, msaa);

        PlayerPrefs.SetInt(MOTION_BLUR, Convert.ToInt32(motionBlur.isOn));
        PlayerPrefs.SetInt(TILT_SHIFT, Convert.ToInt32(tiltShift.isOn));
        PlayerPrefs.SetInt(LENS_DISTORTION, Convert.ToInt32(lensDistortion.isOn));
        PlayerPrefs.SetInt(VIGNETTE, Convert.ToInt32(vignette.isOn));
        PlayerPrefs.SetInt(AMBIENT_OCCLUSION, Convert.ToInt32(ambientOcclusion.isOn));
    }

    public void LoadSettings()
    {
        if (PlayerPrefs.HasKey(RESOLUTION))
            resolution.value = PlayerPrefs.GetFloat(RESOLUTION);
        else
            resolution.value = 1f;
        UpdateRenderScale(resolution.value);

        if (PlayerPrefs.HasKey(SHADOW_DISTANCE))
            shadowDistance.value = PlayerPrefs.GetFloat(SHADOW_DISTANCE);
        else
            shadowDistance.value = 50f;
        UpdateShadowDistance(shadowDistance.value);

        if (PlayerPrefs.HasKey(MSAA))
        {
            msaa = PlayerPrefs.GetInt(MSAA);
            SetMSAA(msaa);
        }
        else
        {
            msaa = 0;
            SetMSAA(msaa);
        }


        if (PlayerPrefs.HasKey(TILT_SHIFT))
        {
            tiltShift.isOn = Convert.ToBoolean(PlayerPrefs.GetInt(TILT_SHIFT));
        }
        else
        { tiltShift.isOn = true; }
        UpdateTiltShift(tiltShift.isOn);

        if (PlayerPrefs.HasKey(MOTION_BLUR))
        {
            motionBlur.isOn = Convert.ToBoolean(PlayerPrefs.GetInt(MOTION_BLUR));
        }
        else
        { motionBlur.isOn = true; }
        UpdateMotionBlur(motionBlur.isOn);

        if (PlayerPrefs.HasKey(LENS_DISTORTION))
        {
            lensDistortion.isOn = Convert.ToBoolean(PlayerPrefs.GetInt(LENS_DISTORTION));
        }
        else
        { lensDistortion.isOn = true; }
        UpdateLensDistortion(lensDistortion.isOn);

        if (PlayerPrefs.HasKey(VIGNETTE))
        {
            vignette.isOn = Convert.ToBoolean(PlayerPrefs.GetInt(VIGNETTE));
        }
        else
        { vignette.isOn = true; }
        UpdateVignette(vignette.isOn);

        if (PlayerPrefs.HasKey(AMBIENT_OCCLUSION))
        {
            ambientOcclusion.isOn = Convert.ToBoolean(PlayerPrefs.GetInt(AMBIENT_OCCLUSION));
        }
        else
        { ambientOcclusion.isOn = true; }
        UpdateAmbientOcclusion(ambientOcclusion.isOn);
    }
}

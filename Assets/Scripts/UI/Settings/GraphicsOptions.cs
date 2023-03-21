using SCPE;
using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class GraphicsOptions : MonoBehaviour
{
    [SerializeField] Slider resolution;
    [SerializeField] Slider shadowDistance;
    [SerializeField] Toggle motionBlurToggle;
    [SerializeField] Toggle tiltShiftToggle;
    [SerializeField] Toggle lensDistortionToggle;
    [SerializeField] Toggle vignetteToggle;
    [SerializeField] Toggle ambientOcclusion;
    [SerializeField] Button msaa0;
    [SerializeField] Button msaa1;
    [SerializeField] Button msaa2;
    [SerializeField] Button msaa3;

    [SerializeField] Volume constantPostProcessVolume;
    VolumeProfile profile;

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
        profile = constantPostProcessVolume.sharedProfile;
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
        if (!profile.TryGet<MotionBlur>(out var motionBlur))
        {
            motionBlur = profile.Add<MotionBlur>(true);
        }
        if (doesBlur == false)
        {
            motionBlur.intensity.value = 0f;
        }
        else
        {
            motionBlur.intensity.value = 1f;
        }
        PlayerPrefs.SetInt(MOTION_BLUR, Convert.ToInt32(motionBlurToggle.isOn));
    }

    public void UpdateTiltShift(bool doesTiltShift)
    {
        if (!profile.TryGet<TiltShift>(out var tilt))
        {
            tilt = profile.Add<TiltShift>(true);
        }
        if (doesTiltShift == false)
        {
            tilt.amount.value = 0f;
        }
        else
        {
            tilt.amount.value = 1f;
        }
        PlayerPrefs.SetInt(TILT_SHIFT, Convert.ToInt32(doesTiltShift));
    }

    public void UpdateLensDistortion(bool doesLensDistortion)
    {
        if (!profile.TryGet<LensDistortion>(out var lensDistortion))
        {
            lensDistortion = profile.Add<LensDistortion>(true);
        }
        if (doesLensDistortion == false)
        {
            lensDistortion.intensity.value = 0f;
        }
        else
        {
            lensDistortion.intensity.value = -0.25f;
        }
        PlayerPrefs.SetInt(LENS_DISTORTION, Convert.ToInt32(lensDistortionToggle.isOn));
    }

    public void UpdateVignette(bool doesVignette)
    {
        if (!profile.TryGet<Vignette>(out var vignette))
        {
            vignette = profile.Add<Vignette>(true);
        }
        if (doesVignette == false)
        {
            vignette.intensity.value = 0f;
        }
        else
        {
            vignette.intensity.value = 0.384f;
        }
        PlayerPrefs.SetInt(VIGNETTE, Convert.ToInt32(vignetteToggle.isOn));
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

        PlayerPrefs.SetInt(MOTION_BLUR, Convert.ToInt32(motionBlurToggle.isOn));
        PlayerPrefs.SetInt(TILT_SHIFT, Convert.ToInt32(tiltShiftToggle.isOn));
        PlayerPrefs.SetInt(LENS_DISTORTION, Convert.ToInt32(lensDistortionToggle.isOn));
        PlayerPrefs.SetInt(VIGNETTE, Convert.ToInt32(vignetteToggle.isOn));
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
            shadowDistance.value = 75f;
        UpdateShadowDistance(shadowDistance.value);

        if (PlayerPrefs.HasKey(MSAA))
        {
            msaa = PlayerPrefs.GetInt(MSAA);
            SetMSAA(msaa);
        }
        else
        {
            msaa = 1;
            SetMSAA(msaa);
        }

        if (PlayerPrefs.HasKey(TILT_SHIFT))
        {
            tiltShiftToggle.isOn = Convert.ToBoolean(PlayerPrefs.GetInt(TILT_SHIFT));
        }
        else
        {
            tiltShiftToggle.isOn = true;
        }
        UpdateTiltShift(tiltShiftToggle.isOn);

        if (PlayerPrefs.HasKey(MOTION_BLUR))
        {
            motionBlurToggle.isOn = Convert.ToBoolean(PlayerPrefs.GetInt(MOTION_BLUR));
        }
        else
        { motionBlurToggle.isOn = true; }
        UpdateMotionBlur(motionBlurToggle.isOn);

        if (PlayerPrefs.HasKey(LENS_DISTORTION))
        {
            lensDistortionToggle.isOn = Convert.ToBoolean(PlayerPrefs.GetInt(LENS_DISTORTION));
        }
        else
        { lensDistortionToggle.isOn = true; }
        UpdateLensDistortion(lensDistortionToggle.isOn);

        if (PlayerPrefs.HasKey(VIGNETTE))
        {
            vignetteToggle.isOn = Convert.ToBoolean(PlayerPrefs.GetInt(VIGNETTE));
        }
        else
        { vignetteToggle.isOn = true; }
        UpdateVignette(vignetteToggle.isOn);

        if (PlayerPrefs.HasKey(AMBIENT_OCCLUSION))
        {
            ambientOcclusion.isOn = Convert.ToBoolean(PlayerPrefs.GetInt(AMBIENT_OCCLUSION));
        }
        else
        { ambientOcclusion.isOn = true; }
        UpdateAmbientOcclusion(ambientOcclusion.isOn);
    }

    public void ResetSettings()
    {
        resolution.value = 1f;
        UpdateRenderScale(resolution.value);
        shadowDistance.value = 75f;
        UpdateShadowDistance(shadowDistance.value);
        msaa = 1;
        SetMSAA(msaa);
        tiltShiftToggle.isOn = true;
        UpdateTiltShift(tiltShiftToggle.isOn);
        motionBlurToggle.isOn = true; 
        UpdateMotionBlur(motionBlurToggle.isOn);
        lensDistortionToggle.isOn = true; 
        UpdateLensDistortion(lensDistortionToggle.isOn);
        vignetteToggle.isOn = true; 
        UpdateVignette(vignetteToggle.isOn);
        ambientOcclusion.isOn = true; 
        UpdateAmbientOcclusion(ambientOcclusion.isOn);
        SaveSettings();
    }
}

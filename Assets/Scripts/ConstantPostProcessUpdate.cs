using SCPE;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ConstantPostProcessUpdate : MonoBehaviour
{
    [SerializeField] Volume volume;
    VolumeProfile profile; 

    void Start()
    {
        profile = volume.sharedProfile;
        
    }

    void Update()
    {
        
    }

    public void UpdateMotionBlur(bool blurEnabled)
    {
        if (!profile.TryGet<MotionBlur>(out var motionBlur))
        {
            motionBlur = profile.Add<MotionBlur>(true);
        }
        if (blurEnabled == false)
        {
            motionBlur.intensity.value = 0f;
        }
        else
        {
            motionBlur.intensity.value = 1f;
        }
    }

    public void UpdateTiltShift(bool tiltShiftEnabled)
    {
        if (!profile.TryGet<TiltShift>(out var tilt))
        {
            tilt = profile.Add<TiltShift>(true);
        }
        if (tiltShiftEnabled == false)
        {
            tilt.amount.value = 0f;
        }
        else
        {
            tilt.amount.value = 1f;
        }
    }

    public void UpdateLensDistortion(bool lensDistortionEnabled)
    {
        if (!profile.TryGet<LensDistortion>(out var lensDistortion))
        {
            lensDistortion = profile.Add<LensDistortion>(true);
        }
        if (lensDistortionEnabled == false)
        {
            lensDistortion.intensity.value = 0f;
        }
        else
        {
            lensDistortion.intensity.value = -0.25f;
        }
    }

    public void UpdateVignette(bool vignetteEnabled)
    {
        if (!profile.TryGet<Vignette>(out var vignette))
        {
            vignette = profile.Add<Vignette>(true);
        }
        if (vignetteEnabled == false)
        {
            vignette.intensity.value = 0f;
        }
        else
        {
            vignette.intensity.value = 0.45f;
        }
    }
}

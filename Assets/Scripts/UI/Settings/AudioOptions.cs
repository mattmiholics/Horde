using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioOptions : MonoBehaviour
{
    [SerializeField] Slider masterVolume;
    [SerializeField] Slider musicVolume;
    [SerializeField] Slider sfxVolume;
    [SerializeField] AudioMixer audioMixer;

    private const string MASTER_VOL = "MasterVolume";
    private const string MUSIC_VOL = "MusicVolume";
    private const string SFX_VOL = "SFXVolume";
    void Start()
    {
        LoadSettings();
    }

    public void SetMasterVol(float newVol)
    {
        masterVolume.value = newVol;
        audioMixer.SetFloat(MASTER_VOL, Mathf.Log10(masterVolume.value / 100) * 20);
        PlayerPrefs.SetFloat(MASTER_VOL, masterVolume.value);
    }
    public void SetMusicVol(float newVol)
    {
        musicVolume.value = newVol;
        audioMixer.SetFloat(MUSIC_VOL, Mathf.Log10(musicVolume.value/100) * 20);
        PlayerPrefs.SetFloat(MUSIC_VOL, musicVolume.value);
    }
    public void SetSfxVol(float newVol)
    {
        sfxVolume.value = newVol;
        audioMixer.SetFloat(SFX_VOL, Mathf.Log10(sfxVolume.value/100) * 20);
        PlayerPrefs.SetFloat(SFX_VOL, sfxVolume.value);
    }

    public void SaveSettings()
    {
        PlayerPrefs.SetFloat(MASTER_VOL, masterVolume.value);
        PlayerPrefs.SetFloat(MUSIC_VOL, musicVolume.value);
        PlayerPrefs.SetFloat(SFX_VOL, sfxVolume.value);
    }

    public void LoadSettings()
    {
        if (PlayerPrefs.HasKey(MASTER_VOL))
            masterVolume.value = PlayerPrefs.GetFloat(MASTER_VOL);
        else
        { masterVolume.value = 50f; }
        audioMixer.SetFloat(MASTER_VOL, Mathf.Log10(masterVolume.value / 100) * 20);

        if (PlayerPrefs.HasKey(MUSIC_VOL))
            musicVolume.value = PlayerPrefs.GetFloat(MUSIC_VOL);
        else 
        { musicVolume.value = 50f; }
        audioMixer.SetFloat(MUSIC_VOL, Mathf.Log10(musicVolume.value / 100) * 20);

        if (PlayerPrefs.HasKey(SFX_VOL))
            sfxVolume.value = PlayerPrefs.GetFloat(SFX_VOL);
        else 
        { sfxVolume.value = 50f; }
        audioMixer.SetFloat(SFX_VOL, Mathf.Log10(sfxVolume.value / 100) * 20);
    }

    public void ResetSettings()
    { 
        masterVolume.value = 50f; 
        audioMixer.SetFloat(MASTER_VOL, Mathf.Log10(masterVolume.value / 100) * 20);
        musicVolume.value = 50f; 
        audioMixer.SetFloat(MUSIC_VOL, Mathf.Log10(musicVolume.value / 100) * 20);
        sfxVolume.value = 50f; 
        audioMixer.SetFloat(SFX_VOL, Mathf.Log10(sfxVolume.value / 100) * 20);
        SaveSettings();
    }
}

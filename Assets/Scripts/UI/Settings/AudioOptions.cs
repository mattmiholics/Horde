using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioOptions : MonoBehaviour
{
    [SerializeField] Slider masterVolume;
    [SerializeField] Slider musicVolume;
    [SerializeField] Slider sfxVolume;
    [SerializeField] AudioMixer audioMixer;
    void Start()
    {
        LoadSettings();
    }

    public void SetMasterVol(float newVol)
    {
        masterVolume.value = newVol;
        audioMixer.SetFloat("MasterVolume", Mathf.Log10(masterVolume.value/100)*20);
    }
    public void SetMusicVol(float newVol)
    {
        musicVolume.value = newVol;
        audioMixer.SetFloat("MusicVolume", Mathf.Log10(musicVolume.value/100) * 20);
    }
    public void SetSfxVol(float newVol)
    {
        sfxVolume.value = newVol;
        audioMixer.SetFloat("SFXVolume", Mathf.Log10(sfxVolume.value/100) * 20);
    }

    public void SaveSettings()
    {
        PlayerPrefs.SetFloat("MasterVolume", masterVolume.value);
        PlayerPrefs.SetFloat("MusicVolume", musicVolume.value);
        PlayerPrefs.SetFloat("SfxVolume", sfxVolume.value);
    }

    public void LoadSettings()
    {
        if (PlayerPrefs.HasKey("MasterVolume"))
            masterVolume.value = PlayerPrefs.GetFloat("MasterVolume");
        else 
            masterVolume.value = 50f;
        audioMixer.SetFloat("MasterVolume", Mathf.Log10(masterVolume.value/100) * 20);

        if (PlayerPrefs.HasKey("MasterVolume"))
            musicVolume.value = PlayerPrefs.GetFloat("MusicVolume");
        else musicVolume.value = 50f;
        audioMixer.SetFloat("MasterVolume", Mathf.Log10(musicVolume.value/100) * 20);

        if (PlayerPrefs.HasKey("MasterVolume"))
            sfxVolume.value = PlayerPrefs.GetFloat("SfxVolume");
        else sfxVolume.value = 50f;
        audioMixer.SetFloat("MasterVolume", Mathf.Log10(sfxVolume.value/100) * 20);
    }
}

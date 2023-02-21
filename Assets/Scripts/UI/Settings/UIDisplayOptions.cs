using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIDisplayOptions : MonoBehaviour
{
    [SerializeField] TMPro.TMP_Dropdown resolutionDropdown;
    [SerializeField] TMPro.TMP_Dropdown displayMonitor;
    [SerializeField] TMPro.TMP_Dropdown refreshRates;
    [SerializeField] Toggle fullscreen;
    [SerializeField] TMPro.TMP_Dropdown upscaling;
    [SerializeField] Slider frameCap;
    [SerializeField] Toggle vsync;

    Resolution[] resolutions;
    Display[] displays;

    const string RESOLUTION = "ResolutionPreference";
    const string FULLSCREEN = "FullscreenPreference";
    const string DISPLAY_MONITOR = "DisplayMonitor";
    const string UPSCALING = "Upscaling";
    const string REFRESH_RATE_PREF = "RefreshRate";
    const string VSYNC = "VsyncPref";
    const string FRAME_LIMIT = "FramerateCap";
    int[] REFRESH_RATES = { 60, 75, 144, 240 };

    // Start is called before the first frame update
    void Start()
    {
        findDisplays();
        int currentResolutionIndex = findResolutions();
        findRefreshRates(Screen.currentResolution.refreshRate);
        LoadSettings(currentResolutionIndex);
    }

    void findDisplays()
    {
        displayMonitor.ClearOptions();

        List<string> displayOptions = new List<string>();
        displays = Display.displays;
        int currentDisplayIndex = 0;
        for (int i = 0; i < displays.Length; i++)
        {
            string option = "Display #" + (i+1);
            displayOptions.Add(option);
            if (displays[i] == Display.main)
            {
                currentDisplayIndex = i;
            }
        }
        displayMonitor.AddOptions(displayOptions);
        displayMonitor.RefreshShownValue();
    }

    public int findResolutions()
    {
        resolutionDropdown.ClearOptions();
        List<string> resOptions = new List<string>();
        resolutions = Screen.resolutions;
        int currentResolutionIndex = 0;

        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height;
            resOptions.Add(option);
            if (resolutions[i].width == Screen.currentResolution.width && resolutions[i].height == Screen.currentResolution.height)
            {
                currentResolutionIndex = i;
            }
        }

        resolutionDropdown.AddOptions(resOptions);
        resolutionDropdown.RefreshShownValue();

        return currentResolutionIndex;
    }

    void findRefreshRates(int maxRefreshRate)
    {
        refreshRates.ClearOptions();
        List<string> refreshOptions = new List<string>();
        if (maxRefreshRate >= 240)
        {
            refreshOptions.Add("60hz");
            refreshOptions.Add("75hz");
            refreshOptions.Add("144hz");
            refreshOptions.Add("240hz");
        }
        else if (maxRefreshRate >= 144)
        {
            refreshOptions.Add("60hz");
            refreshOptions.Add("75hz");
            refreshOptions.Add("144hz");
        }
        else if (maxRefreshRate >= 75)
        { 
            refreshOptions.Add("60hz");
            refreshOptions.Add("75hz");
        }
        else
        {
            refreshOptions.Add("60hz");
        }

        refreshRates.AddOptions(refreshOptions);
        refreshRates.RefreshShownValue();
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }

    public void SetResolution(int resolutionIndex)
    {
        Resolution resolution = resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
        findRefreshRates(resolution.refreshRate);
    }

    public void SetRefreshRate(int refreshIndex)
    {
        Resolution resolution = Screen.currentResolution;
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen, REFRESH_RATES[refreshIndex]);
    }

    public void SetDisplay(int displayIndex)
    {
        Display.displays[displayIndex].Activate();
    }

    public void SetVsync(bool vsync)
    {
        if (vsync)
            QualitySettings.vSyncCount = 1;
        else
            QualitySettings.vSyncCount = 0;
    }

    public void SetFrameLimit(float frameLimit)
    {
        if (frameLimit > 500)
            Application.targetFrameRate = -1;
        else
            Application.targetFrameRate = (int)frameLimit;
    }
    
    public void SaveSettings()
    {
        PlayerPrefs.SetInt(RESOLUTION, resolutionDropdown.value);
        PlayerPrefs.SetInt(FULLSCREEN, Convert.ToInt32(fullscreen.isOn));
        PlayerPrefs.SetInt(DISPLAY_MONITOR, displayMonitor.value);
        PlayerPrefs.SetInt(REFRESH_RATE_PREF, refreshRates.value);
        PlayerPrefs.SetInt(VSYNC, Convert.ToInt32(vsync.isOn));
        PlayerPrefs.SetInt(FRAME_LIMIT, (int)frameCap.value);
    }
    

    public void LoadSettings(int currentResolutionIndex)
    {

        if (PlayerPrefs.HasKey(RESOLUTION))
            resolutionDropdown.value = PlayerPrefs.GetInt(RESOLUTION);
        else
            resolutionDropdown.value = currentResolutionIndex;

        if (PlayerPrefs.HasKey(FULLSCREEN))
        {
            Screen.fullScreen = Convert.ToBoolean(PlayerPrefs.GetInt(FULLSCREEN));
            fullscreen.isOn = Convert.ToBoolean(PlayerPrefs.GetInt(FULLSCREEN));
        }
        else
        {
            Screen.fullScreen = true;
            fullscreen.isOn = true;
        }

        if (PlayerPrefs.HasKey(REFRESH_RATE_PREF))
            refreshRates.value = PlayerPrefs.GetInt(REFRESH_RATE_PREF);
        else
            refreshRates.value = 0;

        if (PlayerPrefs.HasKey(VSYNC))
            vsync.isOn = Convert.ToBoolean(PlayerPrefs.GetInt(VSYNC));
        else
            vsync.isOn = false;

        if (PlayerPrefs.HasKey(FRAME_LIMIT))
            frameCap.value = PlayerPrefs.GetInt(FRAME_LIMIT);
        else
            frameCap.value = 501;
    }

    
}

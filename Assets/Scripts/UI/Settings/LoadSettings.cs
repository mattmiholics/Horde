using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadSettings : MonoBehaviour
{
    [SerializeField] GameObject AudioOptions;
    [SerializeField] GameObject GraphicsOptions;
    [SerializeField] GameObject DisplayOptions;
    [SerializeField] GameObject ControlOptions;
    void Awake()
    {
        AudioOptions.GetComponent<AudioOptions>().LoadSettings();
        GraphicsOptions.GetComponent<GraphicsOptions>().LoadSettings();
        DisplayOptions.GetComponent<UIDisplayOptions>().LoadSettings(DisplayOptions.GetComponent<UIDisplayOptions>().findResolutions());
        ControlOptions.GetComponent<ControlOptions>().LoadSettings();
    }
}

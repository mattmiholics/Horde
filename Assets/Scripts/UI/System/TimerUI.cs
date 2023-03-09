using System;
using UnityEngine;
using TMPro;

public class TimerUI : MonoBehaviour
{
    public float debug;
    private TextMeshProUGUI timer;
    private float time;
    // Start is called before the first frame update
    void Start()
    {
        timer = GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        timer.text = TimeSpan.FromSeconds(Time.timeSinceLevelLoad + debug).ToString($"{(MathF.Floor((Time.timeSinceLevelLoad + debug) / 3600) > 0 ? "h\\:mm\\:ss" : "mm\\:ss")}");
    }
}

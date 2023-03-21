using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

[RequireComponent(typeof(Button))]
public class SpeedButton : SerializedMonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textMesh;
    [ReadOnly, SerializeField] private Button button;

    [ReadOnly, SerializeField] private float currentTimeSpeed { get { return speedsIndex >= 0 && speedsIndex < speeds.Length ? speeds[speedsIndex] : 1; } }

    private readonly float[] speeds = {1f, 2f, 3f};

    private int speedsIndex = 0;

    private void OnEnable()
    {
        SceneLoader.SceneLoaded += SceneLoaded;
        SceneManager.sceneUnloaded += SceneUnloaded;
    }

    private void OnDisable()
    {
        SceneLoader.SceneLoaded -= SceneLoaded;
        SceneManager.sceneUnloaded -= SceneUnloaded;
    }

    private void SceneUnloaded(Scene scene)
    {
        WaveSpawner.Instance.WaveEnded -= ResetTimeSpeed;
    }

    private void SceneLoaded()
    {
        WaveSpawner.Instance.WaveEnded += ResetTimeSpeed;

        ResetTimeSpeed();
    }

    // Start is called before the first frame update
    void Start()
    {
        button = GetComponent<Button>();

        button.onClick.AddListener(() => ChangeTimeSpeed());
    }

    private void ResetTimeSpeed()
    {
        Debug.Log("reset");
        speedsIndex = 0;

        Time.timeScale = currentTimeSpeed;
        if (textMesh)
            textMesh.text = $"x{currentTimeSpeed.ToString("0")}";
    }

    private void ChangeTimeSpeed()
    {
        speedsIndex = speedsIndex >= speeds.Length - 1 ? 0 : ++speedsIndex;

        Time.timeScale = currentTimeSpeed;

        if (textMesh)
            textMesh.text = $"x{currentTimeSpeed.ToString("0")}";
    }
}

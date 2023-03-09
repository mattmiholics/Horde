using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NextButtons : MonoBehaviour
{
    public GameObject nextWaveButton;
    public GameObject nextLevelButton;

    private static NextButtons _instance;
    public static NextButtons Instance { get { return _instance; } }
    public void NextWaveButton()
    {
        WaveSpawner.Instance.SpawnNextWave();
    }

    public void NextLevelButton()
    {
        if (Application.CanStreamedLevelBeLoaded(LevelInfo.Instance.nextLevelString))
            SceneLoader.Instance.LoadWorldScene(LevelInfo.Instance.nextLevelString, true);

        nextWaveButton.SetActive(false);
        nextLevelButton.SetActive(false);
    }

    private void Awake()
    {
        // If an instance of this already exists and it isn't this one
        if (_instance != null && _instance != this)
        {
            // We destroy this instance
            Destroy(this.gameObject);
        }
        else
        {
            // Make this the instance
            _instance = this;
        }
    }
}

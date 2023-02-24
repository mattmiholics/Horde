using UnityEngine;
using TMPro;

public class WaveCountUI : MonoBehaviour
{
    private TextMeshProUGUI waveCount;
    private void Start()
    {
        waveCount = GetComponent<TextMeshProUGUI>();
    }
    // Update is called once per frame
    void Update()
    {
        if (WaveSpawner.Instance != null)
            waveCount.text = $"Wave: {WaveSpawner.Instance.waveNum + "/" + WaveSpawner.Instance.waveCount}";
    }
}
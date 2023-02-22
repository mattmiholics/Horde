using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpdateVolumeText : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI volumeText;
    [SerializeField] Slider vol;
    void Start()
    {
        UpdateText(vol.value);
    }
    public void UpdateText(float newVolume)
    {
        volumeText.text = (Mathf.RoundToInt(newVolume)).ToString();
    }    
}

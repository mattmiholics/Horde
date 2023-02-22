using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpdateVolumeText : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI volumeText;
    void Start()
    {
        UpdateText(GetComponentInParent<Slider>().value);
    }
    public void UpdateText(float newVolume)
    {
        volumeText.text = (Mathf.RoundToInt(newVolume)).ToString();
    }    
}

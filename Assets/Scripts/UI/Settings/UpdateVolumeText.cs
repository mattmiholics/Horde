using UnityEngine;
using UnityEngine.UI;

public class UpdateVolumeText : MonoBehaviour
{
    [SerializeField] Text volumeText;
    public void UpdateText(float newVolume)
    {
        volumeText.text = (Mathf.RoundToInt(newVolume)).ToString();
    }
}

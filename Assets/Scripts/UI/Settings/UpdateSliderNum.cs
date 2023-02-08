using UnityEngine;
using UnityEngine.UI;

public class UpdateSliderNum : MonoBehaviour
{
    [SerializeField] Text numberText;
    public void UpdateText(float newVolume)
    {
        numberText.text = (Mathf.Round(newVolume*10)).ToString();
    }
}

using UnityEngine;
using UnityEngine.UI;

public class UpdateSliderNum : MonoBehaviour
{
    [SerializeField] Text numberText;

    private void Awake()
    {
        UpdateText(GetComponentInParent<Slider>().value);
    }
    public void UpdateText(float newVolume)
    {
        numberText.text = (Mathf.Round(newVolume*10)).ToString();
    }
}

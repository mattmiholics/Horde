using UnityEngine;
using UnityEngine.UI;

public class UpdateSliderNum : MonoBehaviour
{
    [SerializeField] Text numberText;

    private void Awake()
    {
        UpdateText(GetComponentInParent<Slider>().value);
        UpdateFrameRate(GetComponentInParent<Slider>().value);
    }
    public void UpdateText(float newVolume)
    {
        numberText.text = (Mathf.Round(newVolume*10)).ToString();
    }

    public void UpdateTextAndDouble(float newValue)
    {
        numberText.text = (Mathf.Round(newValue * 20)).ToString();
    }

    public void UpdateResScale(float newValue)
    {
        numberText.text = (Mathf.Round(newValue * 50)).ToString();
    }
    
    public void UpdateShadowDistance(float newValue)
    {
        numberText.text = Mathf.Round(newValue).ToString();
    }

    public void UpdateFrameRate(float newFrameRate)
    {
        if (newFrameRate != -1)
            numberText.text = newFrameRate.ToString();
        else
            numberText.text = "No Limit";
    }
}

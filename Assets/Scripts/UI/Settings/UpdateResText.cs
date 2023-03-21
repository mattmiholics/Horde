using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpdateResText : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI numberText;
    private void Awake()
    {
        UpdateResScale(GetComponentInParent<Slider>().value);
    }
    public void UpdateResScale(float newValue)
    {
        numberText.text = (Mathf.Round(newValue * 100)).ToString();
    }
}

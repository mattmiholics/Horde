using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpdateTextAndDouble : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI numberText;
    [SerializeField] Slider vol;

    private void Awake()
    {
        UpdateTextAndDoubleIt(GetComponentInParent<Slider>().value);
    }

    public void UpdateTextAndDoubleIt(float newValue)
    {
        numberText.text = (Mathf.Round(newValue * 20)).ToString();
    }
}

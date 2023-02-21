using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpdateShadowText : MonoBehaviour
{
    [SerializeField] Text numberText;
    private void Awake()
    {
        UpdateShadowDistance(GetComponentInParent<Slider>().value);
    }
    public void UpdateShadowDistance(float newValue)
    {
        numberText.text = Mathf.Round(newValue).ToString();
    }
}

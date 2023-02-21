using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpdateTextAndDouble : MonoBehaviour
{
    [SerializeField] Text numberText;

    private void Awake()
    {
        UpdateTextAndDoubleIt(GetComponentInParent<Slider>().value);
    }

    public void UpdateTextAndDoubleIt(float newValue)
    {
        numberText.text = (Mathf.Round(newValue * 20)).ToString();
    }
}

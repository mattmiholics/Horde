using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class UpdateFrameText : MonoBehaviour
{
    [SerializeField] Text numberText;

    private void Awake()
    {
        UpdateFrameRate(GetComponentInParent<Slider>().value);
    }

    public void UpdateFrameRate(float newFrameRate)
    {
        if (newFrameRate != 501)
            numberText.text = newFrameRate.ToString();
        else
            numberText.text = "No Limit";
    }
}

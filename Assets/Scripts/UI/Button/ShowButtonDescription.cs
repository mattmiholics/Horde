using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowButtonDescription : MonoBehaviour
{
    [SerializeField] GameObject buttonDescriptionGroup;
    public void ShowData(bool isOn)
    {
        if (isOn) buttonDescriptionGroup.SetActive(true);
        else buttonDescriptionGroup.SetActive(false);
    }
}

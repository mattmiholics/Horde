using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DamageNumber : MonoBehaviour
{
    public TextMeshPro damageNumber;
    public void UpdateNumber(float number)
    {
        damageNumber.text = number.ToString("0");
    }
}

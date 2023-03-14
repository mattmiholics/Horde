using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(TextMeshPro))]
public class MarketBuildingNumber : MonoBehaviour
{
    public float animationTime;
    public AnimationCurve jumpCurve;

    private TextMeshPro textMeshPro;
    private Vector3 origionalPosition;

    private Coroutine numberAnimation;


    private void Start()
    {
        textMeshPro = GetComponent<TextMeshPro>();
        origionalPosition = transform.localPosition;
    }

    public void BeginAnimation(float number)
    {
        textMeshPro.text = number.ToString("0");

        if (numberAnimation != null)
            StopCoroutine(numberAnimation);
        numberAnimation = StartCoroutine(NumberAnimation());
    }

    private IEnumerator NumberAnimation()
    {
        float currentTime = 0;

        while(currentTime < animationTime)
        {
            textMeshPro.color = new Color(textMeshPro.color.r, textMeshPro.color.g, textMeshPro.color.b, currentTime / animationTime);
            transform.localPosition = origionalPosition + new Vector3(0, jumpCurve.Evaluate(currentTime / animationTime), 0);

            currentTime += Time.deltaTime;

            yield return null;
        }

        textMeshPro.color = new Color(textMeshPro.color.r, textMeshPro.color.g, textMeshPro.color.b, 0);
        transform.localPosition = origionalPosition + new Vector3(0, jumpCurve.Evaluate(1), 0);
    }
}

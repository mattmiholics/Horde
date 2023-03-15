using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DamageNumber : MonoBehaviour
{
    public TextMeshPro damageNumber;
    public float jumpSpeed = 1;
    public AnimationCurve jumpCurve;
    public float distance = 1;
    public AnimationCurve fadeCurve;
    public float fadeSpeed = 1;

    private Vector3 direction;
    private float currentTime;

    private void OnEnable()
    {
        direction = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
        direction = new Vector3(direction.x, 0, direction.z);

        currentTime = 0;
    }

    public void UpdateNumber(float number, Color color = default, float size = 1)
    {
        damageNumber.text = number.ToString("0");
        damageNumber.color = color;
        transform.localScale = new Vector3(size, size, size);
    }

    private void Update()
    {
        transform.position += direction * Time.deltaTime * distance;
        transform.position += Vector3.up * jumpCurve.Evaluate(currentTime * jumpSpeed);
        damageNumber.color = new Color(damageNumber.color.r, damageNumber.color.g, damageNumber.color.b, fadeCurve.Evaluate(fadeSpeed * currentTime));

        currentTime += Time.deltaTime;
    }
}

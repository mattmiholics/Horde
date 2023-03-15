using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


[RequireComponent(typeof(Renderer))]
public class EmissionJitter : MonoBehaviour
{
    [SerializeField] private float flickerFrequency = 1;
    [SerializeField] private float flickerLowestIntensity = 0.3f;
    [SerializeField] private float flickerOffsetMax = 1f;
    [SerializeField] private float flickerOffsetAnimationTime = 2f;
    [SerializeField] private AnimationCurve flickerOffsetCurve;
    private float randomOffset;
    private float flickerOffset;

    private Renderer _renderer;

    [ReadOnly]
    [ColorUsage(false, true)]
    private Color[] _originalColor;

    Coroutine offsetCoroutine;

    // Start is called before the first frame update
    void Start()
    {
        _renderer = GetComponent<Renderer>();
        _originalColor = _renderer.materials.Select(m => m.GetColor("_EmissionColor")).ToArray();

        randomOffset = Random.Range(0, 1000);
        flickerOffset = 0;

        WaveSpawner.Instance.EnemySpawned += OffsetEvent;

        StartCoroutine(LightFlicker());
    }

    private void OnDestroy()
    {
        WaveSpawner.Instance.EnemySpawned -= OffsetEvent;
    }

    private IEnumerator LightFlicker()
    {
        for (; ; )
        {
            float flicker = Mathf.PerlinNoise(Time.time * flickerFrequency, randomOffset) * ((1 - flickerLowestIntensity) / 1) + flickerLowestIntensity + flickerOffset;

            int count = _renderer.materials.Length;
            for(int i = 0; i < count; i++)
            {
                _renderer.materials[i].SetColor("_EmissionColor", _originalColor[i] * flicker);
            }

            yield return null;
        }
    }

    public void OffsetEvent()
    {
        if (offsetCoroutine != null)
            StopCoroutine(offsetCoroutine);
        offsetCoroutine = StartCoroutine(OffsetValue());
    }

    private IEnumerator OffsetValue()
    {
        flickerOffset = flickerOffsetCurve.Evaluate(0) * flickerOffsetMax;
        float currentTime = 0;

        while(currentTime < flickerOffsetAnimationTime)
        {
            flickerOffset = flickerOffsetCurve.Evaluate(currentTime / flickerOffsetAnimationTime) * flickerOffsetMax;
            Debug.Log(flickerOffset);
            currentTime += Time.deltaTime;

            yield return null;
        }

        flickerOffset = flickerOffsetCurve.Evaluate(1);
        yield return null;
    }
}

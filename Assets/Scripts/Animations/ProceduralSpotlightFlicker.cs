using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class ProceduralSpotlightFlicker : MonoBehaviour
{
    [SerializeField] private Light targetLight;

    [Header("Subtle Drift")]
    [SerializeField] private float intensityVariation = 0.36f;
    [SerializeField] private float outerAngleVariation = 8f;
    [SerializeField] private float innerAngleVariation = 4f;
    [SerializeField] private float driftSpeed = 1.1f;

    [Header("Failing Bulb Flickers")]
    [SerializeField] private Vector2 timeBetweenFlickers = new Vector2(2.5f, 6.5f);
    [SerializeField] private Vector2 flickerDuration = new Vector2(0.06f, 0.18f);
    [SerializeField] private Vector2 flickerIntensityMultiplier = new Vector2(0.12f, 0.55f);

    private float _baseIntensity;
    private float _baseOuterAngle;
    private float _baseInnerAngle;
    private float _noiseSeed;
    private float _flickerMultiplier = 1f;
    private Coroutine _flickerRoutine;

    private void Reset()
    {
        targetLight = GetComponent<Light>();
    }

    private void Awake()
    {
        if (targetLight == null)
        {
            targetLight = GetComponent<Light>();
        }

        if (targetLight == null)
        {
            enabled = false;
            return;
        }

        _baseIntensity = targetLight.intensity;
        _baseOuterAngle = targetLight.spotAngle;
        _baseInnerAngle = targetLight.innerSpotAngle;
        _noiseSeed = Random.Range(0f, 1000f);
    }

    private void OnEnable()
    {
        _flickerRoutine = StartCoroutine(FlickerRoutine());
    }

    private void OnDisable()
    {
        if (_flickerRoutine != null)
        {
            StopCoroutine(_flickerRoutine);
            _flickerRoutine = null;
        }

        _flickerMultiplier = 1f;
        RestoreBaseValues();
    }

    private void Update()
    {
        if (targetLight == null)
        {
            return;
        }

        float t = Time.time * driftSpeed;
        float intensityNoise = Mathf.PerlinNoise(_noiseSeed, t) - 0.5f;
        float outerAngleNoise = Mathf.PerlinNoise(_noiseSeed + 10f, t) - 0.5f;
        float innerAngleNoise = Mathf.PerlinNoise(_noiseSeed + 20f, t) - 0.5f;

        float outerAngle = _baseOuterAngle + outerAngleNoise * outerAngleVariation;
        float innerAngle = _baseInnerAngle + innerAngleNoise * innerAngleVariation;

        targetLight.intensity = Mathf.Max(0f, _baseIntensity + intensityNoise * intensityVariation) * _flickerMultiplier;
        targetLight.spotAngle = Mathf.Clamp(outerAngle, 1f, 179f);
        targetLight.innerSpotAngle = Mathf.Clamp(innerAngle, 1f, targetLight.spotAngle - 1f);
    }

    private IEnumerator FlickerRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(RandomRange(timeBetweenFlickers));

            _flickerMultiplier = RandomRange(flickerIntensityMultiplier);
            yield return new WaitForSeconds(RandomRange(flickerDuration));
            _flickerMultiplier = 1f;
        }
    }

    private void RestoreBaseValues()
    {
        if (targetLight == null)
        {
            return;
        }

        targetLight.intensity = _baseIntensity;
        targetLight.spotAngle = _baseOuterAngle;
        targetLight.innerSpotAngle = _baseInnerAngle;
    }

    private static float RandomRange(Vector2 range)
    {
        return Random.Range(Mathf.Min(range.x, range.y), Mathf.Max(range.x, range.y));
    }
}

using UnityEngine;

public class BarShake : MonoBehaviour
{
    [Header("Shake Settings")]
    public float shakeThreshold = 0.3f;
    public float maxIntensity = 8f;
    public float shakeSpeed = 25f;

    private Vector3 originalPosition;
    private float currentIntensity = 0f;

    void Start()
    {
        originalPosition = transform.localPosition;
    }
    public void UpdateShake(float fraction)
    {
        if (fraction < shakeThreshold)
        {
            float t = 1f - (fraction /  shakeThreshold);
            currentIntensity = Mathf.Lerp(0f, maxIntensity, t);

            float offsetX = Mathf.Sin(Time.time * shakeSpeed * currentIntensity);
            float offsetY = Mathf.Cos(Time.time * shakeSpeed * 0.7f) * currentIntensity * 0.5f;
            transform.localPosition = originalPosition + new Vector3(offsetX, offsetY, 0f);
        }
        else
        {
            transform.localPosition = originalPosition;
            currentIntensity = 0f;
        }
    }
}

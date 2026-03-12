using UnityEngine;

public class GazeIndicator : MonoBehaviour
{
    [Header("Hover Animation")]
    public float hoverHeight = 0.1f;
    public float hoverSpeed = 2f;
    public float rotationSpeed = 45f;

    [Header("Pulse")]
    public bool pulse = true;
    public float pulseMin = 0.85f;
    public float pulseMax = 1.15f;
    public float pulseSpeed = 2f;

    private Vector3 startLocalPosition;
    private Vector3 startLocalScale;

    void Start()
    {
        startLocalPosition = transform.localPosition;
        startLocalScale = transform.localScale;
    }

    void Update()
    {
        float newY = startLocalPosition.y + Mathf.Sin(Time.time * hoverSpeed) * hoverHeight;
        transform.localPosition = new Vector3(startLocalPosition.x, newY, startLocalPosition.z);

        if (rotationSpeed != 0f)
        {
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
        }

        if (pulse)
        {
            float scale = Mathf.Lerp(pulseMin, pulseMax, (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f);
            transform.localScale = startLocalScale * scale;
        }
    }
}
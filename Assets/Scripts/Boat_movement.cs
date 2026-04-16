using UnityEngine;

public class WaterMotion : MonoBehaviour
{
    [Header("Vertical (Up/Down) Motion")]
    public float verticalAmplitude = 0.2f;
    public float verticalFrequency = 0.5f;

    [Header("Side-to-Side Motion")]
    public float horizontalAmplitude = 0.3f;
    public float horizontalFrequency = 0.3f;

    [Header("Randomness")]
    public float randomnessStrength = 0.5f; // how chaotic the sideways motion feels
    public float randomnessSpeed = 0.2f;

    private Vector3 startPosition;
    private float randomOffset;

    void Start()
    {
        startPosition = transform.localPosition;

        // random phase so multiple boats don’t sync
        randomOffset = Random.Range(0f, 100f);
    }

    void Update()
    {
        float time = Time.time;

        // Smooth vertical bobbing (sine wave)
        float vertical = Mathf.Sin(time * verticalFrequency) * verticalAmplitude;

        // Base horizontal sway
        float horizontalBase = Mathf.Sin(time * horizontalFrequency + randomOffset);

        // Add Perlin noise for randomness (natural motion)
        float noise = Mathf.PerlinNoise(time * randomnessSpeed, randomOffset) - 0.5f;

        float horizontal = (horizontalBase + noise * randomnessStrength) * horizontalAmplitude;

        // Apply movement
        transform.localPosition = startPosition + new Vector3(horizontal, vertical, 0f);
    }
}
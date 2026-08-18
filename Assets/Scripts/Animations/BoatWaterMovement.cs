using UnityEngine;

public class BoatWaterMovement : MonoBehaviour
{
    [Header("Main Movement")]
    [SerializeField] private float amplitude = 5f;
    [SerializeField] private float frequency = 0.5f;

    [Header("Rotation Axis")]
    [SerializeField] private Vector3 rotationAxis = new Vector3(1f, 0f, 1f);

    [Header("Randomness")]
    [SerializeField] private float randomnessAmplitude = 1f;
    [SerializeField] private float randomnessFrequency = 0.3f;

    [Header("Calm Period")]
    [SerializeField] private float calmFrequencyMin = 10f;
    [SerializeField] private float calmFrequencyMax = 20f;
    [SerializeField] private float calmFadeSpeed = 0.2f;

    private Quaternion startRotation;

    private float calmTimer;
    private float calmAmount = 1f;
    private bool calming;

    private void Start()
    {
        startRotation = transform.localRotation;

        // Random time before first calm period
        calmTimer = Random.Range(calmFrequencyMin, calmFrequencyMax);
    }

    private void Update()
    {
        // Countdown to next calm period
        if (!calming)
        {
            calmTimer -= Time.deltaTime;

            if (calmTimer <= 0f)
            {
                calming = true;
            }
        }

        // Fade movement down during calm period
        if (calming)
        {
            calmAmount = Mathf.MoveTowards(
                calmAmount,
                0f,
                calmFadeSpeed * Time.deltaTime
            );

            // Once completely calm, begin fading back in
            if (calmAmount <= 0f)
            {
                calming = false;

                calmTimer = Random.Range(
                    calmFrequencyMin,
                    calmFrequencyMax
                );
            }
        }
        else
        {
            // Slowly return to normal movement
            calmAmount = Mathf.MoveTowards(
                calmAmount,
                1f,
                calmFadeSpeed * Time.deltaTime
            );
        }

        // Main movement
        float mainWave =
            Mathf.Sin(Time.time * frequency * Mathf.PI * 2f);

        // Random movement
        float randomWave =
            Mathf.Sin(Time.time * randomnessFrequency * Mathf.PI * 2f)
            * randomnessAmplitude;

        // Combine movement and apply calm amount to BOTH
        float rotationAmount =
            (mainWave * amplitude + randomWave) * calmAmount;

        Vector3 rotation =
            rotationAxis.normalized * rotationAmount;

        transform.localRotation =
            startRotation * Quaternion.Euler(rotation);
    }
}
using UnityEngine;

public class AcousticAudioTrigger : MonoBehaviour
{
    [Header("References")]
    public AudioSource audioSource;
    public AcousticWaveController waveController;

    [Header("Audio Detection")]
    [Range(0f, 1f)]
    public float triggerThreshold = 0.035f;

    [Range(0f, 1f)]
    public float resetThreshold = 0.015f;

    public float minTriggerInterval = 0.3f;

    public float smoothing = 0.04f;

    [Header("Wave Behaviour")]
    public bool triggerBurst = false;

    [Header("Debug")]
    [SerializeField] private float currentRMS;
    [SerializeField] private float smoothedRMS;
    [SerializeField] private int triggerCount;

    private const int SampleSize = 512;

    private readonly float[] samples = new float[SampleSize];

    private bool armed = true;
    private float lastTriggerTime = float.NegativeInfinity;


    private void Awake()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (audioSource == null || waveController == null)
        {
            Debug.LogError(
                "AcousticAudioTrigger: Missing references.",
                this
            );

            enabled = false;
        }
    }


    private void Update()
    {
        // Only analyze audio while playing
        if (!audioSource.isPlaying)
        {
            currentRMS = 0f;
            smoothedRMS = 0f;
            armed = true;
            return;
        }

        // Retrieve audio samples
        audioSource.GetOutputData(samples, 0);

        // Calculate RMS amplitude
        float sum = 0f;

        for (int i = 0; i < samples.Length; i++)
        {
            sum += samples[i] * samples[i];
        }

        currentRMS = Mathf.Sqrt(sum / samples.Length);

        // Smooth amplitude changes
        float smoothFactor =
            1f - Mathf.Exp(
                -Time.deltaTime / Mathf.Max(0.001f, smoothing)
            );

        smoothedRMS = Mathf.Lerp(
            smoothedRMS,
            currentRMS,
            smoothFactor
        );

        // Re-arm when audio becomes quiet
        if (smoothedRMS <= resetThreshold)
        {
            armed = true;
        }

        // Check whether we can trigger another wave
        bool cooldownComplete =
            Time.time - lastTriggerTime >= minTriggerInterval;

        if (armed &&
            cooldownComplete &&
            smoothedRMS >= triggerThreshold)
        {
            TriggerAudioWave();
        }
    }


    private void TriggerAudioWave()
    {
        armed = false;
        lastTriggerTime = Time.time;

        triggerCount++;

        if (triggerBurst)
        {
            waveController.TriggerWaves();
        }
        else
        {
            waveController.TriggerWave();
        }
    }
}
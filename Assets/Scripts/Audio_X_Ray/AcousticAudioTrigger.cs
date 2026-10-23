using UnityEngine;

public class AcousticAudioTrigger : MonoBehaviour
{
    [Header("References")]
    public AudioSource audioSource;
    public AcousticWaveController waveController;

    [Header("Trigger Mode")]
    [Tooltip("Enabled: react to audio peaks. Disabled: emit timed waves without audio for testing.")]
    public bool audioReactive = true;

    [Min(0.01f)]
    [Tooltip("Seconds between test triggers when Audio Reactive is disabled.")]
    public float testTriggerInterval = 1f;

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
    private bool previousAudioReactive;


    private void Awake()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (waveController == null)
        {
            Debug.LogError(
                "AcousticAudioTrigger: Assign a wave controller.",
                this
            );

            enabled = false;
        }
    }

    private void OnEnable()
    {
        previousAudioReactive = audioReactive;
        ResetDetection();
    }

    private void OnDisable()
    {
        if (waveController != null)
            waveController.CancelPendingWaves();
        ResetDetection();
    }

    private void ResetDetection()
    {
        currentRMS = 0f;
        smoothedRMS = 0f;
        armed = true;
        lastTriggerTime = float.NegativeInfinity;
    }

    private void Update()
    {
        if (waveController == null || !waveController.isActiveAndEnabled)
            return;

        if (previousAudioReactive != audioReactive)
        {
            // Do not carry delayed test/burst emissions into the new mode.
            waveController.CancelPendingWaves();
            ResetDetection();
            previousAudioReactive = audioReactive;
        }

        if (!audioReactive)
        {
            if (Time.time - lastTriggerTime >= Mathf.Max(0.01f, testTriggerInterval))
                TriggerAudioWave();
            return;
        }

        // Only analyze audio while playing
        if (audioSource == null || !audioSource.isActiveAndEnabled ||
            !audioSource.isPlaying || audioSource.mute || audioSource.volume <= 0f)
        {
            waveController.CancelPendingWaves();
            ResetDetection();
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

using System.Collections;
using UnityEngine;

public class AcousticWaveController : MonoBehaviour
{
    [Header("References")]
    public Material waveMaterial;
    public Transform waveOrigin;

    [Header("Wave Settings")]
    public float waveSpeed = 2f;
    public float maxRadius = 5f;
    public float waveWidth = 0.1f;

    [Header("Multiple Wave Settings")]
    public float waveInterval = 0.75f;

    [Header("Source Illumination")]
    public float sourceRadius = 3f;
    public float sourceFadeDuration = 0.4f;

    private float sourceIntensity;
    private float sourceTimeRemaining;

    private const int MaxWaves = 3;
    private const float InactiveRadius = -100f;

    private readonly float[] waveRadii = new float[MaxWaves];
    private readonly bool[] waveActive = new bool[MaxWaves];

    private int nextWaveIndex = 0;

    private Coroutine waveBurstCoroutine;

    // Shader property IDs
    private static readonly int[] WaveRadiusIDs =
    {
        Shader.PropertyToID("_WaveRadius"),
        Shader.PropertyToID("_WaveRadius2"),
        Shader.PropertyToID("_WaveRadius3")
    };

    private static readonly int WaveOriginID =
        Shader.PropertyToID("_WaveOrigin");

    private static readonly int WaveWidthID =
        Shader.PropertyToID("_WaveWidth");

    private static readonly int SourceRadiusID =
        Shader.PropertyToID("_SourceRadius");

    private static readonly int SourceIntensityID =
        Shader.PropertyToID("_SourceIntensity");


    private void Start()
    {
        if (waveMaterial == null || waveOrigin == null)
        {
            Debug.LogError(
                "AcousticWaveController: Missing references."
            );

            enabled = false;
            return;
        }

        // Initialize all waves as inactive
        for (int i = 0; i < MaxWaves; i++)
        {
            waveRadii[i] = InactiveRadius;
            waveActive[i] = false;

            waveMaterial.SetFloat(
                WaveRadiusIDs[i],
                InactiveRadius
            );
        }

        // Initialize source illumination
        sourceIntensity = 0f;
        sourceTimeRemaining = 0f;

        waveMaterial.SetFloat(
            SourceIntensityID,
            0f
        );

        waveMaterial.SetFloat(
            SourceRadiusID,
            sourceRadius
        );

        waveMaterial.SetVector(
            WaveOriginID,
            waveOrigin.position
        );
    }


    private void Update()
    {
        // Allow parameter adjustments during Play Mode
        waveMaterial.SetFloat(WaveWidthID, waveWidth);
        waveMaterial.SetFloat(SourceRadiusID, sourceRadius);

        // Keep wave origin synchronized with moving AudioSource
waveMaterial.SetVector(
    WaveOriginID,
    waveOrigin.position
);

        // -----------------------------
        // SOURCE ILLUMINATION
        // -----------------------------

        if (sourceTimeRemaining > 0f)
        {
            sourceTimeRemaining -= Time.deltaTime;

            sourceIntensity = Mathf.Clamp01(
                sourceTimeRemaining /
                Mathf.Max(0.01f, sourceFadeDuration)
            );
        }
        else
        {
            sourceIntensity = 0f;
        }

        waveMaterial.SetFloat(
            SourceIntensityID,
            sourceIntensity
        );


        // -----------------------------
        // WAVE PROPAGATION
        // -----------------------------

        for (int i = 0; i < MaxWaves; i++)
        {
            if (!waveActive[i])
                continue;

            waveRadii[i] += waveSpeed * Time.deltaTime;

            if (waveRadii[i] >= maxRadius)
            {
                waveActive[i] = false;
                waveRadii[i] = InactiveRadius;
            }

            waveMaterial.SetFloat(
                WaveRadiusIDs[i],
                waveRadii[i]
            );
        }
    }


    [ContextMenu("Trigger Wave")]
    public void TriggerWave()
    {
        if (!Application.isPlaying || !enabled)
            return;

        if (waveMaterial == null || waveOrigin == null)
            return;

        // Capture current audio source position
        waveMaterial.SetVector(
            WaveOriginID,
            waveOrigin.position
        );

        // -----------------------------
        // TRIGGER SOURCE ILLUMINATION
        // -----------------------------

        sourceIntensity = 1f;
        sourceTimeRemaining = Mathf.Max(
            0.01f,
            sourceFadeDuration
        );

        waveMaterial.SetFloat(
            SourceIntensityID,
            sourceIntensity
        );

        // The shader has only three wave slots. Reusing a travelling wave
        // resets its radius, so frequent sounds can prevent it from ever
        // reaching distant geometry. Keep active waves and skip excess pulses.
        int index = -1;
        for (int offset = 0; offset < MaxWaves; offset++)
        {
            int candidate = (nextWaveIndex + offset) % MaxWaves;
            if (!waveActive[candidate])
            {
                index = candidate;
                break;
            }
        }

        if (index < 0)
            return;

        waveRadii[index] = 0f;
        waveActive[index] = true;
        waveMaterial.SetFloat(WaveRadiusIDs[index], 0f);

        // Advance wave slot
        nextWaveIndex = (index + 1) % MaxWaves;
    }


    [ContextMenu("Trigger Three Waves")]
    public void TriggerWaves()
    {
        if (!Application.isPlaying || !enabled)
            return;

        if (waveBurstCoroutine != null)
            return;

        waveBurstCoroutine =
            StartCoroutine(TriggerWavesCoroutine());
    }


    private IEnumerator TriggerWavesCoroutine()
    {
        for (int i = 0; i < MaxWaves; i++)
        {
            TriggerWave();

            if (i < MaxWaves - 1)
                yield return new WaitForSeconds(
                    Mathf.Max(0f, waveInterval)
                );
        }

        waveBurstCoroutine = null;
    }
}

using UnityEngine;
using System.Collections;

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

    private Coroutine waveBurstCoroutine;

    private const int MaxWaves = 3;
    private const float InactiveRadius = -100f;

    private readonly float[] waveRadii = new float[MaxWaves];
    private readonly bool[] waveActive = new bool[MaxWaves];

    private int nextWaveIndex = 0;

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


    private void Start()
    {
        if (waveMaterial == null || waveOrigin == null)
        {
            Debug.LogError("AcousticWaveController: Missing references.");
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

        waveMaterial.SetVector(
            WaveOriginID,
            waveOrigin.position
        );
    }


    private void Update()
    {
        // Allow WaveWidth adjustment during Play Mode
        waveMaterial.SetFloat(WaveWidthID, waveWidth);

        for (int i = 0; i < MaxWaves; i++)
        {
            if (!waveActive[i])
                continue;

            // Expand this wave independently
            waveRadii[i] += waveSpeed * Time.deltaTime;

            // Deactivate when maximum radius is reached
            if (waveRadii[i] >= maxRadius)
            {
                waveActive[i] = false;
                waveRadii[i] = InactiveRadius;
            }

            // Update the corresponding shader property
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

        // Select the next available wave slot
        int index = nextWaveIndex;

        // Reset and activate this wave
        waveRadii[index] = 0f;
        waveActive[index] = true;

        // Update shared wave origin
        waveMaterial.SetVector(
            WaveOriginID,
            waveOrigin.position
        );

        waveMaterial.SetFloat(
            WaveRadiusIDs[index],
            0f
        );

        // Advance to the next slot
        nextWaveIndex = (nextWaveIndex + 1) % MaxWaves;
    }
    [ContextMenu("Trigger Three Waves")]
    public void TriggerWaves()
    {
        if (!Application.isPlaying || !enabled)
            return;

        if (waveBurstCoroutine != null)
            return;

        waveBurstCoroutine = StartCoroutine(TriggerWavesCoroutine());
    }

    private IEnumerator TriggerWavesCoroutine()
    {
        for (int i = 0; i < MaxWaves; i++)
        {
            TriggerWave();

            if (i < MaxWaves - 1)
                yield return new WaitForSeconds(waveInterval);
        }

        waveBurstCoroutine = null;
    }
}
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

    private float currentRadius;
    private bool isWaveActive;

    private static readonly int WaveRadiusID =
        Shader.PropertyToID("_WaveRadius");

    private static readonly int WaveOriginID =
        Shader.PropertyToID("_WaveOrigin");

    private static readonly int WaveWidthID =
        Shader.PropertyToID("_WaveWidth");

    private void Start()
    {
        // Ensure the wave is initially invisible.
        waveMaterial.SetFloat(WaveRadiusID, -100f);
    }

    private void Update()
    {
        if (!isWaveActive)
            return;

        currentRadius += waveSpeed * Time.deltaTime;

        if (currentRadius >= maxRadius)
        {
            isWaveActive = false;

            // Hide the wave after propagation.
            waveMaterial.SetFloat(WaveRadiusID, -100f);
            return;
        }

        waveMaterial.SetFloat(WaveRadiusID, currentRadius);
    }

    [ContextMenu("Trigger Wave")]
    public void TriggerWave()
    {
        if (!Application.isPlaying)
            return;

        if (waveMaterial == null || waveOrigin == null)
            return;

        currentRadius = 0f;
        isWaveActive = true;

        // Capture the sound origin at the moment of triggering.
        waveMaterial.SetVector(
            WaveOriginID,
            waveOrigin.position
        );

        waveMaterial.SetFloat(WaveRadiusID, currentRadius);
    }

    [ContextMenu("Trigger Waves")]
    public void TriggerWaves()
    {
        TriggerWave();
        TriggerWave();
        TriggerWave();
        TriggerWave();
    }
}
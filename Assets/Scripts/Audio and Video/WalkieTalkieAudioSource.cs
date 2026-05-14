using UnityEngine;

/// <summary>
/// Makes an AudioSource sound like a walkie-talkie/intercom on a desk.
/// Attach to Props_Intercom_System_v001 (or whichever object holds the
/// AudioSource bound to the VO track in C1_Timeline_Inventory).
///
/// Adds a bandpass filter (high-pass + low-pass) to simulate radio frequency
/// and sets the AudioSource to 3D so the VO comes FROM the desk, not the centre.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class WalkieTalkieAudioSource : MonoBehaviour
{
    [Header("3D Positioning")]
    [Tooltip("1 = fully 3D. Audio comes from the desk position, not the player's head.")]
    [Range(0f, 1f)]
    [SerializeField] private float spatialBlend = 1f;

    [Tooltip("How close the player needs to be before volume is at full.")]
    [SerializeField] private float minDistance = 0.5f;

    [Tooltip("Distance at which the walkie-talkie becomes very quiet.")]
    [SerializeField] private float maxDistance = 6f;

    [Header("Radio Filter")]
    [Tooltip("Simulate the tinny, bandpass sound of a radio/intercom. " +
             "Adds AudioHighPassFilter + AudioLowPassFilter to the GameObject.")]
    [SerializeField] private bool applyRadioFilter = true;

    [Tooltip("High-pass cutoff — removes bass. 600-900 Hz feels like a walkie-talkie.")]
    [SerializeField] private float highPassCutoff = 800f;

    [Tooltip("Low-pass cutoff — removes high-end hiss. 3000-4000 Hz is realistic.")]
    [SerializeField] private float lowPassCutoff = 3500f;

    private AudioSource _audioSource;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _audioSource.playOnAwake = false;
        Apply();
    }

    private void Apply()
    {
        _audioSource.spatialBlend  = spatialBlend;
        _audioSource.minDistance   = minDistance;
        _audioSource.maxDistance   = maxDistance;
        _audioSource.rolloffMode   = AudioRolloffMode.Logarithmic;
        _audioSource.dopplerLevel  = 0f;

        if (!applyRadioFilter) return;

        // High-pass: cut low rumble, keep mid/high voice frequencies
        var hpf = gameObject.GetComponent<AudioHighPassFilter>();
        if (hpf == null) hpf = gameObject.AddComponent<AudioHighPassFilter>();
        hpf.cutoffFrequency         = highPassCutoff;
        hpf.highpassResonanceQ      = 1f;

        // Low-pass: cut high-end, simulates the narrow bandwidth of a radio
        var lpf = gameObject.GetComponent<AudioLowPassFilter>();
        if (lpf == null) lpf = gameObject.AddComponent<AudioLowPassFilter>();
        lpf.cutoffFrequency         = lowPassCutoff;
        lpf.lowpassResonanceQ       = 1f;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (_audioSource == null) _audioSource = GetComponent<AudioSource>();
        if (_audioSource != null) Apply();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.2f, 0.6f, 1f, 0.15f);
        Gizmos.DrawSphere(transform.position, minDistance);
        Gizmos.color = new Color(0.2f, 0.6f, 1f, 0.04f);
        Gizmos.DrawSphere(transform.position, maxDistance);
    }
#endif
}

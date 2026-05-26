using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class FootstepAudioSource : MonoBehaviour
{
    [Header("Spatialisation")]
    [Tooltip("Enables Meta XR HRTF spatializer. Audio comes FROM this object's world position.")]
    [SerializeField] private bool spatialize = true;

    [Tooltip("0 = 2D (no position), 1 = fully 3D positional. Keep at 1 for grounded footsteps.")]
    [Range(0f, 1f)]
    [SerializeField] private float spatialBlend = 1f;

    [Tooltip("Distance at which volume is at full. Keep small — footsteps are close.")]
    [SerializeField] private float minDistance = 1f;

    [Tooltip("Distance at which volume fades to near-zero.")]
    [SerializeField] private float maxDistance = 12f;

    private AudioSource _audioSource;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _audioSource.playOnAwake  = false;
        _audioSource.spatialize   = spatialize;
        _audioSource.spatialBlend = spatialBlend;
        _audioSource.minDistance  = minDistance;
        _audioSource.maxDistance  = maxDistance;
        _audioSource.rolloffMode  = AudioRolloffMode.Logarithmic;
        _audioSource.dopplerLevel = 0f;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (_audioSource == null) _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null) return;

        _audioSource.spatialize   = spatialize;
        _audioSource.spatialBlend = spatialBlend;
        _audioSource.minDistance  = minDistance;
        _audioSource.maxDistance  = maxDistance;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 1f, 0.4f, 0.2f);
        Gizmos.DrawSphere(transform.position, minDistance);
        Gizmos.color = new Color(0f, 1f, 0.4f, 0.04f);
        Gizmos.DrawSphere(transform.position, maxDistance);
    }
#endif
}

using UnityEngine;

/// <summary>
/// Enables Meta XR HRTF spatialisation on an AudioSource so audio comes FROM
/// this GameObject's world position. Tick Spatialize in the Inspector or let
/// this script do it on Awake.
///
/// Setup (per lead guidance):
///   1. Place this GameObject at the position you want the sound to come FROM
///      (e.g. behind the desk where the warder starts, at floor level).
///   2. Add an AudioSource component — assign your clip OR leave blank and let
///      the Timeline Audio Track drive it (bind this object's AudioSource to
///      the footstep Audio Track on Timeline_Inventory's PlayableDirector).
///   3. To animate movement (footsteps walking toward the door):
///      - Add an Animation Track on C1_Timeline_Inventory targeting this GameObject
///      - Keyframe the position from start point → door position over the clip duration
///   4. For the walkie-talkie prop: same setup, place on the desk GameObject.
/// </summary>
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

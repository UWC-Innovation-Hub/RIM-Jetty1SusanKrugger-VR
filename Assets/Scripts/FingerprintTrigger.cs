using UnityEngine;
using UnityEngine.Video;

public class FingerprintTrigger : MonoBehaviour
{
    [Header("Wiring")]
    [SerializeField] private ProjectorController projector;
    [SerializeField] private VideoClip clip;

    [Header("Touch Filtering (optional)")]
    [SerializeField] private string requiredTag = ""; // e.g. "Hand" or "FingerTip"
    [SerializeField] private bool disableColliderWhenLocked = true;

    private Collider _col;
    private MeshRenderer _mesh;
    private bool _armed = true;

    private void Awake()
    {
        _col = GetComponent<Collider>();
        _mesh = GetComponent<MeshRenderer>();
        if (!_col) Debug.LogWarning($"{name}: FingerprintTrigger needs a Collider.");
    }

    public void SetArmed(bool armed)
    {
        _armed = armed;
        if (_col && disableColliderWhenLocked) _col.enabled = armed; _mesh.enabled = armed;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!_armed) return;

        if (!string.IsNullOrEmpty(requiredTag) && !other.CompareTag(requiredTag))
            return;

        if (projector == null || clip == null) return;

        // Only succeeds if projector is idle.
        projector.TryPlay(clip);
    }
}

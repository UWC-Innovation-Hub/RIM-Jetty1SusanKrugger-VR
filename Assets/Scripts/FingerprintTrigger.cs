using UnityEngine;
using UnityEngine.Video;

public class FingerprintTrigger : MonoBehaviour
{
    [Header("Wiring")]
    [SerializeField] private ProjectorController projector;
    public ProjectorController proj;
    [SerializeField] private VideoClip clip;

    [Header("Touch Filtering (optional)")]
    [SerializeField] private string requiredTag = ""; // e.g. "Hand" or "FingerTip"
    [SerializeField] private bool disableColliderWhenLocked = true;

    [SerializeField] private Material FingerGlowMat;

    private Collider _col;
    private MeshRenderer _mesh;
    [SerializeField] private Material _meshMat;
    [SerializeField] private MeshRenderer _Glowmesh;
    private bool _armed = true;

    private AudioSource AS;

    private void Awake()
    {
        _col = GetComponent<Collider>();
        _mesh = GetComponent<MeshRenderer>();
        if (!_col) Debug.LogWarning($"{name}: FingerprintTrigger needs a Collider.");
        AS = GetComponent<AudioSource>();
    }

    public void SetArmed(bool armed)
    {
        _armed = armed;
        if (_col && disableColliderWhenLocked) 
        {
            _col.enabled = armed;
            //_mesh.enabled = armed;

            if (armed)
            {
                _meshMat.EnableKeyword("_EMISSION");
                //FingerGlowMat.SetFloat("_EmissionStrength", 0.2f);
            }
            else
            {
                _meshMat.DisableKeyword("_EMISSION");
                //FingerGlowMat.SetFloat("_EmissionStrength", 0f);
            }                
            _Glowmesh.enabled = armed;

            ////FADE SOLUTION
            ////Change emissive property on armed. (Glow when FP needs selecting, turn off once selected)
            //if (armed)
            //{
            //    FingerGlowMat.SetFloat("_EmissionStrength", 0.2f);
            //}
            //else
            //{
            //    FingerGlowMat.SetFloat("_EmissionStrength", 0f);
            //}
        }        
    }  

    private void OnTriggerEnter(Collider other)
    {
        AS.Play();
        if (!_armed) return;

        if (!string.IsNullOrEmpty(requiredTag) && !other.CompareTag(requiredTag))
            return;

        if (projector == null || clip == null) return;

        // Only succeeds if projector is idle.
        projector.TryPlay(clip);
    }


    private void OnApplicationQuit()
    {
        _meshMat.EnableKeyword("_EMISSION");
    }


}

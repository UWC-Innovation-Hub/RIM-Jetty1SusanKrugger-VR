using System.Collections;
using UnityEngine;
using UnityEngine.Video;

public class ProjectorController : MonoBehaviour
{
    private enum State { Idle, Activating, Playing, Deactivating }

    [Header("Wiring")]
    [SerializeField] private VideoProjectorController projector;   // your existing script on "Projector"
    [SerializeField] private VideoPlayer videoPlayer;              // can be projector.videoPlayer too
    [SerializeField] private FingerprintLockout lockout;           // optional

    [Header("Open / Close Animation")]
    [SerializeField, Min(0.01f)] private float activateDuration = 0.75f;
    [SerializeField, Min(0.01f)] private float deactivateDuration = 0.6f;

    [Tooltip("FOV when 'closed'. Use 1-5 degrees, not 0.")]
    [SerializeField, Range(1f, 170f)] private float closedFOV = 1f;

    [Tooltip("If 0, we'll use whatever projector.fieldOfView was at startup as 'open'.")]
    [SerializeField, Range(0f, 170f)] private float openFOVOverride = 0f;

    [Tooltip("Intensity when 'closed' (off).")]
    [SerializeField, Range(0f, 2f)] private float closedIntensity = 0f;

    [Tooltip("If < 0, we'll use whatever projector.intensity was at startup as 'open'.")]
    [SerializeField] private float openIntensityOverride = -1f;

    [Header("Video Start Gate")]
    [Tooltip("When FOV reaches this fraction of the open FOV during activation, we start the video.")]
    [Range(0f, 1f)] public float videoEnableThreshold = 0.75f;

    [Header("Optional: Stop updating globals when idle")]
    [Tooltip("If true, disables VideoProjectorController when idle (saves a tiny bit of CPU).")]
    [SerializeField] private bool disableProjectorComponentWhenIdle = false;

    private State _state = State.Idle;
    private Coroutine _routine;

    private float _openFOV;
    private float _openIntensity;

    private void Awake()
    {
        if (projector == null)
            projector = GetComponentInChildren<VideoProjectorController>(true);

        if (videoPlayer == null && projector != null)
            videoPlayer = projector.videoPlayer;

        if (projector == null)
            Debug.LogError($"{name}: ProjectorController needs a VideoProjectorController reference.");

        if (videoPlayer == null)
            Debug.LogError($"{name}: ProjectorController needs a VideoPlayer reference.");

        CacheOpenValues();
        ResetToIdleImmediate();
    }

    private void OnEnable()
    {
        if (videoPlayer != null)
            videoPlayer.loopPointReached += OnVideoFinished;
    }

    private void OnDisable()
    {
        if (videoPlayer != null)
            videoPlayer.loopPointReached -= OnVideoFinished;
    }

    private void CacheOpenValues()
    {
        if (projector == null) return;

        _openFOV = (openFOVOverride > 0f) ? openFOVOverride : projector.fieldOfView;
        _openIntensity = (openIntensityOverride >= 0f) ? openIntensityOverride : projector.intensity;

        // Safety clamps
        _openFOV = Mathf.Clamp(_openFOV, 1f, 170f);
        _openIntensity = Mathf.Max(0f, _openIntensity);
    }

    /// <summary>
    /// Called by fingerprints. Returns false if projector is busy.
    /// </summary>
    public bool TryPlay(VideoClip clip)
    {
        if (clip == null) return false;
        if (_state != State.Idle) return false;

        lockout?.SetAllArmed(false);

        if (_routine != null) StopCoroutine(_routine);
        _routine = StartCoroutine(PlaySequence(clip));
        return true;
    }

    private IEnumerator PlaySequence(VideoClip clip)
    {
        _state = State.Activating;

        if (disableProjectorComponentWhenIdle && projector != null)
            projector.enabled = true;

        StopAndDisableVideo();

        // Ensure we start closed/off
        SetFOV(closedFOV);
        SetIntensity(closedIntensity);

        float t = 0f;
        bool videoStarted = false;

        while (t < activateDuration)
        {
            t += Time.deltaTime;
            float u = Mathf.Clamp01(t / activateDuration);
            float eased = EaseInOut(u);

            float fov = Mathf.Lerp(closedFOV, _openFOV, eased);
            float intensity = Mathf.Lerp(closedIntensity, _openIntensity, eased);

            SetFOV(fov);
            SetIntensity(intensity);

            // Start video when we're "open enough"
            if (!videoStarted && eased >= videoEnableThreshold)
            {
                StartVideo(clip);
                videoStarted = true;
                _state = State.Playing;
            }

            yield return null;
        }

        // Guarantee final
        SetFOV(_openFOV);
        SetIntensity(_openIntensity);

        if (!videoStarted)
        {
            StartVideo(clip);
            _state = State.Playing;
        }

        // Now we wait for loopPointReached -> OnVideoFinished
    }

    private void OnVideoFinished(VideoPlayer vp)
    {
        if (_state != State.Playing) return;

        if (_routine != null) StopCoroutine(_routine);
        _routine = StartCoroutine(DeactivateSequence());
    }

    private IEnumerator DeactivateSequence()
    {
        _state = State.Deactivating;

        // Optional: stop video immediately when fade-out begins
        // If you'd rather let last frame “hold” while fading, keep it playing and stop at the end.
        StopAndDisableVideo();

        float t = 0f;
        while (t < deactivateDuration)
        {
            t += Time.deltaTime;
            float u = Mathf.Clamp01(t / deactivateDuration);
            float eased = EaseInOut(u);

            float fov = Mathf.Lerp(_openFOV, closedFOV, eased);
            float intensity = Mathf.Lerp(_openIntensity, closedIntensity, eased);

            SetFOV(fov);
            SetIntensity(intensity);

            yield return null;
        }

        ResetToIdleImmediate();
    }

    private void ResetToIdleImmediate()
    {
        _state = State.Idle;

        StopAndDisableVideo();
        SetFOV(closedFOV);
        SetIntensity(closedIntensity);

        if (disableProjectorComponentWhenIdle && projector != null)
            projector.enabled = false;

        lockout?.SetAllArmed(true);
    }

    private void StartVideo(VideoClip clip)
    {
        if (videoPlayer == null) return;

        videoPlayer.enabled = true;
        videoPlayer.clip = clip;

        // if you ever see first-frame stalls, switch to Prepare() + prepareCompleted
        videoPlayer.Play();
    }

    private void StopAndDisableVideo()
    {
        if (videoPlayer == null) return;

        if (videoPlayer.isPlaying) videoPlayer.Stop();
        videoPlayer.clip = null;
        videoPlayer.enabled = false;
    }

    private void SetFOV(float fov)
    {
        if (projector == null) return;
        projector.fieldOfView = Mathf.Clamp(fov, 1f, 170f);
    }

    private void SetIntensity(float intensity)
    {
        if (projector == null) return;
        projector.intensity = Mathf.Max(0f, intensity);
    }

    private static float EaseInOut(float x)
    {
        // smoothstep
        return x * x * (3f - 2f * x);
    }
}

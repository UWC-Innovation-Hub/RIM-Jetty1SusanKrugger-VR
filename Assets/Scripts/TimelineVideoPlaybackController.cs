using UnityEngine;
using UnityEngine.Video;

[DisallowMultipleComponent]
public class TimelineVideoPlaybackController : MonoBehaviour
{
    [Header("Video Source")]
    [Tooltip("VideoPlayer to control. If left empty, one will be searched for on this object and its children.")]
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private bool autoFindVideoPlayer = true;
    [Tooltip("Optional clips that can be selected by index from Timeline signals.")]
    [SerializeField] private VideoClip[] availableClips;

    [Header("Startup")]
    [Tooltip("Stops the video on Start so Timeline or signals can control the first playback.")]
    [SerializeField] private bool stopPlaybackOnStart = true;
    [Tooltip("Prepares the clip on Start for a faster first Play call.")]
    [SerializeField] private bool prepareOnStart = false;
    [Tooltip("Prepare the selected clip after SetVideoClip is called.")]
    [SerializeField] private bool prepareClipOnChange = true;

    private void Reset()
    {
        ResolveVideoPlayer();
    }

    private void Awake()
    {
        ResolveVideoPlayer();
    }

    private void Start()
    {
        if (!TryGetVideoPlayer(out VideoPlayer player))
        {
            return;
        }

        if (prepareOnStart && !player.isPrepared)
        {
            player.Prepare();
        }

        if (stopPlaybackOnStart)
        {
            player.Stop();
        }
    }

    public void Play()
    {
        if (!TryGetVideoPlayer(out VideoPlayer player))
        {
            return;
        }

        player.Play();
    }

    public void Pause()
    {
        if (!TryGetVideoPlayer(out VideoPlayer player))
        {
            return;
        }

        player.Pause();
    }

    public void Stop()
    {
        if (!TryGetVideoPlayer(out VideoPlayer player))
        {
            return;
        }

        player.Stop();
    }

    public void PlayVideo()
    {
        Play();
    }

    public void PauseVideo()
    {
        Pause();
    }

    public void StopVideo()
    {
        Stop();
    }

    public void SetVideoClip(int clipIndex)
    {
        if (!TryGetVideoPlayer(out VideoPlayer player))
        {
            return;
        }

        if (availableClips == null || availableClips.Length == 0)
        {
            Debug.LogWarning($"{name}: TimelineVideoPlaybackController has no available clips configured.", this);
            return;
        }

        if (clipIndex < 0 || clipIndex >= availableClips.Length)
        {
            Debug.LogWarning($"{name}: Clip index {clipIndex} is out of range for TimelineVideoPlaybackController.", this);
            return;
        }

        VideoClip selectedClip = availableClips[clipIndex];
        SetVideoClip(selectedClip);
    }

    public void SetVideoClip(VideoClip clip)
    {
        if (!TryGetVideoPlayer(out VideoPlayer player))
        {
            return;
        }

        if (clip == null)
        {
            Debug.LogWarning($"{name}: Cannot assign a null VideoClip.", this);
            return;
        }

        player.Stop();
        player.source = VideoSource.VideoClip;
        player.clip = clip;

        if (prepareClipOnChange)
        {
            player.Prepare();
        }
    }

    public void SetAndPlayVideoClip(int clipIndex)
    {
        SetVideoClip(clipIndex);
        PlayFromStart();
    }

    public void PlayFromStart()
    {
        if (!TryGetVideoPlayer(out VideoPlayer player))
        {
            return;
        }

        player.Stop();
        player.Play();
    }

    private void ResolveVideoPlayer()
    {
        if (videoPlayer != null || !autoFindVideoPlayer)
        {
            return;
        }

        videoPlayer = GetComponent<VideoPlayer>();

        if (videoPlayer == null)
        {
            videoPlayer = GetComponentInChildren<VideoPlayer>(true);
        }
    }

    private bool TryGetVideoPlayer(out VideoPlayer player)
    {
        ResolveVideoPlayer();
        player = videoPlayer;

        if (player != null)
        {
            return true;
        }

        Debug.LogWarning($"{name}: TimelineVideoPlaybackController could not find a VideoPlayer.", this);
        return false;
    }
}

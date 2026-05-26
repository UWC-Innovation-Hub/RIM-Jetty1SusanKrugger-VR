using UnityEngine;
using UnityEngine.Video;

public class TutorialVideoController : MonoBehaviour
{
    [Header("Playback")]
    [SerializeField] private TimelineVideoPlaybackController playbackController;
    [SerializeField] private bool autoFindPlaybackController = true;

    [Header("Tutorial Clips")]
    [SerializeField] private VideoClip[] tutorialClips;
    [SerializeField] private int selectedClipIndex = -1;

    private void Reset()
    {
        ResolvePlaybackController();
    }

    private void Awake()
    {
        ResolvePlaybackController();
    }

    public void SelectTutorialClip(int clipIndex)
    {
        if (!TryResolveClip(clipIndex, out VideoClip clip))
        {
            return;
        }

        selectedClipIndex = clipIndex;
        playbackController.SetVideoClip(clip);
    }

    public void PlaySelectedTutorial()
    {
        if (!TryResolveSelectedClip(out VideoClip clip))
        {
            return;
        }

        playbackController.SetVideoClip(clip);
        playbackController.PlayFromStart();
    }

    public void PlayTutorialClip(int clipIndex)
    {
        if (!TryResolveClip(clipIndex, out VideoClip clip))
        {
            return;
        }

        selectedClipIndex = clipIndex;
        playbackController.SetVideoClip(clip);
        playbackController.PlayFromStart();
    }

    public void StopTutorial()
    {
        if (TryGetPlaybackController(out TimelineVideoPlaybackController controller))
        {
            controller.Stop();
        }
    }

    private void ResolvePlaybackController()
    {
        if (playbackController != null || !autoFindPlaybackController)
        {
            return;
        }

        playbackController = GetComponent<TimelineVideoPlaybackController>();

        if (playbackController == null)
        {
            playbackController = GetComponentInChildren<TimelineVideoPlaybackController>(true);
        }
    }

    private bool TryGetPlaybackController(out TimelineVideoPlaybackController controller)
    {
        ResolvePlaybackController();
        controller = playbackController;

        if (controller != null)
        {
            return true;
        }

        Debug.LogWarning($"{name}: TutorialVideoController could not find a TimelineVideoPlaybackController.", this);
        return false;
    }

    private bool TryResolveSelectedClip(out VideoClip clip)
    {
        return TryResolveClip(selectedClipIndex, out clip);
    }

    private bool TryResolveClip(int clipIndex, out VideoClip clip)
    {
        clip = null;

        if (!TryGetPlaybackController(out TimelineVideoPlaybackController controller))
        {
            return false;
        }

        if (tutorialClips == null || tutorialClips.Length == 0)
        {
            Debug.LogWarning($"{name}: TutorialVideoController has no tutorial clips configured.", this);
            return false;
        }

        if (clipIndex < 0 || clipIndex >= tutorialClips.Length)
        {
            Debug.LogWarning($"{name}: Tutorial clip index {clipIndex} is out of range.", this);
            return false;
        }

        clip = tutorialClips[clipIndex];
        if (clip != null)
        {
            return true;
        }

        Debug.LogWarning($"{name}: No tutorial clip is assigned at index {clipIndex}.", this);
        return false;
    }
}

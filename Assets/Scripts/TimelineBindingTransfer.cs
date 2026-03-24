using UnityEngine;
using UnityEditor;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using System.Collections.Generic;
using System.Linq;

public class TimelineBindingTransfer : MonoBehaviour
{
    [Header("Set These In The Inspector")]
    public PlayableDirector originalDirector;
    public PlayableDirector copiedDirector;

    [ContextMenu("Copy Bindings From Original To Copied Timeline")]
    public void CopyBindings()
    {
        if (originalDirector == null || copiedDirector == null)
        {
            Debug.LogError("Please assign both directors.");
            return;
        }

        var originalTimeline = originalDirector.playableAsset as TimelineAsset;
        var copiedTimeline = copiedDirector.playableAsset as TimelineAsset;

        if (originalTimeline == null || copiedTimeline == null)
        {
            Debug.LogError("Both directors must have valid TimelineAssets.");
            return;
        }

        // Recursively match tracks
        var originalTrackMap = new List<TrackAsset>();
        var copiedTrackMap = new List<TrackAsset>();

        CollectTracksRecursive(originalTimeline.GetRootTracks(), originalTrackMap);
        CollectTracksRecursive(copiedTimeline.GetRootTracks(), copiedTrackMap);

        if (originalTrackMap.Count != copiedTrackMap.Count)
        {
            Debug.LogWarning($"Track count mismatch! Original: {originalTrackMap.Count}, Copied: {copiedTrackMap.Count}");
        }

        for (int i = 0; i < Mathf.Min(originalTrackMap.Count, copiedTrackMap.Count); i++)
        {
            var originalTrack = originalTrackMap[i];
            var copiedTrack = copiedTrackMap[i];

            if (originalTrack.GetType() != copiedTrack.GetType())
            {
                Debug.LogWarning($"Track type mismatch at index {i}: {originalTrack.name} vs {copiedTrack.name}");
                continue;
            }

            var binding = originalDirector.GetGenericBinding(originalTrack);
            if (binding != null)
            {
                copiedDirector.SetGenericBinding(copiedTrack, binding);
                Debug.Log($"Copied binding for track: {copiedTrack.name}");
            }
            else
            {
                Debug.LogWarning($"No binding found on original for track: {originalTrack.name}");
            }
        }

        Debug.Log("Finished copying bindings.");
    }

    private void CollectTracksRecursive(IEnumerable<TrackAsset> tracks, List<TrackAsset> collected)
    {
        foreach (var track in tracks)
        {
            collected.Add(track);
            if (track is GroupTrack || track.GetChildTracks().Any())
            {
                CollectTracksRecursive(track.GetChildTracks(), collected);
            }
        }
    }
}

using System.Collections;
using UnityEngine;

public class AmbientAudioManager : MonoBehaviour
{
    public static AmbientAudioManager Instance { get; private set; }

    public enum AudioState
    {
        None      = 0,
        WarderRoom = 1,
        Lobby     = 2,
        Exterior  = 3
    }

    [Header("Ambient Audio Sources")]
    [SerializeField] private AudioSource warderRoomAmbience;
    [SerializeField] private AudioSource lobbyAmbience;
    [SerializeField] private AudioSource exteriorAmbience;

    [Header("Transition")]
    [SerializeField] private float crossfadeDuration = 2f;

    private AudioState _currentState = AudioState.None;
    private Coroutine _crossfadeRoutine;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void SetState(AudioState newState)
    {
        if (newState == _currentState) return;
        _currentState = newState;

        if (_crossfadeRoutine != null)
            StopCoroutine(_crossfadeRoutine);

        _crossfadeRoutine = StartCoroutine(CrossfadeToState(newState));
    }

    // Called from a Timeline Signal Receiver — pass the int value of AudioState
    // None=0, WarderRoom=1, Lobby=2, Exterior=3
    public void SetStateFromTimeline(int stateIndex)
    {
        SetState((AudioState)stateIndex);
    }

    private IEnumerator CrossfadeToState(AudioState targetState)
    {
        AudioSource[] all = { warderRoomAmbience, lobbyAmbience, exteriorAmbience };
        AudioSource target = GetSourceForState(targetState);

        // Record where each source is starting from
        float[] startVolumes = new float[all.Length];
        for (int i = 0; i < all.Length; i++)
            startVolumes[i] = all[i] != null ? all[i].volume : 0f;

        // Start target at zero volume before fading in
        if (target != null && targetState != AudioState.None)
        {
            if (!target.isPlaying)
            {
                target.volume = 0f;
                target.Play();
            }
        }

        float elapsed = 0f;
        while (elapsed < crossfadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / crossfadeDuration);

            for (int i = 0; i < all.Length; i++)
            {
                if (all[i] == null) continue;
                bool isTarget = all[i] == target && targetState != AudioState.None;
                all[i].volume = Mathf.Lerp(startVolumes[i], isTarget ? 1f : 0f, t);
            }

            yield return null;
        }

        // Stop sources that faded to zero
        for (int i = 0; i < all.Length; i++)
        {
            if (all[i] == null) continue;
            bool isTarget = all[i] == target && targetState != AudioState.None;
            if (!isTarget)
            {
                all[i].Stop();
                all[i].volume = 0f;
            }
        }

        _crossfadeRoutine = null;
    }

    private AudioSource GetSourceForState(AudioState state)
    {
        return state switch
        {
            AudioState.WarderRoom => warderRoomAmbience,
            AudioState.Lobby      => lobbyAmbience,
            AudioState.Exterior   => exteriorAmbience,
            _                     => null
        };
    }
}

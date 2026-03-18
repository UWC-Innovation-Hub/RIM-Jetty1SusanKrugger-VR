using Oculus.Interaction;
using Oculus.Interaction.PoseDetection;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(AudioSource))]
public class BreathingInteractionModule : InteractionModuleBase
{
    [Header("Wiring")]
    [SerializeField] private AudioSource breathingAudioSourceOverride;
    [SerializeField] private MonoBehaviour leftBreatheOutState;
    [SerializeField] private MonoBehaviour rightBreatheOutState;

    [Header("Volume Response")]
    [SerializeField] private float minVolume = 0.05f;
    [SerializeField] private float maxVolume = 1.0f;
    [SerializeField] private float decreaseRate = 0.2f;
    [SerializeField] private float increaseRate = 0.2f;

    [Header("Completion")]
    [SerializeField] private float completionVolumeThreshold = 0.1f;
    [SerializeField] private float completionHoldDuration = 10.0f;

    public float NormalizedBreathingValue { get; private set; } = 1f;

    private AudioSource _breathingAudioSource;
    private PoseSignal _leftPoseSignal;
    private PoseSignal _rightPoseSignal;
    private float _completionTimer;

    private void Reset()
    {
        ResolveReferences();
    }

    protected override void Awake()
    {
        base.Awake();
        ResolveReferences();
        ResetRuntimeState(stopPlayback: true);
    }

    public override void Activate()
    {
        base.Activate();

        ResolveReferences();
        if (!ValidateDependencies())
        {
            Complete();
            return;
        }

        GetVolumeBounds(out _, out float maxBound);
        _completionTimer = 0f;
        NormalizedBreathingValue = 1f;

        _breathingAudioSource.loop = true;
        _breathingAudioSource.volume = maxBound;

        if (!_breathingAudioSource.isPlaying)
        {
            _breathingAudioSource.Play();
        }
    }

    public override void Deactivate()
    {
        ResetRuntimeState(stopPlayback: true);
        base.Deactivate();
    }

    private void OnDisable()
    {
        ResetRuntimeState(stopPlayback: true);
    }

    private void Update()
    {
        if (!IsActive || IsComplete || _breathingAudioSource == null)
        {
            return;
        }

        GetVolumeBounds(out float minBound, out float maxBound);
        bool anyFist = IsPoseActive(_leftPoseSignal) || IsPoseActive(_rightPoseSignal);
        float targetVolume = anyFist ? minBound : maxBound;
        float rate = anyFist ? Mathf.Max(0f, decreaseRate) : Mathf.Max(0f, increaseRate);

        _breathingAudioSource.volume = Mathf.MoveTowards(_breathingAudioSource.volume, targetVolume, rate * Time.deltaTime);
        NormalizedBreathingValue = Mathf.InverseLerp(minBound, maxBound, _breathingAudioSource.volume);

        if (anyFist && _breathingAudioSource.volume < completionVolumeThreshold)
        {
            _completionTimer += Time.deltaTime;
            if (_completionTimer >= completionHoldDuration)
            {
                Complete();
            }
        }
        else
        {
            _completionTimer = 0f;
        }
    }

    private bool ValidateDependencies()
    {
        if (_breathingAudioSource == null)
        {
            Debug.LogWarning($"{name}: BreathingInteractionModule requires an AudioSource.");
            return false;
        }

        if (_leftPoseSignal == null || !_leftPoseSignal.IsValid
            || _rightPoseSignal == null || !_rightPoseSignal.IsValid)
        {
            Debug.LogWarning($"{name}: BreathingInteractionModule requires both left and right BreatheOut active states.");
            return false;
        }

        return true;
    }

    private void ResolveReferences()
    {
        _breathingAudioSource = breathingAudioSourceOverride != null
            ? breathingAudioSourceOverride
            : GetComponent<AudioSource>();

        if (leftBreatheOutState == null)
        {
            leftBreatheOutState = FindPoseRootState("breatheoutposeleft");
        }

        if (rightBreatheOutState == null)
        {
            rightBreatheOutState = FindPoseRootState("breatheoutposeright");
        }

        _leftPoseSignal = BuildPoseSignal(leftBreatheOutState, "breatheoutposeleft");
        _rightPoseSignal = BuildPoseSignal(rightBreatheOutState, "breatheoutposeright");
    }

    private MonoBehaviour FindPoseRootState(string normalizedPoseName)
    {
        MonoBehaviour[] components = GetComponentsInChildren<MonoBehaviour>(true);
        for (int i = 0; i < components.Length; i++)
        {
            MonoBehaviour component = components[i];
            if (component == null || !(component is ActiveStateGroup))
            {
                continue;
            }

            string normalizedName = NormalizeName(component.gameObject.name);
            if (normalizedName.Contains(normalizedPoseName))
            {
                return component;
            }
        }

        return null;
    }

    private PoseSignal BuildPoseSignal(MonoBehaviour poseReference, string normalizedPoseName)
    {
        Transform poseRoot = FindPoseRoot(poseReference, normalizedPoseName);
        if (poseRoot == null)
        {
            return PoseSignal.Invalid;
        }

        ShapeRecognizerActiveState[] shapeStates = poseRoot.GetComponentsInChildren<ShapeRecognizerActiveState>(true);
        TransformRecognizerActiveState[] transformStates = poseRoot.GetComponentsInChildren<TransformRecognizerActiveState>(true);

        if (shapeStates.Length == 0 && transformStates.Length == 0)
        {
            IActiveState fallbackState = poseReference as IActiveState;
            return fallbackState != null
                ? new PoseSignal(fallbackState)
                : PoseSignal.Invalid;
        }

        return new PoseSignal(shapeStates, transformStates);
    }

    private Transform FindPoseRoot(MonoBehaviour poseReference, string normalizedPoseName)
    {
        if (poseReference != null)
        {
            Transform current = poseReference.transform;
            while (current != null)
            {
                if (NormalizeName(current.name).Contains(normalizedPoseName))
                {
                    return current;
                }

                if (current == transform)
                {
                    break;
                }

                current = current.parent;
            }

            return poseReference.transform;
        }

        Transform[] transforms = GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < transforms.Length; i++)
        {
            Transform candidate = transforms[i];
            if (NormalizeName(candidate.name).Contains(normalizedPoseName))
            {
                return candidate;
            }
        }

        return null;
    }

    private void ResetRuntimeState(bool stopPlayback)
    {
        _completionTimer = 0f;
        NormalizedBreathingValue = 1f;

        if (_breathingAudioSource == null)
        {
            return;
        }

        GetVolumeBounds(out _, out float maxBound);
        _breathingAudioSource.volume = maxBound;

        if (stopPlayback)
        {
            _breathingAudioSource.Stop();
        }
    }

    private void GetVolumeBounds(out float minBound, out float maxBound)
    {
        minBound = Mathf.Clamp01(minVolume);
        maxBound = Mathf.Clamp01(maxVolume);

        if (maxBound < minBound)
        {
            maxBound = minBound;
        }
    }

    private static bool IsPoseActive(PoseSignal poseSignal)
    {
        return poseSignal != null && poseSignal.IsActive;
    }

    private static string NormalizeName(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        char[] buffer = new char[value.Length];
        int index = 0;
        for (int i = 0; i < value.Length; i++)
        {
            char character = value[i];
            if (char.IsLetterOrDigit(character))
            {
                buffer[index++] = char.ToLowerInvariant(character);
            }
        }

        return new string(buffer, 0, index);
    }

    private sealed class PoseSignal
    {
        public static readonly PoseSignal Invalid = new PoseSignal();

        private readonly ShapeRecognizerActiveState[] _shapeStates;
        private readonly TransformRecognizerActiveState[] _transformStates;
        private readonly IActiveState _fallbackState;

        public bool IsValid =>
            (_shapeStates != null && _shapeStates.Length > 0)
            || (_transformStates != null && _transformStates.Length > 0)
            || _fallbackState != null;

        public bool IsActive
        {
            get
            {
                if (!IsValid)
                {
                    return false;
                }

                bool hasRecognizers = false;

                if (_shapeStates != null && _shapeStates.Length > 0)
                {
                    hasRecognizers = true;
                    for (int i = 0; i < _shapeStates.Length; i++)
                    {
                        if (_shapeStates[i] == null || !_shapeStates[i].Active)
                        {
                            return false;
                        }
                    }
                }

                if (_transformStates != null && _transformStates.Length > 0)
                {
                    hasRecognizers = true;
                    for (int i = 0; i < _transformStates.Length; i++)
                    {
                        if (_transformStates[i] == null || !_transformStates[i].Active)
                        {
                            return false;
                        }
                    }
                }

                return hasRecognizers || (_fallbackState != null && _fallbackState.Active);
            }
        }

        private PoseSignal()
        {
        }

        public PoseSignal(IActiveState fallbackState)
        {
            _fallbackState = fallbackState;
        }

        public PoseSignal(
            ShapeRecognizerActiveState[] shapeStates,
            TransformRecognizerActiveState[] transformStates)
        {
            _shapeStates = shapeStates;
            _transformStates = transformStates;
        }
    }
}

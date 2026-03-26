using Oculus.Interaction;
using Oculus.Interaction.PoseDetection;
using UnityEngine;
using UnityEngine.Serialization;

[DisallowMultipleComponent]
[RequireComponent(typeof(AudioSource))]
public class BreathingInteractionModule : InteractionModuleBase
{
    public enum BreathingPhase
    {
        WaitingForInhale = 0,
        WaitingForExhale = 1
    }

    [Header("Wiring")]
    [SerializeField] private AudioSource breathingAudioSourceOverride;
    [FormerlySerializedAs("leftBreatheOutState")]
    [SerializeField] private MonoBehaviour leftInhalePoseState;
    [FormerlySerializedAs("rightBreatheOutState")]
    [SerializeField] private MonoBehaviour rightInhalePoseState;
    [SerializeField] private MonoBehaviour leftExhalePoseState;
    [SerializeField] private MonoBehaviour rightExhalePoseState;

    [Header("Audio")]
    [SerializeField] private AudioClip inhaleClip;
    [SerializeField] private AudioClip exhaleClip;

    [Header("Breath Count")]
    [SerializeField] private int breathsRequired = 3;
    [SerializeField] private float poseEntryHoldTime = 0.15f;

    [Header("Debug")]
    [SerializeField] private int completedBreathCount;
    [SerializeField] private BreathingPhase currentPhase = BreathingPhase.WaitingForInhale;

    public int CompletedBreathCount => completedBreathCount;
    public BreathingPhase CurrentPhase => currentPhase;

    private AudioSource _breathingAudioSource;
    private PoseSignal _leftInhalePoseSignal;
    private PoseSignal _rightInhalePoseSignal;
    private PoseSignal _leftExhalePoseSignal;
    private PoseSignal _rightExhalePoseSignal;
    private float _inhaleStableTime;
    private float _exhaleStableTime;
    private bool _inhaleLatched;
    private bool _exhaleLatched;
    private bool _warnedMissingInhaleClip;
    private bool _warnedMissingExhaleClip;

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
        ResetRuntimeState(stopPlayback: true);

        if (!ValidateDependencies())
        {
            Complete();
            return;
        }

        _breathingAudioSource.loop = false;
        _breathingAudioSource.volume = 1f;

        if (_breathingAudioSource.isPlaying)
        {
            _breathingAudioSource.Stop();
        }

        if (Mathf.Max(0, breathsRequired) == 0)
        {
            Complete();
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
        if (!IsActive || IsComplete)
        {
            return;
        }

        bool inhaleActive = IsPoseActive(_leftInhalePoseSignal) || IsPoseActive(_rightInhalePoseSignal);
        bool exhaleActive = IsPoseActive(_leftExhalePoseSignal) || IsPoseActive(_rightExhalePoseSignal);

        if (inhaleActive && exhaleActive)
        {
            ResetPoseEntryTracking();
            return;
        }

        UpdateInhaleTracking(inhaleActive);
        UpdateExhaleTracking(exhaleActive);
    }

    private void UpdateInhaleTracking(bool inhaleActive)
    {
        if (!inhaleActive)
        {
            _inhaleStableTime = 0f;
            _inhaleLatched = false;
            return;
        }

        _inhaleStableTime += Time.deltaTime;

        if (_inhaleLatched || _inhaleStableTime < Mathf.Max(0f, poseEntryHoldTime))
        {
            return;
        }

        _inhaleLatched = true;
        TryAcceptInhaleEntry();
    }

    private void UpdateExhaleTracking(bool exhaleActive)
    {
        if (!exhaleActive)
        {
            _exhaleStableTime = 0f;
            _exhaleLatched = false;
            return;
        }

        _exhaleStableTime += Time.deltaTime;

        if (_exhaleLatched || _exhaleStableTime < Mathf.Max(0f, poseEntryHoldTime))
        {
            return;
        }

        _exhaleLatched = true;
        TryAcceptExhaleEntry();
    }

    private void TryAcceptInhaleEntry()
    {
        if (currentPhase != BreathingPhase.WaitingForInhale)
        {
            return;
        }

        currentPhase = BreathingPhase.WaitingForExhale;
        PlayPhaseClip(inhaleClip, nameof(inhaleClip), ref _warnedMissingInhaleClip);
    }

    private void TryAcceptExhaleEntry()
    {
        if (currentPhase != BreathingPhase.WaitingForExhale)
        {
            return;
        }

        completedBreathCount++;
        PlayPhaseClip(exhaleClip, nameof(exhaleClip), ref _warnedMissingExhaleClip);

        if (completedBreathCount >= Mathf.Max(1, breathsRequired))
        {
            Complete();
            return;
        }

        currentPhase = BreathingPhase.WaitingForInhale;
    }

    private void PlayPhaseClip(AudioClip clip, string clipFieldName, ref bool warningLogged)
    {
        if (_breathingAudioSource == null)
        {
            return;
        }

        if (clip == null)
        {
            if (!warningLogged)
            {
                Debug.LogWarning($"{name}: BreathingInteractionModule is missing {clipFieldName}.");
                warningLogged = true;
            }

            return;
        }

        _breathingAudioSource.PlayOneShot(clip);
    }

    private bool ValidateDependencies()
    {
        if (_breathingAudioSource == null)
        {
            Debug.LogWarning($"{name}: BreathingInteractionModule requires an AudioSource.");
            return false;
        }

        if (_leftInhalePoseSignal == null || !_leftInhalePoseSignal.IsValid
            || _rightInhalePoseSignal == null || !_rightInhalePoseSignal.IsValid
            || _leftExhalePoseSignal == null || !_leftExhalePoseSignal.IsValid
            || _rightExhalePoseSignal == null || !_rightExhalePoseSignal.IsValid)
        {
            Debug.LogWarning($"{name}: BreathingInteractionModule requires inhale and exhale pose states for both hands.");
            return false;
        }

        return true;
    }

    private void ResolveReferences()
    {
        _breathingAudioSource = breathingAudioSourceOverride != null
            ? breathingAudioSourceOverride
            : GetComponent<AudioSource>();

        if (leftInhalePoseState == null)
        {
            leftInhalePoseState = FindPoseRootState("breatheoutposeleft");
        }

        if (rightInhalePoseState == null)
        {
            rightInhalePoseState = FindPoseRootState("breatheoutposeright");
        }

        if (leftExhalePoseState == null)
        {
            leftExhalePoseState = FindPoseRootState("stopposeleft");
        }

        if (rightExhalePoseState == null)
        {
            rightExhalePoseState = FindPoseRootState("stopposeright");
        }

        if (inhaleClip == null && _breathingAudioSource != null)
        {
            inhaleClip = _breathingAudioSource.clip;
        }

        _leftInhalePoseSignal = BuildPoseSignal(leftInhalePoseState, "breatheoutposeleft");
        _rightInhalePoseSignal = BuildPoseSignal(rightInhalePoseState, "breatheoutposeright");
        _leftExhalePoseSignal = BuildPoseSignal(leftExhalePoseState, "stopposeleft");
        _rightExhalePoseSignal = BuildPoseSignal(rightExhalePoseState, "stopposeright");
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
        completedBreathCount = 0;
        currentPhase = BreathingPhase.WaitingForInhale;
        ResetPoseEntryTracking();
        _warnedMissingInhaleClip = false;
        _warnedMissingExhaleClip = false;

        if (_breathingAudioSource == null)
        {
            return;
        }

        _breathingAudioSource.loop = false;
        _breathingAudioSource.volume = 1f;

        if (stopPlayback)
        {
            _breathingAudioSource.Stop();
        }
    }

    private void ResetPoseEntryTracking()
    {
        _inhaleStableTime = 0f;
        _exhaleStableTime = 0f;
        _inhaleLatched = false;
        _exhaleLatched = false;
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

using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Serialization;

public class BreathingInteractionModule : InteractionModuleBase
{
    public enum BreathingPhase
    {
        WaitingForInhale = 0,
        WaitingForExhale = 1
    }

    private enum RuntimeState
    {
        Idle = 0,
        Running = 1,
        Finishing = 2
    }

    [Header("Pose Wiring")]
    [FormerlySerializedAs("leftBreatheOutState")]
    [SerializeField] private MonoBehaviour leftInhalePoseState;
    [FormerlySerializedAs("rightBreatheOutState")]
    [SerializeField] private MonoBehaviour rightInhalePoseState;
    [SerializeField] private MonoBehaviour leftExhalePoseState;
    [SerializeField] private MonoBehaviour rightExhalePoseState;

    [Header("Audio Wiring")]
    [FormerlySerializedAs("breathingAudioSourceOverride")]
    [SerializeField] private AudioSource cueAudioSourceOverride;
    [SerializeField] private AudioSource loopAudioSourceA;
    [SerializeField] private AudioSource loopAudioSourceB;
    [SerializeField] private AudioClip fastLoopClip;
    [SerializeField] private AudioClip mediumLoopClip;
    [SerializeField] private AudioClip calmLoopClip;
    [SerializeField] private bool playGestureCueClips = true;
    [SerializeField] private AudioClip inhaleClip;
    [SerializeField] private AudioClip exhaleClip;

    [Header("Vignette Wiring")]
    [SerializeField] private Renderer vignetteRenderer;

    [Header("Breathing Goals")]
    [Min(1)]
    [SerializeField] private int breathsRequired = 3;
    [Min(0f)]
    [SerializeField] private float poseEntryHoldTime = 0.15f;

    [Header("Vignette Motion")]
    [SerializeField] private float inhaleRadius = 0.16f;
    [SerializeField] private float exhaleRadius = 0.30f;
    [Min(0.05f)]
    [SerializeField] private float fastPulseDuration = 1.0f;
    [Min(0.05f)]
    [SerializeField] private float mediumPulseDuration = 1.45f;
    [Min(0.05f)]
    [SerializeField] private float calmPulseDuration = 1.9f;
    [Min(0.05f)]
    [SerializeField] private float finalFadeDuration = 0.6f;

    [Header("Audio Motion")]
    [Min(0.05f)]
    [SerializeField] private float loopCrossfadeDuration = 0.75f;
    [Range(0f, 1f)]
    [SerializeField] private float loopVolume = 0.8f;
    [Range(0f, 1f)]
    [SerializeField] private float cueVolume = 0.35f;

    [Header("Runtime Debug")]
    [SerializeField] private int completedBreathCount;
    [SerializeField] private BreathingPhase currentPhase = BreathingPhase.WaitingForInhale;
    [SerializeField] private int currentCalmStage;
    [SerializeField] private bool inhalePoseActive;
    [SerializeField] private bool exhalePoseActive;

    private static readonly int InnerRadiusPropertyId = Shader.PropertyToID("_InnerRadius");
    private static readonly int OpacityPropertyId = Shader.PropertyToID("_Opacity");

    private PoseSignal _leftInhaleSignal;
    private PoseSignal _rightInhaleSignal;
    private PoseSignal _leftExhaleSignal;
    private PoseSignal _rightExhaleSignal;

    private RuntimeState _runtimeState = RuntimeState.Idle;
    private MaterialPropertyBlock _vignettePropertyBlock;
    private AudioSource _cueAudioSource;
    private AudioSource _currentLoopSource;
    private AudioSource _inactiveLoopSource;

    private float _inhaleStableTime;
    private float _exhaleStableTime;
    private bool _inhaleEntryLatched;
    private bool _exhaleEntryLatched;

    private float _pulseClock;
    private float _finishElapsed;

    private bool _isCrossfading;
    private AudioSource _crossfadeFromSource;
    private AudioSource _crossfadeToSource;
    private float _crossfadeElapsed;

    private bool _warnedMissingLoopSource;
    private bool _warnedMissingVignette;

    protected override void Awake()
    {
        base.Awake();
        EnsureRuntimeReferences();
        RebuildPoseSignals();
        ApplyHiddenVignette();
        StopAllLoopAudio();
    }

    private void Reset()
    {
        AutoResolveSceneReferences();
    }

    public override void Activate()
    {
        base.Activate();

        EnsureRuntimeReferences();
        RebuildPoseSignals();
        ResetRuntimeState();

        PrepareCueSource();
        StartLoopStage(0, immediate: true);

        _runtimeState = RuntimeState.Running;
        ApplyVignette(exhaleRadius, 1f);
    }

    public override void Deactivate()
    {
        ResetRuntimeState();
        StopAllLoopAudio();

        if (_cueAudioSource != null)
        {
            _cueAudioSource.Stop();
        }

        ApplyHiddenVignette();
        _runtimeState = RuntimeState.Idle;

        base.Deactivate();
    }

    private void Update()
    {
        if (!IsActive || IsComplete)
        {
            return;
        }

        float deltaTime = Time.deltaTime;
        UpdateLoopCrossfade(deltaTime);

        if (_runtimeState == RuntimeState.Finishing)
        {
            UpdateFinishing(deltaTime);
            return;
        }

        if (_runtimeState != RuntimeState.Running)
        {
            return;
        }

        UpdateGestureState(deltaTime);
        UpdatePulse(deltaTime);
    }

    private void UpdateGestureState(float deltaTime)
    {
        inhalePoseActive = _leftInhaleSignal.IsActive() || _rightInhaleSignal.IsActive();
        exhalePoseActive = _leftExhaleSignal.IsActive() || _rightExhaleSignal.IsActive();

        bool ambiguous = inhalePoseActive && exhalePoseActive;

        if (inhalePoseActive)
        {
            _inhaleStableTime += deltaTime;
        }
        else
        {
            _inhaleStableTime = 0f;
            _inhaleEntryLatched = false;
        }

        if (exhalePoseActive)
        {
            _exhaleStableTime += deltaTime;
        }
        else
        {
            _exhaleStableTime = 0f;
            _exhaleEntryLatched = false;
        }

        if (ambiguous)
        {
            return;
        }

        if (currentPhase == BreathingPhase.WaitingForInhale && CanAcceptInhaleEntry())
        {
            AcceptInhale();
            return;
        }

        if (currentPhase == BreathingPhase.WaitingForExhale && CanAcceptExhaleEntry())
        {
            AcceptExhale();
        }
    }

    private bool CanAcceptInhaleEntry()
    {
        return inhalePoseActive && !_inhaleEntryLatched && _inhaleStableTime >= poseEntryHoldTime;
    }

    private bool CanAcceptExhaleEntry()
    {
        return exhalePoseActive && !_exhaleEntryLatched && _exhaleStableTime >= poseEntryHoldTime;
    }

    private void AcceptInhale()
    {
        _inhaleEntryLatched = true;
        currentPhase = BreathingPhase.WaitingForExhale;
        PlayCue(inhaleClip, "inhale");
    }

    private void AcceptExhale()
    {
        _exhaleEntryLatched = true;
        PlayCue(exhaleClip, "exhale");

        completedBreathCount++;

        if (completedBreathCount >= breathsRequired)
        {
            BeginFinalFade();
            return;
        }

        currentPhase = BreathingPhase.WaitingForInhale;
        currentCalmStage = Mathf.Clamp(completedBreathCount, 0, 2);
        StartLoopStage(currentCalmStage, immediate: false);
    }

    private void BeginFinalFade()
    {
        currentCalmStage = Mathf.Clamp(breathsRequired - 1, 0, 2);
        currentPhase = BreathingPhase.WaitingForInhale;
        _runtimeState = RuntimeState.Finishing;
        _finishElapsed = 0f;
        _pulseClock = 0f;
        ApplyVignette(exhaleRadius, 1f);
    }

    private void UpdatePulse(float deltaTime)
    {
        _pulseClock += deltaTime;
        float duration = GetPulseDurationForStage(currentCalmStage);
        float normalized = 0.5f + (0.5f * Mathf.Cos(Mathf.PI * 2f * (_pulseClock / duration)));
        float radius = Mathf.Lerp(inhaleRadius, exhaleRadius, normalized);
        ApplyVignette(radius, 1f);
    }

    private void UpdateFinishing(float deltaTime)
    {
        _finishElapsed += deltaTime;
        float normalized = finalFadeDuration <= 0f ? 1f : Mathf.Clamp01(_finishElapsed / finalFadeDuration);
        float loopLevel = Mathf.Lerp(loopVolume, 0f, normalized);
        float vignetteOpacity = Mathf.Lerp(1f, 0f, normalized);

        if (_currentLoopSource != null)
        {
            _currentLoopSource.volume = loopLevel;
        }

        if (_inactiveLoopSource != null)
        {
            _inactiveLoopSource.volume = 0f;
        }

        ApplyVignette(exhaleRadius, vignetteOpacity);

        if (normalized >= 1f)
        {
            Complete();
        }
    }

    private void StartLoopStage(int calmStage, bool immediate)
    {
        AudioClip targetClip = GetLoopClipForStage(calmStage);
        if (targetClip == null)
        {
            Debug.LogWarning($"{name}: No breathing loop clip is assigned for calm stage {calmStage}.");
            return;
        }

        if (_currentLoopSource == null)
        {
            if (!TryResolveLoopSources())
            {
                if (!_warnedMissingLoopSource)
                {
                    Debug.LogWarning($"{name}: Breathing loop sources are not assigned. Loop bed playback is disabled.");
                    _warnedMissingLoopSource = true;
                }

                return;
            }
        }

        if (_currentLoopSource.clip == targetClip && _currentLoopSource.isPlaying)
        {
            _currentLoopSource.volume = loopVolume;
            return;
        }

        if (immediate || _inactiveLoopSource == null)
        {
            _currentLoopSource.Stop();
            _currentLoopSource.clip = targetClip;
            _currentLoopSource.loop = true;
            _currentLoopSource.volume = loopVolume;
            _currentLoopSource.Play();

            if (_inactiveLoopSource != null)
            {
                _inactiveLoopSource.Stop();
                _inactiveLoopSource.volume = 0f;
            }

            _isCrossfading = false;
            return;
        }

        _inactiveLoopSource.Stop();
        _inactiveLoopSource.clip = targetClip;
        _inactiveLoopSource.loop = true;
        _inactiveLoopSource.volume = 0f;
        _inactiveLoopSource.Play();

        _crossfadeFromSource = _currentLoopSource;
        _crossfadeToSource = _inactiveLoopSource;
        _crossfadeElapsed = 0f;
        _isCrossfading = true;
    }

    private void UpdateLoopCrossfade(float deltaTime)
    {
        if (!_isCrossfading || _crossfadeFromSource == null || _crossfadeToSource == null)
        {
            return;
        }

        _crossfadeElapsed += deltaTime;
        float normalized = loopCrossfadeDuration <= 0f ? 1f : Mathf.Clamp01(_crossfadeElapsed / loopCrossfadeDuration);

        _crossfadeFromSource.volume = Mathf.Lerp(loopVolume, 0f, normalized);
        _crossfadeToSource.volume = Mathf.Lerp(0f, loopVolume, normalized);

        if (normalized < 1f)
        {
            return;
        }

        _crossfadeFromSource.Stop();
        _crossfadeFromSource.volume = 0f;

        AudioSource oldCurrent = _crossfadeFromSource;
        _currentLoopSource = _crossfadeToSource;
        _inactiveLoopSource = oldCurrent;

        _crossfadeFromSource = null;
        _crossfadeToSource = null;
        _crossfadeElapsed = 0f;
        _isCrossfading = false;
    }

    private AudioClip GetLoopClipForStage(int calmStage)
    {
        switch (Mathf.Clamp(calmStage, 0, 2))
        {
            case 0:
                return fastLoopClip;
            case 1:
                return mediumLoopClip;
            default:
                return calmLoopClip;
        }
    }

    private float GetPulseDurationForStage(int calmStage)
    {
        switch (Mathf.Clamp(calmStage, 0, 2))
        {
            case 0:
                return fastPulseDuration;
            case 1:
                return mediumPulseDuration;
            default:
                return calmPulseDuration;
        }
    }

    private void PlayCue(AudioClip clip, string cueName)
    {
        if (!playGestureCueClips)
        {
            return;
        }

        if (clip == null)
        {
            Debug.LogWarning($"{name}: Missing {cueName} cue clip.");
            return;
        }

        if (_cueAudioSource == null)
        {
            Debug.LogWarning($"{name}: Missing cue AudioSource.");
            return;
        }

        _cueAudioSource.PlayOneShot(clip, cueVolume);
    }

    private void ResetRuntimeState()
    {
        completedBreathCount = 0;
        currentPhase = BreathingPhase.WaitingForInhale;
        currentCalmStage = 0;
        inhalePoseActive = false;
        exhalePoseActive = false;

        _inhaleStableTime = 0f;
        _exhaleStableTime = 0f;
        _inhaleEntryLatched = false;
        _exhaleEntryLatched = false;

        _pulseClock = 0f;
        _finishElapsed = 0f;

        _isCrossfading = false;
        _crossfadeElapsed = 0f;
        _crossfadeFromSource = null;
        _crossfadeToSource = null;
    }

    private void PrepareCueSource()
    {
        if (_cueAudioSource == null)
        {
            return;
        }

        _cueAudioSource.Stop();
        _cueAudioSource.loop = false;
        _cueAudioSource.playOnAwake = false;
    }

    private void StopAllLoopAudio()
    {
        if (loopAudioSourceA != null)
        {
            loopAudioSourceA.Stop();
            loopAudioSourceA.volume = 0f;
        }

        if (loopAudioSourceB != null)
        {
            loopAudioSourceB.Stop();
            loopAudioSourceB.volume = 0f;
        }
    }

    private void EnsureRuntimeReferences()
    {
        AutoResolveSceneReferences();
        _cueAudioSource = cueAudioSourceOverride != null ? cueAudioSourceOverride : GetComponent<AudioSource>();
        TryResolveLoopSources();

        if (_vignettePropertyBlock == null)
        {
            _vignettePropertyBlock = new MaterialPropertyBlock();
        }
    }

    private void AutoResolveSceneReferences()
    {
        if (cueAudioSourceOverride == null)
        {
            cueAudioSourceOverride = GetComponent<AudioSource>();
        }

        if (leftInhalePoseState == null)
        {
            leftInhalePoseState = FindPoseRootState("BreatheOutPoseLeft");
        }

        if (rightInhalePoseState == null)
        {
            rightInhalePoseState = FindPoseRootState("BreatheOutPoseRight");
        }

        if (leftExhalePoseState == null)
        {
            leftExhalePoseState = FindPoseRootState("StopPoseLeft");
        }

        if (rightExhalePoseState == null)
        {
            rightExhalePoseState = FindPoseRootState("StopPoseRight");
        }

        if (vignetteRenderer == null)
        {
            GameObject vignetteObject = GameObject.Find("Breathing_Vignette");
            if (vignetteObject != null)
            {
                vignetteRenderer = vignetteObject.GetComponent<Renderer>();
            }
        }
    }

    private bool TryResolveLoopSources()
    {
        if (loopAudioSourceA != null && loopAudioSourceB != null)
        {
            PrepareLoopSource(loopAudioSourceA);
            PrepareLoopSource(loopAudioSourceB);
            _currentLoopSource = loopAudioSourceA;
            _inactiveLoopSource = loopAudioSourceB;
            return true;
        }

        AudioSource[] sources = GetComponents<AudioSource>();
        if (sources == null || sources.Length == 0)
        {
            return false;
        }

        List<AudioSource> candidates = new List<AudioSource>();
        for (int i = 0; i < sources.Length; i++)
        {
            if (sources[i] == null || sources[i] == cueAudioSourceOverride)
            {
                continue;
            }

            candidates.Add(sources[i]);
        }

        if (loopAudioSourceA == null && candidates.Count > 0)
        {
            loopAudioSourceA = candidates[0];
        }

        if (loopAudioSourceB == null && candidates.Count > 1)
        {
            loopAudioSourceB = candidates[1];
        }

        if (loopAudioSourceA == null)
        {
            return false;
        }

        _currentLoopSource = loopAudioSourceA;
        _inactiveLoopSource = loopAudioSourceB;

        PrepareLoopSource(loopAudioSourceA);
        PrepareLoopSource(loopAudioSourceB);
        return true;
    }

    private static void PrepareLoopSource(AudioSource source)
    {
        if (source == null)
        {
            return;
        }

        source.playOnAwake = false;
        source.loop = true;
        source.volume = 0f;
    }

    private void RebuildPoseSignals()
    {
        _leftInhaleSignal = BuildPoseSignal(leftInhalePoseState);
        _rightInhaleSignal = BuildPoseSignal(rightInhalePoseState);
        _leftExhaleSignal = BuildPoseSignal(leftExhalePoseState);
        _rightExhaleSignal = BuildPoseSignal(rightExhalePoseState);
    }

    private MonoBehaviour FindPoseRootState(string poseName)
    {
        Transform[] transforms = GetComponentsInChildren<Transform>(true);
        string normalizedPoseName = NormalizeName(poseName);

        for (int i = 0; i < transforms.Length; i++)
        {
            Transform candidate = transforms[i];
            if (NormalizeName(candidate.name) != normalizedPoseName)
            {
                continue;
            }

            MonoBehaviour[] components = candidate.GetComponents<MonoBehaviour>();
            for (int j = 0; j < components.Length; j++)
            {
                if (components[j] == null)
                {
                    continue;
                }

                if (HasReadableActiveFlag(components[j]))
                {
                    return components[j];
                }
            }
        }

        return null;
    }

    private static string NormalizeName(string value)
    {
        return string.IsNullOrEmpty(value) ? string.Empty : value.Replace(" ", string.Empty);
    }

    private static PoseSignal BuildPoseSignal(MonoBehaviour anchor)
    {
        if (anchor == null)
        {
            return PoseSignal.Empty;
        }

        Transform root = anchor.transform;
        MonoBehaviour[] components = root.GetComponentsInChildren<MonoBehaviour>(true);
        List<ActiveAccessor> recognizers = new List<ActiveAccessor>();

        for (int i = 0; i < components.Length; i++)
        {
            MonoBehaviour component = components[i];
            if (component == null)
            {
                continue;
            }

            string typeName = component.GetType().Name;
            if (typeName != "ShapeRecognizerActiveState" && typeName != "TransformRecognizerActiveState")
            {
                continue;
            }

            ActiveAccessor recognizer = ActiveAccessor.Create(component);
            if (recognizer != null)
            {
                recognizers.Add(recognizer);
            }
        }

        if (recognizers.Count > 0)
        {
            return new PoseSignal(recognizers, null);
        }

        ActiveAccessor fallback = ActiveAccessor.Create(anchor);
        return new PoseSignal(new List<ActiveAccessor>(), fallback);
    }

    private static bool HasReadableActiveFlag(MonoBehaviour component)
    {
        return ActiveAccessor.Create(component) != null;
    }

    private void ApplyHiddenVignette()
    {
        ApplyVignette(exhaleRadius, 0f);
    }

    private void ApplyVignette(float radius, float opacity)
    {
        if (vignetteRenderer == null)
        {
            if (!_warnedMissingVignette)
            {
                Debug.LogWarning($"{name}: Breathing vignette renderer is not assigned. Vignette animation is disabled.");
                _warnedMissingVignette = true;
            }

            return;
        }

        if (_vignettePropertyBlock == null)
        {
            _vignettePropertyBlock = new MaterialPropertyBlock();
        }

        vignetteRenderer.GetPropertyBlock(_vignettePropertyBlock);
        _vignettePropertyBlock.SetFloat(InnerRadiusPropertyId, radius);
        _vignettePropertyBlock.SetFloat(OpacityPropertyId, opacity);
        vignetteRenderer.SetPropertyBlock(_vignettePropertyBlock);
    }

    private sealed class PoseSignal
    {
        public static readonly PoseSignal Empty = new PoseSignal(new List<ActiveAccessor>(), null);

        private readonly List<ActiveAccessor> _recognizers;
        private readonly ActiveAccessor _fallback;

        public PoseSignal(List<ActiveAccessor> recognizers, ActiveAccessor fallback)
        {
            _recognizers = recognizers;
            _fallback = fallback;
        }

        public bool IsActive()
        {
            if (_recognizers.Count > 0)
            {
                for (int i = 0; i < _recognizers.Count; i++)
                {
                    if (!_recognizers[i].TryRead(out bool active) || !active)
                    {
                        return false;
                    }
                }

                return true;
            }

            if (_fallback != null && _fallback.TryRead(out bool fallbackActive))
            {
                return fallbackActive;
            }

            return false;
        }
    }

    private sealed class ActiveAccessor
    {
        private readonly MonoBehaviour _component;
        private readonly PropertyInfo _propertyInfo;
        private readonly FieldInfo _fieldInfo;

        private ActiveAccessor(MonoBehaviour component, PropertyInfo propertyInfo, FieldInfo fieldInfo)
        {
            _component = component;
            _propertyInfo = propertyInfo;
            _fieldInfo = fieldInfo;
        }

        public static ActiveAccessor Create(MonoBehaviour component)
        {
            if (component == null)
            {
                return null;
            }

            Type type = component.GetType();
            PropertyInfo propertyInfo = type.GetProperty("Active", BindingFlags.Instance | BindingFlags.Public);
            if (propertyInfo != null && propertyInfo.PropertyType == typeof(bool))
            {
                return new ActiveAccessor(component, propertyInfo, null);
            }

            FieldInfo fieldInfo = type.GetField("Active", BindingFlags.Instance | BindingFlags.Public);
            if (fieldInfo != null && fieldInfo.FieldType == typeof(bool))
            {
                return new ActiveAccessor(component, null, fieldInfo);
            }

            return null;
        }

        public bool TryRead(out bool value)
        {
            value = false;

            if (_component == null)
            {
                return false;
            }

            if (_propertyInfo != null)
            {
                object propertyValue = _propertyInfo.GetValue(_component, null);
                if (propertyValue is bool boolValue)
                {
                    value = boolValue;
                    return true;
                }

                return false;
            }

            if (_fieldInfo != null)
            {
                object fieldValue = _fieldInfo.GetValue(_component);
                if (fieldValue is bool boolValue)
                {
                    value = boolValue;
                    return true;
                }
            }

            return false;
        }
    }
}

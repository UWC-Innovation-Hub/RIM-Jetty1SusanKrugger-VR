using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

/// Select-by-touch: a hand/controller collider entering this object's trigger
/// collider selects it. The trigger collider must be on the same GameObject as this script.
public class ObjectHighlightTrigger : MonoBehaviour
{
    public event Func<ObjectHighlightTrigger, bool> SelectionRequested;

    [Header("Wiring")]
    [Tooltip("Must be a trigger collider on this GameObject. Enabled only while armed.")]
    [SerializeField] private Collider triggerZone;

    [Header("Detection")]
    [Tooltip("Only colliders with this tag count. Empty = any collider.")]
    [SerializeField] private string requiredTag = "";
    [Tooltip("How long a hand must stay inside before it counts. 0 = instant.")]
    [SerializeField] private float dwellSeconds = 0f;

    [Header("Highlight")]
    [SerializeField] private Renderer targetRenderer;
    [SerializeField] private int materialIndex = 0;
    [SerializeField] private string emissionProperty = "_EmissionStrength";
    [SerializeField] private float highlightValue = 1f;
    [SerializeField] private float idleValue = 0f;
    [SerializeField] private float fadeDuration = 0.3f;
    [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Response Playback")]
    [SerializeField] private AudioSource responseAudio;
    [SerializeField] private VideoPlayer responseVideo;

    private readonly HashSet<Collider> _collidersInside = new HashSet<Collider>();

    private MaterialPropertyBlock _mpb;
    private Coroutine _fadeRoutine;
    private Coroutine _dwellRoutine;
    private bool _armed;
    private bool _isComplete;

    public bool IsArmed => _armed;
    public bool IsComplete => _isComplete;
    public AudioSource ResponseAudio => responseAudio;
    public VideoPlayer ResponseVideo => responseVideo;

    private void Awake()
    {
        _mpb = new MaterialPropertyBlock();

        if (targetRenderer == null) targetRenderer = GetComponent<Renderer>();
        if (responseAudio == null) responseAudio = GetComponent<AudioSource>();
        if (triggerZone == null) triggerZone = GetComponent<Collider>();

        if (triggerZone == null)
        {
            Debug.LogWarning($"{name}: ObjectHighlightTrigger needs a trigger collider.", this);
        }
        else if (!triggerZone.isTrigger)
        {
            Debug.LogWarning($"{name}: triggerZone is not set to Is Trigger.", this);
        }

        SetArmed(false);
        SetHighlighted(false, true);
    }

    private void OnDisable()
    {
        ClearColliders();

        if (_fadeRoutine != null)
        {
            StopCoroutine(_fadeRoutine);
            _fadeRoutine = null;
        }
    }

    // ---- State ----

    public void SetArmed(bool armed)
    {
        _armed = armed;

        if (!armed)
        {
            ClearColliders();
        }

        // Toggling the collider means a hand already inside fires Enter when re-armed.
        if (triggerZone != null)
        {
            triggerZone.enabled = armed;
        }
    }

    public void MarkComplete()
    {
        _isComplete = true;
        SetArmed(false);
    }

    public void ResetTrigger()
    {
        _isComplete = false;
        SetArmed(false);
    }

    public void SetHighlighted(bool highlighted, bool instant = false)
    {
        float target = highlighted ? highlightValue : idleValue;

        if (instant)
        {
            ApplyEmission(target);
            return;
        }

        StartFade(target);
    }

    // ---- Detection ----

    private void OnTriggerEnter(Collider other)
    {
        if (!_armed || _isComplete)
        {
            return;
        }

        if (!string.IsNullOrEmpty(requiredTag) && !other.CompareTag(requiredTag))
        {
            return;
        }

        if (_collidersInside.Add(other) && _collidersInside.Count == 1 && _dwellRoutine == null)
        {
            _dwellRoutine = StartCoroutine(DwellRoutine());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        _collidersInside.Remove(other);

        if (_collidersInside.Count == 0)
        {
            CancelDwell();
        }
    }

    private IEnumerator DwellRoutine()
    {
        if (dwellSeconds > 0f)
        {
            yield return new WaitForSeconds(dwellSeconds);
        }

        _dwellRoutine = null;

        // Hand tracking can drop and disable colliders without an Exit firing.
        _collidersInside.RemoveWhere(c => c == null || !c.enabled || !c.gameObject.activeInHierarchy);

        if (_armed && !_isComplete && _collidersInside.Count > 0)
        {
            RequestSelection();
        }
    }

    private void RequestSelection()
    {
        bool accepted = false;

        if (SelectionRequested != null)
        {
            Delegate[] handlers = SelectionRequested.GetInvocationList();
            for (int i = 0; i < handlers.Length; i++)
            {
                accepted |= ((Func<ObjectHighlightTrigger, bool>)handlers[i]).Invoke(this);
            }
        }

        if (accepted)
        {
            PlayResponse();
        }
    }

    private void CancelDwell()
    {
        if (_dwellRoutine != null)
        {
            StopCoroutine(_dwellRoutine);
            _dwellRoutine = null;
        }
    }

    private void ClearColliders()
    {
        _collidersInside.Clear();
        CancelDwell();
    }

    // ---- Response ----

    public void HideResponseVideo()
    {
        if (responseVideo != null)
        {
            responseVideo.Stop();
            responseVideo.gameObject.SetActive(false);
        }
    }

    private void PlayResponse()
    {
        if (responseVideo != null)
        {
            responseVideo.gameObject.SetActive(true);
            responseVideo.Play();
        }

        if (responseAudio != null && responseAudio.clip != null)
        {
            responseAudio.Play();
        }
    }

    // ---- Highlight ----

    private void StartFade(float target)
    {
        if (targetRenderer == null)
        {
            return;
        }

        if (_fadeRoutine != null)
        {
            StopCoroutine(_fadeRoutine);
        }

        _fadeRoutine = StartCoroutine(FadeRoutine(target));
    }

    private void ApplyEmission(float value)
    {
        if (targetRenderer == null)
        {
            return;
        }

        if (_fadeRoutine != null)
        {
            StopCoroutine(_fadeRoutine);
            _fadeRoutine = null;
        }

        targetRenderer.GetPropertyBlock(_mpb, materialIndex);
        _mpb.SetFloat(emissionProperty, value);
        targetRenderer.SetPropertyBlock(_mpb, materialIndex);
    }

    private IEnumerator FadeRoutine(float target)
    {
        targetRenderer.GetPropertyBlock(_mpb, materialIndex);
        float start = _mpb.GetFloat(emissionProperty);

        if (fadeDuration > 0f)
        {
            float elapsed = 0f;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = fadeCurve.Evaluate(Mathf.Clamp01(elapsed / fadeDuration));

                targetRenderer.GetPropertyBlock(_mpb, materialIndex);
                _mpb.SetFloat(emissionProperty, Mathf.Lerp(start, target, t));
                targetRenderer.SetPropertyBlock(_mpb, materialIndex);

                yield return null;
            }
        }

        targetRenderer.GetPropertyBlock(_mpb, materialIndex);
        _mpb.SetFloat(emissionProperty, target);
        targetRenderer.SetPropertyBlock(_mpb, materialIndex);
        _fadeRoutine = null;
    }
}
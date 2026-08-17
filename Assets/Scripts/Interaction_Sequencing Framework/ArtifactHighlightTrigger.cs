using System;
using System.Collections;
using Oculus.Interaction;
using UnityEngine;
using UnityEngine.Video;

public class ArtifactHighlightTrigger : MonoBehaviour
{
    public event Func<ArtifactHighlightTrigger, bool> SelectionRequested;

    [Header("Wiring")]
    [SerializeField] private Grabbable grabbable;
    [SerializeField] private MonoBehaviour interactable;

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

    private MaterialPropertyBlock _mpb;
    private Coroutine _fadeRoutine;
    private bool _armed;
    private bool _isComplete;

    public bool IsArmed => _armed;
    public bool isComplete => _isComplete;
    public AudioSource ResponseAudio => responseAudio;
    public VideoPlayer ResponseVideo => responseVideo;


    private void Awake()
    {
        _mpb = new MaterialPropertyBlock();

        if (targetRenderer == null)
        {
            targetRenderer = GetComponent<Renderer>();
        }

        if (responseAudio == null)
        {
            responseAudio = GetComponent<AudioSource>();
        }

        if (grabbable == null)
        {
            grabbable = GetComponent<Grabbable>();
        }

        if (grabbable == null)
        {
            Debug.LogWarning($"{name}: InteractableHighlightTrigger needs a Grabbable reference.");
        }
    }

    private void OnEnable()
    {
        if (grabbable != null)
        {
            grabbable.WhenPointerEventRaised += HandlePointerEvent;
        }
    }

    private void OnDisable()
    {
        if (grabbable != null)
        {
            grabbable.WhenPointerEventRaised -= HandlePointerEvent;
        }

        if (_fadeRoutine != null)
        {
            StopCoroutine(_fadeRoutine);
            _fadeRoutine = null;
        }
    }

    public void SetArmed(bool armed)
    {
        _armed = armed;

        if (interactable != null)
        {
            interactable.enabled = armed;
        }

        StartFade(armed ? highlightValue : idleValue);
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

    private void HandlePointerEvent(PointerEvent evt)
    {
        if (evt.Type != PointerEventType.Select)
        {
            return;
        }

        if (!_armed || _isComplete)
        {
            return;
        }

        bool accepted = false;

        if (SelectionRequested != null)
        {
            Delegate[] handlers = SelectionRequested.GetInvocationList();
            for (int i = 0; i < handlers.Length; i++)
            {
                accepted |= ((Func<ArtifactHighlightTrigger, bool>)handlers[i]).Invoke(this);
            }
        }

        if (accepted)
        {
            PlayResponse();
        }
    }

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

    private IEnumerator FadeRoutine(float target)
    {
        targetRenderer.GetPropertyBlock(_mpb, materialIndex);
        float start = _mpb.GetFloat(emissionProperty);

        if (fadeDuration <= 0f)
        {
            _mpb.SetFloat(emissionProperty, target);
            targetRenderer.SetPropertyBlock(_mpb, materialIndex);
            _fadeRoutine = null;
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = fadeCurve.Evaluate(Mathf.Clamp01(elapsed / fadeDuration));
            float value = Mathf.Lerp(start, target, t);

            targetRenderer.GetPropertyBlock(_mpb, materialIndex);
            _mpb.SetFloat(emissionProperty, value);
            targetRenderer.SetPropertyBlock(_mpb, materialIndex);

            yield return null;
        }

        targetRenderer.GetPropertyBlock(_mpb, materialIndex);
        _mpb.SetFloat(emissionProperty, target);
        targetRenderer.SetPropertyBlock(_mpb, materialIndex);
        _fadeRoutine = null;
    }
}

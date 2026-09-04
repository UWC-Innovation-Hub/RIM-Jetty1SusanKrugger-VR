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

    [SerializeField] private Collider targetCollider;

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

    [Header("Return To Origin")]
    [SerializeField] private float returnDelaySeconds = 5f;
    [SerializeField] private float returnDuration = 0.5f;
    [SerializeField] private AnimationCurve returnCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    private Vector3 _originalPosition;
    private Quaternion _originalRotation;
    private Rigidbody _rigidbody;
    private Coroutine _returnRoutine;

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

        if (targetCollider == null)
        {
            targetCollider = GetComponent<Collider>();
        }

        _originalPosition = transform.position;
        _originalRotation = transform.rotation;
        _rigidbody = GetComponent<Rigidbody>();

        SetArmed(false);
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

        CancelReturn();
    }

    public void SetArmed(bool armed)
    {
        _armed = armed;

        if (interactable != null)
        {
            interactable.enabled = armed;
        }

        if (targetCollider != null)
        {
            targetCollider.enabled = armed;
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
        if (evt.Type == PointerEventType.Unselect)
        {
            CancelReturn();
            _returnRoutine = StartCoroutine(ReturnAfterDelayRoutine());
            return;
        }

        if (evt.Type != PointerEventType.Select)
        {
            return;
        }

        CancelReturn();

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

    private void CancelReturn()
    {
        if (_returnRoutine != null)
        {
            StopCoroutine(_returnRoutine);
            _returnRoutine = null;
        }
    }

    private IEnumerator ReturnAfterDelayRoutine()
    {
        yield return new WaitForSeconds(returnDelaySeconds);
        yield return ReturnToOrigin();
        _returnRoutine = null;
    }

    private IEnumerator ReturnToOrigin()
    {
        bool hasRigidbody = _rigidbody != null;
        bool wasKinematic = hasRigidbody && _rigidbody.isKinematic;

        if (hasRigidbody)
        {
            _rigidbody.linearVelocity = Vector3.zero;
            _rigidbody.angularVelocity = Vector3.zero;
            _rigidbody.isKinematic = true;
        }

        if (returnDuration <= 0f)
        {
            transform.SetPositionAndRotation(_originalPosition, _originalRotation);
        }
        else
        {
            Vector3 startPosition = transform.position;
            Quaternion startRotation = transform.rotation;

            float elapsed = 0f;

            while (elapsed < returnDuration)
            {
                elapsed += Time.deltaTime;
                float t = returnCurve.Evaluate(Mathf.Clamp01(elapsed / returnDuration));
                transform.SetPositionAndRotation(Vector3.Lerp(startPosition, _originalPosition, t), Quaternion.Slerp(startRotation, _originalRotation, t));
                yield return null;
            }

            transform.SetPositionAndRotation(_originalPosition, _originalRotation);
        }

        if (hasRigidbody)
        {
            _rigidbody.isKinematic = wasKinematic;
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

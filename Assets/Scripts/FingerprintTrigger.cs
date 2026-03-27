using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class FingerprintTrigger : MonoBehaviour
{
    public event Func<FingerprintTrigger, bool> SelectionRequested;

    [Header("Wiring")]
    [SerializeField] private ProjectorController projector;
    public ProjectorController proj;
    [SerializeField] private VideoClip clip;

    [Header("Touch Filtering (optional)")]
    [SerializeField] private string requiredTag = ""; // e.g. "Hand" or "FingerTip"
    [SerializeField] private bool disableColliderWhenLocked = true;

    [SerializeField] private Material FingerGlowMat;

    [Header("Info UI")]
    [SerializeField] private Image infoPanel;
    [SerializeField] private TMP_Text infoText;
    [SerializeField] private float infoFadeInDuration = 0.3f;
    [SerializeField] private float infoFadeOutDuration = 0.3f;

    private Collider _col;
    private MeshRenderer _mesh;
    [SerializeField] private Material _meshMat;
    [SerializeField] private MeshRenderer _Glowmesh;
    private bool _armed = true;

    private AudioSource AS;
    private Coroutine _infoFadeRoutine;
    private Color _infoPanelBaseColor;
    private Color _infoTextBaseColor;
    private float _infoPanelVisibleAlpha;
    private float _infoTextVisibleAlpha;
    private bool _hasInfoUI;
    private bool _infoWarningLogged;

    public VideoClip Clip => clip;
    public bool IsArmed => _armed;
    public AudioSource ResponseAudio => AS;

    private void Awake()
    {
        _col = GetComponent<Collider>();
        _mesh = GetComponent<MeshRenderer>();
        if (!_col) Debug.LogWarning($"{name}: FingerprintTrigger needs a Collider.");
        AS = GetComponent<AudioSource>();

        ResolveInfoUIReferences();
        CacheInfoUIState();
        HideInfoInstant();
    }

    public void SetArmed(bool armed)
    {
        _armed = armed;
        if (_col && disableColliderWhenLocked) 
        {
            _col.enabled = armed;
            //_mesh.enabled = armed;
            _Glowmesh.enabled = armed;

            ////FADE SOLUTION
            ////Change emissive property on armed. (Glow when FP needs selecting, turn off once selected)
            //if (armed)
            //{
            //    FingerGlowMat.SetFloat("_EmissionStrength", 0.2f);
            //}
            //else
            //{
                //    FingerGlowMat.SetFloat("_EmissionStrength", 0f);
                //}
        }        

        if (armed)
        {
            HideInfoInstant();
        }
    }

    public void SetVisible(bool visible)
    {
        if (!visible)
        {
            HideInfoInstant();
        }

        if (gameObject.activeSelf == visible)
        {
            return;
        }

        gameObject.SetActive(visible);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!_armed) return;

        if (!string.IsNullOrEmpty(requiredTag) && !other.CompareTag(requiredTag))
            return;

        bool handledByModule = false;
        bool accepted = false;

        if (SelectionRequested != null)
        {
            Delegate[] handlers = SelectionRequested.GetInvocationList();
            for (int i = 0; i < handlers.Length; i++)
            {
                handledByModule = true;
                accepted |= ((Func<FingerprintTrigger, bool>)handlers[i]).Invoke(this);
            }
        }

        if (!handledByModule)
        {
            if (clip == null || projector == null) return;
            accepted = projector.TryPlay(clip);
        }

        if (accepted)
        {
            AS?.Play();
            BeginInfoFadeIn();
        }
    }

    public void BeginInfoFadeIn()
    {
        if (!_hasInfoUI)
        {
            return;
        }

        StartInfoFade(_infoPanelVisibleAlpha, _infoTextVisibleAlpha, infoFadeInDuration);
    }

    public IEnumerator FadeOutInfoRoutine()
    {
        if (!_hasInfoUI)
        {
            yield break;
        }

        StopInfoFadeRoutine();
        yield return FadeInfoRoutine(GetCurrentPanelAlpha(), 0f, GetCurrentTextAlpha(), 0f, infoFadeOutDuration);
        _infoFadeRoutine = null;
    }

    public void HideInfoInstant()
    {
        StopInfoFadeRoutine();
        SetInfoAlpha(0f, 0f);
    }

    private void OnDisable()
    {
        HideInfoInstant();

        if (_col != null && disableColliderWhenLocked)
        {
            _col.enabled = false;
        }
    }


    private void ResolveInfoUIReferences()
    {
        if (infoPanel != null && infoText != null)
        {
            return;
        }

        Transform infoCanvasTransform = transform.Find("InfoCanvas");
        if (infoCanvasTransform == null)
        {
            return;
        }

        if (infoPanel == null)
        {
            Transform panelTransform = infoCanvasTransform.Find("Panel");
            if (panelTransform != null)
            {
                infoPanel = panelTransform.GetComponent<Image>();
            }
        }

        if (infoText == null)
        {
            if (infoPanel != null)
            {
                infoText = infoPanel.GetComponentInChildren<TMP_Text>(true);
            }

            if (infoText == null)
            {
                infoText = infoCanvasTransform.GetComponentInChildren<TMP_Text>(true);
            }
        }
    }

    private void CacheInfoUIState()
    {
        _hasInfoUI = infoPanel != null && infoText != null;
        if (!_hasInfoUI)
        {
            if (!_infoWarningLogged && (infoPanel != null || infoText != null || transform.Find("InfoCanvas") != null))
            {
                Debug.LogWarning($"{name}: FingerprintTrigger could not fully resolve InfoCanvas panel/text references.");
                _infoWarningLogged = true;
            }

            return;
        }

        _infoPanelBaseColor = infoPanel.color;
        _infoTextBaseColor = infoText.color;
        _infoPanelVisibleAlpha = _infoPanelBaseColor.a;
        _infoTextVisibleAlpha = _infoTextBaseColor.a;
    }

    private void StartInfoFade(float targetPanelAlpha, float targetTextAlpha, float duration)
    {
        if (!_hasInfoUI)
        {
            return;
        }

        StopInfoFadeRoutine();
        _infoFadeRoutine = StartCoroutine(FadeInfoRoutine(
            GetCurrentPanelAlpha(),
            targetPanelAlpha,
            GetCurrentTextAlpha(),
            targetTextAlpha,
            duration));
    }

    private IEnumerator FadeInfoRoutine(
        float fromPanelAlpha,
        float toPanelAlpha,
        float fromTextAlpha,
        float toTextAlpha,
        float duration)
    {
        if (!_hasInfoUI)
        {
            yield break;
        }

        if (duration <= 0f)
        {
            SetInfoAlpha(toPanelAlpha, toTextAlpha);
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            SetInfoAlpha(
                Mathf.Lerp(fromPanelAlpha, toPanelAlpha, t),
                Mathf.Lerp(fromTextAlpha, toTextAlpha, t));
            yield return null;
        }

        SetInfoAlpha(toPanelAlpha, toTextAlpha);
    }

    private void SetInfoAlpha(float panelAlpha, float textAlpha)
    {
        if (infoPanel != null)
        {
            Color panelColor = _infoPanelBaseColor;
            panelColor.a = panelAlpha;
            infoPanel.color = panelColor;
        }

        if (infoText != null)
        {
            Color textColor = _infoTextBaseColor;
            textColor.a = textAlpha;
            infoText.color = textColor;
        }
    }

    private float GetCurrentPanelAlpha()
    {
        return infoPanel != null ? infoPanel.color.a : 0f;
    }

    private float GetCurrentTextAlpha()
    {
        return infoText != null ? infoText.color.a : 0f;
    }

    private void StopInfoFadeRoutine()
    {
        if (_infoFadeRoutine != null)
        {
            StopCoroutine(_infoFadeRoutine);
            _infoFadeRoutine = null;
        }
    }


}

using System.Collections;
using UnityEngine;

public class CharacterFader : MonoBehaviour
{
    [Header("Targets")]
    [SerializeField] private Renderer[] renderers;

    [Header("Fade")]
    [SerializeField] private string colorProperty = "_BaseColor";
    [SerializeField] private float visibleAlpha = 1f;
    [SerializeField] private float hiddenAlpha = 0f;
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Behaviour")]
    [SerializeField] private bool deactivateWhenHidden = true;

    private struct Slot
    {
        public Renderer renderer;
        public int index;
        public Color baseColor;
    }

    private Slot[] _slots;
    private MaterialPropertyBlock _mpb;
    private int _colorId;
    private Coroutine _routine;
    private float _currentAlpha;
    private bool _initialized;

    public float FadeDuration => fadeDuration;

    private void Awake()
    {
        Initialize();
    }

    private void Initialize()
    {
        if (_initialized)
        {
            return;
        }

        _initialized = true;

        _mpb = new MaterialPropertyBlock();
        _colorId = Shader.PropertyToID(colorProperty);
        _currentAlpha = visibleAlpha;

        if (renderers == null || renderers.Length == 0)
        {
            renderers = GetComponentsInChildren<Renderer>(true);
        }

        var slots = new System.Collections.Generic.List<Slot>();
        
        for (int r = 0; r < renderers.Length; r++)
        {
            Renderer rend = renderers[r];

            if (rend == null)
            {
                continue;
            }

            Material[] mats = rend.sharedMaterials;
            for (int m = 0; m < mats.Length; m++)
            {
                if (mats[m] == null || !mats[m].HasProperty(_colorId)) continue;

                slots.Add(new Slot { renderer = rend, index = m, baseColor = mats[m].GetColor(_colorId) });
            }
        }

        _slots = slots.ToArray();

        if (_slots.Length == 0)
        {
            Debug.LogWarning($"{name}: no materials with '{colorProperty}' found, fade will have no visible effect.", this);
        }
    }

    public void FadeOut()
    {
        StartFade(hiddenAlpha, deactivateWhenHidden);
    }

    public void FadeIn()
    {
        gameObject.SetActive(true);
        Initialize();
        StartFade(visibleAlpha, false);
    }

    public void SetVisibleInstant()
    {
        gameObject.SetActive(true);
        Initialize();

        if (_routine != null)
        {
            StopCoroutine(_routine);
            _routine = null;
        }

        ApplyAlpha(visibleAlpha);
    }

    private void StartFade(float target, bool deactivateAtEnd)
    {
        Initialize();

        if (_routine != null)
        {
            StopCoroutine(_routine);
        }

        _routine = StartCoroutine(FadeRoutine(target, deactivateAtEnd));
    }

    private IEnumerator FadeRoutine(float target, bool deactivateAtEnd)
    {
        float start = _currentAlpha;

        if (fadeDuration > 0f)
        {
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = fadeCurve.Evaluate(Mathf.Clamp01(elapsed / fadeDuration));
                ApplyAlpha(Mathf.Lerp(start, target, t));
                yield return null;
            }
        }

        ApplyAlpha(target);
        _routine = null;

        if (deactivateAtEnd)
        {
            gameObject.SetActive(false);
        }
    }

    private void ApplyAlpha(float alpha)
    {
        _currentAlpha = alpha;

        for (int i = 0; i < _slots.Length; i++)
        {
            Slot slot = _slots[i];
            if (slot.renderer == null)
            {
                continue;
            }

            Color c = slot.baseColor;
            c.a = alpha;

            slot.renderer.GetPropertyBlock(_mpb, slot.index);
            _mpb.SetColor(_colorId, c);
            slot.renderer.SetPropertyBlock(_mpb, slot.index);
        }
    }
}

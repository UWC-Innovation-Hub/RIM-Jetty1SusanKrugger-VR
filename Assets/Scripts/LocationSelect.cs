using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class LocationSelect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Scene To Load")]
    public string sceneName;

    [Header("Map Marker")]
    public Renderer markerRenderer;
    public Color glowColor = Color.cyan;

    [Header("Glow Settings")]
    public float glowIntensity = 3f;
    public float glowSpeed = 5f;
    public float pulseSpeed = 2f;
    public float pulseAmount = 0.3f;

    private Material markerMaterial;

    private float currentGlow = 0f;
    private float targetGlow = 0f;
    private bool isHovered = false;

    private void Start()
    {
        if (markerRenderer != null)
        {
            markerMaterial = markerRenderer.material;
            markerMaterial.EnableKeyword("_EMISSION");
            markerMaterial.SetColor("_EmissionColor", Color.black);
        }

        GetComponent<Button>().onClick.AddListener(LoadScene);
    }

    private void Update()
    {
        if (markerMaterial == null)
        {
            return;
        }

        currentGlow = Mathf.Lerp(currentGlow, targetGlow, Time.deltaTime * glowSpeed);

        float finalIntensity = currentGlow;

        if (isHovered)
        {
            float pulse = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;
            finalIntensity += pulse;

            float pulseStrength = pulse * pulseAmount * glowIntensity;

            finalIntensity = currentGlow + pulseStrength;
        }


        markerMaterial.SetColor("_EmissionColor", glowColor * finalIntensity);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
        targetGlow = glowIntensity;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        targetGlow = 0f;
    }

    private void LoadScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }
}

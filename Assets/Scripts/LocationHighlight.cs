using UnityEngine;
using UnityEngine.SceneManagement;

public class LocationHighlight : MonoBehaviour
{
    [Header("Scene")]
    public string sceneToLoad;

    [Header("Hatch Overlay")]
    public Renderer hatchRenderer;

    [Header("Fade Settings")]
    public float fadeSpeed = 5f;

    [Header("Pointer")]
    public Transform pointer;

    [Header("Ray Settings")]
    public float rayDistance = 10f;
    public LayerMask interactLayer;

    private float currentAlpha = 0f;
    private float targetAlpha = 0f;

    private bool isHovered = false;


    void Start()
    {
        hatchRenderer.material = new Material(hatchRenderer.material);
    }

    void Update()
    {
        HandleRaycast();

        currentAlpha = Mathf.Lerp(currentAlpha, targetAlpha, Time.deltaTime * fadeSpeed);
        ApplyAlpha();

        if (isHovered && IsSelectPressed())
        {
            LoadScene();
        }
    }

    void HandleRaycast()
    {
        Ray ray = new Ray(pointer.position, pointer.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, rayDistance, interactLayer))
        {
            if (hit.collider.gameObject == gameObject)
            {
                if (!isHovered)
                {
                    isHovered = true;
                    targetAlpha = 1f;
                }
                return;
            }
        }

        if (isHovered)
        {
            isHovered = false;
            targetAlpha = 0f;
        }
    }

    bool IsSelectPressed()
    {
        return OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger);
    }

    void LoadScene()
    {
        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneToLoad);
        }
        else
        {
            Debug.LogWarning("No scene assigned on " + gameObject.name);
        }
    }

    void ApplyAlpha()
    {
        if (hatchRenderer  != null)
        {
            Color color = hatchRenderer.material.color;
            color.a = currentAlpha;
            hatchRenderer.material.color = color;
        }
    }
}

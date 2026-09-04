using UnityEngine;

[DisallowMultipleComponent]
public class ConversationRaycast : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] private Camera gazeCamera;

    [Header("Raycast Settings")]
    [SerializeField] private float maxDistance = 10f;
    [SerializeField] private LayerMask characterLayers;

    private IConversationGazeTarget _currentTarget;

    private void Reset()
    {
        gazeCamera = Camera.main;
    }

    private void Awake()
    {
        if (gazeCamera == null)
        {
            gazeCamera = Camera.main;
        }

        if (gazeCamera == null)
        {
            Debug.LogError("[ConversationRaycast] No camera assigned");
        }
    }

    private void Update()
    {
        ShowRaycast();
    }

    private void ShowRaycast()
    {
        if (gazeCamera == null)
        {
            return;
        }

        Ray ray = new Ray(gazeCamera.transform.position, gazeCamera.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, characterLayers))
        {
            Debug.Log($"[ConversationRaycast] Raycast hit collider '{hit.collider.name}' on layer {LayerMask.LayerToName(hit.collider.gameObject.layer)}");

            IConversationGazeTarget target = hit.collider.GetComponentInParent<IConversationGazeTarget>();

            if (target != null)
            {
                SetTarget(target);
                return;
            }
        }

        SetTarget(null);
    }

    private void SetTarget(IConversationGazeTarget newTarget)
    {

        if (newTarget == _currentTarget)
        {
            return;
        }

        if (newTarget != null)
        {
            Debug.Log($"[ConversationRaycast] Now gazing at '{((MonoBehaviour)newTarget).name}'");
        }

        _currentTarget?.OnGazeExit();
        _currentTarget = newTarget;
        _currentTarget?.OnGazeEnter();
    }
}

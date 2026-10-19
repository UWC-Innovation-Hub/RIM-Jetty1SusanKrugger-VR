using UnityEngine;

public class CameraLayerSwitcher : MonoBehaviour
{
    [Header("Wiring")]
    [SerializeField] private Camera[] cameras;

    [Header("Layers")]
    [SerializeField] private LayerMask layersToSwitch;

    private void Awake()
    {
        if (cameras == null || cameras.Length == 0)
        {
            cameras = Camera.main != null ? new[] { Camera.main } : new Camera[0];
        }
    }

    public void Hide()
    {
        for (int i = 0; i < cameras.Length; i++)
        {
            if (cameras[i] != null)
            {
                cameras[i].cullingMask &= ~layersToSwitch.value;
            }
        }
    }

    public void Show()
    {
        for (int i = 0; i < cameras.Length; i++)
        {
            if (cameras[i] != null)
            {
                cameras[i].cullingMask |= layersToSwitch.value;
            }
        }
    }
}

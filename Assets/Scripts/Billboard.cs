using UnityEngine;

[DisallowMultipleComponent]
public class Billboard : MonoBehaviour
{
    public enum BillboardMode { FullFace, YAxisOnly, SmoothYAxis }

    [SerializeField] private BillboardMode mode = BillboardMode.YAxisOnly;
    [SerializeField] private float smoothSpeed = 5f;
    [SerializeField] private Transform rotateTarget;
    [SerializeField] private Transform cameraOverride;

    private Transform _cameraTransform;

    private void Start()
    {
        ResolveCameraTransform();
    }

    private void LateUpdate()
    {
        Transform cameraTransform = ResolveCameraTransform();
        Transform targetTransform = rotateTarget != null ? rotateTarget : transform;

        if (cameraTransform == null || targetTransform == null)
        {
            return;
        }

        switch (mode)
        {
            case BillboardMode.FullFace:
                Vector3 fullFaceDirection = cameraTransform.position - targetTransform.position;
                if (fullFaceDirection.sqrMagnitude > Mathf.Epsilon)
                {
                    targetTransform.rotation = Quaternion.LookRotation(-fullFaceDirection.normalized, Vector3.up);
                }
                break;

            case BillboardMode.YAxisOnly:
                Vector3 dir = cameraTransform.position - targetTransform.position;
                dir.y = 0f;
                if (dir.sqrMagnitude > Mathf.Epsilon)
                {
                    targetTransform.rotation = Quaternion.LookRotation(-dir.normalized, Vector3.up);
                }
                break;

            case BillboardMode.SmoothYAxis:
                Vector3 smoothDir = cameraTransform.position - targetTransform.position;
                smoothDir.y = 0f;
                if (smoothDir.sqrMagnitude > Mathf.Epsilon)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(-smoothDir.normalized, Vector3.up);
                    targetTransform.rotation = Quaternion.Lerp(
                        targetTransform.rotation,
                        targetRotation,
                        Time.deltaTime * smoothSpeed);
                }
                break;
        }
    }

    private Transform ResolveCameraTransform()
    {
        if (cameraOverride != null)
        {
            return cameraOverride;
        }

        if (_cameraTransform == null)
        {
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                _cameraTransform = mainCamera.transform;
            }
        }

        return _cameraTransform;
    }
}

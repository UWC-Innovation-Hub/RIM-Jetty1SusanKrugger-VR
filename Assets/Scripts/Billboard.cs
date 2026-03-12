using UnityEngine;

public class Billboard : MonoBehaviour
{
   public enum BillboardMode { FullFace, YAxisOnly, SmoothYAxis }

    [SerializeField] private BillboardMode mode = BillboardMode.YAxisOnly;
    [SerializeField] private float smoothSpeed = 5f;

    private Transform cameraTransform;

    void Start()
    {
        cameraTransform = Camera.main.transform;
    }

    void LateUpdate()
    {
        switch (mode)
        {
            case BillboardMode.FullFace:
                transform.LookAt(cameraTransform);
                transform.Rotate(0, 180f, 0);
                break;

            case BillboardMode.YAxisOnly:
                Vector3 dir = cameraTransform.position - transform.position;
                dir.y = 0f;
                if (dir != Vector3.zero)
                {
                    transform.rotation = Quaternion.LookRotation(-dir);
                }
                break;

            case BillboardMode.SmoothYAxis:
                Vector3 smoothDir = cameraTransform.position - transform.position;
                smoothDir.y = 0f;
                if (smoothDir != Vector3.zero)
                {
                    Quaternion target = Quaternion.LookRotation(-smoothDir);
                    transform.rotation = Quaternion.Lerp(transform.rotation, target, Time.deltaTime * smoothSpeed);
                }
                break;
        }
    }
}

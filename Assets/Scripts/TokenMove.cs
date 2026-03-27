using UnityEngine;

public class TokenMove : MonoBehaviour
{
    [Header("References")]
    public Transform coin;
    public Transform vrCamera;
    private Animator animator;

    [Header("Movement Settings")]
    public float distanceFromCamera = 1.5f;
    public float moveSmoothTime = 0.2f;
    public float stopDistance = 0.03f;

    [Header("Final Local Offset")]
    public Vector3 finalLocalOffset = new Vector3(0.3f, -0.3f, 1.5f);

    [Header("Final Rotation")]
    public Vector3 finalRotationEuler = new Vector3(0f, 0f, 0f);

    private Vector3 targetWorldPosition;
    private Vector3 velocity = Vector3.zero;

    private bool isMoving = false;
    private bool attachAfterMove = false;

    void Start()
    {
        animator = coin.GetComponent<Animator>();

        Camera cam = vrCamera.GetComponent<Camera>();

        targetWorldPosition = cam.ViewportToWorldPoint(new Vector3(0.8f, -0.2f, distanceFromCamera));
    }

    void Update()
    {
        if (isMoving)
        {
            coin.position = Vector3.SmoothDamp(coin.position, targetWorldPosition, ref velocity, moveSmoothTime);

            coin.LookAt(vrCamera);

            if (Vector3.Distance(coin.position, targetWorldPosition) < stopDistance)
            {
                coin.position = targetWorldPosition;
                isMoving = false;

                StopSpin();
                attachAfterMove = true;
            }
        }

        if (attachAfterMove)
        {
            AttachToCamera();
            attachAfterMove = false;
        }
    }

    public void StartMove()
    {
        isMoving = true;
    }

    void StopSpin()
    {
        if (animator != null)
        {
            animator.enabled = false;
        }
    }

    void AttachToCamera()
    {
        coin.SetParent(vrCamera);

        coin.localPosition = finalLocalOffset;
        coin.localRotation = Quaternion.Euler(finalRotationEuler);

        coin.localScale *= 1f;
    }
}

using UnityEngine;

public class MatchTransformPosition : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private Transform referenceTransform;

    [Header("Axis Controls")]
    [SerializeField] private bool matchX = true;
    [SerializeField] private bool matchY = true;
    [SerializeField] private bool matchZ = true;

    private void LateUpdate()
    {
        if (referenceTransform == null)
            return;

        Vector3 newPosition = transform.position;

        if (matchX)
            newPosition.x = referenceTransform.position.x;

        if (matchY)
            newPosition.y = referenceTransform.position.y;

        if (matchZ)
            newPosition.z = referenceTransform.position.z;

        transform.position = newPosition;
    }
}
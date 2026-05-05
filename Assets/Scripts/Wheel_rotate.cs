using System.Collections.Generic;
using UnityEngine;

public class Wheel_rotate: MonoBehaviour
{
    [Header("Movement Source")]
    public Transform target; // Object whose movement drives the wheels

    [Header("Wheel Setup")]
    public List<Transform> wheels = new List<Transform>();

    [Header("Radius Setup (defines circumference)")]
    public Transform radiusPointA;
    public Transform radiusPointB;

    [Header("Rotation Settings")]
    public Vector3 rotationAxis = Vector3.right; // Local axis of rotation
    public float direction = 1f; // 1 or -1
    public float speedMultiplier = 1f;

    private Vector3 lastPosition;
    private float wheelCircumference;

    void Start()
    {
        if (target == null)
            target = transform;

        lastPosition = target.position;

        CalculateCircumference();
    }

    void CalculateCircumference()
    {
        if (radiusPointA != null && radiusPointB != null)
        {
            float radius = Vector3.Distance(radiusPointA.position, radiusPointB.position);
            wheelCircumference = 2f * Mathf.PI * radius;
        }
        else
        {
            Debug.LogWarning("Radius points not assigned.");
            wheelCircumference = 1f;
        }
    }

    void Update()
    {
        Vector3 currentPosition = target.position;
        Vector3 movement = currentPosition - lastPosition;

        // Project movement onto forward direction (so reversing works properly)
        float distanceMoved = Vector3.Dot(movement, target.forward);

        // Calculate rotation in degrees
        float rotationAmount = (distanceMoved / wheelCircumference) * 360f;
        rotationAmount *= speedMultiplier * direction;

        foreach (Transform wheel in wheels)
        {
            if (wheel != null)
            {
                wheel.Rotate(rotationAxis, rotationAmount, Space.Self);
            }
        }

        lastPosition = currentPosition;
    }
}
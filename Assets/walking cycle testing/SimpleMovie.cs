using UnityEngine;

public class AxisMover : MonoBehaviour
{
    public enum Axis { X, Y, Z }

    [Header("Movement")]
    [SerializeField] private Axis moveAxis = Axis.X;
    [SerializeField] private float baseSpeed = 1f;
    [SerializeField] private bool useLocalSpace = false;

    [Header("Pulse Settings")]
    [SerializeField] private bool usePulse = false;
    [SerializeField] private float pulseAmplitude = 1f;
    [SerializeField] private float pulseFrequency = 1f;
    [SerializeField] private bool clampToPositiveSpeed = true;

    private Vector3 direction;

    void Awake()
    {
        switch (moveAxis)
        {
            case Axis.X: direction = Vector3.right; break;
            case Axis.Y: direction = Vector3.up; break;
            case Axis.Z: direction = Vector3.forward; break;
        }
    }

    void Update()
    {
        float speed = baseSpeed;

        if (usePulse)
        {
            float pulse = Mathf.Sin(Time.time * pulseFrequency) * pulseAmplitude;
            speed += pulse;

            if (clampToPositiveSpeed)
                speed = Mathf.Max(0f, speed);
        }

        Vector3 movement = direction * speed * Time.deltaTime;

        if (useLocalSpace)
            transform.Translate(movement, Space.Self);
        else
            transform.Translate(movement, Space.World);
    }
}
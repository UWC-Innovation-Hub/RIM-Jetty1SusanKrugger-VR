using UnityEngine;

public class Simple_Rotate : MonoBehaviour
{
    [Header("Rotation Settings")]
    public Vector3 rotationAxis = Vector3.up; // Choose axis (x, y, z)
    public float speed = 50f; // Degrees per second

    void Update()
    {
        transform.Rotate(rotationAxis * speed * Time.deltaTime);
    }
}
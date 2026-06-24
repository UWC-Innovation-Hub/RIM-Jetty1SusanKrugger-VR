using UnityEngine;

public class ClockAnim : MonoBehaviour
{
    [Header("Clock Arms")]
    [SerializeField] private GameObject hourHand;
    [SerializeField] private GameObject minuteHand;
    [SerializeField] private GameObject secondHand;

    private float secondTickTimer;
    private const float SecondTickAngle = 360f / 60f; // 6 degrees

    public enum Axis
    {
        X,
        Y,
        Z
    }

    [Header("Rotation Axis")]
    [SerializeField] private Axis spinAxis = Axis.Z;

    [Header("Direction")]
    [SerializeField] private bool clockwise = true;

    private const float HourHandSpeed = 360f / (12f * 60f * 60f); // 12 hours
    private const float MinuteHandSpeed = 360f / (60f * 60f);     // 1 hour
    private const float SecondHandSpeed = 360f / 60f;             // 1 minute

    private void Update()
    {
        float direction = clockwise ? -1f : 1f;
        Vector3 axis = GetAxisVector();

        if (hourHand != null)
        {
            hourHand.transform.Rotate(
                axis,
                HourHandSpeed * direction * Time.deltaTime,
                Space.Self);
        }

        if (minuteHand != null)
        {
            minuteHand.transform.Rotate(
                axis,
                MinuteHandSpeed * direction * Time.deltaTime,
                Space.Self);
        }

        if (secondHand != null)
        {
            // Tick second hand once per second
            secondTickTimer += Time.deltaTime;

            while (secondTickTimer >= 1f)
            {
                secondTickTimer -= 1f;

                if (secondHand != null)
                {
                    secondHand.transform.Rotate(
                        axis,
                        SecondTickAngle * direction,
                        Space.Self);
                }
            }
        }
    }

    private Vector3 GetAxisVector()
    {
        switch (spinAxis)
        {
            case Axis.X: return Vector3.right;
            case Axis.Y: return Vector3.up;
            default: return Vector3.forward;
        }
    }
}
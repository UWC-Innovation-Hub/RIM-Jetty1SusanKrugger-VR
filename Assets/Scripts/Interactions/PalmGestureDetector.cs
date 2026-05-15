using UnityEngine;
using Oculus.Interaction.Input;

public class PalmGestureDetector : MonoBehaviour
{
    public OVRHand hand;
    public Transform palmPoint;  // attach a small sphere on palm

    public bool IsOpenPalm { get; private set; }

    void Update()
    {
        bool thumb = !hand.GetFingerIsPinching(OVRHand.HandFinger.Thumb);
        bool index = !hand.GetFingerIsPinching(OVRHand.HandFinger.Index);
        bool middle = !hand.GetFingerIsPinching(OVRHand.HandFinger.Middle);
        bool ring = !hand.GetFingerIsPinching(OVRHand.HandFinger.Ring);
        bool pinky = !hand.GetFingerIsPinching(OVRHand.HandFinger.Pinky);

        IsOpenPalm = thumb && index && middle && ring && pinky;
    }
}

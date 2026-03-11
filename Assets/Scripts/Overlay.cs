using UnityEngine;

public class Overlay : MonoBehaviour
{
    [Header("Vignette Material")]
    public Material mat;

    [Header("Meta Hands")]
    public OVRHand leftHand;
    public OVRHand rightHand;


    [Header("Fade Settings")]
    public float fadeSpeed = 3f;
    public float fistThreshold = 0.8f;
    public float maxAlpha = 0.8f;

    float currentAlpha = 0f;
    float targetAlpha = 0f;

    void Update()
    {
        bool leftFist = IsHandFist(leftHand);
        bool rightFist = IsHandFist(rightHand);

        if (leftFist && rightFist)
        {
            targetAlpha = maxAlpha;
        }
        else
        {
            targetAlpha = 0f;
        }

        currentAlpha = Mathf.Lerp(currentAlpha, targetAlpha, Time.deltaTime * fadeSpeed);

        MaterialAlpha(currentAlpha);
    }

    bool IsHandFist(OVRHand hand)
    {
        if (hand == null || !hand.IsTracked)
        {
            return false;
        }

        bool index = hand.GetFingerIsPinching(OVRHand.HandFinger.Index);
        Debug.Log("Index pinch: " + hand.GetFingerIsPinching(OVRHand.HandFinger.Index));
        bool middle = hand.GetFingerIsPinching(OVRHand.HandFinger.Middle);
        bool ring = hand.GetFingerIsPinching(OVRHand.HandFinger.Ring);
        bool pinky = hand.GetFingerIsPinching(OVRHand.HandFinger.Pinky);

        return index && middle && ring && pinky;
    }

    void MaterialAlpha(float alpha)
    {
        Color color = mat.color;
        color.a = alpha;
        mat.color = color;
    }
}

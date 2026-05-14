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
        if (hand == null || !hand.IsTracked || !hand.IsPointerPoseValid)
        {
            return false;
        }
        Debug.Log(Vector3.Distance(hand.PointerPose.position, hand.transform.position));
        Transform pointer = hand.PointerPose;

        Vector3 palmPos = hand.transform.position;

        float distance = Vector3.Distance(pointer.position, palmPos);

        return distance < 0.06f;

        
    }

    void MaterialAlpha(float alpha)
    {
        Color color = mat.color;
        color.a = alpha;
        mat.color = color;
    }
}

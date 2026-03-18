using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Hands;
using UnityEngine.XR.Management;

public class HandGestureListener : MonoBehaviour
{
    public VRTutorial tutorial;

    private XRHandSubsystem handSystem;

    [Header("Gesture Settings")]
    public float fistThreshold = 0.06f;
    public float clapThreshold = 0.08f;
    private float cooldown = 1.5f;

    private float lastTriggerTime = -10f;

    private enum GestureType
    {
        None, Fist, Clap
    }

    private GestureType currentGesture = GestureType.None;

    void OnEnable()
    {
        handSystem = XRGeneralSettings.Instance.Manager.activeLoader.GetLoadedSubsystem<XRHandSubsystem>();

        if (handSystem != null)
        {
            handSystem.updatedHands += OnHandsUpdated;
        }
    }

    void OnDisable()
    {
        if (handSystem != null)
        {
            handSystem.updatedHands -= OnHandsUpdated;
        }
    }

    private void OnHandsUpdated(XRHandSubsystem subsystem, XRHandSubsystem.UpdateSuccessFlags flags, XRHandSubsystem.UpdateType updateType)
    {
        if (Time.time - lastTriggerTime < cooldown)
        {
            return;
        }

        bool fistDetected = isFist(subsystem.leftHand) || isFist(subsystem.rightHand);
        bool clapDetected = isClap(subsystem.leftHand, subsystem.rightHand);

        GestureType detectedGesture = GestureType.None;

        //PRIORITY ORDER
        if (clapDetected)
        {
            detectedGesture = GestureType.Clap;
        }
        else if (fistDetected)
        {
            detectedGesture = GestureType.Fist;
        }

        // Only trigger if gesture CHANGED
        if (detectedGesture != GestureType.None && detectedGesture != currentGesture)
        {
            Debug.Log(detectedGesture + "detected");
            tutorial.NextStep();
            lastTriggerTime = Time.time;
        }

        currentGesture = detectedGesture;
    }

    bool isFist(XRHand hand)
    {
        if (!hand.isTracked)
        {
            return false;
        }

        var palm = hand.GetJoint(XRHandJointID.Palm);

        if (!palm.TryGetPose(out Pose palmPose))
        {
            return false;
        }

        XRHandJointID[] fingertips =
        {
            XRHandJointID.ThumbTip, XRHandJointID.IndexTip, XRHandJointID.MiddleTip, XRHandJointID.RingTip, XRHandJointID.LittleTip
        };

        foreach (var tipID in fingertips)
        {
            var tip = hand.GetJoint(tipID);

            if (!tip.TryGetPose(out Pose tipPose))
            {
                return false;
            }

            float distance = Vector3.Distance(tipPose.position, palmPose.position);

            if (distance > fistThreshold)
            {
                return false;
            }
        }

        return true;
    }

    bool isClap(XRHand leftHand, XRHand rightHand)
    {
        if (!leftHand.isTracked || !rightHand.isTracked)
        {
            return false;
        }

        var leftPalm = leftHand.GetJoint(XRHandJointID.Palm);
        var rightPalm = rightHand.GetJoint(XRHandJointID.Palm);

        if (!leftPalm.TryGetPose(out Pose leftPose))
        {
            return false;
        }

        if (!rightPalm.TryGetPose(out Pose rightPose))
        {
            return false;
        }

        float palmDistance = Vector3.Distance(leftPose.position, rightPose.position);

        return palmDistance > clapThreshold;
    }
}

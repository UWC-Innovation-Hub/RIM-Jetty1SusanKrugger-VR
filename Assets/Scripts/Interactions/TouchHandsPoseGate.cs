using Oculus.Interaction.Input;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider))]
public class TouchHandsPoseGate : MonoBehaviour
{
    [SerializeField] private GameObject poseDetectionRoot;
    [SerializeField] private bool handCollidersOnly = true;

    private void Reset()
    {
        Collider trigger = GetComponent<Collider>();
        if (trigger != null)
        {
            trigger.isTrigger = true;
        }

        if (poseDetectionRoot == null)
        {
            Transform poseRoot = transform.Find("BasicPoseDetectionPoses");
            if (poseRoot != null)
            {
                poseDetectionRoot = poseRoot.gameObject;
            }
        }
    }

    private void Awake()
    {
        SetPoseDetectionActive(false);
    }

    private void OnDisable()
    {
        SetPoseDetectionActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (IsValidCollider(other))
        {
            SetPoseDetectionActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (IsValidCollider(other))
        {
            SetPoseDetectionActive(false);
        }
    }

    private bool IsValidCollider(Collider other)
    {
        if (other == null)
        {
            return false;
        }

        return !handCollidersOnly || other.GetComponentInParent<HandRef>() != null;
    }

    private void SetPoseDetectionActive(bool isActive)
    {
        if (poseDetectionRoot != null)
        {
            poseDetectionRoot.SetActive(isActive);
        }
    }
}

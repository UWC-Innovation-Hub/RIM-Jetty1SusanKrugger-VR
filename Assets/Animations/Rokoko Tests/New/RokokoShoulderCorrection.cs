
using UnityEngine;

[DefaultExecutionOrder(10000)]
public class RokokoShoulderCorrection : MonoBehaviour
{
    [Header("Bone Detection")]
    public Animator animator;
    public bool findBonesOnAwake = true;

    [Header("Detected Bones")]
    public Transform leftShoulder;
    public Transform rightShoulder;
    public Transform leftUpperArm;
    public Transform rightUpperArm;

    [Header("Master Correction")]
    [Range(0f, 1f)]
    public float correctionWeight = 1f;

    [Header("Slider Limits")]
    public float minAngle = -180f;
    public float maxAngle = 180f;

    [Header("Left Shoulder")]
    public Vector3 leftShoulderOffset;

    [Header("Right Shoulder")]
    public Vector3 rightShoulderOffset;

    [Header("Left Upper Arm")]
    public Vector3 leftArmOffset;

    [Header("Right Upper Arm")]
    public Vector3 rightArmOffset;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (findBonesOnAwake)
            FindBones();
    }

    [ContextMenu("Find Mixamo Bones")]
    public void FindBones()
    {
        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (animator == null)
        {
            Debug.LogError("No Animator found.", this);
            return;
        }

        leftShoulder = FindMixamoBone("LeftShoulder");
        rightShoulder = FindMixamoBone("RightShoulder");

        leftUpperArm = FindMixamoBone("LeftArm");
        rightUpperArm = FindMixamoBone("RightArm");

        Debug.Log(
            "Mixamo Bone Detection:\n" +
            "Left Shoulder: " + BoneStatus(leftShoulder) + "\n" +
            "Right Shoulder: " + BoneStatus(rightShoulder) + "\n" +
            "Left Arm: " + BoneStatus(leftUpperArm) + "\n" +
            "Right Arm: " + BoneStatus(rightUpperArm),
            this
        );
    }

    private Transform FindMixamoBone(string boneName)
    {
        Transform[] bones =
            animator.GetComponentsInChildren<Transform>(true);

        string target = "mixamorig:" + boneName;

        foreach (Transform bone in bones)
        {
            if (bone.name == target)
                return bone;
        }

        foreach (Transform bone in bones)
        {
            if (bone.name == boneName)
                return bone;
        }

        return null;
    }

    private string BoneStatus(Transform bone)
    {
        return bone != null ? bone.name : "NOT FOUND";
    }

    private void LateUpdate()
    {
        if (animator == null || !animator.enabled)
            return;

        ApplyCorrection(leftShoulder, leftShoulderOffset);
        ApplyCorrection(rightShoulder, rightShoulderOffset);

        ApplyCorrection(leftUpperArm, leftArmOffset);
        ApplyCorrection(rightUpperArm, rightArmOffset);
    }

    private void ApplyCorrection(Transform bone, Vector3 offset)
    {
        if (bone == null)
            return;

        Quaternion correction = Quaternion.Euler(
            offset.x * correctionWeight,
            offset.y * correctionWeight,
            offset.z * correctionWeight
        );

        bone.localRotation *= correction;
    }
}

using UnityEngine;

public class IKTargetFollowVRRig : MonoBehaviour
{
    [System.Serializable]
    public class VRMap
    {
        public Transform vrTarget;     // e.g. CenterEyeAnchor / LeftHandAnchor / RightHandAnchor
        public Transform ikTarget;     // the Animation Rigging target
        public Vector3 trackingPositionOffset;
        public Vector3 trackingRotationOffset;

        public void Map()
        {
            if (vrTarget == null || ikTarget == null) return;

            ikTarget.position = vrTarget.TransformPoint(trackingPositionOffset);
            ikTarget.rotation = vrTarget.rotation * Quaternion.Euler(trackingRotationOffset);
        }
    }





    [Header("VR Maps")]
    public VRMap head;
    public VRMap leftHand;
    public VRMap rightHand;


    [Header("Body Yaw Logic")]
    [Tooltip("Degrees of head/body yaw difference before body begins to turn.")]
    public float bodyRotationStartThreshold = 45f;

    [Tooltip("Degrees of head/body yaw difference where body stops turning (smaller than start).")]
    public float bodyRotationStopThreshold = 10f;

    [Tooltip("Time (in seconds) for body to smoothly align to target yaw once turning.")]
    public float bodyTurnSmoothTime = 0.2f;

    private float _bodyYawVelocity;   // for SmoothDampAngle
    private bool _isRotatingBody;



    [Header("Body Positioning")]
    [Tooltip("Offset from head to body root, in body-local space.")]
    public Vector3 headBodyPositionOffset = new Vector3(0f, -0.8f, -0.05f);

    [Header("Body Yaw Logic")]
    [Tooltip("Degrees of head/body yaw difference before body begins to turn.")]
    public float bodyRotationThreshold = 45f;

    [Tooltip("Degrees per second that the body can rotate toward the head.")]
    public float bodyTurnSpeed = 180f;

    // current body yaw in world space
    private float _bodyYaw;

    void Start()
    {
        // Initialise body yaw from the current root rotation
        _bodyYaw = transform.rotation.eulerAngles.y;
    }

    void LateUpdate()
    {
        if (head == null || head.vrTarget == null) return;

        // 1. Drive IK targets from VR rig
        head.Map();
        leftHand.Map();
        rightHand.Map();

        // 2. Compute yaw difference
        float headYaw = head.vrTarget.eulerAngles.y;
        float deltaYaw = Mathf.DeltaAngle(_bodyYaw, headYaw);
        float absDelta = Mathf.Abs(deltaYaw);

        // 3. Update rotation state with hysteresis
        if (_isRotatingBody)
        {
            // Keep rotating until we're basically aligned
            if (absDelta < bodyRotationStopThreshold)
                _isRotatingBody = false;
        }
        else
        {
            // Start rotating only when we exceed the larger threshold
            if (absDelta > bodyRotationStartThreshold)
                _isRotatingBody = true;
        }

        // 4. Update body yaw
        if (_isRotatingBody)
        {
            // Smoothly move body yaw toward head yaw
            _bodyYaw = Mathf.SmoothDampAngle(
                _bodyYaw,
                headYaw,
                ref _bodyYawVelocity,
                bodyTurnSmoothTime
            );
        }

        Quaternion bodyRot = Quaternion.Euler(0f, _bodyYaw, 0f);
        transform.rotation = bodyRot;

        // 5. Position body relative to head in body space
        Vector3 headPos = head.ikTarget != null ? head.ikTarget.position : head.vrTarget.position;
        transform.position = headPos + bodyRot * headBodyPositionOffset;
    }

}



//using UnityEngine;

//[System.Serializable]
//public class VRMap
//{
//    public Transform vrTarget;
//    public Transform ikTarget;
//    public Vector3 trackingPositionOffset;
//    public Vector3 trackingRotationOffset;
//    public void Map()
//    {
//        ikTarget.position = vrTarget.TransformPoint(trackingPositionOffset);
//        ikTarget.rotation = vrTarget.rotation * Quaternion.Euler(trackingRotationOffset);
//    }
//}

//public class IKTargetFollowVRRig : MonoBehaviour
//{
//    [Range(0,1)]
//    public float turnSmoothness = 0.1f;
//    public VRMap head;
//    public VRMap leftHand;
//    public VRMap rightHand;

//    public Vector3 headBodyPositionOffset;
//    public float headBodyYawOffset;

//    // Update is called once per frame
//    void LateUpdate()
//    {
//        transform.position = head.ikTarget.position + headBodyPositionOffset;
//        float yaw = head.vrTarget.eulerAngles.y;
//        transform.rotation = Quaternion.Lerp(transform.rotation,Quaternion.Euler(transform.eulerAngles.x, yaw, transform.eulerAngles.z),turnSmoothness);

//        head.Map();
//        leftHand.Map();
//        rightHand.Map();
//    }
//}

using UnityEngine;
using Oculus.Interaction.Input;

public class HandMenuActivator : MonoBehaviour
{
    [Header("Menu References")]
    [Tooltip("The UI GameObject to activate/deactivate")]
    public GameObject MenuUI;
    
    [Tooltip("Parent transform where the menu will be attached at runtime")]
    public Transform MenuHolder;
    
    [Tooltip("The wrist bone transform to track palm orientation")]
    public Transform WristBoneTransform;
    
    [Tooltip("The VR camera to calculate palm facing direction")]
    public Camera VRCamera;
    
    [Header("Meta XR Hand Tracking")]
    [Tooltip("Reference to the Oculus Hand component for hand tracking state")]
    public Hand OculusHand;
    
    [Tooltip("Which hand to track")]
    public Handedness TrackedHand = Handedness.Left;
    
    [Header("Activation Settings")]
    [Tooltip("Maximum angle between palm normal and camera direction to activate menu")]
    [Range(0f, 90f)]
    public float ActivationAngleThreshold = 45f;
    
    [Tooltip("Which local axis of the wrist bone points out from the palm (try Forward, Up, or Down)")]
    public PalmNormalAxis PalmNormal = PalmNormalAxis.Forward;
    
    [Tooltip("Time in seconds the palm must face forward before activating")]
    [Range(0f, 2f)]
    public float ActivationDelay = 0.3f;
    
    [Tooltip("Menu stays visible after activation until dismissed with swipe gesture")]
    public bool StayVisibleUntilDismissed = true;
    
    [Tooltip("Require hand tracking to be active (recommended)")]
    public bool RequireHandTracking = true;
    
    [Header("Dismiss Gesture Settings")]
    [Tooltip("Minimum swipe distance in meters to dismiss menu")]
    [Range(0.05f, 0.3f)]
    public float SwipeDistanceThreshold = 0.15f;
    
    [Tooltip("Maximum time in seconds for swipe gesture")]
    [Range(0.1f, 2f)]
    public float SwipeTimeThreshold = 0.8f;
    
    [Tooltip("Minimum horizontal component (0-1) to count as left/right swipe")]
    [Range(0.3f, 1f)]
    public float SwipeDirectionalThreshold = 0.6f;
    
    [Tooltip("Time after menu activation before swipe detection starts")]
    [Range(0.1f, 2f)]
    public float SwipeActivationDelay = 0.5f;
    
    [Header("Menu Positioning")]
    [Tooltip("Offset from wrist in local space")]
    public Vector3 MenuOffset = new Vector3(0f, 0.05f, 0f);
    
    [Tooltip("Should menu always face the camera")]
    public bool BillboardToCamera = true;
    
    [Tooltip("Use full 3D billboard rotation (true) or Y-axis only (false)")]
    public bool FullBillboardRotation = true;
    
    [Tooltip("Additional rotation applied to menu after billboard (Euler angles)")]
    public Vector3 MenuRotationOffset = new Vector3(-75f, -180f, 0f);
    
    [Tooltip("Additional forward offset in world space (pushes menu toward camera)")]
    [Range(0f, 0.5f)]
    public float ForwardOffset = 0.1f;
    
    [Tooltip("Lock menu position after activation (recommended for stability)")]
    public bool LockPositionAfterActivation = true;
    
    [Header("Debug")]
    [Tooltip("Show debug logs")]
    public bool DebugMode = false;
    
    private bool isMenuActive = false;
    private float currentActivationTime = 0f;
    private bool isInitialized = false;
    
    private Vector3 swipeStartPosition;
    private float swipeStartTime;
    private bool isTrackingSwipe = false;
    private bool positionLocked = false;
    private float menuActivationTime;
    
    private void Start()
    {
        InitializeMenu();
        TryAutoFindHand();
        SetupCanvasCamera();
    }
    
    private void SetupCanvasCamera()
    {
        if (MenuUI == null || VRCamera == null)
            return;
        
        Canvas canvas = MenuUI.GetComponent<Canvas>();
        if (canvas != null && canvas.renderMode == RenderMode.WorldSpace)
        {
            if (canvas.worldCamera == null)
            {
                canvas.worldCamera = VRCamera;
                
                if (DebugMode)
                    Debug.Log($"[HandMenuActivator] Set Canvas worldCamera to {VRCamera.name}");
            }
        }
    }
    
    private void InitializeMenu()
    {
        if (MenuUI == null)
        {
            Debug.LogWarning("[HandMenuActivator] MenuUI is not assigned!");
            return;
        }
        
        if (MenuHolder != null)
        {
            MenuUI.transform.SetParent(MenuHolder, false);
            MenuUI.transform.localPosition = Vector3.zero;
            MenuUI.transform.localRotation = Quaternion.identity;
            
            if (DebugMode)
                Debug.Log($"[HandMenuActivator] MenuUI parented to {MenuHolder.name}");
        }
        else
        {
            Debug.LogWarning("[HandMenuActivator] MenuHolder not assigned. Menu will not be parented at runtime.");
        }
        
        MenuUI.SetActive(false);
        isInitialized = true;
    }
    
    private void TryAutoFindHand()
    {
        if (OculusHand != null)
            return;
        
        string handName = TrackedHand == Handedness.Left ? "LeftInteractions" : "RightInteractions";
        GameObject handInteractions = GameObject.Find(handName);
        
        if (handInteractions != null)
        {
            OculusHand = handInteractions.GetComponent<Hand>();
            if (OculusHand != null && DebugMode)
            {
                Debug.Log($"[HandMenuActivator] Auto-found Oculus Hand: {handName}");
            }
        }
        
        if (OculusHand == null)
        {
            Debug.LogWarning($"[HandMenuActivator] Could not find Oculus Hand component for {TrackedHand} hand. Hand tracking validation will be limited.");
        }
    }
    
    private void Update()
    {
        if (!isInitialized || MenuUI == null)
            return;
        
        if (!ValidateReferences())
            return;
        
        if (isMenuActive)
        {
            if (StayVisibleUntilDismissed)
            {
                CheckForDismissGesture();
            }
            else
            {
                if (!ShouldActivateMenu())
                {
                    currentActivationTime = 0f;
                    DeactivateMenu();
                }
            }
            
            if (isMenuActive && MenuHolder != null)
            {
                if (!LockPositionAfterActivation || !positionLocked)
                {
                    UpdateMenuPosition();
                    
                    if (LockPositionAfterActivation)
                    {
                        positionLocked = true;
                    }
                }
            }
        }
        else
        {
            bool shouldActivate = ShouldActivateMenu();
            
            if (shouldActivate)
            {
                currentActivationTime += Time.deltaTime;
                
                if (currentActivationTime >= ActivationDelay)
                {
                    ActivateMenu();
                }
            }
            else
            {
                currentActivationTime = 0f;
            }
        }
    }
    
    private bool ValidateReferences()
    {
        if (WristBoneTransform == null)
        {
            Debug.LogWarning("[HandMenuActivator] WristBoneTransform is not assigned!");
            return false;
        }
        
        if (VRCamera == null)
        {
            Debug.LogWarning("[HandMenuActivator] VRCamera is not assigned!");
            return false;
        }
        
        return true;
    }
    
    private bool ShouldActivateMenu()
    {
        if (RequireHandTracking && !IsHandTracked())
        {
            if (DebugMode && isMenuActive)
                Debug.Log("[HandMenuActivator] Hand tracking lost");
            return false;
        }
        
        Vector3 palmNormal = GetPalmNormal();
        Vector3 directionToCamera = (VRCamera.transform.position - WristBoneTransform.position).normalized;
        float angleToCamera = Vector3.Angle(palmNormal, directionToCamera);
        
        if (DebugMode)
        {
            Debug.Log($"[HandMenuActivator] Angle: {angleToCamera:F1}° | Threshold: {ActivationAngleThreshold}° | Active: {isMenuActive} | Should Activate: {angleToCamera <= ActivationAngleThreshold}");
            Debug.DrawRay(WristBoneTransform.position, palmNormal * 0.1f, Color.blue, 0.1f);
            Debug.DrawRay(WristBoneTransform.position, directionToCamera * 0.1f, Color.green, 0.1f);
        }
        
        return angleToCamera <= ActivationAngleThreshold;
    }
    
    private Vector3 GetPalmNormal()
    {
        switch (PalmNormal)
        {
            case PalmNormalAxis.Forward:
                return WristBoneTransform.forward;
            case PalmNormalAxis.Back:
                return -WristBoneTransform.forward;
            case PalmNormalAxis.Up:
                return WristBoneTransform.up;
            case PalmNormalAxis.Down:
                return -WristBoneTransform.up;
            case PalmNormalAxis.Right:
                return WristBoneTransform.right;
            case PalmNormalAxis.Left:
                return -WristBoneTransform.right;
            default:
                return WristBoneTransform.forward;
        }
    }
    
    private bool IsHandTracked()
    {
        if (OculusHand == null)
            return true;
        
        return OculusHand.IsTrackedDataValid;
    }
    
    private void ActivateMenu()
    {
        MenuUI.SetActive(true);
        isMenuActive = true;
        currentActivationTime = 0f;
        menuActivationTime = Time.time;
        isTrackingSwipe = false;
        
        if (DebugMode)
            Debug.Log("[HandMenuActivator] Menu activated");
    }
    
    private void DeactivateMenu()
    {
        MenuUI.SetActive(false);
        isMenuActive = false;
        currentActivationTime = 0f;
        isTrackingSwipe = false;
        positionLocked = false;
        
        if (DebugMode)
            Debug.Log("[HandMenuActivator] Menu deactivated");
    }
    
    private void CheckForDismissGesture()
    {
        if (Time.time - menuActivationTime < SwipeActivationDelay)
        {
            return;
        }
        
        if (!isTrackingSwipe)
        {
            swipeStartPosition = WristBoneTransform.position;
            swipeStartTime = Time.time;
            isTrackingSwipe = true;
            
            if (DebugMode)
                Debug.Log($"[HandMenuActivator] Started tracking swipe from position {swipeStartPosition}");
            
            return;
        }
        
        float swipeDuration = Time.time - swipeStartTime;
        Vector3 swipeDelta = WristBoneTransform.position - swipeStartPosition;
        float swipeDistance = swipeDelta.magnitude;
        
        Vector3 cameraRight = VRCamera.transform.right;
        cameraRight.y = 0f;
        cameraRight.Normalize();
        
        Vector3 horizontalDelta = swipeDelta;
        horizontalDelta.y = 0f;
        
        float horizontalDistance = horizontalDelta.magnitude;
        float horizontalSwipe = 0f;
        
        if (horizontalDistance > 0.001f)
        {
            horizontalSwipe = Vector3.Dot(horizontalDelta.normalized, cameraRight);
        }
        
        if (DebugMode && swipeDistance > 0.02f && swipeDuration < SwipeTimeThreshold)
        {
            Debug.Log($"[HandMenuActivator] Swipe: Dist={swipeDistance:F3}m, HorizDist={horizontalDistance:F3}m, Dir={horizontalSwipe:F2}, Time={swipeDuration:F2}s");
        }
        
        if (swipeDistance >= SwipeDistanceThreshold && swipeDuration <= SwipeTimeThreshold)
        {
            if (Mathf.Abs(horizontalSwipe) >= SwipeDirectionalThreshold)
            {
                string direction = horizontalSwipe > 0 ? "Right" : "Left";
                
                if (DebugMode)
                    Debug.Log($"[HandMenuActivator] ✓ SWIPE DETECTED! Dist={swipeDistance:F3}m, Dir={direction}, Time={swipeDuration:F2}s");
                
                DeactivateMenu();
                return;
            }
        }
        
        if (swipeDuration > SwipeTimeThreshold)
        {
            if (DebugMode && swipeDistance > 0.05f)
                Debug.Log($"[HandMenuActivator] Swipe timeout (Dist={swipeDistance:F3}m, Dir={horizontalSwipe:F2}) - resetting");
            
            swipeStartPosition = WristBoneTransform.position;
            swipeStartTime = Time.time;
        }
    }
    
    private void UpdateMenuPosition()
    {
        Vector3 basePosition = WristBoneTransform.position + WristBoneTransform.TransformDirection(MenuOffset);
        
        Quaternion baseRotation;
        
        if (BillboardToCamera)
        {
            Vector3 directionToCamera = (VRCamera.transform.position - basePosition).normalized;
            
            if (FullBillboardRotation)
            {
                baseRotation = Quaternion.LookRotation(directionToCamera);
            }
            else
            {
                directionToCamera.y = 0f;
                if (directionToCamera != Vector3.zero)
                {
                    baseRotation = Quaternion.LookRotation(directionToCamera);
                }
                else
                {
                    baseRotation = WristBoneTransform.rotation;
                }
            }
        }
        else
        {
            baseRotation = WristBoneTransform.rotation;
        }
        
        MenuHolder.rotation = baseRotation * Quaternion.Euler(MenuRotationOffset);
        MenuHolder.position = basePosition + MenuHolder.forward * ForwardOffset;
    }
    
    private void OnDisable()
    {
        if (MenuUI != null)
        {
            MenuUI.SetActive(false);
        }
        isMenuActive = false;
        currentActivationTime = 0f;
        isTrackingSwipe = false;
        positionLocked = false;
    }
}

public enum Handedness
{
    Left,
    Right
}

public enum PalmNormalAxis
{
    Forward,
    Back,
    Up,
    Down,
    Right,
    Left
}

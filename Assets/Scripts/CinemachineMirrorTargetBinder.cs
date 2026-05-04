using Unity.Cinemachine;
using UnityEngine;

[DisallowMultipleComponent]
public class CinemachineMirrorTargetBinder : MonoBehaviour
{
    private enum TargetSource
    {
        BrainOutputCamera,
        ActiveVirtualCamera
    }

    [SerializeField] private TargetSource targetSource = TargetSource.BrainOutputCamera;
    [SerializeField] private CinemachineBrain brain;
    [SerializeField] private mirror_movement mirrorMovement;

    private void Reset()
    {
        ResolveMissingReferences();
    }

    private void Awake()
    {
        ResolveMissingReferences();
    }

    private void OnEnable()
    {
        CinemachineCore.CameraActivatedEvent.AddListener(OnCameraActivated);
        CinemachineCore.CameraUpdatedEvent.AddListener(OnCameraUpdated);

        if (targetSource == TargetSource.BrainOutputCamera)
            AssignBrainOutput(brain);
        else
            AssignVirtualCamera(brain != null ? brain.ActiveVirtualCamera : null);
    }

    private void OnDisable()
    {
        CinemachineCore.CameraActivatedEvent.RemoveListener(OnCameraActivated);
        CinemachineCore.CameraUpdatedEvent.RemoveListener(OnCameraUpdated);
    }

    private void OnCameraActivated(ICinemachineCamera.ActivationEventParams evt)
    {
        if (targetSource != TargetSource.ActiveVirtualCamera)
            return;

        if (brain != null && !ReferenceEquals(evt.Origin, brain))
            return;

        AssignVirtualCamera(evt.IncomingCamera);
    }

    private void OnCameraUpdated(CinemachineBrain updatedBrain)
    {
        if (targetSource != TargetSource.BrainOutputCamera)
            return;

        if (brain != null && updatedBrain != brain)
            return;

        AssignBrainOutput(updatedBrain);
    }

    private void AssignBrainOutput(CinemachineBrain targetBrain)
    {
        if (mirrorMovement == null || targetBrain == null || targetBrain.OutputCamera == null)
            return;

        mirrorMovement.SetPlayerTarget(targetBrain.OutputCamera.transform);
        mirrorMovement.RefreshMirrorPose();
    }

    private void AssignVirtualCamera(ICinemachineCamera camera)
    {
        if (mirrorMovement == null)
            return;

        if (camera is CinemachineVirtualCameraBase vcam)
        {
            mirrorMovement.SetPlayerTarget(vcam.transform);
            mirrorMovement.RefreshMirrorPose();
        }
    }

    private void ResolveMissingReferences()
    {
        if (brain == null)
            TryGetComponent(out brain);

        if (mirrorMovement == null)
            mirrorMovement = FindFirstObjectByType<mirror_movement>();
    }
}

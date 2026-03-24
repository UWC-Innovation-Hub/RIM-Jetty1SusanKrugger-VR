# RIM Jetty 1 & Susan Kruger — VR Experience

An immersive virtual reality (VR) project built in Unity for Meta Quest 3, designed to honour and preserve the stories of those who passed through the Jetty 1 and Susan Kruger sites during South Africa's struggle against apartheid.

Through VR, visitors can walk through historically significant locations on Robben Island, discovering stories, visuals, and archival material from the apartheid era in a way that is both emotionally engaging and educational.

## Project Info

| Field | Detail |
|---|---|
| **Organisation** | UWC Immersive Zone (UWC Innovation Hub) |
| **Platform** | Meta Quest 3 |
| **Engine** | Unity 6 (6.3.8f1) |
| **XR SDKs** | Meta XR All-in-One SDK v78, XR Interaction Toolkit v3.1.1/v3.2.1, XR Hands v1.5.0, Meta XR Movement SDK |
| **Audio** | Meta XR Audio SDK (spatial audio, ambisonics) |
| **3D Tools** | Blender, 3ds Max |
| **Language** | C# |
| **License** | See `LICENSE.txt` |

## Repository Structure

<!-- AUTO-STRUCTURE-START -->
```
        ├── Materials/
    ├── Samples/
        ├── Meta XR Interaction \342\200\213SDK/
├── .dotnet/
├── .gitattributes/
    ├── ISSUE_TEMPLATE/
    ├── workflows/
├── .gitignore/
├── .vsconfig/
├── Assets/
    ├── Animations/
        ├── C1/
        ├── J1/
    ├── Assets/
        ├── RewardToken/
    ├── Audio/
        ├── C1/
            ├── Master/
            ├── SFX/
            ├── VO/
        ├── C2/
            ├── Master/
            ├── SFX/
            ├── VO/
        ├── C3/
        ├── C4/
            ├── Master/
        ├── C5/
        ├── C6/
            ├── Master/
    ├── CompositionLayers/
        ├── UserSettings/
            ├── Resources/
    ├── Custom Prefabs/
    ├── InteractionSDK/
    ├── InventoryPrefabs/
        ├── Materials/
    ├── Materials/
        ├── C1_J2_Lobby/
            ├── Materials/
        ├── C4_J5/
        ├── C6_SK1_The_Hold/
        ├── Cargo space/
        ├── Cargo space exterior/
            ├── Silhouettes/
        ├── Cell/
        ├── Custom Shaders/
            ├── C1_InventoryMats/
            ├── C1_Prisoner_Direction/
        ├── General/
        ├── J1/
            ├── Entrance room/
        ├── MAIO_Tests/
        ├── Registration Office/
        ├── Warders/
    ├── Media/
        ├── Audio/
        ├── Video/
            ├── C1/
            ├── RIM Show and Tell/
    ├── MetaXR/
    ├── Models/
        ├── 3D_Scans/
            ├── OPTIMIZED/
            ├── RAW/
        ├── C1_J2_Lobby/
            ├── 6-infinite-mirror-3december2019/
            ├── Characters/
            ├── Flag_Pole_Animated/
            ├── SM_Mirror/
            ├── Table/
            ├── Temporary_Characters/
            ├── opel-blitz-truck/
            ├── picture-frame-11mb/
        ├── C2_J3_J4_Cell/
            ├── Lamp_iwanPlays/
            ├── prison-bed/
        ├── C4_J6_Black_Visitors_Room/
            ├── folded-wrinkled-paper/
        ├── C6_SK1_The_Hold/
            ├── GrayBox/
        ├── Cargo Space/
        ├── Cargo Space exterior/
            ├── Animations/
        ├── Cell/
        ├── NAACo/
            ├── Warehouse_Pack_HD/
        ├── Props_1st_Pass/
            ├── Props/
        ├── Registration office/
        ├── Textures/
        ├── Warders/
            ├── characters/
    ├── Oculus/
    ├── Plugins/
        ├── Android/
    ├── Prefabs/
        ├── Interactions/
        ├── Walking man/
    ├── Prisoner Shaders/
    ├── RIM_jetty1_assets/
        ├── 3dContent/
            ├── Old_Scans_Polycam/
        ├── Images/
        ├── Material/
        ├── Videos/
        ├── scripts/
    ├── Resources/
    ├── Samples/
        ├── Meta XR All-in-One SDK/
        ├── Meta XR Audio SDK/
        ├── Meta XR Movement SDK/
        ├── Scriptable Render Pipeline Core/
        ├── Shader Graph/
        ├── XR Hands/
        ├── XR Interaction Toolkit/
    ├── Scenes/
        ├── BasicScene/
        ├── C1_J2_Warden_Experience/
        ├── C2_J3_Cell/
        ├── C4_J6_Black_Visitors_Room/
        ├── C6_SK1_The_Hold/
        ├── JS_Testing/
            ├── J1_Testing_lightbake_JS/
            ├── Registration office setup/
        ├── SampleScene/
        ├── TestScenes/
            ├── BlackWaitingRoomTest/
            ├── C2_J3_Cell_Rec_Assets/
            ├── MemoryProjectorTest/
            ├── SortPrisonerTest/
    ├── Script Prefabs/
    ├── Scripts/
        ├── InventoryTest/
    ├── Settings/
        ├── Build Profiles/
        ├── Project Configuration/
    ├── Starter Assets/
        ├── Editor/
        ├── Runtime/
        ├── Sample/
        ├── TutorialInfo/
    ├── TextMesh Pro/
        ├── Fonts/
        ├── Resources/
            ├── Fonts & Materials/
            ├── Sprite Assets/
            ├── Style Sheets/
        ├── Shaders/
        ├── Sprites/
    ├── Textures/
        ├── C1_J2/
        ├── C2_J3/
            ├── Fingerprints/
            ├── Materials/
        ├── Cargo Space/
        ├── Cell/
        ├── Silhouettes/
        ├── Test Textures/
        ├── Warders/
            ├── Warder Cuffs/
            ├── Warder Holster/
            ├── Warder pistol/
    ├── TokenTextures/
        ├── Textures/
    ├── UI Script Prefabs/
    ├── VR Body/
        ├── Animations/
        ├── Ch32_nonPBR.fbm/
        ├── Models/
    ├── VRTemplateAssets/
        ├── Android XR/
        ├── Audio/
        ├── Fonts/
        ├── Graphics/
        ├── Materials/
        ├── Models/
        ├── Prefabs/
        ├── Scripts/
        ├── Shaders/
        ├── Sprites/
        ├── Themes/
        ├── Tutorial/
        ├── Videos/
    ├── XR/
        ├── Loaders/
        ├── Resources/
        ├── Settings/
        ├── UserSimulationSettings/
            ├── Resources/
    ├── XRI/
        ├── Settings/
            ├── Resources/
├── LICENSE.txt/
├── Packages/
        ├── Unity-Movement-78.0.0/
            ├── .Documentation/
            ├── Editor/
            ├── Resources/
            ├── Runtime/
            ├── Samples~/
            ├── Shared/
├── ProjectSettings/
        ├── com.unity.learn.iet-framework/
        ├── com.unity.testtools.codecoverage/
├── README.md/
├── RIM-Jetty1SusanKrugger-VR.slnx/
```
<!-- AUTO-STRUCTURE-END -->

## Key Scripts

<!-- AUTO-SCRIPTS-START -->
| Script | Path |
|---|---|
| `Coin emmision test.cs` | `Assets/Assets/RewardToken/Coin emmision test.cs` |
| `FootstepSMB.cs` | `Assets/FootstepSMB.cs` |
| `ElapsedTtimer.cs` | `Assets/RIM_jetty1_assets/scripts/ElapsedTtimer.cs` |
| `SceneManager.cs` | `Assets/RIM_jetty1_assets/scripts/SceneManager.cs` |
| `UISceneLoader.cs` | `Assets/RIM_jetty1_assets/scripts/UISceneLoader.cs` |
| `Background_Character_Spawning.cs` | `Assets/Scripts/Background_Character_Spawning.cs` |
| `BarShake.cs` | `Assets/Scripts/BarShake.cs` |
| `Billboard.cs` | `Assets/Scripts/Billboard.cs` |
| `BreathingInteractionModule.cs` | `Assets/Scripts/BreathingInteractionModule.cs` |
| `ButtonScale.cs` | `Assets/Scripts/ButtonScale.cs` |
| `CanvasFade.cs` | `Assets/Scripts/CanvasFade.cs` |
| `CargoDoorDriver.cs` | `Assets/Scripts/CargoDoorDriver.cs` |
| `ClapConfirmationUI.cs` | `Assets/Scripts/ClapConfirmationUI.cs` |
| `ClapDetector.cs` | `Assets/Scripts/ClapDetector.cs` |
| `CountUpTimer.cs` | `Assets/Scripts/CountUpTimer.cs` |
| `CountdownTimer.cs` | `Assets/Scripts/CountdownTimer.cs` |
| `FadeControllerScript.cs` | `Assets/Scripts/FadeControllerScript.cs` |
| `FingerprintLockout.cs` | `Assets/Scripts/FingerprintLockout.cs` |
| `FingerprintProjectionInteractionModule.cs` | `Assets/Scripts/FingerprintProjectionInteractionModule.cs` |
| `FingerprintTrigger.cs` | `Assets/Scripts/FingerprintTrigger.cs` |
| `GazeIndicator.cs` | `Assets/Scripts/GazeIndicator.cs` |
| `GazeRaycaster.cs` | `Assets/Scripts/GazeRaycaster.cs` |
| `GazeTarget.cs` | `Assets/Scripts/GazeTarget.cs` |
| `HandGestureListener.cs` | `Assets/Scripts/HandGestureListener.cs` |
| `HandTouchInteractionModule.cs` | `Assets/Scripts/HandTouchInteractionModule.cs` |
| `Headlock.cs` | `Assets/Scripts/Headlock.cs` |
| `HighFiveTrigger.cs` | `Assets/Scripts/HighFiveTrigger.cs` |
| `HighlightExit.cs` | `Assets/Scripts/HighlightExit.cs` |
| `HighlightExitManager.cs` | `Assets/Scripts/HighlightExitManager.cs` |
| `IGazeTarget.cs` | `Assets/Scripts/IGazeTarget.cs` |
| `InteractionCompleteListener.cs` | `Assets/Scripts/InteractionCompleteListener.cs` |
| `InteractionModuleBase.cs` | `Assets/Scripts/InteractionModuleBase.cs` |
| `InventoryModule.cs` | `Assets/Scripts/InventoryModule.cs` |
| `AvatarAttachmentPoint.cs` | `Assets/Scripts/InventoryTest/AvatarAttachmentPoint.cs` |
| `BodyAttachmentRig.cs` | `Assets/Scripts/InventoryTest/BodyAttachmentRig.cs` |
| `EquippableItem.cs` | `Assets/Scripts/InventoryTest/EquippableItem.cs` |
| `InventoryManager.cs` | `Assets/Scripts/InventoryTest/InventoryManager.cs` |
| `SimpleBodyAttachments.cs` | `Assets/Scripts/InventoryTest/SimpleBodyAttachments.cs` |
| `LocationSelect.cs` | `Assets/Scripts/LocationSelect.cs` |
| `MAIO_Vid_Controller.cs` | `Assets/Scripts/MAIO_Vid_Controller.cs` |
| `MaterialOpacityFader.cs` | `Assets/Scripts/MaterialOpacityFader.cs` |
| `MirrorFollow.cs` | `Assets/Scripts/MirrorFollow.cs` |
| `Overlay.cs` | `Assets/Scripts/Overlay.cs` |
| `PalmGestureDetector.cs` | `Assets/Scripts/PalmGestureDetector.cs` |
| `PickUpUI.cs` | `Assets/Scripts/PickUpUI.cs` |
| `PrisonerRoute.cs` | `Assets/Scripts/PrisonerRoute.cs` |
| `PrisonerSortModule.cs` | `Assets/Scripts/PrisonerSortModule.cs` |
| `ProgressBar.cs` | `Assets/Scripts/ProgressBar.cs` |
| `ProgressObject.cs` | `Assets/Scripts/ProgressObject.cs` |
| `ProgressTracker.cs` | `Assets/Scripts/ProgressTracker.cs` |
| `ProjectionController.cs` | `Assets/Scripts/ProjectionController.cs` |
| `ReactivateRoutes.cs` | `Assets/Scripts/ReactivateRoutes.cs` |
| `RewardUI.cs` | `Assets/Scripts/RewardUI.cs` |
| `SequenceBrain.cs` | `Assets/Scripts/SequenceBrain.cs` |
| `SequenceSignalRouter.cs` | `Assets/Scripts/SequenceSignalRouter.cs` |
| `SetNextPrisoner.cs` | `Assets/Scripts/SetNextPrisoner.cs` |
| `TimelineBindingTransfer.cs` | `Assets/Scripts/TimelineBindingTransfer.cs` |
| `TimelineVideoPlaybackController.cs` | `Assets/Scripts/TimelineVideoPlaybackController.cs` |
| `TouchHandsPoseGate.cs` | `Assets/Scripts/TouchHandsPoseGate.cs` |
| `TruckManager.cs` | `Assets/Scripts/TruckManager.cs` |
| `VRTutorial.cs` | `Assets/Scripts/VRTutorial.cs` |
| `VideoProjectorController.cs` | `Assets/Scripts/VideoProjectorController.cs` |
| `VideoTrigger.cs` | `Assets/Scripts/VideoTrigger.cs` |
| `AnimateOnInput.cs` | `Assets/VR Body/AnimateOnInput.cs` |
| `IKFootSolver.cs` | `Assets/VR Body/IKFootSolver.cs` |
| `IKTargetFollowVRRig.cs` | `Assets/VR Body/IKTargetFollowVRRig.cs` |
| `CharacterRetargeterBlockData.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Editor/Native/Scripts/Block/CharacterRetargeterBlockData.cs` |
| `InstallMovementBuildingBlock.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Editor/Native/Scripts/Block/InstallMovementBuildingBlock.cs` |
| `JointAlignmentUtility.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Editor/Native/Scripts/JointAlignmentUtility.cs` |
| `MSDKUtilityEditor.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Editor/Native/Scripts/MSDKUtilityEditor.cs` |
| `MSDKUtilityEditorConfig.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Editor/Native/Scripts/MSDKUtilityEditorConfig.cs` |
| `MSDKUtilityEditorMetadata.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Editor/Native/Scripts/MSDKUtilityEditorMetadata.cs` |
| `MSDKUtilityEditorOverlay.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Editor/Native/Scripts/MSDKUtilityEditorOverlay.cs` |
| `MSDKUtilityEditorPlaybackUI.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Editor/Native/Scripts/MSDKUtilityEditorPlaybackUI.cs` |
| `MSDKUtilityEditorPreviewer.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Editor/Native/Scripts/MSDKUtilityEditorPreviewer.cs` |
| `MSDKUtilityEditorStage.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Editor/Native/Scripts/MSDKUtilityEditorStage.cs` |
| `MSDKUtilityEditorUIConstants.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Editor/Native/Scripts/MSDKUtilityEditorUIConstants.cs` |
| `MSDKUtilityEditorUIFactory.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Editor/Native/Scripts/MSDKUtilityEditorUIFactory.cs` |
| `MSDKUtilityEditorUISections.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Editor/Native/Scripts/MSDKUtilityEditorUISections.cs` |
| `MSDKUtilityEditorWindow.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Editor/Native/Scripts/MSDKUtilityEditorWindow.cs` |
| `NetworkCharacterNGOInstallationRoutine.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Editor/Native/Scripts/Networking/NGO/NetworkCharacterNGOInstallationRoutine.cs` |
| `NetworkCharacterNGOSetupRules.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Editor/Native/Scripts/Networking/NGO/NetworkCharacterNGOSetupRules.cs` |
| `NetworkCharacterSpawnerNGOEditor.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Editor/Native/Scripts/Networking/NGO/NetworkCharacterSpawnerNGOEditor.cs` |
| `NetworkCharacterRetargeterEditor.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Editor/Native/Scripts/Networking/NetworkCharacterRetargeterEditor.cs` |
| `NetworkCharacterSpawnerEditor.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Editor/Native/Scripts/Networking/NetworkCharacterSpawnerEditor.cs` |
| `NetworkCharacterFusionInstallationRoutine.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Editor/Native/Scripts/Networking/PhotonFusion/NetworkCharacterFusionInstallationRoutine.cs` |
| `NetworkCharacterFusionSetupRules.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Editor/Native/Scripts/Networking/PhotonFusion/NetworkCharacterFusionSetupRules.cs` |
| `NetworkCharacterSpawnerFusionEditor.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Editor/Native/Scripts/Networking/PhotonFusion/NetworkCharacterSpawnerFusionEditor.cs` |
| `CharacterRetargeterConfigEditor.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Editor/Native/Scripts/Retargeting/CharacterRetargeterConfigEditor.cs` |
| `CharacterRetargeterEditor.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Editor/Native/Scripts/Retargeting/CharacterRetargeterEditor.cs` |
| `JointPairPropertyDrawer.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Editor/Native/Scripts/Retargeting/JointPairPropertyDrawer.cs` |
| `AnimationSkeletalProcessorDrawer.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Editor/Native/Scripts/Retargeting/Processors/AnimationSkeletalProcessorDrawer.cs` |
| `CCDSkeletalDataPropertyDrawer.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Editor/Native/Scripts/Retargeting/Processors/CCDSkeletalDataPropertyDrawer.cs` |
| `CCDSkeletalProcessorDrawer.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Editor/Native/Scripts/Retargeting/Processors/CCDSkeletalProcessorDrawer.cs` |
| `CustomProcessorDrawer.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Editor/Native/Scripts/Retargeting/Processors/CustomProcessorDrawer.cs` |
| `HandSkeletalProcessorDrawer.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Editor/Native/Scripts/Retargeting/Processors/HandSkeletalProcessorDrawer.cs` |
| `HipPinningDataPropertyDrawer.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Editor/Native/Scripts/Retargeting/Processors/HipPinningDataPropertyDrawer.cs` |
| `HipPinningSkeletalProcessorDrawer.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Editor/Native/Scripts/Retargeting/Processors/HipPinningSkeletalProcessorDrawer.cs` |
| `ISDKSkeletalProcessorDrawer.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Editor/Native/Scripts/Retargeting/Processors/ISDKSkeletalProcessorDrawer.cs` |
| `LocomotionSkeletalProcessorDrawer.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Editor/Native/Scripts/Retargeting/Processors/LocomotionSkeletalProcessorDrawer.cs` |
| `SourceProcessorContainerDrawer.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Editor/Native/Scripts/Retargeting/Processors/SourceProcessorContainerDrawer.cs` |
| `TargetProcessorContainerDrawer.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Editor/Native/Scripts/Retargeting/Processors/TargetProcessorContainerDrawer.cs` |
| `TwistDataPropertyDrawer.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Editor/Native/Scripts/Retargeting/Processors/TwistDataPropertyDrawer.cs` |
| `TwistSkeletalProcessorDrawer.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Editor/Native/Scripts/Retargeting/Processors/TwistSkeletalProcessorDrawer.cs` |
| `ShapePoseDataPropertyDrawer.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Editor/Native/Scripts/Retargeting/ShapePoseDataPropertyDrawer.cs` |
| `SkeletonRetargeterDrawer.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Editor/Native/Scripts/Retargeting/SkeletonRetargeterDrawer.cs` |
| `TargetJointIndexPropertyDrawer.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Editor/Native/Scripts/Retargeting/TargetJointIndexPropertyDrawer.cs` |
| `ComponentScanner.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Editor/Native/Scripts/Telemetry/ComponentScanner.cs` |
| `TelemetryManager.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Editor/Native/Scripts/Telemetry/TelemetryManager.cs` |
| `BodyPoseAlignmentDetectorConfigDrawer.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Editor/Tracking/Scripts/BodyTrackingForFitness/BodyPoseAlignmentDetectorConfigDrawer.cs` |
| `BodyPoseBoneTransformsEditor.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Editor/Tracking/Scripts/BodyTrackingForFitness/BodyPoseBoneTransformsEditor.cs` |
| `BodyPoseControllerEditor.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Editor/Tracking/Scripts/BodyTrackingForFitness/BodyPoseControllerEditor.cs` |
| `EditorTransformAwareness.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Editor/Tracking/Scripts/BodyTrackingForFitness/EditorTransformAwareness.cs` |
| `HelperMenusFace.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Editor/Tracking/Scripts/HelperMenusFace.cs` |
| `MovementSDKProjectSetupTasks.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Editor/Tracking/Scripts/MovementSDKProjectSetupTasks.cs` |
| `InspectorGuiHelper.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Editor/Tracking/Scripts/Utils/InspectorGuiHelper.cs` |
| `VisemeDriverEditor.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Editor/Tracking/Scripts/VisemeDriverEditor.cs` |
| `KnownJointFinder.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Runtime/Native/Scripts/KnownJointFinder.cs` |
| `MSDKUtility.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Runtime/Native/Scripts/MSDKUtility.cs` |
| `MSDKUtilityExtension.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Runtime/Native/Scripts/MSDKUtilityExtension.cs` |
| `MSDKUtilityHelper.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Runtime/Native/Scripts/MSDKUtilityHelper.cs` |
| `INetworkCharacterBehaviour.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Runtime/Native/Scripts/Networking/INetworkCharacterBehaviour.cs` |
| `INetworkCharacterHandler.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Runtime/Native/Scripts/Networking/INetworkCharacterHandler.cs` |
| `INetworkCharacterSpawner.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Runtime/Native/Scripts/Networking/INetworkCharacterSpawner.cs` |
| `NetworkCharacterBehaviourNGO.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Runtime/Native/Scripts/Networking/NGO/NetworkCharacterBehaviourNGO.cs` |
| `NetworkCharacterSpawnerNGO.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Runtime/Native/Scripts/Networking/NGO/NetworkCharacterSpawnerNGO.cs` |
| `NetworkCharacterBehaviourLocal.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Runtime/Native/Scripts/Networking/NetworkCharacterBehaviourLocal.cs` |
| `NetworkCharacterHandler.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Runtime/Native/Scripts/Networking/NetworkCharacterHandler.cs` |
| `NetworkCharacterRetargeter.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Runtime/Native/Scripts/Networking/NetworkCharacterRetargeter.cs` |
| `NetworkCharacterBehaviourFusion.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Runtime/Native/Scripts/Networking/PhotonFusion/NetworkCharacterBehaviourFusion.cs` |
| `NetworkCharacterSpawnerFusion.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Runtime/Native/Scripts/Networking/PhotonFusion/NetworkCharacterSpawnerFusion.cs` |
| `CharacterRetargeter.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Runtime/Native/Scripts/Retargeting/CharacterRetargeter.cs` |
| `CharacterRetargeterConfig.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Runtime/Native/Scripts/Retargeting/CharacterRetargeterConfig.cs` |
| `IKUtilities.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Runtime/Native/Scripts/Retargeting/IK/IKUtilities.cs` |
| `ISourceDataProvider.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Runtime/Native/Scripts/Retargeting/ISourceDataProvider.cs` |
| `MetaSourceDataProvider.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Runtime/Native/Scripts/Retargeting/MetaSourceDataProvider.cs` |
| `SkeletonData.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Runtime/Native/Scripts/Retargeting/SkeletonData.cs` |
| `SkeletonDraw.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Runtime/Native/Scripts/Retargeting/SkeletonDraw.cs` |
| `SkeletonJobs.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Runtime/Native/Scripts/Retargeting/SkeletonJobs.cs` |
| `SkeletonRetargeter.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Runtime/Native/Scripts/Retargeting/SkeletonRetargeter.cs` |
| `SkeletonUtilities.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Runtime/Native/Scripts/Retargeting/SkeletonUtilities.cs` |
| `SourceProcessor.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Runtime/Native/Scripts/Retargeting/SourceProcessor.cs` |
| `SourceProcessorContainer.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Runtime/Native/Scripts/Retargeting/SourceProcessorContainer.cs` |
| `ISDKSkeletalProcessor.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Runtime/Native/Scripts/Retargeting/SourceProcessors/ISDKSkeletalProcessor.cs` |
| `TargetProcessor.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Runtime/Native/Scripts/Retargeting/TargetProcessor.cs` |
| `TargetProcessorContainer.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Runtime/Native/Scripts/Retargeting/TargetProcessorContainer.cs` |
| `AnimationSkeletalProcessor.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Runtime/Native/Scripts/Retargeting/TargetProcessors/AnimationSkeletalProcessor.cs` |
| `CCDSkeletalProcessor.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Runtime/Native/Scripts/Retargeting/TargetProcessors/CCDSkeletalProcessor.cs` |
| `CustomProcessor.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Runtime/Native/Scripts/Retargeting/TargetProcessors/CustomProcessor.cs` |
| `CustomProcessorBehavior.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Runtime/Native/Scripts/Retargeting/TargetProcessors/CustomProcessorBehavior.cs` |
| `HandSkeletalProcessor.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Runtime/Native/Scripts/Retargeting/TargetProcessors/HandSkeletalProcessor.cs` |
| `HipPinningSkeletalProcessor.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Runtime/Native/Scripts/Retargeting/TargetProcessors/HipPinningSkeletalProcessor.cs` |
| `LocomotionSkeletalProcessor.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Runtime/Native/Scripts/Retargeting/TargetProcessors/LocomotionSkeletalProcessor.cs` |
| `TwistSkeletalProcessor.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Runtime/Native/Scripts/Retargeting/TargetProcessors/TwistSkeletalProcessor.cs` |
| `BandwidthRecorder.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Runtime/Native/Scripts/Serialization/BandwidthRecorder.cs` |
| `IPlaybackBehaviour.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Runtime/Native/Scripts/Serialization/IPlaybackBehaviour.cs` |
| `PlaybackFunctions.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Runtime/Native/Scripts/Serialization/PlaybackFunctions.cs` |
| `SequenceFileReader.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Runtime/Native/Scripts/Serialization/SequenceFileReader.cs` |
| `SequencePlaybackManager.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Runtime/Native/Scripts/Serialization/SequencePlaybackManager.cs` |
| `CharacterRetargeterButtonCalibration.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Runtime/Native/Scripts/Utils/CharacterRetargeterButtonCalibration.cs` |
| `FollowTransformDirection.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Runtime/Native/Scripts/Utils/FollowTransformDirection.cs` |
| `HMDRemountRestartTracking.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Runtime/Native/Scripts/Utils/HMDRemountRestartTracking.cs` |
| `ISDKHelper.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Runtime/Native/Scripts/Utils/ISDKHelper.cs` |
| `InspectorButton.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Runtime/Native/Scripts/Utils/InspectorButton.cs` |
| `MirrorTransforms.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Runtime/Native/Scripts/Utils/MirrorTransforms.cs` |
| `DenseMatrix.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Runtime/Tracking/Scripts/A2E/DenseMatrix.cs` |
| `FaceDriver.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Runtime/Tracking/Scripts/A2E/FaceDriver.cs` |
| `FaceRetargeterComponent.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Runtime/Tracking/Scripts/A2E/FaceRetargeterComponent.cs` |
| `FaceTrackingTooltips.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Runtime/Tracking/Scripts/A2E/FaceTrackingTooltips.cs` |
| `IRigLogic.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Runtime/Tracking/Scripts/A2E/IRigLogic.cs` |
| `JSONRigParser.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Runtime/Tracking/Scripts/A2E/JSONRigParser.cs` |
| `Mapper.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Runtime/Tracking/Scripts/A2E/Mapper.cs` |
| `Matrix.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Runtime/Tracking/Scripts/A2E/Matrix.cs` |
| `OVRWeightsProvider.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Runtime/Tracking/Scripts/A2E/OVRWeightsProvider.cs` |
| `Retargeter.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Runtime/Tracking/Scripts/A2E/Retargeter.cs` |
| `RigLogic.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Runtime/Tracking/Scripts/A2E/RigLogic.cs` |
| `SimpleRigLogic.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Runtime/Tracking/Scripts/A2E/SimpleRigLogic.cs` |
| `SparseMatrix.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Runtime/Tracking/Scripts/A2E/SparseMatrix.cs` |
| `WeightsProvider.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Runtime/Tracking/Scripts/A2E/WeightsProvider.cs` |
| `AddComponentsHelper.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Runtime/Tracking/Scripts/AddComponentsHelper.cs` |
| `AutomatedTimer.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Runtime/Tracking/Scripts/BodyTrackingForFitness/AutomatedTimer.cs` |
| `BodyBoneName.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Runtime/Tracking/Scripts/BodyTrackingForFitness/BodyBoneName.cs` |
| `BodyPoseAlignmentDetector.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Runtime/Tracking/Scripts/BodyTrackingForFitness/BodyPoseAlignmentDetector.cs` |
| `BodyPoseBoneTransforms.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Runtime/Tracking/Scripts/BodyTrackingForFitness/BodyPoseBoneTransforms.cs` |
| `BodyPoseController.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Runtime/Tracking/Scripts/BodyTrackingForFitness/BodyPoseController.cs` |
| `BodyPoseRuntimeRecorder.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Runtime/Tracking/Scripts/BodyTrackingForFitness/BodyPoseRuntimeRecorder.cs` |
| `BoneGroup.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Runtime/Tracking/Scripts/BodyTrackingForFitness/BoneGroup.cs` |
| `BoneLink.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Runtime/Tracking/Scripts/BodyTrackingForFitness/BoneLink.cs` |
| `Counter.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Runtime/Tracking/Scripts/BodyTrackingForFitness/Counter.cs` |
| `FitnessCommon.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Runtime/Tracking/Scripts/BodyTrackingForFitness/FitnessCommon.cs` |
| `FullBodySkeletonTPose.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Runtime/Tracking/Scripts/BodyTrackingForFitness/FullBodySkeletonTPose.cs` |
| `OVRBodyPose.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Runtime/Tracking/Scripts/BodyTrackingForFitness/OVRBodyPose.cs` |
| `SkeletalDrawContainer.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Runtime/Tracking/Scripts/BodyTrackingForFitness/SkeletalDrawContainer.cs` |
| `HandDeformation.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Runtime/Tracking/Scripts/HandDeformation.cs` |
| `NormalRecalculator.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Runtime/Tracking/Scripts/NormalRecalculation/NormalRecalculator.cs` |
| `RecalculateNormals.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Runtime/Tracking/Scripts/NormalRecalculation/RecalculateNormals.cs` |
| `Tooltips.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Runtime/Tracking/Scripts/Tooltips.cs` |
| `EnumNamedArrayAttribute.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Runtime/Tracking/Scripts/Utils/EnumNamedArrayAttribute.cs` |
| `VisemeDriver.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Runtime/Tracking/Scripts/Viseme/VisemeDriver.cs` |
| `VertexKey.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Runtime/Tracking/ThirdParty/SchemingDeveloper/VertexKey.cs` |
| `MovementBuildSamples.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Shared/Scripts/Editor/MovementBuildSamples.cs` |
| `MovementPBRShaderGUI.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Shared/Scripts/Editor/MovementPBRShaderGUI.cs` |
| `MovementPackageChecker.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Shared/Scripts/Editor/MovementPackageChecker.cs` |
| `MovementSamplesProjectSetupTasks.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Shared/Scripts/Editor/MovementSamplesProjectSetupTasks.cs` |
| `MovementAudioTrigger.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Shared/Scripts/UI/MovementAudioTrigger.cs` |
| `MovementBodyAnimationToggle.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Shared/Scripts/UI/MovementBodyAnimationToggle.cs` |
| `MovementBodyTrackingFidelityToggle.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Shared/Scripts/UI/MovementBodyTrackingFidelityToggle.cs` |
| `MovementBodyTrackingJointToggle.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Shared/Scripts/UI/MovementBodyTrackingJointToggle.cs` |
| `MovementCharacterSpawnMenu.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Shared/Scripts/UI/MovementCharacterSpawnMenu.cs` |
| `MovementCharacterSwapMenu.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Shared/Scripts/UI/MovementCharacterSwapMenu.cs` |
| `MovementDebugDrawSkeletonMenu.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Shared/Scripts/UI/MovementDebugDrawSkeletonMenu.cs` |
| `MovementSceneLoader.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Shared/Scripts/UI/MovementSceneLoader.cs` |
| `MovementSceneSelectIcon.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Shared/Scripts/UI/MovementSceneSelectIcon.cs` |
| `MovementSuggestBodyTrackingCalibration.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Shared/Scripts/UI/MovementSuggestBodyTrackingCalibration.cs` |
| `MovementToggleIcon.cs` | `Packages/Unity-Movement-78.0.0/Unity-Movement-78.0.0/Shared/Scripts/UI/MovementToggleIcon.cs` |
<!-- AUTO-SCRIPTS-END -->

## Scenes & Chapters

The VR experience is divided into chapters across two historical sites:

**Jetty 1:**
- **C1** — J2 Warden's Lobby (warden experience, prisoner sorting)
- **C2** — J3/J4 The Cell (projection-based storytelling)
- **C4** — J6 Black Visitors Room

**Susan Kruger:**
- **C6** — SK1 The Hold (cargo hold of the ship)

### Test Scenes
Extensive test scenes for prototyping interactions: clap detection, gesture recognition, inventory, prisoner sorting with Meta Movement body tracking, memory projector, and cell projection tests.

## Interaction Design

The VR experience uses a range of interaction methods:
- **Hand tracking** — Meta XR Hands SDK for gesture-based interactions (clap, palm, high-five)
- **Body tracking** — Meta XR Movement SDK for full-body presence
- **Sequenced narrative** — `SequenceBrain` orchestrates interactions in story order with fade transitions
- **Object interaction** — Grab, equip, and inventory systems
- **Projected media** — Video/image projection onto in-scene surfaces for archival content
- **Spatial audio** — Meta XR Audio SDK with ambisonics and room acoustics

## Getting Started

### Prerequisites
- Unity 6 (6.3.8f1 or compatible)
- Meta XR All-in-One SDK v78
- Meta Quest 3 headset
- Android Build Support module
- XR Interaction Toolkit

### Setup
1. Clone the repository
2. Open the project root as a Unity project
3. Import any missing packages via the Package Manager
4. Ensure Meta Quest developer mode is enabled on your headset
5. Build and run to Meta Quest 3

## Team

Created by **UWC Immersive Zone** (UWC Innovation Hub)

## License

See [LICENSE.txt](LICENSE.txt) for details.

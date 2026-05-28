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
    ├── Audio/
        ├── C1/
            ├── Master/
            ├── SFX/
            ├── VO/
        ├── C2/
            ├── Breathing_Temp/
            ├── Fingerprint_Audio/
            ├── Freedom Songs/
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
    ├── Documentation/
    ├── Fonts/
        ├── digital-7/
    ├── InteractionSDK/
    ├── Materials/
        ├── C1_J2_Lobby/
            ├── Materials/
        ├── C2_J3_J4_Cell/
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
        ├── Susan Kruger/
        ├── Warders/
            ├── Warder materials/
        ├── prisoners/
            ├── prisoner 1/
            ├── prisoner 2/
            ├── prisoner 3/
            ├── prisoner 4/
            ├── prisoner 5/
            ├── prisoner 6/
    ├── Media/
        ├── RIM_jetty1_assets/
            ├── 3dContent/
            ├── Images/
            ├── Material/
            ├── Videos/
            ├── scripts/
        ├── Video/
            ├── C1/
            ├── C2/
            ├── RIM Show and Tell/
            ├── Video Tuts/
    ├── MetaXR/
    ├── Models/
        ├── 3D_Scans/
            ├── OPTIMIZED/
            ├── RAW/
        ├── C1_J2_Lobby/
            ├── Characters/
            ├── Environment/
            ├── PoliceBadge/
            ├── Temporary_Characters/
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
        ├── Prisoners/
            ├── Animation Controllers/
            ├── Animations/
            ├── Models_V2/
        ├── Props_1st_Pass/
            ├── Props/
            ├── ToiletPaperWithWriting3DModel/
        ├── Registration office/
        ├── Susan Kruger/
        ├── Textures/
            ├── Chain_Textures/
            ├── Studebaker_v003_ATLAS/
        ├── Warders/
            ├── characters/
    ├── Oculus/
    ├── Plugins/
        ├── Android/
    ├── Prefabs/
        ├── Characters V001/
            ├── Organization/
            ├── body track/
        ├── Interactions/
        ├── Objects/
        ├── Rooms/
        ├── Script Prefabs/
        ├── UI/
        ├── Walking man/
    ├── Prisoner Shaders/
    ├── Resources/
    ├── Ropes and cables Tool/
        ├── OptimizedRopesAndCables/
            ├── Example/
            ├── Material/
            ├── Script/
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
            ├── C1_Final_Pass_TEST_Beta/
            ├── C2_J3_Cell_Rec_Assets/
            ├── Cell_ProjectionTest_Backup/
            ├── MemoryProjectorTest/
            ├── SortPrisonerTest/
            ├── SortPrisonerTest_MetaMovement_Sequenced/
    ├── Scripts/
        ├── Animations/
        ├── Audio and Video/
        ├── GazeInteraction/
        ├── Interaction_Sequencing Framework/
        ├── Interactions/
        ├── InventoryTest/
        ├── Mirror/
        ├── Prisoner/
        ├── Timeline/
        ├── UI/
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
        ├── Textures/
        ├── Warders/
            ├── Warder Cuffs/
            ├── Warder Holster/
            ├── Warder pistol/
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
    ├── body tracking test/
        ├── prefabs/
├── LICENSE.txt/
├── Packages/
        ├── Unity-Movement-78.0.0/
            ├── .Documentation/
            ├── Editor/
            ├── Resources/
            ├── Runtime/
            ├── Samples~/
            ├── Shared/
    ├── com.unity.animation.rigging/
        ├── Documentation~/
            ├── constraints/
        ├── Editor/
            ├── AnimationRig/
            ├── Attributes/
            ├── Effectors/
            ├── Icons/
            ├── InverseSolve/
            ├── Shaders/
            ├── Shapes/
            ├── Utils/
        ├── Runtime/
            ├── AnimationJobs/
            ├── AnimationRig/
            ├── Attributes/
            ├── DocCodeExamples/
            ├── Effectors/
            ├── Utils/
        ├── Samples~/
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
| `ElapsedTtimer.cs` | `Assets/Media/RIM_jetty1_assets/scripts/ElapsedTtimer.cs` |
| `SceneManager.cs` | `Assets/Media/RIM_jetty1_assets/scripts/SceneManager.cs` |
| `UISceneLoader.cs` | `Assets/Media/RIM_jetty1_assets/scripts/UISceneLoader.cs` |
| `CameraMove.cs` | `Assets/Ropes and cables Tool/OptimizedRopesAndCables/Example/Scripts/CameraMove.cs` |
| `PointsAssignExample.cs` | `Assets/Ropes and cables Tool/OptimizedRopesAndCables/Example/Scripts/PointsAssignExample.cs` |
| `RopeEditor.cs` | `Assets/Ropes and cables Tool/OptimizedRopesAndCables/Script/Editor/RopeEditor.cs` |
| `Rope.cs` | `Assets/Ropes and cables Tool/OptimizedRopesAndCables/Script/Rope.cs` |
| `RopeMesh.cs` | `Assets/Ropes and cables Tool/OptimizedRopesAndCables/Script/RopeMesh.cs` |
| `RopeWindEffect.cs` | `Assets/Ropes and cables Tool/OptimizedRopesAndCables/Script/RopeWindEffect.cs` |
| `Boat_movement.cs` | `Assets/Scripts/Animations/Boat_movement.cs` |
| `CargoDoorDriver.cs` | `Assets/Scripts/Animations/CargoDoorDriver.cs` |
| `Coin emmision test.cs` | `Assets/Scripts/Animations/Coin emmision test.cs` |
| `ControlBoatGate.cs` | `Assets/Scripts/Animations/ControlBoatGate.cs` |
| `FadeControllerScript.cs` | `Assets/Scripts/Animations/FadeControllerScript.cs` |
| `MaterialOpacityFader.cs` | `Assets/Scripts/Animations/MaterialOpacityFader.cs` |
| `TokenMove.cs` | `Assets/Scripts/Animations/TokenMove.cs` |
| `TruckManager.cs` | `Assets/Scripts/Animations/TruckManager.cs` |
| `Wheel_rotate.cs` | `Assets/Scripts/Animations/Wheel_rotate.cs` |
| `AudioSourceControl.cs` | `Assets/Scripts/Audio and Video/AudioSourceControl.cs` |
| `CinemachineMirrorTargetBinder.cs` | `Assets/Scripts/Audio and Video/CinemachineMirrorTargetBinder.cs` |
| `FootstepAudioSource.cs` | `Assets/Scripts/Audio and Video/FootstepAudioSource.cs` |
| `FootstepSMB.cs` | `Assets/Scripts/Audio and Video/FootstepSMB.cs` |
| `MAIO_Vid_Controller.cs` | `Assets/Scripts/Audio and Video/MAIO_Vid_Controller.cs` |
| `ProjectionController.cs` | `Assets/Scripts/Audio and Video/ProjectionController.cs` |
| `SimpleMovie.cs` | `Assets/Scripts/Audio and Video/SimpleMovie.cs` |
| `TutorialVideoController.cs` | `Assets/Scripts/Audio and Video/TutorialVideoController.cs` |
| `VideoProjectorController.cs` | `Assets/Scripts/Audio and Video/VideoProjectorController.cs` |
| `VideoTrigger.cs` | `Assets/Scripts/Audio and Video/VideoTrigger.cs` |
| `WalkieTalkieAudioSource.cs` | `Assets/Scripts/Audio and Video/WalkieTalkieAudioSource.cs` |
| `GazeIndicator.cs` | `Assets/Scripts/GazeInteraction/GazeIndicator.cs` |
| `GazeRaycaster.cs` | `Assets/Scripts/GazeInteraction/GazeRaycaster.cs` |
| `GazeTarget.cs` | `Assets/Scripts/GazeInteraction/GazeTarget.cs` |
| `IGazeTarget.cs` | `Assets/Scripts/GazeInteraction/IGazeTarget.cs` |
| `BreathingInteractionModule.cs` | `Assets/Scripts/Interaction_Sequencing Framework/BreathingInteractionModule.cs` |
| `FingerprintProjectionInteractionModule.cs` | `Assets/Scripts/Interaction_Sequencing Framework/FingerprintProjectionInteractionModule.cs` |
| `HandTouchInteractionModule.cs` | `Assets/Scripts/Interaction_Sequencing Framework/HandTouchInteractionModule.cs` |
| `InteractionModuleBase.cs` | `Assets/Scripts/Interaction_Sequencing Framework/InteractionModuleBase.cs` |
| `InventoryModule.cs` | `Assets/Scripts/Interaction_Sequencing Framework/InventoryModule.cs` |
| `PrisonerRoute.cs` | `Assets/Scripts/Interaction_Sequencing Framework/PrisonerRoute.cs` |
| `PrisonerSortData.cs` | `Assets/Scripts/Interaction_Sequencing Framework/PrisonerSortData.cs` |
| `PrisonerSortModule.cs` | `Assets/Scripts/Interaction_Sequencing Framework/PrisonerSortModule.cs` |
| `ReactivateRoutes.cs` | `Assets/Scripts/Interaction_Sequencing Framework/ReactivateRoutes.cs` |
| `RouteHoldSelector.cs` | `Assets/Scripts/Interaction_Sequencing Framework/RouteHoldSelector.cs` |
| `SequenceBrain.cs` | `Assets/Scripts/Interaction_Sequencing Framework/SequenceBrain.cs` |
| `SequenceSignalRouter.cs` | `Assets/Scripts/Interaction_Sequencing Framework/SequenceSignalRouter.cs` |
| `SetNextPrisoner.cs` | `Assets/Scripts/Interaction_Sequencing Framework/SetNextPrisoner.cs` |
| `Background_Character_Spawning.cs` | `Assets/Scripts/Interactions/Background_Character_Spawning.cs` |
| `ClapDetector.cs` | `Assets/Scripts/Interactions/ClapDetector.cs` |
| `FingerprintLockout.cs` | `Assets/Scripts/Interactions/FingerprintLockout.cs` |
| `FingerprintTrigger.cs` | `Assets/Scripts/Interactions/FingerprintTrigger.cs` |
| `HeadLockedHud.cs` | `Assets/Scripts/Interactions/HeadLockedHud.cs` |
| `HighFiveTrigger.cs` | `Assets/Scripts/Interactions/HighFiveTrigger.cs` |
| `HighlightExit.cs` | `Assets/Scripts/Interactions/HighlightExit.cs` |
| `HighlightExitManager.cs` | `Assets/Scripts/Interactions/HighlightExitManager.cs` |
| `InstructionManager.cs` | `Assets/Scripts/Interactions/InstructionManager.cs` |
| `InstructionObject.cs` | `Assets/Scripts/Interactions/InstructionObject.cs` |
| `InteractionCompleteListener.cs` | `Assets/Scripts/Interactions/InteractionCompleteListener.cs` |
| `PalmGestureDetector.cs` | `Assets/Scripts/Interactions/PalmGestureDetector.cs` |
| `ProgressObject.cs` | `Assets/Scripts/Interactions/ProgressObject.cs` |
| `ProgressTracker.cs` | `Assets/Scripts/Interactions/ProgressTracker.cs` |
| `TimedChoiceManager.cs` | `Assets/Scripts/Interactions/TimedChoiceManager.cs` |
| `TouchHandsPoseGate.cs` | `Assets/Scripts/Interactions/TouchHandsPoseGate.cs` |
| `AvatarAttachmentPoint.cs` | `Assets/Scripts/InventoryTest/AvatarAttachmentPoint.cs` |
| `BodyAttachmentRig.cs` | `Assets/Scripts/InventoryTest/BodyAttachmentRig.cs` |
| `EquippableItem.cs` | `Assets/Scripts/InventoryTest/EquippableItem.cs` |
| `InventoryManager.cs` | `Assets/Scripts/InventoryTest/InventoryManager.cs` |
| `SimpleBodyAttachments.cs` | `Assets/Scripts/InventoryTest/SimpleBodyAttachments.cs` |
| `Mirror movement.cs` | `Assets/Scripts/Mirror/Mirror movement.cs` |
| `MirrorFollow.cs` | `Assets/Scripts/Mirror/MirrorFollow.cs` |
| `mirror_movement.cs` | `Assets/Scripts/Mirror/mirror_movement.cs` |
| `PrisonerActorController.cs` | `Assets/Scripts/Prisoner/PrisonerActorController.cs` |
| `Walk_Cycle_modifier.cs` | `Assets/Scripts/Prisoner/Walk_Cycle_modifier.cs` |
| `TimelineBindingTransfer.cs` | `Assets/Scripts/Timeline/TimelineBindingTransfer.cs` |
| `TimelineVideoPlaybackController.cs` | `Assets/Scripts/Timeline/TimelineVideoPlaybackController.cs` |
| `Billboard.cs` | `Assets/Scripts/UI/Billboard.cs` |
| `ButtonScale.cs` | `Assets/Scripts/UI/ButtonScale.cs` |
| `CanvasFade.cs` | `Assets/Scripts/UI/CanvasFade.cs` |
| `ClapConfirmationUI.cs` | `Assets/Scripts/UI/ClapConfirmationUI.cs` |
| `CountdownTimer.cs` | `Assets/Scripts/UI/CountdownTimer.cs` |
| `Overlay.cs` | `Assets/Scripts/UI/Overlay.cs` |
| `PickUpUI.cs` | `Assets/Scripts/UI/PickUpUI.cs` |
| `RewardUI.cs` | `Assets/Scripts/UI/RewardUI.cs` |
| `TenSecTimer.cs` | `Assets/Scripts/UI/TenSecTimer.cs` |
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
| `BlendConstraintEditor.cs` | `Packages/com.unity.animation.rigging/Editor/AnimationRig/Constraints/BlendConstraintEditor.cs` |
| `ChainIKConstraintEditor.cs` | `Packages/com.unity.animation.rigging/Editor/AnimationRig/Constraints/ChainIKConstraintEditor.cs` |
| `DampedTransformEditor.cs` | `Packages/com.unity.animation.rigging/Editor/AnimationRig/Constraints/DampedTransformEditor.cs` |
| `MultiAimConstraintEditor.cs` | `Packages/com.unity.animation.rigging/Editor/AnimationRig/Constraints/MultiAimConstraintEditor.cs` |
| `MultiParentConstraintEditor.cs` | `Packages/com.unity.animation.rigging/Editor/AnimationRig/Constraints/MultiParentConstraintEditor.cs` |
| `MultiPositionConstraintEditor.cs` | `Packages/com.unity.animation.rigging/Editor/AnimationRig/Constraints/MultiPositionConstraintEditor.cs` |
| `MultiReferentialConstraintEditor.cs` | `Packages/com.unity.animation.rigging/Editor/AnimationRig/Constraints/MultiReferentialConstraintEditor.cs` |
| `MultiRotationConstraintEditor.cs` | `Packages/com.unity.animation.rigging/Editor/AnimationRig/Constraints/MultiRotationConstraintEditor.cs` |
| `OverrideTransformEditor.cs` | `Packages/com.unity.animation.rigging/Editor/AnimationRig/Constraints/OverrideTransformEditor.cs` |
| `TwistChainConstraintEditor.cs` | `Packages/com.unity.animation.rigging/Editor/AnimationRig/Constraints/TwistChainConstraintEditor.cs` |
| `TwistCorrectionEditor.cs` | `Packages/com.unity.animation.rigging/Editor/AnimationRig/Constraints/TwistCorrectionEditor.cs` |
| `TwoBoneIKConstraintEditor.cs` | `Packages/com.unity.animation.rigging/Editor/AnimationRig/Constraints/TwoBoneIKConstraintEditor.cs` |
| `RigBuilderEditor.cs` | `Packages/com.unity.animation.rigging/Editor/AnimationRig/RigBuilderEditor.cs` |
| `RigEditor.cs` | `Packages/com.unity.animation.rigging/Editor/AnimationRig/RigEditor.cs` |
| `RigLayerDrawer.cs` | `Packages/com.unity.animation.rigging/Editor/AnimationRig/RigLayerDrawer.cs` |
| `RigTransformEditor.cs` | `Packages/com.unity.animation.rigging/Editor/AnimationRig/RigTransformEditor.cs` |
| `WeightedTransformArrayDrawer.cs` | `Packages/com.unity.animation.rigging/Editor/AnimationRig/WeightedTransformArrayDrawer.cs` |
| `WeightedTransformDrawer.cs` | `Packages/com.unity.animation.rigging/Editor/AnimationRig/WeightedTransformDrawer.cs` |
| `AssemblyInfo.cs` | `Packages/com.unity.animation.rigging/Editor/AssemblyInfo.cs` |
| `BakeParametersAttribute.cs` | `Packages/com.unity.animation.rigging/Editor/Attributes/BakeParametersAttribute.cs` |
| `CustomOverlayAttribute.cs` | `Packages/com.unity.animation.rigging/Editor/Attributes/CustomOverlayAttribute.cs` |
| `InverseRigConstraintAttribute.cs` | `Packages/com.unity.animation.rigging/Editor/Attributes/InverseRigConstraintAttribute.cs` |
| `IRigEffector.cs` | `Packages/com.unity.animation.rigging/Editor/Effectors/IRigEffector.cs` |
| `IRigEffectorOverlay.cs` | `Packages/com.unity.animation.rigging/Editor/Effectors/IRigEffectorOverlay.cs` |
| `RigEffector.cs` | `Packages/com.unity.animation.rigging/Editor/Effectors/RigEffector.cs` |
| `RigEffectorOverlay.cs` | `Packages/com.unity.animation.rigging/Editor/Effectors/RigEffectorOverlay.cs` |
| `RigEffectorRenderer.cs` | `Packages/com.unity.animation.rigging/Editor/Effectors/RigEffectorRenderer.cs` |
| `RigEffectorWizard.cs` | `Packages/com.unity.animation.rigging/Editor/Effectors/RigEffectorWizard.cs` |
| `MultiAimInverseConstraintJob.cs` | `Packages/com.unity.animation.rigging/Editor/InverseSolve/AnimationJobs/MultiAimInverseConstraintJob.cs` |
| `MultiParentInverseConstraintJob.cs` | `Packages/com.unity.animation.rigging/Editor/InverseSolve/AnimationJobs/MultiParentInverseConstraintJob.cs` |
| `MultiPositionInverseConstraintJob.cs` | `Packages/com.unity.animation.rigging/Editor/InverseSolve/AnimationJobs/MultiPositionInverseConstraintJob.cs` |
| `MultiReferentialInverseConstraintJob.cs` | `Packages/com.unity.animation.rigging/Editor/InverseSolve/AnimationJobs/MultiReferentialInverseConstraintJob.cs` |
| `MultiRotationInverseConstraintJob.cs` | `Packages/com.unity.animation.rigging/Editor/InverseSolve/AnimationJobs/MultiRotationInverseConstraintJob.cs` |
| `TwistChainInverseConstraintJob.cs` | `Packages/com.unity.animation.rigging/Editor/InverseSolve/AnimationJobs/TwistChainInverseConstraintJob.cs` |
| `TwoBoneIKInverseConstraintJob.cs` | `Packages/com.unity.animation.rigging/Editor/InverseSolve/AnimationJobs/TwoBoneIKInverseConstraintJob.cs` |
| `MultiAimInverseConstraint.cs` | `Packages/com.unity.animation.rigging/Editor/InverseSolve/Constraints/MultiAimInverseConstraint.cs` |
| `MultiParentInverseConstraint.cs` | `Packages/com.unity.animation.rigging/Editor/InverseSolve/Constraints/MultiParentInverseConstraint.cs` |
| `MultiPositionInverseConstraint.cs` | `Packages/com.unity.animation.rigging/Editor/InverseSolve/Constraints/MultiPositionInverseConstraint.cs` |
| `MultiReferentialInverseConstraint.cs` | `Packages/com.unity.animation.rigging/Editor/InverseSolve/Constraints/MultiReferentialInverseConstraint.cs` |
| `MultiRotationInverseConstraint.cs` | `Packages/com.unity.animation.rigging/Editor/InverseSolve/Constraints/MultiRotationInverseConstraint.cs` |
| `TwistChainInverseConstraint.cs` | `Packages/com.unity.animation.rigging/Editor/InverseSolve/Constraints/TwistChainInverseConstraint.cs` |
| `TwoBoneIKInverseConstraint.cs` | `Packages/com.unity.animation.rigging/Editor/InverseSolve/Constraints/TwoBoneIKInverseConstraint.cs` |
| `AnimationRiggingContextMenus.cs` | `Packages/com.unity.animation.rigging/Editor/Utils/AnimationRiggingContextMenus.cs` |
| `AnimationRiggingEditorUtils.cs` | `Packages/com.unity.animation.rigging/Editor/Utils/AnimationRiggingEditorUtils.cs` |
| `AnimationRiggingMenu.cs` | `Packages/com.unity.animation.rigging/Editor/Utils/AnimationRiggingMenu.cs` |
| `AnimationWindowUtils.cs` | `Packages/com.unity.animation.rigging/Editor/Utils/AnimationWindowUtils.cs` |
| `BakeUtils.cs` | `Packages/com.unity.animation.rigging/Editor/Utils/BakeUtils.cs` |
| `BoneRendererEditor.cs` | `Packages/com.unity.animation.rigging/Editor/Utils/BoneRendererEditor.cs` |
| `BoneRendererUtils.cs` | `Packages/com.unity.animation.rigging/Editor/Utils/BoneRendererUtils.cs` |
| `CommonContent.cs` | `Packages/com.unity.animation.rigging/Editor/Utils/CommonContent.cs` |
| `EditorCurveBindingUtils.cs` | `Packages/com.unity.animation.rigging/Editor/Utils/EditorCurveBindingUtils.cs` |
| `EditorHelpers.cs` | `Packages/com.unity.animation.rigging/Editor/Utils/EditorHelpers.cs` |
| `ExpandChildrenDrawer.cs` | `Packages/com.unity.animation.rigging/Editor/Utils/ExpandChildrenDrawer.cs` |
| `FoldoutState.cs` | `Packages/com.unity.animation.rigging/Editor/Utils/FoldoutState.cs` |
| `Preferences.cs` | `Packages/com.unity.animation.rigging/Editor/Utils/Preferences.cs` |
| `SceneViewOverlay.cs` | `Packages/com.unity.animation.rigging/Editor/Utils/SceneViewOverlay.cs` |
| `Vector3BoolDrawer.cs` | `Packages/com.unity.animation.rigging/Editor/Utils/Vector3BoolDrawer.cs` |
| `AnimationJobCache.cs` | `Packages/com.unity.animation.rigging/Runtime/AnimationJobs/AnimationJobCache.cs` |
| `BlendConstraintJob.cs` | `Packages/com.unity.animation.rigging/Runtime/AnimationJobs/BlendConstraintJob.cs` |
| `ChainIKConstraintJob.cs` | `Packages/com.unity.animation.rigging/Runtime/AnimationJobs/ChainIKConstraintJob.cs` |
| `DampedTransformJob.cs` | `Packages/com.unity.animation.rigging/Runtime/AnimationJobs/DampedTransformJob.cs` |
| `IAnimatableProperty.cs` | `Packages/com.unity.animation.rigging/Runtime/AnimationJobs/IAnimatableProperty.cs` |
| `IAnimationJobBinder.cs` | `Packages/com.unity.animation.rigging/Runtime/AnimationJobs/IAnimationJobBinder.cs` |
| `IAnimationJobData.cs` | `Packages/com.unity.animation.rigging/Runtime/AnimationJobs/IAnimationJobData.cs` |
| `IWeightedAnimationJob.cs` | `Packages/com.unity.animation.rigging/Runtime/AnimationJobs/IWeightedAnimationJob.cs` |
| `MultiAimConstraintJob.cs` | `Packages/com.unity.animation.rigging/Runtime/AnimationJobs/MultiAimConstraintJob.cs` |
| `MultiParentConstraintJob.cs` | `Packages/com.unity.animation.rigging/Runtime/AnimationJobs/MultiParentConstraintJob.cs` |
| `MultiPositionConstraintJob.cs` | `Packages/com.unity.animation.rigging/Runtime/AnimationJobs/MultiPositionConstraintJob.cs` |
| `MultiReferentialConstraintJob.cs` | `Packages/com.unity.animation.rigging/Runtime/AnimationJobs/MultiReferentialConstraintJob.cs` |
| `MultiRotationConstraintJob.cs` | `Packages/com.unity.animation.rigging/Runtime/AnimationJobs/MultiRotationConstraintJob.cs` |
| `OverrideTransformJob.cs` | `Packages/com.unity.animation.rigging/Runtime/AnimationJobs/OverrideTransformJob.cs` |
| `RigSyncSceneToStreamJob.cs` | `Packages/com.unity.animation.rigging/Runtime/AnimationJobs/RigSyncSceneToStreamJob.cs` |
| `TransformHandle.cs` | `Packages/com.unity.animation.rigging/Runtime/AnimationJobs/TransformHandle.cs` |
| `TwistChainConstraintJob.cs` | `Packages/com.unity.animation.rigging/Runtime/AnimationJobs/TwistChainConstraintJob.cs` |
| `TwistCorrectionJob.cs` | `Packages/com.unity.animation.rigging/Runtime/AnimationJobs/TwistCorrectionJob.cs` |
| `TwoBoneIKConstraintJob.cs` | `Packages/com.unity.animation.rigging/Runtime/AnimationJobs/TwoBoneIKConstraintJob.cs` |
| `WeightedTransformArrayBinder.cs` | `Packages/com.unity.animation.rigging/Runtime/AnimationJobs/WeightedTransformArrayBinder.cs` |
| `BlendConstraint.cs` | `Packages/com.unity.animation.rigging/Runtime/AnimationRig/Constraints/BlendConstraint.cs` |
| `ChainIKConstraint.cs` | `Packages/com.unity.animation.rigging/Runtime/AnimationRig/Constraints/ChainIKConstraint.cs` |
| `DampedTransform.cs` | `Packages/com.unity.animation.rigging/Runtime/AnimationRig/Constraints/DampedTransform.cs` |
| `MultiAimConstraint.cs` | `Packages/com.unity.animation.rigging/Runtime/AnimationRig/Constraints/MultiAimConstraint.cs` |
| `MultiParentConstraint.cs` | `Packages/com.unity.animation.rigging/Runtime/AnimationRig/Constraints/MultiParentConstraint.cs` |
| `MultiPositionConstraint.cs` | `Packages/com.unity.animation.rigging/Runtime/AnimationRig/Constraints/MultiPositionConstraint.cs` |
| `MultiReferentialConstraint.cs` | `Packages/com.unity.animation.rigging/Runtime/AnimationRig/Constraints/MultiReferentialConstraint.cs` |
| `MultiRotationConstraint.cs` | `Packages/com.unity.animation.rigging/Runtime/AnimationRig/Constraints/MultiRotationConstraint.cs` |
| `OverrideTransform.cs` | `Packages/com.unity.animation.rigging/Runtime/AnimationRig/Constraints/OverrideTransform.cs` |
| `TwistChainConstraint.cs` | `Packages/com.unity.animation.rigging/Runtime/AnimationRig/Constraints/TwistChainConstraint.cs` |
| `TwistCorrection.cs` | `Packages/com.unity.animation.rigging/Runtime/AnimationRig/Constraints/TwistCorrection.cs` |
| `TwoBoneIKConstraint.cs` | `Packages/com.unity.animation.rigging/Runtime/AnimationRig/Constraints/TwoBoneIKConstraint.cs` |
| `IRigConstraint.cs` | `Packages/com.unity.animation.rigging/Runtime/AnimationRig/IRigConstraint.cs` |
| `IRigLayer.cs` | `Packages/com.unity.animation.rigging/Runtime/AnimationRig/IRigLayer.cs` |
| `OverrideRigConstraint.cs` | `Packages/com.unity.animation.rigging/Runtime/AnimationRig/OverrideRigConstraint.cs` |
| `OverrideRigLayer.cs` | `Packages/com.unity.animation.rigging/Runtime/AnimationRig/OverrideRigLayer.cs` |
| `Rig.cs` | `Packages/com.unity.animation.rigging/Runtime/AnimationRig/Rig.cs` |
| `RigBuilder.cs` | `Packages/com.unity.animation.rigging/Runtime/AnimationRig/RigBuilder.cs` |
| `RigBuilderUtils.cs` | `Packages/com.unity.animation.rigging/Runtime/AnimationRig/RigBuilderUtils.cs` |
| `RigConstraint.cs` | `Packages/com.unity.animation.rigging/Runtime/AnimationRig/RigConstraint.cs` |
| `RigLayer.cs` | `Packages/com.unity.animation.rigging/Runtime/AnimationRig/RigLayer.cs` |
| `RigTransform.cs` | `Packages/com.unity.animation.rigging/Runtime/AnimationRig/RigTransform.cs` |
| `RigUtils.cs` | `Packages/com.unity.animation.rigging/Runtime/AnimationRig/RigUtils.cs` |
| `SyncSceneToStreamLayer.cs` | `Packages/com.unity.animation.rigging/Runtime/AnimationRig/SyncSceneToStreamLayer.cs` |
| `WeightedTransform.cs` | `Packages/com.unity.animation.rigging/Runtime/AnimationRig/WeightedTransform.cs` |
| `WeightedTransformArray.cs` | `Packages/com.unity.animation.rigging/Runtime/AnimationRig/WeightedTransformArray.cs` |
| `AssemblyInfo.cs` | `Packages/com.unity.animation.rigging/Runtime/AssemblyInfo.cs` |
| `ExpandChildrenAttribute.cs` | `Packages/com.unity.animation.rigging/Runtime/Attributes/ExpandChildrenAttribute.cs` |
| `SyncSceneToStreamAttribute.cs` | `Packages/com.unity.animation.rigging/Runtime/Attributes/SyncSceneToStreamAttribute.cs` |
| `WeightRangeAttribute.cs` | `Packages/com.unity.animation.rigging/Runtime/Attributes/WeightRangeAttribute.cs` |
| `CustomPlayableGraphEvaluator.cs` | `Packages/com.unity.animation.rigging/Runtime/DocCodeExamples/CustomPlayableGraphEvaluator.cs` |
| `CustomRigBuilderEvaluator.cs` | `Packages/com.unity.animation.rigging/Runtime/DocCodeExamples/CustomRigBuilderEvaluator.cs` |
| `IRigEffectorHolder.cs` | `Packages/com.unity.animation.rigging/Runtime/Effectors/IRigEffectorHolder.cs` |
| `RigEffectorData.cs` | `Packages/com.unity.animation.rigging/Runtime/Effectors/RigEffectorData.cs` |
| `AffineTransform.cs` | `Packages/com.unity.animation.rigging/Runtime/Utils/AffineTransform.cs` |
| `AnimationRuntimeUtils.cs` | `Packages/com.unity.animation.rigging/Runtime/Utils/AnimationRuntimeUtils.cs` |
| `BoneRenderer.cs` | `Packages/com.unity.animation.rigging/Runtime/Utils/BoneRenderer.cs` |
| `ConstraintsUtils.cs` | `Packages/com.unity.animation.rigging/Runtime/Utils/ConstraintsUtils.cs` |
| `QuaternionExt.cs` | `Packages/com.unity.animation.rigging/Runtime/Utils/QuaternionExt.cs` |
| `Vector3Bool.cs` | `Packages/com.unity.animation.rigging/Runtime/Utils/Vector3Bool.cs` |
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

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
            ├── Master/
            ├── SFX/
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
        ├── Warders/
    ├── Media/
        ├── Audio/
        ├── Video/
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
        ├── Cell/
        ├── Textures/
        ├── Warders/
    ├── Oculus/
    ├── Plugins/
        ├── Android/
    ├── Prefabs/
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
        ├── SampleScene/
        ├── TestScenes/
            ├── BlackWaitingRoomTest/
            ├── C2_J3_Cell_Rec_Assets/
            ├── MemoryProjectorTest/
            ├── SortPrisonerTest/
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
| `FootstepSMB.cs` | `Assets/FootstepSMB.cs` |
| `ElapsedTtimer.cs` | `Assets/RIM_jetty1_assets/scripts/ElapsedTtimer.cs` |
| `SceneManager.cs` | `Assets/RIM_jetty1_assets/scripts/SceneManager.cs` |
| `UISceneLoader.cs` | `Assets/RIM_jetty1_assets/scripts/UISceneLoader.cs` |
| `BarShake.cs` | `Assets/Scripts/BarShake.cs` |
| `Billboard.cs` | `Assets/Scripts/Billboard.cs` |
| `BlinkingOverlay.cs` | `Assets/Scripts/BlinkingOverlay.cs` |
| `CargoDoorDriver.cs` | `Assets/Scripts/CargoDoorDriver.cs` |
| `ClapConfirmationUI.cs` | `Assets/Scripts/ClapConfirmationUI.cs` |
| `ClapDetector.cs` | `Assets/Scripts/ClapDetector.cs` |
| `FadeControllerScript.cs` | `Assets/Scripts/FadeControllerScript.cs` |
| `FingerprintLockout.cs` | `Assets/Scripts/FingerprintLockout.cs` |
| `FingerprintTrigger.cs` | `Assets/Scripts/FingerprintTrigger.cs` |
| `GazeIndicator.cs` | `Assets/Scripts/GazeIndicator.cs` |
| `GazeTarget.cs` | `Assets/Scripts/GazeTarget.cs` |
| `Headlock.cs` | `Assets/Scripts/Headlock.cs` |
| `HighFiveTrigger.cs` | `Assets/Scripts/HighFiveTrigger.cs` |
| `HighlightExit.cs` | `Assets/Scripts/HighlightExit.cs` |
| `HighlightExitManager.cs` | `Assets/Scripts/HighlightExitManager.cs` |
| `InteractionCompleteListener.cs` | `Assets/Scripts/InteractionCompleteListener.cs` |
| `InteractionModuleBase.cs` | `Assets/Scripts/InteractionModuleBase.cs` |
| `InventoryModule.cs` | `Assets/Scripts/InventoryModule.cs` |
| `AvatarAttachmentPoint.cs` | `Assets/Scripts/InventoryTest/AvatarAttachmentPoint.cs` |
| `BodyAttachmentRig.cs` | `Assets/Scripts/InventoryTest/BodyAttachmentRig.cs` |
| `EquippableItem.cs` | `Assets/Scripts/InventoryTest/EquippableItem.cs` |
| `InventoryManager.cs` | `Assets/Scripts/InventoryTest/InventoryManager.cs` |
| `SimpleBodyAttachments.cs` | `Assets/Scripts/InventoryTest/SimpleBodyAttachments.cs` |
| `LookAtUI.cs` | `Assets/Scripts/LookAtUI.cs` |
| `MAIO_Vid_Controller.cs` | `Assets/Scripts/MAIO_Vid_Controller.cs` |
| `MirrorFollow.cs` | `Assets/Scripts/MirrorFollow.cs` |
| `Overlay.cs` | `Assets/Scripts/Overlay.cs` |
| `PalmGestureDetector.cs` | `Assets/Scripts/PalmGestureDetector.cs` |
| `PrisonerRoute.cs` | `Assets/Scripts/PrisonerRoute.cs` |
| `PrisonerSortModule.cs` | `Assets/Scripts/PrisonerSortModule.cs` |
| `ProjectionController.cs` | `Assets/Scripts/ProjectionController.cs` |
| `ReactivateRoutes.cs` | `Assets/Scripts/ReactivateRoutes.cs` |
| `RewardUI.cs` | `Assets/Scripts/RewardUI.cs` |
| `SequenceBrain.cs` | `Assets/Scripts/SequenceBrain.cs` |
| `SequenceSignalRouter.cs` | `Assets/Scripts/SequenceSignalRouter.cs` |
| `SetNextPrisoner.cs` | `Assets/Scripts/SetNextPrisoner.cs` |
| `Timer.cs` | `Assets/Scripts/Timer.cs` |
| `TruckManager.cs` | `Assets/Scripts/TruckManager.cs` |
| `VideoProjectorController.cs` | `Assets/Scripts/VideoProjectorController.cs` |
| `VideoTrigger.cs` | `Assets/Scripts/VideoTrigger.cs` |
| `AnimateOnInput.cs` | `Assets/VR Body/AnimateOnInput.cs` |
| `IKFootSolver.cs` | `Assets/VR Body/IKFootSolver.cs` |
| `IKTargetFollowVRRig.cs` | `Assets/VR Body/IKTargetFollowVRRig.cs` |
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

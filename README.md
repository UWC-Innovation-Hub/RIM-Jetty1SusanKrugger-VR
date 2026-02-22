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

```
├── .github/
│   └── ISSUE_TEMPLATE/               # Issue templates for project management
│       ├── 2d-assets-task.md
│       ├── 3d-assests-task.md
│       ├── audio-task.md
│       ├── bug-task.md
│       ├── doc-task.md
│       ├── pre-prod-task.md
│       ├── unity-development-task.md
│       ├── video-task.md
│       └── xr-test-case.md
│
├── Assets/                            # Unity Project Root (Assets)
│   ├── Animations/                    # Animation clips & controllers
│   │   └── C1/                        # Chapter 1 animations
│   │
│   ├── Audio/                         # Audio assets per chapter
│   │   ├── C1/ – C6/                  # Chapter-specific audio clips
│   │
│   ├── CompositionLayers/             # Meta Quest composition layers
│   │
│   ├── Custom Prefabs/                # Custom reusable prefabs
│   │
│   ├── InteractionSDK/                # Meta Interaction SDK config
│   │
│   ├── InventoryPrefabs/              # Inventory system prefabs & materials
│   │
│   ├── Materials/                     # Materials organised by chapter/zone
│   │   ├── C1_J2_Lobby/
│   │   ├── C4_J5/
│   │   ├── C6_SK1_The_Hold/
│   │   ├── Cell/
│   │   ├── Custom Shaders/
│   │   ├── General/
│   │   └── MAIO_Tests/
│   │
│   ├── Media/                         # Audio & video media files
│   │   ├── Audio/
│   │   └── Video/
│   │
│   ├── MetaXR/                        # Meta XR platform config
│   │
│   ├── Models/                        # 3D models & scans
│   │   ├── 3D_Scans/                  # Photogrammetry scans
│   │   ├── C1_J2_Lobby/
│   │   ├── C2_J3_J4_Cell/
│   │   ├── C4_J6_Black_Visitors_Room/
│   │   └── C6_SK1_The_Hold/
│   │
│   ├── Oculus/                        # Oculus platform integration
│   │
│   ├── Plugins/                       # Android plugins
│   │   └── Android/
│   │
│   ├── Prefabs/                       # Shared prefab assets
│   │
│   ├── Prisoner Shaders/              # Custom shaders for prisoner visuals
│   │
│   ├── RIM_jetty1_assets/             # Jetty 1 specific assets
│   │   ├── 3dContent/
│   │   ├── Images/
│   │   ├── Material/
│   │   ├── Videos/
│   │   └── scripts/                   # ElapsedTimer, SceneManager, UISceneLoader
│   │
│   ├── Samples/                       # SDK samples
│   │   ├── Meta XR All-in-One SDK/
│   │   ├── Meta XR Audio SDK/         # Spatial audio examples
│   │   ├── Meta XR Interaction SDK/   # Hand/controller interaction examples
│   │   ├── Meta XR Movement SDK/      # Body tracking samples
│   │   ├── Scriptable Render Pipeline Core/
│   │   ├── Shader Graph/              # UGUI shaders, feature examples
│   │   ├── XR Hands/                  # Hand tracking visualiser
│   │   └── XR Interaction Toolkit/    # Hands demo, starter assets
│   │
│   ├── Scenes/
│   │   ├── C1_J2_Warden_Experience.unity        # Ch1 — Warden's Lobby
│   │   ├── C2_J3_Cell.unity                     # Ch2 — The Cell
│   │   ├── C4_J6_Black_Visitors_Room.unity      # Ch4 — Black Visitors Room
│   │   ├── C6_SK1_The_Hold.unity                # Ch6 — The Hold (Susan Kruger)
│   │   └── TestScenes/                          # Development & test scenes
│   │       ├── Cell_ProjectionTest.unity
│   │       ├── ClapInteraction.unity
│   │       ├── ConfirmInteraction.unity
│   │       ├── InteractionTest.unity
│   │       ├── InventoryTest.unity
│   │       ├── MAIO_Gesture_Recognition_Template.unity
│   │       ├── MAIO_Test.unity
│   │       ├── MemoryProjectorTest.unity
│   │       ├── SortPrisonerTest.unity
│   │       ├── SortPrisonerTest_MetaMovement.unity
│   │       ├── SortPrisonerTest_MetaMovement_Sequenced.unity
│   │       └── (more test/prototype scenes)
│   │
│   ├── Scripts/                       # Core application scripts
│   │   ├── ClapConfirmationUI.cs
│   │   ├── ClapDetector.cs
│   │   ├── FadeControllerScript.cs
│   │   ├── FingerprintLockout.cs
│   │   ├── FingerprintTrigger.cs
│   │   ├── HighFiveTrigger.cs
│   │   ├── HighlightExit.cs
│   │   ├── HighlightExitManager.cs
│   │   ├── InteractionCompleteListener.cs
│   │   ├── InteractionModuleBase.cs
│   │   ├── InventoryModule.cs
│   │   ├── MAIO_Vid_Controller.cs
│   │   ├── MirrorFollow.cs
│   │   ├── PalmGestureDetector.cs
│   │   ├── PrisonerRoute.cs
│   │   ├── PrisonerSortModule.cs
│   │   ├── ProjectionController.cs
│   │   ├── ReactivateRoutes.cs
│   │   ├── SequenceBrain.cs
│   │   ├── SequenceSignalRouter.cs
│   │   ├── SetNextPrisoner.cs
│   │   ├── VideoProjectorController.cs
│   │   └── InventoryTest/            # Inventory system scripts
│   │       ├── AvatarAttachmentPoint.cs
│   │       ├── BodyAttachmentRig.cs
│   │       ├── EquippableItem.cs
│   │       ├── InventoryManager.cs
│   │       └── SimpleBodyAttachments.cs
│   │
│   ├── Settings/                      # Build profiles & project config
│   │
│   ├── Starter Assets/                # First/third person controller templates
│   │
│   ├── Textures/                      # Texture assets
│   │
│   ├── VR Body/                       # VR body/avatar scripts
│   │   ├── AnimateOnInput.cs
│   │   ├── IKFootSolver.cs
│   │   └── IKTargetFollowVRRig.cs
│   │
│   ├── VRTemplateAssets/              # VR template utilities
│   │
│   ├── XR/                            # XR configuration
│   │
│   ├── XRI/                           # XR Interaction Toolkit config
│   │
│   └── _Recovery/                     # Recovery scene backups
│
├── Packages/                          # Unity package manifest
├── ProjectSettings/                   # Unity project settings
├── .gitignore
├── .gitattributes
└── LICENSE.txt
```

## Key Scripts

### Core Interaction (`Assets/Scripts/`)
| Script | Purpose |
|---|---|
| `SequenceBrain.cs` | Master sequencer — orchestrates the order of interactions and narrative events |
| `SequenceSignalRouter.cs` | Routes signals between interactions and the sequence brain |
| `InteractionModuleBase.cs` | Base class for all interaction modules |
| `InteractionCompleteListener.cs` | Listens for interaction completion to trigger next steps |
| `FadeControllerScript.cs` | Fade-in/out transitions between interactions and scenes |

### Gesture & Hand Interactions
| Script | Purpose |
|---|---|
| `ClapDetector.cs` | Detects hand-clap gestures via Meta XR Hands |
| `ClapConfirmationUI.cs` | UI confirmation feedback for clap detection |
| `PalmGestureDetector.cs` | Detects open-palm gestures |
| `HighFiveTrigger.cs` | Triggers events on high-five gesture |
| `FingerprintTrigger.cs` | Fingerprint-based interaction trigger |
| `FingerprintLockout.cs` | Lockout mechanic tied to fingerprint interaction |

### Prisoner Experience
| Script | Purpose |
|---|---|
| `PrisonerSortModule.cs` | Prisoner sorting interaction — historical classification re-enactment |
| `PrisonerRoute.cs` | Defines prisoner movement routes through the space |
| `SetNextPrisoner.cs` | Advances to the next prisoner in the sorting sequence |
| `ReactivateRoutes.cs` | Re-enables routes after interaction completion |

### Media & Projection
| Script | Purpose |
|---|---|
| `ProjectionController.cs` | Controls projected content on in-scene surfaces |
| `VideoProjectorController.cs` | Video playback on projected surfaces |
| `MAIO_Vid_Controller.cs` | MAIO (Memory Archive Interactive Object) video controller |

### Inventory System (`Assets/Scripts/InventoryTest/`)
| Script | Purpose |
|---|---|
| `InventoryManager.cs` | Manages collected items and inventory state |
| `InventoryModule.cs` | Interaction module for inventory pickup |
| `EquippableItem.cs` | Items that can be equipped by the player |
| `BodyAttachmentRig.cs` | Attaches items to the VR body rig |
| `AvatarAttachmentPoint.cs` | Defines attachment points on the avatar |
| `SimpleBodyAttachments.cs` | Simplified body attachment system |

### Navigation & UI
| Script | Purpose |
|---|---|
| `HighlightExit.cs` | Highlights exit points in the environment |
| `HighlightExitManager.cs` | Manages multiple exit highlights |
| `MirrorFollow.cs` | Mirror camera follows the player's movements |

### VR Body (`Assets/VR Body/`)
| Script | Purpose |
|---|---|
| `IKTargetFollowVRRig.cs` | IK targets follow the VR headset and controllers |
| `IKFootSolver.cs` | Procedural foot placement using inverse kinematics |
| `AnimateOnInput.cs` | Triggers body animations based on controller input |

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

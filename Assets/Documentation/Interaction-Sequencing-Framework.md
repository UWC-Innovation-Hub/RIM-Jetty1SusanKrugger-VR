# Interaction-Sequencing Framework

This document describes the project framework used to hand control from a scene Timeline to a player interaction, then resume the Timeline once the interaction has met its completion requirement. It is intended for developers and designers who need to maintain existing scene gates or author new ones.

## Core Idea

The scene Timeline owns the narrative flow until it reaches an interaction gate. At that gate, a Timeline signal calls `SequenceSignalRouter.EnterInteractionByIndex(index)`. The router selects one configured `InteractionModuleBase`, then asks `SequenceBrain` to enter interaction mode.

`SequenceBrain` handles the shared sequence state:

1. Transition from `InSequence` to `TransitionToInteraction`.
2. Optionally fade out while the scene Timeline is still running.
3. Pause the scene `PlayableDirector`.
4. Activate the selected interaction module.
5. Switch to `InInteraction` and optionally fade back in.
6. Wait for the active module to raise its `Completed` event.
7. Fade out if configured, deactivate the module, return to `InSequence`, and resume the scene Timeline.

This means each interaction module only needs to know how to start, run, and decide when it is complete. It does not need to own the main Timeline.

## Main Components

### `SequenceParent`

[`SequenceParent`](../Prefabs/Script%20Prefabs/SequenceParent.prefab) is the reusable scene prefab for the sequencing system. It packages:

- A main Timeline object with a `PlayableDirector`.
- `SequenceBrain`, which owns the sequence state machine and Timeline pause/resume behavior.
- `FadeController`, used by startup and interaction transitions.
- `SequenceSignals`, which contains Unity's `SignalReceiver`.
- `SequenceSignalRouter`, which maps Timeline signal integer arguments to interaction modules.

Scene instances override the prefab wiring by binding the scene Timeline, signal events, and scene-specific interaction modules.

### `SequenceBrain`

[`SequenceBrain`](../Scripts/SequenceInteractionMechanism/SequenceBrain.cs) is the central coordinator. It tracks four states:

- `InSequence`
- `TransitionToInteraction`
- `InInteraction`
- `TransitionToSequence`

The brain pauses and resumes the main scene `PlayableDirector`, toggles shared behaviours for sequence or interaction mode, and subscribes to the active module's `Completed` event. It also prevents duplicate transitions and queues an exit request if a module completes during the enter transition.

### `SequenceSignalRouter`

[`SequenceSignalRouter`](../Scripts/SequenceInteractionMechanism/SequenceSignalRouter.cs) is the Timeline-facing adapter. Timeline signals call:

```csharp
EnterInteractionByIndex(int moduleIndex)
```

The integer argument must match the module's position in `interactionModules`. The router sets that module as the active interaction on `SequenceBrain`, then calls `EnterInteraction()`.

### `InteractionModuleBase`

[`InteractionModuleBase`](../Scripts/SequenceInteractionMechanism/InteractionModuleBase.cs) is the base class for all gated interactions. It provides:

- `Activate()` and `Deactivate()` lifecycle methods.
- The `Completed` event consumed by `SequenceBrain`.
- Protected `Complete()`, which derived modules call when their completion rule is satisfied.
- Module-local enable/disable and active/inactive toggles.
- Optional interaction-specific `PlayableDirector` playback.
- Optional tutorial video preload and stop behavior.
- Optional timeout auto-completion.
- Optional fog and fade transition settings.

Derived modules should override `Activate()` and `Deactivate()`, call `base.Activate()` or `base.Deactivate()`, and call `Complete()` exactly once the player-facing requirement is satisfied.

The base interaction timeout is a hard whole-module bypass. When it expires, it calls the protected `OnInteractionTimedOut()` hook, whose default implementation calls `Complete()` exactly as the original timeout did. Derived modules may override the hook for module-specific exit work without changing other interactions. Transition fades remain independent and are applied by `SequenceBrain` only when the module's `useTransitionFade` setting is enabled.

## Authoring Workflow

1. Add or reuse `SequenceParent` in the scene.
2. Create or place the required `InteractionModuleBase` subclasses in the scene.
3. Assign those modules to `SequenceSignalRouter.interactionModules` in the order Timeline signals will reference them.
4. Configure each module's local toggles:
   - `enableWhenActive`
   - `disableWhenActive`
   - `activeWhenActive`
   - `inactiveWhenActive`
5. Assign optional interaction Timeline directors, tutorial clips, timeouts, fades, and environment settings.
6. Place Timeline signals at narrative gate moments.
7. In each signal receiver event, call `SequenceSignalRouter.EnterInteractionByIndex(index)` with the correct module index.
8. Ensure the module calls `Complete()` only after the required player action, gesture, selection, or playback sequence has finished.

For new modules, keep the module-specific logic inside the derived class and let `SequenceBrain` own Timeline control. That keeps the handoff contract consistent across scenes.

## Scene Examples

### `C1_Final_Pass_TEST_Beta`

[`C1_Final_Pass_TEST_Beta`](../Scenes/TestScenes/C1_Final_Pass_TEST_Beta.unity) uses the sequencing framework for the warden/lobby flow.

The scene's `SequenceSignalRouter.interactionModules` array is configured as:

| Index | Module | Purpose |
|---:|---|---|
| `0` | `InventoryModule` | Pauses the Timeline while the player equips the required inventory items in order. |
| `1` | `PrisonerSortModule` | Pauses the Timeline while the player sorts prisoners/routes through the cargo scene interaction. |

`InventoryModule` listens to `InventoryManager` equip/unequip events. It tracks ordered item progress, highlights the expected item and attachment points, and calls `Complete()` when the required equipped count is reached.

`PrisonerSortModule` coordinates with `ReactivateRoutes`, `PrisonerRoute`, `RouteHoldSelector`, and `SetNextPrisoner`. Route selection and participant arrival/finish events advance the active batch. The module calls `Complete()` when all required batches or legacy arrivals have finished.

Authoring note: the scene YAML currently contains at least one Timeline signal override passing index `2`, while the router array only contains indices `0` and `1`. Treat that as suspicious wiring during cleanup or Timeline editing; do not document index `2` as a valid C1 module unless a third module is intentionally assigned.

### `C2_Final_Pass`

[`C2_Final_Pass`](../Scenes/TestScenes/C2_Final_Pass.unity) uses the framework for cell-scene interactions with interaction-specific Timelines and tutorial clips.

The scene's `SequenceSignalRouter.interactionModules` array is configured as:

| Index | Module | Purpose |
|---:|---|---|
| `0` | `BreathingInteractionModule` | Guides the player through inhale/exhale pose recognition, calming audio loops, vignette animation, and HUD feedback. |
| `1` | `FingerprintProjectionInteractionModule` | Shows selectable fingerprints, plays response audio, fades selected info out, and completes after all fingerprints are consumed. |
| `2` | `HandTouchInteractionModule` | Reveals hands, waits for player selection, plays projection clips, and completes after all configured hand steps are consumed. |

The scene contains Timeline signals that pass indices `0`, `1`, and `2`. Some indices are reused in multiple Timeline events, which is expected when the same interaction type appears more than once in the narrative flow.

## Module Summaries

### `InventoryModule`

[`InventoryModule`](../Scripts/SequenceInteractionMechanism/InventoryModule.cs) completes when the configured number of items have been equipped. It can enforce item order, reject out-of-order equips, lock correctly placed items, highlight the next item, and highlight compatible attachment points. When `relocateOnInteractionTimeout` is enabled, a whole-interaction timeout uses the same delayed relocation and audio fade as successful completion.

Completion source: inventory equip state reaches `requiredEquippedCount`.

### `PrisonerSortModule`

[`PrisonerSortModule`](../Scripts/SequenceInteractionMechanism/PrisonerSortModule.cs) completes after prisoner sorting requirements are met. It supports legacy arrival counting and session/batch-based sorting through `PrisonerSortSession` and `PrisonerSortBatch`.

Completion source: required legacy arrivals are registered, or all configured session batches are completed.

### `BreathingInteractionModule`

[`BreathingInteractionModule`](../Scripts/SequenceInteractionMechanism/BreathingInteractionModule.cs) completes after the player performs the required number of inhale/exhale cycles. It reads pose recognizer active states, animates a breathing vignette, crossfades loop audio through calm stages, and updates a head-locked HUD counter.

Completion source: `completedBreathCount` reaches `breathsRequired`, followed by the final calm hold/fade sequence.

### `FingerprintProjectionInteractionModule`

[`FingerprintProjectionInteractionModule`](../Scripts/SequenceInteractionMechanism/FingerprintProjectionInteractionModule.cs) completes after all configured fingerprints are selected and consumed. It subscribes to `FingerprintTrigger.SelectionRequested`, hides non-selected fingerprints during playback, waits for response audio, then fades out the selected fingerprint info. Its optional per-item inactivity assistance selects one remaining fingerprint in configured order, waits for that response to finish, then gives the player a fresh interaction window before assisting again.

Completion source: every fingerprint in the linked `FingerprintLockout` has been consumed.

### `HandTouchInteractionModule`

[`HandTouchInteractionModule`](../Scripts/SequenceInteractionMechanism/HandTouchInteractionModule.cs) completes after all configured hand steps have been selected and their projection clips have finished. It can reveal available hands, mute hand audio while active, play clips through `ProjectorController`, and restore scene lighting before completion. Its optional per-item inactivity assistance selects one remaining hand in configured order and restarts the inactivity window only after that hand's projection has finished.

Completion source: all configured hand steps are consumed, or an enabled auto-projection bypass completes.

## Extension Points

The public sequencing contract is intentionally small:

- `InteractionModuleBase.Activate()`
- `InteractionModuleBase.Deactivate()`
- `InteractionModuleBase.Completed`
- `InteractionModuleBase.Complete()`
- `SequenceBrain.SetActiveInteraction(InteractionModuleBase module)`
- `SequenceBrain.EnterInteraction()`
- `SequenceBrain.ExitInteractionResumeTimeline()`
- `SequenceSignalRouter.EnterInteractionByIndex(int moduleIndex)`

When creating a new interaction module, inherit from `InteractionModuleBase`, implement the interaction's runtime logic, and call `Complete()` when the module is done. Avoid pausing or resuming the main scene Timeline inside the module; that responsibility belongs to `SequenceBrain`.

## Verification Checklist

Use this checklist when adding or repairing a gated interaction:

- The scene contains one `SequenceParent`.
- The main scene Timeline is assigned to `SequenceBrain.director`.
- The `SignalReceiver` invokes `SequenceSignalRouter.EnterInteractionByIndex(index)`.
- The signal integer argument exists in `interactionModules`.
- The selected module inherits from `InteractionModuleBase`.
- Module-local objects and behaviours start inactive if `forceInactiveOnAwake` is enabled.
- The module has a clear completion rule and calls `Complete()`.
- Optional interaction Timeline directors are assigned or discoverable as children.
- Tutorial clip indices and timeout settings are intentional.
- Repeated Timeline gates reuse module indices only when the same interaction module should run again.

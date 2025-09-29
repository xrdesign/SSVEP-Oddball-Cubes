# Puzzle & Interaction Systems

This document walks through the gameplay-focused scripts that power the escape-room puzzles and interactive experiences. Use it when tweaking puzzle logic, hooking new interactables, or diagnosing state transitions inside the main rooms.

## Trigger / Action Framework
- **Core types**: `EscapeTrigger`, `EscapeAction` (`Assets/Scripts/Triggers` and `Assets/Scripts/Actions`).
- **Workflow**:
  1. Each action lists one or more trigger components in the `triggers` field.
  2. At `Start`, the action registers itself with every trigger.
  3. Every trigger subclass (position checks, collision volumes, etc.) calls `Trigger()` when its own condition passes. `EscapeTrigger` fan-outs that call to all attached actions.
  4. `EscapeAction.OnTriggerFire` increments a counter and executes `DoAction()` once all registered triggers have fired. `EscapeAction.OnTriggerClear` decrements the counter when a trigger exits.
- **Variants**:
  - `EscapeTransformTrigger` watches a target `Transform` for positional/rotational tolerances (`EscapeUtil.EpsilonEquals`).
  - `EscapeTriggerEnter` monitors an AABB volume defined by `minPosition`/`maxPosition` and a target transform.
  - `EscapeFadeAction` lerps a material color?s alpha.
  - `EscapeSetTransform` animates position/rotation/scale to predefined values using linear interpolation.
  - `EscapeShowVignette` drives NewtonVR?s vignette for locomotion comfort.

### Usage Tips
- Combine multiple triggers (e.g., both handles rotated) to gate a single `EscapeAction`.
- Because `EscapeAction` counts trigger firings, ensure you add every relevant trigger or the action may never fire.
- For one-shot actions set `EscapeTrigger.fireOnce = true` (default) to prevent repeated firings.

## Spinner Lock (Room 1)
- **Scripts**: `NVRDigitSpinner`, `SpinnerUnlocker`, `GradualMover`.
- **Algorithm**:
  - `NVRDigitSpinner` extends `NVRInteractableRotator` and quantizes the Z-angle into six discrete rungs. When physics settle (`angularVelocity.z` between 0.001 and 0.5), it snaps to the closest rung and converts the angle into a symbol via `LETTERLIST`.
  - `SpinnerUnlocker` reads the three spinner letters each `Update()`, compares against `unlockPW`, and once matched:
    - Enables grabbing on the containing `NVRInteractableItem` and disables the spinners.
    - Plays an unlock sound.
    - Calls `GradualMover.UnlockAfter(2.0f)` to translate the locked object to its open position.
- **Gotchas**: `NVRDigitSpinner.GetLetter()` assumes six rungs; adjust `LETTERLIST` if you change the mesh markings.

## Disk Puzzle Manager
- **Scripts**: `EscapeRoom_PuzzleManager`, `GradualMover`, `GlobalTimer` (utility).
- **Flow**:
  - Each disk calls `EscapeRoom_PuzzleManager.unlock(i)` or `lockback(i)` when inserted/removed.
  - The manager flips `unlockBool[i]`, plays LSL markers (`"Correct disk i"` or `"Wrong disk i"`), and re-checks if all entries are true.
  - When every slot is correct and the puzzle hasn?t been solved, the manager:
    - Enables `objToUnlock.CanAttach`, triggers `GradualMover.Unlock()` on the target object, and plays `unlockSound`.
    - Streams `"(1) Unlock All Disk"` over the `TestMarker` LSL outlet.
- **Integration**: Hook puzzle pieces to the manager and ensure the target object has `GradualMover` configured to slide the lock away.

## Button Combination Table
- **Script**: `ButtonPuzzle`.
- **Algorithm**:
  - Maintains a rolling window of the last three button IDs pressed.
  - Each press sends a LSL event through `EscapeRoom_PuzzleManager.SendButton(number)` (if assigned).
  - When the window equals `correctAnswer` (`{3,4,5}` by default), activates the secret table and plays feedback audio.
  - To change the combination, edit `correctAnswer` in the inspector.

## Keyed Drawer & Level Transition
- **Scripts**: `unlockDrawer`, `unlockLevel`.
- **Behavior**:
  - Both listen for collisions with key prefabs. Only the correct key (`key_grey` for drawers, `key_gold` for scene transitions) keeps the key, disables its collider, and toggles the associated rigidbody/scene load.
  - Wrong keys or controller collisions play `lockSound` feedback.
  - Drawer variant parents the key to `KeySnapPosition` and enables the drawer`s `NVRInteractableItem` so it can be pulled open.
  - Level variant triggers `SteamVR_LoadLevel.Begin` once the correct key is inserted.

## Item Search Trials (Room 2)
- **Scripts**: `ItemSpawner`, `HintController`, `HighlightItemController`, `ItemTags`, `GoalDetector`, `RespawnController`.
- **Key Concepts**:
  - `ItemSpawner` holds prefab templates (collected from its children at `Start`). `NextTrial()` resets hint state, fires an LSL event (`"Trial starts."`), and sets `isSpawning`.
  - On the next `Update()`, the spawner:
    - Clears previous items via `HighlightItemController.CleanItems()`.
    - Picks a target template and instantiates it plus `numberOfDistractors` random distractors inside `spawnArea`, using `RandomSpawnUntilNoCollision()` to avoid overlaps.
    - Configures highlighting based on `mode`:
      - `SimilarVolume`: derive a volume band from the target?s renderer bounds.
      - `SimilarName`: set `highlightName` prefix.
      - `SimilarTags`: copy `ItemTags` bitmask.
    - Emits LSL events describing the target selection and location.
  - `HintController` listens for controller use buttons. Hold either `Use` button to clone the current target in front of the player, flag `itemSpawner.hintShown`, and send `Enable/Disable Hint` markers. The clone is made kinematic and rendered even if the original is hidden.
  - `HighlightItemController` collects `ItemTags` under its transform, temporarily hides meshes while hints are displayed, delays reveal by `delayAfterHint`, highlights according to the chosen mode using `Outline` components, and streams a continuous timer via `LSLStreamer.SendTimer`.
  - `GoalDetector` watches the goal volume: if the player delivers the exact instantiated target, it sends `"Received the correct item. Trial ends."` and calls `itemSpawner.NextTrial()`. It currently also emits `"Received the wrong item"` for every trigger; consider guarding that call if you need cleaner logs.
  - `RespawnController` can either reset misplaced objects to their initial coordinates or re-run `RandomSpawnUntilNoCollision` when they hit a `respawnTriggerName` volume.

```mermaid
graph TD
    A[NextTrial invoked] --> B[Set isSpawning true & reset hint flags]
    B --> C[Highlight controller cleans previous items]
    C --> D[Choose target template & instantiate]
    D --> E[Run RandomSpawnUntilNoCollision]
    E --> F[Spawn distractors with collision checks]
    F --> G[Configure highlight mode & Outline filters]
    G --> H[Emit LSL events (target metadata)]
    H --> I[Player searches & grabs objects]
    I --> J{GoalDetector trigger}
    J -->|Correct item| K[Send "Trial ends" marker & call NextTrial]
    J -->|Wrong item| L[Send "Received the wrong item" marker]
```

### Implementation Notes
- `ItemSpawner.CalculateLocalBounds()` currently calls `GetComponentsInChildren<Renderer>()` on the spawner rather than the candidate item; make sure the script?s GameObject has no unrelated renderers or adjust the function to query the argument directly.
- Highlighting expects every spawned prefab to carry `ItemTags` and an `Outline` component (QuickOutline). Add them to new assets before adding to the template list.

## Countdown Bullet Challenge (Room 3)
- **Scripts**: `CountDownTimer`, `Destroy_Bullet`, `Spawn_Bullets_In_Order`, `CountUpBullets`, `AudioManager`.
- **Loop**:
  - `Spawn_Bullets_In_Order` activates three bullet targets at `Start`, deactivates the rest, and pushes their spawn coordinates to the `BulletMarker` stream.
  - `CountDownTimer.Start()` initialises a 10-minute timer, locates the gun (`Colt Prefab`), and preloads audio clips. `StartCountDown()` is called either when the gun is grabbed (`GrabStreamer` hooking into `NVRInteractable`) or via trigger contact.
  - During play the timer swaps audio loops (`clockNormal`, `clockFaster`) and when time expires plays `clockFail`. Success is detected by `Destroy_Bullet.bullets_found == 30`, in which case `clockSuccess` plays and the timer stops.
  - Each bullet collision increments `Destroy_Bullet.bullets_found`; once a visible bullet is collected the spawner activates another random inactive bullet and logs the spawn position.
  - `CountUpBullets` mirrors the gun counter to the UI every frame.

## Grab & Teleport Instrumentation
- **Scripts**: `GrabStreamer`, `GrabStreamerPuzzle`.
- **Behavior**:
  - These components create dedicated LSL streams (`GrabMarker`) and emit events whenever `NVRInteractable` objects are grabbed by left/right controllers or when teleportation starts/ends. Assign them to manager objects to maintain experiment logs.

## Miscellaneous Helpers
- `FollowLogically` / `AttachBonesLogically`: Automatically parent matching bone hierarchies so VR-tracked rigs follow animated skeletons.
- `EscapeLimitTransform`: Attempts to clamp a transform inside min/max bounds. Note that it currently computes but does not reassign the clamped vector?update the script if you rely on it.
- `FadeRemover`: Disables `SteamVR_Fade` on startup to prevent vignette fading when scenes load.

Reference the corresponding scripts in `Assets/Scripts` when extending these systems, and keep the LSL messages/documentation in sync so downstream analytics stay reliable.

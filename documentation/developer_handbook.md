# Developer Handbook

Welcome to the Escape Room VR project. This Unity 2019.3.3f1 experience combines NewtonVR/VRTK interaction layers, SteamVR tracking, eye-tracking integrations, and a collection of cognitive and puzzle-based game loops. Use this handbook to get your development environment ready and to understand where the major systems are defined before diving into the deeper subsystem guides in this folder.

## Environment & Dependencies
- **Unity**: 2019.3.3f1 (`ProjectSettings/ProjectVersion.txt`).
- **XR runtime**: SteamVR/OpenVR via `com.unity.xr.management` and `com.unity.xr.openvr.standalone`.
- **Interaction frameworks**: NewtonVR, VRTK legacy prefabs, SteamVR Interaction System.
- **Eye tracking**: HTC Vive Pro Eye (`ViveSR.anipal.Eye`), aGlass SDK (`aGlassDKII`), optional HP Omnicept (`HP.Omnicept`).
- **Analytics/streaming**: Lab Streaming Layer (LSL) C# wrapper (`Assets/Scripts/LSL.cs`) plus pylsl in companion Python tools.
- **Rendering helpers**: QuickOutline (highlight shader), custom heatmap shader under `Assets/PostProcessing/Utilities/CustomMotionTexture`.

Install the matching vendor SDKs (SteamVR, HTC SRAnipal, NewtonVR) before opening scenes; missing plugins will manifest as compiler errors.

## Repository Layout
| Path | Purpose |
| --- | --- |
| `Assets/Scenes` | Main game scenes (`Room_1_v3`, `Room_2_CogTraining`, `Room_3_v2`, plus archived variants). |
| `Assets/Scripts` | Gameplay, instrumentation, and support scripts (see subsystem docs for details). |
| `Assets/Scripts/CogTraining` | Highlight/heatmap tooling for cognitive training trials. |
| `Assets/Scripts/Actions` & `Assets/Scripts/Triggers` | Generic trigger/action framework used throughout the escape-room puzzles. |
| `Assets/Scripts/Replay` | Frame recorder/replayer built on `UniqueID` components. |
| `Assets/Glia`, `Assets/HTC.UnityPlugin`, `Assets/VRTK`, `Assets/NewtonVR` | Third-party packages kept in-source. |
| `Packages/manifest.json` | Unity package dependencies; keep in sync when upgrading Unity. |
| Root `.py` files | Desktop utilities (audio guidance, keyboard streaming) that publish controller events via LSL. |

## Core Scenes & Focus Areas
| Scene | Description |
| --- | --- |
| `Room_1_v3.unity` | Classic escape-room puzzle set featuring spinner locks, key drawers, and disk puzzles. |
| `Room_2_CogTraining.unity` | Cognitive training room with item search trials, highlight/hint system, and eye-tracking overlays. |
| `Room_3_v2.unity` | Timed bullet-search puzzle using countdown and randomized target spawns. |

Archived scenes under `Assets/Scenes/archive` preserve previous iterations; use them for regression reference but keep new work on the latest variants.

## Build & Play Workflow
1. Open the project with Unity 2019.3.3f1.
2. Import vendor SDK packages if Unity prompts for missing assemblies (SteamVR, NewtonVR, SRAnipal, Omnicept).
3. Open a target scene from `Assets/Scenes`.
4. Press Play with a SteamVR-compatible headset connected. Controller bindings are defined by the included `action.json` manifest and NewtonVR/VRTK scripts.
5. Use the `SteamVR_ScenesLoader` UI (look for the dropdown in the lobby or desktop companion app) to load other rooms when testing.

## Coding Patterns to Know
- **Trigger/Action pairs**: `EscapeTrigger` derivatives detect state changes, queue `EscapeAction` subclasses, and fire once all trigger preconditions are met. Many puzzles simply wire prefabs together with these components.
- **NewtonVR interactables**: Any grab/move object derives from `NVRInteractableItem` or extension scripts like `NVRDigitSpinner`. Verify colliders & rigidbodies are configured, then hook puzzle scripts to the resulting interactions.
- **LSL instrumentation**: Most user-facing events stream to LSL outlets for experiment logging. Keep stream names stable; update the tables in `documentation/data_streams_and_tooling.md` when adding new channels.
- **Data logging**: Several systems write to `C:\EscapeRoomData` (surveys, gaze logs). Ensure that path exists or make it configurable before shipping.

## Next Steps
- For puzzle mechanics and interaction details, see `puzzles_and_interactions.md`.
- For data streaming, eye-tracking, calibration, and tooling workflows, see `data_streams_and_tooling.md`.

Happy hacking!

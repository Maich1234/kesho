# Scenes, characters, animation and smoothness review — 0.5.1

Reviewed 22 October 2026 (Africa/Nairobi), source `9e5975ab3b6b33b9f9c6869456e5cdf40023dd5a`. This is a report-only review. No gameplay/runtime files were changed.

## Decision

The opening is not ready for visual-quality or smoothness sign-off. Its interaction logic has improved, but the scene/character assets and animation remain a procedural prototype. Frame rate, frame pacing, RAM/GPU memory, heat and touch latency are **unmeasured**. Source inspection reveals concrete defects and likely bottlenecks; it cannot establish whether this APK runs smoothly on a particular phone.

Current 0.5.1 [core CI](https://github.com/Maich1234/kesho/actions/runs/39826668069) passed. It runs engine-independent checks and the build-report checker, not Unity rendering, animation or device profiling. The recently added Unity integration tests have no execution evidence available in this review. No fresh 0.5.1 screenshot, Unity render, APK session or profiler capture was supplied/accessible.

The original four-person portrait atlas was visually inspected. It provides illustrated faces, hair detail, expressive poses and a coherent palette. The 3D construction uses shared rounded body parts, coloured materials and simple transforms; it does not reproduce that level of facial/clothing detail. This is an asset/source comparison, not a claim that a current phone render was inspected. The requested Shujaaz-inspired original comic direction and Monopoly-level presentation benchmark remain unverified.

## Current delivery by area

| Area | Source evidence | Assessment |
|---|---|---|
| Scenes | Campus/Arrival share construction; Room/Night share construction; Courtyard is separate; Office/Dispatch share a legacy counter. Current story uses three physical layouts with time variants. | A small connected opening, not a developed city/countryside. Campus has buildings, signs, benches, desks, trees and a kiosk, but no researched landmark assets or lively world routines. |
| Characters | One shared body/face construction; colour, hairstyle and role/position variations. Default campus creates nine people. | Original portraits are more developed than the 3D characters. Distinct anatomy, facial expressions, wardrobe details and matching character performance are not produced. |
| Animation | Direct sine-driven limb/head rotations for walk/talk; actor-facing turns; no Animator, authored clips, skinning, foot constraints or interaction poses in active source. | Suitable as placeholder movement. No sleeping, sitting, studying, working, handing over objects or lip-sync performance. |
| Camera | Position/rotation ease; orthographic size changes immediately. | Camera motion is partly smoothed. Zoom and facing can still jump. Composition/occlusion/readability require actual renders. |
| UI | Safe area, scroll panels, touch buttons, short lines, 0.125-second fades, sound cues and captured overlay return callbacks. | Useful interaction foundation. Rebuilding controls, lost customisation scroll state and fade/input behaviour still need polish. |
| Performance | 30 fps target, 0.85 render scale, 2x MSAA, shared meshes/materials, static scenery combined by colour, small-grid BFS. | Reasonable starting choices. They do not establish a sustained 30 fps result. No measured quality tiers or GPU/thermal profile. |
| Sound | Four OGG assets, synthesized foley/campus loop, pooled effect sources and dialogue ducking. | No recorded story voice assets; natural dialogue, real crowd ambience and sleeping performance are absent. Volume/persistence handling needs correction. |

## Priority findings

“Confirmed source” means the code or geometry establishes the behaviour. Performance impact and visible severity still require runtime measurements. P1 is the next polish package; P2 is subsequent improvement, not proof of an observed frame-rate failure.

| ID | Priority / confidence | Finding and player impact | Required repair / evidence |
|---|---|---|---|
| CHAR-01 | P1, geometry reproduced | Every leg's lower mesh ends at local y=-0.565 while its shoe ends at y=-0.725 on top. There is a **0.16 model-unit gap** at rest. Both are children of the same leg transform, so joint rotation does not join them. Source: `ComicActor.Limb`. | Repair proportions/overlap or replace with an intentional connected rig. Inspect standing/walking characters from gameplay and close-up angles. |
| CAM-01 | P1, confirmed source | `ResetCamera`, `Conversation`, `PresentSpeaker` and `Preview` immediately assign sizes 9.7, 5.7, 4.4 and 3.4. `LateUpdate` eases position/rotation only. Framing therefore changes scale instantly during an otherwise eased move. `Face` also turns characters instantly. | Ease camera size alongside framing; blend facing where appropriate; reduced motion must use a deliberate immediate transition. Record approach, conversation, self/phone and exit transitions. |
| UI-01 | P1, confirmed source | Each appearance/background/mode button calls `CharacterForm`, which recreates its scroll panel without saving scroll position or focus. Choosing a hairstyle halfway down can jump back to the top. | Update values/preview in place or preserve scroll/selection. Verify repeated appearance changes and name/keyboard handling on a narrow landscape phone. |
| PERF-01 | P1, synchronous path confirmed; timing unmeasured | Autosave every 20 seconds and decision/checkpoint saves run on the main thread. They serialize/validate, hash, write, `Flush(true)`, read back and validate files. A decision can also write its checkpoint and prepare/save a following director moment. | Profile save spikes first. Preserve durability, ordering and recovery while moving suitable disk work to an ordered writer using immutable snapshots. Do not remove checksums/flush guarantees simply to hide latency. Test interruption and storage failure again. |
| PERF-02 | P1, allocation path confirmed; timing unmeasured | Every `StoryHud.Open` disables/destroys the previous hierarchy and creates a new scroll view, masks, layout, labels, buttons, effects and fade. Every dialogue advance repeats this. `RefreshObjective` also recreates an action control; story reconfiguration recreates the destination marker and reapplies appearance. | Reuse stable controls/panels, update content and pool only where justified. Preserve button de-duplication and overlay return semantics. Profile allocations, layout/canvas rebuilds and frame-time peaks during rapid advance. |
| PERF-03 | P1, static construction estimate | Default campus character construction yields **276 separate mesh-renderer parts** across nine people, approximately **52,992 submitted character triangles**, including shadows and degenerate cap triangles. Static scenery combining does not merge the actors. This is not a measured draw-call count. | Frame Debugger/Profiler capture before choosing actor mesh merging, skinning or instancing. Reduce unnecessary renderer fragmentation while retaining appearance and animation. Do not optimize triangle count alone. |
| AUDIO-01 | P1, confirmed source; playback not captured | `SetVolume(0)` stores the preference but music fades via `MoveTowards`; it is not immediately silenced. At the default steady idle music level 0.1105, the authored fade rate takes about **0.368 seconds** to reach zero. Music/ambience also start before their source volumes are explicitly initialized, creating a separate startup-mute risk. | Apply saved levels before Play, make mute immediate across active sources, and test muted cold launch and slider-to-zero during playback. Keep gentle ducking for ordinary conversations separately. |
| PERF-04 | P1, confirmed source; timing unmeasured | Each volume-slider value event calls `PlayerPrefs.Save`, causing repeated synchronous preference persistence while dragging. | Update the mix immediately but persist on deliberate commit/debounce/pause, preserving the final setting. Profile the drag and background/relaunch. |
| NAV-01 | P2, source arithmetic reproduced | NPC occupancy is never added to `Blocked`. A valid campus route `(3,3) → (4,3) → (5,3)` crosses Kito's standing position `(4,3)`. The grid avoids selected furniture, not people. | Add appropriate actor occupancy/separation or route steering; preserve reachable interaction approach points. Verify crossing crowded areas without body overlap or new blocked paths. |
| ANI-01 | P1 quality gap, confirmed source | Walk and talk states directly replace limb rotations from global time. Stopping sets the stride to zero immediately; talking uses the same repeating head/arm gesture. No transition blending or foot planting exists; footstep playback uses an independent timer. | Add a small coherent animation set/state blend, stride/contact-synchronized sounds and distinctive restrained gestures. Record idle/walk/stop/turn/talk and one actual sit/sleep interaction. |
| LIGHT-01 | P2, confirmed shader/source mismatch | Stage changes sun colour/intensity and ambient light, but `Comic.shader` uses only main-light direction, base colour, rim darkening and global exposure. It does not consume `light.color` or ambient values and has no ShadowCaster pass. The morning fix still changes sky/window/exposure, but component-level light tests cannot establish warm lighting or cast shadows on these surfaces. | Decide the intended comic lighting model, then implement/test the relevant light/shadow behaviour. Compare real morning/evening/night renders rather than checking light components alone. |
| PERF-05 | P2, source path confirmed; timing unmeasured | `LoadSceneAsync` loads scenes, but all mesh/actor/sign construction and scenery combining execute synchronously in `Stage.Awake`. Old parts are created and destroyed during the load. Async scene loading does not make this construction asynchronous. | Profile first usable frame and repeated transitions; consider authored reusable prefabs/prebuilt meshes and an appropriate loading transition. Measure peak memory, not just steady state. |
| PERF-06 | P2, source overhead; magnitude unmeasured | Each actor repeatedly looks up `StreetWalker` and reduced-motion preferences and writes idle rotations. Stage checks preferences each frame; HUD writes safe-area anchors every frame. Cue playback repeatedly looks up resources. | Cache stable references/assets/settings, update layout on actual changes, and avoid unnecessary idle work. Confirm measured benefits; nine NPCs alone do not prove a CPU bottleneck. |
| PERF-07 | P2, startup path confirmed; timing unmeasured | `Soundscape.Awake` synthesizes roughly 314,212 mono samples, including a 12-second campus loop, before creating the HUD. Noise/trigonometric loops, arrays and AudioClip population all run synchronously during startup. | Profile cold launch. Consider prebuilt original/licensed ambience or preparation behind an explicit loading state; measure CPU and peak memory before changing it. |
| UX-02 | P2, confirmed source / runtime test needed | Every new panel starts its fade at alpha zero while CanvasGroup interaction remains enabled. Generic action/Pause/cast controls do not all share the ordinary button debounce. A failed free-walk `WalkTo` can be ignored without feedback. | Test rapid taps through changing panels; prevent invisible actionable transitions and unintended double advances. Give brief feedback for invalid destinations. Preserve responsive deliberate input. |

## Reproducible source probes

Run:

```sh
python3 tools/review/presentation_probe.py
```

[Recorded output](PRESENTATION_PROBE_0_5_1.json) includes the latest commit affecting the probed runtime files, leg/shoe bounds, per-character mesh counts, authored camera sizes, the example actor-crossing route and the conditional music fade calculation. It intentionally leaves FPS, frame times, draw calls, memory and thermal measurements null.

The probe reads relevant actor dimensions and construction counts from source. Its campus occupancy example explicitly reproduces the current obstacle helper parameters; it is not a Unity pathfinding test. Its renderer/triangle calculation assumes default hairstyles and active authored mesh parts, excludes scenery/UI, and does not account for GPU culling/batching or future appearance changes. The 192-triangle rounded mesh contains degenerate pole triangles, which the estimate includes.

## Improvements to retain

- Shared basic meshes and a colour-material palette avoid repeatedly creating unique geometry/materials for every body part.
- Scenery combines by material; audio effects reuse six sources; mesh/material cleanup is present. No memory leak was reproduced in this review.
- Tap movement uses a small bounded grid, cancels pending arrival callbacks when interrupted, and checks UI interaction before world movement.
- Speaker identities, phone/off-screen rules, authored time of day, private knowledge and nested screen return paths now have explicit code/tests. The previous defects should not be presented as unchanged.
- Core saves retain checksums, recovery and once-only commands. Performance work must preserve those guarantees.

## Required runtime review

Use the pinned editor, a development build for diagnosis and a comparable release build for player-facing performance. At minimum record the actual model/SoC/GPU/OS of the user's 6 GB Android phone and test a second device with a different GPU when available. RAM capacity alone does not establish graphics performance.

| Capture | Test route | Evidence to keep |
|---|---|---|
| Visual/animation | Character creation, every hairstyle, campus walk/turn/stop, all speakers, phone, courtyard, room, sleep/morning | Landscape video and matching screenshots at exploration/close-up distances; inspect connected limbs, feet, body overlap, face readability and camera snaps. |
| UI | Repeated customisation, long text/choices, rapid advance, settings/journal nesting, Back, name keyboard, safe areas | Video/touch observations; scroll preservation, visible feedback, no clipped/overlapping controls or unintended advances. |
| CPU/GC | Walk, advance dialogue, drag volume, autosave, decisions/checkpoints, 20 repeated scene transitions | Unity Profiler capture with frame-time/GC/allocation timeline, marked saves/scene builds; identify causes before changing architecture. |
| Rendering | Campus all characters, conversation close-up, room night/morning | Frame Debugger events/draw calls, batches/material changes, visible triangles, shader/texture state and actual shadows. |
| Memory/loading | Cold start, warm scenes, repeated transitions/customisation | Load-to-usable time, peak/steady managed/native/GPU memory, growth after cycles, texture/audio import footprint. Source PNG/OGG size is not runtime memory. |
| Sustained play | At least 20 minutes of representative play | Frame-time percentiles, stalls, battery/thermal trend and degradation compared with warm-up. |
| Sound | Saved mute cold launch, unmute, slider-to-zero, dialogue ducking, pause/resume | Audible capture and confirmation of correct levels; no claim that placeholder sounds are natural recordings. |

Proposed acceptance targets, not measured results: sustain the agreed 30 fps on selected devices (33.3 ms frame budget); give visible touch feedback within roughly 100 ms; avoid recurring >50 ms interactive stalls; retain readable controls/characters under all authored lighting; show no persistent memory growth after repeated scene cycles. Loading and initial creation need a visible transition and a measured budget after a baseline exists. Performance captures should report median/p95/p99 and worst spikes rather than average FPS alone.

Unity's official [Profiler overview](https://docs.unity3d.com/6000.3/Documentation/Manual/Profiler.html) and [Frame Debugger introduction](https://docs.unity3d.com/6000.3/Documentation/Manual/FrameDebugger.html) describe the tools for examining execution and rendering. They do not supply measurements for Kesho.

## Recommended next work package

1. Fix the disconnected legs, immediate camera zoom, customisation scroll reset and mute/preference-write handling.
2. Reuse the dialogue/HUD surfaces and profile save/load work while preserving transactional save safety.
3. Establish one connected, expressive character with blended movement/gesture and an actual seated/sleeping pose; inspect it in a fully reviewed campus scene.
4. Capture actual device CPU/render/memory data, then reduce actor renderer fragmentation and tune quality based on evidence.
5. Use this approved character/scene/UI standard for college expansion. More story nodes will not resolve these presentation issues.

No numeric smoothness score, current FPS, GPU bottleneck, final visual fidelity or Monopoly-equivalence claim is justified from the evidence available here.

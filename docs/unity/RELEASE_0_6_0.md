# Kesho 0.6.0 — presentation repair loop

This package implements source fixes from the [0.5.1 presentation review](SCENES_ANIMATION_PERFORMANCE_REVIEW_0_5_1.md). Android build code is 6. The existing opening saves and director schema remain compatible. This is still the college opening, not the promised full 24-hour lifetime.

## Brand asset

The original repository's Kesho K/sun/path mark now has a 1024px Unity launcher icon. `ProjectSetup.Configure` assigns the default player icon and imports it without mipmaps or texture compression. Android uses that icon on the next configured build. Native Android does not use browser favicons.

`Assets/WebGLTemplates/Kesho` supplies the same mark as SVG, 16/32/48px ICO and 192px PNG, with favicon/apple-touch links, a loading indicator and accessible failure message. Configure selects `PROJECT:Kesho` for future browser exports. No browser export or hosted deployment has been produced here. Android is the active build target. Browser autosave retains a synchronous fallback because the ordinary WebGL target has no worker threads.

## Report disposition

“Implemented” below describes source work; Unity rendering/device acceptance is a separate gate. No item has been declared visually approved from source checks alone.

| Finding | Change | Remaining evidence/work |
|---|---|---|
| CHAR-01 | Leg sleeve now overlaps the shoe by 0.125 model units instead of leaving a 0.16-unit gap. | Inspect standing and walking from gameplay/close-up views. |
| CAM-01 | Zoom targets blend with camera position/rotation; conversation facing blends. Reduced motion snaps deliberately on the next update. | Record every framing transition and inspect occlusion. |
| UI-01 | Customisation buttons update labels/preview in place. Name input and scroll hierarchy remain intact. | Phone keyboard and narrow landscape touch review. |
| PERF-01 | Periodic autosave serializes an immutable string on the owner thread and runs verified disk writes on one background worker. Backlog is bounded; critical saves/reads drain earlier work before proceeding. Errors are surfaced. Checksums, readback, durable flush and backups remain. | Critical decision/checkpoint/pause saves deliberately remain synchronous for durability. Profile their frame cost before changing decision flow. |
| PERF-02 | Reuse scroll panel/mask/layout; pool dialogue text/buttons; replace listeners on reuse; reuse action control and same-stage marker; skip identical appearance updates. | Measure canvas rebuilds/GC during rapid dialogue and nested overlays. Settings fields/cast cards are infrequent transient controls. |
| PERF-03 | Merge static pieces per animated rigid joint/material; cache five hair meshes and release owned meshes with the actor. Default authored estimate falls from 276 to 153 character renderers, including nine ground shadows. | Estimate is not draw calls or measured speed. Triangle count remains about 52,992; combined meshes trade renderer count for native mesh memory. Capture both rendering and memory on target hardware. |
| AUDIO-01 | Set saved levels/mute before playback; master zero immediately mutes every source and sets its volume to zero. | Muted cold launch, dragging, ducking and background/resume audible checks. |
| PERF-04 | Persist slider values after 0.5 seconds without changes or on pause/destruction. Mix changes remain immediate. | Relaunch/background persistence on device. |
| NAV-01 | Reserve NPC cells in walkability while retaining separate interaction approach points; invalid free-walk taps give feedback. | Execute all destination tests and inspect crowded routes. Static NPCs only; moving crowds will need occupancy updates/steering. |
| ANI-01 | Distance-driven stride and step cues share a step distance; walk/talk transitions and facing blend; talking gestures vary by actor offset; settled idle actors stop writing rotations. Sleep uses a basic actual lying pose on the room bed. | Partial quality improvement: still rigid procedural limbs, no authored skeletal clips, foot IK, lip sync, seated/study/work performances or professional character animation. Bed pose needs rendered inspection. |
| LIGHT-01 | Comic surfaces now use explicit morning/evening/night light and ambient tints and key-light strength as well as exposure/direction. The intended mobile style retains simple ground-contact blobs rather than adding expensive real-time shadow passes without profiling. | Partial: no cast-shadow implementation; inspect lighting/contrast in actual renders before choosing further shadow features. |
| PERF-05 | Visible location-loading overlay yields a frame before asynchronous loading; add named scene-construction/combining Profiler samples. | Partial: procedural `Awake` construction remains synchronous. Need measured cold/warm load, peak memory and first usable frame before replacing it with prefabs/prebuilt meshes. |
| PERF-06 | Cache walker/settings/audio lookups, skip stable safe-area writes, settle idle rotations, cache unchanged appearances. | Runtime CPU/allocation comparison. |
| PERF-07 | Bake original paper/bedding/campus placeholders to OGG offline; remove synthesis from audio startup. Reproducer: `python3 tools/assets/bake_foley.py` (ffmpeg required). | Clips remain synthesized placeholders. Natural dialogue/crowd/sleep recordings and their rights/performance work remain outstanding. |
| UX-02 | Fade blocks panel interaction until visible; shared 200ms click guard covers ordinary/Pause/cast/action controls; pooled callbacks are replaced; invalid destinations show a hint. | Real touch/rapid tapping, Back/overlay nesting and accessibility review. |

## Verification loop performed

1. Inspected the report and actual runtime code; implemented the repairs above.
2. Compiled and executed engine-independent checks: 1,553,552 story assertions / 60,984 paths, 3,932 director checks, 154 presentation checks, 12 real-file recovery checks, plus 8 new background-writer checks (ordering, bounded work, observable failure and recovery).
3. Ran seven Python tests covering Unity report gates and icon/template/audio container integrity. Decoded baked clips with ffprobe: campus 12s, bedding 1.8s, paper approximately 0.45s. Viewed the exported 1024px icon.
4. Re-ran the source geometry/presentation probe. It proves overlap and the source occupancy repair, estimates renderer fragmentation and leaves all device metrics null. Its source hashes identify the exact files. Historical 0.5.1 output is retained; check out that revision to reproduce the old probe.
5. Reviewed reuse/lifecycle paths, corrected same-stage marker ownership, prevented inactive pooled callbacks from receiving duplicate taps, retained ordered durability, protected the sleep pose from conversation turning, and aligned old interaction tests with visible-panel timing. Added five Unity PlayMode regressions plus two EditMode JSON/worker-ordering and storage-failure checks. Re-ran local checks after corrections.

A Roslyn parser scanned all 23 Unity C# sources with zero syntax errors. It lacks Unity/NUnit assemblies and therefore is **not** a Unity compilation or test result. The Unity shader, editor APIs and IL2CPP must pass the licensed build environment.

Background JSON validation uses plain data classes with `JsonUtility.FromJson(string)`, which Unity documents as callable on background threads: [Unity 6000.3 API](https://docs.unity.com/en-us/engine/6000.3/script-reference/unityengine/jsonutility/fromjson). No Unity scene objects, `PlayerPrefs`, HUD calls or mutable game state enter the writer task.

## Next acceptance loop

Run the configured Unity Build Automation build from current `main`, retaining pre-export method `Kesho.Editor.ProjectSetup.Configure`. Run both Unity test suites and require a successful test report before accepting an APK. No activated Unity editor or phone is available in this workspace, so that gate could not be run here.

Then inspect icon → character customisation → campus walking/conversations → phone → courtyard → room → sleep/morning → journal/settings/checkpoint → pause/relaunch. Repeat scene/customisation cycles and test 20 minutes of representative play on the actual 6GB Android phone. Capture frame-time median/p95/p99, save spikes, draw calls, peak/steady RAM and temperature trend. Proposed target remains sustained 30fps; no current FPS is claimed. Fix any failures from those captures and rerun the same route.

Production work still needed: matching high-quality scene/character assets, authored interaction animation and natural sound/voices. The city/countryside, later life stages and full lifetime content remain outside this presentation repair. There is no evidence to declare “everything is OK” on a phone yet.

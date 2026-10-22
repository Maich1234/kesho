# Unity implementation validation

Status: **source and core logic delivered; Unity editor/runtime validation pending.**

## Executed in this workspace

- .NET SDK 8.0.408 compiled the actual engine-independent C# core with warnings as errors.
- Core suite passed **2,432,811 assertions over 241,920 completed branch paths** across all three backgrounds.
- Coverage includes every authored node, affordable choices, no blocked critical paths, one-time commands, copied checkpoint state, cross-life rewind rejection, permanent-mode rewind rejection, malformed state rejection, deterministic reruns, both seeded accepted-deal outcomes, branch-specific night lines, and viable work/education continuations.
- Each of the 65 dialogue lines has a recording ID and stays within the current short-line budget. No node exposes more than three authored decisions.

This tests the story engine, not whether a player enjoys the scene or can reach a target in Unity. Large assertion counts come from exhaustive branching, not millions of independent product features.

## Added but not executed here

- Unity EditMode tests for Unity JSON deserialization and save/backup recovery.
- Unity PlayMode route tests for every authored scene destination.
- Editor URP/mobile configuration and Android export entry point.
- Seven Unity scene assets and runtime blockout construction, navigation, uGUI, audio playback and lifecycle code.
- A manual Android workflow requiring an activated `kesho-unity` runner.

## Required before claiming a playable Unity APK

1. Import pinned editor/packages and compile all runtime/editor/test assemblies.
2. Run Unity EditMode and PlayMode route tests; add real-device touch tests after import.
3. Play every scene target and transition, including Room → Night, affordable routes and interrupted conversations.
4. Verify real touch controls, scroll panels, keyboard dismissal, safe areas and Android Back.
5. Exercise corrupt/missing/backup saves, new-life overwrite confirmation, same-life checkpoint branches and background/resume.
6. Build/sign development Android APK; inspect package permissions and install on representative devices.
7. Inspect frame pacing, memory, heat and loading. Target 30 fps; currently unmeasured.
8. Record and integrate performances/foley; check subtitles and audible mix on phone speaker/headphones.
9. Observe first-time playtests for navigation, comprehension, tension and duration. Current opening is not claimed to last 45–60 minutes; full 24-hour lifetime content is not implemented.

## Honest delivery limits

No activated Unity editor is available in the execution workspace. No Unity APK, visual screenshot or successful Unity compilation is claimed. No trained ML model, Supabase connection, online economy or multiplayer session is active. The core's pacing helper and dialogue guard are bounded foundations, not a substitute for those systems or their evaluation.

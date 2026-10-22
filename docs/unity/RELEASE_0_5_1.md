# Release 1 — opening repairs, 0.5.1

Source implementation of the approved first package. Android version code 5; package ID, story IDs, main save version and save location remain unchanged. No lifetime expansion or new online dependency is introduced. Unity/cloud/device verification is still required before calling this an accepted phone release.

## Changes

| Problem | Implemented repair | Evidence available here |
|---|---|---|
| Private note disclosed to Kito | Director choices carry an explicit disclosure rule. Private preservation creates only the player's incident/journal history. Director schema 2 migrates schema 1 by removing the incorrect Kito fact while retaining valid knowledge, trust, obligations, incident history and life identity. | Core checks cover every option's disclosure, JSON persistence and a legacy save containing both private and legitimate facts. Actual Unity JsonUtility migration check added. |
| Kito description outside campus | Descriptions are keyed by scene and target; courtyard, room and campus inspections have separate text. A courtyard contact point is a place, not an invented person. | Core location checks; courtyard inspection/settings restoration PlayMode test added. |
| Wrong speaker animates | Presentation resolves authored speaker tokens, not formatted names or interaction targets. Clerk, support, kiosk owner and a queue student are bound to distinct actors. Self, phone, off-screen and written lines have separate presentation rules. Waiting for a response, settings, results and journal stop speaking gestures. | Core tests cover all main-story lines, player-name independence, a Rehema line at Kito's target, queue speakers and remote/off-screen lines. Unity actor-binding test added. |
| Result lost through settings/journal | Each panel retains its rendering callback and each overlay captures its own return destination. Nested settings/journal screens restore the previous view without replaying a transaction. Inspection and character-selection/confirmation panels also retain their own return targets. | Actual GameSession PlayMode test added: pay C10, duplicate callback, result → settings → journal → settings → back through each layer → same result → main story. Tests use isolated temporary saves. |
| Morning scene looks like night | Every authored beat specifies time of day. Stage applies daylight/ambient/sky/window/exposure from that beat. The legacy scene ID `Night` can present morning, preserving older save locations. | All 29 time assignments checked; morning actor/lighting PlayMode test added. Visual readability still needs render/device review. |
| Startup/build inconsistency | Campus is first in committed scene settings and generated configuration. Configure validates story time and build scene existence. Android output is resolved from the project directory. Licensed-runner workflow configures first and rejects missing/empty/failed/inconclusive Unity test reports. | Python report-gate tests pass; Unity configuration and build must still execute on a licensed editor. |

The cash HUD refreshes after director costs. Checkpoints capture the current decision position, and the shared Room/Night geometry preserves position across sleep/morning transitions. Same- and cross-scene checkpoint restoration have new UI integration tests. The actor-facing helper avoids zero-length look directions. The existing shader's stylised shading now receives a stage exposure value for distinct night/evening/day presentation; this is not a new production lighting pipeline.

## Executed validation

- Actual engine-independent Core C# compiled successfully with warnings treated as errors.
- **3,932 director assertions** passed, including all event options, migration and 64 director-enabled routes.
- **154 presentation assertions** passed, including every authored line and explicit time-of-day coverage.
- Existing **1,553,552 story assertions / 60,984 completed branch combinations / 29 beats** passed.
- Existing **12 real-file save recovery checks** passed.
- **3 Python build-report test methods** passed, including multiple failed/empty/inconclusive/malformed/missing report cases.
- C# source syntax scan returned no syntax diagnostics. This scan lacks Unity/NUnit references and cannot establish API compatibility or Unity compilation.

These results are logic and source evidence, not a device-playability or enjoyment claim. The counts reflect exhaustive combinations through a short opening, not separate stories or playable hours.

## Added Unity tests — not run here

`OpeningRepairTests` covers nested panel restoration and once-only cost, the courtyard inspection, correct speaker gestures, morning lighting, same-scene checkpoint coordinates, cross-scene coordinates/lighting and sleep/morning position continuity. `OpeningTests` adds actual Unity serializer migration coverage. Existing route, HUD, portrait/shader/mesh and save tests remain.

No activated Unity editor or Unity Build Automation session is available in this workspace. No successful Unity compile, IL2CPP export, new APK or phone rendering/performance result is claimed.

## Phone/cloud acceptance

1. Use the latest `main`, Unity project directory `unity`, pinned editor **6000.3.26f1**. Keep pre-export method **`Kesho.Editor.ProjectSetup.Configure`**. Existing empty pre/post-build script fields need no change.
2. Run EditMode and PlayMode tests in Unity Build Automation where configured; review results and logs. The GitHub licensed-runner workflow is a separate route requiring its registered editor/runner.
3. Install the produced **0.5.1 / code 5** APK and verify title/version and campus startup. Retain the same package/signing key to test upgrades; do not uninstall if preserving local saves.
4. On a paid-fixer path, preserve the message privately. Check the journal and verify the reply path remains distinct. An old 0.5.0 save should retain its money, life and legitimate memory after migration.
5. Inspect the courtyard contact spot, pause and return. It must remain a courtyard description.
6. Watch the gate's Rehema/Kito exchange and clerk/student exchange. Only the actual speaker gestures. Phone messages and written notices must not animate someone nearby.
7. Pay for shared blank copies where offered. Open settings, journal and settings again; return through every layer. The same result should remain visible and the charge occur once.
8. Confirm evening/sleep/morning visual continuity, legible skin/props/UI and no overly dark shader output. Morning remains morning after returning from the legacy Night scene to Room.
9. Restore both same- and cross-scene checkpoints. Verify position, time of day, cash, selected event and history; permanent mode must still offer no rewind.
10. Check tapping, Back, scrolling, click sound, background/relaunch and saving on the target phone. Record frame time/memory/heat before claiming the 30 fps goal.

## Next package

After this gate is accepted, add chapter/time progression and occurrence-safe repeated study/work. The current opening still ends before orientation. Romance, adult careers, land/livestock arcs, prison, death/endings, account integration and the full 24-hour path remain future work. No trained neural model, copied Shujaaz corpus or new voice recordings are included.

# Kesho

A story-led African life simulation about family, opportunity and the hidden price of success.

**Active development: Unity 6.3 LTS / C# / URP, in [`unity/`](unity/).** The approved direction is **24 hours of meaningful active play for one complete ordinary life path**, from college through adult life and achievement to a final outcome of success, failure or death. Optional detours can extend that path. These are production targets, not delivered content claims.

## Current delivery

Version **0.6.2** fixes the remaining build #3 failure: the missing audio listener during startup/scene loading. One persistent listener now follows the active camera; strict log checks remain enabled. See [build #3 evidence and the repair](docs/unity/BUILD_FIX_0_6_2.md). A successful APK rebuild is still required.

Version **0.6.1** repairs the cloud import failure in the 0.6.0 icon/portrait/audio metadata and adds failed-test details to cloud logs. See the [build failure diagnosis and retry settings](docs/unity/BUILD_FIX_0_6_1.md). The failed run used Unity 6000.6.6f1 rather than the project pin 6000.3.26f1. A fresh successful Unity build is still required.

Version **0.6.0** adds the Kesho launcher icon/browser favicon, camera and character movement polish, persistent customisation controls, pooled dialogue UI, immediate mute, baked foley and an ordered background autosave. See the [report fixes and outstanding runtime checks](docs/unity/RELEASE_0_6_0.md). Local core/storage/asset checks pass; Unity compilation, rendered quality and device performance remain unverified.

Version **0.5.1** repairs the opening's private knowledge, scene descriptions, speaker binding, settings/journal restoration, authored lighting and startup/build checks. Existing director saves migrate to schema 2 while retaining life history. See [release changes, verification limits and phone acceptance](docs/unity/RELEASE_0_5_1.md). Core/presentation checks pass; Unity compilation and phone acceptance remain pending.

Version **0.5.0** integrates the first offline **Life Director** into the Maweni College opening. It adds seven original event families, two context-selected moments per opening path, separate NPC memory/trust/obligations, committed outcomes and private support follow-ups. It is a rules model, not a trained neural model or an LLM. See the [model design, implementation and phone checks](docs/unity/LIFE_DIRECTOR.md).

The underlying opening retains 29 connected beats, 59 main-story lines, an unofficial fixer, official registration, payment support, housing and a next-morning consequence, with original comic portraits and the existing 3D campus. See the [college-opening delivery and limits](docs/unity/COLLEGE_OPENING.md). Reporting after paying now preserves a distinct reporting consequence.

Tap-to-walk, private verified saves, checkpoints, permanent-mode rewind prevention and seeded consequences remain. Runtime primitive creation has been removed to address the reported missing-collider error. Rewritten-story saves are kept separately from earlier development lives. Music and interaction cues are included; new foley/ambient synthesis is temporary. **Natural voice performances and real crowd recordings are not recorded.** No runtime generative model is enabled.

The engine-independent suite passes **60,984 complete branch paths** across all backgrounds plus real-file save recovery checks. Unity editor compilation, shader/scene rendering, mobile performance and final audio must still be validated in the cloud/editor and on a phone. No new APK is claimed until that build passes.

## Start in Unity

1. Install **Unity 6000.3.26f1**, Android Build Support, SDK/NDK and OpenJDK through Unity Hub; activate an eligible license.
2. Add the `unity` directory as the project. Allow package import and compilation.
3. Run **Kesho → Configure mobile project**, then **Kesho → Validate opening content**.
4. Open `Assets/Kesho/Scenes/Campus.unity` and enter Play mode.
5. Run EditMode tests. Use **Kesho → Build Android development APK** only after tests and scene QA pass.

The initial import configures URP and mobile build settings. Those generated settings should be reviewed and committed from the editor. The Android Unity package ID is separate from the legacy Godot app; old saves are not silently migrated.

```sh
dotnet run --project tools/StoryChecks/StoryChecks.csproj --configuration Release -- unity/Assets/Kesho/Resources/Story/opening.json
```

The `Unity story core` workflow runs automatically. The Unity Android workflow is manual and requires a registered `kesho-unity` runner with the pinned editor and an activated license. Do not put credentials or license files in Git.

## Design and production

- [Approved implementation plan](docs/unity/IMPLEMENTATION_PLAN.md)
- [Opening scene and character plan](docs/unity/OPENING_PRODUCTION.md)
- [Audio production contract](docs/unity/AUDIO.md)
- [Voice recording sheet](docs/unity/VOICE_LINES.csv)
- [Validation status](docs/unity/VALIDATION.md)
- [Full-game design foundation](docs/DESIGN.md)

Supabase remains the planned account/cloud-save/storage backend. Online play has a separate authoritative economy and is not implemented in this opening. The legacy Godot files remain at repository root for reference; their workflow is named **Legacy Godot Android prototype**. See [legacy instructions](docs/LEGACY_GODOT_README.md) only when working on that archived direction.

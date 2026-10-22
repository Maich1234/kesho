# Kesho

A story-led African life simulation about family, opportunity and the hidden price of success.

**Active development: Unity 6.3 LTS / C# / URP, in [`unity/`](unity/).** The approved direction is a complete ordinary lifetime of at least **24 hours of meaningful active play**, with deeper branches targeting 30–50 hours. These are production targets, not delivered content claims.

## Current delivery

The Unity opening source contains 31 connected story beats, 65 short dialogue lines and seven scene blockouts: Arrival, Courtyard, Room, Office, Dispatch, Campus and Night. It follows a sponsorship opportunity, a false acknowledgement and a consequential night encounter. Decisions change information, relationships, evidence and later contact. There are work and ordinary application routes alongside accepting or investigating the deal.

Implemented source includes tap-to-walk navigation, short dialogue presentation, character setup, pause/settings, private saves with backup recovery, same-life checkpoints, permanent-mode rewind prevention, seeded consequences, audio buses/voice slots and Android build tooling. Art is a blockout; the existing original music/ambience is temporary. **Natural voice performances and scene-specific foley are not recorded.** No runtime generative model is enabled.

The story core is executable outside Unity. Unity editor compilation, play-mode interaction, mobile rendering, performance and final audio must be validated in an activated Unity environment. No Unity APK is claimed until that build passes.

## Start in Unity

1. Install **Unity 6000.3.26f1**, Android Build Support, SDK/NDK and OpenJDK through Unity Hub; activate an eligible license.
2. Add the `unity` directory as the project. Allow package import and compilation.
3. Run **Kesho → Configure mobile project**, then **Kesho → Validate opening content**.
4. Open `Assets/Kesho/Scenes/Arrival.unity` and enter Play mode.
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

# Kesho

**Every tomorrow has a price.** An African life simulation and political thriller about ambition, family, hidden power and the cost of success.

## Current milestone: first offline playable, v0.1.1

A native landscape Godot prototype with an original procedural 3D district. Create a 19-year-old character, choose one of three modest family backgrounds, meet your contact, arrange accommodation, explore work and education leads, and manage three days of money, time and energy. Branching conversations, seeded uncertain outcomes, a private editable journal, automatic local saving and corrupt-save backup recovery are implemented. v0.1.1 fixes native touch input and adds an original score, ambience, click/confirmation/footstep audio, optional haptics, motion feedback, persistent preferences and explicit unavailable-action explanations.

This is a short systems prototype, **not the complete 30–50-hour game**. The blockout art is not the final semi-realistic art direction. Accounts, multiplayer, real-time AI dialogue, full careers, family switching, death/checkpoint branching, content filters, ads and purchases are not implemented. Beliefs, gender, mode and family personality are saved selections; most deeper effects are future work. Kiswahili currently covers navigation and status labels only.

## Play on Android

Open **Actions → Android prototype → Run workflow**, wait for the `Kesho-Android-test-build` artifact, download and extract the ZIP, then install `Kesho-Prototype.apk`. It targets ARM64 Android devices. It is a debug-signed test APK, not a Play Store release. Android may require allowing installation from the app used to open the file. Builds are not production signed; signing keys must never be committed.

Use Sound & controls on the title or Settings in the city to adjust music, effects, ambience, vibration and reduced motion. Preferences are saved independently of your story. Audio pauses when the app loses focus.

Tap a destination in the bottom bar to walk there and open the conversation. Tap the street for free movement. Enable the virtual joystick in Settings if preferred. Reading and walking do not consume planning hours; choices do. Arrange accommodation before ending the day. Day three ends the prototype. Save & exit returns to the title. One active save slot: beginning a new life asks before replacing it.

## Develop

Pinned engine: **Godot 4.5.1 standard edition, GDScript**, Compatibility renderer. Open `project.godot`, then run the main scene. No paid assets or runtime dependencies are required. Original procedural geometry lives in `scripts/district.gd`; story content is `data/opening.json`.

```bash
godot --headless --editor --import --quit
godot --headless --script res://tests/test_life.gd
godot --headless --script res://tests/test_touch.gd
mkdir -p build
godot --headless --export-debug Android build/Kesho-Prototype.apk
```

For Android exports install Java 17, Android SDK Platform 35, platform-tools and build-tools 35.0.0. Set Java SDK and Android SDK paths in Godot Editor Settings and install the matching export templates. The CI workflow performs these steps without requiring a PC on the player's side. CI availability and provider quotas still apply. iOS requires a macOS/Xcode build and signing path; no iOS build is included.

Tests deliberately write and recover local saves; use an isolated `XDG_DATA_HOME` when running them on Linux to protect your development save. Visual smoke captures can be generated with a working display:

```bash
KESHO_QA_DIR=/absolute/output godot --audio-driver Dummy --script res://tests/visual_smoke.gd
```

## Structure

- `scripts/life.gd`: offline state, gated transactions, seeded outcomes, durable saves.
- `scripts/district.gd`: procedural world, pathfinding, walking and character blockouts.
- `scripts/main.gd`: title, character setup, dialogue, HUD, journal and controls.
- `data/opening.json`: authored scenes and outcome definitions.
- `docs/DESIGN.md`: agreed design and phased delivery.
- `docs/OPENING_CHAPTER.md`: opening sequence, cast and playable routes.
- `docs/VALIDATION.md`: checks performed and remaining device validation.
- `supabase/migrations/`: account-linked private-save foundation, not deployed or connected.

## Backend boundary

The current APK is offline and requests no internet permission. Supabase is planned for authentication, private save sync, friendships and opt-in summaries. Live sessions need an authoritative server. Offline money or state must never be imported into the online economy. Do not embed service-role keys in the game. The supplied SQL is a reviewable starting migration; it has not been applied to a Supabase project.

## Research and representation

The city, organisations and current scenes are fictional. No current scene is claimed to adapt a researched news case. Historical research from 2007–2026 and broader African reporting is a separate writing workstream; preserve allegations vs findings and combine sources into original plots. Real ethnic-group names will not be used as faction substitutes. See the design's research protocol.

Original project code and narrative copyright remains with the project owner; no broad redistribution licence is granted here. Godot's MIT notice and the bundled DejaVu font licence are in `THIRD_PARTY_NOTICES.md` and `assets/fonts/LICENSE.txt`.

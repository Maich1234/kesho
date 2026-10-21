# v0.1 validation — 22 October 2026

## Completed locally

- Godot 4.5.1 imported the project and ran it without GDScript parse or runtime errors.
- **185 state/content/navigation checks passed**: action gating; background-specific shelter; one-off reward protection; money/time/energy rules; bed requirement; seeded random replay; both errand outcomes across seeds; three background/waiver paths; chapter completion; malformed save rejection; primary save roundtrip; corrupt-primary backup recovery; private summary boundaries; decision-ID uniqueness; outcome-weight sums; every destination connected to every other destination.
- Native OpenGL software-rendered visual smoke exercised the title, creator, district, contact dialogue, committed result, journal and automatic walking-to-courtyard interaction. Seven screens were inspected. A wrapping HUD title and overbright lighting were corrected.
- Android export succeeded for ARM64 (the mid-range target; ARMv7 disabled to reduce download size). APK signature verifies with v2/v3 schemes. The APK is debug signed and requests no internet permission.
- The cloud-build workflow is supplied; its actual GitHub run status is separate from these local results.

The deliberate corrupt-save test now uses a parser that returns an error without logging a spurious engine exception. It recovers the previous backup and reports recovery to the UI.

## Not established yet

- The APK has **not been installed or performance-tested on a physical Android phone**. No claim of sustained 30 fps, thermal behaviour, battery cost, safe-area coverage or broad Android compatibility is made.
- No iOS build, signing or device test.
- No deployed Supabase migration, live signup, account recovery, cloud save, friendship sharing or multiplayer verification. The review-only SQL foundation is not a claim that these flows work.
- No historical-news evidence review, external cultural review or localisation signoff. Current chapter scenes are original fiction.
- No completed full-life checkpoint/death system or 30–50-hour content library.

## Phone acceptance pass

1. Install the APK, rotate into landscape, confirm the complete title and creation form are accessible. Scroll the creator if needed.
2. Start each background. Tap Contact, then Courtyard. Verify each destination opens only after walking there.
3. Arrange a bed; try a legitimate job, an education lead and one suspicious offer across separate lives.
4. Spend time and energy, try a disabled action, eat and end the day. Check balances against the dialogue.
5. Edit journal notes, Save & exit, reopen and confirm recovery. Background the app and return.
6. Enable joystick and release it outside its starting pad; movement must stop. Check tap-to-move still works.
7. Change UI language; verify navigation and status labels, with story-English limitation stated.
8. End day three. Review the summary, journal and new-life replacement confirmation.
9. Report device model, Android version, average frame rate if measurable, freezes, text size and any obstructed controls.

Future signed builds should use a retained test signing key stored as a CI secret. Ephemeral CI debug keys can require uninstalling an older test APK before installing a new one; uninstalling removes local progress. A save export/migration path is needed before long-lived tester progress is encouraged.

## Technical references

- https://docs.godotengine.org/en/4.5/tutorials/export/exporting_for_android.html
- https://docs.godotengine.org/en/4.5/tutorials/editor/command_line_tutorial.html
- https://docs.godotengine.org/en/stable/tutorials/export/exporting_for_ios.html
- https://supabase.com/docs/guides/database/postgres/row-level-security
- https://supabase.com/docs/guides/realtime


## v0.1.1 — touch and feedback fix

The reported title-screen bug was reproduced with `InputEventScreenTouch`: no character-creation screen opened while `pointing/emulate_mouse_from_touch=false`. Enabling it made the same finger event open the creator. Earlier direct screen-method calls did not cover this input route.

The project now uses one primary emulated mouse pointer for GUI and world, with raw touch excluded from world routing to prevent double actions. GUI taps cannot also move the character. Multi-finger gestures are not a supported control scheme in this prototype. Android Back navigates screens rather than immediately terminating the app.

**53 additional integration checks** send finger events through the actual input dispatcher. They cover title controls, selector opening, character creation, settings and audio-bus output/muting, saved preferences, modal z-order after language change, navigation and dialogue, a single committed story decision, journal preservation on Android Back, joystick release outside its pad, save/resume and background audio pause/resume. The master audio mixer was checked for nonzero output and silence when muted. This is automated engine validation, not a claim of a physical speaker/headphone or vibration test.

Original audio assets: 43.6-second score, 16-second city ambience, seven short UI/movement cues; less than 350 KB combined. PCM generation is deterministic and reproducible with `tools/generate_audio.py`. The renderer does no procedural audio synthesis per frame.

The build targets ARM64 Android, version code 2 / 0.1.1. The same local debug signing key is retained from the initial direct APK; verify certificates before presenting an update as compatible. Physical-device touch responsiveness, speaker balance, vibration, thermal behaviour and sustained frame rate still need user testing.

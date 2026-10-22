# Kesho 0.4.0 — college opening

Approved implementation scope: replace the placeholder opening with an original comic-style Kenyan-inspired campus, original selectable cast, short conversations and a playable unofficial-registration offer. This is the opening slice, not the complete 24-hour lifetime.

## What is implemented in source

- Maweni College: admissions hall, colonnade, library wing, fictional city skyline, gate, minibus, paved courtyard, trees, seats, tea/copy kiosk, fee notice, admissions queue and student support desk. Architecture is assembled from reusable explicit meshes and batched by material.
- An original cel-style shader with stepped lighting and dark edge shading. Warm paper UI, ink borders, mustard actions, short speech cards and illustrated close-ups for Kito and Rehema. No Shujaaz artwork or characters are copied.
- Amani and Nia portrait selection plus a custom option. Name, background, gender, belief, four skin tones, four outfit colours and five hairstyles. Live 3D appearance preview. Preset background connections affect family dialogue; personality remains player-shaped.
- Articulated 3D figures with face features, hair, backpacks, arm/leg animation and talking gestures. Reduced-motion preference removes gesture and camera interpolation effects.
- Tap-to-walk; tap NPCs/notices/desks to approach and inspect; contextual story button approaches the next target. No timed dialogue choices. Up to three authored responses per beat.
- 29 connected beats and 59 short lines: college gate → fixer/notice/mentor → verification or queue → payment or provisional support → housing → first night → next-morning consequence. The fixer cannot guarantee a place. Paid/provisional routes and free overnight accommodation keep low-cash lives viable.
- Original existing music/click/step clips, dialogue ducking, synthesized paper/bedding rustle and campus air/birds. These additions are placeholders, **not recorded natural performances or real crowd recordings**. Voice slots remain unrecorded. Full natural sound production still needs recordings and phone-speaker review.
- Explicit meshes replace all runtime `GameObject.CreatePrimitive` calls. This removes implicit collider creation from the shipped scene construction. A small link.xml also preserves the three collider types for imported assets. The screenshot's SphereCollider failure still needs confirmation on the rebuilt Android player.

## Saved lives

This is a rewritten story graph with chapter ID `college_first_day`. Saves use `kesho-unity-v2`, separate from the earlier `kesho-unity-v1` development opening. The title explains this when earlier data exists. Earlier files are preserved; no incompatible node is silently mapped into the new storyline. Hair, cast identity and customisation are saved and validated. Checkpoint and permanent-consequence rules remain.

## Approved future direction (not delivered by this slice)

- Fictional cities and place names, with Nairobi/Kisumu/Mombasa-inspired architecture and rural Kenyan farms, cattle and wildlife in suitable habitats; later African-inspired destinations unlock through the story.
- Barbers/salons with previews and earned-money purchases; optional hair growth. This slice includes only free initial hair customisation.
- Distinct individual speech styles, mostly readable subtitles, occasional pronunciation quirks and natural voice performance. No tribe labels or automatic accent-to-morality mapping.
- Entire lifetime: college, adult work/family, influence, crisis and legacy; about 24 meaningful active hours per ordinary completed path. This opening is not a claim of that duration.
- Supabase accounts/cloud storage, online city, a second adult family character, multiplayer and bounded generated side dialogue remain later phases. No trained ML system is present here.

## Build and phone acceptance

Unity Build Automation: repository `Maich1234/kesho`, branch `main`, project subfolder `unity`, pinned editor `6000.3.26f1`, pre-export method `Kesho.Editor.ProjectSetup.Configure`. Keep APK output and run the existing EditMode/PlayMode suites. This release is **0.4.0 / version code 3**, package `com.wabunifu.kesho.unity`.

1. Import/compile all assemblies; ensure the custom shader has no errors. Run tests. Campus is first in build settings.
2. Install on the target phone. Title, portraits and materials must load; no missing-SphereCollider console error.
3. Choose each cast option; change all five hairstyles; confirm readable controls, correct preview, scroll and keyboard dismissal.
4. Complete official registration, paid fixer and investigation/payment-support routes. Inspect all optional hotspots and check queues/furniture cannot trap the player.
5. Reopen after each location transition; verify money, appearance, evidence and checkpoint identity. Background during sleep; returning must preserve the transition and save.
6. Inspect phone frame pacing, thermal behaviour and memory for at least 15 minutes. 30 fps is a target, not yet measured. Reduce actor count or mesh complexity if needed.
7. Review original comic art, face readability and campus composition in the actual APK. A generated portrait atlas is not a screenshot of Unity gameplay.
8. Confirm music ducks during conversation and sound can be muted. Replace synthesized foley/ambience with licensed recordings before claiming natural sound quality.

There is no activated Unity editor in this workspace. Core tests and source checks are executed here; Unity compilation, shader rendering, PlayMode tests and Android/device acceptance are pending the cloud build.

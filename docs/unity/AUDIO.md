# Audio production contract

Natural speech and believable domestic sound are required for the approved slice. The checked-in source has playback hooks, subtitles, voice IDs and a recording sheet. **It does not contain recorded dialogue or realistic sleep/chat foley.** Existing original synthesized score, air bed and click/footstep cues are temporary development assets carried forward with their original authorship.

## Deliveries

Record `VOICE_LINES.csv` into `unity/Assets/Kesho/Resources/Voice/<voice_id>.wav` or `.ogg`. Preserve the ID when editing a line. Capture dry mono voice, consistent microphone distance, clean room tone, several natural performances and separate breaths/reactions when useful. Keep a licensing/performer consent record. English, Swahili and context-sensitive Sheng need fluent review; don't force slang or stereotyped accents into every line. Dynamic name/contact lines need separately authored variants or performance edits before final VO.

| Resource key under Resources/Sound | Required sound | Mix direction |
|---|---|---|
| bus_square | Bus, distant vendors, distinct open-air space | Establish place; never obscure the contact |
| courtyard | Neighbours, dishes, cloth, occasional distant traffic | Warmth with room to hear pauses |
| cups | Cup/utensil handling, cloth contact | Attach to the actual hand action |
| phone | Handset interaction, filtered remote voice where appropriate | Avoid real emergency/alarm confusion |
| room_tone | Small-room air and distant household | Continuous quiet, not digital silence |
| bedding | Sheet and mattress movement, latch | Short sleep sequence; no forced waiting |
| office | Fan, paper, chair, distant queue | Music drops beneath the negotiation |
| paper | Fold, stamp, page movement | Trigger on the corresponding action |
| dispatch | Yard activity and signed handover | Distinct from the office |
| campus | Restrained courtyard ambience | Relief without implying all danger ended |
| knock | Location-specific door knock or phone variation | Event-linked, never a repeated random scare |
| night | Restrained night exterior through the room | Leave space before the response |

`Soundscape` keeps voice separate from music, ambience and six bounded effects sources. Skipping a line stops its voice; backgrounding pauses audio. Missing clips do not generate fake dialogue. Subtitles preserve the scene's meaning, but a subtitle is not evidence that audio production is finished. Validate loudness and intelligibility on a phone speaker and headphones; test mute, interruptions and return from background.

Before production release, fail the audio coverage gate if any required voice/foley is absent. The development Android build intentionally permits missing performances so editor work can proceed; label it as a blockout.

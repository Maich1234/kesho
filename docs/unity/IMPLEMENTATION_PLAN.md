> **0.4.0 approved update:** The user approved the college-registration rebuild on 22 October 2026. The active opening starts at Maweni College with a suspicious shortcut. Fictional city names remain, with mainly Kenyan visual references. Mixed live 3D/comic conversations, original cast/custom character and initial hairstyle selection are in scope. See [current delivery](COLLEGE_OPENING.md); older bus-arrival references below are historical. The full 24-hour lifetime goal is unchanged.

# Approved Unity implementation plan

Approved by the user on 22 October 2026. Supersedes the Godot gameplay rewrite. Build a tense, relationship-led life story; no unrelated arcade activities or repetitive work added to inflate playtime.

## Product contract

- Unity 6 LTS, C#, URP. Android first, supported iPhones later. Landscape; tap-to-move first, optional joystick in the usability increment.
- One coherent fictional African city and countryside, informed by researched contexts rather than a collage of ethnic stereotypes. Contemporary setting; later episodes may adapt mechanisms from verified 2007–2026 reporting, including reform successes.
- The lifetime begins at college entry, with a brief arrival/funding transition where needed. Adult characters begin with limited means and choose background, appearance, gender and beliefs. Several connected paths follow into adult life, with family/romance central. One household protagonist is the focus.
- **One complete ordinary path targets 24 hours of meaningful active gameplay across the whole lifetime:** college, establishing an adult life, becoming someone, and a final outcome of success, failure or death. The 24 hours are not an opening-chapter budget or the combined length of alternative routes. The earlier 30–50-hour ambition applies to optional/deeper detours rather than the standard path. Early death remains a shorter ending under the agreed consequence modes; prison, recovery and rebuilding can change duration. Checkpoint and permanent modes retain their agreed meanings.
- All powerful characters face an earned crisis. Honesty is not immunity; corruption is not guaranteed success or automatic death. Victimisation and medical outcomes are not morality punishments.
- Offline and online lives remain separate. Supabase is planned for accounts, private cloud saves and storage. Shared political systems require authoritative commands, bounded institutional power and abuse moderation independent of fictional corruption.

## Duration budget

| Stage | Baseline active time | Dramatic work |
|---|---:|---|
| College and foundations | 5 h | Brief arrival, funding, study, friendships, first relationships and gatekeepers |
| First foothold after college | 4 h | Graduation or leaving, work, unemployment, enterprise and first obligations |
| Building an adult life | 5 h | Independence, partner/family decisions, responsibility and changing loyalties |
| Becoming someone | 4 h | Career, business, community or public influence; wealth is one possible milestone |
| Reckoning and crisis | 4 h | Earlier choices converge; success can unravel, or rebuilding and reform become possible |
| Final outcome and legacy | 2 h | Earned success, failure or death; resolve the central conflict and household consequences |
| Total for one complete ordinary path | 24 h | Entire lifetime arc, to be proved through content and playtests |

Chapters cover years through meaningful transitions. The game does not simulate every day of a lifespan in real time. Completing college, getting a job or becoming a millionaire is progression, not automatically the end credits. A success/failure ending may close the life story while the protagonist is alive; death is another possible final outcome. The final crisis and resolution must complete the chosen path before it counts toward the 24-hour target.

No waiting screen, compulsory repeat job, slow walking requirement or unskippable dialogue counts as delivering this target. Measure fastest ordinary completed routes as well as median first-time routes. Local engagement telemetry distinguishes active play from menus/background/extended inactivity; telemetry alone cannot prove meaningful duration.

## Milestones and acceptance gates

1. **Narrative/system foundation — source delivered:** 31 authored opening beats; typed state/choice rules; seeded night resolution; knowledge-constrained dialogue boundary; save/checkpoint rules; seven scene blockouts; Android build entry point.
2. **Unity verification — blocked on editor/activation environment:** resolve packages, compile entire project, run EditMode tests, play all scene transitions, inspect touch and safe-area layouts, test interrupted saves and background resume. A core-only test pass does not satisfy this milestone.
3. **45–60-minute opening vertical slice — production target:** replace blockout scenes with licensed/modular environment and adult character assets; stage family contact and fund confrontation; record/edit key dialogue and foley; integrate authored investigation and household tasks with real scene interaction. Expand only where it develops plot. Current source length is unmeasured and is not claimed to reach this target.
4. **Opening evaluation:** first-time testers find their contact without coaching, understand the suspicious acknowledgement, remember two NPC motives, and explain a consequence using earlier evidence. Track stalled goals, skipped dialogue and moments of tension/release. Revise before expanding.
5. **Life-path production:** extend multiple paths with shared institutions and distinct relationships. Validate every critical-path gate, recovery route and ending. Research and cultural review enter before event-inspired story production.
6. **Accounts, then a bounded online experiment:** verified Supabase isolation/RLS, cloud-save conflicts, consented sharing, then one authoritative cooperative reform activity. No production multiplayer claim from database scaffolding.

## Rules applied during implementation

- Show the current goal in the world and HUD; one contextual interaction. Avoid long exposition menus.
- Choices clearly express intention. Consequence certainty follows what the character can reasonably know.
- Exploration is interruptible. Decisions are not timed; reading does not deplete resources.
- Tension alternates with warmth and quiet. The director cannot spawn a punishment to force drama.
- Authored world facts are authoritative. Text generation cannot change money, evidence, knowledge, consent or political outcomes.
- A random result is committed once. Save generator state; retries and duplicate input cannot award twice.
- Minimum comfortable touch targets, safe areas, visible subtitles, audio control, motion reduction and later content softening. Test on actual devices.
- Target stable 30 fps on selected 6 GB Android devices with measured GPU/thermal/memory behaviour. RAM alone is not a performance specification.
- Save failure must be visible. Preserve last valid save and keep old-engine saves separate.
- Full visual/audio quality, 24-hour duration and any ML quality require independent evidence, not a completion checkbox.

## AI/ML sequence

First ship deterministic state transitions, explicit NPC knowledge and an explainable pacing director. The opening's director only selects a mood; it is not a trained model. The optional dialogue guard is an interface boundary, not proof of semantic safety. Later generated side dialogue needs schema validation, known-fact retrieval, contradiction evaluation, moderation, budgets and authored fallbacks before being enabled. Training/adaptive pacing needs a consented dataset, evaluation split and measurable improvement over authored pacing. Never advertise a perfect model.

## Development environment

The source can be edited from a phone/cloud workspace. Unity scene authoring and builds require a supported editor host and activation. An existing eligible desktop/remote workstation is preferable to committing to a paid cloud machine without a budget. The manual Android workflow targets such a host; it is not a provisioned build service.

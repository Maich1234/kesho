# Kesho offline app audit — 0.5.0

Audited 22 October 2026, source commit `686408ded3fac2c8f5af6dfb157318a84744128c`. Scope: the complete active Unity offline client, Core, authored content, assets, tests, editor setup and build workflows. Legacy Godot code and undeployed Supabase SQL are not counted as delivered Unity features. This audit changes documentation only.

## Assessment

Kesho currently delivers source for a short college-registration opening with a small rules director. It does not yet deliver the agreed lifetime game. The main content ends the next morning before orientation. No complete ordinary path can reach graduation, adult work, household development or a final life outcome. No measured evidence supports a 24-hour playthrough or a completion percentage.

The foundation is useful: original portraits, procedural 3D locations, appearance choices, walking, concise authored dialogue, costs, flags, seeded outcomes, private saves and checkpoint restrictions. Most of the offline product remains to be built on that foundation. Passing branch tests establishes logic coverage, not visual quality, enjoyment, Unity compatibility or playable hours.

## Evidence and verification

| Evidence | Result and limits |
|---|---|
| Source inventory | One story JSON, `college_first_day`: 29 beats, 59 main-story lines, 56 main-story choices. Seven director event families, with two resolved moments on each tested complete opening route. |
| Locations in the current story | Campus 21 beats, Courtyard 1, Room 6, Night 1. Seven Unity scene files exist, but Arrival repeats the campus construction, Room/Night share construction, and Office/Dispatch use the same legacy service-counter construction. Seven scene files are not seven developed districts. |
| Audio inventory | Four OGG files: music, ambience, click and step; procedural paper/bedding/campus synthesis. No `Resources/Voice` performances. `Speak` silently skips missing recordings. |
| Core tests rerun during audit | 1,553,552 existing story assertions over 60,984 complete combinations; 3,929 director assertions including 64 director-enabled complete routes; 12 real-file recovery checks. All passed. |
| Independent CI | [Unity story core run](https://github.com/Maich1234/kesho/actions/runs/39822604238) completed successfully for the audited commit. This workflow compiles/runs .NET Core logic, not Unity. |
| Additional audit probes | Compiled against current Core: private message preservation is added to Kito's memory; repeating the same work node/choice a second time is rejected; unused DialogueGuard accepts invented prose when given an empty fact list. |
| Runtime limits | No activated Unity editor, fresh Unity cloud log, installed 0.5.0 APK, phone recording or device profiler capture was available for this audit. Unity/API compilation, visual appearance, input, Android export and performance are unverified. |

## Defects and technical blockers

P0 is a release gate, not a claim that compilation is known to fail. P1 includes defects and architecture required before expanding the game. P2 covers presentation/interaction issues. Source-only findings require Unity reproduction where indicated.

| ID | Priority / evidence | Finding and consequence | Required acceptance |
|---|---|---|---|
| OFF-01 | P0 verification gate | There is no available Unity/IL2CPP/device evidence for 0.5.0. A passing .NET workflow does not exercise the actual app. | Pinned editor imports and compiles; EditMode/PlayMode pass; APK installs; complete touch route, save/relaunch, audio and sustained frame/memory/thermal checks on named target phones. |
| OFF-02 | P1 reproduced logic defect | `LifeDirector.Resolve` always calls `Remember(e.person, ...)`. Choosing `fixer_boundary/record` stores `message_preserved` in Kito's memory despite the result saying he learns nothing. The current UI does not expose his memory; the stored knowledge is nevertheless wrong. | Separate the player performing a private action from witnesses/recipients. Private preservation changes only player history; an explicit reply can update Kito. Add negative-knowledge tests for every private option. |
| OFF-03 | P1 expansion blocker | `GameSession.Awake` loads only `Story/opening`; `LifeState.Validate` requires that chapter. No chapter registry or transition path exists. | Save and resume across at least two chapters, retaining identity, appearance, finances, choices, memory and location. |
| OFF-04 | P1 expansion blocker, reproduced | Main-story idempotency is `node/choice` for the entire life. A two-node work loop returns true, true, false on its first three transitions. The second shift cannot run. | A scheduled activity has a unique occurrence/command identity. Retrying one shift pays once; a later legitimate shift can pay. Preserve today's duplicate protection. |
| OFF-05 | P1 model limitation | Obligations and accountability are stored, but do not currently drive a behavioural policy or future institutional result. Person trust affects one authored greeting; global trust remains separately updated. | Define per-person decisions, promises, boundaries and scoped knowledge; remove ambiguous parallel trust semantics or document their distinct roles. Demonstrate a later behaviour change caused by a prior relationship, not merely a different greeting. |
| OFF-06 | P1 lifetime-data limit | Director history stops new optional moments at 256. Person memory validates at 64 facts each/32 entries; people come from seven fixed IDs. Flags and main decisions also have fixed caps. | Versioned retention/summary policy and persistent episode IDs; stress-test long lives and migrations. Reaching a cap must not block critical story progress or silently disable the promised game. |
| OFF-07 | P1 future generation boundary, reproduced | Unused `DialogueGuard` accepts factual invention when the supplied fact array is empty. It is not sufficient protection for an LLM. No runtime generator is connected, so this is not an active model-output exploit. | Keep it disconnected until scoped context, authorised intents, contradiction tests and authored fallback exist. Schema/fact-ID checks alone cannot verify prose. |
| OFF-08 | P2 source finding | `TryInspect` describes every `contact` as Kito. Courtyard defines a `contact` hotspot too. | Scene-owned descriptors/person IDs; no off-campus Kito description unless he is actually there. |
| OFF-09 | P2 source finding | `Stage.Conversation` animates a target, not the current line's speaker. Admissions clerk is not in its speaking-actor map; support is a desk without a corresponding speaking actor. Player/self, off-screen and phone lines do not have explicit acting rules. | Bind speaker IDs per line; animate the person speaking and use appropriate framing for phone/self/off-screen dialogue. |
| OFF-10 | P2 source finding | Character-form cycling rebuilds the panel, with no retained scroll/focus state. | Keep scroll position and name/keyboard state while changing hair, background and appearance; verify on narrow landscape phones. |
| OFF-11 | P2 source finding | Settings/journal return to `ShowLine`, rather than a saved panel type. Opening settings on the director result screen can return to the main story instead of the result. Optional inspections likewise return to exploration. | Preserve the precise interaction/result screen across settings and journal, without replaying a cost or skipping a result. Verify with Unity UI tests. |
| OFF-12 | P2 source finding | The `morning_message` beat loads Night: dark window glass, while the same stage setup still creates an afternoon sun. | Explicit time-of-day lighting/environment and scene continuity consistent with the narrative. |
| OFF-13 | P1 release/reproducibility gate | Committed build settings list Arrival first; `Configure` rewrites Campus first. Most project/URP settings are generated during import. GitHub Android workflow requires a separately registered licensed runner. | Demonstrate a clean checkout/cloud build using the documented pre-export method; retain correct startup/version/settings evidence and avoid confusing legacy Godot APKs with Unity output. |

### Improvements already present in 0.5.0

The earlier report-after-payment precedence defect is fixed: report/evidence history now leads to the witness follow-up while payment history remains. Journal retention removes the oldest entry at 1,000 instead of blocking the next transition. Checkpoint restoration now saves without first overwriting restored coordinates from the present transform; actual Unity pose restoration still needs testing. Do not list these as unchanged open defects from the older audit.

## Offline feature gap inventory

“Partial” means there is source for a useful subset, not that it passed phone acceptance.

| Area | Current status | What remains for the agreed offline app |
|---|---|---|
| Arrival/college opening | Partial | Device-verified staging, orientation and sustained activities following registration. Current applications, slips and housing flags do not become complete life systems. |
| Lifetime/chapter progression | Missing | Chapter navigation, calendars, ageing, life-stage entry/exit, career changes, completion records and resumable transitions. |
| Study and training | Missing | Time allocation, attendance/study outcomes, exams, skills, scholarship/bursary resolution, graduation and alternatives after interruption/failure. |
| Work | Missing | Actual shifts, pay, fatigue/time tradeoffs, hiring, unemployment, fair/corrupt opportunities, contracts and career progression. |
| Money and livelihoods | Partial | Current one-off costs/rewards need income/expense schedules, debt/repayment, household obligations, assets, business operations, losses and recovery. No sustained economy exists. |
| Household and family | Minimal | Three background/contact labels need selectable key relatives, individual members, needs, personalities, relationships, secrets, responsibilities and evolving household state. |
| Adult romance | Missing | Distinct partners, consent/boundaries, compatibility, shared decisions, separation and fulfilling single/child-free paths. |
| NPC behaviour | Minimal | Daily activity, goals, motives, resources, learned/disclosed knowledge, promises, betrayal, support and relationship-driven choices. Existing crowds stand in place. |
| Evidence and investigation | Minimal | Flags and observed actions need evidence objects, origin, authenticity/uncertainty, custody, corroboration, allegation/finding status and controlled disclosure. |
| Corruption/institutions | Minimal | The fixer/report scene needs functioning employers, college, police/courts, public services and bounded institutional responses. Honest help and successful accountability must be viable. |
| Land and housing | Housing opening only | Durable tenure/ownership, disputes, rent/development pressures, claims and negotiated or collective remedies. No land-grabbing arc exists. |
| Livestock/rural life | Missing | Countryside hubs, farms/livestock state, market and resource pressures, loss/recovery/mediation and researched cattle-conflict content. |
| Cooperation | One short interaction | Savings groups/cooperatives, rules, contributions, shared projects, ledgers, disagreements and visible collective benefits. |
| Technology paths | Missing | Skill-building/projects, useful products, employment/business contracts, misinformation/scam decisions and privacy-aware outcomes. |
| Faith communities | Belief label only | Fictional congregations/leaders, ordinary belonging and aid, internal conflicts, finances and leadership/accountability paths. No active church/mosque lifecycle. |
| Women's opportunity/safety | Not produced | Authored adult storylines on unequal opportunity/coercion/harassment, support and recovery; younger relatives' non-graphic education/safety stories. Not just trauma or automatic punishment. |
| Health, danger and death | Missing | Health/access-to-care model, contextual hazards, injury/recovery, meaningful death transitions and credible causality. `dead` is a flag with no delivered story setting it. |
| Police, courts and prison | Missing | Investigation and due process, allegations vs findings, short-sentence time jumps, major prison chapter, release and rebuilding. |
| Power, crisis and endings | Missing | Path-specific success, leadership, history-dependent final crisis, survival/failure/death, legacy and permanent-mode new-life flow. `opening_end` is a development stop, not a lifetime ending. |
| Free exploration | Partial | Walkable micro-locations and inspections need district connections, points of interest, meaningful optional activities and scene schedules. No coherent explorable city/countryside network. |
| Character art/customisation | Partial | Two named cast choices plus custom configuration, five mesh hair variants and shared body construction need distinctive production characters, expressions, wardrobe/salon continuation and consistent customised portraits. |
| World art | Partial/procedural | Bespoke, reviewed environments and props, coherent Kenyan-inspired landmarks, readable composition, day/night/weather where relevant, and countryside. Existing palette meshes cannot establish the requested visual benchmark without an actual render review. |
| Animation/camera | Partial | Mesh limb/head oscillation needs authored performance, interaction poses, sitting/sleeping/study/work, expressions and properly bound dialogue cameras. No lip sync or animation production pipeline. |
| Sound/music | Partial | Recorded/licensed greetings and reactions, natural foley/crowds, contextual location/music transitions, independent mix controls and listening review. Current synthesis is temporary; there are no recorded story performances. |
| Language | Partial | Mostly English with a few Sheng expressions; complete English/Kiswahili localisation, consistent Sheng editing, language selection, clear subtitles and optional slang explanations. No full language system. |
| Controls/accessibility | Partial | Tap walking, subtitles, safe-area layout, volume/reduced motion need optional joystick, text scaling, contrast and comfort review, discoverability/onboarding and accessible alternatives for any later danger sequences. |
| Sensitive content | Missing settings | Content tags and equivalent softened/skipped depictions preserving choices and essential information, before producing sensitive chapters. |
| Saves/checkpoints | Partial, core tested | One life slot and one checkpoint with recovery need broader schema/rules/content migrations, stable content IDs, long-life summaries, platform lifecycle tests and a deliberate backup/export/recovery policy. Multiple slots/branch browsing are potential UX enhancements, not an assumed original requirement. |
| Journal/reflection | Partial | Current recent-decision log needs key memories, delayed consequence links and optional reflection on beneficiaries, harms and alternatives without a single moral score. |
| Account requirement | Not integrated | The agreed first-time signup/recovery flow and ownership of private saves need design/integration. Routine offline play and save access must not depend on a live server. This does not require building multiplayer first. |
| Research/content pipeline | Discovery only | The 34-source catalogue needs report review, verified evidence cards, original scripts, rights checks, cultural/language review and scenario evaluation. No licensed Shujaaz corpus or trained model exists. |
| Android/iOS release | Android setup only | Verified cloud APK/install flow, save upgrade and interruption tests, actual device matrix/profiling, release signing/distribution and later iOS configuration/macOS build/signing/tests. |

## Tests still missing before expanding confidently

Existing Unity route tests call `WalkTo` and stop; they do not wait for movement, interact, complete each dialogue, cross every transition or validate the final on-screen layout. The duplicate-button test exercises `StoryHud` in isolation, not a complete `GameSession` director transaction. The core suite is valuable, but passing it did not catch OFF-02.

Add targeted checks for:

- Per-choice disclosure: observers vs recipients vs the player, including private notes and silent refusal.
- Full GameSession flow: pending director event, affordable option, save, result, Back/settings/journal, main choice and scene transition.
- Touch arrival and callback cancellation, obstructed routes, scene loading and same/cross-scene checkpoint pose restoration.
- Actual Unity JsonUtility legacy/current/long-life saves, app interruption and storage failure; verify no duplicated spending/rewards on retry.
- Generated/imported settings, intended start scene, shader compilation/stripping, missing assets and Android IL2CPP.
- UI at representative landscape aspect ratios, phone keyboard, scrolling, language expansion, notches and reduced-motion mode.
- Profiling: sustained frame times, allocation/GC, load stalls, audio memory, battery/heat, package size and repeated scene changes. No measured performance defect is asserted without a capture.
- Observed Kenyan playtests for comprehension, humour, agency, cultural accuracy, navigation and meaningful active duration. Large branch counts cannot answer these questions.

## Recommended order to finish the offline game

| Order | Work package | Exit condition |
|---|---|---|
| 1 | Stabilise and verify the existing opening | Fix the knowledge leak and scene/panel/speaker defects; successful Unity build, interaction/save tests and real phone acceptance. |
| 2 | Build reusable lifetime systems | Two connected chapters, time/age advancement, repeatable occurrence-safe study/work, durable money/household state and save migrations, with no loss of existing life identity. |
| 3 | Produce a substantial college chapter | Orientation, actual study/work planning, friendships, household pressure and one coherent corruption/integrity conflict; measured fun and agency, not extra dialogue volume. |
| 4 | Add economic and relationship depth | Jobs/business, family/romance, negotiated obligations, evidence and bounded institution response, including honest success and recovery. |
| 5 | Add city/countryside arcs | Land/housing, livestock and cooperation/technology/faith paths using the shared systems; appropriate art, actors, language and sound. Not disconnected issue-of-the-day flags. |
| 6 | Finish lifetime outcomes | Developed adulthood and later-life paths, major crisis, prison/rebuilding where relevant, genuine success/failure/death and legacy. |
| 7 | Validate the whole offline release | Timing of a complete ordinary path toward the agreed 24 hours, balance/replay, content settings, first-time account/offline access policy, device performance, upgrade/recovery and release packaging. |

Art, audio, localisation and accessibility should accompany every playable package rather than be postponed until the end. Future model training is not a substitute for the missing playable activities and authored life stages. Online multiplayer, shared political office, PvP and live-world governance are outside this offline completion scope.

The next implementation should be package 1, then package 2. Expanding the event catalogue alone would add text to the opening without solving the missing game progression.

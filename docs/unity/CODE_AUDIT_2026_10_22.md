# Kesho: code audit and next production plan

Reviewed 22 October 2026 against `a23161a3ece2773c1fa71ac2ff102c4dcfa33dc5` on main. This is a review and proposed implementation sequence; no new gameplay systems or chapters are implemented by this document.

## Assessment

The active Unity project is an offline branching opening, not yet a lifetime simulation. It contains 29 authored beats and 59 lines, tap navigation, original portraits, procedural 3D scenes, character appearance, verified private saves, checkpoints and a seeded night result. The ending explicitly stops before orientation. Earlier exhaustive core checks covered 60,984 paths; that number counts combinations through the same short graph, not distinct stories or playable hours. No current Unity cloud-build log or phone recording was available for this review.

The old Godot project remains at repository root; active changes belong in `unity/`. The Supabase SQL is labelled review-only and is not connected to the Unity client. The separate GitHub Android workflow requires a licensed self-hosted runner; the user's configured Unity Build Automation is the actual phone-accessible build route.

## Findings in priority order

| Priority | Finding and source | Player impact | Required change / acceptance |
|---|---|---|---|
| P0 verification | Unity/editor/IL2CPP/device evidence is missing for 0.4.0. Source: build workflow, validation notes and runtime mesh/shader changes. | Cannot yet guarantee launch, readable scenes, collision-error removal or 30 fps. | Cloud compile + Edit/PlayMode results, then target-phone traversal, touch/back/resume, sound and sustained performance. |
| P1 before expansion | `GameSession.Awake` loads only `Story/opening`; `LifeState.Validate` accepts only the supplied chapter's nodes. Age stays 19; choices support only empty/sleep/night_resolution effects. | No chapter transitions, graduation, ageing, employment lifecycle or meaningful terminal life outcome. | Chapter registry, transition rules, explicit time advancement and versioned save migrations. Load a saved life across two chapters without resetting identity/history. |
| P1 before expansion | `StoryEngine.Apply` deduplicates forever on `node/choice`. | Correct for today's one-shot graph, but a repeated shift, class or negotiation becomes impossible. | Deduplicate by command/event occurrence while preserving atomic, once-only rewards. Two different work shifts must both complete; retrying one must not pay twice. |
| P1 before expansion | `ResolveNight` checks `accepted_deal` before `copied_ledger`. | Paying and then reporting produces the same outcome family as paying without reporting. The report survives as a flag but has no distinct morning response. | Explicit precedence/composed consequences for payment, evidence and reporting. Test all combinations, including pay → report and pay → stay silent. |
| P1 before expansion | `LifeState` contains one global trust integer and pressure integer, with no NPC/household/property records. `Available` gates choices only by cash and flags. | Relationships, manipulation, romance, land, livestock and cooperation cannot yet behave as independent systems. | Per-person memory/trust/obligations, evidence records, asset claims and scoped institutions. Different NPCs may reasonably react differently to one decision. |
| P1 before generated dialogue | `DialogueGuard.Accept` trusts the caller's declared fact list. An empty list allows invented factual prose. Runtime does not call this helper or `PaceDirector`. | These helpers are not an active ML model or sufficient factual control. | Keep generated state changes disabled; retrieval from authorised knowledge, structured output validation, contradiction evaluation, cost limits and authored fallback before enabling generated dialogue. |
| P2 current UX | Checkpoint button restores state, then calls `Save()` with position capture before `LoadCurrent`. | When checkpoint and current scene match, the current transform can overwrite the saved checkpoint position. | Restore state and pose before capturing position; verify both same-scene and cross-scene rewind in Unity. Finding is from source inspection, not a device reproduction. |
| P2 current UX | `TryInspect` gives all `contact` targets Kito's campus description, regardless of scene. Courtyard also defines `contact`. | An optional courtyard inspection can describe a person/place that is not there. | Scene-owned interaction descriptors and target/actor IDs. Check all interactable descriptions against the current location. |
| P2 production | `Stage.Conversation` chooses the talking actor by target, while `ShowLine` changes speakers without updating that actor. | Multi-speaker scenes can animate the wrong speaker. | Per-line actor binding and phone/off-screen speaker rules. |
| P2 long-lived saves | `LifeState.Validate` rejects more than 1,000 journal entries; every committed choice adds one. | The 1,001st entry blocks the transition. Current opening stays below this, but lifetime production needs retention. | Bounded recent journal plus persistent key-event summaries; migrations tested on long histories. |
| P2 production | Character-form buttons rebuild the whole form; illustrations are fixed atlas portraits; 3D faces/animation remain generated mesh assemblies. Audio foley/ambience is partly synthesized; all 59 voice slots are unrecorded. | Scrolling may be cumbersome, visual quality is not yet established, and natural-sounding performances are absent. | Preserve form position/focus, visually review actual device capture, add authored animation/prefabs and recorded licensed audio. |
| P2 before sensitive chapters | Settings provide volume and reduced motion, not the agreed softening/skipping controls. | Future coercion/violence chapters cannot yet honour content preferences. | Content tags, equivalent non-graphic versions and choices that preserve agency without forcing depiction. |

### Targeted checks executed during this audit

Compiled and ran a small probe against the actual Core classes (without modifying them):

- Paid + reported flags at `first_sleep` produced `collector_calls` for the test seed; no witness outcome. Inspection confirms accepted-deal branch takes priority for all seeds.
- `DialogueGuard.Accept("The registrar secretly sold your land yesterday.", emptyFacts, state)` returned true despite no such known fact.
- Two-node repeatable-activity probe applied A → B and B → A, then rejected the second A → B because the old command key remained.
- Applying a choice from a valid state with 1,000 journal entries threw validation on the 1,001st entry.

These demonstrate expansion limitations. The first two need stronger consequence/knowledge rules; the repeat guard is an intentional opening safeguard that must become occurrence-aware, not be removed. Unity-only findings still need runtime reproduction.

## What remains to deliver the agreed game

1. Device-verified opening and natural audio/animation production.
2. Chapter/time/age progression; planning study, work and household time; expenses, income, debt and recovery paths.
3. Per-person relationships, memory, motives, consent, family membership and adult romance; later second-family-character unlock.
4. Evidence inventory with provenance, corroboration, disclosure and who knows what; delayed incidents and bounded institutional response.
5. Assets and ownership: rentals, land, businesses and livestock; livelihoods, shared savings/cooperatives and negotiated responsibilities.
6. Full college, work/business/faith/civic paths, meaningful prison/rebuilding chapters, final crisis and legacy/endings.
7. Content comfort settings, optional joystick, accessibility/legibility, salon previews and later optional hair growth.
8. Supabase registration/authentication, private cloud saves, conflict resolution and consented friend summaries; subsequently a separate server-authoritative online world and moderation.
9. Optional bounded generated side conversation, only after evaluations. No perfect-model or autonomous-government claim.
10. Actual timing/playtests of complete ordinary paths to meet the 24-hour target. Counts of nodes, branches or idle time are not duration evidence.

## New themes as playable arcs

All named places/people/institutions are fictional. The player begins as an adult. Younger relatives may appear in protective, non-graphic family storylines. Real tribal labels are not used, and accent, appearance or community never determines criminality.

| Arc | Trigger | Player actions | Systems and possible outcomes |
|---|---|---|---|
| Land grabbing: The Missing Boundary | A survey team marks the family plot; later a lucrative investment includes disputed land. | Visit the plot, compare claims and records, speak separately to relatives/neighbours, secure advice, negotiate, organise a group or knowingly back the developer. | Asset/claim records, testimony, institutional resources and long-term obligations. Recovery, fair agreement, delayed dispute, displacement/rebuilding or profitable complicity with future exposure. A document is evidence, not automatic proof of ownership. |
| Cattle rustling: The Empty Boma | Family livestock disappears just before school fees or a loan payment. | Account for losses, compare conflicting reports, assess a suspicious buyer, support coordinated recovery, seek mediation and protect witnesses. | Livestock/livelihood state, rumours vs evidence, faction relationships and recovery costs. Return of some animals, livelihood rebuilding, a negotiated peace or an escalating dispute. Crime choices stay at story-decision level, not operational raid instruction. |
| Young women's safety and opportunity: The Screenshot | An adult student faces a manipulative sponsor, academic harassment or online blackmail; another thread concerns a younger relative under pressure to leave school. | Set boundaries, select trusted support, preserve evidence privately, choose whether/how to report, pursue alternative funding or help the relative continue education. | Consent, privacy, support networks, institutional accountability and content settings. Perpetrators carry responsibility; abuse or medical harm is not a punishment for the victim. Include ambition, friendship, achievement and recovery, not only trauma. |
| Manipulation: A Favour With Interest | A mentor, partner, employer or politician offers help and gradually attaches obligations. | Clarify terms, notice inconsistent stories, compare independent accounts, disclose conflicts, refuse, renegotiate, cooperate or exploit the relationship. | Person-specific motives, promises, leverage, secrets, dependency and boundaries. No all-purpose manipulation score. |
| Cooperation: Our First Common Fund | Friends form a savings group, student enterprise or farming cooperative. | Agree roles and rules, pool resources, inspect accounts, resolve disagreements, admit members or challenge a trusted organiser. | Contributions, transparent ledgers, member consent, votes and business results. Honest collaboration can succeed, mismanagement can be repaired, and capture can be contested. NPC cooperation first; online cooperation later. |
| Technology: Build or Sell Out | A student tech project grows into a job/startup offer; a client wants private data or misleading results. | Learn skills, ship a useful tool, agree contracts, protect data, challenge a vendor, audit records or accept a corrupt deal. | Skills, projects, contracts, privacy, reputation and procurement evidence. Useful products, fair work and digital organising are viable achievements; scams and abuse are story risks, not the only tech content. |
| Wider civic life | A family emergency exposes clinic shortages; a flood exposes failed drainage; a business faces a permit gatekeeper. | Compare service promises with delivery, cooperate on solutions, investigate procurement, negotiate lawful access or participate in patronage. | Shared institution/evidence/economy systems. Selected later arcs, not every national problem in every life. |

## Narrative integration and pacing

A connected first chain could be: campus funding → tech-club work → a disputed housing development → family land pressure → a rural livestock/cooperative arc. Shared characters and records connect some events. Other problems should have independent causes; a single secret cartel does not explain every misfortune.

Use one main conflict and at most two supporting threads at a time. Place friendships, meals, family jokes, romance and genuine victories between tense events. Success and cooperation deserve authored scenes and persistent benefits. A corrupt rise may initially succeed; every powerful life later faces a history-dependent crisis, without forcing the same downfall on everyone.

Randomness should choose between eligible, plausible outcomes using the incident history, evidence, support and institutional capacity. Commit a seeded draw once per incident. Preparation changes possibilities; it does not guarantee safety. A content warning, a font choice or an accent must not become a risk statistic. Do not derive individual failure probabilities directly from aggregate research prevalence.

## Implementation order and review gates

1. **Verify and repair 0.4.0:** cloud/device gate, checkpoint pose, location descriptions, speaker binding and report-after-payment consequence. Retain working one-shot protection.
2. **Build the reusable life systems:** chapter/time model, occurrence-aware commands, per-NPC relationships, evidence and scheduled consequences, asset/household economy, save migrations and content preferences. Test each through one complete two-chapter life.
3. **Produce the first substantial college chapter:** orientation, study/work planning, friendship, a tech-club project, cooperative finances and one carefully written manipulation/online-abuse arc. End with an earned chapter transition. Target 45–60 minutes of measured meaningful play for this next reviewed slice, as a contribution to the planned five-hour college stage.
4. **Add the land arc and one countryside hub:** use existing records/evidence/relationships; include a cooperative recovery route. Prove property and household consequences across saves.
5. **Add livestock, adult careers, family and institutions:** introduce rural livelihoods and conflict recovery after the asset model works, then connect adulthood to the final crisis and legacy. Preserve the full 24-hour ordinary-life allocation.
6. **Connect accounts and online systems:** private save sync first, then consented sharing, then one bounded cooperative online activity before political offices or adversarial player economies.

Do not add all themes as isolated dialogue flags. Each milestone needs a playable conflict with a useful action, a plausible response, an earned consequence and a recovery path where appropriate. Quality gates precede additional locations.

## Initial research anchors (not a complete 2007–2026 review)

- UNFPA Kenya, 4 April 2024, tertiary-institution study on technology-facilitated gender-based violence. The study covers selected institutions, not all Kenyan youth. Useful for online-abuse/support pathways, not individual probability settings: https://kenya.unfpa.org/en/news/new-study-reveals-extent-technology-facilitated-gender-based-violence-kenyas-higher-learning
- FAO Kenya, 23 July 2026, youth peace efforts in West Pokot. Documents livestock/resource conflict and youth-led cooperation; use mechanisms without transferring ethnic labels into fictional characters: https://www.fao.org/kenya/news/newsdetails/from-conflict-to-cooperation--youth-leading-peace-efforts-in-west-pokot/en
- EACC, recovered public land title-deed handover. Useful for both official collusion and successful recovery, without making every fictional official corrupt: https://eacc.go.ke/en/default/eacc-hands-over-39-title-deeds-for-recovered-public-land-worth-5-2-billion/

Before production, each arc needs its own source ledger separating verified events, allegations, outcomes, date of event and date of publication, followed by cultural/survivor-informed review where relevant. No real person should be assigned an invented crime. These are creative proposals, not legal or medical guidance.

## AI and ML design discussion

### Current status

No model is trained, packaged or called by the Unity client. Seeded randomness is not ML. The `PaceDirector` and `DialogueGuard` classes are small unused-at-runtime rules helpers. Their names must not imply autonomous dialogue, a government simulation or validated intelligence.

### Recommended responsibilities

| Layer | First implementation | Possible ML role later | Authority boundary |
|---|---|---|---|
| World rules | C# state transitions, validated commands, ownership, budgets, consent and scheduled outcomes | Usually none needed | Only this layer commits money, property, injury, legal state or chapter transitions. For online lives the server owns it. |
| NPC/institution behaviour | Utility-based decisions using motives, obligations, known evidence, resources and relationship history | A learned policy only if it improves a measured behaviour | NPCs choose among permitted actions; institutions have budgets, reach and incomplete knowledge. No omniscient cartel. |
| Story director | Filter authored events by chapter, prerequisites, unresolved threads and content settings, then score novelty, relevance and tension/rest balance | Small event-ranking model trained on reviewed examples and consented playtest feedback | It ranks eligible events. It cannot invent a death, ignore immunity/consent rules, force an ending or alter probabilities because a player declines an ad. |
| Dialogue | Authored short English/Kiswahili/Sheng lines with personality variants | An existing language model generates optional online side dialogue from scoped memory and an approved conversational intent | No direct state-write tools. Templates remain fallback. Schema validation alone does not prove that natural-language claims are true. |
| Presentation | Authored acting/foley, gestures, camera and accessible subtitles | Voice/animation tooling can be evaluated separately | Dialogue text generation does not deliver a matching natural spoken performance automatically. |

### One example: disputed family land

The world records a disputed claim, corroborated documents, unresolved allegations, financial pressure, and what each person has learned. A relative knows about an offer; a reporter does not know until told. The relative's behaviour policy can propose negotiation, refusal or concealment only where authorised. The director chooses an eligible meeting after the player returns home, avoiding interruption of another sensitive scene. Dialogue expresses the approved motive and known facts. The player decides. The rules validate the chosen action, commit any cost or disclosure, and schedule a later institutional response. A seeded outcome is saved with the incident ID so a retry cannot reroll it.

The same foundations serve cattle recovery, a cooperative audit, a manipulative sponsor or a technology contract. The model does not have to memorise every storyline.

### Offline, online and cost

- Offline is complete using authored content, rules, utility NPC behaviour and a local director. No network call is required to finish a private life.
- If playtests demonstrate a need, a compact ranking model can run locally. Evaluate a simple scoring/linear/tree baseline before a neural model. Choose a Unity-compatible runtime only after verifying its model/operator support and profiling the target Android device.
- Unity's `com.unity.ai.inference` provides neural inference and supports importing models; package compatibility and performance must be checked against the pinned editor, rather than assuming any model runs well on a 6 GB phone. Primary documentation: https://docs.unity.cn/6000.3/Documentation/Manual/com.unity.ai.inference.html
- Unity ML-Agents supports training behaviour with reinforcement/imitation learning. That is a separate workflow from language-model dialogue; it is not the default solution for a narrative director. Primary documentation: https://unity-technologies.github.io/ml-agents/ML-Agents-Overview/
- Optional online dialogue goes through an authenticated backend. Supabase can hold scoped memory, versions, consent and quotas; it is not itself the language model. Provider keys stay off the APK. Enforce output limits, session budgets, timeout/cancellation, rate limits and an authored fallback. Cache only appropriate non-private or correctly scoped responses.
- Given the near-zero budget, do not train a large language model from scratch or require remote calls for every conversation. Do not promise hosted inference is free. Choose a provider only after a small English/Kiswahili/Sheng evaluation and a measured cost/latency comparison.

### Dataset and learning sequence

1. Build a reviewed event catalogue: prerequisites, permitted effects, eligible people, evidence visibility, content tags, cooldown, resolution/recovery paths, language variants and source references. News informs scenario mechanisms; it is not an automatically trustworthy training set.
2. Author about 200 evaluation situations before model selection: ordinary conversation, conflicting witness claims, repeated play, low cash, an absent character, attempted secret extraction, unsupported accusations, coercion, offline fallback and all supported language styles. Keep a held-out set grouped by story family to reduce leakage.
3. Establish the authored/utility baseline. Record which event was eligible, why it was selected and what happened; do not silently collect real private conversations. Consent-based playtest signals can include repetition, confusion, voluntary comprehension ratings and pacing feedback.
4. Train a small ranking model only once there are enough reviewed examples to compare it meaningfully with the baseline. Synthetic simulations help test logic but do not establish real-world probabilities or prove human enjoyment.
5. For dialogue, evaluate an existing model with retrieved facts and approved intentions first. Fine-tuning for tone is a later option using licensed or consented dialogue and native-speaker review. Do not train on harvested victim accounts or use accent/ethnicity as a criminality predictor.
6. Freeze and version each release; record event/rules/model versions and committed outcomes in saves. Evaluate proposed updates on held-out cases and a controlled playtest before rollout. Do not let the live model rewrite its own policy from individual player behaviour.

### Acceptance before enabling any model

- World invariants and command authorisation hold under adversarial input; model text cannot grant assets, expose private player data or bypass consent.
- NPCs do not reveal facts they have not learned. Measure false factual claims, contradictions and missed uncertainty with human review; a passing test suite is not a claim of perfection.
- Kenyan reviewers assess natural English, Kiswahili and Sheng, individual voice consistency, humour and cultural accuracy. Accents are personality/voice choices, not labels for intelligence or morality.
- Repeated runs avoid duplicated unresolved incidents, maintain plausible cause and effect and balance tension with ordinary life and success.
- Interrupted requests, absent network, rate limits, invalid output and provider failures return to authored dialogue without blocking progress or spending resources twice.
- On-device inference meets measured memory, thermal and frame-time budgets. Hosted dialogue has measured response time and cost per session.
- Optimise coherence, player agency, comprehension and satisfying pacing. Do not optimise distress, compulsive session length or ad exposure; the 24-hour target comes from meaningful authored content.

**Recommended next AI milestone:** implement and test the rule-based director, per-NPC knowledge and validated event system inside the two-chapter life foundation. Then benchmark one optional dialogue model. A learned director follows only if evidence shows it improves the authored baseline.

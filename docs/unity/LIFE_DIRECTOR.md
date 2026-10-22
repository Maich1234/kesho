# Kesho Life Director — design and first integration, 0.5.0

## Delivered model

The first Life Director is an offline C# rules model integrated into the existing college opening. It is not a trained neural network or an LLM. It requires no provider key, server or network connection. Seven original event definitions support two short interactive moments per current opening path. The full lifetime, autonomous government, property system and online world remain future work.

The design separates persistent world state, per-person knowledge, event eligibility/ranking, validated choices and presentation. Authored content determines what can happen. Randomness is bounded and persisted. No external report text, comic panels or participant transcripts are packaged in the runtime.

## Data and authority

| Layer | Current implementation | Limits |
|---|---|---|
| Life state | Existing money, pressure, flags, story node, day, seed and identity | No new age/chapter/asset simulation in this release. |
| Person memory | Separate trust, obligations and observed-action facts for Kito, Rehema, admissions, support, household, landlord and copy seller | Facts record actions a participant witnessed. No social gossip transmission or full evidence graph yet. Household is one connection, not a complete family simulation. |
| Institutional action | Support appointment requests increment a bounded accountability counter | This counts participation, not guilt, conviction or proof of institutional reform. It is not yet a government policy model. |
| Event director | Filter by authored beat, required/blocked flags, living state and completed occurrence; score context, rest need and previous occurrence | Seven fixed event families; not arbitrary AI-generated scenarios. |
| Consequence resolver | Atomically checks occurrence, eligibility and funds, then applies only authored deltas and records outcome | No code or financial command can come from dialogue text. |
| Presentation | Brief dialogue, two or three touch responses, result and return to the existing 3D story/exploration | New events use existing scenes and synthesized audio environment. No new voice recordings or animation assets. |

## Selection policy

1. Candidate must match current story node and its required/blocked flags. Dead lives are ineligible.
2. Current occurrence is `chapter/node/day`. Only one director moment can resolve per occurrence. This is appropriate for the current one-pass opening, not a complete repeatable-shift calendar. Future repeatable activities need explicit scheduled instance IDs.
3. Context priorities: recorded complaint 100; pending bursary/work application 80; unpaid attention from fixer 70; social/rest opportunities 25; family budgeting 20.
4. Rest receives `pressure / 5` extra points (integer division). A previously used event receives a 10-point penalty if it is eligible in a later occurrence.
5. Highest score wins. Equal highest scores use one saved xorshift tie-break. Lower-ranked conflicts do not randomly displace a more relevant follow-up.
6. Selection is saved before presenting choices. Stepping away, reopening settings or reloading keeps the same pending event. Only explicit checkpoint branches can restore earlier world history.

New lives already receive distinct time-based seeds. A saved life is deterministic under the same decisions; this is intentional, rather than rerolling every load.

## Authored content in this release

| Moment | Eligibility | Useful actions and effects |
|---|---|---|
| Bursary reference | After registration, bursary pending | Check the free tracking process or retain privacy; support and relationship memory, not an awarded bursary. |
| Work hours | After registration, shift application | Request written hours or protect class time; records a boundary, does not invent a paid job. |
| Shared blank copies | After registration, no higher-priority thread | Contribute C10, help organise at no cost or leave. No private documents are shared. |
| Chai/water break | After registration; favoured under higher pressure | Pay C20 for chai or take a free water break. Both provide rest and a relationship benefit. Rehema's own trust changes her authored greeting. |
| Complaint follow-up | Morning, concern recorded | Request private appointment or keep reference without consenting yet. Support mentions the payment only if its own memory includes the player's report. |
| Fixer boundary | Morning, shortcut accepted and no recorded complaint | Request discussion at an official desk or preserve the message privately. No forced meeting, automatic injury or refund. |
| Family budget | Morning, shortcut not accepted and no higher-priority thread | Share a rough budget or establish financial privacy. No automatic remittance. |

The appointment request has two authored outcomes: a slot request logged, or a full next appointment with the request still logged. The fictional setback rate is 25%, used only when that action resolves. It is a gameplay tuning value, not a survey statistic. Its draw and result are saved; retrying the same incident does not create a new draw. Other director responses have deterministic effects. Stronger contextual outcome models require actual institutional capacity, evidence and resource systems before they can be added honestly.

## Integration and persistence

- `GameSession.LoadCurrent` reserves the eligible moment. Reaching the current story target presents it before the main beat; optional hotspots cannot silently skip its choices.
- `StoryEngine.PrepareMoment` and `ResolveMoment` operate on copied state, validate it, and commit only a successful transition.
- `StoryEngine.Available` prevents the underlying main-story choice from bypassing an unresolved director moment.
- Each NPC only receives observations from interactions at their own target. A private payment to Kito creates a Kito obligation; it does not inform student support.
- Pending event, occurrence, people and completed outcome records live in the existing checksummed offline save. Legacy saves without a director field upgrade to an empty director while retaining life identity and progress. Unity's exact deserialization behavior has an added EditMode check, still awaiting editor execution.
- Copy/checkpoint state deep-copies nested memories and event records. The checkpoint UI now avoids capturing the present transform over the restored checkpoint pose.
- The most recent 1,000 journal entries are retained; hitting the journal cap no longer prevents a transition. Director history is bounded at 256 records, after which new optional moments stop being offered. This is an opening release, not an unbounded lifetime archive. Current person-memory limits are 32 participants and 64 distinct observations each; expanding chapters requires a reviewed retention/migration policy.
- Reporting after paying now follows the witness branch while preserving the fact that the shortcut was accepted. Reporting is not erased by earlier complicity.

## Research boundary

The [source collection](../research/SHUJAAZ_AND_KENYA_SOURCES.md) and [structured catalogue](../research/source_catalogue.json) support future editorial research. They are not an approved runtime corpus. All external training/runtime approvals remain false. The current dialogue and events are original authored material. Broad themes include livelihoods, boundaries, cooperation and institutional accountability; no source is used to infer a fictional person's crime or medical risk.

## Verification and phone acceptance

Executed against actual engine-independent Core code:

- 3,929 director assertions, including every event/response, scoped memory, context ranking, seeded selection, committed outcome replay, atomic money changes, legacy state upgrade, journal retention and invalid pending-event rejection.
- 64 end-to-end opening routes with the director enabled, with JSON serialization/reconstruction after each resolved moment. Each reaches a college outcome with two director moments and affordable continuation.
- Existing 12 real-file save recovery checks; existing exhaustive authored-opening checks remain in the same command.

Added Unity EditMode tests cover actual `JsonUtility` pending-event/memory persistence and legacy JSON. These have not run in this workspace. Unity assembly compilation, PlayMode, rendering, touch layout, Android IL2CPP and device performance are pending; .NET compilation cannot establish them.

Phone/cloud acceptance for version 0.5.0 (Android version code 4):

1. Build the latest `main`, project directory `unity`, pinned Unity 6000.3.26f1. Keep pre-export `Kesho.Editor.ProjectSetup.Configure`; no new credentials or build scripts needed.
2. Run Unity EditMode/PlayMode checks and inspect the build log before installing.
3. Play one bursary path and one work path; confirm appropriate Rehema event after registration.
4. On the paid-fixer route, report the slip. Confirm the witness morning line and the support appointment choice. On a separate silent route, confirm the fixer boundary message.
5. Step away from an unresolved moment; reopen, background/relaunch and verify the same selection. Resolve once; relaunch and verify no repeat charge or duplicate obligation.
6. In checkpoint mode restore before an appointment decision and repeat it; confirm the same result. Check restored location too. Permanent mode must offer no rewind.
7. Test zero/low-cash social choices; verify free alternatives and continuation to housing. Check English/Sheng readability, result panel, Back/settings, no overlapping buttons and audible existing click feedback.

No APK or cloud success is claimed by this document.

## Subsequent model work

Add chapter/time transitions and explicit scheduled incident IDs, then evidence provenance, household/property state and bounded institution resources. Use those systems to produce land, livestock and cooperation chapters. Add content preferences before sensitive arcs. Build reviewed ranking examples and evaluate against this deterministic baseline before introducing learned ranking. Optional language generation must use scoped knowledge, authored intent, semantic review and an authored fallback; the old `DialogueGuard` helper is not sufficient and remains disconnected from runtime. Online authority and Supabase integration remain separate milestones.

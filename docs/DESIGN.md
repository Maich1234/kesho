# Kesho — design foundation

Status: agreed direction plus explicitly bounded v0.1 implementation. Working title: **Kesho**. Naming availability has not been cleared. Core promise: ambition and the hidden price of success.

## Pillars

1. An ordinary life matters before headlines do: housing, family, learning, work and belonging.
2. Power is relational: favours, loyalties, secrets, resources and institutions determine access.
3. Uncertainty is explainable: outcomes vary, but the world respects evidence, memory and established rules.
4. Reform is difficult and achievable: collective action can create durable institutions, not just replace a puppet.
5. Human range: humour, romance and warmth make pressure meaningful; characters are more than their flaws.

## World and people

One fictional African city and its countryside at launch, with later regions possible. Architecture, geography and social life draw from multiple researched African contexts, organised into coherent districts. Do not combine cultural signifiers indiscriminately. Fictional cultural communities have stable histories and customs; no real ethnic names or thin aliases for real tribes. No community inherently produces corrupt characters.

Every protagonist begins with limited means. Players choose appearance, gender, beliefs and family background, plus key relatives and broad public personalities. Authored family foundations contain variable secrets and circumstances. Relatives can be ordinary, principled, conflicted or involved in risky livelihoods; each has independent needs and dignity.

Real religions coexist with fictional movements and traditional beliefs. Use fictional congregations and leaders. Show aid, belonging and integrity alongside abuse of trust. Belief may change through play; it is not a virtue modifier. Sexual coercion is abuse of power, not a romance reward or victim culpability mechanism. Medical outcomes follow circumstances and access to care, not moral punishment.

## Offline structure

Target a 30–50-hour full life, not that duration in the prototype. Chapters allow exploration before advancing. Multiple distinct paths—education, work, business, public service, religious leadership, politics and illicit networks—intersect through people, institutions and city events. Direction can change, with genuine opportunity costs and history.

Core loop: explore → converse → allocate time/resources → decide → observe consequences → reflect. Planning dominates routine activity. Tactical options handle most danger, with occasional short real-time scenes and a choice-based accessibility alternative. Player knowledge comes through relationships, rumours, favours and betrayal; documents can confirm rather than dominate the game.

Romance and family decisions are central, including developed single and child-free lives. The protagonist remains central while selected fictional household members can become playable. Family members retain distinct knowledge and motives. All sexual storylines concern adults; children receive age-appropriate scenes only.

Major cases can open a playable prison chapter; shorter sentences use significant decisions and time jumps. Wrongful or political prosecution is possible. Release opens rebuilding rather than erasing history.

Checkpoint mode permits private alternate timelines as the same character. Permanent-consequences mode requires a new life after death. Mode cannot be silently changed to invalidate an ending. Neither mode can rewind the shared online world.

## Consequences

Use bounded stochastic rules instead of one morality roll. Model the relevant agent's resources, motive, knowledge, evidence, institutional exposure and relationship memory. An honest person can suffer injustice; corruption can yield rewards. Do not make all hardship evidence of wrongdoing.

Every powerful character faces a culminating crisis whose cause and outcome reflect their history. Seed revelations in earlier events. A crisis may lead to survival, reform, disgrace, prison, exile, death or rebuilding; avoid an automatic punishment cinematic. An apparent victory can precede an earned reversal.

The optional private journal records what the player knew then and invites reflection on beneficiaries, costs and alternatives. No single good/evil score. Summaries may show conflicting achievements and losses.

## Online world

Independent characters, progress and economy. Private offline summaries can be shared with friends by explicit consent, excluding sensitive information by default. They are unverified and grant no competitive advantage.

Start as an ordinary adult. Unlock one suitable adult family relative later: maximum two playable family characters per account. Each has a public identity, personal standing and optional discoverable aliases; moderation always operates at account level. Switching cannot erase obligations or share secret character knowledge automatically.

A permanent city has political seasons. Elections and new crises refresh conflict; businesses, relationships, history and durable reforms can survive. World events advance while personal needs and essential story progression pause during absence. Business liabilities during absence need separate transparent rules before implementation.

Most activity is asynchronous, with small scheduled/live gatherings and cooperative missions. Begin with one complete shared activity before general live traversal. Players may reach top offices. AI cartels influence contracts, appointments and networks through bounded resources and competing interests. Reform targets these mechanisms; AI cannot arbitrarily invalidate a legitimate victory.

Scams, theft and sabotage use fictional currency and are limited to signposted risky contexts. Protect starter assets, basic progress and paid cosmetics; cap losses and repeated targeting. No routine attack on an offline player. Define server-backed disconnect grace to prevent escape exploits. Ordinary PvP defeat is recoverable; any permanent-death route must be explicit and opted in.

Romance with fictional adults or consenting adult players is possible. Real players retain control; shared finances and household changes need mutual agreement. Blocking, leaving and moderation remain independent of the fictional institutions. Corrupt in-world courts must never corrupt abuse reporting.

**Open decision:** online re-entry after permanent death. The agreed private checkpoint branch does not decide online replacement characters, inheritance or continuing political claims. Resolve before building those mechanics.

## Presentation

Landscape, semi-realistic 3D with elevated isometric framing and closer dramatic shots. Stable 30 fps is a target on selected mid-range 6 GB Android devices, not a claim established by RAM. Select actual GPU/SoC test devices; define the oldest supported iPhone before iOS work. Tap-to-move default with optional joystick. Scalable detail and connected locations keep cost and performance manageable.

English/Swahili text, natural context-sensitive Sheng, short voiced greetings and reactions. Use human language review; optional slang explanation. Selected mature themes can be softened or summarised without withholding needed information or meaningful choices. No explicit sexual imagery. Adult audience, some visible violence; age ratings require later platform assessment.

## AI boundaries

Major storylines are authored. Offline play must remain complete with authored variations and deterministic simulation. Online generative AI supplies bounded side conversations or candidate side events. It never directly commits balances, arrests, inventory, political office, evidence or relationship state. Validate proposals against allowed transitions and authoritative knowledge; enforce length/rate/cost limits, cache where safe, provide authored fallbacks. Free text from players is untrusted input, not permission to bypass rules.

## Research protocol

Current opening scenes are wholly fictional and not claimed as researched adaptations. For later source-inspired episodes, use 2007–2026 Kenyan reporting and broader African sources. Collect original reporting, court findings, audit reports and official investigations where available. Record date, jurisdiction, source links, corroboration, allegation/finding status and the mechanism being adapted. Research successful reform and integrity as well as scandals. Combine documented mechanisms into original people and events. Avoid recognisable private individuals, changing only names, and claims stronger than their evidence. Get relevant cultural and lived-experience review before release.

## Architecture and delivery

Provisional: Godot 4.5.1/GDScript, Supabase Auth/Postgres/Storage, authoritative live-session server. Offline state is local; RNG state, immutable decision IDs and schema version travel with saves. Public summaries are explicitly labelled unverified. Online balances, offices and rewards are server controlled. Put database RLS on private records, never distribute service-role credentials, and use idempotency keys for consequential online commands. Hosting and provider quotas are budget constraints, not evidence that live sessions are free at scale.

Phone-first workflow: GitHub source and cloud Android builds, installable test APKs. Validate iOS via macOS/Xcode signing later. No assumption that a browser editor alone can export native builds. v0.1 is a development build without account gating; production initial signup and offline credential behaviour must be integrated before an account-enabled beta.

## Business model

Free entry, optional rewarded ads and cosmetics. No forced ads in sensitive scenes. No purchase of online wealth, legal immunity, political influence or death reversal. Purchased cosmetics cannot be stolen. No real-money redemption or player wagering. Rewarded-ad benefits need an explicit non-competitive design before integration.

## Milestones and gates

- **v0.1, implemented:** one procedural district, three arrival variants, housing/work/education choices, time/energy/money, local persistence, journal, Android build workflow.
- **v0.2:** verify on actual Android hardware; strengthen character art, readable touch targets and collision/navigation; implement one consequential continuation, checkpoint branches and save slots; validate transitions and language with players.
- **Account-enabled beta:** integrate Supabase Auth, signout/recovery, private saves and conflict resolution, explicit snapshot sharing/friend permissions. Verify with separate real accounts. Keep economy isolated.
- **Online experiment:** one authoritative cooperative activity and reform result; test retries, disconnects, permissions, costs and abuse reporting. No claim of a persistent multiplayer city before this exists.
- **Expanded chapter production:** only after player evidence supports navigation, pacing, emotional investment and consequences. Build depth in successive paths toward the full-life target.

Do not claim the supplied migration is deployed, the APK is device-tested, historical research is complete or 30–50 hours of content exists until demonstrated.

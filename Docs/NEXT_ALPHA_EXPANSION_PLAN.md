# Next Alpha Expansion Plan (Player Feedback + Emergent Depth)

This roadmap translates player-facing feedback into concrete, scoped implementation work for the next Alpha cycle. It is designed to improve **clarity**, **choice impact**, and **replayability** without destabilizing Alpha-1 save/load and day-slice reliability.

---

## Prioritization model

- **P0 (must do first):** player clarity and usability items that increase moment-to-moment comprehension.
- **P1 (next):** systems that raise emergent depth in day-slice loops with moderate implementation risk.
- **P2 (after stabilization):** major replayability and long-term complexity expansions.

Each item includes:
- **Current baseline** (what exists now)
- **Gap**
- **Implementation direction**
- **Acceptance checks**

---

## P0 — Player Feedback & UI Flow Improvements

## 1) Dashboard / status clarity

### Current baseline
- Needs, money, clock, and event feed are already represented through HUD/feed/UI pipelines.

### Gap
- Key state interpretation can still be too implicit when pressure rises (need collapse risk, health trend, debt pressure).

### Implementation direction
- Add icon + tooltip layer for:
  - Needs trend (rising/falling/critical)
  - Health pressure summary
  - Money trend (income/expenses/debt warning)
  - Time pressure (upcoming shift/appointment/day-slice stage)
- Add severity color conventions and shared legend used by HUD, popup, and journal cards.

### Acceptance checks
- New players can answer “what is most urgent right now?” within 3 seconds.
- Every critical state uses a consistent icon/severity mapping in HUD + popup + journal.

## 2) Dynamic guidance

### Current baseline
- Event/popup infrastructure exists and can surface contextual system messages.

### Gap
- Early play lacks explicit “what should I do now?” coaching for basic survival cadence.

### Implementation direction
- Add `GuidanceHintSystem` (event-driven, non-blocking) with:
  - Starter hints (first 1–3 days)
  - Context-sensitive hints (low water, unpaid bills, untreated injury, etc.)
  - Cooldown/anti-spam gating
- Bind hints to actionable buttons where possible (open inventory, open grocery, view map).

### Acceptance checks
- First-day onboarding can be completed with no external documentation.
- Hint frequency remains useful (no repeated spam for same issue within cooldown window).

## 3) Event transparency (without spoilers)

### Current baseline
- Structured event outputs and consequence systems already exist.

### Gap
- Player cannot reliably estimate risk/reward before committing to an action.

### Implementation direction
- Add lightweight “Possible Outcomes” panel on high-impact actions:
  - show probability bands / qualitative risk labels (Low / Medium / High)
  - show immediate domains affected (money, social, health, legal) without revealing exact hidden rolls

### Acceptance checks
- Players can identify likely downside categories before choosing.
- Surprise is preserved by withholding exact roll thresholds and hidden modifiers.

## 4) Household / family summary

### Current baseline
- Household systems and relationship memory exist.

### Gap
- Household state is distributed across systems and not summarized in one quick view.

### Implementation direction
- Add a “Household Pulse” panel:
  - each member: top need, mood band, current location/activity, relationship trend with active character
  - household-level tension meter (financial stress + conflict + chores backlog)

### Acceptance checks
- Player can triage who needs help first in <10 seconds.

## 5) Mobile / controller optimization pass

### Current baseline
- Multiple interaction surfaces already exist but are desktop-first.

### Gap
- Day-slice actions may require too many taps/clicks on smaller screens or controller input.

### Implementation direction
- Define command-path budget:
  - max 3 inputs for common actions (eat, rest, hygiene, contact, travel)
- Add focus order maps + controller hints on major panels.
- Add touch target size audit for high-frequency actions.

### Acceptance checks
- Core day-slice checklist is completable on controller/mobile mock flow without friction spikes.

---

## P1 — Skill / Activity Depth and Emergent Looping

## 6) Skill synergy + conflict model

### Current baseline
- Large skill and skill-tree catalogs exist with progression hooks.

### Gap
- Skill interactions are mostly additive; fewer meaningful tradeoffs.

### Implementation direction
- Add pairwise modifiers:
  - Synergy examples: Cooking + Home organization improves meal efficiency/waste reduction.
  - Conflict examples: Quick Meals habit increases speed but reduces nutrition quality over time.
- Store interactions as data tables to avoid hardcoding.

### Acceptance checks
- At least 10 synergy/conflict pairs influence real outcomes by tuning-visible amounts.

## 7) Passive skill impact + micro activities

### Current baseline
- Action loops and skill progression exist.

### Gap
- Progression relies heavily on explicit macro actions.

### Implementation direction
- Add passive trait/skill triggers (e.g., “Steady Hands” lowers repair/medical failure rate).
- Add micro actions (quick tidy, stretch, hydration prep, stitch, brief budgeting) that grant small XP/condition adjustments.

### Acceptance checks
- Micro actions are viable but not exploitable (diminishing returns + cooldowns).

## 8) Skill-reactive incidents

### Current baseline
- Incident director and event systems exist.

### Gap
- Not all incidents leverage skill-based alternative resolutions.

### Implementation direction
- For incident templates, define:
  - default outcome path
  - 1–2 skill-gated mitigation paths
  - failure escalation path

### Acceptance checks
- Random incidents consistently offer at least one skill-informed mitigation route.

---

## P1/P2 — Emergent Gameplay and Character Depth

## 9) Reactive NPC behavior and town ecosystem pressure

### Implementation direction
- Add memory flags for observed player habits (skips chores, hoards supplies, consistent lateness, generosity).
- Feed flags into social reputation + service response + household friction.
- Expand economy pressure coupling (supply/demand multipliers affected by town incidents and aggregate behavior).

### Acceptance checks
- Repeated habits lead to visible NPC/town response shifts within a playable week.

## 10) Long-term consequence propagation

### Implementation direction
- Persist monthly/seasonal consequence markers (career reputation, chronic stress burden, legal history, recurring medical debt).
- Add long-horizon modifiers to opportunities and risk rolls.

### Acceptance checks
- A decision made in week 1 can still materially influence month-2 options.

## 11) Psychological + lifestyle depth

### Implementation direction
- Add psychological trait axes (anxiety, patience, resilience) into decision weights and efficiency curves.
- Add lifestyle profile accumulation (diet quality, movement, social hygiene, sleep regularity) and long-term health/happiness modifiers.

### Acceptance checks
- Lifestyle trends become visible in dashboards and affect event probabilities.

## 12) Aging and generational legacy expansion

### Implementation direction
- Deepen inheritance/tradition carry-over:
  - assets/debt transfer
  - household rituals/traditions continuity
  - intergenerational relationship momentum
  - genetics + upbringing composite effects

### Acceptance checks
- Family arc outcomes feel distinct across two generations in simulation harness runs.

---

## P2 — Content & replayability expansions

## 13) Seasonal / environmental diversity

### Implementation direction
- Add biome profile overlays (urban/coastal/forest/rural) with survival modifiers.
- Expand season-linked activity/event pools (festivals, weather hazards, seasonal jobs/markets).

### Acceptance checks
- Starting biome materially changes optimal strategy and available opportunities.

## 14) Dynamic careers + story hooks

### Implementation direction
- Add job volatility events (layoffs, transfer offers, workplace incidents, credential gates).
- Tie hooks to prior choices and relationship/legal/health history.

### Acceptance checks
- Career trajectories diverge based on player decisions and incident history.

---

## Reliability gate (must stay green while expanding)

Before each content wave, keep these mandatory checks green:

1. Save/load parity for newly introduced runtime state.
2. Day-slice continuity under unusual action sequences.
3. Performance under stress (large households, dense towns, incident spikes).
4. Event-spam protection for any new hint/incident layers.

No new system should bypass these gates.

---

## Suggested rollout order (impact vs effort)

1. P0.1 Dashboard clarity
2. P0.2 Dynamic guidance
3. P0.4 Household pulse
4. P0.3 Event transparency
5. P1.6 Skill synergy/conflict
6. P1.7 Passive + micro activities
7. P1.8 Skill-reactive incidents
8. P1/2.9 Reactive NPC + town ecosystem response
9. P1/2.10 Long-term consequence propagation
10. P2 content diversity and dynamic career arcs

This order maximizes perceived player improvement early while preserving simulation stability.

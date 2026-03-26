# Complete Game Code & Operations Guide

This guide is the practical, organized "how-to" for running, extending, and finishing the Survivebest game codebase.
Use it as the primary operations manual for day-to-day development and completion work.

---

## 1) What is already completed in code

Survivebest already includes implemented systems across core life-sim pillars:

- **Simulation backbone:** world clock, day slices, weather, event hub, bootstrap/recovery utilities.
- **Character simulation:** needs, health/conditions/recovery, emotion/conflict, personality/value systems.
- **World and society:** law/crime/justice, elections, town simulation, NPC schedule/autonomy/career.
- **Economy/content loops:** inventory, economy, grocery, recipes, ordering, crafting, contracts.
- **Presentation/UI flow:** splash → main menu → world creator → character creator → household maker → gameplay; HUD/feed/action popups.
- **Depth and pacing systems:** AI drama director, autonomous story generation, progression/milestones, family memory and relationship compatibility.
- **Validation/readiness tooling:** integration dry runs, balance advisor, readiness reporting, stability monitor.

If you need subsystem specifics, pair this guide with:

- `README.md`
- `Docs/DEEP_SYSTEMS_ASSETS_TUTORIAL.md`
- `Docs/COMPLETE_GAMEPLAY_SYSTEM_AUDIT.md`
- `Docs/FullGameAfterCodingChecklist.md`

---

## 2) Repository organization map (where to do what)

### Runtime game code
- `Assets/Scripts/Core` → simulation orchestration, progression, balancing, persistence, system glue.
- `Assets/Scripts/World` → world-time/weather/genetics/presentation-state world logic.
- `Assets/Scripts/NPC` → scheduling, autonomy, careers.
- `Assets/Scripts/UI` → screens, HUD, navigation, popup flow, presentation adapters.
- `Assets/Scripts/*` domain folders (`Health`, `Social`, `Commerce`, `Crime`, etc.) → feature-specific systems.

### Testing
- `Assets/Tests/EditMode` → Unity EditMode test suites across gameplay domains.

### Design/spec/reference docs
- `Docs/Reference` → content catalogs (foods, jobs, skills, crimes, etc.).
- `Docs/Resolvers` → resolver/pipeline contracts.
- Top-level `Docs/*.md` → audits, checklists, integration/tutorial docs.

---

## 3) Complete setup and run how-to

1. Install **Unity 2022 LTS or newer**.
2. Open Unity Hub and add project folder `Survivebest`.
3. Open the active simulation scene (or create one for wiring).
4. Ensure package resolution from `Packages/manifest.json`.
5. In-scene, wire core managers first:
   - `GameEventHub`
   - `WorldClock`
   - `DaySliceManager`
   - `HouseholdManager`
   - `LocationManager`
   - `WeatherManager`
6. Add at least one character object with:
   - `CharacterCore`
   - `NeedsSystem`
   - `HealthSystem`
   - `EmotionSystem`
   - `SocialSystem`
   - `ActivitySystem`
7. Wire menu/gameplay controllers if building full user flow:
   - `MainMenuFlowController`
   - `WorldCreatorScreenController`
   - `CharacterCreatorDashboardController`
   - `HouseholdMakerScreenController`
   - `GameplayScreenController`
8. Press Play and verify event feed/HUD updates.

---

## 4) Completion workflow (what to do next, in order)

Follow this sequence whenever the goal is "finish and organize the game":

1. **Stability first**
   - Run existing EditMode suites.
   - Resolve compile/test regressions.
2. **Hookup completeness**
   - Use scene/prefab checklist docs and readiness reports.
   - Ensure all optional UI refs are assigned.
3. **Save/load parity**
   - Verify every active subsystem state serializes/restores correctly.
4. **Balancing pass**
   - Run dry-run + balance advisor profile checks.
   - Tune economy/needs/story cadence.
5. **PlayMode authored-scene smoke tests**
   - Validate full-loop player journey and edge cases.
6. **Content depth pass**
   - Fill catalog/reference gaps in jobs, recipes, events, dialogue, and progression rewards.
7. **Release gating**
   - Execute Alpha checklist and remaining doc audits.

---

## 5) How to add new gameplay code cleanly

When adding a new system:

1. Place the system in the most specific domain folder (or create one if needed).
2. Keep logic deterministic and event-driven where possible.
3. Publish meaningful events through `GameEventHub` for UI/readability.
4. Add save/load model coverage if runtime state persists.
5. Add or update EditMode tests under `Assets/Tests/EditMode`.
6. Update at least one operations/reference doc so the feature stays discoverable.

Definition of done for a new system:

- Compiles in Unity.
- Has at least baseline tests.
- Emits readable event outputs.
- Has documented wiring steps.
- Is represented in save/load if stateful.

---

## 6) How to keep code organized over time

Use these maintenance rules:

- Keep one primary responsibility per system class.
- Avoid silent cross-domain coupling; use explicit service references or event contracts.
- Co-locate tests with behavior area (EditMode naming by domain).
- Prefer additive extension over rewriting stable, tested systems.
- Update docs/checklists in the same PR as code changes.

Recommended periodic housekeeping:

- Weekly: run readiness and dry-run tooling, record regressions.
- Per milestone: update completion checklist docs and content gap audits.
- Before merge: verify no new undocumented manager/wiring dependency was introduced.

---

## 7) Practical "how-to" for common goals

### A) Add a new life activity
- Extend activity/minigame catalog sources.
- Hook action into popup/controller flow.
- Apply need/emotion/skill/reward effects.
- Add tests for availability and outcomes.

### B) Add a new NPC profession
- Add role definition and requirements in career systems.
- Ensure schedule patterns and service-availability checks exist.
- Connect to economy/pay cadence and world incidents where relevant.

### C) Add a new social narrative beat
- Add event pattern to story/director systems.
- Ensure relationship memory and reputation impacts are applied.
- Surface readable UI feedback card/text.

### D) Add a new home-life interaction
- Add hotspot/action option + popup path.
- Connect housing/needs/appearance/emotion consequences.
- Ensure inventory/economy dependency is explicit (if item/tool required).

---

## 8) One-page execution checklist

Use this condensed checklist each time you push toward "complete":

- [ ] Project compiles in Unity.
- [ ] Core managers are wired.
- [ ] Menu-to-gameplay flow runs without dead ends.
- [ ] Save/load preserves active systems.
- [ ] EditMode tests pass locally/CI.
- [ ] Balance dry-runs reviewed and applied.
- [ ] Content catalogs updated for newly added loops.
- [ ] Docs updated (setup + extension + reference).

---

## 9) If you only read one thing

For full completion efforts, do this combination every cycle:

1. Build/test → 2) Wire/validate scenes → 3) Save/load checks → 4) Balance tuning → 5) Documentation sync.

That loop is the fastest path to a complete, stable, and organized Survivebest build.

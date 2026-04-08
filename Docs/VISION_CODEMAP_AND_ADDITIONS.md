# Vision Codemap: What Exists in Code, What It Supports, and What to Add Next

This document translates the current repository into a practical product map for your vision:

- **Beauty** (readable, emotionally expressive presentation)
- **Function** (real playable life loops)
- **Anything can happen** (broad choices + systemic consequences)

It is designed as a fast decision tool for "what exists" and "what should be added next."

---

## 1) What the current codebase already contains (by gameplay pillar)

### A. Simulation backbone (world state and progression)

Use these systems to keep the world running coherently over time:

- `WorldClock`, `DaySliceManager`, `GameEventHub`, `WorldEventDirector`, `WeatherManager`, `WorldPersistenceCullingSystem`
- These provide time flow, daily sequencing, event publication, ambient world incidents, weather state, and off-screen persistence/culling support.

**Vision role:** This is the foundation for predictable causality and long-run simulation stability.

### B. Character lifecycle and identity

Use these systems for growth, age, personality, and inherited identity:

- `CharacterCore`, `LifeStageManager`, `AgingExperienceSystem`, `LifeMilestonesEngine`
- `GeneticsSystem`, `VisualGenome`, `PhenotypeResolver`, `BloodlineInheritanceResolver`
- `PersonalityMatrixSystem`, `PersonalityDecisionSystem`, `MoralValueSystem`, `PreferenceSystem`, `MindStateSystem`

**Vision role:** Enables evolving people, not static avatars.

### C. Needs, health, stress, recovery

Use these systems for body/mental pressure and survival realism:

- `NeedsSystem`, `HealthSystem`, `MedicalConditionSystem`, `AdvancedHealthRecoverySystem`, `InjuryRecoverySystem`, `HealthcareGameplaySystem`
- `StatusEffectSystem`, `PsychologicalGrowthMentalHealthEngine`, `SeasonalAllergySystem`

**Vision role:** Creates consequence depth and meaningful risk/recovery loops.

### D. Social, relationship, and narrative dynamics

Use these systems for social chain reactions and story emergence:

- `SocialSystem`, `DialogueSystem`, `NarrativePromptSystem`, `InteractionDialogueBridge`
- `RelationshipMemorySystem`, `RelationshipCompatibilityEngine`, `SocialDramaEngine`, `RomanticTensionSystem`, `LoveLanguageSystem`
- `AutonomousStoryGenerator`, `AIDirectorDramaManager`, `AdaptiveLifeEventsDirector`

**Vision role:** Supports “anything can happen” through gossip, conflict, memory, and emergent incident pacing.

### E. Economy, work, crafting, and household viability

Use these systems to model resources, money, jobs, and living costs:

- `EconomyManager`, `InventoryManager`, `EconomyInventorySystem`
- `GrocerySystem`, `OrderingSystem`, `RecipeSystem`, `CraftingProfessionSystem`
- `NpcCareerSystem`, `ScheduleSystem`, `NpcScheduleSystem`, `NPCAutonomyController`
- `HousingPropertySystem`, `HouseholdChoreSystem`

**Vision role:** Enables survival pressure and strategy diversity (stable, ambitious, risky, chaotic runs).

### F. Law, crime, justice, and governance

Use these systems for legal constraints and civic outcomes:

- `LawSystem`, `CrimeSystem`, `JusticeSystem`, `SubstanceSystem`, `RehabilitationSystem`, `ParoleEvaluationSystem`
- `ElectionCycleSystem`

**Vision role:** Gives high-stakes consequence routing with recovery pathways.

### G. Presentation, UI flow, and player readability

Use these systems to make the simulation understandable and actionable:

- Main flow and screens: `MainMenuFlowController`, `WorldCreatorScreenController`, `HouseholdMakerScreenController`, `GameplayScreenController`, `CharacterScreenController`, `LoadGameScreenController`, `SettingsPageController`
- In-session readability: `GameHUD`, `JournalFeedUI`, `DialogueOverlayController`, `SidebarContextMenu`, `ActionPopupController`, `GameplayPresentationStateCoordinator`, `AvatarPresentationStateResolver`

**Vision role:** Converts hidden simulation into readable player-facing decisions.

### H. Save/load and operational safety

Use these systems to preserve continuity and improve iteration discipline:

- `SaveGameManager`, `SaveSchemaMigrationTests`
- `IntegrationDryRunService`, `BalanceTuningAdvisor`, `ValidationToolkit`, `SimulationStabilityMonitor`, `AssetReadinessReporter`

**Vision role:** Supports production control while scaling complexity.

---

## 2) How this maps to your stated vision

Your vision doc asks for **beauty + function + anything can happen** from a USA-baseline life simulation.

Current code status against that goal:

- **Beauty:** Strong technical base exists (presentation resolvers, portrait/render hooks, UI orchestration), but visual asset depth and feedback polish still depend on scene/prefab hookup and content production.
- **Function:** Core life loops are broadly represented in code (needs/health/social/economy/career/law/home), with unusually wide domain coverage for a foundation-phase project.
- **Anything can happen:** Incident + memory + director systems already create nonlinear potential; the biggest remaining gap is reliability of cross-system consequences in authored gameplay scenes and clearer player-facing explanation chains.

---

## 3) What to add next (priority order)

Apply the Breadth/Depth/Consequence gate to all additions.

### Priority 1 — Consequence explanation layer (highest value)

Add a dedicated “Why this happened” causal trace model surfaced in HUD/feed cards:

- Add a lightweight consequence trace DTO (source event, affected systems, resulting state shifts).
- Log chain snippets in event cards and tooltips.
- Add tests for multi-hop causal narration.

**Why first:** Your systems are already deep; clarity now multiplies player trust and retention.

### Priority 2 — Recovery path framework

Standardize comeback arcs for severe spirals (debt, addiction, social collapse, burnout, legal crisis):

- Add explicit recovery opportunity generators per domain.
- Add “minimum viable rebound” guarantees so failure states remain playable.
- Add balancing tests for rebound feasibility windows.

**Why second:** Prevents the simulation from feeling punitive or dead-ended.

### Priority 3 — Cross-system reaction contracts

Formalize contract-level links between major domains:

- Sleep/fatigue ↔ work performance/pay risk ↔ relationship tension ↔ legal/financial outcomes.
- Weather/housing quality ↔ health/mood ↔ attendance/performance.
- Gossip/reputation ↔ job opportunity ↔ law/community response.

**Why third:** Converts many parallel systems into one truly emergent world.

### Priority 4 — USA baseline differentiators in runtime data

Expand district/county policy + cost-of-living + service-access presets:

- Region profile assets (prices, weather volatility, labor demand, policing strictness, healthcare access).
- Election/law changes that concretely alter those values over time.

**Why fourth:** Makes the “USA baseline world model” materially felt in each run.

### Priority 5 — Content authoring throughput tools

Add templates and generators for dialogue/events/jobs/recipes/quests:

- Authoring assistants that enforce schema validity.
- Batch import/export for content tables.
- Coverage dashboards per system.

**Why fifth:** Needed to scale from “systems exist” to “systems feel alive every day.”

### Priority 6 — Long-run simulation QA automation

Expand run-based validation:

- 30/60/120-day seeded run harness checks.
- KPI assertions: bankruptcy rate bands, recovery success rates, relationship volatility, legal-event frequency bands.
- Regression snapshots for tuning changes.

**Why sixth:** Protects simulation integrity as content and rules grow.

---

## 4) Suggested implementation checklist for your next milestone

1. Add consequence-trace event schema + UI rendering.
2. Add recovery generators for debt/addiction/legal/social burnout.
3. Add three mandatory cross-domain reaction contracts.
4. Add 3–5 region profiles with differentiated law/economy/service/weather values.
5. Add long-run seeded simulation assertions in automated tests.
6. Run dry-run + balance advisor and compare KPI deltas before/after.

---

## 5) Short answer

Your codebase already contains a strong **simulation-first architecture** with most major life pillars represented.
The highest-impact additions are not “more isolated systems,” but:

1) **Consequence explainability**,
2) **Recovery-path reliability**, and
3) **Stronger cross-system contracts**.

Those three will make your vision feel cohesive, understandable, and replayable.

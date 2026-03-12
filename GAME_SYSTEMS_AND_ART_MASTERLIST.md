# Survivebest Master List: Systems, Flows, and Art Objects

This file is a comprehensive implementation inventory for the current codebase.

## 1) Implemented Runtime Systems (by script)

### Events
- `Assets/Scripts/Events/GameEventHub.cs`

### World / Time / Environment
- `Assets/Scripts/World/WorldClock.cs`
- `Assets/Scripts/World/WeatherManager.cs`
- `Assets/Scripts/World/WeatherEffectSystem.cs`
- `Assets/Scripts/World/BirthdayManager.cs`
- `Assets/Scripts/World/WorldCreatorManager.cs`
- `Assets/Scripts/World/VisualGenome.cs`
- `Assets/Scripts/World/GeneticsSystem.cs`

### Core Character / Household / Lifecycle
- `Assets/Scripts/Core/CharacterCore.cs`
- `Assets/Scripts/Core/HouseholdManager.cs`
- `Assets/Scripts/Core/FamilyManager.cs`
- `Assets/Scripts/Core/LegacyManager.cs`
- `Assets/Scripts/LifeStage/LifeStageManager.cs`
- `Assets/Scripts/Core/BodyCompositionSystem.cs`
- `Assets/Scripts/Core/SaveGameManager.cs`
- `Assets/Scripts/Core/SkillSystem.cs`
- `Assets/Scripts/Core/SkillTreeSystem.cs`
- `Assets/Scripts/Core/DaySliceManager.cs`
- `Assets/Scripts/Core/LongTermProgressionSystem.cs`
- `Assets/Scripts/Core/PersonalityDecisionSystem.cs`

### Needs / Health / Medical / Status
- `Assets/Scripts/Needs/NeedsSystem.cs`
- `Assets/Scripts/Health/HealthSystem.cs`
- `Assets/Scripts/Health/MedicalConditionSystem.cs`
- `Assets/Scripts/Health/InjuryRecoverySystem.cs`
- `Assets/Scripts/Health/AdvancedHealthRecoverySystem.cs`
- `Assets/Scripts/Status/StatusEffectSystem.cs`
- `Assets/Scripts/Crime/SubstanceSystem.cs`

### Emotions / Social / Dialogue / Conflict
- `Assets/Scripts/Emotion/EmotionSystem.cs`
- `Assets/Scripts/Social/SocialSystem.cs`
- `Assets/Scripts/Social/RelationshipMemorySystem.cs`
- `Assets/Scripts/Dialogue/DialogueSystem.cs`
- `Assets/Scripts/Dialogue/NarrativePromptSystem.cs`
- `Assets/Scripts/Emotion/ConflictSystem.cs`

### Activity / Routine / Minigame
- `Assets/Scripts/Activity/ActivitySystem.cs`
- `Assets/Scripts/Activity/DailyRoutineSystem.cs`
- `Assets/Scripts/Minigames/MinigameManager.cs`

### Commerce / Inventory / Recipes / Ordering
- `Assets/Scripts/Economy/EconomyInventorySystem.cs`
- `Assets/Scripts/Catalog/IngredientCatalog.cs`
- `Assets/Scripts/Catalog/SupplyCatalog.cs`
- `Assets/Scripts/Food/FoodDatabase.cs`
- `Assets/Scripts/Food/DrinkDatabase.cs`
- `Assets/Scripts/Commerce/GrocerySystem.cs`
- `Assets/Scripts/Commerce/RecipeSystem.cs`
- `Assets/Scripts/Commerce/OrderingSystem.cs`
- `Assets/Scripts/Commerce/CraftingProfessionSystem.cs`

### Quest / Contracts
- `Assets/Scripts/Quest/ContractBoardSystem.cs`
- `Assets/Scripts/Quest/QuestOpportunitySystem.cs`

### NPC / Town Simulation
- `Assets/Scripts/NPC/NpcScheduleSystem.cs`
- `Assets/Scripts/NPC/NpcCareerSystem.cs`

### Law / Crime / Justice
- `Assets/Scripts/Society/LawSystem.cs`
- `Assets/Scripts/Crime/CrimeSystem.cs`
- `Assets/Scripts/Crime/JusticeSystem.cs`

### Location / Interaction / Transport
- `Assets/Scripts/Location/LocationManager.cs`
- `Assets/Scripts/Location/TownSimulationSystem.cs`
- `Assets/Scripts/Location/HousingPropertySystem.cs`
- `Assets/Scripts/Interaction/Interactable.cs`
- `Assets/Scripts/Interaction/InteractionController.cs`
- `Assets/Scripts/Interaction/HomeInteractionHotspot.cs`
- `Assets/Scripts/Transport/CarSystem.cs`

### UI / Menus / Gameplay Panels / Popups
- `Assets/Scripts/UI/SplashScreenController.cs`
- `Assets/Scripts/UI/MainMenuFlowController.cs`
- `Assets/Scripts/UI/LoadGameScreenController.cs`
- `Assets/Scripts/UI/SettingsPageController.cs`
- `Assets/Scripts/UI/SettingsTabsController.cs`
- `Assets/Scripts/UI/WorldCreatorScreenController.cs`
- `Assets/Scripts/UI/HouseholdMakerScreenController.cs`
- `Assets/Scripts/UI/GameplayScreenController.cs`
- `Assets/Scripts/UI/GameHUD.cs`
- `Assets/Scripts/UI/CharacterScreenController.cs`
- `Assets/Scripts/UI/CharacterRosterHUD.cs`
- `Assets/Scripts/UI/CharacterPortraitRenderer.cs`
- `Assets/Scripts/UI/JournalFeedUI.cs`
- `Assets/Scripts/UI/JournalCardView.cs`
- `Assets/Scripts/UI/SidebarContextMenu.cs`
- `Assets/Scripts/UI/ActionPopupController.cs`
- `Assets/Scripts/UI/ZoneScenePanel.cs`
- `Assets/Scripts/UI/SuccessionUI.cs`
- `Assets/Scripts/UI/UIEventFeedbackRouter.cs`
- `Assets/Scripts/UI/BuildModeManager.cs`
- `Assets/Scripts/UI/FurnitureStoreController.cs`
- `Assets/Scripts/UI/FurniturePlaceable.cs`
- `Assets/Scripts/UI/UIGlassStyleController.cs`
- `Assets/Scripts/UI/TraitPillTagView.cs`

### Rendering / View / Utility
- `Assets/Scripts/Rendering/CharacterSortingGroupBinder.cs`
- `Assets/Scripts/View/ViewManager.cs`
- `Assets/Scripts/Utility/AssetReadinessReporter.cs`
- `Assets/Scripts/Utility/PlaceholderGenerator.cs`
- `Assets/Scripts/Utility/SimulationSceneBootstrapper.cs`

### Automated Tests
- `Assets/Tests/EditMode/EconomyInventorySystemTests.cs`
- `Assets/Tests/EditMode/SaveSchemaMigrationTests.cs`
- `Assets/Tests/EditMode/QuestOpportunitySystemTests.cs`
- `Assets/Tests/EditMode/LongTermProgressionSystemTests.cs`
- `Assets/Tests/EditMode/PersonalityDecisionSystemTests.cs`
- `Assets/Tests/EditMode/HousingPropertySystemTests.cs`

---

## 2) Gameplay Flows Currently Implemented

1. **Splash → Main Menu flow**
   - Entry scene handoff and menu routing.

2. **Main Menu navigation stack**
   - New/Load/Settings/creator routes with back-and-forth transitions.

3. **Settings flow**
   - Tabbed settings with persistent values and UI styling hooks.

4. **Load/save slot flow**
   - 3-slot save metadata + payload restore (world time, room, per-character state).

5. **World creator flow**
   - Tab-driven world setup and generation handoff.

6. **Household maker flow**
   - Character/household setup route into gameplay.

7. **Gameplay HUD flow**
   - Left nav/context + center world panel + resource/vitals UI surfaces.

8. **Sidebar context action flow**
   - Room/theme-sensitive actions generated by `SidebarContextMenu`.

9. **Action popup flow**
   - Confirm/cancel + action outcome events for buy/sell/trade/medical/forage/camp/skills/animal sightings.

10. **Animal sighting encounter flow**
    - Encounter preview, optional animal image, success/fail resolution, payout + stat/skill/status consequences.

11. **Interaction click flow**
    - Mouse click → hotspot/interactable/furniture/build-mode checks → action execution.

12. **Home hotspot flow**
    - Doorway, hygiene, bed, TV, desk, pantry, store, build-toggle, etc. with simulation effects.

13. **Build mode flow**
    - Toggle build mode, drag/move furniture only when build mode is enabled.

14. **Furniture store flow**
    - Purchase + spawn + build handoff loop.

15. **Car travel flow**
    - Drive with fuel/condition/cleanliness + needs impacts + travel result events.

16. **Ordering flow**
    - Place order, delayed delivery, apply food effects on arrival.

17. **Grocery + pantry flow**
    - Buy/consume/quantity checks + inventory events.

18. **Recipe cooking flow**
    - Use inventory to cook, fallback patterns available in day-slice orchestration.

19. **Minigame flow**
    - Start overlay → resolve via skill + needs → apply post effects + XP + events.

20. **Daily routine autonomy flow**
    - Hourly autonomous activity based on needs/emotion state.

21. **Day-slice orchestration flow**
    - 10-stage day progression with real per-stage outcomes.

22. **Weather flow**
    - Season/day weighted weather selection + weather events.

23. **Weather effects flow**
    - Weather state start/tick effects on active character/household needs and health.

24. **Needs decay flow**
    - Minute/hour updates and critical thresholds.

25. **Medical condition flow**
    - Add condition → hourly damage/needs penalties → expire/heal.

26. **Status effect flow**
    - Add/refresh/tick/expire with generated library and event emission.

27. **Substance lifecycle flow**
    - Use → active duration/tick → dependency progression → withdrawal/crash end-state.

28. **Crime → justice flow**
    - Crime commit → enforcement chance → justice outcome.

29. **Persistent justice flow**
    - Fines/debt + jail timer + hourly sentence penalties + release event.

30. **Social/dialogue flow**
    - Intent + success chance + relationship and emotion updates.

31. **Conflict/violence flow**
    - Fight resolution with damage, relationship loss, and crime linkage.

32. **Aging/life-stage flow**
    - Year progression updates life stage and body composition.

33. **Genetics morph flow**
    - Founder/inheritance genes mapped to body/face/appearance traits with age remapping.

34. **Succession flow**
    - Character death and successor handoff hooks.

35. **Contract board flow**
    - Contract spawn → accept → complete/fail/expire with timed deadlines and reward payout.

36. **NPC schedule flow**
    - Hourly schedule resolution for town NPC states by job archetype.

37. **Skill tree flow**
    - Skill XP progression unlocks node-based profession branches with prerequisites.

38. **Injury treatment flow**
    - Injury start → untreated/treated hourly outcomes → recovery.

39. **Save schema migration flow**
    - Load payload schema check → migrate legacy payloads → apply state safely.

40. **Scene bootstrap flow**
    - Bootstrap helper auto-creates missing core managers and triggers readiness auto-wire/report pass.

41. **UX feedback routing flow**
    - Simulation events map to UI text pulse + severity-based SFX hooks.

42. **Recipe unlock progression flow**
    - Recipe starts locked → skill/node requirements met → unlock event → craft enabled.

43. **Unified inventory authority flow**
    - Item instance created → ownership scope applied → reserve/equip/spoil/depreciate lifecycle → resale value evaluation.

44. **Town lot lifecycle flow**
    - Lot open/close by hour → district/lot risk+wealth signals update → route costs vary by weather.

45. **NPC living-town flow**
    - NPC schedule state resolves against jail/health/weather/business-hours, then routes to home/work/social/commercial lots with memory/reputation drift.

46. **NPC career flow**
    - Role assignment → shift attendance/pay/performance updates → promotion/demotion/unemployment outcomes with role-linked gear/tools.

47. **Relationship memory flow**
    - Social event recorded → trust/fear/respect/loyalty updated → public events propagate gossip/reputation effects.

48. **Quest opportunity flow**
    - Opportunity published → accepted → objective progress → success/fail/expire with branching follow-up opportunities.

49. **Long-term progression flow**
    - Aspirations progress → milestones unlock perks → fame/infamy/prestige shift social class and legacy bonuses.

50. **Personality decision flow**
    - Trait/personality profile resolves autonomous weighted choices, including biased/irrational tendencies.

51. **Advanced health recovery flow**
    - Region-specific injury added → medication/treatment/rehab loop → infection/relapse/disability/scar outcomes.

52. **Housing utility/property flow**
    - Bills processed daily → utilities on/off, room-quality/cleanliness/clutter/storage dynamics, repair request lifecycle, ownership transfer.

53. **Crafting profession flow**
    - Blueprint/station/tool/skill checks → ingredient/substitution handling → quality-weighted crafted output item instance.

---

## 3) State Lifecycles with Explicit Start/End (implemented)

- **Status effects**: add → tick hourly → expire.
- **Medical conditions**: add → apply hourly penalties → heal/expire.
- **Substance effects**: use start → active ticking → end crash/withdrawal.
- **Jail sentence**: sentence start → hourly countdown/penalties → release.
- **Orders**: placed → pending → delivered.
- **Minigame**: start → running → completed.
- **Day-slice stage**: enter stage → execute stage logic → advance/loop.
- **Weather**: resolve new state → apply immediate state effects → hourly weather effects.

---

## 4) Art Objects You Need (master production list)

Use this as the practical art backlog for scenes/prefabs/UI hookups.

### A. Frontend / Screen-Level Art
- Splash background(s), logo treatments, loading particles, prompt text effects.
- Main menu backgrounds, button states (idle/hover/pressed/disabled), logo lockups.
- Load-slot cards (empty/filled/selected/hover), metadata icons.
- Settings tab icons + panel frames + slider/toggle/dropdown skins.

### B. Gameplay HUD + World Panel
- Left navigation buttons/icons for all location categories.
- World panel maps/backgrounds for each area/time/weather variant.
- Resource row icon set and chip backgrounds.
- Character vitals panel bars/icons/frames for needs and health.
- Event feed card art variants (info/warn/critical).

### C. Popup and Context Menu Art
- Popup container glass variants and transition anims.
- Confirm/cancel/action button skins.
- Context action icons (commerce, medical, social, survival, training).
- Animal sighting popup assets:
  - encounter frame variant,
  - preview image mask frame,
  - rarity/tag badges,
  - success/fail state overlays.

### D. Character / Paper-Doll / Portrait Layers
- Head/neck/ears/eyes/nose/mouth/eyebrows/eyelashes/makeup layers.
- Hair variants (front/side/back), body silhouettes, facial-feature overlays.
- Life-stage visual variants (baby→elder).
- Genetics-driven facial/skin detail overlays.
- Trait pill backgrounds and status icon set.

### E. World & Environment Art
- Room backgrounds per theme (residential/nature/store/work/hospital/civic).
- Transition overlays for day/night cycle.
- Weather VFX sets (rain/snow/fog/storm/blizzard/heat haze/wind).
- Weather hazard overlays and iconography.

### F. Home/Build/Furniture Art
- Build mode toggles, gizmo handles, drag indicators.
- Furniture item sprites/tiles/prefab visuals.
- Home hotspot decals and icon overlays.
- Furniture store item cards + prices/rarity tags.

### G. Commerce / Food / Crafting Art
- Ingredient icons, supply icons, recipe card visuals.
- Vendor logos and menu card templates.
- Delivery pending/delivered markers and notification icons.

### H. Society / Crime / Justice Art
- Law profile icons and enforcement meter visuals.
- Crime type icons and incident markers.
- Justice outcomes: warning/fine/probation/jail badges.
- Debt/jail state indicators for character UI.

### I. Transport Art
- Vehicle sprites by type + condition/cleanliness visual states.
- Fuel/repair/clean service icons.
- Travel route marker FX and travel result indicators.

### J. Minigame Art
- Overlay frame, timer/progress widgets, success/fail banners.
- Activity-specific icon packs (cooking/repairs/first aid/cleaning).

### K. Audio (still needed for immersion)
- UI click/hover/confirm/cancel sounds.
- Menu ambience, location ambience, weather loops.
- Event stingers (critical need, justice outcome, delivery, death/succession).

---

## 5) What’s Still Missing / Next Features To Build

Status update: items 1 through 8 and 10 now have foundational runtime systems; item 9 remains major production work.


Additional progress this pass:
- Added **L. Autonomous Story Generator** (`AutonomousStoryGenerator`) with weighted incidents and local news feed entries.
- Added **M. World Persistence / Simulation Culling Rules** (`WorldPersistenceCullingSystem`) for lot activation budgets, off-screen NPC abstraction, and catch-up simulation.
- Added **N. AI Director / Drama Manager** (`AIDirectorDramaManager`) to pace disruption/recovery beats based on boredom and tension.
- Added **O. Animation-State / Feedback / Juice Layer** (`AnimationFeedbackJuiceSystem`) to map simulation events into animation/SFX/VFX/UI feedback cues.
- Enhanced NPC agents with zone-aware routine routing + personality-driven crowded social conflict escalation in `NpcScheduleSystem`.
- Added service-staffing observability in `NpcCareerSystem` (`CountOnDuty`, `IsServiceAvailable`) plus critical-service outage events for unattended shifts.
- Added family-level social consequence propagation in `RelationshipMemorySystem` so one insult can affect wider family reputation.


1. **Scene prefab completeness**
   - Most systems are code-ready but still need full prefab/inspector wiring in Unity scenes.

2. **Unified inventory/economy model**
   - A single financial/inventory authority system can replace dispersed wallet/item logic.

3. **NPC AI schedules and jobs**
   - Extend from household-centric simulation to robust town-wide NPC behavior.

4. **Quest/contract framework**
   - Formalize sighting contracts, job boards, deadlines, and chain rewards.

5. **Crafting depth & skill trees**
   - Introduce unlockable recipes, trait synergies, and profession branches.

6. **Combat/health realism pass**
   - Better injury types, treatment outcomes, and long-term recovery timelines.

7. **Save schema versioning/migration**
   - Add version tags and migration logic for future backward compatibility.

8. **Automated tests**
   - PlayMode/EditMode tests for WorldClock, needs decay, save/load restore integrity, justice/substance lifecycle, and day-slice transitions.

9. **Balancing pass**
   - Tune coefficients (needs decay, justice penalties, weather impact, substance dependency) for fair progression.

10. **Final polish layer**
    - Animation, VFX, SFX, transitions, accessibility, controller support, and UX microinteractions.

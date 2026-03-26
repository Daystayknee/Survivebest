# Household Maker Unity Hookup (Detailed Implementation Guide)

This guide is a step-by-step setup for wiring `HouseholdMakerScreenController` so the Household Maker feels like a full life-sim authoring flow (Sims-style depth).

---

## 1) Scene and Object Layout

Create or open your Household Maker scene and establish this hierarchy:

1. `Canvas_HouseholdMaker` (Screen Space Overlay or Camera)
2. `Panel_Root` (whole screen content)
3. `Panel_Header`
4. `Panel_LeftTabs`
5. `Panel_MainContent`
6. `Panel_RightSummary`
7. `Panel_FooterActions`
8. `CharacterPreviewRoot` (3D/2D preview setup)

Attach `HouseholdMakerScreenController` to a controller object like:

- `HouseholdMakerController` (empty GameObject under scene root)

---

## 2) Required Script References (Inspector Wiring)

In `HouseholdMakerScreenController`, assign these first:

- `menuFlowController` -> `MainMenuFlowController` in your menu flow object.
- `householdManager` -> the active `HouseholdManager` singleton/scene object.
- `characterCamera` -> preview camera (usually dedicated Creator camera).
- `characterPivot` -> transform that rotates the preview character when no pivot list is used.
- `gameEventHub` -> global `GameEventHub`.

If any of these are missing, core actions (back/start/cycle character/events) won’t fully function.

---

## 3) Tab Panels and Navigation Setup

The enum order used by this controller:

- Appearance
- Genetics
- FamilyGenetics
- Clothing
- Shoes
- Hats
- Accessories
- Makeup
- Traits
- Skills
- Relationships
- Household

### Steps

1. Create one panel GameObject per tab under `Panel_MainContent`.
2. Add each panel into `tabPanels` list in Inspector.
3. For each list entry:
   - Set `Tab` enum
   - Set `Root` to the corresponding panel object
4. Toggle `wrapTabs = true` if `NextTab`/`PreviousTab` should loop.

Wire buttons:

- `Button_TabNext` -> `HouseholdMakerScreenController.NextTab()`
- `Button_TabPrev` -> `HouseholdMakerScreenController.PreviousTab()`
- Optional direct button -> `SetTab(int)`

---

## 4) Text and Summary UI Wiring

Assign text outputs:

- `tabText` -> current tab label
- `characterNameText` -> active member display name
- `creatorSummaryText` -> full multiline authored household summary
- `familyLockStateText` -> “Family Locked In / Editable”
- `familyVisionText` -> short narrative family vision block

`creatorSummaryText` now includes:

- Family profile (surname, district, origin, planning priority)
- Living profile (lot, budget, vibe, archetype)
- Conflict and progression profile (children/pets/career, generational goal)
- Lifestyle profile (commute strategy, daily rhythm)
- Build profile (room blueprint list)
- Member profile (life stage/species/talent/lock state)

---

## 5) Family Planning Inputs (All New Fields)

Create these UI controls and wire methods:

### Text Inputs

- Surname input -> `SetFamilySurname(string)`
- Home district input -> `SetHomeDistrict(string)`
- Story prompt input -> `SetHouseholdStoryPrompt(string)`
- Generational goal input -> `SetGenerationalGoal(string)`
- Commute strategy input -> `SetCommuteStrategy(string)`
- Daily rhythm input -> `SetDailyRhythm(string)`

### Dropdowns (use enum index order)

- Origin focus -> `SetOriginFocus(int)`
- Planning priority -> `SetPlanningPriority(int)`
- Lot type -> `SetLotType(int)`
- Budget tier -> `SetBudgetTier(int)`
- Household vibe -> `SetHouseholdVibe(int)`
- Conflict approach -> `SetConflictApproach(int)`
- Household archetype -> `SetHouseholdArchetype(int)`

### Toggles

- Wants children soon -> `SetWantsChildrenSoon(bool)`
- Includes pets -> `SetIncludesPets(bool)`
- Prioritizes career mobility -> `SetPrioritizesCareerMobility(bool)`

---

## 6) Room Blueprint Authoring UI (High-Detail Feature)

`roomPlans` lets designers author interior intent per room. Build a “Room Planner” section:

### Suggested UI

1. Dropdown: Room Type
2. Dropdown: Style Direction
3. Slider/Input: Priority (1..5)
4. InputField: Notes
5. Button: Add Room
6. Button: Remove Last Room

### Method wiring

- Add Room button -> `AddRoomPlan(int roomType, int styleDirection, int priority, string notes)`
- Remove button -> `RemoveLastRoomPlan()`

### Recommended supporting UI behavior

- Mirror the currently-authored list in a scroll view.
- Rebuild list whenever a room is added/removed.
- Keep enum option labels exactly aligned to enum order.

---

## 7) Character Preview Controls

If using multi-art variants:

- Fill `characterArtPivots` with each character rig/sprite pivot.
- Use:
  - `NextArtPivot()`
  - `PreviousArtPivot()`
  - `SetActiveArtPivot(int)`

Rotation and zoom wiring:

- Drag left/right -> `RotateCharacter(float direction)`
- Scroll/pinch -> `ZoomCamera(float delta)`

Settings:

- `rotateAllArtPivots = true` to keep all variants aligned together.
- Tune `rotateSpeed`, `zoomSpeed`, `minZoom`, `maxZoom` in Inspector.

---

## 8) Household Member Flow

Wire character cycle and lock flow:

- Next member button -> `NextHouseholdCharacter()`
- Previous member button -> `PreviousHouseholdCharacter()`
- Lock active member button -> `ToggleLockActiveCharacter()`
- Lock whole family draft -> `LockFamilyDraft()`
- Unlock whole family draft -> `UnlockFamilyDraft()`

This enables preserving selected member appearance while iterating other members.

---

## 9) Save/Load Draft Integration

Use a slot id system (`slot_01`, `playerA_main`, etc.) and wire:

- Save button -> `SaveFamilyDraft(string slotId)`
- Load button -> `LoadFamilyDraft(string slotId)`

Persisted data includes:

- Active tab/active character
- Lock states
- All planning and narrative fields
- Archetype/routine strategy
- Room blueprint plans

Implementation uses `PlayerPrefs` JSON key:

- `household_draft_<slotId>`

---

## 10) Events and Analytics

When tabs change or genetics validation runs, controller emits `SimulationEvent` via `GameEventHub`.

Use this for:

- UX telemetry (tab usage frequency)
- QA event logs
- Tutorial prompts (e.g., first visit to FamilyGenetics tab)

---

## 11) Buttons for Flow and Validation

Wire:

- Back -> `Back()`
- Start/Continue -> `StartGame()`
- Validate genetics -> `ValidateHouseholdGenetics()`
- Open family-genetics tab -> `OpenFamilyGeneticsSection()`

---

## 12) Art and UI Asset Requirements (Production Ready)

To make this screen feel premium:

1. **Background Layers**
   - Neutral creator studio
   - Family/house-themed overlay
2. **Icon Set**
   - Per-tab icons
   - Planning category icons (budget, lot, conflict, pets, etc.)
3. **Typography**
   - Header, section, body, metadata styles
4. **Card/Panel Prefabs**
   - Reusable row cards for room plans and member summaries
5. **Feedback FX**
   - Button press, selection highlight, draft saved toast
6. **State Colors**
   - Locked vs editable
   - Priority badges (1-5)

---

## 13) QA Checklist (Step-by-Step)

1. Open screen and confirm default tab and summary text appear.
2. Switch through all tabs with next/prev and direct buttons.
3. Edit all text fields; verify summary updates instantly.
4. Toggle all booleans (children/pets/career) and verify summary.
5. Add 3+ room plans and verify display order and priorities.
6. Remove last room plan and verify summary redraw.
7. Cycle household members; verify name and composition update.
8. Lock one member, then lock family; verify state labels.
9. Save draft, leave screen, reload, and confirm full restoration.
10. Run genetics validation and check event output/logging.
11. Start game from household screen and ensure flow proceeds.

---

## 14) Common Pitfalls

- **Dropdown enum mismatch:** UI options out of order cause wrong enum values.
- **Null text refs:** summary won’t render unless text refs are assigned.
- **Missing `householdManager`:** character cycle/lock and member composition become inert.
- **Missing `gameEventHub`:** event telemetry fallback may be unavailable.
- **Overwriting slot IDs:** accidental draft replacement if slot naming is not scoped.

---

## 15) Recommended Next Upgrades

If you want true “Sims-level” depth next:

1. Multiple saved room layouts per household (not just one list).
2. Budget simulation preview (monthly cost estimate).
3. Relationship graph editor in Household tab.
4. Trait conflict warnings and compatibility suggestions.
5. Visual room plan thumbnails/icons per row.
6. Import/export household blueprint JSON for sharing.

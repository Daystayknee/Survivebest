# Gameplay Vertical Slice Spec

This spec defines the exact gameplay loops Unity must prove before broad scene wiring expands.

## Milestone 0 — Human Day Slice

### Ship gate rule
No system is considered implemented until it is playable, visible, and persistent through the gameplay scene UI.

A feature is only complete if:
- it is reachable from scene UI
- a player can trigger it manually
- the result is visible in HUD, world state, or action feedback
- it survives save/load correctly

The following do **not** count as completion paths:
- debug buttons only
- setup scripts only
- tests only
- inspector-only state injection

### Milestone goal
Prove one complete playable human day from wake-up through save/load parity without relying on debug-only entry points.

### Non-negotiable ship checklist
1. Wake in apartment.
2. Inspect needs, money, and world summary.
3. Shower / bathroom.
4. Eat.
5. Text/contact someone.
6. Go to work or skip.
7. Return home.
8. Resolve household pressure.
9. Trigger one social event.
10. End day.
11. Save.
12. Reload and verify parity.

This is the entire near-term ship target for the gameplay slice. Do not broaden scope until all 12 steps satisfy the ship gate.

### Player-facing acceptance tests

#### 1. Wake in apartment
**Scene entry point**
- Start New Game.
- Enter gameplay scene.
- Active character is spawned or restored in apartment.

**UI control used**
- Main menu `Start New Game` or a real `Continue`/`Load Slot` flow.

**Expected simulation changes**
- Character has valid starting needs.
- World time is morning.
- Active household and home context exist.

**Expected visible feedback**
- Apartment panel is visible.
- Character portrait is visible.
- Current room is visible.
- Time/date is visible.

**Save/load expectations**
- After reload, the character is still in the apartment with correct room and time parity.

#### 2. Inspect needs, money, world summary
**Scene entry point**
- HUD loads automatically when gameplay scene opens.
- Player can open the character/world panel if collapsed.

**UI control used**
- HUD toggle, overview panel button, or default visible HUD.

**Expected simulation changes**
- Values come from real simulation state, not placeholder text.

**Expected visible feedback**
- Hunger/hydration/energy or top needs are shown.
- Money/balance is shown.
- Date/time/weather/location are shown.
- One short world summary line is shown.

**Save/load expectations**
- The same values restore consistently after reload.

#### 3. Shower / bathroom
**Scene entry point**
- Player selects a bathroom hotspot or a context action in the home scene.

**UI control used**
- Bathroom hotspot, room action button, or contextual action menu entry.

**Expected simulation changes**
- Hygiene changes.
- Bladder changes if implemented.
- Time advances.
- Grooming/appearance continuity updates if already supported.

**Expected visible feedback**
- The action appears in the action list.
- The action resolves visibly.
- Updated need/status values are shown after completion.

**Save/load expectations**
- Post-shower state persists after reload.

#### 4. Eat
**Scene entry point**
- Player selects fridge, kitchen, meal, quick meal, cook, or order-food fallback from gameplay UI.

**UI control used**
- Kitchen hotspot, food choice list, or home action menu.

**Expected simulation changes**
- Hunger changes.
- Hydration changes if applicable.
- Inventory/pantry/order funds update if applicable.
- Time advances.

**Expected visible feedback**
- Eat action is selectable from scene UI.
- Hunger visibly changes.
- Food source or result is shown in HUD/feed/action feedback.

**Save/load expectations**
- Consumed food, money, and need changes remain correct after reload.

#### 5. Text/contact someone
**Scene entry point**
- Player opens phone/social/contact UI.
- Player selects a contact.
- Player chooses a text/call/social interaction.

**UI control used**
- Phone button, contact list button, or social panel action.

**Expected simulation changes**
- Relationship value and/or memory changes.
- Mood/emotion may update.
- Social thought/journal line may update.

**Expected visible feedback**
- Contact list is visible.
- Interaction is selectable.
- Feedback line is visible.

**Save/load expectations**
- Relationship/social outcome remains after reload.

#### 6. Go to work or skip
**Scene entry point**
- Action menu shows work choice when scheduled.
- Player chooses `Go To Work` or `Skip Work`.

**UI control used**
- Work button, commute action, or schedule action list entry.

**Expected simulation changes**
- If going to work: time passes and income/performance/progression/stress can change.
- If skipping work: attendance/reputation/money consequences can change.

**Expected visible feedback**
- The button exists.
- Result text or state change is visible.
- Time visibly advances.

**Save/load expectations**
- Work outcome state persists after reload.

#### 7. Return home
**Scene entry point**
- Player travels back home through location/map UI or via automatic post-work resolution.

**UI control used**
- Travel button, map selection, home button, or return-home prompt.

**Expected simulation changes**
- Current lot/room updates correctly.
- Travel time/cost is reflected if applicable.

**Expected visible feedback**
- Location changes back to home.
- Room/home summary is visible again.

**Save/load expectations**
- Reload restores the correct home location.

#### 8. Resolve household pressure
**Scene entry point**
- Player sees a household issue in UI.
- Player chooses a visible resolving action.

**UI control used**
- Home issue card, household panel action, or room-context action.

**Expected simulation changes**
- One real household pressure decreases.
- Some other resource changes: time, energy, cleanliness, money, or relationship tension.

**Expected visible feedback**
- At least one real pressure is shown in HUD/feed/home panel.
- A visible action exists to address it.
- The pressure warning/card clears or downgrades after resolution.

**Save/load expectations**
- Resolved or unresolved state remains accurate after reload.

#### 9. Trigger one social event
**Scene entry point**
- Event occurs through text, return-home flow, household interaction, or another gameplay-triggered social source.

**UI control used**
- Popup response button, feed card acknowledgement, or dialogue choice.

**Expected simulation changes**
- At least one of relationship, memory, mood, reputation, or household climate changes.

**Expected visible feedback**
- Popup/feed/dialogue/social card is visible.
- Player can respond or acknowledge it.

**Save/load expectations**
- Event aftermath persists after reload.

#### 10. End day
**Scene entry point**
- Player chooses a sleep or end-day action.

**UI control used**
- Bed hotspot, sleep button, or explicit end-day action.

**Expected simulation changes**
- Sleep resolves.
- Daily systems tick.
- Routine/memory/journal/day summary may update.

**Expected visible feedback**
- Day closes clearly.
- Date/time advances.
- New-day summary or state change appears.

**Save/load expectations**
- Start-of-next-day state is stable after reload.

#### 11. Save
**Scene entry point**
- Player uses actual save UI from the gameplay flow.

**UI control used**
- Save menu button and slot selection UI.

**Expected simulation changes**
- State is captured from real gameplay progress.

**Expected visible feedback**
- Slot selection is shown.
- Save confirmation is shown.
- Slot metadata is visible if possible.

**Save/load expectations**
- Saved slot contains the current human day slice state.

#### 12. Reload and verify parity
**Scene entry point**
- Player leaves and reloads from the load screen or uses in-session load flow.

**UI control used**
- Continue/load button and slot selection UI.

**Expected simulation changes**
- No obvious reset or broken references appear.

**Expected visible feedback**
- Correct location is shown.
- Correct time/date is shown.
- Correct needs/money are shown.
- Correct household pressure state is shown.
- Correct relationship/social aftermath is shown.
- Same-day continuation appears intact.

**Save/load expectations**
- Reloaded state matches the pre-save state according to the visible proofs above.

### Minimum visible proof rule
For every checklist item, define the exact on-screen proof that the system is real. If there is no visible proof, it is not integrated yet.

Preferred proof examples:
- hunger bar changes
- money text changes
- world summary updates
- action disappears after completion
- dirty-room warning clears
- relationship blurb changes
- save slot metadata changes

### Milestone board states
Track each slice item through these explicit states:
- Not Reachable
- Reachable but Fake
- Reachable and Works
- Persists Across Save/Load
- Ship-Ready

### Stable test scenario
Create exactly one repeatable scenario for this slice with:
- one active human character
- one small apartment
- one household pressure already present
- one contact available
- one work option available
- one social event source available
- pantry/food source available
- save slot enabled

### Recommended first implementations
**Household pressure:** Dirty dishes.
- easy to understand
- visibly human
- easy UI feedback
- affects home feel
- connects to chores/home systems

**Social event:** Household member or contact reacts to your day.
- ties into texting/contact
- ties into work choice
- ties into household pressure
- ties into relationship state

### Delivery order
1. Wake in apartment.
2. Inspect HUD needs/money/world summary.
3. Shower / bathroom.
4. Eat.
5. Text/contact someone.
6. Go to work or skip.
7. Return home.
8. Resolve household pressure.
9. Trigger one social event.
10. End day.
11. Save.
12. Reload and verify parity.

This order intentionally moves from passive display to action execution, then consequence, then persistence proof.

### Architecture proof points
- bootstrap flow initializes a deterministic session
- command layer drives every player action
- facades produce HUD-facing state
- presentation coordinator resolves one coherent screen state
- save/load parity remains intact across the full loop


## Beyond Milestone 0 — Endless Human Life Sim

Milestone 0 is the ship gate, not the ceiling. Once the Human Day Slice is stable, the project should expand into a continuous, multi-choice life sim / survival RPG where repeated days, accumulating consequences, and diverging human strategies define the experience.

Use `Docs/EndlessLifeSimFramework.md` as the follow-on framework for widening the game only after the ship gate is proven.

Expansion after Milestone 0 should prioritize:
- repeated multi-day continuity
- multiple valid ways to solve pressures
- delayed consequences that shape later days
- broader human-condition simulation across needs, money, health, social life, and environment
- RPG progression that deepens capability without trivializing the sim

## Slice 1 — Vampire Night Loop

### Goal
Prove the secrecy/hunger/consequence loop after the human day slice is stable.

### Required flow
1. Wake at dusk.
2. Surface hunger pressure.
3. Take one work or night errand.
4. Trigger one suspicious interaction.
5. Present one feeding choice.
6. Apply one concealment consequence.
7. Return before sunrise.
8. Save and reload with continuity.

### Architecture proof points
- vampire-specific warnings pulse correctly
- command layer supports feeding, compulsion, and concealment actions
- presentation state reflects hunger, suspicion, and timeline consequences
- save/load preserves vampire runtime state

# Unity Hookup - RPG Rewards, Achievements, and Celebration Popups

Use this setup if you want stronger RPG feedback in-scene:
- skill level-up celebrations
- goal completion popups
- achievement unlocks

This guide is intentionally concrete and tells you exactly what objects to create and what fields to assign.

Use this when you want the shortest practical setup path for reward popups in Unity.
It focuses on scene wiring, event flow, and first-time validation rather than implementation details.

## Scene objects to add

### Recommended scene hierarchy

```text
Scene
├─ Systems
│  ├─ GameEventHub
│  ├─ LongTermProgressionSystem
│  └─ AchievementSystem
└─ UI
   └─ RewardPopupCanvas
      └─ RewardPopupRoot
         ├─ TitleText
         ├─ BodyText
         └─ RpgCelebrationPopupController
```

### 1. Global event hub
Make sure the scene has:
- `GameEventHub`

If your scene does not already have one:
1. Create an empty GameObject
2. Name it `GameEventHub`
3. Add the `GameEventHub` component

### 2. Progression systems
Add these to a systems object such as `GameSystems`:
- `LongTermProgressionSystem`
- `AchievementSystem`

If you do not already have a systems object:
1. Create an empty GameObject
2. Name it `GameSystems`
3. Add both components there

Wire in the Inspector:
- `AchievementSystem -> Game Event Hub`
- `AchievementSystem -> Long Term Progression System`

### Recommended systems hierarchy
The simplest setup is:

```text
GameSystems
├─ GameEventHub
├─ LongTermProgressionSystem
└─ AchievementSystem
```

If your project already splits global systems differently, that is fine.
The important part is that the references are assigned and all three objects are active.

### Achievement definitions setup
On `AchievementSystem`, add entries to the `Achievements` list.

Example first achievement:
- **Achievement Id** = `hunt_1`
- **Title** = `First Hunt Level`
- **Description** = `Reach level 1 in Hunting`
- **Trigger Type** = `SkillLevelReached`
- **Trigger Key** = `Hunting`
- **Threshold** = `1`

### 3. Celebration popup UI
Create a UI object and add:
- `RpgCelebrationPopupController`

### Step-by-step popup creation
1. Create a **Canvas** if your scene does not already have one
2. Inside it, create an empty panel object named:
   - `RewardPopupRoot`
3. Add two text elements:
   - `TitleText`
   - `BodyText`
4. Add `RpgCelebrationPopupController` to the popup root or a parent UI object

Assign:
- `Game Event Hub`
- `Popup Root`
- `Title Text`
- `Body Text`
- optional `Animator`

### Suggested first-pass UI setup
For the easiest test:

1. Create a screen-space canvas
2. Create `RewardPopupRoot` as a child object
3. Add a background image if you want visible framing
4. Create `TitleText`
5. Create `BodyText`
6. Disable `RewardPopupRoot` in the hierarchy if you want it hidden until the first event
7. Add `RpgCelebrationPopupController` to `RewardPopupRoot`

### Optional controller values worth checking
If you expose these in the Inspector, verify:
- **Skill Level Up Trigger** = `SkillLevelUp`
- **Goal Completed Trigger** = `GoalCompleted`
- **Achievement Trigger** = `AchievementUnlocked`
- **Visible Seconds** = around `2.75`

### Exact field mapping
- **Game Event Hub** -> scene `GameEventHub`
- **Popup Root** -> `RewardPopupRoot`
- **Title Text** -> `TitleText`
- **Body Text** -> `BodyText`
- **Animator** -> optional, only if you want animated celebration triggers

## Where the events come from

### Skill level-up
`SkillSystem` now emits `SimulationEventType.SkillLevelUp` when experience crosses a level boundary.

This means:
- if a character gains enough XP in `Hunting`, `Cooking`, etc.
- and the XP crosses a level threshold
- the popup controller can react immediately
- the popup title should display `Skill Level Up!`

### Goal complete
`LongTermProgressionSystem` now emits `SimulationEventType.GoalCompleted` when a goal finishes.

That means goals can now produce a distinct “Goal Complete!” reward moment instead of only silently updating fame/prestige.

The popup title should display `Goal Complete!`

### Achievement unlocked
`AchievementSystem` listens to progression events and emits `SimulationEventType.AchievementUnlocked`.

That gives you a final celebratory layer on top of skill/goal events.

The popup title should display `Achievement Unlocked!`

## Recommended animator triggers
If your popup Animator is used, create triggers such as:
- `SkillLevelUp`
- `GoalCompleted`
- `AchievementUnlocked`

### What to do in Animator
If you want a flashy popup:
1. Add an `Animator` to the popup root
2. Create three trigger parameters:
   - `SkillLevelUp`
   - `GoalCompleted`
   - `AchievementUnlocked`
3. Make transitions from idle/hidden to your animation states using those triggers

If you do not need animation yet, leave the `Animator` field empty and validate text-only popups first.

## Quick validation
1. Give a character enough XP to cross a skill level threshold
2. Confirm a popup appears
3. Complete a progression goal
4. Confirm a goal-complete popup appears
5. Add an achievement definition for that event
6. Confirm the achievement popup appears afterward

If you are debugging your very first setup, validate in this order:
- skill level-up first
- goal completion second
- achievements third

That keeps the event chain simple and makes it easier to see which link is missing.

## Full first-time test walkthrough

### Test A - Skill level-up
1. Enter Play Mode
2. Find a character with `SkillSystem`
3. Give that skill enough XP to cross a threshold
4. Watch for:
   - `SkillLevelUp` event
   - level-up popup
   - any linked animation/audio feedback

Expected popup:
- title: `Skill Level Up!`
- body: the event `Reason`

### Test B - Goal completion
1. Add one simple goal to `LongTermProgressionSystem`
2. Make its target very small, like `1`
3. Trigger `ProgressGoal(...)`
4. Watch for:
   - `GoalCompleted` event
   - goal popup
   - fame/prestige update

Expected popup:
- title: `Goal Complete!`
- body: the event `Reason`

### Test C - Achievement unlock
1. Add an achievement definition that listens for the exact event you are testing
2. Trigger the related event
3. Confirm:
   - the achievement becomes unlocked
   - `AchievementUnlocked` event fires
   - achievement popup appears

Expected popup:
- title: `Achievement Unlocked!`
- body: `Achievement unlocked: <Title>`

## Common mistakes

### No popup appears
Check:
- `GameEventHub` exists in scene
- `RpgCelebrationPopupController` is enabled
- `Popup Root` is assigned
- `Title Text` and `Body Text` are assigned
- the popup object is part of an active canvas
- the event-producing systems are enabled

### Skill level-up never fires
Check:
- the character really has `SkillSystem`
- enough XP is being added to cross the configured level threshold
- you are testing a valid skill name like `Hunting`
- `GameEventHub` is available for the event to publish through

### Achievement never unlocks
Check:
- `Trigger Type` matches the event you expect
- `Trigger Key` matches exactly, including case/spelling
- `Threshold` is reachable
- `AchievementSystem` is wired to both `GameEventHub` and `LongTermProgressionSystem`

### Popup flashes too quickly or never hides
Check:
- `Visible Seconds` is greater than `0.5`
- the popup root is the same object being activated/deactivated
- no other script is immediately hiding or reconfiguring the popup

### Animator exists but no animation plays
Check:
- the `Animator` field is assigned
- the trigger parameter names match exactly:
  - `SkillLevelUp`
  - `GoalCompleted`
  - `AchievementUnlocked`
- the controller transitions are driven by those triggers

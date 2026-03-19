# Unity Hookup - RPG Rewards, Achievements, and Celebration Popups

Use this setup if you want stronger RPG feedback in-scene:
- skill level-up celebrations
- goal completion popups
- achievement unlocks

This guide is intentionally concrete and tells you exactly what objects to create and what fields to assign.

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

### Goal complete
`LongTermProgressionSystem` now emits `SimulationEventType.GoalCompleted` when a goal finishes.

That means goals can now produce a distinct “Goal Complete!” reward moment instead of only silently updating fame/prestige.

### Achievement unlocked
`AchievementSystem` listens to progression events and emits `SimulationEventType.AchievementUnlocked`.

That gives you a final celebratory layer on top of skill/goal events.

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

## Quick validation
1. Give a character enough XP to cross a skill level threshold
2. Confirm a popup appears
3. Complete a progression goal
4. Confirm a goal-complete popup appears
5. Add an achievement definition for that event
6. Confirm the achievement popup appears afterward

## Full first-time test walkthrough

### Test A - Skill level-up
1. Enter Play Mode
2. Find a character with `SkillSystem`
3. Give that skill enough XP to cross a threshold
4. Watch for:
   - `SkillLevelUp` event
   - level-up popup
   - any linked animation/audio feedback

### Test B - Goal completion
1. Add one simple goal to `LongTermProgressionSystem`
2. Make its target very small, like `1`
3. Trigger `ProgressGoal(...)`
4. Watch for:
   - `GoalCompleted` event
   - goal popup
   - fame/prestige update

### Test C - Achievement unlock
1. Add an achievement definition that listens for the exact event you are testing
2. Trigger the related event
3. Confirm:
   - the achievement becomes unlocked
   - `AchievementUnlocked` event fires
   - achievement popup appears

## Common mistakes

### No popup appears
Check:
- `GameEventHub` exists in scene
- `RpgCelebrationPopupController` is enabled
- `Popup Root` is assigned
- `Title Text` and `Body Text` are assigned

### Skill level-up never fires
Check:
- the character really has `SkillSystem`
- enough XP is being added to cross the configured level threshold
- you are testing a valid skill name like `Hunting`

### Achievement never unlocks
Check:
- `Trigger Type` matches the event you expect
- `Trigger Key` matches exactly, including case/spelling
- `Threshold` is reachable
- `AchievementSystem` is wired to both `GameEventHub` and `LongTermProgressionSystem`

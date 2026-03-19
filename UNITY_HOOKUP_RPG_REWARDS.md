# Unity Hookup - RPG Rewards, Achievements, and Celebration Popups

Use this setup if you want stronger RPG feedback in-scene:
- skill level-up celebrations
- goal completion popups
- achievement unlocks

## Scene objects to add

### 1. Global event hub
Make sure the scene has:
- `GameEventHub`

### 2. Progression systems
Add these to a systems object such as `GameSystems`:
- `LongTermProgressionSystem`
- `AchievementSystem`

Wire in the Inspector:
- `AchievementSystem -> Game Event Hub`
- `AchievementSystem -> Long Term Progression System`

### 3. Celebration popup UI
Create a UI object and add:
- `RpgCelebrationPopupController`

Assign:
- `Game Event Hub`
- `Popup Root`
- `Title Text`
- `Body Text`
- optional `Animator`

## Where the events come from

### Skill level-up
`SkillSystem` now emits `SimulationEventType.SkillLevelUp` when experience crosses a level boundary.

### Goal complete
`LongTermProgressionSystem` now emits `SimulationEventType.GoalCompleted` when a goal finishes.

### Achievement unlocked
`AchievementSystem` listens to progression events and emits `SimulationEventType.AchievementUnlocked`.

## Recommended animator triggers
If your popup Animator is used, create triggers such as:
- `SkillLevelUp`
- `GoalCompleted`
- `AchievementUnlocked`

## Quick validation
1. Give a character enough XP to cross a skill level threshold
2. Confirm a popup appears
3. Complete a progression goal
4. Confirm a goal-complete popup appears
5. Add an achievement definition for that event
6. Confirm the achievement popup appears afterward

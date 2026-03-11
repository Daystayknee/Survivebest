# Project Architecture — 2D Life Sim Survival RPG (Unity)

Use this document as the **source of truth** for GitHub Copilot/Codex prompts and implementation consistency.

## Master System Prompt

```text
I am building a 2D Life Sim Survival RPG in Unity. The game uses a Point-and-Click interface with Modular Paper Doll characters.
Technical Requirements:
 * Modular 2D Avatars: Each character is a collection of separate SpriteRenderer child objects (Head, Nose, Mouth, Eyes, FrontHair, SideHair, BackHair, Chest, Hips, Booty, Arms, Thighs, Calves, Feet).
 * Scaling Genetics: Use Transform.localScale on specific body parts to handle different body shapes (e.g., 'BustSize', 'HipWidth', 'Height').
 * Life Stages: Create an Enum for: Baby, Infant, Toddler, Child, Preteen, Teen, Young Adult, Adult, Older Adult, Elder.
 * Household System: The player can switch between characters in a household. When a character dies, the player selects a successor.
 * Point-and-Click: Interactions are handled via Raycasting on 2D objects.
Please generate a base C# script called CharacterCore.cs that holds these variables and a BodyManager.cs to handle the sprite swapping and scaling.
```

## Unity Hierarchy Setup (Manual)

1. Create an Empty GameObject named `PlayerCharacter`.
2. Create child GameObjects for every body part (`Nose`, `Mouth`, etc.).
3. Add a `SpriteRenderer` to each child.
4. Configure Sorting Order so `BackHair` is lowest (`-1`) and `Nose`/`Eyes` are highest (`5`).

## Next Prompt — Interaction System

```text
Write a PointAndClickManager.cs for Unity 2D. It should detect when I left-click on a GameObject with an Interactable tag. If I click a character, it should switch the active controlled character. If I click a 'Door', it should trigger a background swap.
```

## Coding Workflow Tips

- **Context rule:** When starting a new script, include previous scripts and tell Copilot: “Using the CharacterCore script as a reference, write a new script for [X system].”
- **Naming conventions:** Use **PascalCase** for methods/types and **camelCase** for variables/fields.
- **Aging logic prompt:**

```text
Create a TimeManager that tracks hours, days, and years. Every [X] days, tell the CharacterCore to update its Life Stage.
```

## 4-Step Roadmap with Prompts

### Step 1 — Character Creator UI

```text
Reference my BodyPartSwapper script. I need a CharacterCreatorUI script. Create functions for NextNose(), PreviousNose(), and a slider-based function for SetHeight(float value). These functions should call the swapper to update the sprites and localScale of the body parts in real-time as I slide the bars.
```

### Step 2 — Living Engine (Survival & Needs)

```text
Create a NeedsSystem.cs. It should track: Hunger, Energy, Bladder, Hygiene, and Mood.
 * Use a Timer so that every 5 real-world seconds, 1 in-game minute passes.
 * Every in-game hour, decrease Hunger by 5 and Energy by 2.
 * If Bladder hits 100, trigger an 'Accident' event.
 * If Mood is low, the character's 'Social' interactions should have a higher chance of failing.
```

### Step 3 — Point-and-Click Navigation

```text
Write a LocationManager.cs. It should hold a list of Room objects (Kitchen, Bedroom, etc.). Each Room has a background Sprite and a 'Spawn Point'. When I click a 'Navigation Button' in the UI, fade the screen to black, swap the background sprite, and move all characters in the household to that room's spawn point.
```

### Step 4 — Succession & Death (Legacy)

```text
Create a LegacyManager.cs.
 * Listen for the OnCharacterDeath event from the LifeStageManager.
 * When triggered, find all other characters in the HouseholdManager.
 * Open a UI window showing portraits of the survivors.
 * When the player clicks a survivor, set IsPlayerControlled = true for that character and resume the game.
 * If there are no survivors, trigger a 'Game Over' screen.
```

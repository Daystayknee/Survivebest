# Project Architecture — 2D Life Sim Survival RPG (Unity)

Use this file as the **source of truth** for Copilot/Codex so generated scripts remain system-aware and compatible.

## 1) Master System Prompt (Bootstrapping)

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

## 2) Unity Hierarchy Setup (Manual)

1. Create an Empty GameObject named `PlayerCharacter`.
2. Create child GameObjects for each body part (`Head`, `Nose`, `Mouth`, `Eyes`, `FrontHair`, `SideHair`, `BackHair`, `Chest`, `Hips`, `Booty`, `Arms`, `Thighs`, `Calves`, `Feet`).
3. Add a `SpriteRenderer` to each child.
4. Configure Sorting Order so `BackHair` is lowest (`-1`) and `Nose`/`Eyes` are highest (`5`).

## 3) Core Prompt — Interaction System

```text
Write a PointAndClickManager.cs for Unity 2D. It should detect when I left-click on a GameObject with an Interactable tag. If I click a character, it should switch the active controlled character. If I click a 'Door', it should trigger a background swap.
```

## 4) Workflow Rules for Consistent Copilot Output

- **Context rule:** “Using `CharacterCore` as reference, write [X system].”
- **Naming conventions:** PascalCase for methods/types, camelCase for private fields.
- **Aging prompt template:**

```text
Create a TimeManager that tracks hours, days, and years. Every [X] days, tell the CharacterCore to update its Life Stage.
```

## 5) System-Aware Integration Contract (Important)

Use this before generating any advanced script:

```text
Treat this as a system-aware architecture. Ensure new scripts integrate with existing managers and events:
- CharacterCore owns identity, life-stage, and references to Needs/Social/Visual systems.
- HouseholdManager owns active household roster and active player switching.
- UI scripts call manager APIs only (no duplicated gameplay logic in UI).
- Interaction scripts use Interactable + 2D raycasts and dispatch typed actions/events.
- WorldClock drives time progression; time events update Needs, Weather, and LifeStage systems.
- Death flow dispatches OnPlayerDeath/OnCharacterDeath and transitions into Succession UI.
Return code with clear public API methods and event hooks.
```

## 6) Roadmap Prompts (Phase 1)

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

## 7) Remaining Core Systems Prompts (Phase 2)

### 7.1 Deep Genetics & Appearance

```text
Using my CharacterCore script, write a VisualGenome.cs script.
The Logic:
 * Define a struct called PhysicalTraits containing floats for: NeckLength, ShoulderWidth, BustSize, ChestDepth, ArmThickness, HipWidth, BootySize, ThighGirth, CalfGirth, and Height.
 * Write a function ApplyPhysicalTraits(PhysicalTraits traits) that finds the specific child GameObjects (Neck, Chest, Hips, etc.) and adjusts their localScale based on these floats.
 * Write a GenerateRandomDNA() function that creates a random set of traits within a 'Human' range (e.g., 0.8 to 1.3 scale).
 * Include a function InheritTraits(PhysicalTraits parentA, PhysicalTraits parentB) that blends these values with a 10% chance of random mutation.
```

### 7.2 Relationship & Role System

```text
Create a SocialSystem.cs.
Requirements:
 * Create a Relationship class that stores a TargetCharacterID, a RelationshipValue (-100 to 100), and a RelationshipType Enum (Family, Roommate, Partner, Lover, Enemy).
 * Create a UI-bound function AddHouseholdMember(RelationshipType type). When clicked, it should spawn a new 2D character using the VisualGenome script and add them to the HouseholdManager.
 * Write a function UpdateRelationship(int amount) that triggers based on interactions (like 'Chat' or 'Argue') and updates the Enum if the value crosses a certain threshold (e.g., if value > 80 and type is Roommate, change to Partner).
```

### 7.3 Calendar, Holidays, and Weather

```text
Write a WorldClock.cs script for a 2D Life Sim.
Details:
 * Track Minutes, Hours, Days, Months, and Years.
 * Define a Season Enum (Spring, Summer, Fall, Winter).
 * Create a Holiday system: Define specific days (e.g., Day 25 of Winter = Winterfest). Broadcast an event when a Holiday starts.
 * Create a WeatherManager that changes a global WeatherState variable (Sunny, Rainy, Snowy) based on the Season.
 * Aging Hook: Every time Years increases, call the AgeUp() function in the LifeStageManager for every character in the household.
```

### 7.4 Daily Life / Bathroom Loop

```text
Create an InteractionController.cs.
Mechanics:
 * Use 2D Raycasting to detect clicks on objects with an Interactable script.
 * If the player clicks a 'Toilet', move the active character to the toilet's position and play a 'Using' animation (or just a progress bar), then reset the Bladder need to 0.
 * If the player clicks a 'Fridge', trigger a 'Mini-game' event (a simple 2D click-matching game). If the mini-game is won, reset the Hunger need to 0.
 * Add 'Rest' (Bed) and 'Drink' (Sink) interactions following the same logic.
```

### 7.5 Legacy Succession UI

```text
Write a SuccessionUI.cs script.
Logic:
 * This script should remain disabled until it receives a OnPlayerDeath signal.
 * When enabled, it should clear a UI ScrollView and populate it with 'Character Cards' for every remaining member in the HouseholdManager.
 * Each card should show the character's Name, Life Stage, and a Portrait (using a Render Texture of their face).
 * When a card is clicked, set that character as the new ActivePlayer and reload the main game UI.
```

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


## 8) Interaction & Experience Layer Prompts (Phase 3)

### 8.1 Modular Minigame Framework

```text
Create a MinigameManager.cs for Unity.
Logic:
 * Use a Singleton pattern so it can be called from anywhere.
 * Create an Enum for MinigameType (e.g., Cooking, Repairs, FirstAid, Cleaning).
 * Write a function StartMinigame(MinigameType type, Action<bool> onComplete).
 * When a minigame starts, it should overlay a specific UI Canvas.
 * Example Logic: For 'Cooking', create a simple 'Click the moving bar in the green zone' mechanic.
 * On success or failure, return the result to the calling script (e.g., if successful, Hunger is restored more; if failed, an 'Injury' might occur).
```

### 8.2 Hobbies, Skills, and Talent Genetics

```text
Write a SkillSystem.cs that tracks a character's progress in different Hobbies.
Requirements:
 * Create a Dictionary<string, float> skillLevels for skills like Cooking, Fitness, Gaming, Art, and Social.
 * Add an Experience system where doing an activity (like 'Resting' or 'Eating' with a book) increases the skill.
 * The Genetic Link: Include a 'Talent' multiplier in CharacterData. If a character has the 'Artistic' trait from their parents, their Art skill should level up 1.5x faster.
 * Save and Load these skills using a JSON format so progress persists.
```

### 8.3 Illness, Injury, and Medical System

```text
Create a HealthSystem.cs that handles Illness and Injury.
Details:
 * Track a Vitality float (0-100). If it hits 0, the character dies regardless of their Life Stage.
 * Create a Condition class for things like 'The Flu', 'Broken Arm', or 'Infection'.
 * Contagion Logic: If a character has 'The Flu' and spends time in the same 'Room' (from the LocationManager) as another character, there is a 20% chance the illness spreads.
 * Include a Healing function that requires the 'FirstAid' minigame or 'Rest' to increase Vitality.
```

### 8.4 Birth and Family Expansion

```text
Write a FamilyManager.cs that handles adding new members.
Functions:
 * CreateRoommate(): Spawns a new character with random genetics and no biological relation to the household.
 * HaveBaby(CharacterData parentA, CharacterData parentB): Creates a new character at the 'Baby' Life Stage.
 * The Heritage Logic: The baby must inherit the VisualGenome (skin tone, eye shape, hip width) from the parents using the inheritance function we wrote earlier.
 * Automatically add the new character to the HouseholdUI portrait list.
```

### 8.5 Camera View Toggle (Chest-Up vs Full Body)

```text
Write a ViewManager.cs that controls the character's appearance in the UI.
Logic:
 * Use two Unity Cinemachine (or standard) cameras.
 * Camera A (Full Body): Positioned to see the whole character in the room.
 * Camera B (Portrait): Positioned to see the character from the chest up.
 * Add a button listener that toggles between these two cameras.
 * When in Portrait mode, ensure the BodyPartSwapper only renders the 'Head' and 'Torso' layers to a specific UI RawImage for clarity.
```

### 8.6 Optimization Prompt — Character Layer Cohesion

```text
Write a script that 'Parents' all my 2D body part Sprites to a single 'Sorting Group' component, so that when characters overlap, the entire character is treated as one layer to prevent their arms and legs from clipping through each other.
```


### 8.7 Weather Driven by Seasons

```text
Write a WeatherManager.cs that subscribes to WorldClock season/day events and sets a global WeatherState (Sunny, Rainy, Snowy).
- Winter should bias toward Snowy, Spring/Fall toward Rainy, Summer toward Sunny.
- Broadcast OnWeatherChanged when the state changes so UI/VFX can react.
```

### 8.8 Calendar Events and Holidays

```text
Extend WorldClock.cs to include Day/Month/Year progression, Season calculation, and holiday definitions.
- Emit OnDateChanged(day, month, year), OnSeasonChanged(season), and OnHolidayStarted(name, day, month, year).
- Keep existing minute/hour/day events for backward compatibility with NeedsSystem.
```


## 9) Production Expansion Prompts (Phase 4)

### 9.1 Aging and Life Stage Automation

```text
Create a LifeStageManager.cs that subscribes to WorldClock.OnYearPassed.
- Track AgeYears.
- Map age thresholds to LifeStage enum transitions.
- Call CharacterCore.SetLifeStage(...) when thresholds are crossed.
- Optionally reduce vitality in elder stage via HealthSystem.
```

### 9.2 Room Navigation and Scene Feel

```text
Write a LocationManager.cs with Room data (name, background sprite, spawn point).
- Add NavigateToRoom(string roomName).
- Fade to black, swap background, move household members to room spawn, then fade back in.
- Keep it event-safe and interrupt-safe if multiple navigation requests occur.
```

### 9.3 Camera Mode Toggle

```text
Create a ViewManager.cs using two cameras (full body + portrait).
- Toggle on button press.
- In portrait mode, switch to portrait layer/render path for head/torso clarity.
- Expose IsPortraitMode for UI state.
```

### 9.4 Character Sorting Cohesion

```text
Create CharacterSortingGroupBinder.cs.
- Ensure every paper-doll character root has a SortingGroup.
- Bind child SpriteRenderers to the group sorting layer/order to reduce inter-character clipping.
```


### 9.5 Food Variety and Nutrition Depth

```text
Create a FoodDatabase.cs and expand NeedsSystem nutrition effects.
- Define food categories and food items with hunger, energy, mood, hygiene, and vitality deltas.
- On fridge/cooking interaction, select food from FoodDatabase and apply full effects to needs/health.
- Include healthy and unhealthy outcomes so gameplay choices matter over time.
```

### 9.6 Appearance Depth (Hair/Eyes/Skin)

```text
Create an AppearanceManager.cs for modular paper-doll characters.
- Support hair styles, hair colors, eye colors, skin tones, skin issues (e.g., freckles, acne, rosacea, vitiligo), and beauty marks.
- Provide RandomizeAppearance() and setter methods for each appearance axis.
- Apply sprites/colors to the appropriate SpriteRenderers and expose an OnAppearanceChanged event for UI refresh.
```


### 9.7 Human Behavior Depth (Dialogue/Anger/Love/Fighting)

```text
Create EmotionSystem.cs, DialogueSystem.cs, and ConflictSystem.cs.
- EmotionSystem tracks Anger, Affection, and Stress with events.
- DialogueSystem resolves intents (chat/flirt/argue/insult/apologize) and updates relationship + emotional outcomes.
- ConflictSystem can trigger fights when anger/stress is high and apply health/relationship consequences.
```

### 9.8 Activities and Drinking Loop

```text
Create ActivitySystem.cs and DrinkDatabase.cs.
- Add activities: Rest, Workout, Read, Drive, Drink, Cook, Socialize.
- Drinks should restore hydration and affect mood/energy/health depending on type (water/coffee/soda/alcohol).
- Route effects through NeedsSystem and HealthSystem APIs.
```

### 9.9 Cars and Transport

```text
Create CarSystem.cs with car inventory and trip logic.
- Track car fuel and condition.
- DriveToRoom(roomName) should consume fuel/condition and call LocationManager.NavigateToRoom.
- Include refuel and repair functions.
```


### 9.10 Crime, Substances, and Justice by Area Laws

```text
Create LawSystem.cs + CrimeSystem.cs + SubstanceSystem.cs + JusticeSystem.cs.
- Define area law profiles where legality/enforcement differs by area.
- Include substances: alcohol, weed, prescription drugs, hard drugs.
- Crime events (theft/assault/etc.) should be processed by justice outcomes (warning/fine/jail).
- Location transitions should update active area law profile.
```

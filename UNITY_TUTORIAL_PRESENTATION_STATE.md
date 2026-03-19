# Unity Tutorial - Using the Presentation State System

This tutorial walks through the actual Unity workflow for the new presentation-state / asset-matrix system.

Use this when you want to:
- hook up a household/player character
- hook up a background NPC
- create the `MasterAssetMatrix` asset
- verify that posture/expression/overlay keys are resolving correctly

---

## Part 1 - What these pieces do

### `MasterAssetMatrix`
This is your lookup asset.
It stores rows like:
- trait/state name
- morph range
- life stage
- art key / file name

It answers questions like:
- “Which nose art variant should this 0.78 nose bridge use?”
- “Which overlay art should a medium bruise use?”

### `UnityPresentationStateHookup`
This is the Unity-facing component.
Attach it to a GameObject that already has `GeneticsSystem`.

It can:
- resolve presentation for a normal household/player character
- resolve presentation for a background NPC using `NpcScheduleSystem`
- query the `MasterAssetMatrix`
- force a refresh from a context menu

---

## Part 2 - Create the Master Asset Matrix asset

### Step-by-step
1. Open the **Project** window.
2. Create or choose a folder such as:
   - `Assets/Data/Art/`
3. Right-click in that folder.
4. Select:
   - **Create -> Survivebest -> Art -> Master Asset Matrix**
5. Name the asset:
   - `MasterAssetMatrix`

### Recommended location
- `Assets/Data/Art/MasterAssetMatrix.asset`

### What to enter first
Start with only a few rows:
- one nose row
- one mouth row
- one body row
- one overlay row

That gives you a small debugable matrix before scaling up.

---

## Part 3 - Hook up a household / player character

### Requirements on the character object
The GameObject should already have:
- `CharacterCore`
- `GeneticsSystem`

### Add the presentation component
1. Select the character GameObject or prefab.
2. Click **Add Component**.
3. Search for:
   - `Unity Presentation State Hookup`
4. Add it.

### Inspector setup
Set these fields:
- **Genetics System** -> drag the same object’s `GeneticsSystem`
- **Master Asset Matrix** -> assign `MasterAssetMatrix.asset`
- **Use Npc Profile** -> leave **off**
- **Resolve On Enable** -> usually **on**

### What happens
In this mode, the component uses:
- `GeneticsSystem.ApplyDynamicPresentationState()`

That means the active presentation state is resolved from the character’s current phenotype, needs, emotion, and illness read.

---

## Part 4 - Hook up a background NPC

Use this for background or “unused/off-focus” characters that still carry genetics and should still look alive.

### Requirements
The NPC object/prefab should have:
- `GeneticsSystem`
- `UnityPresentationStateHookup`

And your scene should have:
- `NpcScheduleSystem`

### Inspector setup
On `UnityPresentationStateHookup`:
- **Genetics System** -> assign the local `GeneticsSystem`
- **Master Asset Matrix** -> assign `MasterAssetMatrix.asset`
- **Use Npc Profile** -> **on**
- **Npc Schedule System** -> drag the scene’s `NpcScheduleSystem`
- **Npc Id** -> enter the matching id from the `NpcProfile`

### What happens
In NPC mode the hookup:
1. pulls the NPC profile from `NpcScheduleSystem`
2. reads stress, health, state, and reputation
3. calls `AvatarPresentationStateResolver.ResolveNpcState(...)`
4. applies the resulting posture/expression/overlay keys back to the phenotype/avatar layers

This is the important part if your NPCs are not active player characters but still need genetics/presentation logic.

---

## Part 5 - Force a refresh in the editor

### Context menu
On the `UnityPresentationStateHookup` component:
1. open the component menu
2. click:
   - **Resolve Presentation State Now**

Use this when:
- tweaking NPC ids
- testing an asset matrix row
- validating a prefab before entering Play Mode

---

## Part 6 - Turn on automatic refresh

If you want the component to keep updating while running:

### Enable these fields
- **Auto Refresh** -> on
- **Refresh Interval** -> e.g. `1.0` to `5.0`

### Recommended usage
- **creator/debug scenes**: short interval
- **background NPCs**: longer interval
- **stable prefabs**: leave off unless needed

---

## Part 7 - Query the asset matrix in code

You can use:

`UnityPresentationStateHookup.ResolveAssetEntry(traitOrState, normalizedValue)`

### Example uses
- `ResolveAssetEntry("NoseBridgeHeight", 0.78f)`
- `ResolveAssetEntry("LipFullness", 0.63f)`
- `ResolveAssetEntry("BruiseSeverity", 0.45f)`

This returns the best matching matrix row for the current character life stage.

---

## Part 8 - Recommended first test setup

Create one debug character and verify the full path before scaling up.

### Debug checklist
1. Create `MasterAssetMatrix.asset`
2. Add 3-5 starter rows
3. Add `GeneticsSystem` to a test character
4. Add `UnityPresentationStateHookup`
5. Assign the matrix
6. Use **Resolve Presentation State Now**
7. Inspect the phenotype/avatar layer keys

### Good first rows
- `NoseBridgeHeight`
- `LipFullness`
- `ChestBustPresentation`
- `Bruise`
- `StressWarning`

---

## Part 9 - Common setup mistakes

### Nothing changes when resolving
Check:
- `GeneticsSystem` is assigned
- the object actually has a populated phenotype
- `Resolve On Enable` or manual resolve is being used

### NPC mode does nothing
Check:
- **Use Npc Profile** is enabled
- `NpcScheduleSystem` is assigned
- `Npc Id` exactly matches an `NpcProfile.NpcId`

### Asset lookup returns null
Check:
- `MasterAssetMatrix.asset` is assigned
- the `TraitOrState` spelling matches the matrix entry
- the `MorphRange` contains the value
- the row’s life stage matches the character’s phenotype life stage

---

## Part 10 - Where to add these in the project

### Assets
- `Assets/Data/Art/MasterAssetMatrix.asset`

### Household / player prefabs
- add `UnityPresentationStateHookup` directly on the same object as `GeneticsSystem`

### NPC prefabs
- add `UnityPresentationStateHookup` on any NPC prefab/object that also owns a `GeneticsSystem`
- enable NPC mode and wire `NpcScheduleSystem` + `Npc Id`

### Creator/debug scenes
- add the hookup to preview characters to force refresh while authoring art

---

## Part 11 - Best practice

Do not try to fill the full matrix all at once.

Start with:
- a few facial features
- one or two overlays
- one UI feedback state

Then validate:
- lookup works
- posture/expression keys change
- NPC mode resolves correctly

Once that is stable, scale up to the full production matrix.

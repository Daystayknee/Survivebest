# Unity Tutorial - Using the Presentation State System

This tutorial walks through the actual Unity workflow for the new presentation-state / asset-matrix system.
It is intentionally detailed and assumes you want a hand-held setup path instead of high-level notes.

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

### What you should see in the Inspector
After creating it, click the asset and confirm you can see:
- an `Entries` list

If you do **not** see `Entries`, the asset was likely not created from the correct menu path.
Delete it and recreate it from:
- **Create -> Survivebest -> Art -> Master Asset Matrix**

### Recommended location
- `Assets/Data/Art/MasterAssetMatrix.asset`

### What to enter first
Start with only a few rows:
- one nose row
- one mouth row
- one body row
- one overlay row

That gives you a small debugable matrix before scaling up.

### Example starter row values
If you want a very literal first setup, add an `Entries` element with values like:
- **System** = `Face`
- **Region** = `Nose`
- **TraitOrState** = `NoseBridgeHeight`
- **VariantKey** = `face_nose_roman_02`
- **TriggerSource** = `PhenotypeResolver`
- **MorphRange** = `0.70` to `0.90`
- **LifeStage** = `Adult`
- **Layer** = `face_mid`
- **Palette** = `base`
- **FileName** = `face_nose_roman_02_adult_base_highbridge.png`
- **Enabled** = checked

### Add three more rows before your first test
For a more realistic smoke test, add at least these additional rows:

1. a mouth row such as:
   - **TraitOrState** = `LipFullness`
   - **MorphRange** = `0.55` to `0.80`
   - **VariantKey** = `face_lips_full_01`
2. a body row such as:
   - **TraitOrState** = `BodyMass`
   - **MorphRange** = `0.40` to `0.70`
   - **VariantKey** = `body_medium_01`
3. an overlay row such as:
   - **TraitOrState** = `BruiseSeverity`
   - **MorphRange** = `0.20` to `0.60`
   - **VariantKey** = `overlay_bruise_light_01`

That gives you enough variety to verify:
- one facial trait
- one body trait
- one state/overlay lookup

### How matching works
The matrix lookup matches against:
- `TraitOrState`
- `MorphRange`
- `LifeStage`
- `Enabled`

If two rows both match, the system chooses the **narrower morph range**.
That means a more specific row can override a broader fallback row.

Example:
- Row A: `0.00` to `1.00`
- Row B: `0.70` to `0.90`

For a value of `0.78`, Row B should win because it is more specific.

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

### Recommended object structure
For the first working version, keep it simple and put these on the **same GameObject**:
- `CharacterCore`
- `GeneticsSystem`
- `UnityPresentationStateHookup`

Example hierarchy:

```text
HouseholdCharacter_Root
├─ CharacterCore
├─ GeneticsSystem
├─ UnityPresentationStateHookup
├─ AppearanceManager
└─ CharacterPortraitRenderer
```

### Inspector setup
Set these fields:
- **Genetics System** -> drag the same object’s `GeneticsSystem`
- **Master Asset Matrix** -> assign `MasterAssetMatrix.asset`
- **Use Npc Profile** -> leave **off**
- **Resolve On Enable** -> usually **on**

### Exact first-time values I recommend
- **Resolve On Enable** = `true`
- **Auto Refresh** = `false` for the first test
- **Refresh Interval** = leave default until you confirm manual refresh works

### What happens
In this mode, the component uses:
- `GeneticsSystem.ApplyDynamicPresentationState()`

That means the active presentation state is resolved from the character’s current phenotype, needs, emotion, and illness read.

### Good first smoke-test setup
Before trying a complex prefab, use one object in a blank scene with:
- `CharacterCore`
- `GeneticsSystem`
- `UnityPresentationStateHookup`
- your assigned `MasterAssetMatrix`

Avoid testing through a deep prefab chain first.
Start with the most direct setup possible, confirm it works, then move the component onto production prefabs.

### How to confirm it is actually working
After entering Play Mode or using the context menu:
- inspect the `GeneticsSystem` phenotype data
- confirm keys like these are changing/populated:
  - `ExpressionPresetKey`
  - `PosturePresetKey`
  - `IdleBehaviorKey`
  - `HealthOverlayKey`
  - `StateOverlayKey`

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

### Important: how `Npc Id` must match
The `Npc Id` must exactly match the `NpcProfile.NpcId` value in `NpcScheduleSystem`.

If the profile says:
- `npc_farmer_01`

Then the hookup field must also be:
- `npc_farmer_01`

If it says:
- `Farmer01`

Then do **not** use:
- `farmer01`
- `npc_farmer_01`
- `Farmer_01`

It must be exact.

### What happens
In NPC mode the hookup:
1. pulls the NPC profile from `NpcScheduleSystem`
2. reads stress, health, state, and reputation
3. calls `AvatarPresentationStateResolver.ResolveNpcState(...)`
4. applies the resulting posture/expression/overlay keys back to the phenotype/avatar layers

This is the important part if your NPCs are not active player characters but still need genetics/presentation logic.

### First NPC test I recommend
Create one scene-only NPC object with:
- a valid `GeneticsSystem`
- a `UnityPresentationStateHookup`
- **Use Npc Profile** enabled
- the scene `NpcScheduleSystem`
- one known-good `Npc Id`

Then trigger **Resolve Presentation State Now** and confirm the phenotype keys update before you try multiple background NPCs at once.

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

### Exact click path
1. Select the GameObject with `UnityPresentationStateHookup`
2. In the component header, click the three-dot or context menu area
3. Click:
   - **Resolve Presentation State Now**

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

### Practical interval guidance
Try these starting values:
- `0.5` to `1.0` seconds for fast debug iteration
- `2.0` to `5.0` seconds for background NPC monitoring
- off for prefabs that only need one-shot setup

If you see unnecessary churn, increase the interval before changing any code.

---

## Part 7 - Query the asset matrix in code

You can use:

`UnityPresentationStateHookup.ResolveAssetEntry(traitOrState, normalizedValue)`

### Example uses
- `ResolveAssetEntry("NoseBridgeHeight", 0.78f)`
- `ResolveAssetEntry("LipFullness", 0.63f)`
- `ResolveAssetEntry("BruiseSeverity", 0.45f)`

This returns the best matching matrix row for the current character life stage.

### Example script usage
If another component wants to ask the matrix what art row to use:

```csharp
AssetMatrixEntry noseEntry = hookup.ResolveAssetEntry("NoseBridgeHeight", 0.78f);
if (noseEntry != null)
{
    Debug.Log(noseEntry.FileName);
}
```

### Another example for state overlays
```csharp
AssetMatrixEntry bruiseEntry = hookup.ResolveAssetEntry("BruiseSeverity", 0.45f);
if (bruiseEntry != null)
{
    Debug.Log($"Overlay variant: {bruiseEntry.VariantKey}");
}
```

---

## Part 8 - A complete first-time scene recipe

If you want the shortest path to a working test scene, do this in order:

1. Create a new debug scene
2. Add one root object named `PresentationDebugCharacter`
3. Add:
   - `CharacterCore`
   - `GeneticsSystem`
   - `UnityPresentationStateHookup`
4. Create `MasterAssetMatrix.asset`
5. Add at least four rows:
   - `NoseBridgeHeight`
   - `LipFullness`
   - `BodyMass`
   - `BruiseSeverity`
6. Assign the matrix to the hookup
7. Leave **Use Npc Profile** off
8. Enter Play Mode
9. Use **Resolve Presentation State Now**
10. Inspect the phenotype/presentation keys

If that works, your matrix + hookup foundation is good.
Only after that should you move into NPC mode, creator scenes, or portrait integration.

---

## Part 9 - What to verify in the Inspector

### On `UnityPresentationStateHookup`
Confirm:
- `Genetics System` is not empty
- `Master Asset Matrix` is not empty
- `Resolve On Enable` matches your intended workflow
- `Auto Refresh` is off unless you are actively testing repeated resolution

### On the matrix asset
Confirm each row has:
- the correct `TraitOrState`
- a valid `MorphRange`
- a `LifeStage` that matches the tested character
- `Enabled` checked

### On NPC setups
Confirm:
- `Use Npc Profile` is enabled
- `Npc Schedule System` is assigned
- `Npc Id` exactly matches the schedule system profile id

---

## Part 10 - Troubleshooting

### The context menu runs but nothing changes
Check:
- `GeneticsSystem` exists on the same GameObject or is assigned manually
- `geneticsSystem.Phenotype` is not null
- you are looking at the phenotype/presentation keys after the refresh

### Asset rows are not resolving
Check:
- the `TraitOrState` string matches exactly
- the tested value is inside `MorphRange`
- `LifeStage` matches the active character
- the row is enabled

### NPC mode always looks the same
Check:
- the `NpcScheduleSystem` actually returns a profile for that id
- the profile has different stress/health/activity values
- you are not accidentally testing a household character with **Use Npc Profile** still off

### Auto refresh feels broken
Remember:
- it only runs while the GameObject is active
- it only repeats if **Auto Refresh** is enabled
- it waits based on `Refresh Interval`

If you need immediate confirmation, prefer the context menu first, then enable auto refresh after the manual path works.

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

### Minimum viable scene setup
If you want the absolute simplest possible test scene:

```text
Scene
├─ Systems
│  └─ GameEventHub
└─ Character_Test
   ├─ CharacterCore
   ├─ GeneticsSystem
   ├─ UnityPresentationStateHookup
   ├─ AppearanceManager
   └─ CharacterPortraitRenderer
```

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
- the character has already had genetics/phenotype applied
- you are checking the correct object in the Inspector

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

### The hookup exists but I still see no art changes
That usually means one of these:
- the matrix row resolves, but no renderer is using that key yet
- the phenotype key changes, but your portrait/body art asset is not assigned in Unity
- the character has the resolver, but not the corresponding portrait/paper-doll art hookup

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

### Best folder locations
- matrix asset: `Assets/Data/Art/`
- character prefabs: `Assets/Prefabs/Characters/`
- debug prefab variants: `Assets/Prefabs/Debug/`

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

## Part 12 - What to read next

After this tutorial, the next useful docs are:
- `UNITY_HOOKUP_PRESENTATION_STATE.md` for the short version
- `UNITY_HOOKUP_RPG_REWARDS.md` if you also want level-up / goal-complete / achievement popups

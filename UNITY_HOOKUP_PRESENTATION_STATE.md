# Unity Hookup - Presentation State + Master Asset Matrix

This guide shows exactly where to add the presentation-state pieces in Unity and what each Inspector field should contain.
Use it when you want the short practical hookup checklist, while `UNITY_TUTORIAL_PRESENTATION_STATE.md` is the longer walkthrough.

## 1. What this hookup gives you

Once connected, `UnityPresentationStateHookup` can do two jobs:

- resolve live presentation state for a household/player character by calling `GeneticsSystem.ApplyDynamicPresentationState()`
- resolve presentation state for a background NPC by reading `NpcScheduleSystem` and applying the result through `AvatarPresentationStateResolver`

It also exposes `ResolveAssetEntry(...)`, which lets other systems ask the `MasterAssetMatrix` for the best art row for a trait or state value.

## 2. Create the Master Asset Matrix asset

In the Unity **Project** window:

1. Right-click in a content folder such as `Assets/Data/Art/`
2. Choose **Create -> Survivebest -> Art -> Master Asset Matrix**
3. Name it `MasterAssetMatrix`
4. Select it and confirm the Inspector shows an `Entries` list

Recommended location:
- `Assets/Data/Art/MasterAssetMatrix.asset`

### Suggested first entries

Do not start by filling the full matrix. Add a few obvious rows first:

- one face trait row such as `NoseBridgeHeight`
- one lip trait row such as `LipFullness`
- one body row
- one overlay row such as `BruiseSeverity`

Example starter row:

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

## 3. Add the hookup component to character prefabs

Add `UnityPresentationStateHookup` to any prefab or scene object that already carries a `GeneticsSystem`.

Recommended places:
- player/household character prefabs
- NPC prefabs with genetics data
- portrait preview prefabs in creator/debug scenes

### Preferred first-pass structure

For the easiest setup, keep these on the same GameObject:

- `CharacterCore`
- `GeneticsSystem`
- `UnityPresentationStateHookup`

Example:

```text
CharacterRoot
├─ CharacterCore
├─ GeneticsSystem
├─ UnityPresentationStateHookup
├─ AppearanceManager
└─ CharacterPortraitRenderer
```

### Inspector fields

Assign these fields on `UnityPresentationStateHookup`:

- **Genetics System**: reference the local `GeneticsSystem`
- **Master Asset Matrix**: assign `MasterAssetMatrix.asset`
- **Use Npc Profile**: enable only for background NPC mode
- **Npc Schedule System**: assign the scene's `NpcScheduleSystem` when NPC mode is enabled
- **Npc Id**: fill in the exact matching NPC id from `NpcScheduleSystem`
- **Resolve On Enable**: usually enabled for the first setup pass
- **Auto Refresh**: usually disabled until manual refresh is confirmed working
- **Refresh Interval**: leave default until you need continuous updates

## 4. Player / household characters

For active or household characters:

- keep **Use Npc Profile** disabled
- the hookup will call `GeneticsSystem.ApplyDynamicPresentationState()`
- use the component's **Resolve Presentation State Now** context menu to force-refresh while testing

### Good first-time values

- **Use Npc Profile** = off
- **Resolve On Enable** = on
- **Auto Refresh** = off

### Expected result

After Play Mode begins or you run the context menu, inspect the phenotype/presentation fields and verify keys like these are being populated:

- `ExpressionPresetKey`
- `PosturePresetKey`
- `IdleBehaviorKey`
- `HealthOverlayKey`
- `StateOverlayKey`

## 5. Background NPCs

For background NPCs that still need genetics/phenotype state:

- enable **Use Npc Profile**
- assign `NpcScheduleSystem`
- assign the NPC's id exactly

### Important NPC id rule

The `Npc Id` field must exactly match the profile id stored by `NpcScheduleSystem`.

If the profile id is:
- `npc_farmer_01`

Then the hookup must also use:
- `npc_farmer_01`

Do not change case, punctuation, or prefixes.

### Expected flow

When NPC mode is enabled, the hookup:

1. fetches the profile from `NpcScheduleSystem`
2. reads profile stress/health/activity/reputation
3. calls `AvatarPresentationStateResolver.ResolveNpcState(...)`
4. applies the resolved posture/expression/overlay data back onto the phenotype/avatar layer profile

## 6. Accessing the asset matrix at runtime

Use `UnityPresentationStateHookup.ResolveAssetEntry(traitOrState, normalizedValue)` when you need the best matrix row for a current value.

Example uses:
- find the best nose variant for `NoseBridgeHeight`
- find the best mouth variant for `LipFullness`
- find overlay art for a severity band

Example:

```csharp
AssetMatrixEntry overlayEntry = hookup.ResolveAssetEntry("BruiseSeverity", 0.45f);
if (overlayEntry != null)
{
    Debug.Log($"Resolved file: {overlayEntry.FileName}");
}
```

## 7. Minimal scene checklist

For a scene to use the new system cleanly:

- a `GeneticsSystem` on the character object
- a `UnityPresentationStateHookup` on the same object
- a `MasterAssetMatrix.asset` assigned in the Inspector
- for NPC mode: a valid `NpcScheduleSystem` and matching `NpcId`
- phenotype data that already exists on the `GeneticsSystem`

## 8. Quick validation path

Inside Unity:

1. Select a character GameObject
2. Confirm `GeneticsSystem` exists
3. Add `UnityPresentationStateHookup`
4. Assign `MasterAssetMatrix.asset`
5. If NPC, enable NPC mode and assign the schedule system plus exact npc id
6. Open the component context menu and click **Resolve Presentation State Now**
7. Inspect phenotype/avatar layer keys in the debugger or Inspector
8. Call `ResolveAssetEntry(...)` from a helper script if you also want to confirm matrix lookups

## 9. Troubleshooting

### `ResolveAssetEntry(...)` always returns null

Check:

- `MasterAssetMatrix` is assigned
- `GeneticsSystem` is assigned or lives on the same GameObject
- the `TraitOrState` string exactly matches your matrix row name
- the character life stage matches the row, or the row uses `All`
- the input value actually falls inside the row's `MorphRange`

### Household character never updates

Check:

- `Use Npc Profile` is disabled
- the object has a valid `GeneticsSystem`
- `Resolve On Enable` is on, or you manually ran **Resolve Presentation State Now**

### NPC looks unchanged

Check:

- `Use Npc Profile` is enabled
- `Npc Schedule System` is assigned
- `Npc Id` matches exactly
- the returned NPC profile actually has meaningful stress/health/activity data to resolve

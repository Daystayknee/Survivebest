# Unity Hookup - Presentation State + Master Asset Matrix

This guide shows where to add the new presentation-state and asset-matrix pieces inside Unity.

## 1. Create the Master Asset Matrix asset

In the Unity Project window:

1. Right-click in a content folder such as `Assets/Data/Art/`
2. Choose **Create -> Survivebest -> Art -> Master Asset Matrix**
3. Name it `MasterAssetMatrix`
4. Populate `Entries` in the Inspector with the trait/state bands you want the resolver to look up

Recommended location:
- `Assets/Data/Art/MasterAssetMatrix.asset`

## 2. Add the hookup component to character prefabs

Add `UnityPresentationStateHookup` to any prefab or scene object that already carries a `GeneticsSystem`.

Recommended places:
- player/household character prefabs
- NPC prefabs that have a `GeneticsSystem`
- portrait preview prefabs in creator/debug scenes

Inspector fields:
- **Genetics System**: reference the local `GeneticsSystem`
- **Master Asset Matrix**: assign `MasterAssetMatrix.asset`
- **Use Npc Profile**: enable this for background NPCs driven by `NpcScheduleSystem`
- **Npc Schedule System**: assign the scene's `NpcScheduleSystem`
- **Npc Id**: fill in the matching NPC id from `NpcScheduleSystem`

## 3. Player/household characters

For active or household characters:
- keep **Use Npc Profile** disabled
- the hookup will call `GeneticsSystem.ApplyDynamicPresentationState()`
- use the component's **Resolve Presentation State Now** context menu to force-refresh in edit/debug workflows

## 4. Background NPCs

For background NPCs that still have genetics/phenotype state:
- enable **Use Npc Profile**
- assign `NpcScheduleSystem`
- assign the NPC's id

The hookup will:
- read stress/health/activity/reputation from `NpcProfile`
- call `AvatarPresentationStateResolver.ResolveNpcState(...)`
- apply the resolved posture/expression/overlay keys back onto the phenotype/avatar layer profile

## 5. Accessing the asset matrix at runtime

Use `UnityPresentationStateHookup.ResolveAssetEntry(traitOrState, normalizedValue)` when you need the best matrix row for a current value.

Example uses:
- find the best nose variant for `NoseBridgeHeight`
- find the best mouth variant for `LipFullness`
- find overlay art for a severity band

## 6. Minimal scene checklist

For a scene to use the new system cleanly:
- a `GeneticsSystem` on the character object
- a `UnityPresentationStateHookup` on the same object
- a `MasterAssetMatrix.asset` assigned in the Inspector
- for NPC mode: a valid `NpcScheduleSystem` and matching `NpcId`

## 7. Quick validation path

Inside Unity:

1. Select a character GameObject
2. Confirm `GeneticsSystem` exists
3. Add `UnityPresentationStateHookup`
4. Assign `MasterAssetMatrix.asset`
5. If NPC, enable NPC mode and assign the schedule system + npc id
6. Open the component context menu and click **Resolve Presentation State Now**
7. Inspect the phenotype/avatar layer keys in the debugger/Inspector

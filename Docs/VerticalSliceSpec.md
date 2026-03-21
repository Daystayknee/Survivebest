# Gameplay Vertical Slice Spec

This spec defines the exact gameplay loops Unity must prove before broad scene wiring expands.

## Slice 1 — Human Day Loop

### Goal
Prove one complete playable day from wake-up through save/reload continuity.

### Required flow
1. Wake up in apartment.
2. Inspect needs, money, and world summary.
3. Use bathroom or shower.
4. Eat at home or order food.
5. Text/contact someone.
6. Go to work or intentionally skip.
7. Return home.
8. Resolve one household pressure.
9. Trigger one social event.
10. End day.
11. Save.
12. Reload and verify parity.

### Architecture proof points
- bootstrap flow initializes a deterministic session
- command layer drives every player action
- facades produce HUD-facing state
- presentation coordinator resolves one coherent screen state
- save/load parity remains intact across the full loop

## Slice 2 — Vampire Night Loop

### Goal
Prove the secrecy/hunger/consequence loop after the human day slice is stable.

### Required flow
1. Wake at dusk.
2. Surface hunger pressure.
3. Take one work or night errand.
4. Trigger one suspicious interaction.
5. Present one feeding choice.
6. Apply one concealment consequence.
7. Return before sunrise.
8. Save and reload with continuity.

### Architecture proof points
- vampire-specific warnings pulse correctly
- command layer supports feeding, compulsion, and concealment actions
- presentation state reflects hunger, suspicion, and timeline consequences
- save/load preserves vampire runtime state

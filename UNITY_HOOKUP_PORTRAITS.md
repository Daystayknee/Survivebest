# Unity Hookup

## Controller Script(s)
- Scene-specific controller for this screen.

## ViewModel/Data Needed
- Bind data from Assets/Scripts/UI/ViewModels models.

## Events Listened To
- SimulationEvent stream from GameEventHub.
- Screen-specific user action events.

## Unity References To Assign
- Root panel/canvas group
- Primary buttons
- Text labels
- Optional icon/image references

## Expected Art Assets
- Background(s)
- Icon set
- Font/theme assets
- Transition/feedback assets

## Validation Checklist
- Open/close transitions work.
- Null-safe if optional assets are missing.
- View binding does not mutate simulation state directly.

## Paper-Doll Portrait Rig Additions
- Support separate close-up face framing and full-body paper-doll framing.
- Keep face layers modular: brows, eye whites, irises, pupils, lashes, eyelids, nose, mouth, freckles/scars, age lines, makeup.
- Add state overlay roots for tears, sweat, fatigue, sickness, bruises, dirt, and pregnancy if relevant.
- Expression changes should come from simulation state (emotion, stress, illness, attachment, burnout), not only manual UI toggles.
- Ensure family resemblance is readable by preserving inherited shape families before fashion/makeup overlays are applied.

## Suggested Portrait Views
1. Face close-up for dialogue/emotion.
2. Full-body paper-doll for outfit/body/posture.
3. Family comparison panel for parent-child/sibling resemblance previews.

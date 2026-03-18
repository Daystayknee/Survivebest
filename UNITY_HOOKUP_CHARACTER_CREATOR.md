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
- Orthographic preview camera for 2D paper-doll framing
- Background roots for creator themes (neutral, genetics, neighborhood, government/laws, home)
- Face-close and body-close focus buttons/labels if used
- Face/body/genetics detail labels plus selectors for face shape, jaw, nose, lips, skin tone, and eye color
- DNA-mode controls for creation mode switching (Random Population / DNA Edit / Visual Sculpt), population region templates, direct gene edits for melanin/height/body fat/muscle/cognition, epigenetic stress/diet inputs, and mutation triggers if exposing the genome editor

## Expected Art Assets
- Background(s)
- Icon set
- Font/theme assets
- Transition/feedback assets
- 2D-ready character preview layers so the face/body close camera presets still read as paper-doll art instead of a 3D scene

## Validation Checklist
- Open/close transitions work.
- Null-safe if optional assets are missing.
- View binding does not mutate simulation state directly.
- Preview camera defaults to a legit 2D orthographic presentation if assigned.
- Genetics tab and background can be previewed separately from face/body close-up sections.
- Face/body/genetics controls should push through the genome layer so creator edits become inheritable DNA rather than isolated cosmetic sliders.

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

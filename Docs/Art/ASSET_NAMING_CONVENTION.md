# Asset Naming Convention

## Goal
Create stable, readable names that work in spreadsheets, resolver rules, and production folders.

## Format
`[system]_[region]_[family]_[variant]_[lifeStage]_[presentation]_[detail]`

## Segment Rules
- `system`: broad class like `face`, `hair`, `body`, `overlay`, `outfit`
- `region`: nose, cheek_left, chest, thigh, etc.
- `family`: roman, heartfull, curly, athletic, bruise, wrinkle
- `variant`: two-digit ordinal like `01`, `02`, `03`
- `lifeStage`: infant, toddler, child, teen, adult, elder
- `presentation`: neutral, smile, sick, tense, base
- `detail`: optional specialty note such as left, right, highbridge, matte

## Examples
- `face_nose_roman_02_adult_base_highbridge`
- `face_mouth_heartfull_03_teen_smile_base`
- `overlay_cheek_left_bruise_01_adult_base`
- `hair_scalp_longwavy_04_adult_base_centerpart`
- `outfit_upper_formal_02_adult_base_tailored`

## Rules
- use lowercase only
- use underscores only
- never encode spaces
- variant numbers should be zero-padded
- avoid ambiguous abbreviations unless standardized in the asset matrix
- keep left/right and upper/lower as explicit detail tokens

## Reserved presentation tokens
- `base`
- `neutral`
- `soft`
- `smile`
- `frown`
- `sick`
- `tense`
- `tired`
- `alert`

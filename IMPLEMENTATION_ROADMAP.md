# Translation Layer Implementation Roadmap

This roadmap turns Survivebest's existing simulation-heavy genetics/phenotype stack into a production-ready
translation layer that can consistently resolve hidden state into art, UI, and gameplay feedback.

## Goal

Build the missing chain:

`DNA -> hidden genes -> phenotype values -> resolver tables -> art key selection -> overlay stacking -> portrait state blend -> UI feedback -> player-readable result`

## Phase 1 - Canonical Mapping

### 1. Master Asset Matrix
**Purpose:** create the single source of truth for every production art key.

**Deliverables**
- `Docs/Art/MasterAssetMatrix.csv` or equivalent spreadsheet export
- one row per asset key
- ownership/status columns for production tracking

**Required columns**
- System
- Region
- TraitOrState
- VariantKey
- TriggerSource
- MorphRange
- LifeStage
- Layer
- Palette
- FileName
- Notes
- Status

### 2. Morph Band Mapping Tables
**Purpose:** convert normalized morph outputs into concrete sprite choices.

**Examples**
- `LipFullness 0.0-0.2 -> mouth_thin_01`
- `LipFullness 0.2-0.4 -> mouth_thin_02`
- `LipFullness 0.4-0.6 -> mouth_medium_01`
- `LipFullness 0.6-0.8 -> mouth_full_01`
- `LipFullness 0.8-1.0 -> mouth_full_02`

**Target systems**
- face
- nose
- jaw
- chin
- cheeks
- eyes
- body regions

### 3. Asset Naming Convention
**Purpose:** keep future matrix rows and sprite lookups stable.

**Canonical format**
`[region]_[family]_[variant]_[lifeStage]_[presentation]_[detail]`

**Examples**
- `nose_roman_02_adult_neutral_base`
- `mouth_heartfull_03_teen_soft_smile`
- `overlay_bruise_cheek_left_01_adult_base`
- `hair_longwavy_centerpart_04_adult_base`

### 4. Region Mask Framework
**Purpose:** give overlays and conditions explicit placement logic.

**Core regions**
- forehead
- cheeks
- nose
- chin
- neck
- shoulders
- chest
- abdomen
- arms
- hands
- thighs
- knees
- calves
- feet

**Used for**
- acne zones
- bruising zones
- rash zones
- tan-line regions
- stretch-mark regions
- sweat regions
- makeup regions
- dirt regions

## Phase 2 - Visible Character Logic

### 5. Overlay Resolver
**Purpose:** translate overlay intensities plus regions into actual visible sprite selections.

**Inputs**
- condition overlay intensities
- body/face region masks
- life stage
- stress/health state
- current grooming state

**Outputs**
- overlay keys per region
- local alpha/intensity values
- visible/hidden flags

### 6. Overlay Priority and Conflict Rules
**Purpose:** prevent visual chaos when many conditions stack.

**Core rule examples**
- sweat sheen sits above most skin overlays
- tears are top-priority face overlays
- dirt sits above base skin tint but below severe blood/injury states
- severe rash can suppress beauty-mark visibility in the same region
- sunburn can reduce tan-line readability

### 7. Presentation State Resolver
**Purpose:** centralize portrait/body state decisions so the sim feels alive.

**Inputs**
- phenotype behavior tendencies
- current needs
- temporary health conditions
- social context
- environment
- ongoing activity
- confidence/readability cues

**Outputs**
- posture preset
- facial expression blend
- blink/fidget behavior
- visible fatigue/stress markers
- grooming/disarray level
- sickness visibility
- confidence/readability cues

### 8. Condition and Body-Zone Interaction Rules
**Purpose:** make conditions inspectable and gameplay-relevant.

**Examples**
- bruised knee highlights the lower-body condition panel
- facial rash routes to hygiene/medical advice
- illness pallor and fatigue change both overlays and UI urgency

## Phase 3 - Feel Alive

### 9. Genetics to Behavior Coupling Rules
Codify how hidden tendencies affect visible behavior.

### 10. Expression Blending
Support mixed emotional states, eye-only changes, and micro-expressions.

### 11. Reactive UI Feedback Rules
Translate hidden values into readable moment-to-moment UX signals.

### 12. Life-Stage Transition Curves
Move aging/weight/composition changes from abrupt swaps to continuous drift.

## Phase 4 - Premium Depth

### 13. Clothing Fit and Material System
- fit classes
- material types
- wear/tear rules
- deformation compatibility

### 14. Body Asymmetry and Distribution Mapping
- per-region fat distribution
- per-region muscle distribution
- asymmetry rules

### 15. DNA Visualization UI
- dominant/recessive bars
- family comparison view
- inheritance summaries

### 16. Style-Lock / Render Presets
- house-style render presets
- lighting/shader consistency rules
- palette enforcement

## Execution Order
1. Master Asset Matrix
2. Morph Band Mapping Tables
3. Asset Naming Convention
4. Region Mask Framework
5. Overlay Resolver
6. Overlay Priority Rules
7. Presentation State Resolver
8. Condition/Body-Zone Interaction Rules
9. Genetics -> Behavior Coupling
10. Expression Blending
11. Reactive UI Feedback
12. Life-Stage Curves
13. Clothing Fit/Material
14. Asymmetry/Distribution Mapping
15. DNA Visualization UI
16. Style-Lock/Render Presets

## Definition of Done
The translation layer is considered production-ready when:
- every phenotype output maps to a documented art rule or explicit "not visualized" decision
- every visible overlay/state has a priority rule
- every body/face region has mask support or an explicit exclusion
- every portrait state is resolved centrally instead of ad hoc
- every asset key has a matrix row and canonical file name
- UI feedback rules cover the top player-facing hidden states

# Presentation State Resolver Spec

## Purpose

The Presentation State Resolver is the bridge between hidden simulation and visible portrait/body state.
It consumes phenotype tendencies, temporary conditions, and current context to decide what the player should see.

## Inputs

### Stable inputs
- phenotype behavior tendencies
- family resemblance style keys
- life stage
- body silhouette archetype
- skin/hair baseline values

### Dynamic inputs
- stress
- sleep quality / exhaustion
- mood / current emotion
- illness / health state
- confidence
- current activity
- social context
- hygiene / grooming state
- weather / environment pressure

## Outputs
- posture preset key
- resting expression key
- eye expression set
- mouth expression set
- blink cadence
- fidget key
- fatigue visibility
- sickness visibility
- grooming drift state
- confidence readability cue
- overlay requests

## Resolution Order
1. Determine safety/urgency state
2. Determine energy and fatigue read
3. Determine emotional bias
4. Determine social presentation bias
5. Determine grooming/disarray state
6. Select posture preset
7. Select expression blend
8. Request overlays and UI cues

## Suggested Rule Buckets

### Stress
- low: calmer blink cadence, open posture
- medium: guarded expression bias
- high: tense posture, fidget bias, stronger under-eye/fatigue read

### Sleep / fatigue
- mild fatigue: softer eyes, slight under-eye darkness
- heavy fatigue: tired posture, reduced gloss/wetness, slower idle cadence

### Illness
- minor illness: pallor, slight eye dullness
- moderate illness: sick posture, fatigue overlay
- severe illness: strong pallor, low-energy behavior, urgent UI cue

### Confidence
- low confidence: reserved posture, reduced eye contact
- medium confidence: neutral presentation
- high confidence: open posture, higher speaking confidence, stronger smile bias

### Grooming drift
- clean: neutral presentation
- slightly messy: minor flyaways / reduced polish
- disheveled: stronger flyaways, tired/guarded presentation bias

## Output Example

Input snapshot:
- Stress: 0.82
- Sleep debt: 0.71
- Illness: 0.35
- Confidence: 0.28
- Activity: social interaction

Output:
- posture preset: `posture_tense`
- resting expression key: `resting_wary`
- eye expression set: `Alert`
- mouth expression set: `Frown`
- blink cadence: `high`
- fidget key: `habit_avoid_gaze`
- fatigue visibility: `0.68`
- sickness visibility: `0.22`
- overlay requests: `dark_circles_mid`, `health_overlay_tense`
- ui cue: `stress_warning_soft`

## Non-Goals
- direct sprite file assignment
- low-level atlas concerns
- art naming itself

Those belong to the asset matrix and morph/overlay resolvers.

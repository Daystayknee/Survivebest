# 2D Paper-Doll Genetics, Portrait Rig, and Psychology Pipeline

This document reframes Survivebest's character stack around a **2D paper-doll / live-portrait avatar pipeline**.
It is intended to complement the existing runtime scripts (`GeneticsSystem`, `PhenotypeResolver`, `AvatarPresentationStateResolver`, `CharacterPortraitRenderer`) with a clearer art-and-simulation target.

## 1) Design Goal

Survivebest should treat realism as **truthful simulation expressed through layered 2D parts**, not full 3D mesh sculpting.

The core pipeline is:

`DNA -> hidden trait values -> phenotype resolver -> layered 2D parts + tints + proportions + portrait-state behavior`

That means the project should prioritize:
- believable inheritance
- visible family resemblance
- body silhouette variation
- expression logic and emotional portrait behavior
- health/aging overlays
- household/social stress being readable in the portrait

## 2) Three-Layer Character Model

### A. Hidden simulation layer
Stores systemic data that does not need to be drawn directly.

Primary domains:
- genetics and recessive inheritance
- psychology/personality predispositions
- health risks and body tendencies
- memories, attachment, burnout, chronic stress
- household and relationship pressures

### B. Resolver layer
Converts hidden values into visible outputs.

Resolver responsibilities:
- choose face/body part families
- choose per-slot sprite variants
- compute tint palettes
- pick expression presets and animation tendencies
- select overlays (fatigue, illness, aging, freckles, dirt, bruises, etc.)
- preserve family resemblance rules across parents, siblings, and children

### C. Presentation layer
Displays the resolved result in UI and gameplay.

Required views:
1. **Face close-up view** for emotion, psychology, illness, and dialogue readability
2. **Full-body paper-doll view** for silhouette, outfit, posture, injury, pregnancy, and aging
3. **Family comparison view** for parent-child resemblance, sibling clustering, and offspring preview

## 3) Genetics in a 2D Game

In Survivebest, genetics should generate **trait expression**, not polygon geometry.

### Genetics should control

#### Face genetics
- face shape family
- jaw width / jaw family
- chin family
- cheek fullness
- nose family / bridge / projection
- lip family / fullness / mouth width
- eye shape / size / spacing tendency
- eyelid type
- brow weight / arch tendency
- ear family

#### Color genetics
- skin tone
- undertone
- blush tendency
- freckle tendency
- eye color
- hair color
- hair highlight behavior
- age graying pattern

#### Body genetics
- height class
- frame size
- shoulder width
- torso length
- leg ratio
- body-fat tendency
- muscle gain tendency
- chest genetics
- hip genetics
- waist tendency
- hand/foot size class
- posture baseline

#### Health / biology genetics
- metabolism
- fertility
- disease susceptibility
- aging speed
- hormone sensitivity
- sleep need
- appetite profile

#### Psychology-linked predispositions
These should be tendencies, not destiny.
- emotional sensitivity
- novelty seeking
- impulsivity tendency
- sociability tendency
- stress reactivity

## 4) Resolver Strategy

Use a hybrid model with three resolution stages.

### Stage 1: Hidden genes
Examples:
- melanin
- undertone
- facial width
- eye size
- nose projection
- lip fullness
- brow heaviness
- body-fat tendency
- muscle tendency
- height potential
- stress reactivity

### Stage 2: Expression resolver
Maps normalized values into concrete art choices.

Example:
- `lip_fullness = 0.82`
- `mouth_width = 0.63`
- `cupid_bow = 0.71`

Resolver output:
- `LipSet_FullRound_03`
- width modifier: slight-wide
- overlay: soft cupid-bow accent

### Stage 3: Render output
The renderer/portrait rig displays:
- chosen sprite family per layer
- chosen color palette/tint
- chosen body-region scaling values
- chosen overlays
- chosen expression/pose preset
- chosen idle/fidget behavior pack

## 5) Part Library Requirements

A believable 2D genetics system needs a **library of reusable modular art families**.

### Face/head families
- oval
- round
- long oval
- square
- heart
- diamond
- triangle
- soft pear
- broad oval
- narrow angular

### Nose families
- button
- straight
- roman
- wide bridge
- flat bridge
- hooked
- rounded
- short upturned
- long narrow
- broad soft

### Lip families
- thin upper / fuller lower
- balanced full
- cupid-bow full
- soft round full
- flatter wide
- heart-shaped
- downturned corners
- pout-heavy

### Eye families
- almond
- round
- hooded
- monolid
- upturned
- downturned
- deep set
- protruding
- narrow
- wide-set

The key rule: **do not build one universal face and fake all variation only with tint or scale.**
Use modular head families plus per-feature libraries so inheritance feels recognizable.

## 6) Paper-Doll Layer Stack

Characters should be authored as layered 2D components.

### Base body layers
- body base
- body shadow
- body blush
- body detail overlays

### Head layers
- head base
- ears
- neck
- facial shadow

### Face layers
- brows
- eye whites
- irises
- pupils
- lashes
- eyelids
- nose
- mouth
- teeth/tongue for open mouth states
- freckles / moles / acne / scars
- age lines
- makeup

### Hair layers
- back hair
- mid hair
- side strands
- bangs / front pieces
- flyaways
- accessories

### Clothing layers
- underwear base
- tops
- jackets
- bottoms
- socks
- shoes
- jewelry
- glasses
- hats
- bags

### State overlays
- sweat
- tears
- dark circles
- sickness tint
- bruises
- cuts
- rashes
- pregnancy belly stages
- sunburn
- dirt

## 7) VTuber-Style Portrait Rig Rules

The portrait should feel alive even when the game is menu-heavy or close-up focused.

### Eyes
Support at minimum:
- open
- half-lidded
- closed
- squint
- widened
- crying
- irritated
- sleepy

### Brows
Support at minimum:
- neutral
- raised
- angry
- worried
- sad
- playful

### Mouths
Support at minimum:
- closed neutral
- slight smile
- grin
- frown
- talk-open shapes
- laugh
- pout
- grimace
- scream

### Head/body portrait states
- idle breathing
- subtle bob
- tired slump
- confident posture
- tense posture
- sick posture

## 8) Body Realism in 2D

Avoid reducing bodies to only thin / medium / fat.

Compose silhouettes from parameter groups:
- height class
- shoulder width
- ribcage size
- chest size
- waist width
- belly fullness
- hip width
- butt fullness
- thigh fullness
- calf fullness
- arm softness or muscle
- posture
- neck length

This allows two characters with similar BMI or body-fat values to still read differently:
- pear-shaped
- hourglass
- soft athletic
- broad curvy
- compact curvy
- lanky narrow-frame

## 9) Inheritance Rules That Players Can Read

Inheritance should produce results the player can notice without opening debug data.

Support:
- direct inheritance
- blended inheritance
- skipped-trait inheritance
- resurfacing grandparent traits
- sibling clustering
- dominant family-feature weighting

Examples:
- mother's lip shape with father's nose bridge
- father's jaw width with mother's cheek fullness
- blended skin tone with one parent's undertone
- shared smile silhouette across siblings

## 10) Character Creator Modes

The creator should support four complementary modes.

### 1. Quick randomize
- full random
- region/culture weighted random
- family-based random
- age-based random
- attractiveness-neutral realistic random

### 2. Advanced visual creator
- head
- eyes
- nose
- lips
- brows
- skin
- body
- hair
- style

### 3. Genetics creator
- dominant family features
- hidden recessive traits
- fertility and family tendencies
- offspring preview

### 4. Family builder
- create partner / ex / parent / sibling / child / roommate
- auto-generate shared features from family data
- preview resemblance before confirmation

## 11) Personality and Psychology for Portrait-Driven Play

Because Survivebest can communicate a lot through portraits, psychology should be visible.

### Core personality traits
- introversion / extroversion
- warmth
- orderliness
- ambition
- patience
- sensitivity
- emotional stability
- confidence
- curiosity
- stubbornness

### Social traits
- flirtiness
- jealousy
- honesty
- empathy
- dominance
- clinginess
- forgiveness
- conflict avoidance

### Inner traits
- insecurity
- self-esteem
- resilience
- trust baseline
- shame sensitivity
- impulse control

## 12) Emotional State Model

Use layered emotional states instead of only flat moodlets.

### Short-term visible states
- happy
- sad
- angry
- anxious
- embarrassed
- lonely
- guilty
- jealous
- hopeful
- disgusted
- exhausted
- calm

### Longer-term hidden states
- burnout
- grief
- depression phase
- chronic stress
- confidence growth
- resentment
- attachment

These should alter:
- expression preset choice
- eye contact level
- blink/fidget frequency
- dialogue tone
- decision weighting
- romance and parenting behavior

## 13) Memory-Driven Portrait Behavior

Memories should influence visible presentation, not only relationship numbers.

Characters should remember events like:
- insult / humiliation
- help / rescue
- first love / first breakup
- cheating
- family neglect
- major praise
- illness or injury
- public embarrassment
- financial hardship

Examples of visible output:
- more suspicion in romance scenes after betrayal
- flatter smile during conflict-heavy relationships
- tense brows when a distant partner appears
- faster fatigue presentation during chronic stress periods

## 14) Household and Roommate Friction Layer

Roommates and family members should not be interchangeable occupancy slots.

Recommended setup traits:
- cleanliness
- noise tolerance
- spending style
- food sharing habits
- privacy level
- guest frequency
- work schedule alignment
- sleep schedule alignment

Recommended relationship labels:
- spouse
- fiance / fiancee
- ex
- co-parent
- sibling / half sibling / step sibling
- adoptive parent
- grandparent
- cousin
- roommate
- best-friend roommate
- enemy roommate
- landlord
- dependent child

## 15) Recommended Production Priorities

### Increase investment in
- trait database depth
- inheritance resolver quality
- modular part libraries
- portrait rig state libraries
- tint/material systems
- psychology-to-expression mapping
- health and aging overlays
- family comparison tools

### Reduce dependence on
- full 3D mesh sculpting assumptions
- heavy geometry-driven inheritance logic
- realism plans that require world traversal animation to communicate emotion

## 16) Implementation Mapping for Existing Survivebest Scripts

This document aligns well with the current code direction.

- `GeneticsSystem` should remain the hidden simulation authority for inherited values and epigenetic pressure.
- `PhenotypeResolver` should stay responsible for converting hidden values into body/face/health presentation data.
- `AvatarPresentationStateResolver` should expand toward expression presets, portrait fidgets, overlay selection, and posture tendencies.
- `CharacterPortraitRenderer` should continue evolving into the presentation endpoint for layered paper-doll portraits.
- `HouseholdMakerScreenController` and `CharacterCreatorDashboardController` should expose quick-random, visual, genetics, and family-builder modes.

## 17) Source-of-Truth Summary

Survivebest should aim for:

> a stylized human life engine with truthful systems underneath

The visual target is not photorealistic humans.
The target is a believable 2D life-sim avatar stack where genetics, emotion, health, memory, and relationships all become readable through layered portraits and paper-doll bodies.

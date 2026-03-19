# 2D Paper-Doll Art Asset and Human Trait Checklist

This document turns the current genetics/phenotype code into a production checklist for art and content.
It is the practical companion to `2D_PAPERDOLL_GENETICS_PIPELINE.md`.

---

## 1. What sprite art the game currently needs

The runtime currently resolves a layered portrait / paper-doll character through `AvatarLayerProfile`, `LifeStageMorphResolver`, `PhenotypeResolver`, and `AvatarPresentationStateResolver`.
That means art production needs reusable sprite sets for the following slots.

### 1.1 Core face and body layer slots
These are the always-on modular parts currently selected by layer key or family.

#### Base body
- `body_base_*`
- body silhouette shapes for size / build variation
- neck region integration
- chest scaling compatibility
- waist / hip / thigh / calf scale compatibility
- hand scale compatibility
- foot scale compatibility

#### Head and face structure
- `head_*`
- `eyes_*`
- `nose_*`
- `mouth_*`
- `brow_*`
- `ear_*`
- jaw/chin support variants matching head families

#### Hair
- `hair_texture_*`
- front hair pieces
- side hair pieces
- back hair pieces
- hairline / recession variants
- density-compatible variants
- gray/receding age variants

### 1.2 Family buckets the current resolver expects
The layer family enums and resolver currently collapse many traits into a reusable piece-family system.

#### Shared piece families
Create art coverage for these family buckets where applicable:
- `Default`
- `Soft`
- `Sharp`
- `Wide`
- `Narrow`
- `Youthful`
- `Mature`

#### Hair texture families
Create texture / silhouette coverage for:
- `Straight`
- `Wavy`
- `Curly`
- `Coily`

### 1.3 Expression sets and portrait state sets
The code expects interchangeable expression systems rather than one baked face.

#### Eye expression sets
- `Neutral`
- `Soft`
- `Alert`
- `Sharp`
- `Sleepy`
- `Wide`

#### Mouth expression sets
- `Neutral`
- `Soft`
- `Smile`
- `Frown`
- `Smirk`
- `Full`

#### Expression preset keys
The current dynamic/static systems resolve these expression preset states:
- `exp_neutral_neutral`
- `exp_soft_smile`
- `exp_sleepy_neutral`
- `exp_sleepy_frown`
- `exp_alert_frown`

#### Resting expression keys
- `resting_neutral`
- `resting_soft`
- `resting_wary`
- `resting_composed`
- `resting_hard`

### 1.4 Posture, idle, and behavior-driven presentation assets
These can be implemented as pose swaps, rig presets, or keyed portrait offsets.

#### Posture presets
- `posture_neutral`
- `posture_confident`
- `posture_shy`
- `posture_tense`
- `posture_tired`
- `posture_sick`

#### Idle behavior sets
- `idle_balanced`
- `idle_assured`
- `idle_reserved`
- `idle_guarded`
- `idle_fidgety`
- `idle_sharp`
- `idle_slow`

#### Habit / micro-animation keys
If animated portraits are supported, the current system can request:
- `habit_fidget`
- `habit_avoid_gaze`
- `habit_lip_bite`
- `habit_sigh`

### 1.5 Overlay sprite inventory
The phenotype and dynamic-state resolvers expect layered overlays for condition, health, age, and state readability.

#### Age / skin-age overlays
- `skin_age_infant_soft`
- `skin_age_toddler_soft`
- `skin_age_youth_clear`
- `skin_age_teen_transition`
- `skin_age_adult_base`
- `skin_age_older_adult`
- `skin_age_elder`
- `skin_state_fatigue`
- `skin_state_pallor`

#### Wrinkle overlays
- `wrinkle_none`
- `wrinkle_light`
- `wrinkle_high`
- `wrinkle_stress_mid`
- `wrinkle_stress_ill`

#### Health overlays
- `health_overlay_clear`
- `health_overlay_tense`
- `health_overlay_fatigued`
- `health_overlay_exhausted`
- `health_overlay_sick`

#### State overlays
- `state_overlay_none`
- `state_overlay_stress`
- `state_overlay_soft`
- `state_overlay_sick`
- `state_overlay_sunburn`

#### Condition-overlay content that should exist visually
Even where a direct layer key is not yet emitted for each one, the phenotype profile already models these conditions and the art library should plan for them:
- freckles
- beauty marks
- moles
- vitiligo
- acne
- scars
- wrinkles
- under-eye discoloration
- hyperpigmentation
- illness pallor
- stretch marks
- burns
- cuts
- bruises
- rashes
- dirt
- sweat sheen
- tears
- sunburn
- tan lines

### 1.6 Body silhouette sets
The resolver currently names these silhouette archetypes, so art should support each with a compatible torso / hips / limb read:
- `NarrowStraight`
- `SoftRectangle`
- `Pear`
- `Spoon`
- `Hourglass`
- `TopHeavy`
- `Athletic`
- `SoftAthletic`
- `BroadCurvy`
- `CompactCurvy`
- `Lanky`
- `Stocky`
- `PlusSizePear`
- `PlusSizeHourglass`
- `PlusSizeApple`
- `ElderSoftened`
- `Postpartum`
- `TonedSlender`
- `MuscularBroad`

### 1.7 Life-stage presentation sets
The art stack also needs life-stage-specific bodies, overlays, and outfit support.

#### Life-stage art modes
- `BundlePortrait` for baby / infant
- `ToddlerCrawl`
- `ChildSimpleRig`
- `TeenRig`
- `AdultRig`
- `ElderRig`

#### Life-stage support assets
- bundled infant body
- toddler crawl pose set
- youth outfit set
- teen outfit set
- adult outfit set
- swaddle outfit
- onesie layer

#### Current named life-stage keys
- `outfit_swaddle`
- `outfit_onesie`
- `outfit_youth`
- `outfit_teen`
- `outfit_adult`
- `onesie_default`
- `pose_crawl_set_a`

### 1.8 Background / context support
These are not necessarily character sprites, but the phenotype system already resolves these presentation dimensions and art direction should support them:
- regional background variants
- culture presentation motifs
- socioeconomic dressing cues
- household lifestyle cues
- privacy / family-closeness / tradition visual accents

---

## 2. Minimum art-production matrix

If you want the shortest possible “what do we absolutely need first?” list, start here.

### Phase 1: must-have modular sets
- base body silhouettes
- head families
- eyes families
- nose families
- mouth families
- brow families
- ears
- hair texture families
- age overlays
- wrinkle overlays
- health overlays
- state overlays
- infant / toddler / child / teen / adult / elder support

### Phase 2: realism and readability boosters
- freckles / moles / vitiligo / acne / scar overlays
- under-eye / fatigue overlays
- body condition overlays such as bruises, cuts, dirt, rashes, stretch marks
- posture presets
- idle / habit portrait animation sets
- gray / thinning / receding hair variants

### Phase 3: family resemblance polish
- alternate head/nose/eye/mouth/brow variants per family bucket
- sibling / parent resemblance anchor variants
- body silhouette refinements per archetype
- culturally inflected outfit / accessory packs

---

## 3. Full list of human traits, features, and genetics currently modeled

Below is the current “system inventory” of human-facing content already represented in code.
This is the best current source of truth for design, UI copy, balancing, and future art expansion.

### 3.1 High-level genetic systems
- chromosome pairs
- genes
- alleles
- gene expression rules
- mutation flags
- mutation origins
- region profile
- population gene pool
- lineage record
- epigenetic markers
- mutation profile
- blood genetics
- creator genetics modes
- family resemblance modes

### 3.2 Core scalar trait cache currently resolved from the genome
These are the main numeric traits directly driving phenotype and art.

#### Skin / pigmentation / surface
- melanin range
- undertone warmth
- surface tint variation
- freckle tendency
- mole tendency
- vitiligo chance
- hyperpigmentation tendency
- blush visibility
- sun sensitivity

#### Face structure
- face width
- jaw width
- chin prominence
- cheek fullness
- eye size
- eye spacing
- ear size
- nose bridge height
- nostril width
- lip fullness
- brow heaviness

#### Body structure / build
- height potential
- frame size
- shoulder width
- chest / bust potential
- waist / hip bias
- glute fullness
- thigh fullness
- calf shape
- wrist size
- hand size
- finger length
- ankle size
- foot size
- muscle potential
- fat distribution
- bone density
- posture baseline
- limb proportion

#### Hair / micro facial detail
- hair pigment
- hair curl
- hair density
- hairline shape
- hair strand thickness
- eyelash density
- teeth spacing
- gum exposure
- eye wetness
- graying tendency
- balding tendency

#### Health / biology
- acne tendency
- stretch-mark chance
- allergy susceptibility
- skin sensitivity
- metabolism rate
- sleep-quality tendency
- stress sensitivity
- addiction vulnerability
- recovery tendency
- illness vulnerability
- aging speed

### 3.3 Detailed genome subprofiles
These are the more specific human features already exposed for future art/UI use.

#### FaceStructureGenomeProfile
- head width
- head height
- face length
- forehead height
- forehead slope
- temple width
- cheek fullness
- cheekbone projection
- midface length
- jaw width
- jaw sharpness
- chin width
- chin length
- chin projection
- ear size
- ear protrusion

#### EyeGenomeProfile
- eye size
- eye width
- eye roundness
- eye depth
- eye spacing
- eye tilt
- upper-lid fullness
- lower-lid fullness
- lash density
- lash-length tendency
- brow ridge strength
- iris size
- sclera visibility

#### NoseGenomeProfile
- bridge height
- bridge width
- nose length
- tip shape
- nostril width
- nostril flare
- projection
- curve
- softness

#### MouthGenomeProfile
- upper-lip fullness
- lower-lip fullness
- cupid-bow sharpness
- mouth width
- mouth-corner tilt
- philtrum depth
- lip projection
- lip asymmetry tendency
- tooth spacing tendency
- gum-show tendency

#### SkinGenomeProfile
- melanin range
- undertone
- blush visibility
- freckle tendency
- mole tendency
- acne tendency
- scar tendency
- stretch-mark tendency
- wrinkle tendency
- pore visibility
- sun sensitivity
- tanning tendency

#### HairGenomeProfile
- density
- strand thickness
- curl pattern
- wave pattern
- growth speed
- hairline shape
- widow’s-peak tendency
- baby-hair density
- body-hair level
- facial-hair tendency
- graying age
- graying pattern
- baldness tendency

#### BodyGenomeProfile
- height potential
- frame size
- shoulder width
- ribcage width
- arm length
- torso length
- waist tendency
- hip width
- thigh fullness
- calf shape
- butt fullness
- chest-size tendency
- muscle response
- fat distribution
- metabolism
- posture tendency

#### BiologyGenomeProfile
- fertility level
- menopause timing tendency
- puberty timing
- appetite tendency
- sleep need
- disease predisposition
- stress sensitivity
- pain sensitivity
- addiction vulnerability
- hormone sensitivity
- immune resilience
- aging speed

#### TemperamentGenomeProfile
- baseline sensitivity
- irritability tendency
- novelty seeking
- sociability tendency
- shyness tendency
- impulsivity tendency
- caution tendency
- emotional intensity
- resilience tendency

### 3.4 Psychology, personality, talents, and identity already modeled

#### PsychologicalGeneticsProfile
- openness
- conscientiousness
- extraversion
- agreeableness
- neuroticism
- impulsivity
- risk tolerance
- empathy depth
- narcissism
- trauma sensitivity
- addiction risk

#### TalentGeneticsProfile
- music affinity
- athletic affinity
- social affinity
- analytical affinity
- artistic affinity
- vocal-texture potential

#### IdentityGeneticsProfile
- gender-identity spectrum
- sexual-orientation spectrum
- cultural affinity
- voice-pitch range
- speech cadence

### 3.5 Hormones, micro-details, reproduction, and blood

#### HormoneRegulationProfile
- estrogen / androgen balance
- growth-hormone sensitivity
- cortisol regulation
- aging resilience

#### MicroDetailGenomeProfile
- acne-scar risk
- stretch response
- tooth crowding
- lash length
- iris-ring depth
- hairline asymmetry

#### ReproductiveGenomeProfile
- fertility signal
- twin chance
- meiotic stability
- recombination rate
- rare-trait resurfacing

#### BloodGeneticsProfile
- ABO parent allele A
- ABO parent allele B
- Rh parent allele A
- Rh parent allele B
- resolved blood type outcomes:
  - O-
  - O+
  - A-
  - A+
  - B-
  - B+
  - AB-
  - AB+

### 3.6 Environmental and mutation systems affecting phenotype

#### EpigeneticMarkerProfile
- stress imprint
- diet-quality imprint
- toxin exposure
- social-safety signal
- sun exposure
- trauma expression

#### MutationProfile
- random mutation load
- environmental mutation load
- inherited mutation chain
- beneficial mutation chance
- hidden-trait skip chance

### 3.7 Runtime phenotype outputs the art/UI stack can read

#### FaceMorphProfile outputs
- face width
- jaw width
- chin prominence
- cheek fullness
- eye size
- eye spacing
- nose bridge height
- nostril width
- lip fullness
- ear size
- brow heaviness

#### BodyMorphProfile outputs
- height
- neck
- shoulders
- chest/bust presentation
- waist
- stomach
- hips
- thighs
- knees
- calves
- ankles
- wrists
- hands
- fingers
- feet
- frame size
- muscle expression
- fat expression
- limb proportion

#### SkinProfile outputs
- tone
- undertone
- surface tint variation
- blush strength
- sun sensitivity
- all condition overlays listed above

#### HairProfile outputs
- pigment
- curl
- density
- graying
- hairline recession
- front-piece density
- side-piece density
- back-piece density
- frizz
- dryness
- oiliness
- tangling

#### HealthPredispositionProfile outputs
- allergy susceptibility
- skin sensitivity
- metabolism rate
- sleep-quality tendency
- stress sensitivity
- addiction vulnerability
- recovery tendency
- illness vulnerability
- blood-type key

#### PortraitBehaviorProfile outputs
- eye contact
- blink rate
- fidgeting
- smile frequency
- speaking confidence
- crying threshold
- anger intensity
- embarrassment visibility
- tiredness visibility
- self-care presentation
- idle behavior key
- posture preset key
- resting expression key
- likely expression style
- habit animation keys

#### BackgroundPresentationProfile outputs
- region id
- culture key
- socioeconomic key
- household lifestyle key
- cultural affinity
- privacy norms
- family closeness norms
- style tradition weight

#### FamilyResemblanceProfile outputs
- head family key
- eye family key
- nose family key
- mouth family key
- brow family key
- hair texture key
- posture style key
- resting expression key
- visible trait summary

### 3.8 Trait-catalog clusters already represented for balancing and UI
The trait metadata system groups the current human-feature set into these production clusters:
- head / skull
- midface / cheeks
- jaw / chin
- eyes / brows
- nose system
- mouth system
- ear system
- skin surface
- hair surface
- upper body
- lower body
- proportions / posture
- biology
- temperament
- special flags

### 3.9 Genetic inheritance / expression rules already represented
The underlying system already distinguishes these modeling styles:
- dominant / recessive
- blended
- polygenic
- threshold
- hormone-mediated
- dominant expression mode
- recessive expression mode
- codominant expression mode
- incomplete-dominance expression mode
- polygenic expression mode
- threshold expression mode
- latent expression mode

---

## 4. Recommended way to hand this to artists

For production, split the work into four Jira / task-board buckets:
1. **Portrait modular parts**: head, eyes, nose, mouth, brow, ear, hair.
2. **Body silhouette packs**: torso and limb silhouettes across life stages.
3. **Condition and aging overlays**: health, age, stress, dirt, injury, skin conditions.
4. **Behavior and state packs**: expressions, posture presets, idle/habit portrait sets.

If you want, the next step can be a stricter spreadsheet-style deliverable with one row per asset key
(e.g. `head_soft`, `nose_sharp`, `health_overlay_tense`, `outfit_teen`, etc.) and columns for
priority, life stage, palette needs, and animation requirements.

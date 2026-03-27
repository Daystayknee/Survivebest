# Survival Feature & Object Breakdown (Redundancy Audit)

This reference enumerates survival-related features currently authored in code so we can spot overlap and keep content intentional.

## 1) Systems / Objects that currently own survival content

| Area | Object | Purpose | Redundancy Status |
|---|---|---|---|
| Skills | `Assets/Scripts/Core/SkillSystem.cs` (`SkillSystem`) | Canonical XP-tracked skill key catalog. | **Primary source of truth** for skill keys; dictionary prevents duplicate keys. |
| Activities | `Assets/Scripts/Core/LifeActivityCatalog.cs` (`LifeActivityCatalog`) | Authored practical actions, including survival activity picker. | Survival activities are grouped in a dedicated pool and audited for duplicates in tests. |
| Traits | `Assets/Scripts/Core/ContentExplosionCatalog.cs` (`ContentExplosionCatalog`) | Positive/negative authored trait definitions used by narrative/content generation. | Survival-tagged traits are audited for unique IDs/labels in tests. |
| Tests | `Assets/Tests/EditMode/SurvivalFeatureBreakdownAuditTests.cs` | Regression checks for duplicate/empty entries and missing key survival content. | Enforces uniqueness and minimum coverage. |

## 2) Skill breakdown (XP-trackable)

### Core survival pillars
- Foraging
- Plant ID
- Firecraft / Fire Starting / Fuel Management
- Shelter Building / Shelter Construction / Base Building
- Navigation / Route Finding / Map Reading / Compass Use / Star Navigation
- Tracking / Animal Tracking / Trap Setting / Fishing & Angling
- Water Purification / Water Sourcing / Boiling & Sterilization / Filtration / Chemical Treatment
- First Aid & Medicine / Trauma Care / Infection Control / Herbal Treatment
- Reconnaissance / Weather Reading / Cold Weather Survival / Heat Management / Morale Leadership

### Coverage note
- We intentionally keep both broad and granular keys (example: `Firecraft` plus `Fire Starting`) so gameplay can support either:
  - high-level progression tracks, and
  - detailed sub-skill progression.

## 3) Practical survival activity breakdown

`LifeActivityCatalog` survival practical pool currently includes:
- Gather wood and tinder
- Build and maintain a fire
- Identify safe wild plants and berries
- Construct a debris shelter
- Build a lean-to with weatherproofing
- Track animal signs near water routes
- Set simple snare and fish traps
- Butcher and preserve harvested food
- Craft survival tools and improvised weapons
- Filter and boil drinking water
- Scout nearby terrain and mark safe routes
- Cook a survival meal over open fire

## 4) Trait breakdown (survival-tagged)

### Positive
- steady hands
- keen eyes
- efficient forager
- cold resistant
- medic touch

### Negative
- claustrophobic
- fragile bones
- impatient
- thirst prone

## 5) Redundancy policy

- **Skills**: broad+granular naming is allowed when one is a parent track and the other is a sub-discipline.
- **Activities**: exact duplicates (case-insensitive) are disallowed.
- **Traits**: duplicate IDs and duplicate labels are disallowed within survival-tagged entries.
- **Audit source**: enforced in `SurvivalFeatureBreakdownAuditTests`.

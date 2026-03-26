# Sprite System Audit

This is a code-grounded inventory of systems that currently depend on sprites, plus how each one works at runtime.

## 1) Character phenotype/paper-doll sprite contract (largest sprite surface)

### What needs sprites
- Core keyed layer slots in `AvatarLayerProfile`:
  - `BaseBodyLayerKey`, `HeadLayerKey`, `EyeLayerKey`, `NoseLayerKey`, `MouthLayerKey`, `BrowLayerKey`, `EarLayerKey`, `HairTextureLayerKey`, `BodySilhouetteLayerKey`.
  - Overlay keys: `SkinAgeOverlayKey`, `WrinkleOverlayKey`, `HealthOverlayKey`, `StateOverlayKey`.
  - Outfit/pose keys: `OutfitLayerKey`, `OnesieLayerKey`, `CrawlPoseSetKey`.
  - Presentation keys: `ExpressionPresetKey`, `PosturePresetKey`, `IdleBehaviorKey`, `RestingExpressionKey`, and `HabitAnimationKeys`.
- Family and expression buckets that imply reusable sprite sets:
  - `LayerPieceFamily` (Default/Soft/Sharp/Wide/Narrow/Youthful/Mature + hair curl families), `EyeExpressionSet`, `MouthExpressionSet`, and `BodySilhouetteArchetype`.
- Condition overlay channels in `ConditionOverlayProfile` (freckles, moles, vitiligo, acne, scars, wrinkles, bruises, burns, tears, sunburn, etc.).

### How it works
- `PhenotypeResolver.ResolveAvatarLayers(...)` converts genetics into deterministic art keys (e.g., `body_base_*`, `head_*`, `eyes_*`, `hair_texture_*`, `silhouette_*`) and base behavior/expression presets.
- `LifeStageMorphResolver.ApplyLifeStageMorph(...)` then remaps age-specific art modes and keys:
  - infant/toddler/youth/teen/adult outfit keys,
  - age skin overlays,
  - wrinkle overlays,
  - crawl pose set toggles,
  - bundled infant body toggles.
- `AvatarPresentationStateResolver.ResolveDynamicState(...)` updates expression and overlays from current sim state (stress/anger/energy/illness/confidence), producing keys like:
  - expression keys (`exp_alert_frown`, `exp_soft_smile`, etc.),
  - overlays (`health_overlay_sick`, `state_overlay_stress`, `skin_state_fatigue`, etc.),
  - posture/idle/resting-expression keys.
- `AvatarPresentationStateResolver.ApplyResolvedState(...)` writes those dynamic keys back into `AvatarLayerProfile`, so the portrait/body renderer can swap sprite pieces.

## 2) Runtime appearance assembly (sprite renderers)

### What needs sprites
- `AppearanceManager` has dedicated `SpriteRenderer` slots for:
  - scalp hair pieces (front/side/back + side L/R + bangs + flyaways + hairline),
  - facial hair pieces (mustache, beard jaw/chin/neck, sideburns),
  - body hair pieces (chest/arm/leg/forearm/lower leg/armpit/lower abdomen),
  - face/body parts (head, neck, ears, eyes, nose, mouth, brows, lashes, makeup, skin, beauty mark, vitiligo overlay).
- Legacy simple `HairStyleVariant` still expects `FrontHair`, `SideHair`, `BackHair` sprites.

### How it works
- `BuildHairRenderContract(...)` delegates to the hair assembly resolver and returns a list of sprite pieces + slot targets.
- `ApplyLayeredHairContract(...)` clears all hair/body-hair slots and applies each `HairRenderPiece` to the resolved slot renderer.
- Eye/skin/hair colors are applied as tint colors on top of assigned sprites.

## 3) UI portrait renderer (2D UI image layers)

### What needs sprites
- `CharacterPortraitRenderer` requires mapped sprite libraries for:
  - face shape,
  - eye shape,
  - hair style,
  - body type,
  - clothing style,
  - plus a fallback portrait sprite.
- Optional layered hair UI slots also need sprites for hair/facial-hair piece slots.

### How it works
- On refresh, renderer maps character enums to sprite entries and assigns to UI `Image` layers.
- If `AppearanceManager` exists, it builds a hair render contract and copies each hair piece sprite into corresponding UI hair slots.
- It also estimates combinatorial variation and checks a >10,000-variant roster target.

## 4) Food and ingredient icon systems

### What needs sprites
- `IngredientCatalog.IngredientItem` has `IconSprite` plus `SpriteId`.
- `FoodDatabase.FoodItem` and `FoodRecipeDefinition` each have `IconSprite` plus `SpriteId`.

### How it works
- Catalog/database initialization ensures every entry has a normalized `SpriteId` fallback when not explicitly assigned.
- This supports either direct `IconSprite` usage or ID-driven lookup pipelines.

## 5) Location and zone scene imagery

### What needs sprites
- `LocationManager.Room` requires a `Background` sprite per room.
- `ZoneScenePanel.ZoneThemeContent` has an `Illustration` sprite per zone theme.

### How it works
- Navigating rooms fades out, swaps `backgroundRenderer.sprite` to room background, then fades back in.
- Zone panel listens for room/theme changes and updates `illustrationImage.sprite` from the matched theme content entry.

## 6) Action popup wildlife previews

### What needs sprites
- `AnimalSightingEncounter` includes `AnimalPreview` sprite.

### How it works
- Popup checks whether current action is `animal_sight` and an encounter is active.
- If yes, it enables preview image and assigns encounter sprite; if missing, it shows a translucent placeholder state and a label prompting image assignment.

## 7) Journal/event feed portraits

### What needs sprites
- `JournalFeedUI` has a `defaultPortrait` fallback sprite.
- `JournalCardView` binds a portrait sprite into `portraitImage` per event card.

### How it works
- On each published simulation event, feed tries to resolve source character portrait from an `Image` on that character.
- If none is found, it uses `defaultPortrait`, then binds into card UI.

## 8) Character creator style thumbnails

### What needs sprites
- `CharacterCreatorDashboardController.StyleVariantCardView` includes a thumbnail `Image` per style card.

### How it works
- Style grid cards are data-driven and expect thumbnail images to visually represent selectable variants.

## 9) Existing production checklist reference

- `2D_PAPERDOLL_ART_ASSET_AND_TRAIT_CHECKLIST.md` already documents the paper-doll art backlog in detail:
  - layer families,
  - expression sets,
  - age/health/state overlays,
  - silhouette archetypes,
  - posture/idle/habit keys.
- It aligns directly with the resolver contracts above and should be treated as the canonical content-production checklist.

## 10) Render stack / draw order contract

Define a stable stack order so the same content renders consistently across world-space (`SpriteRenderer`) and UI-space (`Image`) portraits.

### Recommended baseline order (back -> front)
1. Room/background
2. Body base / silhouette
3. Neck
4. Head base
5. Ear base
6. Hair back
7. Hair side (style-dependent: behind or in front of ears)
8. Eyes
9. Nose
10. Mouth
11. Brows
12. Lashes
13. Skin condition overlays (vitiligo/freckles/scars/acne)
14. Health/state overlays
15. Bangs/front hair
16. Facial hair
17. Makeup
18. Clothing base
19. Outerwear/accessories
20. FX overlays (tears/sweat/highlight)

### Life-stage exceptions
- Infant/toddler bundles must enforce `outfit_swaddle`/`outfit_onesie` precedence and disable incompatible layers when `UseBundledInfantBody` or `EnableOnesieLayer` is active.
- Toddler crawl pose sets can change limb visibility and must include explicit draw-order overrides.

## 11) Asset registry / canonical key database

The project needs a single source of truth (CSV/ScriptableObject/JSON) for every sprite key used by resolvers and UI systems.

### Required registry columns
- `AssetKey`
- `Category` (head/eyes/hair/overlay/outfit/icon/background/etc.)
- `RendererSlot` (e.g., `HairFront`, `Mouth`, `HealthOverlay`)
- `LifeStageSupport` (baby/toddler/child/teen/adult/elder)
- `SexFrameSupport` (fem/masc/androgynous or all)
- `BodySupport` (silhouette/build families)
- `OverlayCompatible` (yes/no)
- `State` (implemented/placeholder/missing)
- `FallbackKey`
- `TintPolicy` (grayscale_tintable / baked_color)
- `AnimationCapability` (static / pose_swap / frame_anim)

### Example schema rows
| Asset Key | Category | Slot | Life Stage | Sex Frame Support | Body Type Support | Overlay Compatible | Exists? | Fallback |
|---|---|---|---|---|---|---|---|---|
| head_heart_soft_01 | head | head | teen,adult | fem,masc,androgynous | slim,avg,curvy | yes | yes | default_head |
| health_overlay_sick_01 | overlay | health_overlay | all | all | all | yes | no | none |

## 12) Asset validation and missing-key reporting

Add editor/build-time checks that validate generated keys from:
- `PhenotypeResolver`
- `LifeStageMorphResolver`
- `AvatarPresentationStateResolver`
- UI and content-driven sprite IDs (`FoodDatabase`, `IngredientCatalog`, etc.)

### Validation checks
- Missing key detection (key not found in registry)
- Duplicate key detection
- Invalid slot assignment (asset tagged for wrong slot)
- Invalid life-stage remap (e.g., toddler key mapped to adult-only sprite)
- Fallback spam reporting (counts and top offenders)
- Unused sprite detection (registry entries never requested)

### Recommended output
- Per-scene report + project summary report
- CI artifact with severity levels (`error`, `warning`, `info`)
- Gate release builds on unresolved critical character keys

## 13) Alignment, pivots, anchors, and region masks

Define explicit placement rules for modular art.

### Alignment contract
- Standardized canvas bounds per slot category
- Pivot conventions per slot (`head`, `eyes`, `nose`, `mouth`, `hair_front`, etc.)
- Anchor points per head family and jaw family
- Overlay region masks for cheeks, under-eye, forehead, jawline, neck
- Outfit anchors aligned to chest/waist/hip scale bands

### Required authoring guides
- Head-family anchor guide
- Jaw/chin mouth-offset guide
- Hairline + bangs alignment guide
- Beard-to-jaw placement guide
- Body silhouette garment fit guide

## 14) Clothing and overlay clipping/conflict rules

Define hide/show and occlusion behavior to avoid clipping artifacts.

### Conflict classes
- Long hair vs shoulders/chest
- Beard vs high collars/masks
- Sleeves vs arm scale
- Pants/skirts vs hip/thigh scale
- Overlays hidden by full-coverage clothing
- Pose-specific visibility (crawl, seated, bundled infant)

### Runtime rules
- Auto-disable conflicting layers by priority
- Apply mask regions before final draw
- Log conflict resolutions for content QA

## 15) Accessory and wearable sprite support

Track current support status for accessories even if not fully implemented.

### Accessory categories to audit
- Earrings, necklaces, glasses, hats
- Face/body piercings
- Hair accessories
- Socks/shoes
- Jackets/outerwear
- Bags
- Handheld props

### Status model per category
- `unsupported`
- `planned`
- `partial`
- `production_ready`

## 16) Animation/micro-state support (blink, talk, emote)

Document sprite capability per category.

| Category | Static | Pose Swap | Frame Anim | Notes |
|---|---|---|---|---|
| head | yes | limited | no | base anchor-driven |
| eyes | yes | yes | optional | blink/emote-ready |
| mouth | yes | yes | optional | talk/emote-ready |
| hair_front | yes | optional | optional | sway variants |
| state_overlay | yes | no | no | stack with expression |

### Micro-state checklist
- Blink
- Talk phoneme swaps
- Smile/frown transitions
- Crying/tears
- Anger flare
- Embarrassment/blush
- Exhaustion droop
- Intoxication/confusion states
- Sickness + expression stack compatibility

## 17) Tint-safe art rules and palette compatibility

The project uses tinting for eyes/skin/hair, so every affected asset needs a tint policy.

### Tint policy contract
- `grayscale_tintable` assets: preserve shading under tint
- `baked_color` assets: no runtime tint except brightness-safe adjustments
- Makeup/overlay assets tested across full skin-tone range
- Hair highlight compatibility for dark/light/unnatural colors
- UI icon contrast tests for dark and light themes

### QA checks
- Tone readability at min/max saturation
- No crushed detail on darker tones
- No overblown highlights on lighter tones

## 18) Fallback tier system

Define fallback quality tiers for missing assets.

1. Tier 1: exact intended asset key
2. Tier 2: same family substitute
3. Tier 3: same slot generic substitute
4. Tier 4: global default placeholder
5. Tier 5: slot hidden/disabled

### Reporting
- Include tier frequency by category
- Alert when Tier 4/5 exceeds thresholds

## 19) Import settings / atlas / memory performance

Sprite-heavy pipelines require consistent import and runtime performance rules.

### Import settings contract
- Pixels-per-unit standards by category
- Filter mode (point/bilinear) by asset type
- Compression policy per platform
- Pivot conventions
- Atlas tags / addressable groups
- Transparency correctness
- Mipmap policy for UI/world sprites
- Mesh type policy (`full rect` vs `tight`)

### Runtime performance audit
- Sprite swaps per portrait refresh
- Draw-call footprint for layered characters
- Atlas reuse across world/UI
- Background texture memory budget
- Lazy-loading/addressable strategy for large catalogs

## 20) Save-load appearance parity

Confirm visual identity survives persistence.

### Parity checks
- Phenotype-generated keys restore identically
- Dynamic overlays restore intentionally (or reset intentionally)
- Expression/posture persistence rules are explicit
- Tint values restore losslessly
- Hair contracts rebuild deterministically
- UI and world portraits match after load

## 21) Editor tooling and preview/debug workflow

Add production tooling to make the sprite contract usable.

### Minimum tooling
- Registry browser + search
- Seed/genetics preview generator
- Life-stage/state toggles in preview inspector
- Missing asset badges in inspector
- Slot compatibility validator
- Batch key rename/convention tools
- Clipping conflict visualizer
- Exportable content readiness reports

## 22) Coverage completeness and production readiness matrix

Track not just theoretical variety but practical shipped coverage.

### Coverage metrics
- Theoretical combinations (resolver math)
- Artistically supported combinations
- Automated-test covered combinations
- Fallback-hit combinations
- Unsupported combinations by reason

### Production readiness flags per category
- `CodeReady`
- `ArtSpecReady`
- `NamingLocked`
- `RendererReady`
- `ValidationReady`
- `ContentStarted`
- `AlphaReady`

### Priority implementation order (recommended)
A. Render stack / draw order contract
B. Asset registry + validation
C. Alignment + masking rules
D. Tint safety + fallback tiers
E. Save/load parity + performance audit

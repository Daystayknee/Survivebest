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

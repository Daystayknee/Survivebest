# Survivebest Asset Checklist (Vision-Aligned)

Use this as a production checklist for what still needs art/audio/prefab content to match the target experience.

## 1) Splash Screen Assets
- Fullscreen atmospheric background art (static or animated)
- Game logo (high-res, transparent)
- Subtle particle/fog overlay texture/VFX prefab
- Optional splash SFX + ambient intro loop
- "Press any key" prompt style/font

## 2) Main Menu Assets
- Cinematic animated background scene image/video
- Glass-style button sprites/states (normal/hover/pressed)
- Glow frame/outline sprites
- Logo variants (light/dark)
- Menu music loop and UI click/hover sounds

## 3) Load Game Screen Assets
- Save slot card background (empty/filled variants)
- Slot icon set (world, time, date, household)
- Highlight/selection border sprite
- Empty-state illustration

## 4) Settings Screen Assets
- Tab bar backgrounds/icons (Graphics/Audio/Gameplay/Controls/UI)
- Slider skins (track/fill/handle)
- Toggle skins (on/off)
- Color picker UI prefab visuals
- Optional accessibility icons/text assets

## 5) World Creator Screen Assets
- Left panel tab button visuals
- Dynamic world preview map layers:
  - biome overlays (forest/desert/mixed)
  - climate overlays (clear/storm/snow)
  - population density overlays
  - government/law mood overlays
- Back/Next CTA button styles

## 6) Household Maker Assets
- Character viewport background
- Character turntable platform
- Category tab icons (appearance/clothing/shoes/hats/accessories/makeup/traits/skills)
- Item thumbnail cards
- Rotation/zoom button icons
- Optional idle animation clips

## 7) Character Screen Assets
- Portrait frame assets
- Trait pill/tag background sprites
- Genetics/stats panel skins
- Condition icons (illness, injury, severity tiers)
- Health state badges (healthy/warning/critical)

## 8) Gameplay Screen Assets (Reference Layout)
### Left Location Navigator
- Location row card sprites + icon set:
  - home, outside, world map, friend house, stores, medical, hobbies
- status dot sprites (active/alert)

### Center World Map
- Hand-painted world map base
- Zone labels and label bubble sprites
- NPC marker icons
- Event marker icons (rare resource, conflict, quest)

### Bottom Panels
- Environment panel skin
- Ecology panel skin
- Government panel skin
- Slider styles for environment/ecology controls

### Resources Bar
- Resource icon set (gold/wood/stone/food/water/etc.)
- Horizontal chip backgrounds

### Right Character/Vitals Panel
- Character portrait art variations
- Vitals icon set (health/stamina/hunger/thirst/social)
- Bar backgrounds/fills per stat

## 9) Popup / Action Modal Assets
- Popup frame (glass card)
- Header/body typography styles
- Confirm/cancel button skins
- Category-specific icons (buy/sell/trade/doctor/forage/train)
- Transition animation clips (open/close)

## 10) World/Environment Content Assets
- Room backgrounds per theme:
  - Residential, Nature, StoreInterior, Workplace, Hospital, Civic
- Time-of-day overlays (dawn/day/dusk/night)
- Weather overlays:
  - sunny/cloudy/windy/rain/fog/snow/storm/blizzard/heatwave

## 11) Character Appearance Assets
- Portrait layers: face/eyes/hair/body/clothing sprite atlases
- Hair style sprite variants
- Clothing style sprite variants
- Body silhouette variants

## 12) Food / Commerce Visual Assets
- Ingredient icons (core pantry set)
- Supply icons (medicine, tools, facility symbols)
- Recipe card visuals
- Vendor/fast-food logo icons
- Delivery/order status badges

## 13) Audio Asset Checklist
- Menu ambience
- Gameplay ambience by biome/location
- UI interactions (hover/click/back/confirm/error)
- Event stingers (level-up, condition started, weather alert, save/load)
- Optional character reaction voice barks

## 14) VFX / Feedback Assets
- Button glow pulses
- Subtle panel shimmer
- Weather particles
- Condition warning pulse around vitals
- Notification pop effects for journal feed

## 15) Fonts / Typography
- Header font (stylized)
- Body font (high readability)
- Numeric/UI font for stats
- Fallback multilingual-safe font

## 16) Prefabs You Should Build First
1. `PF_SplashScreen`
2. `PF_MainMenu`
3. `PF_LoadSlotCard`
4. `PF_SettingsTabs`
5. `PF_WorldCreatorPanel`
6. `PF_HouseholdMakerPanel`
7. `PF_CharacterScreen`
8. `PF_GameplayHUD`
9. `PF_ActionPopup`
10. `PF_JournalCard`

## 17) “Done First” Priority Pack (recommended)
- Splash background + logo
- Main menu buttons + click SFX
- Gameplay map base + left nav icons
- Right vitals panel bars/icons
- Popup frame/buttons/icons
- One complete room background per theme


## 18) Build/Interior Interaction Assets
- Build mode toggle icons/states (on/off/locked)
- Furniture gizmo handles (move/rotate/confirm/cancel)
- Room doorway hotspot decals/highlights
- Interactive props (trash can, door, storage) icon overlays
- Furniture store panel card set + price tags + rarity badges
- Drag shadow/placement validity indicators (valid/invalid)

## 19) Home Hotspot + Status Effect Assets
- Hotspot hover decals for: shower, fridge, water cooler, bed, mirror, couch, desk, bookshelf, TV, workout corner, pantry
- Interaction tooltip cards with short buff/debuff previews
- Status effect icon atlas (minimum 220 lightweight glyphs, generated naming allowed)
- Positive/negative status pill backgrounds (glass + warning variants)
- Hourly status tick VFX (soft spark for buffs, subtle glitch/smoke for debuffs)
- Illness trigger FX cues for status-driven condition procs
- UI legend card for status stacks, duration, and magnitude

# Implemented-Only List (No Expansion/Backlog Items)

This document lists currently implemented systems/content lanes only, intentionally excluding expansion/backlog idea entries.

---

## 1) Core flow and UI routing (implemented)

- Splash screen flow to main menu.
- Main menu routing for New Game / Load Game / Settings / Character screen.
- New Game progression:
  - World Creator
  - Character Creator
  - Household Maker
  - Gameplay
- Load Game slots and save metadata display.
- Gameplay HUD + journal/event feed + contextual popup interaction layer.

---

## 2) Core simulation and lifecycle (implemented)

- World clock with day/month/year progression.
- Day-slice stage progression loop.
- Weather system + weather effect integration.
- Structured event hub for simulation-to-UI communication.
- Save/load pipeline with schema migration hooks.

---

## 3) Character/household/social depth (implemented)

- Character lifecycle staging and portrait/trait pipeline.
- Household and family management primitives.
- Needs, health, medical conditions, emotion/conflict systems.
- Social/dialogue systems and relationship memory support.
- Personality/value/preference/profile systems in runtime codebase.

---

## 4) Economy, food, and inventory (implemented)

- Economy authority and account/transaction handling.
- Inventory authority with item stacks/instances and ownership scopes.
- Grocery/order/recipe integration loops.
- Food and drink databases with seeded entries.
- Crafting/profession support systems.

---

## 5) NPC, town, and world reaction systems (implemented)

- NPC scheduling and autonomy controllers.
- NPC career assignment and staffing/service hooks.
- Town simulation manager/system for district and lot activity.
- Story incident generation + AI director pacing support.

---

## 6) Crime, law, substance, and justice (implemented)

- Law/crime categorization and offense hooks.
- Justice consequences including fines/jail lifecycle paths.
- Substance state/effects/dependency lifecycle hooks.
- Related prison/discipline/parole/rehab system scripts present.

---

## 7) Implemented reference-content counts

The following “implemented-first” references exist in current docs/code alignment:

- Jobs/careers implemented roster: **35 professions**.
- Skills: seeded implemented set plus progression support in `SkillSystem`.
- Skill trees: implemented starter nodes (with additional node library documented separately).
- Minigames: implemented profiles listed first in the minigame reference.
- Animals/species: implemented species + seeded pet/animal tokens listed first in reference.
- Human life-stage and appearance enums: implemented sets listed first in reference.
- Housing/property/furniture/waste/laundry baseline enums and systems.
- Crimes/substances implemented categories/types/profiles listed first in their references.

---

## 8) Explicitly excluded from this document

- Any item explicitly marked as:
  - “Expansion …”
  - “proposed …”
  - “future backlog/library …”

Use this file for “what is already in” discussions.

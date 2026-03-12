# Alpha 1 Scope (Code-Complete)

## Goal
Ship a deterministic, saveable, end-of-day playable loop where the player can survive daily needs, handle economy pressure, trigger emergent incidents, and observe NPC/town consequences.

## In Scope
- Splash → Menu → Start/Load flow (data-side complete).
- Core simulation clock + day slices + weather + event stream.
- Character needs/health/status/substance/medical progression.
- Inventory + economy + orders + recipes + groceries.
- NPC schedules + town pressure + service staffing signals.
- Contracts/opportunities + autonomous incidents + AI director pacing.
- Save/load migration with deterministic procedural run/profile/seed data.
- Journal/HUD/popup/feed data pipelines via simulation events.

## Out of Scope (Alpha 1)
- Final art polish and full animation/VFX/SFX coverage.
- Large authored questlines/cutscenes.
- Full PlayMode scene validation for every screen.
- Multiplayer/networking.

## Exit Criteria
- Core loop is stable for 7 in-game days in harness tests.
- Save/load round-trip keeps time/economy/status/jail/orders/contracts.
- Event spam remains bounded.
- No ownership ambiguity for critical state systems.

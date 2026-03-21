# Alpha 1 Scope (Code-Complete)

## Goal
Ship a deterministic, saveable, end-of-day playable loop where the player can survive daily needs, handle economy pressure, trigger emergent incidents, and observe NPC/town consequences.

## Ship gate
Alpha 1 near-term scope is locked to **Milestone 0 — Human Day Slice**.

No gameplay feature is considered complete until it is:
- reachable from scene UI
- manually triggerable by the player
- visibly reflected in HUD/world/action feedback
- persistent through save/load

The following do **not** count as completion:
- debug buttons only
- setup scripts only
- tests only
- inspector-only state injection

## In Scope
- Splash → Menu → Start/Load flow (data-side complete).
- Core simulation clock + day slices + weather + event stream.
- Character needs/health/status/substance/medical progression.
- Inventory + economy + orders + recipes + groceries.
- NPC schedules + town pressure + service staffing signals.
- Contracts/opportunities + autonomous incidents + AI director pacing.
- Save/load migration with deterministic procedural run/profile/seed data.
- Journal/HUD/popup/feed data pipelines via simulation events.
- One fully playable Human Day Slice with visible proof and save/load parity.

## Milestone 0 checklist
1. wake in apartment
2. inspect needs, money, and world summary
3. shower / bathroom
4. eat
5. text/contact someone
6. go to work or skip
7. return home
8. resolve one household pressure
9. trigger one social event
10. end day
11. save
12. reload and verify parity

Do not broaden the milestone until all 12 steps are playable from UI and survive save/load.

## Out of Scope (Alpha 1)
- Final art polish and full animation/VFX/SFX coverage.
- Large authored questlines/cutscenes.
- Full PlayMode scene validation for every screen.
- Multiplayer/networking.
- Additional slice expansion beyond the Human Day Slice before the checklist is complete.

## Exit Criteria
- Human Day Slice is stable and player-completable from gameplay scene UI.
- Every Human Day Slice step has visible on-screen proof.
- Save/load round-trip keeps parity for the Human Day Slice state.
- Core loop is stable for 7 in-game days in harness tests.
- Event spam remains bounded.
- No ownership ambiguity for critical state systems.

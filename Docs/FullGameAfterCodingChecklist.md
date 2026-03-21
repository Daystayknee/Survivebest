# Full Game After Coding Checklist

This document answers the question: **what is still missing after the core gameplay code exists?**

For Survivebest, “after coding” does **not** mean “ready to ship.”
The current repository already has broad gameplay-system code, but a full Unity game still requires the layers below to be finished, validated, and packaged into a player-safe experience.

## 1. A real playable game loop in authored Unity scenes
Code-complete systems are not enough until they are reachable in an actual scene.

You still need:
- a fully wired boot -> menu -> world/character/household -> gameplay flow
- one stable authored gameplay scene that proves the Human Day Slice from UI
- all critical actions reachable without debug-only shortcuts
- visible feedback for every important state change
- save/load parity from real scene UI

If this is missing, you have systems, not a shippable game.

## 2. Final Unity scene and prefab hookup
A full game needs production-safe scene wiring, not just scripts that compile.

You still need:
- manager prefabs or scene roots consistently assigned
- prefab references for HUD, feed, action popups, portraits, paper-doll binders, save/load panels, and location panels
- optional UI references either wired or intentionally null-safe
- scene-to-scene transition validation
- clean startup with no missing-reference spam in the console

## 3. 2D paper-doll presentation that carries gameplay
This project is supposed to be a 2D paper-doll life sim, so the character must visibly live through their body and portrait.

You still need:
- a reliable active-character portrait and paper-doll presentation in gameplay
- outfit, grooming, condition, stress, and hygiene state reflected visually
- work/home/social outcomes that can alter appearance or visible posture
- presentation refresh after actions, time advance, save/load, and scene restore
- missing-art fallback rules so incomplete assets do not break gameplay readability

If the player cannot read the character's life from the screen, the life-sim fantasy is still incomplete.

## 4. Content breadth on top of the systems
A full game needs enough authored or procedural content to stay interesting after the first few loops.

You still need enough content for:
- jobs and work schedules
- food sources, recipes, groceries, and fallback survival options
- household pressures and chores
- contacts, relationships, social reactions, and event variations
- neighborhoods, venues, and world events
- illness, injury, stress, and recovery states
- outfits, body/portrait variations, overlays, and lifestyle reads

The code can support a lot, but a game ships on playable content volume, not only architecture.

## 5. Balance and economy tuning
A full game needs pacing, not just mechanics.

You still need to tune:
- need decay rates
- wages, bills, prices, and debt pressure
- event frequency and incident severity
- work risk/reward versus skipping consequences
- food availability versus survival pressure
- health escalation/recovery cadence
- relationship gain/loss pacing

Without balance, the game may technically function while still feeling unfair, empty, trivial, or exhausting.

## 6. Save/load coverage and migration discipline
A life sim is only real if long-running state survives.

You still need:
- a full save contract for all major runtime systems
- restore verification in actual gameplay scenes, not only in isolated tests
- parity checks for location, time, needs, economy, chores, relationships, schedules, and presentation state
- migration handling for future schema changes
- regression checks for long multi-day households

## 7. Player-facing UX and readability
A full game must explain itself to the player.

You still need:
- a clean HUD hierarchy
- clear action affordances and contextual choices
- understandable feedback when actions succeed, fail, or are blocked
- readable journal/feed summaries
- visible cause-and-effect between player choice and world response
- onboarding/tutorial prompts or first-day guidance
- menus/settings/options that feel complete enough for normal players

If the player cannot understand the game state, they will experience the sim as noise.

## 8. Audio, animation, juice, and feel
A full game needs sensory response.

You still need:
- UI sounds
- action confirmation/cancel/error cues
- ambient room/world audio
- music or mood layers
- portrait/body animation or presentation changes for key emotions and states
- transitions, pulses, highlights, and other readable feedback polish

These are not “extra.” They are part of making the game feel alive.

## 9. Art production and asset completeness
A full Unity game needs art coverage that matches the systems.

You still need enough assets for:
- environments and room backgrounds
- interactable hotspots
- icons and UI chrome
- portrait and paper-doll layers
- clothing, accessories, condition overlays, and expression sets
- event cards, popup art, location art, and loading/menu presentation

You also need:
- naming consistency
- atlas/import settings sanity
- fallback placeholders
- asset coverage tracking against gameplay needs

## 10. Testing in the real engine, not just code review
A full game needs proof inside Unity.

You still need:
- EditMode coverage for logic-heavy systems
- PlayMode coverage for real scene flows
- smoke tests for boot, new game, save, load, day advance, and event handling
- performance checks in content-heavy scenes
- repeated long-run tests for multi-day stability
- bug triage and regression tracking

## 11. Performance and memory discipline
A full game has to run well on target hardware.

You still need:
- frame-time checks in gameplay scenes
- memory checks for sprite-heavy portrait/paper-doll setups
- event spam control and feed throttling
- off-screen simulation budget rules
- scene loading/unloading sanity
- asset size/compression/import reviews

## 12. Production pipeline and shipping infrastructure
A full game needs operational discipline around the Unity project.

You still need:
- consistent branch/build process
- Unity version lock and package stability
- CI or at least repeatable build/test steps
- release checklist for Windows/Mac/Linux targets if planned
- crash/log capture strategy
- save-path and settings-path validation on real machines
- backup/recovery plan for serialized data and content assets

## 13. Legal/business/store readiness
Even a solo game needs non-code shipping work.

You still need:
- game title/logo lock
- store page copy and screenshots
- age-rating/content-warning planning if needed
- licensing verification for fonts, sounds, art, plugins, and middleware
- credits and acknowledgements
- privacy/save-data disclosure if required by platform
- support/contact path for players

## 14. Clear scope discipline
A full game is often blocked by uncontrolled growth more than by missing code.

You still need to decide:
- what counts as Alpha
- what counts as Beta
- what counts as 1.0
- what is intentionally post-launch or future-expansion content
- what systems are required versus nice-to-have

For this project, the most important rule remains:
- finish the Human Day Slice as the ship gate first
- then prove a repeatable multi-day week
- only then widen content breadth toward the endless-life vision

## 15. The shortest honest answer
After coding, you are still missing:
- Unity scene wiring
- UI/UX completion
- art/audio/presentation coverage
- content volume
- tuning/balance
- save/load proof
- PlayMode QA
- performance optimization
- build/release pipeline
- scope control

That is the difference between a repository with many systems and a full game people can actually play.

## Recommended order from here
1. Finish the Human Day Slice in a real gameplay scene.
2. Prove save/load parity in-scene.
3. Lock portrait/paper-doll readability for core life states.
4. Build one repeatable 7-day survival week.
5. Fill missing content in jobs/home/social/health/world lanes.
6. Run balancing and long-form QA.
7. Finish shipping layers: UX, audio, art, builds, store-facing material.

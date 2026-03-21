# Endless Life Sim Framework

This document defines the design target **after** `Milestone 0 — Human Day Slice` is proven.

The Human Day Slice is the ship gate, not the whole game. Its purpose is to verify that the core simulation can support a continuous, replayable, multi-choice human life sim / survival RPG.

## North-star promise
The game should eventually support a continuous life where the player can keep playing day after day, make meaningful choices, absorb consequences, and express very different survival, social, economic, and identity strategies without leaving the core gameplay scene loop.

## Core principle
Every expansion after Milestone 0 must extend the same rule:

- playable from real scene UI
- manually triggerable or naturally encounterable in play
- visibly reflected in HUD/world/feed/action feedback
- persistent across save/load
- able to create downstream consequences in later days

If a system cannot survive that standard, it is still prototype-level, not ship-level.

## Post-gate design pillars

### 1. Endless day continuity
The game must support repeated day-to-day play instead of a single authored proof run.

Required outcomes:
- the player can continue into day 2, day 3, and beyond without hard reset assumptions
- daily transitions preserve unfinished pressures, relationships, debts, habits, and opportunities
- the world keeps producing new pressures and choices rather than exhausting after one loop

### 2. Multi-choice life paths
The player should be able to solve the same pressure in multiple valid ways.

Examples:
- hunger can be handled by cooking, buying cheap food, ordering delivery, visiting someone, skipping meals, stealing, or accepting the penalty
- money pressure can be handled by working, skipping needs to save money, selling items, taking gigs, borrowing, or defaulting
- social needs can be handled by texting, visiting, calling, isolating, appeasing, manipulating, or ignoring consequences

Design requirement:
- avoid a single correct route whenever possible
- each route should trade one pressure for another

### 3. Human-condition simulation
The player-facing loop must eventually expose a broad range of human life conditions rather than only abstract meters.

Target condition lanes:
- physical needs: hunger, thirst, hygiene, energy, sickness, pain, injury
- mental/emotional state: stress, loneliness, shame, confidence, grief, anger, hope, burnout
- financial pressure: cash, debt, bills, groceries, rent, job loss, unstable income
- social pressure: household tension, friendships, romance, family demands, reputation, conflict
- environment pressure: mess, weather, neighborhood safety, home quality, commute friction
- identity and personal expression: appearance, habits, values, secrets, lifestyle, long-term aspirations

### 4. Consequence chains across time
Actions should not only resolve the current click; they should shape future days.

Examples:
- skipping work hurts future income, reputation, schedule stability, and social reactions
- ignoring dishes lowers home comfort and raises household tension later
- overspending solves hunger today but worsens bill pressure tomorrow
- isolating protects time now but worsens loneliness and relationship decay later

Design requirement:
- every major action category should have at least one delayed consequence channel

### 5. RPG progression without breaking the sim
Progression should deepen capability and expression without turning life pressures into irrelevant numbers.

Good progression targets:
- efficiency improvements
- new interaction options
- better social leverage
- improved resilience or recovery speed
- access to better jobs, places, routines, and support networks

Avoid:
- progression that trivializes all needs too early
- upgrades that bypass the human-condition loop entirely

## Content expansion lanes after Milestone 0

### Lane A — Household survival depth
Expand the home from one pressure into a layered domestic loop.

Priority order:
1. dirty dishes
2. trash overflow
3. laundry pile
4. unpaid utility bill
5. low groceries
6. appliance failure or maintenance issue

Each pressure should have:
- a visible warning
- at least one manual fix path
- at least one consequence if ignored
- save/load persistence

### Lane B — Work and money depth
Expand work from one binary choice into a living economy loop.

Priority examples:
- scheduled shift attendance
- late/absent consequences
- wages and paycheck timing
- side gigs / odd jobs
- essential bills and debt pressure
- job performance and promotion risk

Each addition should answer:
- how does the player access it from UI?
- what resource changes immediately?
- what future day consequence does it create?

### Lane C — Social and relationship depth
Turn one text interaction into a persistent social fabric.

Priority examples:
- relationship values plus memory records
- inbound texts/calls/requests
- conflict, apology, support, and favor exchanges
- household member reactions to chores/work/mood
- romance/friendship/family differentiation

Each addition should support:
- visible relationship state
- a player choice
- a persistent aftermath

### Lane D — Body and health depth
Make survival feel embodied instead of abstract.

Priority examples:
- fatigue quality and sleep debt
- sickness and recovery
- pain or injury penalties
- stress-body crossover effects
- substance use / medication / self-care tradeoffs

Each addition should connect to:
- moment-to-moment feedback
- daily planning consequences
- save/load parity

### Lane E — World and opportunity depth
The outside world should keep offering risk, relief, temptation, and change.

Priority examples:
- neighborhood events
- service availability by time/day
- weather friction on travel and mood
- random opportunities or emergencies
- local reputation spillover

## Expansion rule: add width only after depth is real
When adding new systems, use this order:
1. make one action reachable
2. make its result visible
3. make it persistent
4. make it create a next-day consequence
5. only then add variants/content breadth

This prevents fake complexity.

## “Endless” implementation standard
A loop is considered ready for endless play only when:
- it can recur across multiple days
- it can combine with at least two other systems
- it has at least two player choices
- at least one choice carries delayed consequences
- the state survives save/load without obvious resets

## Practical roadmap after the Human Day Slice

### Phase 1 — Repeatable survival week
After the 12-step slice passes, the next target is a stable 7-day run with rotating needs, food, work, home pressure, and social consequences.

### Phase 2 — Branching livelihood and household play
Add multiple work outcomes, multiple food strategies, multiple household pressures, and more than one social contact path.

### Phase 3 — Condition-heavy human life simulation
Layer illness, debt spirals, burnout, relationship volatility, and more expressive personal routines.

### Phase 4 — Large possibility space
Expand neighborhoods, jobs, contacts, events, and long-term progression only after the earlier phases stay readable and persistent.

## Content test heuristic
When deciding whether to add a new feature, ask:
1. what pressure does this create or relieve?
2. what visible proof shows it is real?
3. what next-day consequence can it cause?
4. what other systems does it interact with?
5. does it create a genuinely different life choice?

If the answer is weak, the feature is probably breadth without depth.

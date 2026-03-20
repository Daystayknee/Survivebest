# Skill Trees Reference

This document lists the authored default skill tree nodes. Availability depends on species path plus prerequisite and raw-skill requirements.

## Unlock rules
- Universal nodes are open to any owner when skill requirements are met.
- Human and Vampire branches are species-locked.
- Nodes can require prerequisite node IDs plus minimum raw-skill value in the linked skill name.

## Default nodes (11 nodes)
- **Routine Anchor** (`universal_routine_anchor`) — path `Universal`; category `Universal`; skill `Home organization`; required value `10`; prerequisites: none. Benefit: Turns everyday discipline into steadier mood and recovery windows.
- **Neighbor Web** (`human_neighbor_web`) — path `Human`; category `Community`; skill `Conflict mediation`; required value `10`; prerequisites: none. Benefit: Humans build trust networks, call in favors, and cool rumor spirals.
- **Career Ladder** (`human_career_ladder`) — path `Human`; category `Career`; skill `Project management`; required value `15`; prerequisites: human_neighbor_web. Benefit: Unlocks promotions, bureaucracy handling, and modern adult stability arcs.
- **Cultural Fluency** (`human_cultural_fluency`) — path `Human`; category `Identity`; skill `Language learning`; required value `20`; prerequisites: human_neighbor_web. Benefit: Supports friendships, reinvention, and social navigation across scenes.
- **Maker Instinct** (`human_maker_instinct`) — path `Human`; category `Craft`; skill `Carpentry`; required value `12`; prerequisites: none. Benefit: Humans turn homes, hobbies, and side hustles into resilience.
- **Care Circle** (`human_care_circle`) — path `Human`; category `Relationships`; skill `Teaching`; required value `18`; prerequisites: human_maker_instinct. Benefit: Deepens parenting, mentoring, and multi-generational support loops.
- **Blood Palate** (`vampire_blood_palate`) — path `Vampire`; category `Blood Economy`; skill `Hemocraft`; required value `10`; prerequisites: none. Benefit: Differentiate rare blood, manage storage, and resist low-quality feeding spirals.
- **Masquerade Tailor** (`vampire_masquerade_tailor`) — path `Vampire`; category `Secrecy`; skill `Masquerade`; required value `14`; prerequisites: none. Benefit: Strengthens cover stories, fake identities, and witness cleanup plans.
- **Midnight Court** (`vampire_midnight_court`) — path `Vampire`; category `Hierarchy`; skill `Night politics`; required value `18`; prerequisites: vampire_masquerade_tailor. Benefit: Navigate elders, blood debts, and feeding territory politics.
- **Dawn Runner** (`vampire_dawn_runner`) — path `Vampire`; category `Survival`; skill `Sun avoidance`; required value `16`; prerequisites: vampire_blood_palate. Benefit: Improves shelter planning, panic control, and sunrise escape timing.
- **Sire's Burden** (`vampire_sire_burden`) — path `Vampire`; category `Legacy`; skill `Turning control`; required value `22`; prerequisites: vampire_midnight_court, vampire_dawn_runner. Benefit: Handles fledgling training, first-hunger support, and failed turning fallout.

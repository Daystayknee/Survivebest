# Jobs and Careers Reference

This reference is synchronized with the currently implemented work/career lists in gameplay code.

## Implemented profession roster

Current implemented `ProfessionType` entries excluding `None`: **35**.

### Healthcare / care work
1. **Doctor** — `General Hospital`; shift `8:00-16:00`; pay `$62/hr`; transit `car`; tags `licensed`, `triage`, `authority`.
2. **Nurse** — `General Hospital`; shift `7:00-19:00`; pay `$38/hr`; transit `car`; tags `care_team`, `charting`, `triage`.
3. **Veterinarian** — `Animal Care Clinic`; shift `8:00-18:00`; pay `$44/hr`; transit `car`; tags `animal_care`, `community_trust`.
4. **Social Worker** — `Community Services Center`; shift `8:00-17:00`; pay `$31/hr`; transit `car`; tags `fieldwork`, `paperwork`, `caseload`.

### Education / student life
5. **Teacher** — `Elementary School`; shift `7:00-15:00`; pay `$29/hr`; transit `bus`; tags `lesson_plans`, `parent_emails`.
6. **Student** — `Community College`; shift `8:00-14:00`; pay `$0/hr`; transit `bus`; tags `campus`, `course_load`.

### Public safety
7. **Police Officer** — `Police Precinct`; shift `6:00-18:00`; pay `$36/hr`; transit `car`; tags `chain_of_command`, `public_scrutiny`.
8. **Firefighter** — `Fire Station`; shift `6:00-18:00`; pay `$34/hr`; transit `truck`; tags `crew_bond`.
9. **Security Guard** — `Mall Plaza`; shift `14:00-23:00`; pay `$22/hr`; transit `bus`; tags `patrol`, `incident_reports`, `mall_drama`.

### Retail / service / nightlife
10. **Postal Clerk** — `Post Office`; shift `8:00-17:00`; pay `$20/hr`; transit `car`; tags `sorting`, `counter_service`.
11. **Retail Associate** — `Downtown Grocery`; shift `9:00-21:00`; pay `$18/hr`; transit `bus`; tags `stockroom`, `customer_service`.
12. **Sales Associate** — `Mall Plaza`; shift `10:00-21:00`; pay `$21/hr`; transit `bus`; tags `commissions`, `floor_competition`.
13. **Barber** — `Neighborhood Barbershop`; shift `9:00-18:00`; pay `$24/hr`; transit `car`; tags `repeat_clients`, `chair_talk`.
14. **Bartender** — `Downtown Bar`; shift `16:00-02:00`; pay `$26/hr`; transit `rideshare`; tags `tips`, `regulars`, `gossip`.
15. **Strip Club Dancer** — `Velvet Room`; shift `18:00-03:00`; pay `$34/hr`; transit `rideshare`; tags `house_rules`, `status`, `vip_rooms`.

### Food service
16. **Chef** — `City Diner`; shift `10:00-22:00`; pay `$24/hr`; transit `car`; tags `rush_tickets`, `line_hierarchy`.

### Office / white-collar / media
17. **Office Administrator** — `Tech Office`; shift `8:00-17:00`; pay `$26/hr`; transit `train`; tags `email_threads`, `calendar_politics`.
18. **Accountant** — `Corporate Tower`; shift `8:00-17:00`; pay `$35/hr`; transit `train`; tags `deadlines`, `promotion_track`.
19. **Software Engineer** — `Tech Campus`; shift `9:00-18:00`; pay `$48/hr`; transit `train`; tags `hybrid`, `sprint_review`, `stack_ranking`.
20. **HR Manager** — `Corporate Tower`; shift `8:00-17:00`; pay `$37/hr`; transit `car`; tags `policy`, `rumor_control`.
21. **Journalist** — `City Newsroom`; shift `9:00-19:00`; pay `$29/hr`; transit `car`; tags `deadline`, `scoops`, `rumor_mill`.

### Trades / repairs / construction
22. **Mechanic** — `Auto Garage`; shift `8:00-18:00`; pay `$28/hr`; transit `car`; tags `diagnostics`, `shop_talk`.
23. **Electrician** — `Construction Yard`; shift `7:00-17:00`; pay `$33/hr`; transit `van`; tags `jobsite`, `certified_trade`.
24. **Construction Worker** — `Construction Yard`; shift `7:00-17:00`; pay `$30/hr`; transit `truck`; tags `jobsite`, `crew_hierarchy`.
25. **Plumber** — `Service Trades Depot`; shift `7:00-17:00`; pay `$34/hr`; transit `van`; tags `jobsite`, `emergency_calls`.

### Logistics / transportation
26. **Truck Driver** — `Warehouse Hub`; shift `5:00-15:00`; pay `$31/hr`; transit `truck`; tags `dispatch`, `hours_log`.
27. **Warehouse Associate** — `Distribution Center`; shift `6:00-16:00`; pay `$23/hr`; transit `bus`; tags `fulfillment`, `pick_rate`, `dock_pressure`.
28. **Delivery Driver** — `Last Mile Depot`; shift `8:00-18:00`; pay `$25/hr`; transit `van`; tags `route`, `customer_ratings`, `algorithm_pressure`.
29. **Bus Driver** — `Regional Bus Depot`; shift `5:00-14:00`; pay `$27/hr`; transit `bus`; tags `route`, `public_contact`, `schedule_pressure`.
30. **Dispatcher** — `Transit Dispatch Center`; shift `6:00-18:00`; pay `$32/hr`; transit `train`; tags `control_room`, `radio_chatter`, `blame_chain`.
31. **Commercial Pilot** — `Regional Airport`; shift `5:00-13:00`; pay `$78/hr`; transit `plane`; tags `flight_rotation`, `seniority`, `crew_briefing`.
32. **Flight Attendant** — `Regional Airport`; shift `6:00-16:00`; pay `$33/hr`; transit `plane`; tags `flight_rotation`, `crew_gossip`, `passenger_service`.
33. **Train Conductor** — `Union Rail Yard`; shift `5:00-15:00`; pay `$36/hr`; transit `train`; tags `line_run`, `crew_seniority`, `dispatch`.
34. **Railroad Engineer** — `Union Rail Yard`; shift `4:00-14:00`; pay `$42/hr`; transit `train`; tags `line_run`, `safety_checks`.

### Other implemented entries
35. **Clerk / General service fallback** — represented in system mappings for service availability and lower-complexity workplace coverage.

## Career simulation metadata now tracked

`NpcCareerSystem` now models more than assignment and wages. Current career records also track:

- `CareerLevel`
- `Performance`
- `Attendance`
- `TotalHoursWorked`
- `TotalEarnings`
- `WorkDrama`
- `RumorExposure`
- `WorkplaceStatus`
- `Burnout`
- `JobSatisfaction`

## Workplace drama / status / rumor hooks

Current code supports work summaries and drama prompts through:

- **Status pressure** — promotion lanes, resentment, prestige climb.
- **Rumor pressure** — workplace gossip, coworker chatter, newsroom/HR/nightlife rumor loops.
- **Burnout** — customer exposure, long shifts, commute drag, schedule pressure.
- **Transportation-specific pressure** — dispatch timing, route reliability, crew seniority, flight rotations.

## Work-life profile support for player-facing human texture

`HumanLifeExperienceLayerSystem` now includes `AmericanWorkLifeProfile`, which can store:

- `JobTitle`
- `CareerDomain`
- `WorkplaceName`
- `CommuteMode`
- `ShiftWindow`
- `WorkStatus`
- `CoworkerRumors`
- `Certifications`
- `JobPrestige`
- `WorkplaceDrama`
- `RumorHeat`
- `Burnout`
- `PromotionPressure`

This profile now feeds:

- human texture summaries,
- human texture pulses,
- everyday-life suggestions,
- and save/load persistence via `HumanLifeRuntimeState`.

## World workplace-area name pool currently aligned to career content

Current workplace-area names seeded in world generation:

1. `Office Plaza`
2. `Warehouse Hub`
3. `Auto Garage`
4. `Factory Row`
5. `Business Center`
6. `Corporate Tower`
7. `Tech Campus`
8. `Distribution Center`
9. `Regional Bus Depot`
10. `Transit Dispatch Center`
11. `Regional Airport`
12. `Union Rail Yard`
13. `Last Mile Depot`
14. `Mall Plaza`
15. `Neighborhood Barbershop`
16. `Downtown Bar`
17. `Velvet Room`
18. `City Newsroom`
19. `Service Trades Depot`

## Notes

- This document now reflects the implemented profession/workplace lists in `NpcCareerSystem`, `HumanLifeExperienceLayerSystem`, `WorldCreatorManager`, and `WorldCreatorScreenController`.
- Older “100+ expansion idea” wording has been removed here because the current need is documentation accuracy for the active lists in code.

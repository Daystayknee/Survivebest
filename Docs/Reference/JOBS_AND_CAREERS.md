# Jobs and Careers Reference

This document captures the profession enum, the seeded USA-common role definitions, and the systems-level rules that determine promotion, service coverage, and equipment seeding.

## Career system rules
- NPC roles track profession, workplace lot, career level, performance, attendance, unemployment days, total hours worked, and total earnings.
- Promotions require strong performance, strong attendance, and any required skill node; demotions happen when attendance or performance collapse.
- Critical world services emit outage events when no one is on duty during the scheduled shift window.

## Seeded roles (15 definitions)
- **Doctor** — workplace `General Hospital`; shift `8:00-16:00`; base pay `$62/hr`; critical service `true`; uniform `medical_scrubs`; tool `medical_kit`; access tag `hospital`.
- **Nurse** — workplace `General Hospital`; shift `7:00-19:00`; base pay `$38/hr`; critical service `true`; uniform `nurse_scrubs`; tool `medical_kit`; access tag `hospital`.
- **Veterinarian** — workplace `Animal Care Clinic`; shift `8:00-18:00`; base pay `$44/hr`; critical service `true`; uniform `vet_scrubs`; tool `animal_medical_kit`; access tag `animal_care`.
- **Teacher** — workplace `Elementary School`; shift `7:00-15:00`; base pay `$29/hr`; critical service `true`; uniform `teacher_badge`; tool `lesson_tablet`; access tag `school`.
- **Police** — workplace `Police Precinct`; shift `6:00-18:00`; base pay `$36/hr`; critical service `true`; uniform `police_uniform`; tool `duty_belt`; access tag `public_safety`.
- **Firefighter** — workplace `Fire Station`; shift `6:00-18:00`; base pay `$34/hr`; critical service `true`; uniform `fire_uniform`; tool `rescue_kit`; access tag `public_safety`.
- **Clerk** — workplace `Post Office`; shift `8:00-17:00`; base pay `$20/hr`; critical service `false`; uniform `service_uniform`; tool `scanner`; access tag `postal`.
- **RetailAssociate** — workplace `Downtown Grocery`; shift `9:00-21:00`; base pay `$18/hr`; critical service `false`; uniform `retail_apron`; tool `scanner`; access tag `retail`.
- **Chef** — workplace `City Diner`; shift `10:00-22:00`; base pay `$24/hr`; critical service `true`; uniform `chef_jacket`; tool `chef_knife`; access tag `food_service`.
- **Mechanic** — workplace `Auto Garage`; shift `8:00-18:00`; base pay `$28/hr`; critical service `true`; uniform `mechanic_overalls`; tool `toolbox`; access tag `repairs`.
- **TruckDriver** — workplace `Warehouse Hub`; shift `5:00-15:00`; base pay `$31/hr`; critical service `true`; uniform `hi_vis_vest`; tool `route_manifest`; access tag `logistics`.
- **OfficeAdministrator** — workplace `Tech Office`; shift `8:00-17:00`; base pay `$26/hr`; critical service `false`; uniform `office_badge`; tool `laptop`; access tag `office`.
- **Electrician** — workplace `Construction Yard`; shift `7:00-17:00`; base pay `$33/hr`; critical service `true`; uniform `trade_vest`; tool `electrical_kit`; access tag `trades`.
- **ConstructionWorker** — workplace `Construction Yard`; shift `7:00-17:00`; base pay `$30/hr`; critical service `true`; uniform `hard_hat`; tool `power_tools`; access tag `trades`.
- **Student** — workplace `Community College`; shift `8:00-14:00`; base pay `$0/hr`; critical service `false`; uniform `student_badge`; tool `textbook`; access tag `education`.

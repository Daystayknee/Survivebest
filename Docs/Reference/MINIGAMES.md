# Minigames Reference

This document lists every minigame scene profile and the role-routing assumptions around them.

## Minigame system rules
- Sessions can run as quick-resolution minigames or as multi-step interactive blueprints.
- Recommended skill values influence success chance through performer skill bonuses.
- Career minigames map professions into relevant activity loops such as surgery, repairs, reading, or emergency response.

## Scene profiles (20 types)
- **Cooking** — backdrop `kitchen_station`; recommended skill `Cooking`; duration multiplier `1`; prompt: Keep prep clean, season correctly, and plate before service delay.
- **Baking** — backdrop `kitchen_oven`; recommended skill `Cooking`; duration multiplier `1.15`; prompt: Measure carefully, control oven timing, and finish a balanced bake.
- **DrinkMixing** — backdrop `kitchen_counter`; recommended skill `Cooking`; duration multiplier `0.9`; prompt: Mix hydration drinks, teas, and shakes with timing and cleanliness.
- **Fishing** — backdrop `riverbank`; recommended skill `Fishing`; duration multiplier `1.2`; prompt: Pick the right bait, cast with rhythm, and react to fish tension.
- **Repairs** — backdrop `garage_bench`; recommended skill `Engineering`; duration multiplier `1.1`; prompt: Diagnose the fault, pick safe tools, and verify the fix under load.
- **FirstAid** — backdrop `triage_room`; recommended skill `First aid`; duration multiplier `1.1`; prompt: Stabilize airway, control bleeding, and monitor vitals.
- **Triage** — backdrop `triage_desk`; recommended skill `First aid`; duration multiplier `1.05`; prompt: Sort patients by urgency, check vitals, and route them to the right care lane.
- **Bandaging** — backdrop `treatment_cart`; recommended skill `First aid`; duration multiplier `0.95`; prompt: Clean the wound, layer gauze, wrap with even pressure, and reassess bleeding.
- **Casting** — backdrop `ortho_bay`; recommended skill `First aid`; duration multiplier `1.25`; prompt: Align the limb, pad pressure points, wrap the cast evenly, and confirm circulation.
- **Pharmacy** — backdrop `pharmacy_counter`; recommended skill `First aid`; duration multiplier `0.9`; prompt: Match the prescription, calculate doses, label clearly, and prevent interactions.
- **Cleaning** — backdrop `home_maintenance`; recommended skill `Survival skills`; duration multiplier `0.95`; prompt: Sanitize high-touch areas and manage supplies without wasting water.
- **Surgery** — backdrop `operating_theater`; recommended skill `First aid`; duration multiplier `1.5`; prompt: Prep sterile field, follow operation checklist, and close safely.
- **VeterinaryCare** — backdrop `vet_operatory`; recommended skill `First aid`; duration multiplier `1.2`; prompt: Restrain gently, read species cues, treat safely, and coach the owner on aftercare.
- **Dermatology** — backdrop `clinic_exam_room`; recommended skill `First aid`; duration multiplier `1.05`; prompt: Inspect skin layers, identify flare triggers, and choose the right topical or procedural care.
- **RestaurantService** — backdrop `restaurant_line`; recommended skill `Cooking`; duration multiplier `1.2`; prompt: Coordinate orders, avoid cross-contamination, and maintain ticket speed.
- **EmergencyResponse** — backdrop `emergency_scene`; recommended skill `Survival skills`; duration multiplier `1.35`; prompt: Secure the scene, triage quickly, and coordinate responders.
- **MovieNight** — backdrop `living_room`; recommended skill `Storytelling`; duration multiplier `0.8`; prompt: Pick a film mood, settle in, and recover stress while staying present.
- **TVMarathon** — backdrop `living_room_tv`; recommended skill `Storytelling`; duration multiplier `0.75`; prompt: Choose episodes by vibe and manage time so tomorrow still works.
- **BookReading** — backdrop `library_corner`; recommended skill `Writing`; duration multiplier `0.85`; prompt: Read deeply, take notes, and absorb ideas for growth.
- **SingingSession** — backdrop `music_corner`; recommended skill `Singing`; duration multiplier `0.95`; prompt: Warm up voice, stay on pitch, and perform with confidence.

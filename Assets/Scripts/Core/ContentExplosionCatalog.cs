using System;
using System.Collections.Generic;
using Survivebest.Core.Procedural;
using UnityEngine;

namespace Survivebest.Core
{
    [Serializable]
    public sealed class AuthoredDefinitionEntry
    {
        public string Id;
        public string Label;
        public string Category;
        public List<string> Tags = new();
        public List<string> Conditions = new();
        public float Weight = 1f;
        public string Description;
    }

    [Serializable]
    public sealed class ArchetypeLibraryEntry
    {
        public string Id;
        public string Label;
        public string Library;
        public List<string> Traits = new();
        public List<string> Tags = new();
        public string Summary;
    }

    [Serializable]
    public sealed class LateNightEventContext
    {
        public string DistrictTag;
        public string Weather;
        [Range(0f, 1f)] public float SafetyLevel = 0.5f;
        [Range(0f, 1f)] public float NightlifeIntensity = 0.5f;
        public string RelationshipStatus;
        public bool VampirePresence;
        [Range(0f, 1f)] public float HungerState = 0.5f;
        public bool RecentScandal;
    }

    public static class ContentExplosionCatalog
    {
        private static readonly List<AuthoredDefinitionEntry> ActivityDefinitions = new()
        {
            CreateDefinition("activity_deep_clean_fridge", "deep clean fridge", "activity", 1.1f, "A very adult burst of control that usually starts with one expired condiment.", "domestic", "cleanup", "stress_relief"),
            CreateDefinition("activity_ramen_1am", "make ramen at 1am", "activity", 1.2f, "A late-night survival meal that feels both tragic and correct.", "late_night", "food", "budget"),
            CreateDefinition("activity_doomscroll_bed", "doomscroll in bed", "activity", 1.3f, "The blankets become a command center for bad decisions and blue light.", "digital", "late_night", "anxiety"),
            CreateDefinition("activity_iron_clothes", "iron clothes", "activity", 0.8f, "A small attempt to force tomorrow into behaving.", "domestic", "presentation", "routine"),
            CreateDefinition("activity_check_bank_app", "check bank app", "activity", 1.15f, "A ritual of bravery measured in available balance.", "finance", "stress", "routine"),
            CreateDefinition("activity_call_grandma", "call grandma", "activity", 0.9f, "A tether to older versions of love and obligation.", "family", "care", "sentimental"),
            CreateDefinition("activity_skip_class", "skip class", "activity", 1.0f, "A decision that feels free for ten minutes and expensive for three days.", "school", "avoidance", "risk"),
            CreateDefinition("activity_wait_in_er", "wait in ER", "activity", 0.95f, "Hours flatten into fluorescent patience and paperwork.", "medical", "stress", "public"),
            CreateDefinition("activity_flirt_badly_bar", "flirt badly at bar", "activity", 0.85f, "Confidence leaves the body right when it is needed most.", "social", "nightlife", "romance"),
            CreateDefinition("activity_stalk_ex_profile", "secretly stalk ex profile", "activity", 1.05f, "Curiosity shows up dressed as closure.", "digital", "romance", "shame"),
            CreateDefinition("activity_blackout_curtains", "prep blackout curtains", "activity", 0.9f, "A practical purchase for sleep, secrets, or both.", "home", "late_night", "vampire_adjacent"),
            CreateDefinition("activity_sterilize_feeding_tools", "sterilize feeding tools", "activity", 0.82f, "Care work disguised as tiny equipment management.", "family", "medical", "caregiving")
        };

        private static readonly List<AuthoredDefinitionEntry> TraitDefinitions = new()
        {
            CreateDefinition("trait_conflict_avoidant", "conflict avoidant", "trait", 1f, "Will rearrange their entire week to avoid one hard conversation.", "relationship", "coping"),
            CreateDefinition("trait_image_obsessed", "image obsessed", "trait", 0.9f, "Treats perception like infrastructure.", "digital", "status"),
            CreateDefinition("trait_secretly_sentimental", "secretly sentimental", "trait", 1.1f, "Keeps receipts, screenshots, and emotional evidence.", "memory", "love"),
            CreateDefinition("trait_debt_anxious", "debt anxious", "trait", 1.05f, "Every purchase arrives with a tiny aftershock.", "finance", "stress"),
            CreateDefinition("trait_night_energized", "night energized", "trait", 0.95f, "Gets more alive when the town starts powering down.", "late_night", "schedule"),
            CreateDefinition("trait_territorial_feeder", "territorial feeder", "trait", 0.75f, "Treats feeding routes as personal boundaries with teeth.", "vampire", "feeding"),
            CreateDefinition("trait_grief_numb", "grief numb", "trait", 0.8f, "Pain has not left; it just stopped announcing itself.", "grief", "trauma"),
            CreateDefinition("trait_spiritually_curious", "spiritually curious", "trait", 0.9f, "Collects meanings the way some people collect proof.", "belief", "occult"),
            CreateDefinition("trait_perfection_loop", "perfection loop", "trait", 1.02f, "Polishes details until deadlines become emotional weather.", "work", "anxiety"),
            CreateDefinition("trait_generous_to_a_fault", "generous to a fault", "trait", 0.92f, "Gives resources away faster than they can replenish them.", "care", "finance"),
            CreateDefinition("trait_deadline_sprinter", "deadline sprinter", "trait", 1.01f, "Delivers best work in the panic window before due time.", "work", "routine"),
            CreateDefinition("trait_soft_spoken_observer", "soft-spoken observer", "trait", 0.88f, "Tracks tone, pauses, and room shifts before speaking.", "social", "perception"),
            CreateDefinition("trait_micro_manager", "micro manager", "trait", 0.95f, "Trust issues appear as checklists and calendar pings.", "career", "control"),
            CreateDefinition("trait_burnout_prone", "burnout prone", "trait", 1.08f, "Can sprint brilliantly and then vanish from fatigue.", "wellness", "stress"),
            CreateDefinition("trait_ritual_driven", "ritual driven", "trait", 0.93f, "Daily rituals stabilize mood more than people realize.", "routine", "identity"),
            CreateDefinition("trait_novelty_hunter", "novelty hunter", "trait", 0.98f, "Needs new places, sounds, or ideas to stay regulated.", "exploration", "dopamine"),
            CreateDefinition("trait_status_anxious", "status anxious", "trait", 1.04f, "Reads small social cues as long-term rank signals.", "social", "status"),
            CreateDefinition("trait_homebody_guarded", "homebody guarded", "trait", 0.9f, "Prefers intimate, predictable spaces to noisy crowds.", "home", "boundaries"),
            CreateDefinition("trait_romantic_idealizer", "romantic idealizer", "trait", 0.97f, "Falls in love with possibility before evidence.", "relationship", "romance"),
            CreateDefinition("trait_pragmatic_romantic", "pragmatic romantic", "trait", 0.91f, "Treats care as consistency, not spectacle.", "relationship", "reliability"),
            CreateDefinition("trait_people_pleaser", "people pleaser", "trait", 1.03f, "Sacrifices own needs to keep group harmony intact.", "social", "boundaries"),
            CreateDefinition("trait_boundary_keeper", "boundary keeper", "trait", 0.89f, "Protects energy by saying no early and clearly.", "wellness", "self-respect"),
            CreateDefinition("trait_financially_disciplined", "financially disciplined", "trait", 0.86f, "Treats budgeting as emotional safety infrastructure.", "finance", "stability"),
            CreateDefinition("trait_impulse_shopper", "impulse shopper", "trait", 1.06f, "Stress converts directly into carts and checkout taps.", "finance", "coping"),
            CreateDefinition("trait_food_explorer", "food explorer", "trait", 0.9f, "Uses meals as a way to investigate culture and mood.", "nutrition", "curiosity"),
            CreateDefinition("trait_sleep_defender", "sleep defender", "trait", 0.87f, "Protects rest like a non-negotiable appointment.", "rest", "health"),
            CreateDefinition("trait_doomscroll_vulnerable", "doomscroll vulnerable", "trait", 1.07f, "Can lose hours to anxious information loops.", "digital", "anxiety"),
            CreateDefinition("trait_repair_oriented", "repair oriented", "trait", 0.88f, "Prefers fixing relationships over winning arguments.", "relationship", "conflict"),
            CreateDefinition("trait_competitive_fire", "competitive fire", "trait", 0.96f, "Turns comparison into fuel and occasional turbulence.", "career", "drive"),
            CreateDefinition("trait_teacher_energy", "teacher energy", "trait", 0.9f, "Explains patiently and naturally mentors peers.", "community", "learning"),
            CreateDefinition("trait_privacy_protective", "privacy protective", "trait", 0.92f, "Treats personal data as a boundary, not a trend.", "digital", "security"),
            CreateDefinition("trait_public_performer", "public performer", "trait", 0.98f, "Feels most alive with an audience and feedback loop.", "creative", "social"),
            CreateDefinition("trait_confession_prone", "confession prone", "trait", 0.94f, "Shares inner life quickly when trust signals appear.", "social", "vulnerability"),
            CreateDefinition("trait_skeptical_realist", "skeptical realist", "trait", 0.89f, "Needs evidence before excitement feels safe.", "cognition", "risk"),
            CreateDefinition("trait_optimistic_builder", "optimistic builder", "trait", 0.88f, "Converts setbacks into practical next steps.", "resilience", "growth"),
            CreateDefinition("trait_family_loyalist", "family loyalist", "trait", 0.9f, "Prioritizes family duty even when it costs momentum.", "family", "duty"),
            CreateDefinition("trait_career_maximizer", "career maximizer", "trait", 0.97f, "Frames most choices through long-horizon career outcomes.", "career", "ambition"),
            CreateDefinition("trait_neighborhood_connector", "neighborhood connector", "trait", 0.91f, "Knows everyone and turns proximity into community.", "community", "social"),
            CreateDefinition("trait_crisis_calm", "crisis calm", "trait", 0.87f, "Becomes steadier when situations become urgent.", "stress", "leadership"),
            CreateDefinition("trait_steady_hands", "steady hands", "trait", 0.86f, "Calm fine motor control lowers failure risk during delicate survival tasks.", "survival", "precision"),
            CreateDefinition("trait_keen_eyes", "keen eyes", "trait", 0.84f, "Spots hidden resources, tracks, and threats sooner than average.", "survival", "perception"),
            CreateDefinition("trait_efficient_forager", "efficient forager", "trait", 0.88f, "Moves through gathering loops with less stamina waste.", "survival", "foraging"),
            CreateDefinition("trait_cold_resistant", "cold resistant", "trait", 0.83f, "Cold environments drain energy slower, improving winter endurance.", "survival", "climate"),
            CreateDefinition("trait_medic_touch", "medic touch", "trait", 0.87f, "Improves treatment outcomes when patching wounds and preventing infection.", "survival", "medicine"),
            CreateDefinition("trait_claustrophobic", "claustrophobic", "trait", 1.12f, "Tight spaces can trigger panic spikes that disrupt focus.", "survival", "stress"),
            CreateDefinition("trait_fragile_bones", "fragile bones", "trait", 1.15f, "Falls and impacts carry a higher injury risk.", "survival", "injury"),
            CreateDefinition("trait_impatient", "impatient", "trait", 1.09f, "Complex tasks suffer when rushed under pressure.", "survival", "crafting"),
            CreateDefinition("trait_thirst_prone", "thirst prone", "trait", 1.1f, "Hydration drains faster and water management becomes more urgent.", "survival", "needs")
        };

        private static readonly List<AuthoredDefinitionEntry> IncidentDefinitions = new()
        {
            CreateDefinition("incident_landlord_warning", "landlord warning", "incident", 1f, "A paper-thin reminder that housing can become conditional overnight.", "housing", "finance", "stress"),
            CreateDefinition("incident_viral_embarrassment", "viral embarrassment", "incident", 0.95f, "A private bad angle escapes into public memory.", "digital", "reputation", "shame"),
            CreateDefinition("incident_forgotten_birthday", "forgotten birthday", "incident", 0.85f, "An omission that hurts because it sounds so small on paper.", "family", "friendship", "hurt"),
            CreateDefinition("incident_loose_pet_scare", "loose pet scare", "incident", 0.9f, "The whole block suddenly shares one heartbeat.", "neighborhood", "care", "panic"),
            CreateDefinition("incident_power_outage_dinner", "power outage dinner", "incident", 0.8f, "Candles and melting groceries create accidental intimacy.", "household", "weather", "bonding"),
            CreateDefinition("incident_neck_bruises_rumor", "suspicious neck bruises rumor", "incident", 0.88f, "Whispers move faster than facts when someone looks unexplainably marked.", "vampire", "gossip", "reputation"),
            CreateDefinition("incident_blood_bag_theft", "blood bag theft investigation", "incident", 0.78f, "An ugly mystery with medical paperwork and hungry motives.", "medical", "crime", "vampire"),
            CreateDefinition("incident_plagiarism_accusation", "school plagiarism accusation", "incident", 0.83f, "Academic trust collapses in a single email thread.", "school", "reputation", "legalish")
        };

        private static readonly List<ArchetypeLibraryEntry> ArchetypeLibraries = new()
        {
            CreateArchetype("family_hushed_strivers", "Hushed Strivers", "families", "Ambitious family that loves through logistics instead of declarations.", new[] { "work-first", "secretly-devoted", "conflict-avoidant" }, new[] { "middle-class", "image-management" }),
            CreateArchetype("district_neon_market", "Neon Market", "districts", "Dense night district where snacks, gossip, and danger all stay open too late.", new[] { "walkable", "nightlife-heavy", "informal-economy" }, new[] { "late-night", "food", "vampire-risk" }),
            CreateArchetype("school_saints_mercy", "Saint Mercy Prep", "schools", "Prestigious school that sells discipline and quietly breeds panic.", new[] { "high-pressure", "scholarship-kids", "reputation-fragile" }, new[] { "education", "status" }),
            CreateArchetype("friends_group_chat_triage", "Group Chat Triage", "friend_groups", "Friend group bonded by emergency memes and soft surveillance.", new[] { "online-native", "protective", "messy-loyal" }, new[] { "friendship", "digital" }),
            CreateArchetype("workplace_overnight_clinic", "Overnight Clinic", "workplaces", "A night-shift workplace built from fluorescent fatigue and competence.", new[] { "burnout-prone", "gallows-humor", "high-stakes" }, new[] { "medical", "night-shift" }),
            CreateArchetype("prison_old_block_politic", "Old Block Politic", "prisons", "Hierarchy maintained through memory, favors, and public reading of weakness.", new[] { "territorial", "watchful", "ritualized" }, new[] { "crime", "survival" }),
            CreateArchetype("club_velvet_cinder", "Velvet Cinder", "clubs", "A club that treats danger as part of the dress code.", new[] { "exclusive", "fashion-forward", "predatory" }, new[] { "nightlife", "vampire-adjacent" }),
            CreateArchetype("faction_glass_thirst", "Glass Thirst Compact", "vampire_factions", "A secrecy-first faction that believes elegance is the best camouflage.", new[] { "disciplined", "status-conscious", "resource-hoarding" }, new[] { "vampire", "politics" }),
            CreateArchetype("feeding_style_consent_calendar", "Consent Calendar", "feeding_styles", "Schedules feeding like a morally anxious project manager.", new[] { "structured", "ethical", "tense" }, new[] { "vampire", "feeding" }),
            CreateArchetype("donor_archetype_lonely_helper", "Lonely Helper", "donor_archetypes", "Offers care because being needed feels easier than being known.", new[] { "caretaking", "boundary-blurry", "soft-spoken" }, new[] { "human", "feeding-economy" }),
            CreateArchetype("persona_main_character_crisis", "Main Character Crisis", "digital_persona_types", "Online persona built from aspirational confidence and private collapse.", new[] { "high-engagement", "curated", "fragile" }, new[] { "digital", "reputation" })
        };

        public static IReadOnlyList<AuthoredDefinitionEntry> GetActivities() => ActivityDefinitions;
        public static IReadOnlyList<AuthoredDefinitionEntry> GetTraits() => TraitDefinitions;
        public static IReadOnlyList<AuthoredDefinitionEntry> GetIncidents() => IncidentDefinitions;
        public static IReadOnlyList<ArchetypeLibraryEntry> GetArchetypeLibrary(string library)
            => ArchetypeLibraries.FindAll(entry => entry != null && string.Equals(entry.Library, library, StringComparison.OrdinalIgnoreCase));

        public static string GenerateLateNightEvent(LateNightEventContext context, IRandomService randomService)
        {
            context ??= new LateNightEventContext();
            WeightedTable<string> table = new();

            AddLateNightOption(table, "A random hookup flickers into being under bad neon and worse judgment.", 0.5f + (context.NightlifeIntensity * 1.2f),
                context.NightlifeIntensity >= 0.45f && IsRelationship(context.RelationshipStatus, "single", "complicated"));
            AddLateNightOption(table, "A mugging attempt turns the walk home into pure adrenaline arithmetic.", 0.25f + ((1f - context.SafetyLevel) * 1.5f),
                context.SafetyLevel <= 0.55f);
            AddLateNightOption(table, "A food truck becomes a tiny sanctuary where hot grease and sympathy reset the mood.", 0.55f + (context.Weather == "rainy" ? 0.25f : 0f),
                true);
            AddLateNightOption(table, "A night market encounter blooms into gossip, flirtation, or a suspiciously specific rumor.", 0.45f + (HasTag(context.DistrictTag, "market", "arts", "night") ? 0.45f : 0f),
                true);
            AddLateNightOption(table, "A blood temptation incident makes every pulse in the alley sound louder than common sense.", 0.2f + (context.VampirePresence ? 0.85f : 0f) + (context.HungerState * 0.75f),
                context.VampirePresence || context.HungerState >= 0.6f);
            AddLateNightOption(table, "Police questioning stalls the night just long enough for everyone to rethink their story.", 0.2f + (context.RecentScandal ? 0.5f : 0f) + ((1f - context.SafetyLevel) * 0.35f),
                context.RecentScandal || context.SafetyLevel <= 0.45f);
            AddLateNightOption(table, "A secret meet-up happens behind ordinary excuses and very deliberate text timing.", 0.35f + (IsRelationship(context.RelationshipStatus, "taken", "complicated") ? 0.45f : 0f),
                true);
            AddLateNightOption(table, "An eerie alley rumor spreads because somebody swears they saw hunger wearing a human face.", 0.18f + (context.VampirePresence ? 0.6f : 0.05f) + (context.RecentScandal ? 0.2f : 0f),
                true);

            string picked = table.Pick(randomService);
            return string.IsNullOrWhiteSpace(picked)
                ? "The late-night hours pass in a tense blur of headlights, appetite, and unfinished conversations."
                : picked;
        }

        public static string BuildTownHeadline(float pressure, bool scandalActive, bool vampireRisk)
        {
            if (pressure >= 75f)
            {
                return scandalActive
                    ? "Town headline: pressure spikes as scandal and rumor start feeding each other."
                    : "Town headline: the whole city feels one argument away from becoming tomorrow's lead story.";
            }

            if (vampireRisk)
            {
                return "Town headline: nightlife chatter keeps circling strange sightings nobody wants to document clearly.";
            }

            return "Town headline: the city keeps performing normal while everybody privately tracks the cracks.";
        }

        public static string BuildAnonymousForumPost(string subject, float paranoia, bool vampireRisk)
        {
            string topic = string.IsNullOrWhiteSpace(subject) ? "this neighborhood" : subject.Trim();
            if (vampireRisk)
            {
                return $"Anonymous post: is anyone else noticing people in {topic} acting weird about blood drives, blackout curtains, and who walks home before dawn?";
            }

            return paranoia >= 0.6f
                ? $"Anonymous post: maybe I'm spiraling, but {topic} feels off lately and everyone keeps pretending the weird parts are normal." 
                : $"Anonymous post: not saying {topic} is cursed, just saying too many suspicious things happen there after midnight.";
        }

        public static string BuildMedicalSummary(string subject, string incidentLabel)
        {
            string name = string.IsNullOrWhiteSpace(subject) ? "Patient" : subject.Trim();
            string incident = string.IsNullOrWhiteSpace(incidentLabel) ? "an unclear after-hours event" : incidentLabel.Trim();
            return $"Medical summary: {name} presented after {incident} with elevated stress, fragmented recall, and a strong need for sleep, hydration, and follow-up observation.";
        }

        public static string BuildLegalSummary(string subject, string incidentLabel, bool evidenceThin)
        {
            string name = string.IsNullOrWhiteSpace(subject) ? "Subject" : subject.Trim();
            string incident = string.IsNullOrWhiteSpace(incidentLabel) ? "the reported incident" : incidentLabel.Trim();
            string evidence = evidenceThin ? "available evidence is thin and heavily rumor-contaminated" : "available evidence appears internally consistent but socially volatile";
            return $"Legal summary: {name} is linked to {incident}; {evidence}, so witness management and timeline verification remain critical.";
        }

        private static void AddLateNightOption(WeightedTable<string> table, string value, float weight, bool include)
        {
            if (include)
            {
                table.AddOption(value, Mathf.Max(0.01f, weight));
            }
        }

        private static bool HasTag(string input, params string[] fragments)
        {
            if (string.IsNullOrWhiteSpace(input) || fragments == null)
            {
                return false;
            }

            for (int i = 0; i < fragments.Length; i++)
            {
                if (input.IndexOf(fragments[i], StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsRelationship(string status, params string[] options)
        {
            if (string.IsNullOrWhiteSpace(status) || options == null)
            {
                return false;
            }

            for (int i = 0; i < options.Length; i++)
            {
                if (string.Equals(status, options[i], StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private static AuthoredDefinitionEntry CreateDefinition(string id, string label, string category, float weight, string description, params string[] tags)
        {
            return new AuthoredDefinitionEntry
            {
                Id = id,
                Label = label,
                Category = category,
                Weight = weight,
                Description = description,
                Tags = tags != null ? new List<string>(tags) : new List<string>()
            };
        }

        private static ArchetypeLibraryEntry CreateArchetype(string id, string label, string library, string summary, string[] traits, string[] tags)
        {
            return new ArchetypeLibraryEntry
            {
                Id = id,
                Label = label,
                Library = library,
                Summary = summary,
                Traits = traits != null ? new List<string>(traits) : new List<string>(),
                Tags = tags != null ? new List<string>(tags) : new List<string>()
            };
        }
    }
}

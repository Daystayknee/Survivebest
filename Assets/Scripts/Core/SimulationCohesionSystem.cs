using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Survivebest.Events;
using Survivebest.Needs;
using Survivebest.Social;

namespace Survivebest.Core
{
    public enum LifeChainType
    {
        BadSleep,
        VampireHunger,
        Recovery,
        PressureStack
    }

    public enum LifePatternBundleType
    {
        StrugglingSingleParent,
        CollegeBurnout,
        RichButLonelySocialite,
        RecoveringAddict,
        SmallTownGoldenChild,
        PrisonReentryArc,
        SecretVampireRoommate,
        NightlifeBartender,
        AspiringInfluencer,
        OldMoneyFamilyHeir,
        NightShiftNurse,
        OccultBloggerHuntedByRumors
    }

    public enum ContradictionArchetype
    {
        IntimacySeekingCommitmentAvoidant,
        MoneySeekingDisciplineAverse,
        FamilyLoyalAvoidant,
        MoralIdealistPowerEnjoyer,
        ControlSeekingChaosCraving,
        EmpathicResentfulLowWorthLoyalist
    }

    [Serializable]
    public class LifeChainState
    {
        public string CharacterId;
        public LifeChainType ChainType;
        [Range(0f, 1f)] public float Intensity;
        public List<string> TriggerSteps = new();
        public List<string> OutcomeSteps = new();
        public string Summary;
        public int LastEvaluatedDay;
        public int LastEvaluatedHour;
    }

    [Serializable]
    public class LifePatternBundleProfile
    {
        public string CharacterId;
        public LifePatternBundleType BundleType;
        public List<string> NeedsHooks = new();
        public List<string> ScheduleHooks = new();
        public List<string> MoneyHooks = new();
        public List<string> ReputationHooks = new();
        public List<string> RelationshipHooks = new();
        public List<string> MemoryHooks = new();
        public List<string> OpportunityHooks = new();
        public List<string> VulnerabilityHooks = new();
        [Range(0f, 1f)] public float Coherence = 0.5f;
        public string Snapshot;
    }

    [Serializable]
    public class ContradictionState
    {
        public string CharacterId;
        public ContradictionArchetype Archetype;
        [Range(0f, 1f)] public float Intensity;
        public string DesirePole;
        public string ResistancePole;
        public string BehaviorSummary;
        public string MessyOutcome;
    }

    [Serializable]
    public class PressureStackState
    {
        public string CharacterId;
        [Range(0f, 1f)] public float FinancialPressure;
        [Range(0f, 1f)] public float DomesticPressure;
        [Range(0f, 1f)] public float SleepPressure;
        [Range(0f, 1f)] public float SocialPressure;
        [Range(0f, 1f)] public float ShamePressure;
        [Range(0f, 1f)] public float CompositePressure;
        public string BreakdownSummary;
    }

    [Serializable]
    public class RecoveryArchitectureState
    {
        public string CharacterId;
        [Range(0f, 1f)] public float Stability;
        [Range(0f, 1f)] public float SleepRecovery;
        [Range(0f, 1f)] public float NutritionRecovery;
        [Range(0f, 1f)] public float SocialRecovery;
        [Range(0f, 1f)] public float TherapyRecovery;
        [Range(0f, 1f)] public float EnvironmentRecovery;
        [Range(0f, 1f)] public float VampireRecovery;
        public string Summary;
    }

    public class SimulationCohesionSystem : MonoBehaviour
    {
        [SerializeField] private HumanLifeExperienceLayerSystem humanLifeExperienceLayerSystem;
        [SerializeField] private RelationshipMemorySystem relationshipMemorySystem;
        [SerializeField] private GameEventHub gameEventHub;
        [SerializeField] private World.WorldClock worldClock;
        [SerializeField] private List<LifeChainState> lifeChains = new();
        [SerializeField] private List<LifePatternBundleProfile> patternBundles = new();
        [SerializeField] private List<ContradictionState> contradictions = new();
        [SerializeField] private List<PressureStackState> pressureStacks = new();
        [SerializeField] private List<RecoveryArchitectureState> recoveries = new();

        public IReadOnlyList<LifeChainState> LifeChains => lifeChains;
        public IReadOnlyList<LifePatternBundleProfile> PatternBundles => patternBundles;
        public IReadOnlyList<ContradictionState> Contradictions => contradictions;
        public IReadOnlyList<PressureStackState> PressureStacks => pressureStacks;
        public IReadOnlyList<RecoveryArchitectureState> Recoveries => recoveries;

        public LifeChainState EvaluateBadSleepChain(CharacterCore actor, NeedsSnapshot needs)
        {
            if (actor == null || needs == null)
            {
                return null;
            }

            float sleepPenalty = Mathf.Clamp01((100f - needs.SleepQuality + needs.SleepDebt) / 180f);
            LifeChainState state = GetOrCreateLifeChain(actor.CharacterId, LifeChainType.BadSleep);
            state.Intensity = sleepPenalty;
            state.TriggerSteps = new List<string>
            {
                $"sleep quality {needs.SleepQuality:0}",
                $"sleep debt {needs.SleepDebt:0}",
                $"motivation {needs.Motivation:0}",
                $"grooming {needs.Grooming:0}"
            };

            List<string> outcomes = new();
            if (sleepPenalty > 0.25f) outcomes.Add("motivation penalty settles in early");
            if (needs.Grooming < 45f || sleepPenalty > 0.45f) outcomes.Add("grooming is easier to skip than maintain");
            if (sleepPenalty > 0.35f) outcomes.Add("work performance softens and errors become more likely");
            if (sleepPenalty > 0.45f) outcomes.Add("coworker reactions grow less patient and less generous");
            if (sleepPenalty > 0.55f) outcomes.Add("stress rises faster than resilience can absorb");
            if (needs.Hunger < 55f || sleepPenalty > 0.6f) outcomes.Add("junk-food logic starts sounding reasonable");
            if (needs.Mood < 50f || sleepPenalty > 0.65f) outcomes.Add("household friction feels sharper by night");
            if (outcomes.Count == 0) outcomes.Add("fatigue is noticeable, but the day is still salvageable");

            state.OutcomeSteps = outcomes;
            state.Summary = $"Bad sleep chain: {string.Join(" -> ", outcomes)}.";
            Stamp(state);

            if (humanLifeExperienceLayerSystem != null && sleepPenalty > 0.45f)
            {
                humanLifeExperienceLayerSystem.RecordLifeTimelineEvent(actor, "Bad sleep chain", state.Summary, "simulation_cohesion");
                relationshipMemorySystem?.RecordEvent(actor.CharacterId, null, "showed up strained from poor sleep", -Mathf.RoundToInt(sleepPenalty * 10f), false, "workplace");
            }

            Publish(actor.CharacterId, "BadSleepChain", state.Summary, state.Intensity, sleepPenalty > 0.6f ? SimulationEventSeverity.Warning : SimulationEventSeverity.Info);
            return state;
        }

        public LifeChainState EvaluateVampireHungerChain(CharacterCore actor, float bloodHunger, float selfControl, float suspicion, float donorFixation)
        {
            if (actor == null || !actor.IsVampire)
            {
                return null;
            }

            float hunger = Mathf.Clamp01(bloodHunger);
            LifeChainState state = GetOrCreateLifeChain(actor.CharacterId, LifeChainType.VampireHunger);
            state.Intensity = hunger;
            state.TriggerSteps = new List<string>
            {
                $"blood hunger {hunger:0.00}",
                $"self-control {selfControl:0.00}",
                $"suspicion {suspicion:0.00}",
                $"donor fixation {donorFixation:0.00}"
            };

            state.OutcomeSteps = new List<string>
            {
                "blood hunger rises",
                hunger > 0.35f ? "self-control decreases" : "self-control still holds",
                hunger > 0.25f ? "night focus increases" : "night instincts remain background noise",
                hunger > 0.45f ? "social warmth decreases" : "warmth only flickers",
                Mathf.Max(hunger, suspicion) > 0.55f ? "suspicion risk increases" : "masquerade risk stays manageable",
                hunger > 0.6f ? "poor choices feel efficient" : "predatory shortcuts are resisted",
                Mathf.Max(hunger, donorFixation) > 0.65f ? "donor fixation risk intensifies" : "favorite donor remains a temptation, not a plan",
                hunger > 0.72f ? "frenzy threshold lowers" : "frenzy threshold remains buffered"
            };
            state.Summary = $"Vampire hunger chain: {string.Join(" -> ", state.OutcomeSteps)}.";
            Stamp(state);

            if (humanLifeExperienceLayerSystem != null && hunger > 0.5f)
            {
                humanLifeExperienceLayerSystem.RecordLifeTimelineEvent(actor, "Vampire hunger chain", state.Summary, "simulation_cohesion");
            }

            Publish(actor.CharacterId, "VampireHungerChain", state.Summary, state.Intensity, hunger > 0.72f ? SimulationEventSeverity.Critical : SimulationEventSeverity.Warning);
            return state;
        }

        public LifePatternBundleProfile ApplyPatternBundle(CharacterCore actor, LifePatternBundleType bundleType)
        {
            if (actor == null)
            {
                return null;
            }

            LifePatternBundleProfile profile = patternBundles.Find(x => x != null && x.CharacterId == actor.CharacterId && x.BundleType == bundleType);
            if (profile == null)
            {
                profile = new LifePatternBundleProfile { CharacterId = actor.CharacterId, BundleType = bundleType };
                patternBundles.Add(profile);
            }

            ConfigureBundle(profile, bundleType);
            profile.Coherence = 0.7f;
            profile.Snapshot = BuildBundleSnapshot(profile);
            Publish(actor.CharacterId, "PatternBundle", profile.Snapshot, profile.Coherence, SimulationEventSeverity.Info);
            return profile;
        }

        public ContradictionState EvaluateContradiction(CharacterCore actor, ContradictionArchetype archetype, float desire, float resistance, float shame = 0f, float loyalty = 0f)
        {
            if (actor == null)
            {
                return null;
            }

            ContradictionState state = contradictions.Find(x => x != null && x.CharacterId == actor.CharacterId && x.Archetype == archetype);
            if (state == null)
            {
                state = new ContradictionState { CharacterId = actor.CharacterId, Archetype = archetype };
                contradictions.Add(state);
            }

            state.Intensity = Mathf.Clamp01((Mathf.Clamp01(desire) + Mathf.Clamp01(resistance) + Mathf.Clamp01(shame) + Mathf.Clamp01(loyalty)) / (archetype == ContradictionArchetype.EmpathicResentfulLowWorthLoyalist ? 4f : 2f));
            switch (archetype)
            {
                case ContradictionArchetype.IntimacySeekingCommitmentAvoidant:
                    state.DesirePole = "wants intimacy";
                    state.ResistancePole = "fears commitment";
                    state.BehaviorSummary = "seeks warmth, then pulls back when closeness becomes real";
                    state.MessyOutcome = "mixed signals, late replies, and longing without stability";
                    break;
                case ContradictionArchetype.MoneySeekingDisciplineAverse:
                    state.DesirePole = "wants money";
                    state.ResistancePole = "hates discipline";
                    state.BehaviorSummary = "dreams at a premium level but resists routines that compound into security";
                    state.MessyOutcome = "spurts of hustle followed by expensive avoidance";
                    break;
                case ContradictionArchetype.FamilyLoyalAvoidant:
                    state.DesirePole = "loves family";
                    state.ResistancePole = "avoids them";
                    state.BehaviorSummary = "interprets duty as love, but dodges the rooms where feelings would have to be spoken";
                    state.MessyOutcome = "helpful in crises, absent in ordinary closeness";
                    break;
                case ContradictionArchetype.MoralIdealistPowerEnjoyer:
                    state.DesirePole = "wants morality";
                    state.ResistancePole = "enjoys power";
                    state.BehaviorSummary = "argues for ethics until leverage appears, then likes how force feels";
                    state.MessyOutcome = "self-justified control with flashes of guilt";
                    break;
                case ContradictionArchetype.ControlSeekingChaosCraving:
                    state.DesirePole = "wants control";
                    state.ResistancePole = "craves feeding chaos";
                    state.BehaviorSummary = "builds rules specifically to stand near the edge of breaking them";
                    state.MessyOutcome = "meticulous planning punctured by predatory lapses";
                    break;
                default:
                    state.DesirePole = "high empathy";
                    state.ResistancePole = "high resentment / low self-worth / family loyalty";
                    state.BehaviorSummary = "absorbs everyone else's pain, keeps score privately, and still shows up when family calls";
                    state.MessyOutcome = "protective sacrifice followed by sharp bitterness and self-erasure";
                    break;
            }

            if (humanLifeExperienceLayerSystem != null && state.Intensity > 0.55f)
            {
                humanLifeExperienceLayerSystem.RecordLifeTimelineEvent(actor, "Contradiction spike", $"{state.DesirePole} but {state.ResistancePole}: {state.MessyOutcome}.", "simulation_cohesion");
            }

            return state;
        }

        public PressureStackState EvaluatePressureStack(CharacterCore actor, float financialPressure, float domesticPressure, float sleepPressure, float socialPressure, float shamePressure)
        {
            if (actor == null)
            {
                return null;
            }

            PressureStackState state = pressureStacks.Find(x => x != null && x.CharacterId == actor.CharacterId);
            if (state == null)
            {
                state = new PressureStackState { CharacterId = actor.CharacterId };
                pressureStacks.Add(state);
            }

            state.FinancialPressure = Mathf.Clamp01(financialPressure);
            state.DomesticPressure = Mathf.Clamp01(domesticPressure);
            state.SleepPressure = Mathf.Clamp01(sleepPressure);
            state.SocialPressure = Mathf.Clamp01(socialPressure);
            state.ShamePressure = Mathf.Clamp01(shamePressure);
            float peak = Mathf.Max(state.FinancialPressure, state.DomesticPressure, state.SleepPressure, state.SocialPressure, state.ShamePressure);
            float average = (state.FinancialPressure + state.DomesticPressure + state.SleepPressure + state.SocialPressure + state.ShamePressure) / 5f;
            state.CompositePressure = Mathf.Clamp01(average * 0.7f + peak * 0.3f + (average > 0.58f ? 0.1f : 0f));
            state.BreakdownSummary = state.CompositePressure switch
            {
                > 0.8f => "Pressure stacking is sharp enough to worsen mood, dialogue outcomes, impulsivity, memory imprint, and resilience all at once.",
                > 0.6f => "Multiple life pressures are overlapping and starting to amplify each other.",
                > 0.35f => "Several stressors are present, but they have not fully cascaded yet.",
                _ => "Pressure is present in isolated pockets rather than a full stack."
            };

            if (humanLifeExperienceLayerSystem != null && state.CompositePressure > 0.6f)
            {
                humanLifeExperienceLayerSystem.RecordLifeTimelineEvent(actor, "Pressure stack", state.BreakdownSummary, "simulation_cohesion");
            }

            return state;
        }

        public RecoveryArchitectureState EvaluateRecovery(CharacterCore actor, float sleepRecovery, float nutritionRecovery, float socialRecovery, float therapyRecovery, float environmentRecovery, float vampireRecovery = 0f)
        {
            if (actor == null)
            {
                return null;
            }

            RecoveryArchitectureState state = recoveries.Find(x => x != null && x.CharacterId == actor.CharacterId);
            if (state == null)
            {
                state = new RecoveryArchitectureState { CharacterId = actor.CharacterId };
                recoveries.Add(state);
            }

            state.SleepRecovery = Mathf.Clamp01(sleepRecovery);
            state.NutritionRecovery = Mathf.Clamp01(nutritionRecovery);
            state.SocialRecovery = Mathf.Clamp01(socialRecovery);
            state.TherapyRecovery = Mathf.Clamp01(therapyRecovery);
            state.EnvironmentRecovery = Mathf.Clamp01(environmentRecovery);
            state.VampireRecovery = Mathf.Clamp01(vampireRecovery);
            float divisor = actor.IsVampire ? 6f : 5f;
            state.Stability = Mathf.Clamp01((state.SleepRecovery + state.NutritionRecovery + state.SocialRecovery + state.TherapyRecovery + state.EnvironmentRecovery + state.VampireRecovery) / divisor);
            state.Summary = state.Stability switch
            {
                > 0.78f => actor.IsVampire
                    ? "Recovery feels earned: routines are holding, feeding is controlled, and the wider life system is softening."
                    : "Recovery feels earned: better sleep, food, support, and home order are producing visible stability.",
                > 0.5f => "Recovery is underway, but still fragile enough to lose ground under renewed pressure.",
                > 0.3f => "Repair has started, though the body and relationships still remember the strain.",
                _ => "Relief exists in fragments, not yet in a dependable pattern."
            };

            if (humanLifeExperienceLayerSystem != null && state.Stability > 0.45f)
            {
                humanLifeExperienceLayerSystem.RecordLifeTimelineEvent(actor, "Recovery architecture", state.Summary, "simulation_cohesion");
            }

            Publish(actor.CharacterId, "RecoveryArchitecture", state.Summary, state.Stability, state.Stability > 0.75f ? SimulationEventSeverity.Info : SimulationEventSeverity.Warning);
            return state;
        }

        public string BuildCohesionSummary(string characterId)
        {
            if (string.IsNullOrWhiteSpace(characterId))
            {
                return "No character selected for cohesion summary.";
            }

            StringBuilder builder = new();
            LifeChainState badSleep = lifeChains.Find(x => x != null && x.CharacterId == characterId && x.ChainType == LifeChainType.BadSleep);
            LifeChainState vampire = lifeChains.Find(x => x != null && x.CharacterId == characterId && x.ChainType == LifeChainType.VampireHunger);
            LifePatternBundleProfile bundle = patternBundles.Find(x => x != null && x.CharacterId == characterId);
            ContradictionState contradiction = contradictions.Find(x => x != null && x.CharacterId == characterId);
            PressureStackState pressure = pressureStacks.Find(x => x != null && x.CharacterId == characterId);
            RecoveryArchitectureState recovery = recoveries.Find(x => x != null && x.CharacterId == characterId);

            if (badSleep != null) builder.AppendLine($"Bad sleep: {badSleep.Summary}");
            if (vampire != null) builder.AppendLine($"Vampire hunger: {vampire.Summary}");
            if (bundle != null) builder.AppendLine($"Pattern bundle: {bundle.Snapshot}");
            if (contradiction != null) builder.AppendLine($"Contradiction: {contradiction.DesirePole} / {contradiction.ResistancePole} -> {contradiction.MessyOutcome}.");
            if (pressure != null) builder.AppendLine($"Pressure stack: {pressure.BreakdownSummary}");
            if (recovery != null) builder.AppendLine($"Recovery: {recovery.Summary}");

            return builder.Length > 0 ? builder.ToString().TrimEnd() : "No cohesion state recorded yet.";
        }

        private void ConfigureBundle(LifePatternBundleProfile profile, LifePatternBundleType bundleType)
        {
            profile.NeedsHooks.Clear();
            profile.ScheduleHooks.Clear();
            profile.MoneyHooks.Clear();
            profile.ReputationHooks.Clear();
            profile.RelationshipHooks.Clear();
            profile.MemoryHooks.Clear();
            profile.OpportunityHooks.Clear();
            profile.VulnerabilityHooks.Clear();

            switch (bundleType)
            {
                case LifePatternBundleType.StrugglingSingleParent:
                    profile.NeedsHooks.AddRange(new[] { "sleep debt", "caregiver fatigue", "meal shortcuts" });
                    profile.ScheduleHooks.AddRange(new[] { "school dropoff", "double-booked errands" });
                    profile.MoneyHooks.AddRange(new[] { "rent pressure", "childcare cost spikes" });
                    profile.ReputationHooks.AddRange(new[] { "seen as dependable", "quietly judged for lateness" });
                    profile.RelationshipHooks.AddRange(new[] { "coparent strain", "child loyalty bond" });
                    profile.MemoryHooks.AddRange(new[] { "missed recital guilt", "tiny home rituals" });
                    profile.OpportunityHooks.AddRange(new[] { "overtime offer", "community aid" });
                    profile.VulnerabilityHooks.AddRange(new[] { "burnout", "custody conflict" });
                    break;
                case LifePatternBundleType.CollegeBurnout:
                    profile.NeedsHooks.AddRange(new[] { "sleep inversion", "stimulant dependence" });
                    profile.ScheduleHooks.AddRange(new[] { "deadline pileups", "class skipping" });
                    profile.MoneyHooks.AddRange(new[] { "tuition panic", "gig work" });
                    profile.ReputationHooks.AddRange(new[] { "smart but fading", "online academic image" });
                    profile.RelationshipHooks.AddRange(new[] { "roommate tension", "friendship drift" });
                    profile.MemoryHooks.AddRange(new[] { "exam meltdowns", "all-nighter nostalgia" });
                    profile.OpportunityHooks.AddRange(new[] { "mentor rescue", "internship door" });
                    profile.VulnerabilityHooks.AddRange(new[] { "dropout risk", "shame spiral" });
                    break;
                case LifePatternBundleType.RichButLonelySocialite:
                    profile.NeedsHooks.AddRange(new[] { "curated appearance", "touch hunger" });
                    profile.ScheduleHooks.AddRange(new[] { "event circuit", "empty mornings" });
                    profile.MoneyHooks.AddRange(new[] { "family allowance", "status spending" });
                    profile.ReputationHooks.AddRange(new[] { "high visibility", "rumor magnet" });
                    profile.RelationshipHooks.AddRange(new[] { "transactional dating", "family distance" });
                    profile.MemoryHooks.AddRange(new[] { "photographed nights", "private loneliness" });
                    profile.OpportunityHooks.AddRange(new[] { "philanthropy pivot", "true friend" });
                    profile.VulnerabilityHooks.AddRange(new[] { "public embarrassment", "substance escape" });
                    break;
                case LifePatternBundleType.RecoveringAddict:
                    profile.NeedsHooks.AddRange(new[] { "craving management", "sleep repair" });
                    profile.ScheduleHooks.AddRange(new[] { "meetings", "avoidance windows" });
                    profile.MoneyHooks.AddRange(new[] { "debt repayment", "small honest income" });
                    profile.ReputationHooks.AddRange(new[] { "trust rebuilding", "old label follows" });
                    profile.RelationshipHooks.AddRange(new[] { "sponsor support", "family caution" });
                    profile.MemoryHooks.AddRange(new[] { "using flashbacks", "clean streak pride" });
                    profile.OpportunityHooks.AddRange(new[] { "second chance job", "peer leadership" });
                    profile.VulnerabilityHooks.AddRange(new[] { "relapse cues", "shame-trigger isolation" });
                    break;
                case LifePatternBundleType.SmallTownGoldenChild:
                    profile.NeedsHooks.AddRange(new[] { "performance pressure", "image maintenance" });
                    profile.ScheduleHooks.AddRange(new[] { "community events", "family obligations" });
                    profile.MoneyHooks.AddRange(new[] { "stable support", "inheritance expectations" });
                    profile.ReputationHooks.AddRange(new[] { "everyone knows you", "grace shrinks after mistakes" });
                    profile.RelationshipHooks.AddRange(new[] { "old flames linger", "parents stay involved" });
                    profile.MemoryHooks.AddRange(new[] { "high school peak", "legacy comparisons" });
                    profile.OpportunityHooks.AddRange(new[] { "mayoral grooming", "escape route" });
                    profile.VulnerabilityHooks.AddRange(new[] { "public failure", "identity foreclosure" });
                    break;
                case LifePatternBundleType.PrisonReentryArc:
                    profile.NeedsHooks.AddRange(new[] { "hypervigilance", "routine repair" });
                    profile.ScheduleHooks.AddRange(new[] { "parole check-ins", "employment gaps" });
                    profile.MoneyHooks.AddRange(new[] { "fees", "cash fragility" });
                    profile.ReputationHooks.AddRange(new[] { "stigma", "earned respect in pockets" });
                    profile.RelationshipHooks.AddRange(new[] { "family caution", "old crew pull" });
                    profile.MemoryHooks.AddRange(new[] { "inside survival habits", "release-day hope" });
                    profile.OpportunityHooks.AddRange(new[] { "reentry program", "union training" });
                    profile.VulnerabilityHooks.AddRange(new[] { "technical violations", "relapse or recidivism" });
                    break;
                case LifePatternBundleType.SecretVampireRoommate:
                    profile.NeedsHooks.AddRange(new[] { "fake sleep schedule", "blood-storage logistics" });
                    profile.ScheduleHooks.AddRange(new[] { "night cleaning", "daytime hiding" });
                    profile.MoneyHooks.AddRange(new[] { "split rent", "medical cover expenses" });
                    profile.ReputationHooks.AddRange(new[] { "weird roommate rumors", "nightlife mystique" });
                    profile.RelationshipHooks.AddRange(new[] { "protective secrecy", "feeding boundary strain" });
                    profile.MemoryHooks.AddRange(new[] { "shared lease secrets", "close-call dawns" });
                    profile.OpportunityHooks.AddRange(new[] { "trusted donor pact", "safehouse upgrade" });
                    profile.VulnerabilityHooks.AddRange(new[] { "masquerade breach", "roommate suspicion" });
                    break;
                case LifePatternBundleType.NightlifeBartender:
                    profile.NeedsHooks.AddRange(new[] { "sleep inversion", "social fatigue" });
                    profile.ScheduleHooks.AddRange(new[] { "closing shifts", "weekend peaks" });
                    profile.MoneyHooks.AddRange(new[] { "tip volatility", "cash temptation" });
                    profile.ReputationHooks.AddRange(new[] { "scene familiarity", "gossip relay" });
                    profile.RelationshipHooks.AddRange(new[] { "regulars blur boundaries", "coworker intimacy" });
                    profile.MemoryHooks.AddRange(new[] { "last call fights", "glittered loneliness" });
                    profile.OpportunityHooks.AddRange(new[] { "club promotion", "after-hours network" });
                    profile.VulnerabilityHooks.AddRange(new[] { "substance access", "predator proximity" });
                    break;
                case LifePatternBundleType.AspiringInfluencer:
                    profile.NeedsHooks.AddRange(new[] { "appearance labor", "algorithm anxiety" });
                    profile.ScheduleHooks.AddRange(new[] { "posting cadence", "brand call windows" });
                    profile.MoneyHooks.AddRange(new[] { "sponsorship instability", "gear investment" });
                    profile.ReputationHooks.AddRange(new[] { "public metrics", "cancelability" });
                    profile.RelationshipHooks.AddRange(new[] { "content-versus-intimacy", "DM overexposure" });
                    profile.MemoryHooks.AddRange(new[] { "viral spike", "comment-section bruises" });
                    profile.OpportunityHooks.AddRange(new[] { "brand deal", "creator house invite" });
                    profile.VulnerabilityHooks.AddRange(new[] { "embarrassment archive", "identity commodification" });
                    break;
                case LifePatternBundleType.OldMoneyFamilyHeir:
                    profile.NeedsHooks.AddRange(new[] { "emotional restraint", "legacy burden" });
                    profile.ScheduleHooks.AddRange(new[] { "board lunches", "charity obligations" });
                    profile.MoneyHooks.AddRange(new[] { "trust income", "inheritance control" });
                    profile.ReputationHooks.AddRange(new[] { "family name pressure", "quiet scandal risk" });
                    profile.RelationshipHooks.AddRange(new[] { "strategic marriage talk", "sibling rivalry" });
                    profile.MemoryHooks.AddRange(new[] { "ancestral expectations", "boarding-school wounds" });
                    profile.OpportunityHooks.AddRange(new[] { "dynastic power", "philanthropic reinvention" });
                    profile.VulnerabilityHooks.AddRange(new[] { "entitlement drift", "succession warfare" });
                    break;
                case LifePatternBundleType.NightShiftNurse:
                    profile.NeedsHooks.AddRange(new[] { "sleep disruption", "care fatigue" });
                    profile.ScheduleHooks.AddRange(new[] { "rotating nights", "on-call interruptions" });
                    profile.MoneyHooks.AddRange(new[] { "steady pay", "burnout overtime" });
                    profile.ReputationHooks.AddRange(new[] { "trusted healer", "hospital rumor chain" });
                    profile.RelationshipHooks.AddRange(new[] { "missed daytime rituals", "patients linger mentally" });
                    profile.MemoryHooks.AddRange(new[] { "codes", "quiet saves", "break-room tears" });
                    profile.OpportunityHooks.AddRange(new[] { "specialty certification", "travel contract" });
                    profile.VulnerabilityHooks.AddRange(new[] { "compassion exhaustion", "boundary collapse" });
                    break;
                default:
                    profile.NeedsHooks.AddRange(new[] { "obsessive focus", "sleep neglect" });
                    profile.ScheduleHooks.AddRange(new[] { "late-night posting", "rumor chasing" });
                    profile.MoneyHooks.AddRange(new[] { "monetized clicks", "precarious ad revenue" });
                    profile.ReputationHooks.AddRange(new[] { "niche fame", "town suspicion" });
                    profile.RelationshipHooks.AddRange(new[] { "source secrecy", "friends worry" });
                    profile.MemoryHooks.AddRange(new[] { "creepy sightings", "receipts nobody believed" });
                    profile.OpportunityHooks.AddRange(new[] { "big exposé", "cult whistleblower" });
                    profile.VulnerabilityHooks.AddRange(new[] { "harassment", "being hunted for what you found" });
                    break;
            }
        }

        private string BuildBundleSnapshot(LifePatternBundleProfile profile)
        {
            string needs = profile.NeedsHooks.Count > 0 ? profile.NeedsHooks[0] : "basic needs";
            string schedule = profile.ScheduleHooks.Count > 0 ? profile.ScheduleHooks[0] : "routine";
            string money = profile.MoneyHooks.Count > 0 ? profile.MoneyHooks[0] : "money strain";
            string relationship = profile.RelationshipHooks.Count > 0 ? profile.RelationshipHooks[0] : "relationship tension";
            string opportunity = profile.OpportunityHooks.Count > 0 ? profile.OpportunityHooks[0] : "small opportunity";
            string vulnerability = profile.VulnerabilityHooks.Count > 0 ? profile.VulnerabilityHooks[0] : "fragility";
            return $"{profile.BundleType}: needs orbit around {needs}, schedule around {schedule}, money around {money}, relationships around {relationship}, with {opportunity} fighting {vulnerability}.";
        }

        private LifeChainState GetOrCreateLifeChain(string characterId, LifeChainType type)
        {
            LifeChainState state = lifeChains.Find(x => x != null && x.CharacterId == characterId && x.ChainType == type);
            if (state != null)
            {
                return state;
            }

            state = new LifeChainState { CharacterId = characterId, ChainType = type };
            lifeChains.Add(state);
            return state;
        }

        private void Stamp(LifeChainState state)
        {
            state.LastEvaluatedDay = worldClock != null ? worldClock.Day : 0;
            state.LastEvaluatedHour = worldClock != null ? worldClock.Hour : 0;
        }

        private void Publish(string characterId, string eventKey, string description, float intensity, SimulationEventSeverity severity)
        {
            gameEventHub?.Publish(new SimulationEvent
            {
                Type = SimulationEventType.DayStageChanged,
                SourceCharacterId = characterId,
                ChangeKey = eventKey,
                Reason = description,
                Magnitude = Mathf.Clamp01(intensity),
                Severity = severity,
                SystemName = nameof(SimulationCohesionSystem)
            });
        }
    }
}

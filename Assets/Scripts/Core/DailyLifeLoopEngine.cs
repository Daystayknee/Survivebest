using System;
using System.Collections.Generic;
using Survivebest.Economy;
using Survivebest.Emotion;
using Survivebest.Health;
using Survivebest.Needs;
using Survivebest.NPC;
using Survivebest.Social;
using Survivebest.World;
using UnityEngine;

namespace Survivebest.Core
{
    [Serializable]
    public class LifeStateSnapshot
    {
        public string CharacterId;
        public float Hunger;
        public float Energy;
        public float Stress;
        public float Money;
        public float Vitality;
        public float RelationshipStrain;
        public float MentalRisk;
        public float WeatherStress;
        public bool HasInjuryRisk;
        public string Summary;
    }

    [Serializable]
    public class DynamicLifeGoal
    {
        public string GoalId;
        public string Label;
        [Range(0f, 1f)] public float Priority;
        public AutonomousActionType SuggestedAction;
        public string ConflictWithGoalId;
    }

    [Serializable]
    public class ActionRippleResult
    {
        public string ActionId;
        public string RippleSummary;
        public List<string> Effects = new();
    }

    public class ContextCollisionEngine : MonoBehaviour
    {
        public string Resolve(LifeStateSnapshot snapshot, AutonomousActionType action)
        {
            if (snapshot == null)
            {
                return "No snapshot.";
            }

            if (snapshot.Energy < 0.35f && snapshot.Stress > 0.65f && action == AutonomousActionType.Socialize)
            {
                return "Exhausted social contact escalated into argument risk.";
            }

            if (snapshot.Hunger > 0.7f && snapshot.Money < 20f && action == AutonomousActionType.Work)
            {
                return "Working while starving increased burnout pressure.";
            }

            if (snapshot.HasInjuryRisk && action != AutonomousActionType.Medicate)
            {
                return "Untreated injury risk worsened under daily load.";
            }

            return "No severe collision detected.";
        }
    }

    public class LifeReflectionEngine : MonoBehaviour
    {
        [SerializeField] private HumanLifeExperienceLayerSystem humanLifeExperienceLayerSystem;
        [SerializeField] private PsychologicalGrowthMentalHealthEngine mentalHealthEngine;

        private void Awake()
        {
            if (humanLifeExperienceLayerSystem == null)
            {
                humanLifeExperienceLayerSystem = GetComponent<HumanLifeExperienceLayerSystem>();
            }

            if (mentalHealthEngine == null)
            {
                mentalHealthEngine = GetComponent<PsychologicalGrowthMentalHealthEngine>();
            }
        }

        public string ProcessDay(CharacterCore actor, LifeStateSnapshot snapshot, ActionRippleResult ripple, string collisionSummary)
        {
            if (actor == null || snapshot == null)
            {
                return "No reflection available.";
            }

            float pressure = Mathf.Clamp01(snapshot.Stress * 0.45f + (1f - snapshot.Energy) * 0.3f + snapshot.RelationshipStrain * 0.25f);
            LifeReflectionType type = pressure > 0.65f ? LifeReflectionType.Fear : snapshot.Money < 20f ? LifeReflectionType.Regret : LifeReflectionType.Hope;
            humanLifeExperienceLayerSystem?.LogReflection(actor, type, pressure);

            if (mentalHealthEngine != null)
            {
                mentalHealthEngine.RecordLifeEvent(actor.CharacterId, pressure > 0.6f ? MentalHealthEventType.CareerPressure : MentalHealthEventType.Reflection, 0.35f + pressure * 0.5f);
            }

            return $"Reflection: {type} | {ripple?.RippleSummary ?? "No ripple"} | {collisionSummary}";
        }
    }

    public class ActionRippleHandler : MonoBehaviour
    {
        [SerializeField] private NeedsSystem needsSystem;
        [SerializeField] private EmotionSystem emotionSystem;
        [SerializeField] private HealthSystem healthSystem;
        [SerializeField] private EconomyManager economyManager;
        [SerializeField] private HumanLifeExperienceLayerSystem humanLifeExperienceLayerSystem;

        private void Awake()
        {
            if (needsSystem == null) needsSystem = GetComponent<NeedsSystem>();
            if (emotionSystem == null) emotionSystem = GetComponent<EmotionSystem>();
            if (healthSystem == null) healthSystem = GetComponent<HealthSystem>();
            if (humanLifeExperienceLayerSystem == null) humanLifeExperienceLayerSystem = GetComponent<HumanLifeExperienceLayerSystem>();
        }

        public ActionRippleResult Execute(CharacterCore actor, AutonomousActionType action)
        {
            ActionRippleResult result = new() { ActionId = action.ToString() };
            if (actor == null)
            {
                result.RippleSummary = "No actor.";
                return result;
            }

            switch (action)
            {
                case AutonomousActionType.Work:
                    needsSystem?.ModifyEnergy(-20f);
                    emotionSystem?.ModifyStress(12f);
                    healthSystem?.Damage(1.5f);
                    economyManager?.Deposit("household", 55f, "Work shift");
                    result.Effects.Add("Energy -20");
                    result.Effects.Add("Stress +12");
                    result.Effects.Add("Money +55");
                    break;
                case AutonomousActionType.Eat:
                    needsSystem?.RestoreHunger(25f);
                    needsSystem?.RestoreHydration(8f);
                    emotionSystem?.ModifyStress(-6f);
                    economyManager?.TryCharge("household", 12f, "Meal");
                    result.Effects.Add("Hunger relief");
                    result.Effects.Add("Stress -6");
                    break;
                case AutonomousActionType.Sleep:
                    needsSystem?.ModifyEnergy(22f);
                    emotionSystem?.ModifyStress(-8f);
                    healthSystem?.Heal(2f);
                    result.Effects.Add("Energy +22");
                    result.Effects.Add("Stress -8");
                    break;
                case AutonomousActionType.Medicate:
                    healthSystem?.Heal(5f);
                    emotionSystem?.ModifyStress(-4f);
                    economyManager?.TryCharge("household", 18f, "Medication");
                    result.Effects.Add("Vitality +5");
                    break;
                case AutonomousActionType.Socialize:
                    emotionSystem?.ApplySocialInteraction(0.7f);
                    needsSystem?.ModifyEnergy(-8f);
                    result.Effects.Add("Social battery used");
                    break;
                default:
                    needsSystem?.ModifyEnergy(-4f);
                    result.Effects.Add("Minor fatigue");
                    break;
            }

            result.RippleSummary = result.Effects.Count > 0 ? string.Join(", ", result.Effects) : "No major ripple.";
            humanLifeExperienceLayerSystem?.RecordLifeTimelineEvent(actor, $"Action ripple: {action}", result.RippleSummary, "action_ripple");
            return result;
        }
    }

    public class DynamicGoalGenerator : MonoBehaviour
    {
        [SerializeField] private LifestyleBehaviorSystem lifestyleBehaviorSystem;
        [SerializeField] private PersonalityDecisionSystem personalityDecisionSystem;

        public List<DynamicLifeGoal> GenerateDynamicGoals(LifeStateSnapshot snapshot)
        {
            List<DynamicLifeGoal> goals = new();
            if (snapshot == null)
            {
                return goals;
            }

            goals.Add(new DynamicLifeGoal
            {
                GoalId = "eat",
                Label = "Eat something",
                Priority = Mathf.Clamp01(snapshot.Hunger),
                SuggestedAction = AutonomousActionType.Eat,
                ConflictWithGoalId = snapshot.Money < 15f ? "work" : null
            });
            goals.Add(new DynamicLifeGoal
            {
                GoalId = "work",
                Label = "Go to work",
                Priority = Mathf.Clamp01((1f - snapshot.Money / 120f) + snapshot.Stress * 0.15f),
                SuggestedAction = AutonomousActionType.Work,
                ConflictWithGoalId = snapshot.Energy < 0.35f ? "sleep" : null
            });
            goals.Add(new DynamicLifeGoal
            {
                GoalId = "recover",
                Label = "Sleep and stabilize",
                Priority = Mathf.Clamp01((1f - snapshot.Energy) * 0.9f + snapshot.Stress * 0.1f),
                SuggestedAction = AutonomousActionType.Sleep,
                ConflictWithGoalId = "work"
            });
            goals.Add(new DynamicLifeGoal
            {
                GoalId = "treat",
                Label = "Treat infection",
                Priority = snapshot.HasInjuryRisk ? 0.85f : 0.25f,
                SuggestedAction = AutonomousActionType.Medicate,
                ConflictWithGoalId = snapshot.Money < 20f ? "eat" : null
            });

            float introvertBias = lifestyleBehaviorSystem != null ? lifestyleBehaviorSystem.GetIdentityStrength("introvert") : 0f;
            float socialPriority = Mathf.Clamp01(snapshot.RelationshipStrain * 0.6f + (1f - introvertBias) * 0.2f);
            goals.Add(new DynamicLifeGoal
            {
                GoalId = "social",
                Label = "Repair relationship",
                Priority = socialPriority,
                SuggestedAction = AutonomousActionType.Socialize,
                ConflictWithGoalId = snapshot.Stress > 0.75f ? "recover" : null
            });

            goals.Sort((a, b) => b.Priority.CompareTo(a.Priority));
            return goals;
        }

        public AutonomousActionType ChooseDirection(CharacterCore actor, LifeStateSnapshot snapshot, List<DynamicLifeGoal> goals, float memoryInfluence = 0.2f)
        {
            if (goals == null || goals.Count == 0)
            {
                return personalityDecisionSystem != null && actor != null
                    ? personalityDecisionSystem.DecideNextAction(actor)
                    : AutonomousActionType.Rest;
            }

            float best = float.MinValue;
            AutonomousActionType pick = AutonomousActionType.Rest;
            for (int i = 0; i < goals.Count; i++)
            {
                DynamicLifeGoal goal = goals[i];
                float emotionModifier = snapshot != null ? snapshot.Stress * (goal.SuggestedAction == AutonomousActionType.Sleep ? -0.1f : 0.12f) : 0f;
                float risk = snapshot != null ? (goal.SuggestedAction == AutonomousActionType.Work && snapshot.Energy < 0.3f ? 0.25f : 0f) : 0f;
                float score = goal.Priority + emotionModifier + memoryInfluence - risk;
                if (score > best)
                {
                    best = score;
                    pick = goal.SuggestedAction;
                }
            }

            return pick;
        }
    }

    public class DailyLifeLoopEngine : MonoBehaviour
    {
        [Header("Wiring")]
        [SerializeField] private NeedsSystem needsSystem;
        [SerializeField] private EmotionSystem emotionSystem;
        [SerializeField] private HealthSystem healthSystem;
        [SerializeField] private EconomyManager economyManager;
        [SerializeField] private RelationshipMemorySystem relationshipMemorySystem;
        [SerializeField] private PsychologicalGrowthMentalHealthEngine psychologicalGrowthMentalHealthEngine;
        [SerializeField] private HouseholdManager householdManager;
        [SerializeField] private WeatherManager weatherManager;
        [SerializeField] private DynamicGoalGenerator dynamicGoalGenerator;
        [SerializeField] private ActionRippleHandler actionRippleHandler;
        [SerializeField] private ContextCollisionEngine contextCollisionEngine;
        [SerializeField] private LifeReflectionEngine lifeReflectionEngine;
        [SerializeField] private AIDirectorDramaManager aiDirectorDramaManager;
        [SerializeField] private AdaptiveLifeEventsDirector adaptiveLifeEventsDirector;
        [SerializeField] private WorldEventDirector worldEventDirector;
        [SerializeField] private WorldClock worldClock;
        [SerializeField] private DaySliceManager daySliceManager;
        [SerializeField] private TownSimulationManager townSimulationManager;
        [SerializeField] private NpcScheduleSystem npcScheduleSystem;
        [SerializeField] private WorldPersistenceCullingSystem worldPersistenceCullingSystem;

        private void Awake()
        {
            if (needsSystem == null) needsSystem = GetComponent<NeedsSystem>();
            if (emotionSystem == null) emotionSystem = GetComponent<EmotionSystem>();
            if (healthSystem == null) healthSystem = GetComponent<HealthSystem>();
            if (economyManager == null) economyManager = GetComponent<EconomyManager>();
            if (relationshipMemorySystem == null) relationshipMemorySystem = GetComponent<RelationshipMemorySystem>();
            if (psychologicalGrowthMentalHealthEngine == null) psychologicalGrowthMentalHealthEngine = GetComponent<PsychologicalGrowthMentalHealthEngine>();
            if (dynamicGoalGenerator == null) dynamicGoalGenerator = GetComponent<DynamicGoalGenerator>();
            if (actionRippleHandler == null) actionRippleHandler = GetComponent<ActionRippleHandler>();
            if (contextCollisionEngine == null) contextCollisionEngine = GetComponent<ContextCollisionEngine>();
            if (lifeReflectionEngine == null) lifeReflectionEngine = GetComponent<LifeReflectionEngine>();
            if (aiDirectorDramaManager == null) aiDirectorDramaManager = GetComponent<AIDirectorDramaManager>();
            if (adaptiveLifeEventsDirector == null) adaptiveLifeEventsDirector = GetComponent<AdaptiveLifeEventsDirector>();
            if (worldClock == null) worldClock = GetComponent<WorldClock>();
            if (daySliceManager == null) daySliceManager = GetComponent<DaySliceManager>();
            if (townSimulationManager == null) townSimulationManager = GetComponent<TownSimulationManager>();
            if (npcScheduleSystem == null) npcScheduleSystem = GetComponent<NpcScheduleSystem>();
            if (worldPersistenceCullingSystem == null) worldPersistenceCullingSystem = GetComponent<WorldPersistenceCullingSystem>();
        }

        public LifeStateSnapshot BuildSnapshot(CharacterCore character)
        {
            if (character == null)
            {
                return null;
            }

            float money = economyManager != null ? economyManager.GetBalance("household") : 0f;
            float vitality = healthSystem != null ? healthSystem.Vitality / 100f : 0.8f;
            float relationshipStrain = 0.4f;
            if (relationshipMemorySystem != null)
            {
                string insight = relationshipMemorySystem.BuildMemoryInsight(character.CharacterId);
                relationshipStrain = insight.Contains("hurt", StringComparison.OrdinalIgnoreCase) || insight.Contains("betray", StringComparison.OrdinalIgnoreCase) ? 0.75f : 0.35f;
            }

            float mentalRisk = 0.35f;
            if (psychologicalGrowthMentalHealthEngine != null)
            {
                List<string> flags = psychologicalGrowthMentalHealthEngine.GetMentalHealthRiskFlags(character.CharacterId);
                mentalRisk = Mathf.Clamp01(flags.Count / 5f);
            }

            float weatherStress = weatherManager != null && (weatherManager.CurrentWeather == WeatherState.Heatwave || weatherManager.CurrentWeather == WeatherState.Stormy) ? 0.7f : 0.25f;

            LifeStateSnapshot snapshot = new()
            {
                CharacterId = character.CharacterId,
                Hunger = needsSystem != null ? Mathf.Clamp01(1f - needsSystem.Hunger / 100f) : 0.4f,
                Energy = needsSystem != null ? Mathf.Clamp01(needsSystem.Energy / 100f) : 0.6f,
                Stress = emotionSystem != null ? Mathf.Clamp01(emotionSystem.Stress / 100f) : 0.4f,
                Money = money,
                Vitality = vitality,
                RelationshipStrain = relationshipStrain,
                MentalRisk = mentalRisk,
                WeatherStress = weatherStress,
                HasInjuryRisk = vitality < 0.55f
            };
            snapshot.Summary = $"Hunger {snapshot.Hunger:0.00}, Energy {snapshot.Energy:0.00}, Stress {snapshot.Stress:0.00}, Money {snapshot.Money:0.0}, Vitality {snapshot.Vitality:0.00}";
            return snapshot;
        }

        public string RunLoop(CharacterCore character)
        {
            if (character == null)
            {
                return "No character to simulate.";
            }

            LifeStateSnapshot snapshot = BuildSnapshot(character);
            List<DynamicLifeGoal> goals = dynamicGoalGenerator != null ? dynamicGoalGenerator.GenerateDynamicGoals(snapshot) : new List<DynamicLifeGoal>();
            AutonomousActionType action = dynamicGoalGenerator != null
                ? dynamicGoalGenerator.ChooseDirection(character, snapshot, goals)
                : AutonomousActionType.Rest;
            ActionRippleResult ripple = actionRippleHandler != null ? actionRippleHandler.Execute(character, action) : new ActionRippleResult { ActionId = action.ToString(), RippleSummary = "No ripple handler." };
            string collision = contextCollisionEngine != null ? contextCollisionEngine.Resolve(snapshot, action) : "No context collision engine.";
            InjectEvents(character, snapshot, action, collision);
            RecordMemory(character, action, ripple, collision);
            string reflection = lifeReflectionEngine != null ? lifeReflectionEngine.ProcessDay(character, snapshot, ripple, collision) : "No reflection engine.";
            AdvanceWorld();

            return $"Loop: {snapshot.Summary} | Goal {(goals.Count > 0 ? goals[0].Label : "none")} | Action {action} | Collision {collision} | {reflection}";
        }

        private void InjectEvents(CharacterCore character, LifeStateSnapshot snapshot, AutonomousActionType action, string collision)
        {
            if (snapshot == null || character == null)
            {
                return;
            }

            if (snapshot.Stress > 0.75f || snapshot.Money < 25f || snapshot.RelationshipStrain > 0.65f)
            {
                aiDirectorDramaManager?.RegisterMeaningfulBeat(Mathf.Clamp01(snapshot.Stress + snapshot.RelationshipStrain));
                aiDirectorDramaManager?.EvaluateAndInject();
                adaptiveLifeEventsDirector?.DirectBeatForActiveCharacter(worldClock != null ? worldClock.Hour : 12);
            }

            if (action == AutonomousActionType.Work && snapshot.Money < 15f)
            {
                aiDirectorDramaManager?.RegisterMeaningfulBeat(0.45f);
                aiDirectorDramaManager?.EvaluateAndInject();
            }
        }

        private void RecordMemory(CharacterCore character, AutonomousActionType action, ActionRippleResult ripple, string collision)
        {
            if (character == null || relationshipMemorySystem == null)
            {
                return;
            }

            int impact = collision.Contains("No", StringComparison.OrdinalIgnoreCase) ? 2 : -5;
            relationshipMemorySystem.RecordEventDetailed(character.CharacterId, character.CharacterId, $"Daily loop {action}: {ripple?.RippleSummary}", impact, false, "daily_loop");
        }

        private void AdvanceWorld()
        {
            int nextHour = worldClock != null ? (worldClock.Hour + 1) % 24 : 0;
            daySliceManager?.AdvanceStage();
            townSimulationManager?.RecomputeTownState();
            worldPersistenceCullingSystem?.SimulateOffscreenHours(1);
        }
    }
}

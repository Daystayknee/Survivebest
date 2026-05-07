using System;
using System.Collections.Generic;
using UnityEngine;
using Survivebest.Core;
using Survivebest.Events;
using Survivebest.Health;
using Survivebest.Needs;
using Survivebest.Status;
using Survivebest.World;

namespace Survivebest.Combat
{
    public enum EnemyArchetype
    {
        FastWeak,
        Tank,
        Ranged,
        Stealth,
        Summoner,
        Elite,
        Miniboss,
        WorldBoss
    }

    public enum EnemyCombatIntent
    {
        Advance,
        Flank,
        Retreat,
        InvestigateSound,
        PackHunt,
        RangedPressure,
        SummonAdds,
        Ambush,
        Enrage
    }

    [Serializable]
    public class EnemyDefinition
    {
        public string Id;
        public string DisplayName;
        public EnemyArchetype Archetype;
        [Range(1f, 5000f)] public float MaxHealth = 20f;
        [Range(0.1f, 3f)] public float MoveSpeed = 1f;
        [Range(0f, 80f)] public float MeleeDamage = 4f;
        [Range(0f, 80f)] public float RangedDamage;
        [Range(0f, 1f)] public float Armor;
        [Range(0f, 1f)] public float StealthRating;
        [Range(0f, 1f)] public float HearingSensitivity = 0.35f;
        [Range(0f, 1f)] public float RetreatHealthThreshold = 0.18f;
        [Range(0f, 1f)] public float FlankPreference = 0.25f;
        [Range(0f, 1f)] public float PackSynergy = 0.2f;
        [Range(0f, 1f)] public float SummonChance;
        public List<string> StatusEffectIds = new List<string>();
        public List<BiomeType> PreferredBiomes = new List<BiomeType>();
    }

    [Serializable]
    public class EnemyRuntimeState
    {
        public string RuntimeId;
        public EnemyDefinition Definition;
        public float Health;
        public bool IsAlerted;
        public bool IsInvestigating;
        public bool IsRetreating;
        public bool HasFlanked;
        public Vector3 LastKnownSoundPosition;
        public float LastHeardSoundIntensity;
        public int SummonedAllies;

        public float HealthPercent => Definition == null || Definition.MaxHealth <= 0f ? 0f : Mathf.Clamp01(Health / Definition.MaxHealth);
    }

    [Serializable]
    public class CombatAiDecision
    {
        public string EnemyRuntimeId;
        public EnemyCombatIntent Intent;
        public string Reason;
        public float ThreatScore;
        public float DamageProjected;
    }

    public class CombatExpansionSystem : MonoBehaviour
    {
        [SerializeField] private BiomeManager biomeManager;
        [SerializeField] private DayNightCycleSystem dayNightCycleSystem;
        [SerializeField] private WeatherManager weatherManager;
        [SerializeField] private GameEventHub gameEventHub;
        [SerializeField] private List<EnemyDefinition> enemyDefinitions = new List<EnemyDefinition>();
        [SerializeField] private List<EnemyRuntimeState> activeEnemies = new List<EnemyRuntimeState>();
        [SerializeField, Range(0f, 1f)] private float groupFlankBonus = 0.2f;
        [SerializeField, Range(0f, 1f)] private float nightStealthBonus = 0.18f;
        [SerializeField, Min(0f)] private float worldBossAnnouncementThreat = 12f;

        public event Action<IReadOnlyList<EnemyRuntimeState>> OnEncounterSpawned;
        public event Action<CombatAiDecision> OnAiDecisionMade;
        public event Action<EnemyRuntimeState> OnEnemyDefeated;

        public IReadOnlyList<EnemyDefinition> EnemyDefinitions => enemyDefinitions;
        public IReadOnlyList<EnemyRuntimeState> ActiveEnemies => activeEnemies;

        private void Awake()
        {
            EnsureEnemyDefinitions();
        }

        public List<EnemyRuntimeState> SpawnEncounter(BiomeType biome, int threatTier, int count = 0)
        {
            EnsureEnemyDefinitions();
            List<EnemyDefinition> pool = enemyDefinitions.FindAll(e => e != null && (e.PreferredBiomes.Count == 0 || e.PreferredBiomes.Contains(biome)));
            if (pool.Count == 0)
            {
                pool = enemyDefinitions;
            }

            int finalCount = count > 0 ? count : Mathf.Clamp(1 + threatTier / 2, 1, 6);
            List<EnemyRuntimeState> spawned = new List<EnemyRuntimeState>();
            for (int i = 0; i < finalCount; i++)
            {
                EnemyDefinition picked = PickEnemyForThreat(pool, threatTier, i == 0);
                EnemyRuntimeState runtime = new EnemyRuntimeState
                {
                    RuntimeId = Guid.NewGuid().ToString("N"),
                    Definition = picked,
                    Health = picked.MaxHealth,
                    IsAlerted = picked.Archetype is EnemyArchetype.Elite or EnemyArchetype.Miniboss or EnemyArchetype.WorldBoss
                };
                activeEnemies.Add(runtime);
                spawned.Add(runtime);
            }

            OnEncounterSpawned?.Invoke(spawned);
            PublishCombatEvent(SimulationEventType.CombatEncounterStarted, "EncounterSpawned", BuildEncounterSummary(spawned, biome), ResolveEncounterMagnitude(spawned), SimulationEventSeverity.Warning);
            return spawned;
        }

        public void ReportSound(Vector3 position, float loudness, string source = "player")
        {
            float clamped = Mathf.Clamp01(loudness);
            for (int i = 0; i < activeEnemies.Count; i++)
            {
                EnemyRuntimeState enemy = activeEnemies[i];
                if (enemy == null || enemy.Definition == null)
                {
                    continue;
                }

                float weatherMuffle = weatherManager != null ? Mathf.Lerp(0.25f, 1f, weatherManager.CurrentGameplayProfile.VisibilityMultiplier) : 1f;
                float heard = clamped * enemy.Definition.HearingSensitivity * weatherMuffle;
                if (heard <= 0.12f)
                {
                    continue;
                }

                enemy.LastKnownSoundPosition = position;
                enemy.LastHeardSoundIntensity = heard;
                enemy.IsInvestigating = true;
                enemy.IsAlerted = heard >= 0.45f;
            }

            PublishCombatEvent(SimulationEventType.CombatAiStateChanged, "SoundReported", $"Enemies heard {source} noise at intensity {clamped:0.00}.", clamped, SimulationEventSeverity.Info);
        }

        public List<CombatAiDecision> ResolveEnemyTurns(CharacterCore target, float targetNoise = 0.2f, bool targetVisible = true)
        {
            List<CombatAiDecision> decisions = new List<CombatAiDecision>();
            int packCount = activeEnemies.FindAll(e => e != null && e.Health > 0f).Count;
            for (int i = activeEnemies.Count - 1; i >= 0; i--)
            {
                EnemyRuntimeState enemy = activeEnemies[i];
                if (enemy == null || enemy.Definition == null || enemy.Health <= 0f)
                {
                    activeEnemies.RemoveAt(i);
                    continue;
                }

                CombatAiDecision decision = BuildDecision(enemy, packCount, targetNoise, targetVisible);
                ApplyDecisionPressure(enemy, target, decision, packCount);
                decisions.Add(decision);
                OnAiDecisionMade?.Invoke(decision);
                PublishCombatEvent(SimulationEventType.CombatAiStateChanged, decision.Intent.ToString(), decision.Reason, decision.ThreatScore, decision.Intent is EnemyCombatIntent.Retreat ? SimulationEventSeverity.Info : SimulationEventSeverity.Warning);
            }

            return decisions;
        }

        public void DamageEnemy(string runtimeId, float rawDamage, string sourceStatusId = null)
        {
            EnemyRuntimeState enemy = activeEnemies.Find(e => e.RuntimeId == runtimeId);
            if (enemy == null || enemy.Definition == null)
            {
                return;
            }

            float armorReduction = Mathf.Clamp01(enemy.Definition.Armor);
            float applied = Mathf.Max(0f, rawDamage * (1f - armorReduction));
            enemy.Health = Mathf.Max(0f, enemy.Health - applied);
            enemy.IsAlerted = true;

            if (enemy.Health <= 0f)
            {
                activeEnemies.Remove(enemy);
                OnEnemyDefeated?.Invoke(enemy);
                PublishCombatEvent(SimulationEventType.EnemyDefeated, enemy.Definition.DisplayName, $"Defeated {enemy.Definition.DisplayName} using {sourceStatusId ?? "direct damage"}.", applied, SimulationEventSeverity.Info);
            }
        }

        private CombatAiDecision BuildDecision(EnemyRuntimeState enemy, int packCount, float targetNoise, bool targetVisible)
        {
            EnemyDefinition definition = enemy.Definition;
            float packPressure = Mathf.Clamp01((packCount - 1) * definition.PackSynergy);
            float stealth = definition.StealthRating + (dayNightCycleSystem != null && dayNightCycleSystem.IsNight ? nightStealthBonus : 0f);
            float flankChance = Mathf.Clamp01(definition.FlankPreference + packPressure + groupFlankBonus);

            if (enemy.HealthPercent <= definition.RetreatHealthThreshold && definition.Archetype is not EnemyArchetype.Tank and not EnemyArchetype.WorldBoss)
            {
                enemy.IsRetreating = true;
                return Decision(enemy, EnemyCombatIntent.Retreat, "Low health triggers retreat and regroup behavior.", 0.35f, 0f);
            }

            if (!targetVisible && enemy.IsInvestigating && enemy.LastHeardSoundIntensity > 0.1f)
            {
                return Decision(enemy, EnemyCombatIntent.InvestigateSound, $"Investigating last sound at {enemy.LastKnownSoundPosition}.", enemy.LastHeardSoundIntensity, 0f);
            }

            if (definition.Archetype == EnemyArchetype.Summoner && enemy.SummonedAllies < 3 && UnityEngine.Random.value <= definition.SummonChance)
            {
                enemy.SummonedAllies++;
                SpawnSummonedAlly();
                return Decision(enemy, EnemyCombatIntent.SummonAdds, "Summoner calls fast weak allies to split player attention.", 0.85f, 0f);
            }

            if (definition.Archetype == EnemyArchetype.Stealth && stealth > 0.55f && !enemy.HasFlanked)
            {
                enemy.HasFlanked = true;
                return Decision(enemy, EnemyCombatIntent.Ambush, "Stealth enemy uses darkness and low visibility to ambush.", stealth, definition.MeleeDamage * 1.35f);
            }

            if (definition.Archetype == EnemyArchetype.Ranged)
            {
                return Decision(enemy, EnemyCombatIntent.RangedPressure, "Ranged enemy keeps distance and pressures exposed targets.", 0.65f, definition.RangedDamage);
            }

            if (packCount >= 3 && UnityEngine.Random.value < packPressure)
            {
                return Decision(enemy, EnemyCombatIntent.PackHunt, "Pack hunting behavior coordinates bites and blocks retreat paths.", packPressure, definition.MeleeDamage * (1f + packPressure));
            }

            if (!enemy.HasFlanked && UnityEngine.Random.value < flankChance)
            {
                enemy.HasFlanked = true;
                return Decision(enemy, EnemyCombatIntent.Flank, "Enemy attempts a flank instead of trading hits head-on.", flankChance, definition.MeleeDamage * 1.15f);
            }

            if (definition.Archetype is EnemyArchetype.Elite or EnemyArchetype.Miniboss or EnemyArchetype.WorldBoss && enemy.HealthPercent < 0.5f)
            {
                return Decision(enemy, EnemyCombatIntent.Enrage, "Elite enemy changes pattern at low health.", 1f, definition.MeleeDamage * 1.25f);
            }

            return Decision(enemy, EnemyCombatIntent.Advance, targetNoise > 0.5f ? "Enemy advances toward loud target." : "Enemy advances cautiously.", 0.45f, definition.MeleeDamage);
        }

        private void ApplyDecisionPressure(EnemyRuntimeState enemy, CharacterCore target, CombatAiDecision decision, int packCount)
        {
            if (target == null || target.IsDead || decision == null)
            {
                return;
            }

            NeedsSystem needs = target.GetComponent<NeedsSystem>();
            HealthSystem health = target.GetComponent<HealthSystem>();
            StatusEffectSystem statuses = target.GetComponent<StatusEffectSystem>();
            float groupMultiplier = 1f + Mathf.Clamp01((packCount - 1) * 0.08f);
            float damage = decision.DamageProjected * groupMultiplier;

            if (damage > 0f)
            {
                health?.Damage(damage);
                needs?.ModifyEnergy(-Mathf.Clamp(damage * 0.35f, 0.2f, 8f));
            }

            ApplyEnemyStatuses(statuses, enemy.Definition);
        }

        private void ApplyEnemyStatuses(StatusEffectSystem statuses, EnemyDefinition definition)
        {
            if (statuses == null || definition == null || definition.StatusEffectIds == null)
            {
                return;
            }

            for (int i = 0; i < definition.StatusEffectIds.Count; i++)
            {
                if (UnityEngine.Random.value <= ResolveStatusChance(definition.Archetype))
                {
                    statuses.ApplyStatusById(definition.StatusEffectIds[i], ResolveStatusDuration(definition.Archetype));
                }
            }
        }

        private void SpawnSummonedAlly()
        {
            EnemyDefinition fast = enemyDefinitions.Find(e => e.Archetype == EnemyArchetype.FastWeak);
            if (fast == null)
            {
                return;
            }

            activeEnemies.Add(new EnemyRuntimeState
            {
                RuntimeId = Guid.NewGuid().ToString("N"),
                Definition = fast,
                Health = fast.MaxHealth * 0.7f,
                IsAlerted = true
            });
        }

        private EnemyDefinition PickEnemyForThreat(List<EnemyDefinition> pool, int threatTier, bool leaderSlot)
        {
            if (threatTier >= 10 && leaderSlot)
            {
                EnemyDefinition worldBoss = pool.Find(e => e.Archetype == EnemyArchetype.WorldBoss) ?? enemyDefinitions.Find(e => e.Archetype == EnemyArchetype.WorldBoss);
                if (worldBoss != null)
                {
                    PublishCombatEvent(SimulationEventType.WorldBossAwakened, worldBoss.DisplayName, $"World boss {worldBoss.DisplayName} has entered the region.", worldBossAnnouncementThreat, SimulationEventSeverity.Critical);
                    return worldBoss;
                }
            }

            if (threatTier >= 6 && leaderSlot)
            {
                EnemyDefinition boss = pool.Find(e => e.Archetype == EnemyArchetype.Miniboss || e.Archetype == EnemyArchetype.Elite);
                if (boss != null)
                {
                    return boss;
                }
            }

            List<EnemyDefinition> filtered = pool.FindAll(e => threatTier switch
            {
                <= 2 => e.Archetype is EnemyArchetype.FastWeak or EnemyArchetype.Ranged or EnemyArchetype.Stealth,
                <= 5 => e.Archetype is not EnemyArchetype.WorldBoss,
                _ => true
            });

            if (filtered.Count == 0)
            {
                filtered = pool;
            }

            return filtered[UnityEngine.Random.Range(0, filtered.Count)];
        }

        private static CombatAiDecision Decision(EnemyRuntimeState enemy, EnemyCombatIntent intent, string reason, float threat, float damage)
        {
            return new CombatAiDecision
            {
                EnemyRuntimeId = enemy.RuntimeId,
                Intent = intent,
                Reason = $"{enemy.Definition.DisplayName}: {reason}",
                ThreatScore = threat,
                DamageProjected = damage
            };
        }

        private static float ResolveStatusChance(EnemyArchetype archetype)
        {
            return archetype switch
            {
                EnemyArchetype.FastWeak => 0.12f,
                EnemyArchetype.Tank => 0.18f,
                EnemyArchetype.Ranged => 0.22f,
                EnemyArchetype.Stealth => 0.28f,
                EnemyArchetype.Summoner => 0.24f,
                EnemyArchetype.Elite => 0.35f,
                EnemyArchetype.Miniboss => 0.42f,
                EnemyArchetype.WorldBoss => 0.55f,
                _ => 0.15f
            };
        }

        private static int ResolveStatusDuration(EnemyArchetype archetype)
        {
            return archetype switch
            {
                EnemyArchetype.Elite => 6,
                EnemyArchetype.Miniboss => 8,
                EnemyArchetype.WorldBoss => 12,
                _ => 4
            };
        }

        private static float ResolveEncounterMagnitude(List<EnemyRuntimeState> spawned)
        {
            float total = 0f;
            for (int i = 0; i < spawned.Count; i++)
            {
                if (spawned[i]?.Definition != null)
                {
                    total += spawned[i].Definition.MaxHealth / 25f + (int)spawned[i].Definition.Archetype;
                }
            }

            return total;
        }

        private static string BuildEncounterSummary(List<EnemyRuntimeState> spawned, BiomeType biome)
        {
            List<string> names = new List<string>();
            for (int i = 0; i < spawned.Count; i++)
            {
                if (spawned[i]?.Definition != null)
                {
                    names.Add($"{spawned[i].Definition.DisplayName} ({spawned[i].Definition.Archetype})");
                }
            }

            return $"{biome} encounter: {string.Join(", ", names)}.";
        }

        private void PublishCombatEvent(SimulationEventType type, string key, string reason, float magnitude, SimulationEventSeverity severity)
        {
            (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
            {
                Type = type,
                Severity = severity,
                SystemName = nameof(CombatExpansionSystem),
                ChangeKey = key,
                Reason = reason,
                Magnitude = magnitude
            });
        }

        private void EnsureEnemyDefinitions()
        {
            if (enemyDefinitions.Count > 0)
            {
                return;
            }

            enemyDefinitions.Add(new EnemyDefinition { Id = "enemy_fast_feral", DisplayName = "Feral Skitter", Archetype = EnemyArchetype.FastWeak, MaxHealth = 12f, MoveSpeed = 1.8f, MeleeDamage = 3f, HearingSensitivity = 0.55f, FlankPreference = 0.45f, PackSynergy = 0.45f, StatusEffectIds = new List<string> { "combat_bleed" }, PreferredBiomes = new List<BiomeType> { BiomeType.Forest, BiomeType.Ruins } });
            enemyDefinitions.Add(new EnemyDefinition { Id = "enemy_tank_bulwark", DisplayName = "Bulwark Brute", Archetype = EnemyArchetype.Tank, MaxHealth = 85f, MoveSpeed = 0.65f, MeleeDamage = 9f, Armor = 0.45f, RetreatHealthThreshold = 0.03f, StatusEffectIds = new List<string> { "combat_shock" }, PreferredBiomes = new List<BiomeType> { BiomeType.Ruins, BiomeType.Volcanic } });
            enemyDefinitions.Add(new EnemyDefinition { Id = "enemy_ranged_spitter", DisplayName = "Venom Spitter", Archetype = EnemyArchetype.Ranged, MaxHealth = 28f, MoveSpeed = 1.05f, MeleeDamage = 2f, RangedDamage = 7f, HearingSensitivity = 0.45f, StatusEffectIds = new List<string> { "combat_poison" }, PreferredBiomes = new List<BiomeType> { BiomeType.Swamp, BiomeType.Toxic } });
            enemyDefinitions.Add(new EnemyDefinition { Id = "enemy_stealth_shade", DisplayName = "Shade Stalker", Archetype = EnemyArchetype.Stealth, MaxHealth = 24f, MoveSpeed = 1.35f, MeleeDamage = 6f, StealthRating = 0.75f, HearingSensitivity = 0.7f, FlankPreference = 0.65f, StatusEffectIds = new List<string> { "combat_fear", "combat_bleed" }, PreferredBiomes = new List<BiomeType> { BiomeType.Forest, BiomeType.Ruins, BiomeType.Toxic } });
            enemyDefinitions.Add(new EnemyDefinition { Id = "enemy_summoner_totemist", DisplayName = "Totem Caller", Archetype = EnemyArchetype.Summoner, MaxHealth = 42f, MoveSpeed = 0.9f, MeleeDamage = 3f, RangedDamage = 5f, SummonChance = 0.4f, StatusEffectIds = new List<string> { "combat_fear" }, PreferredBiomes = new List<BiomeType> { BiomeType.Swamp, BiomeType.Tundra } });
            enemyDefinitions.Add(new EnemyDefinition { Id = "enemy_elite_ashguard", DisplayName = "Ashguard Elite", Archetype = EnemyArchetype.Elite, MaxHealth = 95f, MoveSpeed = 1.05f, MeleeDamage = 12f, RangedDamage = 5f, Armor = 0.22f, FlankPreference = 0.4f, PackSynergy = 0.35f, StatusEffectIds = new List<string> { "combat_burn", "combat_shock" }, PreferredBiomes = new List<BiomeType> { BiomeType.Volcanic, BiomeType.Ruins } });
            enemyDefinitions.Add(new EnemyDefinition { Id = "enemy_miniboss_frostmaw", DisplayName = "Frostmaw Matriarch", Archetype = EnemyArchetype.Miniboss, MaxHealth = 220f, MoveSpeed = 0.95f, MeleeDamage = 18f, Armor = 0.28f, PackSynergy = 0.55f, StatusEffectIds = new List<string> { "combat_freeze", "combat_fear" }, PreferredBiomes = new List<BiomeType> { BiomeType.Tundra } });
            enemyDefinitions.Add(new EnemyDefinition { Id = "enemy_worldboss_titan", DisplayName = "Cinder Titan", Archetype = EnemyArchetype.WorldBoss, MaxHealth = 1400f, MoveSpeed = 0.8f, MeleeDamage = 34f, RangedDamage = 22f, Armor = 0.38f, HearingSensitivity = 0.9f, RetreatHealthThreshold = 0f, FlankPreference = 0.15f, PackSynergy = 0.7f, StatusEffectIds = new List<string> { "combat_burn", "combat_shock", "combat_fear" }, PreferredBiomes = new List<BiomeType> { BiomeType.Volcanic } });
        }
    }
}

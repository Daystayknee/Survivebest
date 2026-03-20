using System;
using System.Collections.Generic;
using UnityEngine;
using Survivebest.Events;
using Survivebest.Health;
using Survivebest.Location;
using Survivebest.Needs;
using Survivebest.Status;
using Survivebest.Appearance;
using Survivebest.World;
using Survivebest.Economy;
using Survivebest.Activity;
using Survivebest.Commerce;
using Survivebest.Crime;
using Survivebest.NPC;
using Survivebest.Quest;
using Survivebest.Story;

namespace Survivebest.Core
{
    [Serializable]
    public class SaveSnapshot
    {
        public string WorldName;
        public string DateLabel;
        public string PlaytimeLabel;
        public int HouseholdMembers;
        public string ActiveRoomName;
    }

    [Serializable]
    public class WorldSnapshot
    {
        public int Year;
        public int Month;
        public int Day;
        public int Hour;
        public int Minute;
    }

    [Serializable]
    public class CharacterSnapshot
    {
        public string CharacterId;
        public string DisplayName;
        public bool IsActive;
        public LifeStage LifeStage;
        public float Vitality;
        public GeneticProfile Genetics;
        public PhenotypeProfile Phenotype;
        public NeedsSnapshot Needs;
        public List<SkillEntry> Skills = new();
        public List<ActiveStatusEffect> Statuses = new();
        public ActivitySystem.ActivityRuntimeState ActivityState;
        public RehabilitationSystem.RehabilitationRuntimeState RehabilitationState;
        public bool HasAppearanceCustomization;
        public AppearanceProfile Appearance;
        public HairProfile Hair = new();
        public FacialHairProfile FacialHair = new();
        public BodyHairProfile BodyHair = new();
    }

    [Serializable]
    public class WorldSystemsSnapshot
    {
        public HouseholdControlMode HouseholdControlMode;
        public List<HouseholdAutonomyNote> HouseholdAutonomyNotes = new();
        public List<HouseholdPetProfile> HouseholdPets = new();
        public List<JusticeSystem.ActiveSentenceRecord> ActiveSentences = new();
        public List<InmateRoutineState> InmateStates = new();
        public OrderingSystem.OrderingRuntimeState Ordering;
        public List<AnimalSightingContract> Contracts = new();
        public List<HouseholdChore> HouseholdChores = new();
        public List<NpcProfile> Npcs = new();
        public List<DistrictDefinition> Districts = new();
        public List<LotDefinition> Lots = new();
        public List<RouteEdge> Routes = new();
        public List<LotSimulationState> LotStates = new();
        public List<RemoteNpcSnapshot> RemoteNpcSnapshots = new();
        public AIDirectorDramaManager.DirectorRuntimeState Director;
        public AutonomousStoryGenerator.StoryRuntimeState Story;
        public List<CultureProfile> Cultures = new();
        public List<CulturalIdentityState> CulturalIdentities = new();
        public List<NeighborhoodMicroCultureProfile> NeighborhoodMicroCultures = new();
        public List<LifeDirectionState> LifeDirections = new();
        public List<RegretEntry> Regrets = new();
        public List<MeaningProfile> MeaningProfiles = new();
        public List<LifeStorySnapshot> LifeStories = new();
        public HumanLifeRuntimeState HumanLife;
    }

    [Serializable]
    public class SaveSlotPayload
    {
        public int SchemaVersion = 1;
        public string WorldName;
        public EconomySnapshot Economy;
        public string ActiveRoomName;
        public WorldSnapshot World = new();
        public List<CharacterSnapshot> HouseholdCharacters = new();
        public WorldSystemsSnapshot Systems = new();
    }

    public class SaveGameManager : MonoBehaviour
    {
        private const int CurrentSchemaVersion = 6;
        private const int LegacySchemaVersion = 1;

        [SerializeField] private WorldClock worldClock;
        [SerializeField] private HouseholdManager householdManager;
        [SerializeField] private LocationManager locationManager;
        [SerializeField] private GameEventHub gameEventHub;
        [SerializeField] private EconomyInventorySystem economyInventorySystem;
        [SerializeField] private JusticeSystem justiceSystem;
        [SerializeField] private PrisonRoutineSystem prisonRoutineSystem;
        [SerializeField] private OrderingSystem orderingSystem;
        [SerializeField] private ContractBoardSystem contractBoardSystem;
        [SerializeField] private HouseholdChoreSystem householdChoreSystem;
        [SerializeField] private NpcScheduleSystem npcScheduleSystem;
        [SerializeField] private TownSimulationSystem townSimulationSystem;
        [SerializeField] private WorldPersistenceCullingSystem worldPersistenceCullingSystem;
        [SerializeField] private AIDirectorDramaManager aiDirectorDramaManager;
        [SerializeField] private AutonomousStoryGenerator autonomousStoryGenerator;
        [SerializeField] private WorldCultureSocietyEngine worldCultureSocietyEngine;
        [SerializeField] private PlayerExperienceCascadeSystem playerExperienceCascadeSystem;
        [SerializeField] private HumanLifeExperienceLayerSystem humanLifeExperienceLayerSystem;

        public bool SaveToSlot(int slotIndex, string worldName)
        {
            if (slotIndex < 1 || slotIndex > 3)
            {
                return false;
            }

            string prefix = GetPrefix(slotIndex);
            SaveSnapshot snapshot = BuildSnapshot(worldName);
            SaveSlotPayload payload = BuildPayload(snapshot.WorldName);

            PlayerPrefs.SetInt(prefix + "_HasData", 1);
            PlayerPrefs.SetString(prefix + "_World", snapshot.WorldName);
            PlayerPrefs.SetString(prefix + "_Date", snapshot.DateLabel);
            PlayerPrefs.SetString(prefix + "_Playtime", snapshot.PlaytimeLabel);
            PlayerPrefs.SetInt(prefix + "_Household", snapshot.HouseholdMembers);
            PlayerPrefs.SetString(prefix + "_Room", snapshot.ActiveRoomName);
            PlayerPrefs.SetString(prefix + "_Payload", JsonUtility.ToJson(payload));
            PlayerPrefs.Save();

            PublishSaveEvent(SimulationEventType.SaveCreated, slotIndex, snapshot.WorldName);
            return true;
        }

        public bool LoadFromSlot(int slotIndex)
        {
            if (slotIndex < 1 || slotIndex > 3)
            {
                return false;
            }

            string prefix = GetPrefix(slotIndex);
            if (PlayerPrefs.GetInt(prefix + "_HasData", 0) != 1)
            {
                return false;
            }

            string worldName = PlayerPrefs.GetString(prefix + "_World", "Unknown World");
            string payloadJson = PlayerPrefs.GetString(prefix + "_Payload", string.Empty);
            if (!string.IsNullOrWhiteSpace(payloadJson))
            {
                SaveSlotPayload payload = JsonUtility.FromJson<SaveSlotPayload>(payloadJson);
                SaveSlotPayload migrated = MigratePayloadIfNeeded(payload);
                ValidatePayload(migrated);
                ApplyPayload(migrated);
            }
            else
            {
                string roomName = PlayerPrefs.GetString(prefix + "_Room", string.Empty);
                if (!string.IsNullOrWhiteSpace(roomName))
                {
                    locationManager?.NavigateToRoom(roomName);
                }
            }

            PublishSaveEvent(SimulationEventType.SaveLoaded, slotIndex, worldName);
            return true;
        }

        public void DeleteSlot(int slotIndex)
        {
            if (slotIndex < 1 || slotIndex > 3)
            {
                return;
            }

            string prefix = GetPrefix(slotIndex);
            PlayerPrefs.DeleteKey(prefix + "_HasData");
            PlayerPrefs.DeleteKey(prefix + "_World");
            PlayerPrefs.DeleteKey(prefix + "_Date");
            PlayerPrefs.DeleteKey(prefix + "_Playtime");
            PlayerPrefs.DeleteKey(prefix + "_Household");
            PlayerPrefs.DeleteKey(prefix + "_Room");
            PlayerPrefs.DeleteKey(prefix + "_Payload");
            PlayerPrefs.Save();
        }

        private SaveSnapshot BuildSnapshot(string worldName)
        {
            int householdCount = householdManager != null && householdManager.Members != null
                ? householdManager.Members.Count
                : 0;

            string date = worldClock != null
                ? $"{worldClock.CurrentSeason} Year {worldClock.Year} Day {worldClock.Day} {worldClock.Hour:00}:{worldClock.Minute:00}"
                : "Year 1 Day 1";

            string playtime = worldClock != null
                ? $"Y{worldClock.Year} M{worldClock.Month} D{worldClock.Day} H{worldClock.Hour}"
                : "0h";

            string room = locationManager != null && locationManager.CurrentRoom != null
                ? locationManager.CurrentRoom.RoomName
                : "Home District";

            return new SaveSnapshot
            {
                WorldName = string.IsNullOrWhiteSpace(worldName) ? "Unnamed World" : worldName,
                DateLabel = date,
                PlaytimeLabel = playtime,
                HouseholdMembers = householdCount,
                ActiveRoomName = room
            };
        }

        private SaveSlotPayload BuildPayload(string worldName)
        {
            SaveSlotPayload payload = new SaveSlotPayload
            {
                SchemaVersion = CurrentSchemaVersion,
                WorldName = worldName,
                ActiveRoomName = locationManager != null && locationManager.CurrentRoom != null
                    ? locationManager.CurrentRoom.RoomName
                    : "Home District",
                World = new WorldSnapshot
                {
                    Year = worldClock != null ? worldClock.Year : 1,
                    Month = worldClock != null ? worldClock.Month : 1,
                    Day = worldClock != null ? worldClock.Day : 1,
                    Hour = worldClock != null ? worldClock.Hour : 8,
                    Minute = worldClock != null ? worldClock.Minute : 0
                },
                Economy = economyInventorySystem != null ? economyInventorySystem.CaptureSnapshot() : null
            };

            if (householdManager != null && householdManager.Members != null)
            {
                for (int i = 0; i < householdManager.Members.Count; i++)
                {
                    CharacterCore member = householdManager.Members[i];
                    if (member == null)
                    {
                        continue;
                    }

                    NeedsSystem needs = member.GetComponent<NeedsSystem>();
                    HealthSystem health = member.GetComponent<HealthSystem>();
                    SkillSystem skills = member.GetComponent<SkillSystem>();
                    StatusEffectSystem status = member.GetComponent<StatusEffectSystem>();
                    AppearanceManager appearance = member.GetComponent<AppearanceManager>();
                    GeneticsSystem genetics = member.GetComponent<GeneticsSystem>();

                    payload.HouseholdCharacters.Add(new CharacterSnapshot
                    {
                        CharacterId = member.CharacterId,
                        DisplayName = member.DisplayName,
                        IsActive = householdManager.ActiveCharacter == member,
                        LifeStage = member.CurrentLifeStage,
                        Vitality = health != null ? health.CaptureVitality() : 100f,
                        Genetics = genetics != null ? genetics.Profile : null,
                        Phenotype = genetics != null ? genetics.Phenotype : null,
                        Needs = needs != null ? needs.CaptureSnapshot() : null,
                        Skills = skills != null ? skills.CaptureSnapshot() : new List<SkillEntry>(),
                        Statuses = status != null ? status.CaptureSnapshot() : new List<ActiveStatusEffect>(),
                        ActivityState = member.GetComponent<ActivitySystem>()?.CaptureRuntimeState(),
                        RehabilitationState = member.GetComponent<RehabilitationSystem>()?.CaptureRuntimeState(),
                        HasAppearanceCustomization = appearance != null,
                        Appearance = appearance != null && appearance.CurrentProfile != null ? CloneAppearance(appearance.CurrentProfile) : null,
                        Hair = appearance != null ? CloneHair(appearance.ScalpHairProfile) : new HairProfile(),
                        FacialHair = appearance != null ? CloneFacialHair(appearance.FacialHairProfile) : new FacialHairProfile(),
                        BodyHair = appearance != null ? CloneBodyHair(appearance.BodyHairProfile) : new BodyHairProfile()
                    });
                }
            }

            payload.Systems = BuildSystemsSnapshot();

            return payload;
        }

        private WorldSystemsSnapshot BuildSystemsSnapshot()
        {
            WorldSystemsSnapshot snapshot = new();
            if (householdManager != null)
            {
                snapshot.HouseholdControlMode = householdManager.ControlMode;
                snapshot.HouseholdAutonomyNotes = new List<HouseholdAutonomyNote>(householdManager.AutonomyNotes);
                snapshot.HouseholdPets = new List<HouseholdPetProfile>(householdManager.Pets);
            }

            snapshot.ActiveSentences = justiceSystem != null ? justiceSystem.CaptureRuntimeState() : new List<JusticeSystem.ActiveSentenceRecord>();
            snapshot.InmateStates = prisonRoutineSystem != null ? prisonRoutineSystem.CaptureRuntimeState() : new List<InmateRoutineState>();
            snapshot.Ordering = orderingSystem != null ? orderingSystem.CaptureRuntimeState() : null;
            snapshot.Contracts = contractBoardSystem != null ? contractBoardSystem.CaptureRuntimeState() : new List<AnimalSightingContract>();
            snapshot.HouseholdChores = householdChoreSystem != null ? householdChoreSystem.CaptureRuntimeState() : new List<HouseholdChore>();
            snapshot.Npcs = npcScheduleSystem != null ? npcScheduleSystem.CaptureRuntimeState() : new List<NpcProfile>();
            snapshot.Districts = townSimulationSystem != null ? new List<DistrictDefinition>(townSimulationSystem.Districts) : new List<DistrictDefinition>();
            snapshot.Lots = townSimulationSystem != null ? new List<LotDefinition>(townSimulationSystem.Lots) : new List<LotDefinition>();
            snapshot.Routes = townSimulationSystem != null ? new List<RouteEdge>(townSimulationSystem.RouteGraph) : new List<RouteEdge>();
            snapshot.LotStates = worldPersistenceCullingSystem != null ? new List<LotSimulationState>(worldPersistenceCullingSystem.LotStates) : new List<LotSimulationState>();
            snapshot.RemoteNpcSnapshots = worldPersistenceCullingSystem != null ? new List<RemoteNpcSnapshot>(worldPersistenceCullingSystem.RemoteNpcSnapshots) : new List<RemoteNpcSnapshot>();
            snapshot.Director = aiDirectorDramaManager != null ? aiDirectorDramaManager.CaptureRuntimeState() : null;
            snapshot.Story = autonomousStoryGenerator != null ? autonomousStoryGenerator.CaptureRuntimeState() : null;
            snapshot.Cultures = worldCultureSocietyEngine != null ? new List<CultureProfile>(worldCultureSocietyEngine.Cultures) : new List<CultureProfile>();
            snapshot.CulturalIdentities = worldCultureSocietyEngine != null ? new List<CulturalIdentityState>(worldCultureSocietyEngine.Identities) : new List<CulturalIdentityState>();
            snapshot.NeighborhoodMicroCultures = worldCultureSocietyEngine != null ? new List<NeighborhoodMicroCultureProfile>(worldCultureSocietyEngine.MicroCultures) : new List<NeighborhoodMicroCultureProfile>();
            snapshot.LifeDirections = playerExperienceCascadeSystem != null ? new List<LifeDirectionState>(playerExperienceCascadeSystem.DirectionStates) : new List<LifeDirectionState>();
            snapshot.Regrets = playerExperienceCascadeSystem != null ? new List<RegretEntry>(playerExperienceCascadeSystem.Regrets) : new List<RegretEntry>();
            snapshot.MeaningProfiles = playerExperienceCascadeSystem != null ? new List<MeaningProfile>(playerExperienceCascadeSystem.MeaningProfiles) : new List<MeaningProfile>();
            snapshot.LifeStories = playerExperienceCascadeSystem != null ? new List<LifeStorySnapshot>(playerExperienceCascadeSystem.StorySnapshots) : new List<LifeStorySnapshot>();
            snapshot.HumanLife = humanLifeExperienceLayerSystem != null ? humanLifeExperienceLayerSystem.CaptureRuntimeState() : null;
            return snapshot;
        }

        private void ApplyPayload(SaveSlotPayload payload)
        {
            if (payload == null)
            {
                return;
            }

            if (worldClock != null && payload.World != null)
            {
                worldClock.SetDateTime(payload.World.Year, payload.World.Month, payload.World.Day, payload.World.Hour, payload.World.Minute);
            }

            if (locationManager != null && !string.IsNullOrWhiteSpace(payload.ActiveRoomName))
            {
                locationManager.NavigateToRoom(payload.ActiveRoomName);
            }

            economyInventorySystem?.ApplySnapshot(payload.Economy);

            CharacterCore activeToSet = null;
            if (householdManager != null && householdManager.Members != null && payload.HouseholdCharacters != null)
            {
                for (int i = 0; i < householdManager.Members.Count; i++)
                {
                    CharacterCore member = householdManager.Members[i];
                    if (member == null)
                    {
                        continue;
                    }

                    CharacterSnapshot snapshot = payload.HouseholdCharacters.Find(c => c.CharacterId == member.CharacterId || c.DisplayName == member.DisplayName);
                    if (snapshot == null)
                    {
                        continue;
                    }

                    member.SetLifeStage(snapshot.LifeStage);

                    GeneticsSystem genetics = member.GetComponent<GeneticsSystem>();
                    if (genetics != null && snapshot.Genetics != null)
                    {
                        genetics.OverrideGenetics(snapshot.Genetics, false);
                    }

                    NeedsSystem needs = member.GetComponent<NeedsSystem>();
                    needs?.ApplySnapshot(snapshot.Needs);

                    if (genetics != null)
                    {
                        genetics.ApplyGeneticsToSystems();
                    }

                    AppearanceManager appearance = member.GetComponent<AppearanceManager>();
                    if (appearance != null && snapshot.HasAppearanceCustomization)
                    {
                        if (snapshot.Appearance != null)
                        {
                            appearance.ApplyAppearance(CloneAppearance(snapshot.Appearance));
                        }

                        appearance.SetHairProfile(CloneHair(snapshot.Hair));
                        appearance.SetFacialHairProfile(CloneFacialHair(snapshot.FacialHair));
                        appearance.SetBodyHairProfile(CloneBodyHair(snapshot.BodyHair));
                        member.SyncPortraitDataFromAppearance(appearance);
                    }

                    HealthSystem health = member.GetComponent<HealthSystem>();
                    health?.ApplyVitality(snapshot.Vitality);

                    SkillSystem skills = member.GetComponent<SkillSystem>();
                    skills?.ApplySnapshot(snapshot.Skills);

                    StatusEffectSystem status = member.GetComponent<StatusEffectSystem>();
                    status?.ApplySnapshot(snapshot.Statuses);
                    member.GetComponent<ActivitySystem>()?.ApplyRuntimeState(snapshot.ActivityState);
                    member.GetComponent<RehabilitationSystem>()?.ApplyRuntimeState(snapshot.RehabilitationState);

                    if (snapshot.IsActive)
                    {
                        activeToSet = member;
                    }
                }
            }

            ApplySystemsPayload(payload.Systems);

            if (activeToSet != null)
            {
                householdManager.SetActiveCharacter(activeToSet);
            }
        }

        private void ApplySystemsPayload(WorldSystemsSnapshot systems)
        {
            if (systems == null)
            {
                return;
            }

            householdManager?.ApplyRuntimeState(systems.HouseholdControlMode, systems.HouseholdAutonomyNotes, systems.HouseholdPets);
            justiceSystem?.ApplyRuntimeState(systems.ActiveSentences, householdManager != null ? householdManager.Members : null);
            prisonRoutineSystem?.ApplyRuntimeState(systems.InmateStates);
            orderingSystem?.ApplyRuntimeState(systems.Ordering, householdManager != null ? householdManager.Members : null);
            contractBoardSystem?.ApplyRuntimeState(systems.Contracts);
            householdChoreSystem?.ApplyRuntimeState(systems.HouseholdChores);
            npcScheduleSystem?.ApplyRuntimeState(systems.Npcs);
            townSimulationSystem?.ApplyRuntimeState(systems.Districts, systems.Lots, systems.Routes);
            worldPersistenceCullingSystem?.ApplyRuntimeState(systems.LotStates, systems.RemoteNpcSnapshots);
            aiDirectorDramaManager?.ApplyRuntimeState(systems.Director);
            autonomousStoryGenerator?.ApplyRuntimeState(systems.Story);
            worldCultureSocietyEngine?.ApplyRuntimeState(systems.Cultures, systems.CulturalIdentities, systems.NeighborhoodMicroCultures);
            playerExperienceCascadeSystem?.ApplyRuntimeState(systems.LifeDirections, systems.Regrets, systems.MeaningProfiles, systems.LifeStories);
            humanLifeExperienceLayerSystem?.ApplyRuntimeState(systems.HumanLife);
        }


        private void ValidatePayload(SaveSlotPayload payload)
        {
            if (payload == null)
            {
                return;
            }

            if (payload.World == null)
            {
                payload.World = new WorldSnapshot();
            }

            payload.World.Year = Mathf.Max(1, payload.World.Year);
            payload.World.Month = Mathf.Max(1, payload.World.Month);
            payload.World.Day = Mathf.Max(1, payload.World.Day);
            payload.World.Hour = Mathf.Clamp(payload.World.Hour, 0, 23);
            payload.World.Minute = Mathf.Clamp(payload.World.Minute, 0, 59);

            if (payload.HouseholdCharacters == null)
            {
                payload.HouseholdCharacters = new List<CharacterSnapshot>();
            }

            payload.Systems ??= new WorldSystemsSnapshot();
            payload.Systems.HouseholdAutonomyNotes ??= new List<HouseholdAutonomyNote>();
            payload.Systems.HouseholdPets ??= new List<HouseholdPetProfile>();
            payload.Systems.ActiveSentences ??= new List<JusticeSystem.ActiveSentenceRecord>();
            payload.Systems.InmateStates ??= new List<InmateRoutineState>();
            payload.Systems.Contracts ??= new List<AnimalSightingContract>();
            payload.Systems.HouseholdChores ??= new List<HouseholdChore>();
            payload.Systems.Npcs ??= new List<NpcProfile>();
            payload.Systems.Districts ??= new List<DistrictDefinition>();
            payload.Systems.Lots ??= new List<LotDefinition>();
            payload.Systems.Routes ??= new List<RouteEdge>();
            payload.Systems.LotStates ??= new List<LotSimulationState>();
            payload.Systems.RemoteNpcSnapshots ??= new List<RemoteNpcSnapshot>();
            payload.Systems.Cultures ??= new List<CultureProfile>();
            payload.Systems.CulturalIdentities ??= new List<CulturalIdentityState>();
            payload.Systems.NeighborhoodMicroCultures ??= new List<NeighborhoodMicroCultureProfile>();
            payload.Systems.LifeDirections ??= new List<LifeDirectionState>();
            payload.Systems.Regrets ??= new List<RegretEntry>();
            payload.Systems.MeaningProfiles ??= new List<MeaningProfile>();
            payload.Systems.LifeStories ??= new List<LifeStorySnapshot>();
            payload.Systems.HumanLife ??= new HumanLifeRuntimeState();

            HashSet<string> seenIds = new HashSet<string>();
            for (int i = payload.HouseholdCharacters.Count - 1; i >= 0; i--)
            {
                CharacterSnapshot character = payload.HouseholdCharacters[i];
                if (character == null)
                {
                    payload.HouseholdCharacters.RemoveAt(i);
                    continue;
                }

                string key = !string.IsNullOrWhiteSpace(character.CharacterId) ? character.CharacterId : character.DisplayName;
                if (string.IsNullOrWhiteSpace(key) || !seenIds.Add(key))
                {
                    payload.HouseholdCharacters.RemoveAt(i);
                    continue;
                }

                character.Vitality = Mathf.Clamp(character.Vitality, 0f, 100f);
                character.Genetics ??= new GeneticProfile();
                character.Phenotype ??= new PhenotypeProfile();
                character.Skills ??= new List<SkillEntry>();
                character.Statuses ??= new List<ActiveStatusEffect>();
                character.Needs ??= new NeedsSnapshot();
                character.ActivityState ??= new ActivitySystem.ActivityRuntimeState();
                character.RehabilitationState ??= new RehabilitationSystem.RehabilitationRuntimeState();
                if (character.HasAppearanceCustomization)
                {
                    character.Appearance ??= new AppearanceProfile();
                    character.Hair ??= new HairProfile();
                    character.FacialHair ??= new FacialHairProfile();
                    character.BodyHair ??= new BodyHairProfile();
                }
            }
        }

        private SaveSlotPayload MigratePayloadIfNeeded(SaveSlotPayload payload)
        {
            if (payload == null)
            {
                return null;
            }

            if (payload.SchemaVersion <= 0)
            {
                payload.SchemaVersion = LegacySchemaVersion;
            }

            if (payload.SchemaVersion == CurrentSchemaVersion)
            {
                return payload;
            }

            if (payload.SchemaVersion == LegacySchemaVersion)
            {
                // Legacy payloads did not define schema; all existing data maps 1:1 for now.
                payload.SchemaVersion = 2;
            }

            if (payload.SchemaVersion == 2)
            {
                // v2 payload had no shared economy snapshot. Keep runtime defaults and upgrade marker.
                payload.SchemaVersion = 3;
            }

            if (payload.SchemaVersion == 3)
            {
                payload.SchemaVersion = 4;
            }

            if (payload.SchemaVersion == 4)
            {
                payload.Systems ??= new WorldSystemsSnapshot();
                payload.SchemaVersion = 5;
            }

            if (payload.SchemaVersion == 5)
            {
                payload.Systems ??= new WorldSystemsSnapshot();
                payload.Systems.HumanLife ??= new HumanLifeRuntimeState();
                payload.SchemaVersion = CurrentSchemaVersion;
                return payload;
            }

            Debug.LogWarning($"[SaveGameManager] Unknown save schema version {payload.SchemaVersion}. Attempting best-effort load.", this);
            payload.SchemaVersion = CurrentSchemaVersion;
            return payload;
        }


        private void PublishSaveEvent(SimulationEventType eventType, int slotIndex, string worldName)
        {
            (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
            {
                Type = eventType,
                Severity = SimulationEventSeverity.Info,
                SystemName = nameof(SaveGameManager),
                ChangeKey = $"Slot{slotIndex}",
                Reason = $"{eventType} for {worldName}",
                Magnitude = slotIndex
            });
        }

        private static AppearanceProfile CloneAppearance(AppearanceProfile profile)
        {
            if (profile == null)
            {
                return null;
            }

            return new AppearanceProfile
            {
                HairStyle = profile.HairStyle,
                HairColor = profile.HairColor,
                EyeColor = profile.EyeColor,
                SkinTone = profile.SkinTone,
                SkinIssue = profile.SkinIssue,
                HasBeautyMark = profile.HasBeautyMark,
                FeminineExpression = profile.FeminineExpression,
                MasculineExpression = profile.MasculineExpression,
                AndrogynyExpression = profile.AndrogynyExpression,
                MakeupColor = profile.MakeupColor
            };
        }

        private static HairProfile CloneHair(HairProfile profile)
        {
            if (profile == null)
            {
                return new HairProfile();
            }

            return JsonUtility.FromJson<HairProfile>(JsonUtility.ToJson(profile));
        }

        private static FacialHairProfile CloneFacialHair(FacialHairProfile profile)
        {
            if (profile == null)
            {
                return new FacialHairProfile();
            }

            return JsonUtility.FromJson<FacialHairProfile>(JsonUtility.ToJson(profile));
        }

        private static BodyHairProfile CloneBodyHair(BodyHairProfile profile)
        {
            if (profile == null)
            {
                return new BodyHairProfile();
            }

            return JsonUtility.FromJson<BodyHairProfile>(JsonUtility.ToJson(profile));
        }

        private static string GetPrefix(int slotIndex) => $"SaveSlot{slotIndex}";
    }
}

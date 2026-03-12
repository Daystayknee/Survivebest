using System;
using System.Collections.Generic;
using UnityEngine;
using Survivebest.Events;
using Survivebest.Health;
using Survivebest.Location;
using Survivebest.Needs;
using Survivebest.Status;
using Survivebest.World;
using Survivebest.Economy;

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
    }

    public class SaveGameManager : MonoBehaviour
    {
        private const int CurrentSchemaVersion = 5;
        private const int LegacySchemaVersion = 1;

        [SerializeField] private WorldClock worldClock;
        [SerializeField] private HouseholdManager householdManager;
        [SerializeField] private LocationManager locationManager;
        [SerializeField] private GameEventHub gameEventHub;
        [SerializeField] private EconomyInventorySystem economyInventorySystem;

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

            if (householdManager == null || householdManager.Members == null)
            {
                return payload;
            }

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
                    Statuses = status != null ? status.CaptureSnapshot() : new List<ActiveStatusEffect>()
                });
            }

            return payload;
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

            if (householdManager == null || householdManager.Members == null || payload.HouseholdCharacters == null)
            {
                return;
            }

            CharacterCore activeToSet = null;
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

                HealthSystem health = member.GetComponent<HealthSystem>();
                health?.ApplyVitality(snapshot.Vitality);

                SkillSystem skills = member.GetComponent<SkillSystem>();
                skills?.ApplySnapshot(snapshot.Skills);

                StatusEffectSystem status = member.GetComponent<StatusEffectSystem>();
                status?.ApplySnapshot(snapshot.Statuses);

                if (snapshot.IsActive)
                {
                    activeToSet = member;
                }
            }

            if (activeToSet != null)
            {
                householdManager.SetActiveCharacter(activeToSet);
            }
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

        private static string GetPrefix(int slotIndex) => $"SaveSlot{slotIndex}";
    }
}

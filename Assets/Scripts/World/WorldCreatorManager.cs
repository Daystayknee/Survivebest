using System;
using System.Collections.Generic;
using UnityEngine;
using Survivebest.Events;
using Survivebest.Location;
using Survivebest.Society;

namespace Survivebest.World
{
    [Serializable]
    public class WorldAreaTemplate
    {
        public string AreaName = "Default";
        public LocationTheme Theme = LocationTheme.Residential;
        public float TheftEnforcement = 0.5f;
        public float ViolenceEnforcement = 0.8f;
        public float PoliceFunding = 0.55f;
        public float PrisonReform = 0.45f;
        public float HealthcareCoverage = 0.5f;
    }

    [Serializable]
    public class WorldGenerationSummary
    {
        public string WorldName = "New World";
        public int MasterSeed;
        public string RegionId = "global";
        public string SettlementDensity = "Town";
        public string EconomyFocus = "Balanced";
        public string GovernmentStyle = "Balanced";
        public int TotalAreas;
        public int ResidentialAreas;
        public int CivicAreas;
        public int WorkplaceAreas;
        public int NatureAreas;
        public int StoreAreas;
        public int HospitalAreas;
    }

    public class WorldCreatorManager : MonoBehaviour
    {
        [SerializeField] private LocationManager locationManager;
        [SerializeField] private LawSystem lawSystem;
        [SerializeField] private GameEventHub gameEventHub;
        [SerializeField] private List<WorldAreaTemplate> areaTemplates = new();
        [SerializeField] private bool useSensibleDefaultsOnStart = true;
        [SerializeField] private WorldGenerationSummary lastGeneratedSummary = new();

        public event Action<int> OnWorldGenerated;

        public WorldGenerationSummary LastGeneratedSummary => lastGeneratedSummary;

        private void Start()
        {
            if (useSensibleDefaultsOnStart)
            {
                GenerateWorldWithDefaults();
            }
        }

        public void SetWorldMetadata(string worldName, int masterSeed, string regionId, string settlementDensity, string economyFocus, string governmentStyle)
        {
            lastGeneratedSummary.WorldName = string.IsNullOrWhiteSpace(worldName) ? "New World" : worldName;
            lastGeneratedSummary.MasterSeed = masterSeed;
            lastGeneratedSummary.RegionId = string.IsNullOrWhiteSpace(regionId) ? "global" : regionId;
            lastGeneratedSummary.SettlementDensity = string.IsNullOrWhiteSpace(settlementDensity) ? "Town" : settlementDensity;
            lastGeneratedSummary.EconomyFocus = string.IsNullOrWhiteSpace(economyFocus) ? "Balanced" : economyFocus;
            lastGeneratedSummary.GovernmentStyle = string.IsNullOrWhiteSpace(governmentStyle) ? "Balanced" : governmentStyle;
        }

        public void GenerateWorldWithDefaults()
        {
            if (areaTemplates == null || areaTemplates.Count == 0)
            {
                areaTemplates = BuildSensibleDefaultAreas();
            }

            SetWorldMetadata("Default World", 0, "global", "Town", "Balanced", "Balanced");
            BuildWorldFromTemplates(areaTemplates);
        }

        public void BuildWorldFromTemplates(List<WorldAreaTemplate> templates)
        {
            if (templates == null || templates.Count == 0)
            {
                return;
            }

            List<AreaLawProfile> profiles = new();
            List<Room> rooms = new();
            WorldGenerationSummary summary = new WorldGenerationSummary
            {
                WorldName = lastGeneratedSummary.WorldName,
                MasterSeed = lastGeneratedSummary.MasterSeed,
                RegionId = lastGeneratedSummary.RegionId,
                SettlementDensity = lastGeneratedSummary.SettlementDensity,
                EconomyFocus = lastGeneratedSummary.EconomyFocus,
                GovernmentStyle = lastGeneratedSummary.GovernmentStyle
            };

            for (int i = 0; i < templates.Count; i++)
            {
                WorldAreaTemplate template = templates[i];
                if (template == null || string.IsNullOrWhiteSpace(template.AreaName))
                {
                    continue;
                }

                profiles.Add(new AreaLawProfile
                {
                    AreaName = template.AreaName,
                    TheftEnforcement = Mathf.Clamp01(template.TheftEnforcement),
                    ViolenceEnforcement = Mathf.Clamp01(template.ViolenceEnforcement),
                    PoliceFunding = Mathf.Clamp01(template.PoliceFunding),
                    PrisonReform = Mathf.Clamp01(template.PrisonReform),
                    HealthcareCoverage = Mathf.Clamp01(template.HealthcareCoverage),
                    SubstanceLaws = BuildDefaultSubstanceLawsByTheme(template.Theme)
                });

                rooms.Add(new Room
                {
                    RoomName = template.AreaName,
                    AreaName = template.AreaName,
                    Theme = template.Theme,
                    Background = null,
                    SpawnPoint = null
                });

                CountTheme(summary, template.Theme);

                (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
                {
                    Type = SimulationEventType.WorldAreaGenerated,
                    Severity = SimulationEventSeverity.Info,
                    SystemName = nameof(WorldCreatorManager),
                    ChangeKey = template.AreaName,
                    Reason = $"Generated area {template.AreaName} ({template.Theme})",
                    Magnitude = (template.TheftEnforcement + template.ViolenceEnforcement) * 0.5f * 100f
                });
            }

            summary.TotalAreas = rooms.Count;
            lastGeneratedSummary = summary;

            lawSystem?.SetAreaProfiles(profiles);
            locationManager?.SetRooms(rooms);

            OnWorldGenerated?.Invoke(rooms.Count);
            (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
            {
                Type = SimulationEventType.WorldCreated,
                Severity = SimulationEventSeverity.Info,
                SystemName = nameof(WorldCreatorManager),
                ChangeKey = "WorldGenerated",
                Reason = $"World {summary.WorldName} generated with {rooms.Count} areas in {summary.RegionId}",
                Magnitude = rooms.Count
            });
        }

        public void VoteLaw(string areaName, SubstanceType substanceType, bool stricter)
        {
            lawSystem?.VoteOnSubstanceLaw(areaName, substanceType, stricter);
        }

        private static void CountTheme(WorldGenerationSummary summary, LocationTheme theme)
        {
            switch (theme)
            {
                case LocationTheme.Residential:
                    summary.ResidentialAreas++;
                    break;
                case LocationTheme.Nature:
                    summary.NatureAreas++;
                    break;
                case LocationTheme.StoreInterior:
                    summary.StoreAreas++;
                    break;
                case LocationTheme.Workplace:
                    summary.WorkplaceAreas++;
                    break;
                case LocationTheme.Hospital:
                    summary.HospitalAreas++;
                    break;
                case LocationTheme.Civic:
                    summary.CivicAreas++;
                    break;
            }
        }

        private static List<WorldAreaTemplate> BuildSensibleDefaultAreas()
        {
            return new List<WorldAreaTemplate>
            {
                new WorldAreaTemplate { AreaName = "Home District", Theme = LocationTheme.Residential, TheftEnforcement = 0.45f, ViolenceEnforcement = 0.7f, PoliceFunding = 0.52f, PrisonReform = 0.46f, HealthcareCoverage = 0.54f },
                new WorldAreaTemplate { AreaName = "Forest Trail", Theme = LocationTheme.Nature, TheftEnforcement = 0.2f, ViolenceEnforcement = 0.35f, PoliceFunding = 0.32f, PrisonReform = 0.48f, HealthcareCoverage = 0.34f },
                new WorldAreaTemplate { AreaName = "Downtown Market", Theme = LocationTheme.StoreInterior, TheftEnforcement = 0.65f, ViolenceEnforcement = 0.75f, PoliceFunding = 0.6f, PrisonReform = 0.42f, HealthcareCoverage = 0.5f },
                new WorldAreaTemplate { AreaName = "Office Plaza", Theme = LocationTheme.Workplace, TheftEnforcement = 0.7f, ViolenceEnforcement = 0.85f, PoliceFunding = 0.62f, PrisonReform = 0.4f, HealthcareCoverage = 0.48f },
                new WorldAreaTemplate { AreaName = "General Hospital", Theme = LocationTheme.Hospital, TheftEnforcement = 0.8f, ViolenceEnforcement = 0.95f, PoliceFunding = 0.58f, PrisonReform = 0.5f, HealthcareCoverage = 0.82f },
                new WorldAreaTemplate { AreaName = "City Hall", Theme = LocationTheme.Civic, TheftEnforcement = 0.75f, ViolenceEnforcement = 0.85f, PoliceFunding = 0.64f, PrisonReform = 0.48f, HealthcareCoverage = 0.58f }
            };
        }

        private static List<SubstanceLaw> BuildDefaultSubstanceLawsByTheme(LocationTheme theme)
        {
            return theme switch
            {
                LocationTheme.Nature => new List<SubstanceLaw>
                {
                    new SubstanceLaw { Substance = SubstanceType.Alcohol, Severity = LawSeverity.Legal },
                    new SubstanceLaw { Substance = SubstanceType.Weed, Severity = LawSeverity.Infraction },
                    new SubstanceLaw { Substance = SubstanceType.PrescriptionDrug, Severity = LawSeverity.Legal },
                    new SubstanceLaw { Substance = SubstanceType.HardDrug, Severity = LawSeverity.Misdemeanor }
                },
                LocationTheme.Hospital => new List<SubstanceLaw>
                {
                    new SubstanceLaw { Substance = SubstanceType.Alcohol, Severity = LawSeverity.Infraction },
                    new SubstanceLaw { Substance = SubstanceType.Weed, Severity = LawSeverity.Misdemeanor },
                    new SubstanceLaw { Substance = SubstanceType.PrescriptionDrug, Severity = LawSeverity.Legal },
                    new SubstanceLaw { Substance = SubstanceType.HardDrug, Severity = LawSeverity.Felony }
                },
                _ => new List<SubstanceLaw>
                {
                    new SubstanceLaw { Substance = SubstanceType.Alcohol, Severity = LawSeverity.Legal },
                    new SubstanceLaw { Substance = SubstanceType.Weed, Severity = LawSeverity.Infraction },
                    new SubstanceLaw { Substance = SubstanceType.PrescriptionDrug, Severity = LawSeverity.Legal },
                    new SubstanceLaw { Substance = SubstanceType.HardDrug, Severity = LawSeverity.Felony }
                }
            };
        }
    }
}

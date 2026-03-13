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
    }

    public class WorldCreatorManager : MonoBehaviour
    {
        [SerializeField] private LocationManager locationManager;
        [SerializeField] private LawSystem lawSystem;
        [SerializeField] private GameEventHub gameEventHub;
        [SerializeField] private List<WorldAreaTemplate> areaTemplates = new();
        [SerializeField] private bool useSensibleDefaultsOnStart = true;

        public event Action<int> OnWorldGenerated;

        private void Start()
        {
            if (useSensibleDefaultsOnStart)
            {
                GenerateWorldWithDefaults();
            }
        }

        public void GenerateWorldWithDefaults()
        {
            if (areaTemplates == null || areaTemplates.Count == 0)
            {
                areaTemplates = BuildSensibleDefaultAreas();
            }

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

            lawSystem?.SetAreaProfiles(profiles);
            locationManager?.SetRooms(rooms);

            OnWorldGenerated?.Invoke(rooms.Count);
            (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
            {
                Type = SimulationEventType.WorldCreated,
                Severity = SimulationEventSeverity.Info,
                SystemName = nameof(WorldCreatorManager),
                ChangeKey = "WorldGenerated",
                Reason = $"World generated with {rooms.Count} areas",
                Magnitude = rooms.Count
            });
        }

        public void VoteLaw(string areaName, SubstanceType substanceType, bool stricter)
        {
            lawSystem?.VoteOnSubstanceLaw(areaName, substanceType, stricter);
        }

        private static List<WorldAreaTemplate> BuildSensibleDefaultAreas()
        {
            return new List<WorldAreaTemplate>
            {
                new WorldAreaTemplate { AreaName = "Home District", Theme = LocationTheme.Residential, TheftEnforcement = 0.45f, ViolenceEnforcement = 0.7f },
                new WorldAreaTemplate { AreaName = "Forest Trail", Theme = LocationTheme.Nature, TheftEnforcement = 0.2f, ViolenceEnforcement = 0.35f },
                new WorldAreaTemplate { AreaName = "Downtown Market", Theme = LocationTheme.StoreInterior, TheftEnforcement = 0.65f, ViolenceEnforcement = 0.75f },
                new WorldAreaTemplate { AreaName = "Office Plaza", Theme = LocationTheme.Workplace, TheftEnforcement = 0.7f, ViolenceEnforcement = 0.85f },
                new WorldAreaTemplate { AreaName = "General Hospital", Theme = LocationTheme.Hospital, TheftEnforcement = 0.8f, ViolenceEnforcement = 0.95f },
                new WorldAreaTemplate { AreaName = "City Hall", Theme = LocationTheme.Civic, TheftEnforcement = 0.75f, ViolenceEnforcement = 0.85f }
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

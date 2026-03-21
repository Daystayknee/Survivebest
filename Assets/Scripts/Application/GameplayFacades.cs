using UnityEngine;
using System;
using System.Collections.Generic;
using Survivebest.Core;
using Survivebest.Crime;
using Survivebest.Economy;
using Survivebest.Location;
using Survivebest.Needs;
using Survivebest.Social;
using Survivebest.Status;
using Survivebest.World;

namespace Survivebest.Application
{
    public sealed class CharacterFacade
    {
        private readonly RelationshipMemorySystem relationshipMemorySystem;
        private readonly HumanLifeExperienceLayerSystem humanLifeExperienceLayerSystem;
        private readonly PaperTrailSystem paperTrailSystem;
        private readonly HouseholdManager householdManager;

        public CharacterFacade(RelationshipMemorySystem relationshipMemorySystem = null, HumanLifeExperienceLayerSystem humanLifeExperienceLayerSystem = null, PaperTrailSystem paperTrailSystem = null, HouseholdManager householdManager = null)
        {
            this.relationshipMemorySystem = relationshipMemorySystem;
            this.humanLifeExperienceLayerSystem = humanLifeExperienceLayerSystem;
            this.paperTrailSystem = paperTrailSystem;
            this.householdManager = householdManager;
        }

        public CharacterDashboardViewModel BuildDashboard(CharacterCore character)
        {
            if (character == null)
            {
                return new CharacterDashboardViewModel { Name = "No character", Age = "Unknown", PortraitStateKey = "none" };
            }

            NeedsSystem needs = character.GetComponent<NeedsSystem>();
            NeedsSnapshot snapshot = needs != null ? needs.CaptureSnapshot() : null;
            StatusEffectSystem statuses = character.GetComponent<StatusEffectSystem>();
            PaperTrailProfile paper = paperTrailSystem != null ? paperTrailSystem.GetOrCreateProfile(character.CharacterId) : null;
            string thought = humanLifeExperienceLayerSystem != null
                ? humanLifeExperienceLayerSystem.BuildInnerMonologueSnapshot(character.CharacterId, true)
                : relationshipMemorySystem != null ? relationshipMemorySystem.BuildMemoryInsight(character.CharacterId) : "No active thought.";

            return new CharacterDashboardViewModel
            {
                CharacterId = character.CharacterId,
                Name = character.DisplayName,
                DisplayName = character.DisplayName,
                Age = character.CurrentLifeStage.ToString(),
                LifeStage = character.CurrentLifeStage.ToString(),
                PortraitStateKey = $"{character.GetSpeciesKey()}_{character.CurrentLifeStage.ToString().ToLowerInvariant()}_{character.ClothingStyle.ToString().ToLowerInvariant()}",
                MoodSummary = BuildMoodSummary(snapshot),
                TopNeeds = BuildTopNeeds(snapshot),
                CurrentThought = thought,
                CurrentAction = householdManager != null ? householdManager.GetLatestIntentForCharacter(character.CharacterId) ?? "Idle" : "Idle",
                ActiveMoodTags = BuildMoodTags(snapshot),
                ActiveStatuses = BuildActiveStatuses(statuses),
                RelationshipHighlights = BuildRelationshipHighlights(character.CharacterId),
                KeyRelationshipAlerts = BuildRelationshipHighlights(character.CharacterId),
                VampireSuspicionLevel = character.IsVampire && paper != null ? paper.VampireAnomalyRisk : 0f
            };
        }

        private static string BuildMoodSummary(NeedsSnapshot snapshot)
        {
            if (snapshot == null)
            {
                return "Unknown";
            }

            if (snapshot.Mood < 30f) return "Strained";
            if (snapshot.BurnoutRisk > 65f) return "Burning out";
            if (snapshot.MentalFatigue > 55f) return "Mentally drained";
            if (snapshot.Mood < 55f) return "Uneasy";
            return "Stable";
        }

        private List<string> BuildTopNeeds(NeedsSnapshot snapshot)
        {
            List<string> needs = new();
            if (snapshot == null)
            {
                return needs;
            }

            AddNeedIfLow(needs, "food", snapshot.Hunger, inverted: false);
            AddNeedIfLow(needs, "energy", snapshot.Energy, inverted: false);
            AddNeedIfLow(needs, "hygiene", snapshot.Hygiene, inverted: false);
            AddNeedIfLow(needs, "mood", snapshot.Mood, inverted: false);
            AddNeedIfLow(needs, "hydration", snapshot.Hydration, inverted: false);
            AddNeedIfHigh(needs, "sleep debt", snapshot.SleepDebt);
            AddNeedIfHigh(needs, "burnout", snapshot.BurnoutRisk);
            return needs;
        }

        private static List<string> BuildMoodTags(NeedsSnapshot snapshot)
        {
            List<string> tags = new();
            if (snapshot == null)
            {
                return tags;
            }

            if (snapshot.Mood < 40f) tags.Add("low_mood");
            if (snapshot.BurnoutRisk > 60f) tags.Add("burnout_risk");
            if (snapshot.MentalFatigue > 55f) tags.Add("mentally_fried");
            if (snapshot.SleepDebt > 50f) tags.Add("sleep_deprived");
            if (tags.Count == 0) tags.Add("steady");
            return tags;
        }

        private List<string> BuildRelationshipHighlights(string characterId)
        {
            List<string> lines = new();
            if (relationshipMemorySystem == null || string.IsNullOrWhiteSpace(characterId))
            {
                return lines;
            }

            List<RelationshipMemory> memories = relationshipMemorySystem.GetMemoriesForCharacter(characterId);
            memories.Sort((a, b) => b.Impact.CompareTo(a.Impact));
            for (int i = 0; i < Math.Min(3, memories.Count); i++)
            {
                RelationshipMemory memory = memories[i];
                lines.Add(memory.Topic);
            }

            return lines;
        }

        private static List<string> BuildActiveStatuses(StatusEffectSystem statusEffectSystem)
        {
            List<string> statuses = new();
            IReadOnlyList<ActiveStatusEffect> activeEffects = statusEffectSystem != null ? statusEffectSystem.ActiveEffects : null;
            if (activeEffects == null)
            {
                return statuses;
            }

            for (int i = 0; i < Mathf.Min(3, activeEffects.Count); i++)
            {
                ActiveStatusEffect effect = activeEffects[i];
                if (effect != null && !string.IsNullOrWhiteSpace(effect.DisplayName))
                {
                    statuses.Add(effect.DisplayName);
                }
            }

            return statuses;
        }

        private static void AddNeedIfLow(List<string> list, string label, float value, bool inverted)
        {
            if (value < 45f)
            {
                list.Add($"{label}:{value:0}");
            }
        }

        private static void AddNeedIfHigh(List<string> list, string label, float value)
        {
            if (value > 55f)
            {
                list.Add($"{label}:{value:0}");
            }
        }
    }

    public sealed class HouseholdFacade
    {
        public HouseholdSummaryViewModel BuildSummary(HouseholdManager householdManager, EconomyInventorySystem economyInventorySystem = null)
        {
            HouseholdSummaryViewModel viewModel = new HouseholdSummaryViewModel
            {
                MemberCount = householdManager != null ? householdManager.Members.Count : 0,
                ActiveCharacterName = householdManager != null && householdManager.ActiveCharacter != null ? householdManager.ActiveCharacter.DisplayName : "None",
                Funds = economyInventorySystem != null ? economyInventorySystem.Funds : 0f
            };

            if (householdManager == null || householdManager.Members.Count == 0)
            {
                return viewModel;
            }

            float hunger = 0f;
            float energy = 0f;
            int sampled = 0;
            for (int i = 0; i < householdManager.Members.Count; i++)
            {
                NeedsSystem needs = householdManager.Members[i] != null ? householdManager.Members[i].GetComponent<NeedsSystem>() : null;
                if (needs == null)
                {
                    continue;
                }

                NeedsSnapshot snap = needs.CaptureSnapshot();
                hunger += snap.Hunger;
                energy += snap.Energy;
                sampled++;
                if (snap.BurnoutRisk > 60f)
                {
                    viewModel.PressureHighlights.Add($"{householdManager.Members[i].DisplayName} burnout risk");
                }
            }

            if (sampled > 0)
            {
                viewModel.AverageHunger = hunger / sampled;
                viewModel.AverageEnergy = energy / sampled;
            }

            return viewModel;
        }
    }

    public sealed class EconomyFacade
    {
        public EconomySummaryViewModel BuildSummary(EconomyInventorySystem economyInventorySystem)
        {
            EconomySummaryViewModel vm = new EconomySummaryViewModel();
            if (economyInventorySystem == null)
            {
                return vm;
            }

            vm.Funds = economyInventorySystem.Funds;
            EconomySnapshot snapshot = economyInventorySystem.CaptureSnapshot();
            if (snapshot == null || snapshot.Inventory == null)
            {
                return vm;
            }

            vm.DistinctInventoryEntries = snapshot.Inventory.Count;
            for (int i = 0; i < snapshot.Inventory.Count; i++)
            {
                SharedInventoryEntry pair = snapshot.Inventory[i];
                if (pair == null || pair.Quantity <= 0)
                {
                    continue;
                }

                vm.InventoryHighlights.Add($"{pair.ItemName}:{pair.Quantity}");
                if (vm.InventoryHighlights.Count >= 3)
                {
                    break;
                }
            }

            return vm;
        }
    }

    public sealed class TownFacade
    {
        public DistrictSummaryViewModel BuildDistrictSummary(TownSimulationManager townSimulationManager, string districtId)
        {
            DistrictSummaryViewModel vm = new DistrictSummaryViewModel { DistrictId = districtId };
            if (townSimulationManager == null || string.IsNullOrWhiteSpace(districtId))
            {
                return vm;
            }

            DistrictActivitySnapshot district = null;
            for (int i = 0; i < townSimulationManager.DistrictActivity.Count; i++)
            {
                DistrictActivitySnapshot candidate = townSimulationManager.DistrictActivity[i];
                if (candidate != null && string.Equals(candidate.DistrictId, districtId, StringComparison.OrdinalIgnoreCase))
                {
                    district = candidate;
                    break;
                }
            }

            DistrictDefinition districtDef = null;
            if (district == null)
            {
                return vm;
            }

            vm.Danger = 100f - (districtDef != null ? districtDef.Safety * 100f : 50f);
            vm.Wealth = districtDef != null ? districtDef.Wealth * 100f : 50f;
            vm.ServiceQuality = Mathf.Clamp(district.ActivityScore * 0.8f, 0f, 100f);
            vm.Nightlife = district.ActivityScore;
            vm.OccultTension = townSimulationManager.GetTownPressureScore();
            for (int i = 0; i < townSimulationManager.RecentCommunityEvents.Count; i++)
            {
                CommunityEventRecord evt = townSimulationManager.RecentCommunityEvents[i];
                if (evt != null && string.Equals(evt.DistrictId, districtId, StringComparison.OrdinalIgnoreCase))
                {
                    vm.LiveEvents.Add(evt.Label);
                }
            }

            if (vm.LiveEvents.Count == 0)
            {
                vm.LiveEvents.Add("No live events");
            }

            vm.WeatherEffects.Add(vm.Nightlife >= 60f ? "late-night spillover" : "calm foot traffic");
            return vm;
        }
    }

    public sealed class JusticeFacade
    {
        public JusticeSummaryViewModel BuildSummary(JusticeSystem justiceSystem, CharacterCore character)
        {
            JusticeSummaryViewModel vm = new JusticeSummaryViewModel { CharacterId = character != null ? character.CharacterId : null };
            if (justiceSystem == null || character == null)
            {
                return vm;
            }

            vm.Incarcerated = justiceSystem.IsIncarcerated(character);
            for (int i = 0; i < justiceSystem.ActiveSentences.Count; i++)
            {
                ActiveSentence sentence = justiceSystem.ActiveSentences[i];
                if (sentence == null || sentence.Offender != character)
                {
                    continue;
                }

                vm.LegalStage = sentence.Stage.ToString();
                vm.OutstandingFine = sentence.OutstandingFine;
                vm.RemainingJailHours = sentence.RemainingJailHours;
                vm.ActiveCases.Add(sentence.CrimeType);
            }

            return vm;
        }
    }

    public sealed class RelationshipFacade
    {
        public RelationshipSummaryViewModel BuildSummary(RelationshipMemorySystem relationshipMemorySystem, string characterId)
        {
            RelationshipSummaryViewModel vm = new RelationshipSummaryViewModel { CharacterId = characterId };
            if (relationshipMemorySystem == null || string.IsNullOrWhiteSpace(characterId))
            {
                return vm;
            }

            vm.CurrentThought = relationshipMemorySystem.BuildMemoryInsight(characterId);
            List<RelationshipMemory> memories = relationshipMemorySystem.GetMemoriesForCharacter(characterId);
            memories.Sort((a, b) => b.Importance.CompareTo(a.Importance));
            for (int i = 0; i < Math.Min(3, memories.Count); i++)
            {
                vm.HighlightPairs.Add(memories[i].Topic);
            }

            vm.RumorPressure = memories.Count > 0 ? relationshipMemorySystem.BuildRumorText(memories[0]) : "No major rumor pressure.";
            return vm;
        }
    }

    public sealed class VampireFacade
    {
        public VampireSummaryViewModel BuildSummary(VampireDepthSystem vampireDepthSystem, CharacterCore character)
        {
            VampireSummaryViewModel vm = new VampireSummaryViewModel { CharacterId = character != null ? character.CharacterId : null };
            if (vampireDepthSystem == null || character == null)
            {
                return vm;
            }

            for (int i = 0; i < vampireDepthSystem.FrenzyStates.Count; i++)
            {
                FrenzyState frenzy = vampireDepthSystem.FrenzyStates[i];
                if (frenzy != null && frenzy.CharacterId == character.CharacterId)
                {
                    vm.HungerPressure = frenzy.HungerPressure;
                    vm.SecrecyRisk = frenzy.SocialConsequenceRisk;
                    if (frenzy.FrenzyActive) vm.Alerts.Add("frenzy_active");
                }
            }

            for (int i = 0; i < vampireDepthSystem.PoliticalProfiles.Count; i++)
            {
                VampirePoliticalProfile politics = vampireDepthSystem.PoliticalProfiles[i];
                if (politics != null && politics.CharacterId == character.CharacterId)
                {
                    vm.PoliticalHeat = politics.SecretCouncilAttention;
                }
            }

            for (int i = 0; i < vampireDepthSystem.DaySurvivalProfiles.Count; i++)
            {
                DaySurvivalProfile day = vampireDepthSystem.DaySurvivalProfiles[i];
                if (day != null && day.CharacterId == character.CharacterId)
                {
                    vm.LastDayIncident = day.LastDayIncident;
                    vm.SecrecyRisk = Mathf.Max(vm.SecrecyRisk, day.SunlightLeakRisk);
                }
            }

            if (vm.Alerts.Count == 0)
            {
                vm.Alerts.Add("stable_for_now");
            }

            return vm;
        }
    }

    public sealed class GameplayFeedFacade
    {
        public List<GameplayFeedItemViewModel> BuildFeed(TownSimulationManager townSimulationManager, NarrativeContentIntelligenceSystem intelligenceSystem = null)
        {
            List<GameplayFeedItemViewModel> items = new();
            if (townSimulationManager == null)
            {
                return items;
            }

            for (int i = 0; i < townSimulationManager.RecentCommunityEvents.Count; i++)
            {
                CommunityEventRecord evt = townSimulationManager.RecentCommunityEvents[i];
                if (evt == null)
                {
                    continue;
                }

                items.Add(new GameplayFeedItemViewModel
                {
                    Category = "community_event",
                    Headline = evt.Label,
                    Body = intelligenceSystem != null ? intelligenceSystem.BuildTownHeadline() : evt.Label,
                    Severity = townSimulationManager.GetTownPressureScore()
                });
            }

            return items;
        }
    }

    public sealed class GameplayFacade
    {
        private readonly CharacterFacade characterFacade;
        private readonly HouseholdFacade householdFacade;
        private readonly EconomyFacade economyFacade;
        private readonly JusticeFacade justiceFacade;
        private readonly RelationshipFacade relationshipFacade;
        private readonly VampireFacade vampireFacade;

        public GameplayFacade(
            CharacterFacade characterFacade = null,
            HouseholdFacade householdFacade = null,
            EconomyFacade economyFacade = null,
            JusticeFacade justiceFacade = null,
            RelationshipFacade relationshipFacade = null,
            VampireFacade vampireFacade = null)
        {
            this.characterFacade = characterFacade ?? new CharacterFacade();
            this.householdFacade = householdFacade ?? new HouseholdFacade();
            this.economyFacade = economyFacade ?? new EconomyFacade();
            this.justiceFacade = justiceFacade ?? new JusticeFacade();
            this.relationshipFacade = relationshipFacade ?? new RelationshipFacade();
            this.vampireFacade = vampireFacade ?? new VampireFacade();
        }

        public GameplayOverviewViewModel BuildOverview(
            HouseholdManager householdManager,
            EconomyInventorySystem economyInventorySystem,
            LocationManager locationManager,
            JusticeSystem justiceSystem,
            RelationshipMemorySystem relationshipMemorySystem,
            VampireDepthSystem vampireDepthSystem,
            WorldClock worldClock = null,
            WeatherManager weatherManager = null,
            TownSimulationManager townSimulationManager = null)
        {
            CharacterCore activeCharacter = householdManager != null ? householdManager.ActiveCharacter : null;
            string currentRoom = locationManager != null && locationManager.CurrentRoom != null ? locationManager.CurrentRoom.RoomName : "Unknown";
            GameplayOverviewViewModel viewModel = new GameplayOverviewViewModel
            {
                Character = characterFacade.BuildDashboard(activeCharacter),
                Household = householdFacade.BuildSummary(householdManager, economyInventorySystem),
                Economy = economyFacade.BuildSummary(economyInventorySystem),
                Justice = justiceFacade.BuildSummary(justiceSystem, activeCharacter),
                Relationship = relationshipFacade.BuildSummary(relationshipMemorySystem, activeCharacter != null ? activeCharacter.CharacterId : null),
                Vampire = vampireFacade.BuildSummary(vampireDepthSystem, activeCharacter),
                World = BuildWorldPanel(worldClock, weatherManager, currentRoom, townSimulationManager, economyInventorySystem),
                Actions = BuildActionPanel(activeCharacter, locationManager, justiceSystem),
                CurrentRoom = currentRoom
            };

            viewModel.AvailableActions.AddRange(viewModel.Actions.ContextActions);
            return viewModel;
        }

        public WorldPanelViewModel BuildWorldPanel(WorldClock worldClock, WeatherManager weatherManager, string currentRoom, TownSimulationManager townSimulationManager, EconomyInventorySystem economyInventorySystem)
        {
            WorldPanelViewModel vm = new WorldPanelViewModel
            {
                DateTimeLabel = worldClock != null ? $"Y{worldClock.Year} M{worldClock.Month} D{worldClock.Day} {worldClock.Hour:00}:{worldClock.Minute:00}" : "Unknown time",
                Weather = weatherManager != null ? weatherManager.CurrentWeather.ToString() : "Unknown weather",
                Location = string.IsNullOrWhiteSpace(currentRoom) ? "Unknown" : currentRoom,
                DistrictVibe = ResolveDistrictVibe(townSimulationManager),
                Danger = townSimulationManager != null ? townSimulationManager.GetTownPressureScore() : 0f,
                MoneySummary = economyInventorySystem != null ? $"${economyInventorySystem.Funds:0}" : "$0"
            };

            if (townSimulationManager != null)
            {
                for (int i = 0; i < Mathf.Min(3, townSimulationManager.RecentCommunityEvents.Count); i++)
                {
                    CommunityEventRecord evt = townSimulationManager.RecentCommunityEvents[i];
                    if (evt != null)
                    {
                        vm.NearbyEvents.Add(evt.Label);
                    }
                }
            }

            if (vm.NearbyEvents.Count == 0)
            {
                vm.NearbyEvents.Add("No nearby events");
            }

            return vm;
        }

        public ActionPanelViewModel BuildActionPanel(CharacterCore activeCharacter, LocationManager locationManager, JusticeSystem justiceSystem)
        {
            ActionPanelViewModel vm = new ActionPanelViewModel();
            List<string> contextActions = BuildAvailableActions(locationManager);
            vm.ContextActions.AddRange(contextActions);

            if (contextActions.Count > 0)
            {
                vm.SuggestedActions.Add(contextActions[0]);
            }

            vm.LockedActions.Add("break_in");
            if (justiceSystem != null && activeCharacter != null && justiceSystem.IsIncarcerated(activeCharacter))
            {
                vm.WarningActions.Add("escape_attempt");
            }

            if (activeCharacter != null && activeCharacter.IsVampire)
            {
                vm.VampireOnlyActions.Add("feed_on_target");
                if (activeCharacter.CanCompelTargets())
                {
                    vm.VampireOnlyActions.Add("use_compulsion");
                }
            }

            return vm;
        }

        private static List<string> BuildAvailableActions(LocationManager locationManager)
        {
            List<string> actions = new();
            Room room = locationManager != null ? locationManager.CurrentRoom : null;
            if (room == null)
            {
                actions.Add("wait");
                return actions;
            }

            switch (room.Theme)
            {
                case LocationTheme.Nature:
                    actions.Add("forage");
                    actions.Add("observe");
                    actions.Add("travel");
                    break;
                case LocationTheme.StoreInterior:
                    actions.Add("browse");
                    actions.Add("buy");
                    actions.Add("chat");
                    break;
                case LocationTheme.Workplace:
                    actions.Add("work");
                    actions.Add("network");
                    actions.Add("leave");
                    break;
                case LocationTheme.Hospital:
                    actions.Add("check_in");
                    actions.Add("rest");
                    actions.Add("leave");
                    break;
                default:
                    actions.Add("talk");
                    actions.Add("rest");
                    actions.Add("travel");
                    break;
            }

            return actions;
        }

        private static string ResolveDistrictVibe(TownSimulationManager townSimulationManager)
        {
            if (townSimulationManager == null)
            {
                return "Unknown";
            }

            float pressure = townSimulationManager.GetTownPressureScore();
            if (pressure >= 75f) return "Combustible";
            if (pressure >= 50f) return "Restless";
            if (pressure >= 25f) return "Watchful";
            return "Calm";
        }
    }
}

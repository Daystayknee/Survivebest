using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Survivebest.Crime;
using Survivebest.Location;
using Survivebest.Social;
using Survivebest.Story;

namespace Survivebest.Core
{
    [Serializable]
    public class NarrativeSnippet
    {
        public string CharacterId;
        public string Category;
        public string Headline;
        public string Body;
        public float Intensity;
        public string ContextId;
    }

    public class NarrativeContentIntelligenceSystem : MonoBehaviour
    {
        [SerializeField] private HumanLifeExperienceLayerSystem humanLifeExperienceLayerSystem;
        [SerializeField] private RelationshipMemorySystem relationshipMemorySystem;
        [SerializeField] private RelationshipCompatibilityEngine relationshipCompatibilityEngine;
        [SerializeField] private TownSimulationManager townSimulationManager;
        [SerializeField] private PrisonRoutineSystem prisonRoutineSystem;
        [SerializeField] private VampireDepthSystem vampireDepthSystem;
        [SerializeField] private List<NarrativeSnippet> generatedSnippets = new();

        public IReadOnlyList<NarrativeSnippet> GeneratedSnippets => generatedSnippets;

        public ProceduralLifeMoment GenerateProceduralLifeMoment(string characterId, string placeId, float pressure = 0.5f)
        {
            string place = string.IsNullOrWhiteSpace(placeId) ? "somewhere familiar" : placeId;
            string headline = pressure switch
            {
                >= 0.8f => $"{characterId} barely holds the day together at {place}",
                >= 0.5f => $"{characterId} keeps a fragile routine alive at {place}",
                _ => $"{characterId} finds a small steady moment at {place}"
            };

            ProceduralLifeMoment moment = new()
            {
                MomentId = Guid.NewGuid().ToString("N"),
                CharacterId = characterId,
                Source = nameof(NarrativeContentIntelligenceSystem),
                Headline = headline,
                PlaceId = placeId,
                Intensity = Mathf.Clamp01(pressure)
            };

            generatedSnippets.Add(new NarrativeSnippet
            {
                CharacterId = characterId,
                Category = "life_moment",
                Headline = "Procedural life moment",
                Body = headline,
                Intensity = moment.Intensity,
                ContextId = placeId
            });

            return moment;
        }

        public ThoughtMessage GenerateContextualThought(string characterId, string placeId, string prompt = null)
        {
            string memoryInsight = relationshipMemorySystem != null ? relationshipMemorySystem.BuildMemoryInsight(characterId, placeId) : "Nothing here feels accidental.";
            string body = string.IsNullOrWhiteSpace(prompt)
                ? memoryInsight
                : $"{prompt.Trim()} {memoryInsight}";

            ThoughtMessage thought = new()
            {
                CharacterId = characterId,
                Source = nameof(NarrativeContentIntelligenceSystem),
                Body = body,
                Intensity = Mathf.Clamp01(0.35f + (body.Length / 240f)),
                PlaceId = placeId
            };

            generatedSnippets.Add(new NarrativeSnippet
            {
                CharacterId = characterId,
                Category = "thought",
                Headline = "Contextual thought",
                Body = body,
                Intensity = thought.Intensity,
                ContextId = placeId
            });

            return thought;
        }

        public string BuildJournalSummary(string characterId, int day)
        {
            StringBuilder builder = new();
            builder.Append($"Day {day}: ");
            builder.Append(relationshipMemorySystem != null ? relationshipMemorySystem.BuildJournalSummary(characterId, day) : "No meaningful memories were logged.");

            if (humanLifeExperienceLayerSystem != null)
            {
                builder.Append(" ");
                builder.Append(humanLifeExperienceLayerSystem.FilterWorldExperience(characterId, "The day keeps asking who you are when nobody is clapping."));
            }

            return builder.ToString().Trim();
        }

        public string BuildRumorText(RelationshipMemory memory)
        {
            return relationshipMemorySystem != null
                ? relationshipMemorySystem.BuildRumorText(memory)
                : "People keep repeating a story with more confidence than proof.";
        }

        public string BuildIncidentSummary(StoryIncidentRecord incident)
        {
            if (incident == null)
            {
                return "No incident summary available.";
            }

            string tone = incident.StoryImpact >= 14f ? "serious" : incident.StoryImpact >= 8f ? "noticeable" : "minor";
            return $"{incident.Title} hit {incident.DistrictId} as a {tone} disruption. {incident.Description}".Trim();
        }

        public string BuildCompatibilityBlurb(string characterAId, string characterBId)
        {
            if (relationshipCompatibilityEngine == null)
            {
                return "Compatibility data is unavailable.";
            }

            return relationshipCompatibilityEngine.BuildCompatibilityBlurb(characterAId, characterBId);
        }

        public string BuildHouseholdClimateBlurb(List<string> householdCharacterIds)
        {
            if (householdCharacterIds == null || householdCharacterIds.Count == 0)
            {
                return "The household is quiet but unreadable.";
            }

            float cumulativeTrust = 0f;
            float cumulativeStrain = 0f;
            int samples = 0;

            if (relationshipCompatibilityEngine != null)
            {
                for (int i = 0; i < householdCharacterIds.Count; i++)
                {
                    for (int j = i + 1; j < householdCharacterIds.Count; j++)
                    {
                        RelationshipCompatibilityProfile profile = relationshipCompatibilityEngine.GetOrCreateProfile(householdCharacterIds[i], householdCharacterIds[j]);
                        if (profile == null)
                        {
                            continue;
                        }

                        cumulativeTrust += profile.Trust + profile.Comfort + profile.Loyalty;
                        cumulativeStrain += profile.Resentment + profile.CohabitationFriction + profile.ParentingStrain + profile.Tension;
                        samples++;
                    }
                }
            }

            if (samples == 0)
            {
                return "The household is sharing space more than emotional weather.";
            }

            float warmth = cumulativeTrust / (samples * 3f);
            float strain = cumulativeStrain / (samples * 4f);
            return warmth >= strain
                ? "The household feels held together by practiced care, even if everyone is a little tired."
                : "The household is functional on paper, but the rooms are carrying more friction than comfort.";
        }

        public string BuildTownVibeBlurb()
        {
            if (townSimulationManager == null)
            {
                return "The town vibe is unreadable right now.";
            }

            float pressure = townSimulationManager.GetTownPressureScore();
            List<UI.SimulationOverlayEntry> overlays = townSimulationManager.BuildDistrictOverlayEntries();
            int highlighted = overlays.FindAll(entry => entry != null && entry.Highlighted).Count;

            return pressure switch
            {
                >= 75f => $"Town pressure is running hot; {highlighted} districts are visibly carrying the strain.",
                >= 45f => $"The town feels busy and reactive, with {highlighted} districts pulling most of the attention.",
                _ => $"The town feels mostly steady, with only {highlighted} districts showing notable ripples."
            };
        }

        public string BuildPrisonFlavorText(string characterId)
        {
            InmateRoutineState state = prisonRoutineSystem != null ? prisonRoutineSystem.GetState(characterId) : null;
            if (state == null)
            {
                return "No prison routine flavor is available.";
            }

            return state.CurrentActivity switch
            {
                PrisonRoutineActivity.Lockdown => $"Lockdown turns every small noise into a threat; guard alert sits at {state.GuardAlert:0.00} while contraband risk creeps to {state.ContrabandRisk:0.00}.",
                PrisonRoutineActivity.YardTime => $"Yard time is a temporary breath of oxygen, but reputation still has to be negotiated in public.",
                _ => $"{state.CurrentActivity} runs on routine, surveillance, and the quiet math of survival."
            };
        }

        public string BuildVampireSecrecyAlert(string characterId)
        {
            if (vampireDepthSystem == null)
            {
                return "No secrecy alert available.";
            }

            FrenzyState frenzy = null;
            for (int i = 0; i < vampireDepthSystem.FrenzyStates.Count; i++)
            {
                FrenzyState candidate = vampireDepthSystem.FrenzyStates[i];
                if (candidate != null && string.Equals(candidate.CharacterId, characterId, StringComparison.OrdinalIgnoreCase))
                {
                    frenzy = candidate;
                    break;
                }
            }

            DaySurvivalProfile day = null;
            for (int i = 0; i < vampireDepthSystem.DaySurvivalProfiles.Count; i++)
            {
                DaySurvivalProfile candidate = vampireDepthSystem.DaySurvivalProfiles[i];
                if (candidate != null && string.Equals(candidate.CharacterId, characterId, StringComparison.OrdinalIgnoreCase))
                {
                    day = candidate;
                    break;
                }
            }

            float secrecyRisk = Mathf.Max(frenzy != null ? frenzy.SocialConsequenceRisk : 0f, day != null ? day.SunlightLeakRisk : 0f);
            return secrecyRisk >= 65f
                ? "Masquerade risk is elevated: routine cover stories are thinning and witnesses would remember the wrong details."
                : "Secrecy is holding for now, but the margin depends on disciplined habits and a dark exit route.";
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Survivebest.Location;

namespace Survivebest.NPC
{
    [Serializable]
    public class NpcChatSuggestion
    {
        public string Label;
        public string PreviewText;
        public bool IsTextMessage;
    }

    public class NpcLifeAIGuideSystem : MonoBehaviour
    {
        [SerializeField] private NpcScheduleSystem npcScheduleSystem;
        [SerializeField] private TownSimulationSystem townSimulationSystem;
        [SerializeField] private WorldClock worldClock;

        public string LatestGuidance { get; private set; }

        private readonly StringBuilder builder = new();

        public string BuildGuidance(Room room)
        {
            NpcProfile npc = FindPrimaryNpcForRoom(room);
            if (npc == null)
            {
                LatestGuidance = "NPC AI: No one nearby stands out enough to profile right now.";
                return LatestGuidance;
            }

            builder.Clear();
            builder.Append("NPC AI: ");
            builder.Append($"{npc.DisplayName} is a {npc.Job} currently {npc.CurrentState}. ");
            builder.Append(npc.Stress >= 65f
                ? "Stress is elevated, so open gently and avoid confrontational topics. "
                : "They seem stable enough for a direct conversation. ");
            builder.Append(npc.Reputation < 0
                ? "Trust is shaky; lead with repair or practical help. "
                : "They are open to rapport-building or useful small talk. ");
            builder.Append($"Memory load: {npc.Memory.Count} recent threads. Best opener: {BuildPrimaryOpener(npc)}.");

            LatestGuidance = builder.ToString().Trim();
            return LatestGuidance;
        }

        public List<NpcChatSuggestion> BuildChatSuggestions(Room room, bool isTextMessage)
        {
            List<NpcChatSuggestion> suggestions = new();
            NpcProfile npc = FindPrimaryNpcForRoom(room);
            if (npc == null)
            {
                return suggestions;
            }

            suggestions.Add(new NpcChatSuggestion
            {
                Label = isTextMessage ? "Check In" : "Warm Greeting",
                PreviewText = isTextMessage
                    ? $"Hey {npc.DisplayName}, just checking in — how's your day going?"
                    : $"Open with a warm read on {npc.DisplayName}'s day before asking for anything.",
                IsTextMessage = isTextMessage
            });

            suggestions.Add(new NpcChatSuggestion
            {
                Label = npc.Job == NpcJobType.Unemployed ? "Ask About Plans" : "Ask About Work",
                PreviewText = isTextMessage
                    ? BuildTextWorkPrompt(npc)
                    : BuildSpokenWorkPrompt(npc),
                IsTextMessage = isTextMessage
            });

            if (npc.Stress >= 65f)
            {
                suggestions.Add(new NpcChatSuggestion
                {
                    Label = isTextMessage ? "Offer Support" : "Calm Them Down",
                    PreviewText = isTextMessage
                        ? $"You seem under pressure. Want backup, supplies, or just a breather later?"
                        : $"Acknowledge their pressure and offer concrete support instead of advice.",
                    IsTextMessage = isTextMessage
                });
            }
            else
            {
                suggestions.Add(new NpcChatSuggestion
                {
                    Label = isTextMessage ? "Invite Out" : "Build Rapport",
                    PreviewText = isTextMessage
                        ? $"When you're free, want to meet up after {npc.CurrentState.ToString().ToLowerInvariant()} wraps up?"
                        : $"Use a lighter social angle — ask about plans, hobbies, or local gossip.",
                    IsTextMessage = isTextMessage
                });
            }

            if (npc.Memory.Count > 0)
            {
                NpcMemoryEntry recent = npc.Memory[npc.Memory.Count - 1];
                suggestions.Add(new NpcChatSuggestion
                {
                    Label = isTextMessage ? "Follow Up" : "Reference Memory",
                    PreviewText = isTextMessage
                        ? $"Still thinking about {recent.Topic}. You okay after that?"
                        : $"Reference {recent.Topic} to show continuity and emotional memory.",
                    IsTextMessage = isTextMessage
                });
            }

            return suggestions;
        }

        public string BuildInteractionSummary(Room room, bool isTextMessage)
        {
            NpcProfile npc = FindPrimaryNpcForRoom(room);
            if (npc == null)
            {
                return isTextMessage ? "No one is available to text right now." : "No one nearby is ready for a meaningful chat.";
            }

            return isTextMessage
                ? $"You sent {npc.DisplayName} a thoughtful message tuned to their {npc.CurrentState.ToString().ToLowerInvariant()} routine."
                : $"You steered the conversation with {npc.DisplayName} around their {npc.Job.ToString().ToLowerInvariant()} life and current mood.";
        }

        private string BuildPrimaryOpener(NpcProfile npc)
        {
            if (npc.Stress >= 65f)
            {
                return "offer help and keep it low-pressure";
            }

            return npc.Job switch
            {
                NpcJobType.Medic => "ask about the shift and who needs help most",
                NpcJobType.Shopkeeper => "ask what the town is buying lately",
                NpcJobType.Teacher => "ask what people are learning around town",
                NpcJobType.Guard => "ask what feels unsafe lately",
                _ => "start with their routine and current plans"
            };
        }

        private static string BuildTextWorkPrompt(NpcProfile npc)
        {
            return npc.Job == NpcJobType.Unemployed
                ? $"Got any plans later, or are you keeping the day flexible?"
                : $"How's {npc.Job.ToString().ToLowerInvariant()} life treating you today?";
        }

        private static string BuildSpokenWorkPrompt(NpcProfile npc)
        {
            return npc.Job == NpcJobType.Unemployed
                ? "Ask what they are trying to build or recover next in life."
                : $"Ask how {npc.Job.ToString().ToLowerInvariant()} work is shaping their mood and schedule.";
        }

        private NpcProfile FindPrimaryNpcForRoom(Room room)
        {
            if (room == null || npcScheduleSystem == null || npcScheduleSystem.NpcProfiles == null)
            {
                return null;
            }

            string roomLotId = ResolveLotId(room);
            int currentHour = worldClock != null ? worldClock.Hour : DateTime.Now.Hour;
            NpcProfile best = null;
            float bestScore = float.NegativeInfinity;
            IReadOnlyList<NpcProfile> profiles = npcScheduleSystem.NpcProfiles;
            for (int i = 0; i < profiles.Count; i++)
            {
                NpcProfile npc = profiles[i];
                if (npc == null || npc.IsDead)
                {
                    continue;
                }

                bool matchesRoom = MatchesRoom(npc, room, roomLotId);
                if (!matchesRoom)
                {
                    continue;
                }

                float score = npc.Reputation * 0.01f + npc.Memory.Count * 0.1f - npc.Stress * 0.01f;
                if (npc.CurrentState == NpcActivityState.Socializing) score += 0.5f;
                if (npc.CurrentState == NpcActivityState.Working) score += 0.25f;
                if (npc.CurrentState == NpcActivityState.Sleeping) score -= 0.4f;
                if (!string.IsNullOrWhiteSpace(roomLotId) && townSimulationSystem != null && !string.IsNullOrWhiteSpace(npc.CurrentLotId))
                {
                    float routeCost = townSimulationSystem.GetRouteCost(roomLotId, npc.CurrentLotId);
                    if (!float.IsPositiveInfinity(routeCost)) score -= routeCost * 0.1f;
                }

                if (npc.CurrentState == NpcActivityState.Working && currentHour >= 8 && currentHour <= 18)
                {
                    score += 0.1f;
                }

                if (score > bestScore)
                {
                    best = npc;
                    bestScore = score;
                }
            }

            return best;
        }

        private string ResolveLotId(Room room)
        {
            if (room == null || townSimulationSystem == null || townSimulationSystem.Lots == null)
            {
                return room != null ? room.RoomName : null;
            }

            for (int i = 0; i < townSimulationSystem.Lots.Count; i++)
            {
                LotDefinition lot = townSimulationSystem.Lots[i];
                if (lot != null && string.Equals(lot.DisplayName, room.RoomName, StringComparison.OrdinalIgnoreCase))
                {
                    return lot.LotId;
                }
            }

            return room.RoomName;
        }

        private static bool MatchesRoom(NpcProfile npc, Room room, string roomLotId)
        {
            return string.Equals(npc.CurrentLotId, roomLotId, StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(npc.CurrentLotId, room.RoomName, StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(npc.WorkLotId, roomLotId, StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(npc.HomeLotId, roomLotId, StringComparison.OrdinalIgnoreCase);
        }
    }
}

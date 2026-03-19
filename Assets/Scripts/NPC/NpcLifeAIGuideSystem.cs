using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Survivebest.Location;
using Survivebest.Social;
using Survivebest.Core;

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
        [SerializeField] private SocialDramaEngine socialDramaEngine;
        [SerializeField] private LongTermProgressionSystem longTermProgressionSystem;

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
            builder.Append($"Memory load: {npc.Memory.Count} recent threads. ");
            AppendMemoryPressure(npc);
            AppendRumorAndSecretPressure(npc);
            AppendLegacyPressure(npc);
            builder.Append($"Best opener: {BuildPrimaryOpener(npc)}.");

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
                    ? BuildGreetingPreview(npc)
                    : BuildGreetingPreview(npc),
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

            NpcMemoryEntry emotionalAnchor = GetMostImportantMemory(npc, includeSecrets: false);
            if (emotionalAnchor != null && emotionalAnchor.IsGrudge)
            {
                suggestions.Add(new NpcChatSuggestion
                {
                    Label = isTextMessage ? "Repair the Rift" : "Address the Grudge",
                    PreviewText = isTextMessage
                        ? $"I know {emotionalAnchor.Topic} still sits badly with you. I want to make that right."
                        : $"Name the grudge around {emotionalAnchor.Topic} directly and offer repair instead of excuses.",
                    IsTextMessage = isTextMessage
                });
            }
            else if (npc.Stress >= 65f)
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
                NpcMemoryEntry recent = GetMostImportantMemory(npc, includeSecrets: false) ?? npc.Memory[npc.Memory.Count - 1];
                suggestions.Add(new NpcChatSuggestion
                {
                    Label = isTextMessage ? "Follow Up" : "Reference Memory",
                    PreviewText = isTextMessage
                        ? $"Still thinking about {recent.Topic}. You okay after that?"
                        : $"Reference {recent.Topic} to show continuity and emotional memory.",
                    IsTextMessage = isTextMessage
                });
            }

            NpcMemoryEntry rumor = FindMemory(npc, memory => memory.IsRumor && memory.Confidence >= 0.35f);
            if (rumor != null)
            {
                suggestions.Add(new NpcChatSuggestion
                {
                    Label = isTextMessage ? "Ask About Rumor" : "Test the Rumor",
                    PreviewText = isTextMessage
                        ? $"People keep mentioning {rumor.Topic}. Is there any truth to it, or is the town exaggerating again?"
                        : $"Bring up the rumor about {rumor.Topic} carefully and give them room to confirm or deny it.",
                    IsTextMessage = isTextMessage
                });
            }

            NpcMemoryEntry legacy = FindMemory(npc, memory => memory.IsLegacyThread);
            if (legacy != null)
            {
                suggestions.Add(new NpcChatSuggestion
                {
                    Label = isTextMessage ? "Ask About Legacy" : "Honor Their History",
                    PreviewText = isTextMessage
                        ? $"You still carry {legacy.Topic} with you. How is that shaping what you want next?"
                        : $"Invite them to talk about how {legacy.Topic} still defines their choices and reputation.",
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

            NpcMemoryEntry strongest = GetMostImportantMemory(npc, includeSecrets: false);
            string thread = strongest != null ? strongest.Topic : $"{npc.Job.ToString().ToLowerInvariant()} life";
            return isTextMessage
                ? $"You sent {npc.DisplayName} a thoughtful message tuned to their {npc.CurrentState.ToString().ToLowerInvariant()} routine and their ongoing thread around {thread}."
                : $"You steered the conversation with {npc.DisplayName} around {thread}, their {npc.Job.ToString().ToLowerInvariant()} life, and their current mood.";
        }

        private string BuildPrimaryOpener(NpcProfile npc)
        {
            NpcMemoryEntry strongest = GetMostImportantMemory(npc, includeSecrets: false);
            if (strongest != null)
            {
                if (strongest.IsFirstImpression && strongest.Sentiment < 0)
                {
                    return $"repair the first impression around {strongest.Topic}";
                }

                if (strongest.IsGrudge)
                {
                    return $"acknowledge the grudge about {strongest.Topic} and offer restitution";
                }

                if (strongest.IsLegacyThread)
                {
                    return $"ask how {strongest.Topic} still shapes their identity and choices";
                }

                if (strongest.IsRumor)
                {
                    return $"carefully ask whether the rumor about {strongest.Topic} is true";
                }
            }

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
            NpcMemoryEntry strongest = GetMostImportantMemory(npc, includeSecrets: false);
            if (strongest != null && strongest.IsLegacyThread)
            {
                return $"Does {strongest.Topic} still affect how you see your future?";
            }

            return npc.Job == NpcJobType.Unemployed
                ? $"Got any plans later, or are you keeping the day flexible?"
                : $"How's {npc.Job.ToString().ToLowerInvariant()} life treating you today?";
        }

        private static string BuildSpokenWorkPrompt(NpcProfile npc)
        {
            NpcMemoryEntry strongest = GetMostImportantMemory(npc, includeSecrets: false);
            if (strongest != null && strongest.IsGrudge)
            {
                return $"Ask how the fallout from {strongest.Topic} is still affecting their work and trust.";
            }

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

                float score = npc.Reputation * 0.01f + npc.Memory.Count * 0.1f - npc.Stress * 0.01f + GetMemoryWeight(npc);
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

        private void AppendMemoryPressure(NpcProfile npc)
        {
            NpcMemoryEntry firstImpression = FindMemory(npc, memory => memory.IsFirstImpression);
            NpcMemoryEntry grudge = FindMemory(npc, memory => memory.IsGrudge);
            if (firstImpression != null)
            {
                builder.Append(firstImpression.Sentiment >= 0
                    ? $"Their first impression around {firstImpression.Topic} still opens doors. "
                    : $"Their first impression around {firstImpression.Topic} still colors trust. ");
            }

            if (grudge != null)
            {
                builder.Append($"There is an active grudge tied to {grudge.Topic}, so disrespect will compound quickly. ");
            }
        }

        private void AppendRumorAndSecretPressure(NpcProfile npc)
        {
            NpcMemoryEntry rumor = FindMemory(npc, memory => memory.IsRumor && memory.Confidence >= 0.35f);
            if (rumor != null)
            {
                builder.Append($"Town rumor pressure is live around {rumor.Topic}. ");
            }

            int secretCount = CountMemories(npc, memory => memory.IsSecret || memory.Sensitivity == NpcKnowledgeSensitivity.Secret);
            if (secretCount > 0)
            {
                builder.Append(secretCount > 1
                    ? "They are guarding multiple secrets, so pushy questions may shut them down. "
                    : "They are guarding a secret, so confidentiality matters. ");
            }

            if (socialDramaEngine != null)
            {
                IReadOnlyList<RumorPacket> rumors = socialDramaEngine.Rumors;
                for (int i = 0; i < rumors.Count; i++)
                {
                    RumorPacket packet = rumors[i];
                    if (packet == null || !string.Equals(packet.SubjectCharacterId, npc.NpcId, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    builder.Append($"Current gossip intensity is {Mathf.RoundToInt(packet.SpreadPower * 100f)}% around {packet.Content}. ");
                    break;
                }
            }
        }

        private void AppendLegacyPressure(NpcProfile npc)
        {
            NpcMemoryEntry legacy = FindMemory(npc, memory => memory.IsLegacyThread);
            if (legacy != null)
            {
                builder.Append($"Legacy weight: {legacy.Topic} still shapes how the town reads them. ");
            }

            if (longTermProgressionSystem != null)
            {
                LegacyProfile legacyProfile = longTermProgressionSystem.Legacy;
                if (legacyProfile != null && (legacyProfile.Fame > 0 || legacyProfile.Infamy > 0 || legacyProfile.HousePrestige > 0))
                {
                    builder.Append($"The wider town is primed for legacy narratives right now (Fame {legacyProfile.Fame}, Infamy {legacyProfile.Infamy}, Prestige {legacyProfile.HousePrestige}). ");
                }
            }
        }

        private static string BuildGreetingPreview(NpcProfile npc)
        {
            NpcMemoryEntry firstImpression = FindMemory(npc, memory => memory.IsFirstImpression);
            if (firstImpression != null && firstImpression.Sentiment < 0)
            {
                return $"Acknowledge that {firstImpression.Topic} landed badly and reset the tone before asking for anything.";
            }

            return $"Open with a warm read on {npc.DisplayName}'s day before asking for anything.";
        }

        private static float GetMemoryWeight(NpcProfile npc)
        {
            if (npc?.Memory == null || npc.Memory.Count == 0)
            {
                return 0f;
            }

            float total = 0f;
            for (int i = 0; i < npc.Memory.Count; i++)
            {
                NpcMemoryEntry memory = npc.Memory[i];
                if (memory == null)
                {
                    continue;
                }

                total += memory.Importance * 0.2f;
                if (memory.IsFirstImpression) total += 0.12f;
                if (memory.IsGrudge) total += 0.2f;
                if (memory.IsRumor) total += 0.08f;
                if (memory.IsSecret) total += 0.12f;
                if (memory.IsLegacyThread) total += 0.14f;
            }

            return total;
        }

        private static int CountMemories(NpcProfile npc, Func<NpcMemoryEntry, bool> predicate)
        {
            if (npc?.Memory == null)
            {
                return 0;
            }

            int count = 0;
            for (int i = 0; i < npc.Memory.Count; i++)
            {
                NpcMemoryEntry memory = npc.Memory[i];
                if (memory != null && predicate(memory))
                {
                    count++;
                }
            }

            return count;
        }

        private static NpcMemoryEntry GetMostImportantMemory(NpcProfile npc, bool includeSecrets)
        {
            return FindMemory(npc, memory => includeSecrets || !(memory.IsSecret || memory.Sensitivity == NpcKnowledgeSensitivity.Secret));
        }

        private static NpcMemoryEntry FindMemory(NpcProfile npc, Func<NpcMemoryEntry, bool> predicate)
        {
            if (npc?.Memory == null || npc.Memory.Count == 0)
            {
                return null;
            }

            return npc.Memory
                .Where(memory => memory != null && (predicate == null || predicate(memory)))
                .OrderByDescending(ComputeMemorySignal)
                .FirstOrDefault();
        }

        private static float ComputeMemorySignal(NpcMemoryEntry memory)
        {
            float signal = memory.Importance * 2f + Mathf.Abs(memory.Sentiment) / 100f + memory.Confidence;
            if (memory.IsFirstImpression) signal += 0.35f;
            if (memory.IsGrudge) signal += 0.55f;
            if (memory.IsRumor) signal += 0.2f;
            if (memory.IsSecret) signal += 0.45f;
            if (memory.IsLegacyThread) signal += 0.4f;
            return signal;
        }
    }
}

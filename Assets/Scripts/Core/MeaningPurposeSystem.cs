using System;
using System.Collections.Generic;
using UnityEngine;
using Survivebest.Needs;

namespace Survivebest.Core
{
    [Serializable]
    public class MeaningState
    {
        public string CharacterId;
        [Range(0f, 1f)] public float Fulfillment = 0.5f;
        [Range(0f, 1f)] public float Emptiness = 0.2f;
        [Range(0f, 1f)] public float Burnout = 0.1f;
        [Range(0f, 1f)] public float ValueAlignment = 0.5f;
        [Range(0f, 1f)] public float Belonging = 0.5f;
    }

    [Serializable]
    public class LifeVector
    {
        public string CharacterId;
        public List<string> Values = new();
        public List<string> ActiveGoals = new();
        public List<string> EnactedBehaviors = new();
    }

    public class MeaningPurposeSystem : MonoBehaviour
    {
        [SerializeField] private NeedsSystem needsSystem;
        [SerializeField] private LongTermProgressionSystem longTermProgressionSystem;
        [SerializeField] private LifestyleBehaviorSystem lifestyleBehaviorSystem;
        [SerializeField] private List<MeaningState> meaningStates = new();
        [SerializeField] private List<LifeVector> lifeVectors = new();

        public IReadOnlyList<MeaningState> MeaningStates => meaningStates;
        public IReadOnlyList<LifeVector> LifeVectors => lifeVectors;

        public MeaningState GetOrCreateMeaningState(string characterId)
        {
            if (string.IsNullOrWhiteSpace(characterId))
            {
                return null;
            }

            MeaningState state = meaningStates.Find(x => x != null && x.CharacterId == characterId);
            if (state != null)
            {
                return state;
            }

            state = new MeaningState { CharacterId = characterId };
            meaningStates.Add(state);
            return state;
        }

        public LifeVector GetOrCreateLifeVector(string characterId)
        {
            if (string.IsNullOrWhiteSpace(characterId))
            {
                return null;
            }

            LifeVector vector = lifeVectors.Find(x => x != null && x.CharacterId == characterId);
            if (vector != null)
            {
                return vector;
            }

            vector = new LifeVector { CharacterId = characterId };
            lifeVectors.Add(vector);
            return vector;
        }

        public void EvaluateMeaning(string characterId)
        {
            MeaningState meaning = GetOrCreateMeaningState(characterId);
            LifeVector vector = GetOrCreateLifeVector(characterId);
            if (meaning == null || vector == null)
            {
                return;
            }

            float alignment = ComputeAlignment(vector.Values, vector.EnactedBehaviors);
            meaning.ValueAlignment = alignment;
            meaning.Fulfillment = Mathf.Clamp01((meaning.Fulfillment * 0.65f) + (alignment * 0.35f));
            meaning.Emptiness = Mathf.Clamp01((meaning.Emptiness * 0.65f) + ((1f - alignment) * 0.35f));

            float needsBurnout = needsSystem != null ? needsSystem.BurnoutRiskValue / 100f : 0f;
            meaning.Burnout = Mathf.Clamp01((meaning.Burnout * 0.7f) + (needsBurnout * 0.3f));

            if (longTermProgressionSystem != null && longTermProgressionSystem.Legacy != null)
            {
                float successSignal = Mathf.Clamp01((longTermProgressionSystem.Legacy.Fame + longTermProgressionSystem.Legacy.HousePrestige) / 400f);
                meaning.Emptiness = Mathf.Clamp01(meaning.Emptiness + Mathf.Max(0f, successSignal - alignment) * 0.15f);
            }

            if (lifestyleBehaviorSystem != null)
            {
                meaning.Belonging = Mathf.Clamp01(0.3f + (lifestyleBehaviorSystem.GetIdentityStrength("community") / 100f) * 0.7f);
            }
        }

        private static float ComputeAlignment(List<string> values, List<string> behaviors)
        {
            if (values == null || values.Count == 0 || behaviors == null || behaviors.Count == 0)
            {
                return 0.35f;
            }

            int matches = 0;
            for (int i = 0; i < values.Count; i++)
            {
                if (behaviors.Exists(b => string.Equals(b, values[i], StringComparison.OrdinalIgnoreCase)))
                {
                    matches++;
                }
            }

            return Mathf.Clamp01((float)matches / Mathf.Max(1, values.Count));
        }
    }
}

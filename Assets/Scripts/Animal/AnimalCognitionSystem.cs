using System;
using System.Collections.Generic;
using UnityEngine;

namespace Survivebest.Animal
{
    [Serializable]
    public class AnimalPerception
    {
        public string AnimalId;
        [Range(0f, 1f)] public float SmellMapSensitivity = 0.6f;
        [Range(0.5f, 3f)] public float HearingRadiusBias = 1f;
        [Range(0f, 1f)] public float ThreatDetection = 0.5f;
    }

    [Serializable]
    public class BondTrust
    {
        public string HumanId;
        [Range(0f, 1f)] public float Trust = 0.4f;
        [Range(0f, 1f)] public float Loyalty = 0.4f;
    }

    [Serializable]
    public class BondState
    {
        public string AnimalId;
        public List<BondTrust> TrustByHumanId = new();
        public List<string> FearTriggers = new();
        public List<string> SafePlaces = new();
    }

    [Serializable]
    public class InstinctStack
    {
        public string AnimalId;
        [Range(0f, 1f)] public float Hunger = 0.5f;
        [Range(0f, 1f)] public float Territory = 0.4f;
        [Range(0f, 1f)] public float Pack = 0.4f;
        [Range(0f, 1f)] public float Avoidance = 0.2f;
    }

    public class AnimalCognitionSystem : MonoBehaviour
    {
        [SerializeField] private List<AnimalPerception> perceptions = new();
        [SerializeField] private List<BondState> bonds = new();
        [SerializeField] private List<InstinctStack> instincts = new();

        public AnimalPerception GetOrCreatePerception(string animalId)
        {
            AnimalPerception value = perceptions.Find(x => x != null && x.AnimalId == animalId);
            if (value != null)
            {
                return value;
            }

            value = new AnimalPerception { AnimalId = animalId };
            perceptions.Add(value);
            return value;
        }

        public BondState GetOrCreateBondState(string animalId)
        {
            BondState value = bonds.Find(x => x != null && x.AnimalId == animalId);
            if (value != null)
            {
                return value;
            }

            value = new BondState { AnimalId = animalId };
            bonds.Add(value);
            return value;
        }

        public InstinctStack GetOrCreateInstinctStack(string animalId)
        {
            InstinctStack value = instincts.Find(x => x != null && x.AnimalId == animalId);
            if (value != null)
            {
                return value;
            }

            value = new InstinctStack { AnimalId = animalId };
            instincts.Add(value);
            return value;
        }

        public void RegisterHumanInteraction(string animalId, string humanId, float trustDelta, bool feltSafe, string locationId)
        {
            BondState bond = GetOrCreateBondState(animalId);
            BondTrust trust = bond.TrustByHumanId.Find(x => x != null && x.HumanId == humanId);
            if (trust == null)
            {
                trust = new BondTrust { HumanId = humanId };
                bond.TrustByHumanId.Add(trust);
            }

            trust.Trust = Mathf.Clamp01(trust.Trust + trustDelta);
            trust.Loyalty = Mathf.Clamp01(trust.Loyalty + (feltSafe ? 0.05f : -0.04f));

            if (feltSafe && !string.IsNullOrWhiteSpace(locationId) && !bond.SafePlaces.Contains(locationId))
            {
                bond.SafePlaces.Add(locationId);
            }
        }
    }
}

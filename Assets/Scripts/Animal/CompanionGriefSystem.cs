using System;
using System.Collections.Generic;
using UnityEngine;

namespace Survivebest.Animal
{
    [Serializable]
    public class CompanionBond
    {
        public string OwnerCharacterId;
        public string CompanionId;
        public string CompanionName;
        public string Species = "Dog";
        [Range(0f, 1f)] public float BondStrength = 0.5f;
        public bool IsAlive = true;
    }

    [Serializable]
    public class CompanionGriefProfile
    {
        public string OwnerCharacterId;
        public string CompanionId;
        [Range(0f, 1f)] public float GriefIntensity = 0f;
        [Range(0f, 1f)] public float RoutineVoid = 0f;
        [Range(0f, 1f)] public float Memorialization = 0f;
        [Range(0f, 1f)] public float Acceptance = 0f;
        public List<string> ComfortObjects = new();
        public List<string> MemorialRituals = new();
    }

    public class CompanionGriefSystem : MonoBehaviour
    {
        [SerializeField] private List<CompanionBond> bonds = new();
        [SerializeField] private List<CompanionGriefProfile> griefProfiles = new();

        public IReadOnlyList<CompanionBond> Bonds => bonds;
        public IReadOnlyList<CompanionGriefProfile> GriefProfiles => griefProfiles;

        public CompanionBond RegisterCompanion(string ownerCharacterId, string companionId, string companionName, string species, float bondStrength)
        {
            CompanionBond existing = bonds.Find(x => x != null && x.OwnerCharacterId == ownerCharacterId && x.CompanionId == companionId);
            if (existing != null)
            {
                existing.CompanionName = companionName;
                existing.Species = string.IsNullOrWhiteSpace(species) ? existing.Species : species;
                existing.BondStrength = Mathf.Clamp01(bondStrength);
                return existing;
            }

            CompanionBond bond = new CompanionBond
            {
                OwnerCharacterId = ownerCharacterId,
                CompanionId = companionId,
                CompanionName = companionName,
                Species = string.IsNullOrWhiteSpace(species) ? "Dog" : species,
                BondStrength = Mathf.Clamp01(bondStrength)
            };
            bonds.Add(bond);
            return bond;
        }

        public CompanionGriefProfile RecordCompanionLoss(string ownerCharacterId, string companionId, string memorialObject = null)
        {
            CompanionBond bond = bonds.Find(x => x != null && x.OwnerCharacterId == ownerCharacterId && x.CompanionId == companionId);
            if (bond == null)
            {
                return null;
            }

            bond.IsAlive = false;
            CompanionGriefProfile profile = GetOrCreateGriefProfile(ownerCharacterId, companionId);

            float dogWeight = string.Equals(bond.Species, "Dog", StringComparison.OrdinalIgnoreCase) ? 0.08f : 0f;
            float griefSpike = 0.35f + bond.BondStrength * 0.5f + dogWeight;
            profile.GriefIntensity = Mathf.Clamp01(profile.GriefIntensity + griefSpike);
            profile.RoutineVoid = Mathf.Clamp01(profile.RoutineVoid + 0.2f + bond.BondStrength * 0.5f);
            profile.Acceptance = Mathf.Clamp01(profile.Acceptance - 0.15f);

            if (!string.IsNullOrWhiteSpace(memorialObject))
            {
                AddUnique(profile.ComfortObjects, memorialObject);
                profile.Memorialization = Mathf.Clamp01(profile.Memorialization + 0.1f);
            }

            return profile;
        }

        public void PerformMemorialRitual(string ownerCharacterId, string companionId, string ritualName)
        {
            CompanionGriefProfile profile = GetOrCreateGriefProfile(ownerCharacterId, companionId);
            if (profile == null)
            {
                return;
            }

            AddUnique(profile.MemorialRituals, ritualName);
            profile.Memorialization = Mathf.Clamp01(profile.Memorialization + 0.15f);
            profile.GriefIntensity = Mathf.Clamp01(profile.GriefIntensity - 0.08f);
            profile.Acceptance = Mathf.Clamp01(profile.Acceptance + 0.12f);
        }

        public void TickGriefRecovery(float delta = 0.04f)
        {
            float clamped = Mathf.Clamp01(delta);
            for (int i = 0; i < griefProfiles.Count; i++)
            {
                CompanionGriefProfile profile = griefProfiles[i];
                if (profile == null)
                {
                    continue;
                }

                profile.GriefIntensity = Mathf.Clamp01(profile.GriefIntensity - clamped * (0.5f + profile.Memorialization * 0.5f));
                profile.RoutineVoid = Mathf.Clamp01(profile.RoutineVoid - clamped * 0.4f);
                profile.Acceptance = Mathf.Clamp01(profile.Acceptance + clamped * (0.3f + profile.Memorialization * 0.3f));
            }
        }

        private CompanionGriefProfile GetOrCreateGriefProfile(string ownerCharacterId, string companionId)
        {
            CompanionGriefProfile profile = griefProfiles.Find(x => x != null && x.OwnerCharacterId == ownerCharacterId && x.CompanionId == companionId);
            if (profile != null)
            {
                return profile;
            }

            profile = new CompanionGriefProfile
            {
                OwnerCharacterId = ownerCharacterId,
                CompanionId = companionId
            };
            griefProfiles.Add(profile);
            return profile;
        }

        private static void AddUnique(List<string> list, string value)
        {
            if (list == null || string.IsNullOrWhiteSpace(value) || list.Contains(value))
            {
                return;
            }

            list.Add(value);
        }
    }
}

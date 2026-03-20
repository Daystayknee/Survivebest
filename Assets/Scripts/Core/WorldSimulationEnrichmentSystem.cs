using System;
using System.Collections.Generic;
using UnityEngine;
using Survivebest.Location;
using Survivebest.World;

namespace Survivebest.Core
{
    [Serializable]
    public class DistrictCultureFlavor
    {
        public string DistrictId;
        public string Subculture = "mixed";
        public List<string> LocalSlang = new();
        public List<string> TrendTags = new();
        [Range(0f, 100f)] public float NightlifeIntensity = 40f;
        [Range(0f, 100f)] public float NeighborhoodFamiliarity = 25f;
        [Range(0f, 100f)] public float OccultAwareness = 10f;
        [Range(0f, 100f)] public float HiddenFactionTolerance = 15f;
    }

    [Serializable]
    public class DistrictServiceEnvelope
    {
        public string DistrictId;
        [Range(0f, 100f)] public float TransitReliability = 65f;
        [Range(0f, 100f)] public float ServiceQualityVariation = 18f;
        [Range(0f, 100f)] public float OutagePropagationRisk = 12f;
        [Range(0f, 100f)] public float SeasonalSupplyShift = 20f;
        [Range(0f, 100f)] public float PollutionLevel = 30f;
        [Range(0f, 100f)] public float AllergenLevel = 25f;
    }

    public class WorldSimulationEnrichmentSystem : MonoBehaviour
    {
        [SerializeField] private LivingWorldInfrastructureEngine livingWorldInfrastructureEngine;
        [SerializeField] private TownSimulationSystem townSimulationSystem;
        [SerializeField] private WeatherManager weatherManager;
        [SerializeField] private List<DistrictCultureFlavor> districtFlavors = new();
        [SerializeField] private List<DistrictServiceEnvelope> districtServiceEnvelopes = new();

        public IReadOnlyList<DistrictCultureFlavor> DistrictFlavors => districtFlavors;
        public IReadOnlyList<DistrictServiceEnvelope> DistrictServiceEnvelopes => districtServiceEnvelopes;

        public DistrictCultureFlavor GetOrCreateFlavor(string districtId)
        {
            DistrictCultureFlavor flavor = districtFlavors.Find(x => x != null && x.DistrictId == districtId);
            if (flavor != null) return flavor;
            flavor = new DistrictCultureFlavor { DistrictId = districtId };
            districtFlavors.Add(flavor);
            return flavor;
        }

        public DistrictServiceEnvelope GetOrCreateServiceEnvelope(string districtId)
        {
            DistrictServiceEnvelope envelope = districtServiceEnvelopes.Find(x => x != null && x.DistrictId == districtId);
            if (envelope != null) return envelope;
            envelope = new DistrictServiceEnvelope { DistrictId = districtId };
            districtServiceEnvelopes.Add(envelope);
            return envelope;
        }

        public void RegisterDistrictFlavor(string districtId, string subculture, List<string> slang, List<string> trendTags)
        {
            DistrictCultureFlavor flavor = GetOrCreateFlavor(districtId);
            flavor.Subculture = string.IsNullOrWhiteSpace(subculture) ? flavor.Subculture : subculture;
            if (slang != null) flavor.LocalSlang = new List<string>(slang);
            if (trendTags != null) flavor.TrendTags = new List<string>(trendTags);
        }

        public void RefreshFromInfrastructure(string districtId)
        {
            DistrictCultureFlavor flavor = GetOrCreateFlavor(districtId);
            DistrictServiceEnvelope envelope = GetOrCreateServiceEnvelope(districtId);
            DistrictResourceGeographyProfile resources = livingWorldInfrastructureEngine != null ? livingWorldInfrastructureEngine.GetDistrictResources(districtId) : null;
            DistrictEcologyProfile ecology = livingWorldInfrastructureEngine != null ? livingWorldInfrastructureEngine.GetDistrictEcology(districtId) : null;
            SeasonalDistrictConsequenceProfile seasonal = livingWorldInfrastructureEngine != null ? livingWorldInfrastructureEngine.GetSeasonalConsequences(districtId) : null;

            if (resources != null)
            {
                flavor.NightlifeIntensity = resources.NightlifeDensity;
                flavor.OccultAwareness = resources.OccultSecrecyTolerance * 0.65f;
                flavor.HiddenFactionTolerance = resources.VampireNightInfrastructure * 0.45f;
                envelope.TransitReliability = resources.TransitAccess;
            }

            if (ecology != null)
            {
                envelope.PollutionLevel = ecology.PollutionLevel;
                envelope.AllergenLevel = ecology.AllergenPressure;
            }

            if (seasonal != null)
            {
                envelope.OutagePropagationRisk = seasonal.OutageRecoveryPressure;
                envelope.SeasonalSupplyShift = seasonal.StormPrepPressure + seasonal.CropYieldModifier * 0.2f;
            }

            if (townSimulationSystem != null)
            {
                DistrictDefinition district = townSimulationSystem.GetDistrict(districtId);
                if (district != null)
                {
                    flavor.NeighborhoodFamiliarity = Mathf.Clamp(flavor.NeighborhoodFamiliarity + district.Wealth * 15f + district.Safety * 10f, 0f, 100f);
                }
            }

            if (weatherManager != null && weatherManager.CurrentWeather == WeatherState.Stormy)
            {
                envelope.OutagePropagationRisk = Mathf.Clamp(envelope.OutagePropagationRisk + 18f, 0f, 100f);
                envelope.TransitReliability = Mathf.Clamp(envelope.TransitReliability - 12f, 0f, 100f);
            }
        }

        public string BuildDistrictVibeSummary(string districtId)
        {
            DistrictCultureFlavor flavor = GetOrCreateFlavor(districtId);
            DistrictServiceEnvelope envelope = GetOrCreateServiceEnvelope(districtId);
            string slang = flavor.LocalSlang.Count > 0 ? string.Join(", ", flavor.LocalSlang) : "no dominant slang yet";
            string trends = flavor.TrendTags.Count > 0 ? string.Join(", ", flavor.TrendTags) : "stable routine";
            return $"{districtId}: {flavor.Subculture} energy, slang [{slang}], trends [{trends}], nightlife {flavor.NightlifeIntensity:0}, transit {envelope.TransitReliability:0}, pollution {envelope.PollutionLevel:0}, allergens {envelope.AllergenLevel:0}, occult awareness {flavor.OccultAwareness:0}.";
        }
    }
}

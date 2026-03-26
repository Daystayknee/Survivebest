using UnityEngine;

namespace Survivebest.Core
{
    [CreateAssetMenu(fileName = "GlobalSimulationSettings", menuName = "Survivebest/Balance/Global Simulation Settings")]
    public class GlobalSimulationSettings : ScriptableObject
    {
        [Header("Difficulty Modifiers")]
        [SerializeField, Min(0f)] private float hungerDecayRateMultiplier = 1f;
        [SerializeField, Min(0f)] private float dailyIncidentSpawnRateMultiplier = 1f;
        [SerializeField, Min(0f)] private float dailyCommunityEventSpawnRateMultiplier = 1f;

        public float HungerDecayRateMultiplier => hungerDecayRateMultiplier;
        public float DailyIncidentSpawnRateMultiplier => dailyIncidentSpawnRateMultiplier;
        public float DailyCommunityEventSpawnRateMultiplier => dailyCommunityEventSpawnRateMultiplier;

        public float ScaleNeedDecay(float baseDecay, float balanceMultiplier = 1f)
        {
            return Mathf.Max(0f, baseDecay) * Mathf.Max(0f, balanceMultiplier) * hungerDecayRateMultiplier;
        }

        public float ScaleSpawnChance(float baseChance, float spawnMultiplier)
        {
            return Mathf.Clamp01(baseChance * Mathf.Max(0f, spawnMultiplier));
        }
    }
}

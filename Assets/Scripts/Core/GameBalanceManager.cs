using UnityEngine;

namespace Survivebest.Core
{
    public class GameBalanceManager : MonoBehaviour
    {
        [Header("Needs")]
        [SerializeField, Min(0.1f)] private float needDecayMultiplier = 1f;
        [SerializeField, Min(0f)] private float socialChangeMultiplier = 1f;
        [SerializeField, Min(0f)] private float emotionalStabilityRange = 1f;

        [Header("Economy")]
        [SerializeField, Min(0.1f)] private float wageMultiplier = 1f;
        [SerializeField, Min(0.1f)] private float itemPriceMultiplier = 1f;
        [SerializeField, Min(0.1f)] private float questRewardMultiplier = 1f;
        [SerializeField, Min(0.1f)] private float jailPunishmentMultiplier = 1f;

        [Header("Risk")]
        [SerializeField, Min(0f)] private float illnessFrequencyMultiplier = 1f;
        [SerializeField, Min(0f)] private float recoveryTimeMultiplier = 1f;
        [SerializeField, Min(0f)] private float addictionSeverityMultiplier = 1f;
        [SerializeField, Min(0f)] private float weatherPenaltyMultiplier = 1f;
        [SerializeField, Min(0f)] private float crimeRiskMultiplier = 1f;

        [Header("Progression")]
        [SerializeField, Min(0.1f)] private float skillXpMultiplier = 1f;

        public float NeedDecayMultiplier => needDecayMultiplier;
        public float SocialChangeMultiplier => socialChangeMultiplier;
        public float EmotionalStabilityRange => emotionalStabilityRange;
        public float WageMultiplier => wageMultiplier;
        public float ItemPriceMultiplier => itemPriceMultiplier;
        public float QuestRewardMultiplier => questRewardMultiplier;
        public float JailPunishmentMultiplier => jailPunishmentMultiplier;
        public float IllnessFrequencyMultiplier => illnessFrequencyMultiplier;
        public float RecoveryTimeMultiplier => recoveryTimeMultiplier;
        public float AddictionSeverityMultiplier => addictionSeverityMultiplier;
        public float WeatherPenaltyMultiplier => weatherPenaltyMultiplier;
        public float CrimeRiskMultiplier => crimeRiskMultiplier;
        public float SkillXpMultiplier => skillXpMultiplier;

        public float ScaleNeedDecay(float value) => value * needDecayMultiplier;
        public float ScaleSocialChange(float value) => value * socialChangeMultiplier;
        public float ScaleEmotionalDelta(float value) => value / Mathf.Max(0.1f, emotionalStabilityRange);
        public float ScalePrice(float value) => value * itemPriceMultiplier;
        public float ScaleWage(float value) => value * wageMultiplier;
        public float ScaleQuestReward(float value) => value * questRewardMultiplier;
        public float ScaleFineAmount(float value) => value * jailPunishmentMultiplier;
        public float ScaleJailHours(float value) => value * jailPunishmentMultiplier;
        public float ScaleWeatherPenalty(float value) => value * weatherPenaltyMultiplier;
        public float ScaleSkillXp(float value) => value * skillXpMultiplier;
        public float ScaleCrimeRisk(float value) => Mathf.Clamp01(value * crimeRiskMultiplier);
    }
}

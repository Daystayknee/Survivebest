using System;
using UnityEngine;
using Survivebest.Events;

namespace Survivebest.Core
{
    public class LifeDriftSystem : MonoBehaviour
    {
        [SerializeField] private CharacterCore owner;
        [SerializeField] private World.WorldClock worldClock;
        [SerializeField] private SocialSystem socialSystem;
        [SerializeField] private SkillSystem skillSystem;
        [SerializeField] private LifestyleBehaviorSystem lifestyleBehaviorSystem;
        [SerializeField] private PersonalityMatrixSystem personalityMatrixSystem;
        [SerializeField] private LongTermProgressionSystem longTermProgressionSystem;
        [SerializeField] private GameEventHub gameEventHub;
        [SerializeField, Range(0f, 1f)] private float passiveDriftStrength = 0.35f;

        private void OnEnable()
        {
            if (worldClock != null)
            {
                worldClock.OnDayPassed += HandleDayPassed;
            }
        }

        private void OnDisable()
        {
            if (worldClock != null)
            {
                worldClock.OnDayPassed -= HandleDayPassed;
            }
        }

        public void ApplyPassiveDay(int day, float idleIntensity = 1f)
        {
            float intensity = Mathf.Clamp(idleIntensity, 0f, 2f) * Mathf.Clamp01(0.4f + passiveDriftStrength);

            socialSystem?.ApplyDailyRelationshipDrift();
            if (skillSystem != null)
            {
                skillSystem.AddExperience("Meditation", -0.2f * intensity);
                skillSystem.AddExperience("Programming", -0.25f * intensity);
            }

            lifestyleBehaviorSystem?.ApplyPassiveIdentityDrift(intensity);
            longTermProgressionSystem?.ApplyGoalDrift(intensity);

            if (personalityMatrixSystem != null && owner != null)
            {
                PersonalityMatrixProfile profile = personalityMatrixSystem.GetOrCreateProfile(owner.CharacterId);
                profile.Adaptability = Mathf.Clamp(profile.Adaptability + intensity * 0.4f, 0f, 100f);
                profile.ComfortDrive = Mathf.Clamp(profile.ComfortDrive + intensity * 0.35f, 0f, 100f);
                profile.Ambition = Mathf.Clamp(profile.Ambition - intensity * 0.25f, 0f, 100f);
            }

            Publish(day, intensity);
        }

        private void HandleDayPassed(int day)
        {
            ApplyPassiveDay(day, 1f);
        }

        private void Publish(int day, float intensity)
        {
            (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
            {
                Type = SimulationEventType.DayStageChanged,
                Severity = SimulationEventSeverity.Info,
                SystemName = nameof(LifeDriftSystem),
                SourceCharacterId = owner != null ? owner.CharacterId : null,
                ChangeKey = "LifeDrift",
                Reason = $"Life drift advanced on day {day}",
                Magnitude = intensity
            });
        }
    }
}

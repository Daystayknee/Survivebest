using System.Text;
using UnityEngine;
using UnityEngine.UI;
using Survivebest.Commerce;
using Survivebest.Core;
using Survivebest.Events;
using Survivebest.Needs;
using Survivebest.World;

namespace Survivebest.UI
{
    public class GameHUD : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private HouseholdManager householdManager;
        [SerializeField] private WorldClock worldClock;
        [SerializeField] private OrderingSystem orderingSystem;
        [SerializeField] private GameEventHub gameEventHub;
        [SerializeField] private GameplayLifeLoopOrchestrator gameplayLifeLoopOrchestrator;

        [Header("Bars")]
        [SerializeField] private Slider hungerBar;
        [SerializeField] private Slider socialBar;
        [SerializeField] private Slider energyBar;

        [Header("Text")]
        [SerializeField] private Text moneyText;
        [SerializeField] private Text clockText;
        [SerializeField] private Text feedText;
        [SerializeField] private Text lifeLoopSummaryText;
        [SerializeField, Min(1)] private int maxFeedLines = 12;

        private readonly StringBuilder feedBuilder = new();
        private NeedsSystem observedNeeds;

        private System.Action<float> hungerHandler;
        private System.Action<float> moodHandler;
        private System.Action<float> energyHandler;

        private void OnEnable()
        {
            if (householdManager != null)
            {
                householdManager.OnActiveCharacterChanged += HandleActiveCharacterChanged;
                HandleActiveCharacterChanged(householdManager.ActiveCharacter);
            }

            if (worldClock != null)
            {
                worldClock.OnMinutePassed += HandleMinutePassed;
                RefreshClock();
            }

            if (orderingSystem != null)
            {
                orderingSystem.OnWalletChanged += HandleWalletChanged;
                HandleWalletChanged(orderingSystem.Wallet);
            }

            if (gameEventHub != null)
            {
                gameEventHub.OnEventPublished += HandleEventPublished;
            }

            if (gameplayLifeLoopOrchestrator != null)
            {
                gameplayLifeLoopOrchestrator.OnSnapshotUpdated += HandleSnapshotUpdated;
                if (gameplayLifeLoopOrchestrator.CurrentSnapshot != null)
                {
                    HandleSnapshotUpdated(gameplayLifeLoopOrchestrator.CurrentSnapshot);
                }
            }
        }

        private void OnDisable()
        {
            if (householdManager != null)
            {
                householdManager.OnActiveCharacterChanged -= HandleActiveCharacterChanged;
            }

            if (worldClock != null)
            {
                worldClock.OnMinutePassed -= HandleMinutePassed;
            }

            if (orderingSystem != null)
            {
                orderingSystem.OnWalletChanged -= HandleWalletChanged;
            }

            if (gameEventHub != null)
            {
                gameEventHub.OnEventPublished -= HandleEventPublished;
            }

            if (gameplayLifeLoopOrchestrator != null)
            {
                gameplayLifeLoopOrchestrator.OnSnapshotUpdated -= HandleSnapshotUpdated;
            }

            if (observedNeeds != null)
            {
                if (hungerHandler != null) observedNeeds.OnHungerChanged -= hungerHandler;
                if (moodHandler != null) observedNeeds.OnMoodChanged -= moodHandler;
                if (energyHandler != null) observedNeeds.OnEnergyChanged -= energyHandler;
            }
        }

        private void HandleActiveCharacterChanged(CharacterCore character)
        {
            if (observedNeeds != null)
            {
                if (hungerHandler != null) observedNeeds.OnHungerChanged -= hungerHandler;
                if (moodHandler != null) observedNeeds.OnMoodChanged -= moodHandler;
                if (energyHandler != null) observedNeeds.OnEnergyChanged -= energyHandler;
            }

            NeedsSystem needs = character != null ? character.GetComponent<NeedsSystem>() : null;
            observedNeeds = needs;

            if (needs == null)
            {
                SetBar(hungerBar, 0f);
                SetBar(socialBar, 0f);
                SetBar(energyBar, 0f);
                return;
            }

            SetBar(hungerBar, needs.Hunger);
            SetBar(socialBar, needs.Mood);
            SetBar(energyBar, needs.Energy);

            hungerHandler = value => SetBar(hungerBar, value);
            moodHandler = value => SetBar(socialBar, value);
            energyHandler = value => SetBar(energyBar, value);

            needs.OnHungerChanged += hungerHandler;
            needs.OnMoodChanged += moodHandler;
            needs.OnEnergyChanged += energyHandler;
        }

        private void HandleWalletChanged(float wallet)
        {
            if (moneyText != null)
            {
                moneyText.text = $"Money: ${wallet:0}";
            }
        }

        private void HandleMinutePassed(int hour, int minute)
        {
            RefreshClock();
        }

        private void RefreshClock()
        {
            if (clockText == null || worldClock == null)
            {
                return;
            }

            clockText.text = $"Day {worldClock.Day} - {worldClock.Hour:00}:{worldClock.Minute:00}";
        }

        private void HandleEventPublished(SimulationEvent simulationEvent)
        {
            if (feedText == null || simulationEvent == null)
            {
                return;
            }

            string line = $"[{simulationEvent.Hour:00}:00] {simulationEvent.Type}: {simulationEvent.Reason}";
            AppendFeed(line);
        }

        private void HandleSnapshotUpdated(LifeLoopExperienceSnapshot snapshot)
        {
            if (lifeLoopSummaryText == null || snapshot == null)
            {
                return;
            }

            lifeLoopSummaryText.text = BuildHudLoopDigest(snapshot);
        }

        private void AppendFeed(string line)
        {
            string current = feedText.text;
            string[] lines = string.IsNullOrEmpty(current) ? new string[0] : current.Split('\n');

            feedBuilder.Clear();
            int start = Mathf.Max(0, lines.Length - (maxFeedLines - 1));
            for (int i = start; i < lines.Length; i++)
            {
                if (!string.IsNullOrWhiteSpace(lines[i]))
                {
                    feedBuilder.AppendLine(lines[i]);
                }
            }

            feedBuilder.AppendLine(line);
            feedText.text = feedBuilder.ToString().TrimEnd();
        }

        private static void SetBar(Slider slider, float value)
        {
            if (slider == null)
            {
                return;
            }

            slider.minValue = 0f;
            slider.maxValue = 100f;
            slider.value = Mathf.Clamp(value, 0f, 100f);
        }

        public string BuildHudLoopDigest(LifeLoopExperienceSnapshot snapshot)
        {
            if (snapshot == null)
            {
                return "No life-loop snapshot available.";
            }

            return $"Now: {snapshot.PresenceLabel}\n" +
                   $"Consequence: {snapshot.ConsequenceLabel}\n" +
                   $"Continuity: {snapshot.ContinuityLabel}\n" +
                   $"Next up: {snapshot.RecommendedAction}";
        }
    }
}

using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using Survivebest.Activity;
using Survivebest.Commerce;
using Survivebest.Core;
using Survivebest.Crime;
using Survivebest.Events;
using Survivebest.Health;
using Survivebest.Location;
using Survivebest.Needs;
using Survivebest.Quest;
using Survivebest.World;

namespace Survivebest.Tests.EditMode
{
    public class LifecycleCompletenessTests
    {
        [Test]
        public void WeatherEffectSystem_HourTick_UpdatesNeedsAndPublishesStructuredWeatherEvent()
        {
            ResetGameEventHubSingleton();
            GameObject root = new GameObject("WeatherLifecycleRoot");
            GameEventHub hub = root.AddComponent<GameEventHub>();
            HouseholdManager household = root.AddComponent<HouseholdManager>();
            WeatherEffectSystem weather = root.AddComponent<WeatherEffectSystem>();

            GameObject memberGo = new GameObject("WeatherMember");
            CharacterCore member = memberGo.AddComponent<CharacterCore>();
            member.Initialize("char_weather", "Weather Test", LifeStage.Adult);
            NeedsSystem needs = memberGo.AddComponent<NeedsSystem>();
            HealthSystem health = memberGo.AddComponent<HealthSystem>();
            household.AddMember(member);
            household.SetActiveCharacter(member);

            SetPrivateField(weather, "householdManager", household);
            SetPrivateField(weather, "gameEventHub", hub);
            SetPrivateField(weather, "currentWeather", WeatherState.Stormy);

            float energyBefore = needs.Energy;
            float vitalityBefore = health.Vitality;

            InvokePrivate(weather, "HandleHourPassed", 1);

            Assert.Less(needs.Energy, energyBefore);
            Assert.Less(health.Vitality, vitalityBefore);
            Assert.AreEqual(SimulationEventType.WeatherChanged, hub.RecentEvents[^1].Type);

            Object.DestroyImmediate(memberGo);
            Object.DestroyImmediate(root);
            ResetGameEventHubSingleton();
        }

        [Test]
        public void DaySliceManager_AdvanceStage_PublishesStageEventAndProgressesLoop()
        {
            ResetGameEventHubSingleton();
            GameObject root = new GameObject("DaySliceLifecycleRoot");
            GameEventHub hub = root.AddComponent<GameEventHub>();
            HouseholdManager household = root.AddComponent<HouseholdManager>();
            DaySliceManager daySlice = root.AddComponent<DaySliceManager>();

            GameObject memberGo = new GameObject("DaySliceMember");
            CharacterCore member = memberGo.AddComponent<CharacterCore>();
            member.Initialize("char_day", "Day Slice Test", LifeStage.Adult);
            ActivitySystem activity = memberGo.AddComponent<ActivitySystem>();
            memberGo.AddComponent<NeedsSystem>();
            household.AddMember(member);
            household.SetActiveCharacter(member);

            SetPrivateField(daySlice, "householdManager", household);
            SetPrivateField(daySlice, "activitySystem", activity);
            SetPrivateField(daySlice, "gameEventHub", hub);

            daySlice.AdvanceStage();

            Assert.AreEqual(DaySliceStage.CheckNeeds, daySlice.CurrentStage);
            Assert.AreEqual(SimulationEventType.DayStageChanged, hub.RecentEvents[^1].Type);

            Object.DestroyImmediate(memberGo);
            Object.DestroyImmediate(root);
            ResetGameEventHubSingleton();
        }

        [Test]
        public void MedicalConditionSystem_ConditionExpiresAfterTick_AndPublishesLifecycleEvent()
        {
            ResetGameEventHubSingleton();
            GameObject root = new GameObject("MedicalLifecycleRoot");
            GameEventHub hub = root.AddComponent<GameEventHub>();
            MedicalConditionSystem medical = root.AddComponent<MedicalConditionSystem>();
            root.AddComponent<NeedsSystem>();
            root.AddComponent<HealthSystem>();
            CharacterCore owner = root.AddComponent<CharacterCore>();
            owner.Initialize("char_medical", "Medical Test", LifeStage.Adult);

            SetPrivateField(medical, "owner", owner);
            SetPrivateField(medical, "gameEventHub", hub);
            SetPrivateField(medical, "activeConditions", new List<MedicalCondition>
            {
                new MedicalCondition
                {
                    Id = "cond_1",
                    DisplayName = "Test Fever",
                    IsIllness = true,
                    IllnessType = IllnessType.CommonCold,
                    Severity = ConditionSeverity.Mild,
                    RemainingHours = 1
                }
            });

            InvokePrivate(medical, "HandleHourPassed", 1);

            Assert.AreEqual(0, medical.ActiveConditions.Count);
            Assert.AreEqual(SimulationEventType.IllnessStarted, hub.RecentEvents[^1].Type);
            Assert.AreEqual("ConditionExpired", hub.RecentEvents[^1].ChangeKey);

            Object.DestroyImmediate(root);
            ResetGameEventHubSingleton();
        }

        [Test]
        public void SubstanceSystem_UseThenExpire_PublishesStructuredSubstanceLifecycleEvents()
        {
            ResetGameEventHubSingleton();
            GameObject root = new GameObject("SubstanceLifecycleRoot");
            GameEventHub hub = root.AddComponent<GameEventHub>();
            CharacterCore owner = root.AddComponent<CharacterCore>();
            NeedsSystem needs = root.AddComponent<NeedsSystem>();
            HealthSystem health = root.AddComponent<HealthSystem>();
            var emotions = root.AddComponent<Survivebest.Emotion.EmotionSystem>();
            var status = root.AddComponent<Survivebest.Status.StatusEffectSystem>();
            SubstanceSystem substance = root.AddComponent<SubstanceSystem>();
            owner.Initialize("char_substance", "Substance Test", LifeStage.Adult);

            SetPrivateField(substance, "owner", owner);
            SetPrivateField(substance, "needsSystem", needs);
            SetPrivateField(substance, "healthSystem", health);
            SetPrivateField(substance, "emotionSystem", emotions);
            SetPrivateField(substance, "statusEffectSystem", status);
            SetPrivateField(substance, "gameEventHub", hub);

            substance.UseSubstance(SubstanceType.Caffeine);
            for (int i = 0; i < 4; i++)
            {
                InvokePrivate(substance, "HandleHourPassed", i);
            }

            Assert.AreEqual(0, substance.ActiveEffects.Count);
            Assert.AreEqual(SimulationEventType.SubstanceStateChanged, hub.RecentEvents[^1].Type);
            StringAssert.Contains("Caffeine", hub.RecentEvents[^1].ChangeKey);

            Object.DestroyImmediate(root);
            ResetGameEventHubSingleton();
        }

        [Test]
        public void JusticeSystem_JailSentence_ReleasesAndClearsLifecycleState()
        {
            ResetGameEventHubSingleton();
            GameObject root = new GameObject("JusticeLifecycleRoot");
            GameEventHub hub = root.AddComponent<GameEventHub>();
            HouseholdManager household = root.AddComponent<HouseholdManager>();
            JusticeSystem justice = root.AddComponent<JusticeSystem>();

            GameObject offenderGo = new GameObject("Offender");
            CharacterCore offender = offenderGo.AddComponent<CharacterCore>();
            offender.Initialize("char_justice", "Justice Test", LifeStage.Adult);
            offenderGo.AddComponent<NeedsSystem>();
            household.AddMember(offender);

            SetPrivateField(justice, "gameEventHub", hub);
            justice.ApplyRuntimeState(new List<JusticeSystem.ActiveSentenceRecord>
            {
                new JusticeSystem.ActiveSentenceRecord
                {
                    OffenderCharacterId = "char_justice",
                    CrimeType = "TestCrime",
                    RemainingJailHours = 1,
                    Stage = LegalProcessStage.Sentenced
                }
            }, household.Members);

            InvokePrivate(justice, "HandleHourPassed", 1);

            Assert.AreEqual(0, justice.ActiveSentences.Count);
            Assert.AreEqual(SimulationEventType.JusticeOutcomeApplied, hub.RecentEvents[^1].Type);
            Assert.AreEqual("JailRelease", hub.RecentEvents[^1].ChangeKey);

            Object.DestroyImmediate(offenderGo);
            Object.DestroyImmediate(root);
            ResetGameEventHubSingleton();
        }

        [Test]
        public void ContractBoardSystem_ExpiredAcceptedContract_PublishesContractLifecycleEvent()
        {
            ResetGameEventHubSingleton();
            GameObject root = new GameObject("ContractLifecycleRoot");
            GameEventHub hub = root.AddComponent<GameEventHub>();
            ContractBoardSystem contracts = root.AddComponent<ContractBoardSystem>();

            SetPrivateField(contracts, "gameEventHub", hub);
            contracts.ApplyRuntimeState(new List<AnimalSightingContract>
            {
                new AnimalSightingContract
                {
                    ContractId = "contract_expire",
                    AnimalName = "Ghost Owl",
                    ZoneName = "Whisper Lake",
                    Reward = 50,
                    DeadlineHour = -1,
                    State = ContractState.Accepted,
                    AcceptedCharacterId = "char_1"
                }
            });

            InvokePrivate(contracts, "HandleHourPassed", 1);

            Assert.AreEqual(ContractState.Expired, contracts.Contracts[0].State);
            Assert.AreEqual(SimulationEventType.ContractStateChanged, hub.RecentEvents[^1].Type);
            StringAssert.Contains("Expired", hub.RecentEvents[^1].ChangeKey);

            Object.DestroyImmediate(root);
            ResetGameEventHubSingleton();
        }

        [Test]
        public void OrderingSystem_OrderLifecycle_DeliversAndPublishesDeliveryEvent()
        {
            ResetGameEventHubSingleton();
            GameObject root = new GameObject("OrderingLifecycleRoot");
            GameEventHub hub = root.AddComponent<GameEventHub>();
            WorldClock clock = root.AddComponent<WorldClock>();
            OrderingSystem ordering = root.AddComponent<OrderingSystem>();
            CharacterCore owner = root.AddComponent<CharacterCore>();
            NeedsSystem needs = root.AddComponent<NeedsSystem>();
            HealthSystem health = root.AddComponent<HealthSystem>();
            owner.Initialize("char_order", "Ordering Test", LifeStage.Adult);

            SetPrivateField(ordering, "worldClock", clock);
            SetPrivateField(ordering, "gameEventHub", hub);

            clock.SetDateTime(1, 1, 1, 0, 0);
            Assert.IsTrue(ordering.OrderOut("Instant Noodles", needs, health, owner.CharacterId));

            clock.SetDateTime(1, 1, 1, 0, 25);
            InvokePrivate(ordering, "HandleMinutePassed", 0, 25);

            Assert.AreEqual(0, ordering.PendingOrders.Count);
            Assert.AreEqual(SimulationEventType.OrderDelivered, hub.RecentEvents[^1].Type);

            Object.DestroyImmediate(root);
            ResetGameEventHubSingleton();
        }

        private static void SetPrivateField(object instance, string fieldName, object value)
        {
            FieldInfo field = instance.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            if (field == null)
            {
                field = instance.GetType().GetField($"<{fieldName}>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance);
            }

            Assert.NotNull(field, $"Missing field: {fieldName}");
            field.SetValue(instance, value);
        }

        private static void InvokePrivate(object instance, string methodName, params object[] args)
        {
            MethodInfo method = instance.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(method, $"Missing method: {methodName}");
            method.Invoke(instance, args);
        }

        private static void ResetGameEventHubSingleton()
        {
            FieldInfo instanceField = typeof(GameEventHub).GetField("<Instance>k__BackingField", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.NotNull(instanceField);
            instanceField.SetValue(null, null);
        }
    }
}

using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using Survivebest.Activity;
using Survivebest.Core;
using Survivebest.Needs;
using Survivebest.NPC;
using Survivebest.Social;
using Survivebest.UI;

namespace Survivebest.Tests.EditMode
{
    public class DiegeticUiAndRoutineTests
    {
        [Test]
        public void DiegeticLifeUiTrioController_BuildsPhoneMirrorAndJournalSnapshots()
        {
            GameObject root = new GameObject("DiegeticTrio");
            HouseholdManager household = root.AddComponent<HouseholdManager>();
            RelationshipMemorySystem relationshipMemory = root.AddComponent<RelationshipMemorySystem>();
            MemoryKernelSystem memoryKernel = root.AddComponent<MemoryKernelSystem>();
            MindStateSystem mindState = root.AddComponent<MindStateSystem>();
            MeaningPurposeSystem meaning = root.AddComponent<MeaningPurposeSystem>();
            LongTermProgressionSystem progression = root.AddComponent<LongTermProgressionSystem>();
            DiegeticLifeUiTrioController controller = root.AddComponent<DiegeticLifeUiTrioController>();

            GameObject charGo = new GameObject("ActiveCharacter");
            CharacterCore character = charGo.AddComponent<CharacterCore>();
            character.Initialize("char_ui", "UI Character", LifeStage.Adult);
            NeedsSystem needs = charGo.AddComponent<NeedsSystem>();
            needs.ApplySnapshot(new NeedsSnapshot { Hygiene = 42f, Grooming = 38f, Energy = 56f, Mood = 48f, Hydration = 55f, Hunger = 50f });
            household.AddMember(character);
            household.SetActiveCharacter(character);

            relationshipMemory.ApplyRuntimeState(
                new List<RelationshipMemory> { new RelationshipMemory { SubjectCharacterId = "char_ui", Topic = "district rumor", Impact = -8, IsPublic = true } },
                new List<RelationshipProfile>(),
                new List<ReputationEntry>());
            memoryKernel.AddMemory("char_ui", MemoryItemType.Core, "long_tail memory note", -0.4f, 0.7f, 0.1f, new[] { "long_tail" }, null);
            mindState.AddThoughtPulse("char_ui", "I should text my sibling.", 0.7f, ThoughtPulseSource.Person, 8);
            meaning.GetOrCreateMeaningState("char_ui");

            SetPrivateField(controller, "householdManager", household);
            SetPrivateField(controller, "relationshipMemorySystem", relationshipMemory);
            SetPrivateField(controller, "memoryKernelSystem", memoryKernel);
            SetPrivateField(controller, "mindStateSystem", mindState);
            SetPrivateField(controller, "meaningPurposeSystem", meaning);
            SetPrivateField(controller, "longTermProgressionSystem", progression);

            PhoneAppSnapshot phone = controller.BuildPhoneSnapshot();
            MirrorSnapshot mirror = controller.BuildMirrorSnapshot();
            JournalSnapshot journal = controller.BuildJournalSnapshot();

            Assert.IsNotEmpty(phone.ContactThreads);
            Assert.IsNotEmpty(phone.RumorAlerts);
            Assert.Greater(mirror.Confidence, 0f);
            Assert.IsNotEmpty(journal.MemoryHighlights);
            Assert.IsFalse(string.IsNullOrWhiteSpace(journal.IdentityArcSummary));

            Object.DestroyImmediate(charGo);
            Object.DestroyImmediate(root);
        }

        [Test]
        public void NpcScheduleSystem_AssignsBehaviorSignatures_AndDoomscrollRaisesStressAtNight()
        {
            GameObject root = new GameObject("NpcBehaviorSignatures");
            NpcScheduleSystem npcSystem = root.AddComponent<NpcScheduleSystem>();
            npcSystem.RegisterNpc("npc_sig", "Signature NPC", NpcJobType.Shopkeeper, "home", "work");
            NpcProfile profile = npcSystem.GetNpcProfile("npc_sig");

            Assert.NotNull(profile);
            Assert.GreaterOrEqual(profile.BehaviorSignatures.Count, 2);

            profile.BehaviorSignatures = new List<string> { "doomscrolls" };
            profile.CurrentState = NpcActivityState.Idle;
            profile.Stress = 20f;
            profile.Schedule = new List<NpcScheduleBlock>(); // keep idle at night

            MethodInfo handleHourPassed = typeof(NpcScheduleSystem).GetMethod("HandleHourPassed", BindingFlags.NonPublic | BindingFlags.Instance);
            handleHourPassed?.Invoke(npcSystem, new object[] { 23 });

            Assert.Greater(profile.Stress, 20f);
            Object.DestroyImmediate(root);
        }

        [Test]
        public void RoutineChainSystem_ExecutesQuickChains_AndRecordsHistory()
        {
            GameObject root = new GameObject("RoutineChains");
            ActivitySystem activity = root.AddComponent<ActivitySystem>();
            NeedsSystem needs = root.AddComponent<NeedsSystem>();
            RoutineChainSystem chains = root.AddComponent<RoutineChainSystem>();

            SetPrivateField(chains, "activitySystem", activity);
            SetPrivateField(chains, "needsSystem", needs);
            MethodInfo awake = typeof(RoutineChainSystem).GetMethod("Awake", BindingFlags.NonPublic | BindingFlags.Instance);
            awake?.Invoke(chains, null);

            RoutineChainExecution execution = chains.ExecuteRoutine("char_chain", RoutineChainType.EveningReset, 21);

            Assert.NotNull(execution);
            Assert.Greater(execution.CompletedSteps.Count, 0);
            Assert.AreEqual(1, chains.ExecutionHistory.Count);
            StringAssert.Contains("Home", chains.BuildQuickRoutineLabel(RoutineChainType.EveningReset));

            Object.DestroyImmediate(root);
        }

        private static void SetPrivateField(object instance, string fieldName, object value)
        {
            FieldInfo field = instance.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            field?.SetValue(instance, value);
        }
    }
}

using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using Survivebest.Core;
using Survivebest.Dialogue;
using Survivebest.Legacy;
using Survivebest.Social;

namespace Survivebest.Tests.EditMode
{
    public class DialogueAndLegacyGameplayTests
    {
        [Test]
        public void DialogueSystem_BuildReplyOptions_IncludesLegacyAndVampireChoices()
        {
            GameObject go = new GameObject("DialogueSystem");
            DialogueSystem dialogue = go.AddComponent<DialogueSystem>();
            CharacterCore owner = go.AddComponent<CharacterCore>();
            owner.Initialize("owner", "Owner", LifeStage.Adult, CharacterSpecies.Vampire);
            SocialSystem social = go.AddComponent<SocialSystem>();

            typeof(DialogueSystem).GetField("owner", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(dialogue, owner);
            typeof(DialogueSystem).GetField("socialSystem", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(dialogue, social);

            GameObject targetGo = new GameObject("Target");
            CharacterCore target = targetGo.AddComponent<CharacterCore>();
            target.Initialize("target", "Target", LifeStage.Adult);

            var options = dialogue.BuildReplyOptions(target, DialogueIntent.FriendlyChat);

            Assert.GreaterOrEqual(options.Count, 4);
            Assert.IsTrue(options.Any(o => o.Tone == DialogueReplyTone.Legacy));
            Assert.IsTrue(options.Any(o => o.Tone == DialogueReplyTone.Supernatural));

            Object.DestroyImmediate(targetGo);
            Object.DestroyImmediate(go);
        }

        [Test]
        public void LegacyManager_BuildSuccessorScoreCards_PrefersEstablishedAdults()
        {
            GameObject root = new GameObject("Legacy");
            HouseholdManager household = root.AddComponent<HouseholdManager>();
            RelationshipMemorySystem memory = root.AddComponent<RelationshipMemorySystem>();
            LegacyManager legacy = root.AddComponent<LegacyManager>();

            typeof(LegacyManager).GetField("householdManager", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(legacy, household);
            typeof(LegacyManager).GetField("relationshipMemorySystem", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(legacy, memory);

            CharacterCore active = new GameObject("Active").AddComponent<CharacterCore>();
            active.Initialize("active", "Active", LifeStage.Adult);
            CharacterCore adult = new GameObject("AdultHeir").AddComponent<CharacterCore>();
            adult.Initialize("adult", "Adult Heir", LifeStage.Adult, CharacterSpecies.Vampire);
            CharacterCore teen = new GameObject("TeenHeir").AddComponent<CharacterCore>();
            teen.Initialize("teen", "Teen Heir", LifeStage.Teen);

            household.AddMember(active);
            household.AddMember(adult);
            household.AddMember(teen);
            household.SetActiveCharacter(active);

            memory.GetOrCreateProfile(adult.CharacterId).Trust = 20;
            memory.GetOrCreateProfile(teen.CharacterId).Trust = 5;
            legacy.RecordLegacyBeat(adult, "Held the manor together", "legacy", 12, 8);

            var cards = legacy.BuildSuccessorScoreCards();

            Assert.AreEqual(2, cards.Count);
            Assert.AreEqual(adult, cards[0].Candidate);
            StringAssert.Contains("legacy weight", cards[0].Summary);

            Object.DestroyImmediate(active.gameObject);
            Object.DestroyImmediate(adult.gameObject);
            Object.DestroyImmediate(teen.gameObject);
            Object.DestroyImmediate(root);
        }
    }
}

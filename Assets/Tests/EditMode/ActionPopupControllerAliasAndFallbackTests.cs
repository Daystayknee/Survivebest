using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using Survivebest.Core;
using Survivebest.UI;

namespace Survivebest.Tests.EditMode
{
    public class ActionPopupControllerAliasAndFallbackTests
    {
        [Test]
        public void NormalizeActionKey_PreservesCheckNeeds_AndMapsTravelAlias()
        {
            MethodInfo normalize = typeof(ActionPopupController).GetMethod("NormalizeActionKey", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.NotNull(normalize);

            string checkNeeds = normalize.Invoke(null, new object[] { "check_needs" }) as string;
            string travel = normalize.Invoke(null, new object[] { "open_map_travel" }) as string;

            Assert.AreEqual("check_needs", checkNeeds);
            Assert.AreEqual("forage", travel);
        }

        [Test]
        public void ConfirmAction_EmitsResolvedActionKey_WhenRequestedKeyMissing()
        {
            GameObject root = new GameObject("ActionPopupConfirmFallbackKey");
            ActionPopupController popup = root.AddComponent<ActionPopupController>();

            GameObject householdGo = new GameObject("HouseholdManager");
            HouseholdManager household = householdGo.AddComponent<HouseholdManager>();
            GameObject actorGo = new GameObject("Actor");
            CharacterCore actor = actorGo.AddComponent<CharacterCore>();
            actor.Initialize("char_001", "Alex", LifeStage.YoungAdult);
            household.AddMember(actor);
            household.SetActiveCharacter(actor);

            typeof(ActionPopupController)
                .GetField("householdManager", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(popup, household);
            typeof(ActionPopupController)
                .GetField("currentActionKey", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(popup, null);
            typeof(ActionPopupController)
                .GetField("currentResolvedActionKey", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(popup, "camp");

            string emittedKey = null;
            popup.OnActionResolved += (key, _, _) => emittedKey = key;

            popup.ConfirmAction();

            Assert.AreEqual("camp", emittedKey);

            Object.DestroyImmediate(actorGo);
            Object.DestroyImmediate(householdGo);
            Object.DestroyImmediate(root);
        }
    }
}

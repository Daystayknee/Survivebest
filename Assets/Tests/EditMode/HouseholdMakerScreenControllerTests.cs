using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using Survivebest.Core;
using Survivebest.UI;
using Survivebest.World;

namespace Survivebest.Tests.EditMode
{
    public class HouseholdMakerScreenControllerTests
    {
        [Test]
        public void SetTab_WrapsWhenEnabled_AndUpdatesTabText()
        {
            GameObject root = new GameObject("HouseholdMakerRoot");
            HouseholdMakerScreenController controller = root.AddComponent<HouseholdMakerScreenController>();

            GameObject textGo = new GameObject("TabText");
            Text tabText = textGo.AddComponent<Text>();
            SetPrivateField(controller, "tabText", tabText);
            SetPrivateField(controller, "wrapTabs", true);

            controller.SetTab(-1);

            Assert.AreEqual(HouseholdMakerTab.Household, controller.CurrentTab);
            Assert.AreEqual(HouseholdMakerTab.Household.ToString(), tabText.text);

            Object.DestroyImmediate(textGo);
            Object.DestroyImmediate(root);
        }

        [Test]
        public void RotateCharacter_RotatesAllConfiguredArtPivots()
        {
            GameObject root = new GameObject("HouseholdMakerRotateRoot");
            HouseholdMakerScreenController controller = root.AddComponent<HouseholdMakerScreenController>();
            SetPrivateField(controller, "rotateAllArtPivots", true);
            SetPrivateField(controller, "rotateSpeed", 180f);

            GameObject pivotAObject = new GameObject("PivotA");
            GameObject pivotBObject = new GameObject("PivotB");
            Transform pivotA = pivotAObject.transform;
            Transform pivotB = pivotBObject.transform;
            SetPrivateField(controller, "characterArtPivots", new System.Collections.Generic.List<Transform> { pivotA, pivotB });

            controller.RotateCharacter(1f);

            Assert.AreNotEqual(0f, pivotA.eulerAngles.y);
            Assert.AreNotEqual(0f, pivotB.eulerAngles.y);

            Object.DestroyImmediate(pivotAObject);
            Object.DestroyImmediate(pivotBObject);
            Object.DestroyImmediate(root);
        }

        [Test]
        public void ValidateHouseholdGenetics_GeneratesMissingSeedsForMembers()
        {
            GameObject root = new GameObject("HouseholdMakerGeneticsRoot");
            HouseholdMakerScreenController controller = root.AddComponent<HouseholdMakerScreenController>();
            HouseholdManager household = root.AddComponent<HouseholdManager>();
            SetPrivateField(controller, "householdManager", household);

            GameObject memberObject = new GameObject("Member");
            CharacterCore member = memberObject.AddComponent<CharacterCore>();
            member.Initialize("m1", "Mina", Survivebest.Core.LifeStage.YoungAdult);
            GeneticsSystem genetics = memberObject.AddComponent<GeneticsSystem>();
            genetics.OverrideGenetics(new GeneticProfile { Seed = 0 }, reapply: false);

            household.AddMember(member);
            controller.ValidateHouseholdGenetics();

            Assert.Greater(genetics.Profile.Seed, 0);

            Object.DestroyImmediate(memberObject);
            Object.DestroyImmediate(root);
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
    }
}

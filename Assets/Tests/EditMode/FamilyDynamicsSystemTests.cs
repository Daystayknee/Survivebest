using NUnit.Framework;
using UnityEngine;
using System.Reflection;
using Survivebest.Core;
using Survivebest.Social;

namespace Survivebest.Tests.EditMode
{
    public class FamilyDynamicsSystemTests
    {
        [Test]
        public void RegisterSpouse_CreatesReciprocalSpouseLinksAndBond()
        {
            GameObject go = new GameObject("FamilyDynamicsSpouse");
            FamilyDynamicsSystem system = go.AddComponent<FamilyDynamicsSystem>();

            system.RegisterSpouse("a", "b");

            Assert.AreEqual(2, system.FamilyNodes.Count);
            FamilyNode aNode = FindNode(system, "a");
            FamilyNode bNode = FindNode(system, "b");
            Assert.AreEqual("b", aNode.Spouse);
            Assert.AreEqual("a", bNode.Spouse);

            FamilyBondProfile bond = system.GetBond("a", "b");
            Assert.NotNull(bond);
            Assert.GreaterOrEqual(bond.Affection, 55f);

            Object.DestroyImmediate(go);
        }

        [Test]
        public void ApplyFamilySupport_IncreasesTrustAndLowersResentment()
        {
            GameObject go = new GameObject("FamilyDynamicsSupport");
            FamilyDynamicsSystem system = go.AddComponent<FamilyDynamicsSystem>();

            system.RegisterGuardian("p", "c");
            FamilyBondProfile before = system.GetBond("p", "c");
            float trustBefore = before.Trust;
            float resentmentBefore = before.Resentment;

            system.ApplyFamilySupport("p", "c", 1f);

            FamilyBondProfile after = system.GetBond("p", "c");
            Assert.Greater(after.Trust, trustBefore);
            Assert.Less(after.Resentment, resentmentBefore);
            Object.DestroyImmediate(go);
        }

        [Test]
        public void ApplyFamilyConflict_IncreasesResentmentAndReducesTrust()
        {
            GameObject go = new GameObject("FamilyDynamicsConflict");
            FamilyDynamicsSystem system = go.AddComponent<FamilyDynamicsSystem>();

            SocialDramaEngine drama = go.AddComponent<SocialDramaEngine>();
            SetPrivateField(system, "socialDramaEngine", drama);

            system.RegisterGuardian("p", "c");
            FamilyBondProfile before = system.GetBond("p", "c");
            float trustBefore = before.Trust;
            float resentmentBefore = before.Resentment;

            system.ApplyFamilyConflict("p", "c", 1f);

            FamilyBondProfile after = system.GetBond("p", "c");
            Assert.Less(after.Trust, trustBefore);
            Assert.Greater(after.Resentment, resentmentBefore);
            Assert.GreaterOrEqual(drama.SocialSignals.Count, 1);
            Object.DestroyImmediate(go);
        }

        private static FamilyNode FindNode(FamilyDynamicsSystem system, string id)
        {
            for (int i = 0; i < system.FamilyNodes.Count; i++)
            {
                FamilyNode node = system.FamilyNodes[i];
                if (node != null && node.CharacterId == id)
                {
                    return node;
                }
            }

            return null;
        }

        private static void SetPrivateField(object instance, string fieldName, object value)
        {
            FieldInfo field = instance.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            field?.SetValue(instance, value);
        }
    }
}

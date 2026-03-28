using NUnit.Framework;
using UnityEngine;
using Survivebest.Appearance;
using Survivebest.Core;

namespace Survivebest.Tests.EditMode
{
    public class TattooSystemTests
    {
        [Test]
        public void AddTattooForLifeStage_BlocksTeenButAllowsYoungAdult()
        {
            GameObject go = new GameObject("TattooSystem");
            TattooSystem system = go.AddComponent<TattooSystem>();

            bool teenResult = system.AddTattooForLifeStage("char_teen", LifeStage.Teen, "tiny_star", "milestone");
            bool adultResult = system.AddTattooForLifeStage("char_adult", LifeStage.YoungAdult, "phoenix", "rebirth");

            Assert.IsFalse(teenResult);
            Assert.IsTrue(adultResult);

            Object.DestroyImmediate(go);
        }

        [Test]
        public void AddPiercingForLifeStage_EnforcesYouthSafetyRules()
        {
            GameObject go = new GameObject("PiercingSystem");
            TattooSystem system = go.AddComponent<TattooSystem>();

            bool childEar = system.AddPiercingForLifeStage("child", LifeStage.Child, "lobe_stud", "left_ear_lobe");
            bool childNose = system.AddPiercingForLifeStage("child", LifeStage.Child, "nose_stud", "left_nose");
            bool teenNose = system.AddPiercingForLifeStage("teen", LifeStage.Teen, "nose_stud", "left_nose");
            bool teenLip = system.AddPiercingForLifeStage("teen", LifeStage.Teen, "lip_ring", "lower_lip");
            bool adultLip = system.AddPiercingForLifeStage("adult", LifeStage.Adult, "lip_ring", "lower_lip");

            Assert.IsTrue(childEar);
            Assert.IsFalse(childNose);
            Assert.IsTrue(teenNose);
            Assert.IsFalse(teenLip);
            Assert.IsTrue(adultLip);

            Object.DestroyImmediate(go);
        }
    }
}

using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using Survivebest.Appearance;
using Survivebest.Core;
using Survivebest.World;

namespace Survivebest.Tests.EditMode
{
    public class CharacterAppearanceDiversityTests
    {
        [Test]
        public void CharacterCore_SetFeatureExpression_ClampsAcrossFeminineMasculineAndrogynousRanges()
        {
            GameObject go = new GameObject("Character");
            CharacterCore character = go.AddComponent<CharacterCore>();

            character.SetFeatureExpression(1.8f, -0.3f, 0.74f);

            Assert.AreEqual(1f, character.FeminineExpression, 0.001f);
            Assert.AreEqual(0f, character.MasculineExpression, 0.001f);
            Assert.AreEqual(0.74f, character.AndrogynyExpression, 0.001f);

            Object.DestroyImmediate(go);
        }

        [Test]
        public void AppearanceEnums_ExposeExpandedSkinEyeBodyAndNoseOptions()
        {
            Assert.IsTrue(System.Enum.IsDefined(typeof(SkinToneType), "Ebony"));
            Assert.IsTrue(System.Enum.IsDefined(typeof(SkinToneType), "Alabaster"));
            Assert.IsTrue(System.Enum.IsDefined(typeof(EyeColorType), "DarkBrown"));
            Assert.IsTrue(System.Enum.IsDefined(typeof(EyeColorType), "Violet"));
            Assert.IsTrue(System.Enum.IsDefined(typeof(BodyType), "Athletic"));
            Assert.IsTrue(System.Enum.IsDefined(typeof(BodyType), "PlusSize"));
            Assert.IsTrue(System.Enum.IsDefined(typeof(NoseShapeType), "Roman"));
            Assert.IsTrue(System.Enum.IsDefined(typeof(NoseShapeType), "Nubian"));
        }

        [Test]
        public void GeneticsSystem_MappingSupportsExpandedSkinAndEyeBands()
        {
            MethodInfo toSkinTone = typeof(GeneticsSystem).GetMethod("ToSkinTone", BindingFlags.NonPublic | BindingFlags.Static);
            MethodInfo toEyeColor = typeof(GeneticsSystem).GetMethod("ToEyeColor", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.NotNull(toSkinTone);
            Assert.NotNull(toEyeColor);

            SkinToneType deepTone = (SkinToneType)toSkinTone.Invoke(null, new object[] { 0.99f });
            EyeColorType eyeColor = (EyeColorType)toEyeColor.Invoke(null, new object[] { 0.93f });

            Assert.AreEqual(SkinToneType.Ebony, deepTone);
            Assert.AreEqual(EyeColorType.DarkBrown, eyeColor);
        }
    }
}

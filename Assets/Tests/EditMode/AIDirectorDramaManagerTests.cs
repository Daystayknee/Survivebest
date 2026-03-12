using NUnit.Framework;
using UnityEngine;
using Survivebest.Core;

namespace Survivebest.Tests.EditMode
{
    public class AIDirectorDramaManagerTests
    {
        [Test]
        public void RegisterMeaningfulBeat_IncreasesTension()
        {
            GameObject go = new GameObject("AIDirectorTest");
            AIDirectorDramaManager manager = go.AddComponent<AIDirectorDramaManager>();

            float before = manager.Tension;
            manager.RegisterMeaningfulBeat(22f);

            Assert.Greater(manager.Tension, before);

            Object.DestroyImmediate(go);
        }
    }
}

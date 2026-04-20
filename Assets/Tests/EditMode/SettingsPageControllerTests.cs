using NUnit.Framework;
using UnityEngine;
using Survivebest.UI;

namespace Survivebest.Tests.EditMode
{
    public class SettingsPageControllerTests
    {
        [SetUp]
        public void SetUp()
        {
            PlayerPrefs.DeleteAll();
        }

        [Test]
        public void SettingsPageController_ClampsUiScaleAndKeepsGameHourFloor()
        {
            GameObject go = new("SettingsController");
            SettingsPageController controller = go.AddComponent<SettingsPageController>();

            controller.SetUiScale(8f);
            controller.SetRealSecondsPerGameHour(1f);

            Assert.AreEqual(3f, controller.CurrentSettings.UiScale, 0.001f);
            Assert.GreaterOrEqual(controller.CurrentSettings.RealSecondsPerGameHour, 6f);

            Object.DestroyImmediate(go);
        }

        [Test]
        public void SettingsPageController_ResetDefaults_RestoresBaseValues()
        {
            GameObject go = new("SettingsControllerDefaults");
            SettingsPageController controller = go.AddComponent<SettingsPageController>();

            controller.SetMasterVolume(0.2f);
            controller.SetFullscreen(false);
            controller.ResetDefaults();

            Assert.AreEqual(1f, controller.CurrentSettings.MasterVolume, 0.001f);
            Assert.IsTrue(controller.CurrentSettings.Fullscreen);

            Object.DestroyImmediate(go);
        }
    }
}

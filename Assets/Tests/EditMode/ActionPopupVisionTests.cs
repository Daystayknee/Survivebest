using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using Survivebest.Location;
using Survivebest.UI;

namespace Survivebest.Tests.EditMode
{
    public class ActionPopupVisionTests
    {
        [Test]
        public void VisionAwarePopupContent_IncludesMoodAndSectionTabs()
        {
            GameObject go = new GameObject("PopupVision");
            ActionPopupController popup = go.AddComponent<ActionPopupController>();
            GameplayVisionSystem vision = go.AddComponent<GameplayVisionSystem>();
            LocationManager location = go.AddComponent<LocationManager>();

            typeof(ActionPopupController).GetField("gameplayVisionSystem", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(popup, vision);
            typeof(ActionPopupController).GetField("locationManager", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(popup, location);
            typeof(LocationManager).GetProperty("CurrentRoom", BindingFlags.Public | BindingFlags.Instance)
                ?.SetValue(location, new Room { RoomName = "Kitchen", Theme = LocationTheme.Residential });

            MethodInfo buildDescription = typeof(ActionPopupController).GetMethod("BuildVisionAwareDescription", BindingFlags.NonPublic | BindingFlags.Instance);
            MethodInfo buildOptions = typeof(ActionPopupController).GetMethod("BuildVisionAwareOptions", BindingFlags.NonPublic | BindingFlags.Instance);

            string description = (string)buildDescription.Invoke(popup, new object[] { "cook_meal" });
            string options = (string)buildOptions.Invoke(popup, new object[] { "cook_meal" });

            StringAssert.Contains("Cooking mode", description);
            StringAssert.Contains("Minigame hook", description);
            StringAssert.Contains("Section tabs:", options);
            StringAssert.Contains("Prep", options);
            StringAssert.Contains("Serve", options);

            Object.DestroyImmediate(go);
        }
    }
}

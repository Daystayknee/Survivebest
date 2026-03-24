using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using Survivebest.UI;

namespace Survivebest.Tests.EditMode
{
    public class SidebarContextMenuTests
    {
        [Test]
        public void DeduplicateOptionsByActionKey_RemovesDuplicateActionKeys()
        {
            GameObject go = new GameObject("SidebarMenu");
            SidebarContextMenu menu = go.AddComponent<SidebarContextMenu>();

            FieldInfo optionsField = typeof(SidebarContextMenu)
                .GetField("currentOptions", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(optionsField);

            List<SidebarOption> options = optionsField.GetValue(menu) as List<SidebarOption>;
            Assert.NotNull(options);
            options.Clear();
            options.AddRange(new[]
            {
                new SidebarOption { Label = "Talk", ActionKey = "npc_chat" },
                new SidebarOption { Label = "Chat Again", ActionKey = "npc_chat" },
                new SidebarOption { Label = "Rest", ActionKey = "rest" }
            });

            typeof(SidebarContextMenu)
                .GetMethod("DeduplicateOptionsByActionKey", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.Invoke(menu, null);

            Assert.AreEqual(2, menu.CurrentOptions.Count);
            Assert.AreEqual("npc_chat", menu.CurrentOptions[0].ActionKey);
            Assert.AreEqual("rest", menu.CurrentOptions[1].ActionKey);

            Object.DestroyImmediate(go);
        }

        [Test]
        public void EnsureMinimumFallbackOptions_AddsRestAndNeedsWhenEmpty()
        {
            GameObject go = new GameObject("SidebarMenuFallback");
            SidebarContextMenu menu = go.AddComponent<SidebarContextMenu>();

            typeof(SidebarContextMenu)
                .GetMethod("EnsureMinimumFallbackOptions", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.Invoke(menu, null);

            Assert.AreEqual(2, menu.CurrentOptions.Count);
            Assert.AreEqual("rest", menu.CurrentOptions[0].ActionKey);
            Assert.AreEqual("check_needs", menu.CurrentOptions[1].ActionKey);

            Object.DestroyImmediate(go);
        }
    }
}

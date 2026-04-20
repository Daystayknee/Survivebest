using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using Survivebest.UI;

namespace Survivebest.Tests.EditMode
{
    public class LoadingAndMenuFlowTests
    {
        [Test]
        public void LoadingScreenController_CompleteLoading_TransitionsToGameplayAndFillsBar()
        {
            GameObject root = new("LoadingScreenRoot");
            MainMenuFlowController flow = root.AddComponent<MainMenuFlowController>();
            LoadingScreenController loading = root.AddComponent<LoadingScreenController>();

            List<ScreenBinding> bindings = new()
            {
                NewBinding(MainMenuScreen.Splash),
                NewBinding(MainMenuScreen.Loading),
                NewBinding(MainMenuScreen.MainMenu),
                NewBinding(MainMenuScreen.Gameplay)
            };

            SetPrivate(flow, "screenBindings", bindings);
            SetPrivate(flow, "initialScreen", MainMenuScreen.Splash);
            SetPrivate(loading, "menuFlowController", flow);

            GameObject fillGo = new("FillImage");
            Image fill = fillGo.AddComponent<Image>();
            SetPrivate(loading, "loadingFill", fill);

            InvokePrivate(flow, "Start");
            flow.StartGameplayWithLoading();
            Assert.AreEqual(MainMenuScreen.Loading, flow.CurrentScreen);

            loading.CompleteLoading();

            Assert.AreEqual(1f, loading.Progress01, 0.0001f);
            Assert.AreEqual(MainMenuScreen.Gameplay, flow.CurrentScreen);

            Object.DestroyImmediate(fillGo);
            Object.DestroyImmediate(root);
            DestroyBindings(bindings);
        }

        [Test]
        public void MainMenuFlowController_StartGameplayWithLoading_GoesThroughLoadingScreen()
        {
            GameObject root = new("MenuFlowRoot");
            MainMenuFlowController flow = root.AddComponent<MainMenuFlowController>();

            List<ScreenBinding> bindings = new()
            {
                NewBinding(MainMenuScreen.Splash),
                NewBinding(MainMenuScreen.Loading),
                NewBinding(MainMenuScreen.Gameplay)
            };

            SetPrivate(flow, "screenBindings", bindings);
            SetPrivate(flow, "initialScreen", MainMenuScreen.Splash);
            InvokePrivate(flow, "Start");

            flow.StartGameplayWithLoading();
            Assert.AreEqual(MainMenuScreen.Loading, flow.CurrentScreen);

            flow.StartGameplay();
            Assert.AreEqual(MainMenuScreen.Gameplay, flow.CurrentScreen);

            Object.DestroyImmediate(root);
            DestroyBindings(bindings);
        }

        private static ScreenBinding NewBinding(MainMenuScreen screen)
        {
            return new ScreenBinding
            {
                Screen = screen,
                Root = new GameObject($"Screen_{screen}")
            };
        }

        private static void DestroyBindings(List<ScreenBinding> bindings)
        {
            for (int i = 0; i < bindings.Count; i++)
            {
                if (bindings[i]?.Root != null)
                {
                    Object.DestroyImmediate(bindings[i].Root);
                }
            }
        }

        private static void SetPrivate(object target, string fieldName, object value)
        {
            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(field, $"Missing private field '{fieldName}' on {target.GetType().Name}.");
            field.SetValue(target, value);
        }

        private static void InvokePrivate(object target, string methodName)
        {
            MethodInfo method = target.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(method, $"Missing private method '{methodName}' on {target.GetType().Name}.");
            method.Invoke(target, null);
        }
    }
}

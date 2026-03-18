using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.UI
{
    public class Graphic : MonoBehaviour
    {
        public Color color;
    }

    public class Text : Graphic
    {
        public string text;
    }

    public class Image : Graphic
    {
        public Sprite sprite;
    }

    public class Slider : MonoBehaviour
    {
        public float value;
    }

    public class Button : MonoBehaviour
    {
        public bool interactable = true;
        public ButtonClickedEvent onClick = new();
    }

    public class ButtonClickedEvent
    {
        private event Action Clicked;

        public void AddListener(Action callback)
        {
            Clicked += callback;
        }

        public void RemoveAllListeners()
        {
            Clicked = null;
        }

        public void Invoke()
        {
            Clicked?.Invoke();
        }
    }
}

namespace TMPro
{
    public class TMP_Text : UnityEngine.UI.Text
    {
    }
}

namespace NUnit.Framework
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class TestAttribute : Attribute
    {
    }

    public static class CollectionAssert
    {
        public static void Contains<T>(IEnumerable<T> collection, T expected)
        {
            if (collection == null)
            {
                throw new Exception("Collection expected to be non-null.");
            }

            foreach (T item in collection)
            {
                if (EqualityComparer<T>.Default.Equals(item, expected))
                {
                    return;
                }
            }

            throw new Exception($"Expected collection to contain '{expected}'.");
        }
    }

    public static class StringAssert
    {
        public static void Contains(string expected, string actual)
        {
            if (actual == null || expected == null || !actual.Contains(expected, StringComparison.Ordinal))
            {
                throw new Exception($"Expected '{actual}' to contain '{expected}'.");
            }
        }
    }

    public static class Assert
    {
        public static void AreEqual<T>(T expected, T actual)
        {
            if (!EqualityComparer<T>.Default.Equals(expected, actual))
            {
                throw new Exception($"Expected: {expected} Actual: {actual}");
            }
        }

        public static void AreNotEqual<T>(T notExpected, T actual)
        {
            if (EqualityComparer<T>.Default.Equals(notExpected, actual))
            {
                throw new Exception($"Did not expect: {actual}");
            }
        }

        public static void AreSame(object expected, object actual)
        {
            if (!ReferenceEquals(expected, actual))
            {
                throw new Exception("Objects are not the same instance.");
            }
        }

        public static void IsTrue(bool condition)
        {
            if (!condition)
            {
                throw new Exception("Condition expected to be true.");
            }
        }

        public static void IsFalse(bool condition)
        {
            if (condition)
            {
                throw new Exception("Condition expected to be false.");
            }
        }

        public static void IsNotNull(object value)
        {
            if (value == null)
            {
                throw new Exception("Value expected to be non-null.");
            }
        }

        public static void NotNull(object value) => IsNotNull(value);

        public static void IsNotEmpty(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new Exception("String expected to be non-empty.");
            }
        }

        public static void Contains(string substring, string actual)
        {
            if (actual == null || substring == null || !actual.Contains(substring, StringComparison.Ordinal))
            {
                throw new Exception($"Expected '{actual}' to contain '{substring}'.");
            }
        }

        public static void Greater(float arg1, float arg2)
        {
            if (!(arg1 > arg2))
            {
                throw new Exception($"Expected {arg1} to be greater than {arg2}.");
            }
        }

        public static void Greater(int arg1, int arg2)
        {
            if (!(arg1 > arg2))
            {
                throw new Exception($"Expected {arg1} to be greater than {arg2}.");
            }
        }

        public static void GreaterOrEqual(float arg1, float arg2)
        {
            if (!(arg1 >= arg2))
            {
                throw new Exception($"Expected {arg1} to be greater than or equal to {arg2}.");
            }
        }

        public static void GreaterOrEqual(int arg1, int arg2)
        {
            if (!(arg1 >= arg2))
            {
                throw new Exception($"Expected {arg1} to be greater than or equal to {arg2}.");
            }
        }

        public static void Less(float arg1, float arg2)
        {
            if (!(arg1 < arg2))
            {
                throw new Exception($"Expected {arg1} to be less than {arg2}.");
            }
        }

        public static void LessOrEqual(float arg1, float arg2)
        {
            if (!(arg1 <= arg2))
            {
                throw new Exception($"Expected {arg1} to be less than or equal to {arg2}.");
            }
        }

        public static void Pass(string message = null)
        {
        }
    }
}

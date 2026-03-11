using System;
using UnityEngine;
using Survivebest.Core;
using Survivebest.Needs;

namespace Survivebest.Emotion
{
    public class EmotionSystem : MonoBehaviour
    {
        [SerializeField] private CharacterCore owner;
        [SerializeField] private NeedsSystem needsSystem;

        [SerializeField, Range(0f, 100f)] private float anger;
        [SerializeField, Range(0f, 100f)] private float affection;
        [SerializeField, Range(0f, 100f)] private float stress;

        public event Action<float> OnAngerChanged;
        public event Action<float> OnAffectionChanged;
        public event Action<float> OnStressChanged;

        public float Anger => anger;
        public float Affection => affection;
        public float Stress => stress;

        public void ModifyAnger(float amount)
        {
            anger = Mathf.Clamp(anger + amount, 0f, 100f);
            OnAngerChanged?.Invoke(anger);
            if (needsSystem != null)
            {
                needsSystem.ModifyMood(-amount * 0.2f);
            }
        }

        public void ModifyAffection(float amount)
        {
            affection = Mathf.Clamp(affection + amount, 0f, 100f);
            OnAffectionChanged?.Invoke(affection);
            if (needsSystem != null)
            {
                needsSystem.ModifyMood(amount * 0.15f);
            }
        }

        public void ModifyStress(float amount)
        {
            stress = Mathf.Clamp(stress + amount, 0f, 100f);
            OnStressChanged?.Invoke(stress);
            if (needsSystem != null)
            {
                needsSystem.ModifyMood(-amount * 0.1f);
            }
        }

        public bool IsReadyToFight() => anger >= 70f || stress >= 80f;
        public bool IsInLoveState() => affection >= 75f && anger < 40f;
    }
}

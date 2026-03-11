using UnityEngine;

namespace Survivebest.World
{
    [System.Serializable]
    public struct PhysicalTraits
    {
        public float NeckLength;
        public float ShoulderWidth;
        public float BustSize;
        public float ChestDepth;
        public float ArmThickness;
        public float HipWidth;
        public float BootySize;
        public float ThighGirth;
        public float CalfGirth;
        public float Height;
    }

    public class VisualGenome : MonoBehaviour
    {
        [SerializeField] private Transform neck;
        [SerializeField] private Transform chest;
        [SerializeField] private Transform hips;
        [SerializeField] private Transform arms;
        [SerializeField] private Transform thighs;
        [SerializeField] private Transform calves;
        [SerializeField] private Transform root;

        [SerializeField] private float minHumanScale = 0.8f;
        [SerializeField] private float maxHumanScale = 1.3f;

        public PhysicalTraits CurrentTraits { get; private set; }

        public void ApplyPhysicalTraits(PhysicalTraits traits)
        {
            CurrentTraits = traits;

            if (neck != null) neck.localScale = new Vector3(1f, traits.NeckLength, 1f);
            if (chest != null) chest.localScale = new Vector3(traits.ShoulderWidth, traits.BustSize, traits.ChestDepth);
            if (hips != null) hips.localScale = new Vector3(traits.HipWidth, 1f, traits.BootySize);
            if (arms != null) arms.localScale = new Vector3(traits.ArmThickness, 1f, 1f);
            if (thighs != null) thighs.localScale = new Vector3(traits.ThighGirth, 1f, 1f);
            if (calves != null) calves.localScale = new Vector3(traits.CalfGirth, 1f, 1f);
            if (root != null) root.localScale = new Vector3(1f, traits.Height, 1f);
        }

        public PhysicalTraits GenerateRandomDNA()
        {
            return new PhysicalTraits
            {
                NeckLength = Random.Range(minHumanScale, maxHumanScale),
                ShoulderWidth = Random.Range(minHumanScale, maxHumanScale),
                BustSize = Random.Range(minHumanScale, maxHumanScale),
                ChestDepth = Random.Range(minHumanScale, maxHumanScale),
                ArmThickness = Random.Range(minHumanScale, maxHumanScale),
                HipWidth = Random.Range(minHumanScale, maxHumanScale),
                BootySize = Random.Range(minHumanScale, maxHumanScale),
                ThighGirth = Random.Range(minHumanScale, maxHumanScale),
                CalfGirth = Random.Range(minHumanScale, maxHumanScale),
                Height = Random.Range(minHumanScale, maxHumanScale)
            };
        }

        public PhysicalTraits InheritTraits(PhysicalTraits parentA, PhysicalTraits parentB)
        {
            PhysicalTraits child = new PhysicalTraits
            {
                NeckLength = Blend(parentA.NeckLength, parentB.NeckLength),
                ShoulderWidth = Blend(parentA.ShoulderWidth, parentB.ShoulderWidth),
                BustSize = Blend(parentA.BustSize, parentB.BustSize),
                ChestDepth = Blend(parentA.ChestDepth, parentB.ChestDepth),
                ArmThickness = Blend(parentA.ArmThickness, parentB.ArmThickness),
                HipWidth = Blend(parentA.HipWidth, parentB.HipWidth),
                BootySize = Blend(parentA.BootySize, parentB.BootySize),
                ThighGirth = Blend(parentA.ThighGirth, parentB.ThighGirth),
                CalfGirth = Blend(parentA.CalfGirth, parentB.CalfGirth),
                Height = Blend(parentA.Height, parentB.Height)
            };

            return child;
        }

        private float Blend(float a, float b)
        {
            float value = (a + b) * 0.5f;
            if (Random.value <= 0.1f)
            {
                value += Random.Range(-0.05f, 0.05f);
            }

            return Mathf.Clamp(value, minHumanScale, maxHumanScale);
        }
    }
}

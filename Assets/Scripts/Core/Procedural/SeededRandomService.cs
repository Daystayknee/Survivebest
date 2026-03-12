using System;
using UnityEngine;

namespace Survivebest.Core.Procedural
{
    public sealed class SeededRandomService : IRandomService
    {
        private readonly System.Random random;

        public int Seed { get; }

        public SeededRandomService(int seed)
        {
            Seed = seed;
            random = new System.Random(seed);
        }

        public int NextInt(int minInclusive, int maxExclusive)
        {
            if (maxExclusive <= minInclusive)
            {
                return minInclusive;
            }

            return random.Next(minInclusive, maxExclusive);
        }

        public float NextFloat()
        {
            return (float)random.NextDouble();
        }

        public bool Roll(float chance)
        {
            float clampedChance = Mathf.Clamp01(chance);
            return NextFloat() <= clampedChance;
        }
    }
}

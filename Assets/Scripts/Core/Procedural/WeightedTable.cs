using System;
using System.Collections.Generic;

namespace Survivebest.Core.Procedural
{
    [Serializable]
    public sealed class WeightedOption<T>
    {
        public T Value;
        public float Weight;
    }

    [Serializable]
    public sealed class WeightedTable<T>
    {
        private readonly List<WeightedOption<T>> options = new();

        public IReadOnlyList<WeightedOption<T>> Options => options;

        public void AddOption(T value, float weight)
        {
            options.Add(new WeightedOption<T>
            {
                Value = value,
                Weight = weight
            });
        }

        public T Pick(IRandomService randomService)
        {
            if (randomService == null || options.Count == 0)
            {
                return default;
            }

            float totalWeight = 0f;
            for (int i = 0; i < options.Count; i++)
            {
                if (options[i] != null && options[i].Weight > 0f)
                {
                    totalWeight += options[i].Weight;
                }
            }

            if (totalWeight <= 0f)
            {
                return options[options.Count - 1] != null ? options[options.Count - 1].Value : default;
            }

            float roll = randomService.NextFloat() * totalWeight;
            float cumulative = 0f;
            for (int i = 0; i < options.Count; i++)
            {
                WeightedOption<T> option = options[i];
                if (option == null || option.Weight <= 0f)
                {
                    continue;
                }

                cumulative += option.Weight;
                if (roll <= cumulative)
                {
                    return option.Value;
                }
            }

            return options[options.Count - 1] != null ? options[options.Count - 1].Value : default;
        }
    }
}

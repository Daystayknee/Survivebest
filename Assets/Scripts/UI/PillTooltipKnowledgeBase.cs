using System.Collections.Generic;

namespace Survivebest.UI
{
    public static class PillTooltipKnowledgeBase
    {
        private static readonly List<string> cachedHints = BuildHints();

        public static IReadOnlyList<string> AllHints => cachedHints;

        public static string GetHint(int seed)
        {
            if (cachedHints.Count == 0)
            {
                return "Keep needs balanced to protect mood over time.";
            }

            int index = seed < 0 ? -seed : seed;
            return cachedHints[index % cachedHints.Count];
        }

        private static List<string> BuildHints()
        {
            string[] metrics =
            {
                "mood", "hunger", "energy", "hydration", "hygiene", "focus", "stress", "burnout", "cravings", "pain"
            };

            string[] windows = { "this hour", "next 2 hours", "next 4 hours", "next 8 hours", "today", "overnight" };
            string[] actions =
            {
                "eat a full meal", "drink water", "rest briefly", "take a long sleep", "shower and reset", "socialize", "stretch", "take medicine", "reduce workload", "choose a comfort activity"
            };

            string[] outcomes =
            {
                "stabilize mood", "reduce penalties", "prevent negative status stacks", "improve recovery", "avoid compounding stress", "keep daily rhythm smooth"
            };

            List<string> hints = new(metrics.Length * windows.Length * actions.Length);
            int serial = 1;
            for (int m = 0; m < metrics.Length; m++)
            {
                for (int w = 0; w < windows.Length; w++)
                {
                    for (int a = 0; a < actions.Length; a++)
                    {
                        string outcome = outcomes[(m + w + a) % outcomes.Length];
                        hints.Add($"Hint {serial++}: Watch {metrics[m]} {windows[w]} and {actions[a]} to {outcome}.");
                    }
                }
            }

            return hints;
        }
    }
}

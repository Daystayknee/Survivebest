using UnityEngine;

namespace Survivebest.NPC
{
    public struct AutonomyDecisionContext
    {
        public NpcActivityState ScheduledActivity;
        public float Hunger;
        public float Energy;
        public float Mood;
        public float Stress;
        public float Loneliness;
        public float SocialBattery;
        public float RelationshipAffinity;
        public float MemorySentiment;
        public bool SevereWeather;
    }

    public struct AutonomyDecisionResult
    {
        public NpcActivityState Activity;
        public float SocialDrive;
        public string Reason;
    }

    public static class AutonomyDecisionEngine
    {
        public static AutonomyDecisionResult Decide(AutonomyDecisionContext context)
        {
            NpcActivityState chosen = context.ScheduledActivity;
            float socialDrive = ComputeSocialDrive(context);

            if (context.Hunger < 25f)
            {
                return BuildResult(NpcActivityState.Eating, socialDrive, "Critical hunger");
            }

            if (context.Energy < 20f)
            {
                return BuildResult(NpcActivityState.Sleeping, socialDrive, "Critical fatigue");
            }

            if (context.Stress > 85f)
            {
                return BuildResult(NpcActivityState.SickRest, socialDrive, "Overwhelming stress");
            }

            if (socialDrive > 0.6f)
            {
                chosen = NpcActivityState.Socializing;
            }
            else if (context.Mood < 35f && context.Stress > 65f)
            {
                chosen = NpcActivityState.Idle;
            }

            if (context.SevereWeather && chosen == NpcActivityState.Socializing)
            {
                chosen = context.Energy < 30f ? NpcActivityState.Sleeping : NpcActivityState.Idle;
            }

            return BuildResult(chosen, socialDrive, "Weighted autonomy blend");
        }

        private static float ComputeSocialDrive(AutonomyDecisionContext context)
        {
            float lonelinessPressure = Mathf.InverseLerp(20f, 90f, context.Loneliness);
            float batteryReadiness = Mathf.InverseLerp(10f, 85f, context.SocialBattery);
            float memoryBias = Mathf.InverseLerp(-100f, 100f, context.MemorySentiment);
            float affinityBias = Mathf.InverseLerp(-100f, 100f, context.RelationshipAffinity);
            float stressPenalty = Mathf.InverseLerp(45f, 95f, context.Stress);

            return Mathf.Clamp01(
                lonelinessPressure * 0.38f +
                batteryReadiness * 0.22f +
                memoryBias * 0.2f +
                affinityBias * 0.2f -
                stressPenalty * 0.28f);
        }

        private static AutonomyDecisionResult BuildResult(NpcActivityState activity, float socialDrive, string reason)
        {
            return new AutonomyDecisionResult
            {
                Activity = activity,
                SocialDrive = socialDrive,
                Reason = reason
            };
        }
    }
}

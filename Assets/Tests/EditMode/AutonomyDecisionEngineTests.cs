using NUnit.Framework;
using Survivebest.NPC;

namespace Survivebest.Tests.EditMode
{
    public class AutonomyDecisionEngineTests
    {
        [Test]
        public void Decide_CriticalHunger_PrioritizesEating()
        {
            AutonomyDecisionResult result = AutonomyDecisionEngine.Decide(new AutonomyDecisionContext
            {
                ScheduledActivity = NpcActivityState.Working,
                Hunger = 10f,
                Energy = 90f,
                Mood = 55f,
                Stress = 35f,
                Loneliness = 50f,
                SocialBattery = 70f,
                RelationshipAffinity = 40f,
                MemorySentiment = 20f
            });

            Assert.AreEqual(NpcActivityState.Eating, result.Activity);
        }

        [Test]
        public void Decide_SocialDriveHigh_PrefersSocializing()
        {
            AutonomyDecisionResult result = AutonomyDecisionEngine.Decide(new AutonomyDecisionContext
            {
                ScheduledActivity = NpcActivityState.Idle,
                Hunger = 70f,
                Energy = 75f,
                Mood = 60f,
                Stress = 25f,
                Loneliness = 85f,
                SocialBattery = 85f,
                RelationshipAffinity = 70f,
                MemorySentiment = 80f
            });

            Assert.AreEqual(NpcActivityState.Socializing, result.Activity);
            Assert.Greater(result.SocialDrive, 0.6f);
        }

        [Test]
        public void Decide_SevereWeatherBlocksSocializing()
        {
            AutonomyDecisionResult result = AutonomyDecisionEngine.Decide(new AutonomyDecisionContext
            {
                ScheduledActivity = NpcActivityState.Socializing,
                Hunger = 65f,
                Energy = 60f,
                Mood = 65f,
                Stress = 30f,
                Loneliness = 90f,
                SocialBattery = 80f,
                RelationshipAffinity = 80f,
                MemorySentiment = 75f,
                SevereWeather = true
            });

            Assert.AreNotEqual(NpcActivityState.Socializing, result.Activity);
        }
    }
}

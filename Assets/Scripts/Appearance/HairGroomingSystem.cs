using UnityEngine;
using Survivebest.World;

namespace Survivebest.Appearance
{
    public class HairGroomingSystem : MonoBehaviour
    {
        [SerializeField] private WorldClock worldClock;
        [SerializeField] private AppearanceManager appearanceManager;
        [SerializeField] private HairGrowthRules growthRules = new();

        private int currentAbsoluteDay;

        private void OnEnable()
        {
            if (worldClock != null)
            {
                worldClock.OnHourPassed += HandleHourPassed;
            }

            SyncDay();
        }

        private void OnDisable()
        {
            if (worldClock != null)
            {
                worldClock.OnHourPassed -= HandleHourPassed;
            }
        }

        public void TrimHairToStage(HairGrowthStage stage)
        {
            if (appearanceManager == null)
            {
                return;
            }

            HairProfile profile = appearanceManager.ScalpHairProfile;
            profile.GrowthStage = stage;
            profile.LastTrimDay = currentAbsoluteDay;
            appearanceManager.SetHairProfile(profile);
        }

        public void ShaveFacialHair()
        {
            if (appearanceManager == null)
            {
                return;
            }

            FacialHairProfile profile = appearanceManager.FacialHairProfile;
            profile.MustacheStage = BeardGrowthStage.None;
            profile.BeardStage = BeardGrowthStage.None;
            profile.SideburnStage = BeardGrowthStage.None;
            profile.NeckBeardStage = BeardGrowthStage.None;
            profile.LastShaveDay = currentAbsoluteDay;
            appearanceManager.SetFacialHairProfile(profile);
        }

        public void ShaveBodyHair(bool chest = true, bool arms = true, bool legs = true)
        {
            if (appearanceManager == null)
            {
                return;
            }

            BodyHairProfile profile = appearanceManager.BodyHairProfile;
            profile.IsShavedChest = chest || profile.IsShavedChest;
            profile.IsShavedArms = arms || profile.IsShavedArms;
            profile.IsShavedLegs = legs || profile.IsShavedLegs;
            profile.LastBodyShaveDay = currentAbsoluteDay;
            appearanceManager.SetBodyHairProfile(profile);
        }

        private void HandleHourPassed(int hour)
        {
            if (hour != 0)
            {
                return;
            }

            SyncDay();
            ApplyDailyRegrowth();
        }

        private void ApplyDailyRegrowth()
        {
            if (appearanceManager == null)
            {
                return;
            }

            HairProfile hair = appearanceManager.ScalpHairProfile;
            int daysSinceTrim = Mathf.Max(0, currentAbsoluteDay - hair.LastTrimDay);
            hair.GrowthStage = HairAssemblyResolver.ResolveHairGrowthStage(daysSinceTrim, growthRules);
            appearanceManager.SetHairProfile(hair);

            FacialHairProfile facial = appearanceManager.FacialHairProfile;
            if (facial.GrowthEnabled)
            {
                int daysSinceShave = Mathf.Max(0, currentAbsoluteDay - facial.LastShaveDay);
                BeardGrowthStage stage = HairAssemblyResolver.ResolveBeardGrowthStage(daysSinceShave, growthRules);
                facial.MustacheStage = stage;
                facial.BeardStage = stage;
                facial.SideburnStage = stage;
                facial.NeckBeardStage = stage > BeardGrowthStage.Short ? stage : BeardGrowthStage.None;
                appearanceManager.SetFacialHairProfile(facial);
            }

            BodyHairProfile body = appearanceManager.BodyHairProfile;
            int bodyDays = Mathf.Max(0, currentAbsoluteDay - body.LastBodyShaveDay);
            if (bodyDays >= growthRules.DaysToMedium)
            {
                body.IsShavedChest = false;
                body.IsShavedArms = false;
                body.IsShavedLegs = false;
                appearanceManager.SetBodyHairProfile(body);
            }
        }

        private void SyncDay()
        {
            if (worldClock == null)
            {
                return;
            }

            currentAbsoluteDay = ((worldClock.Year - 1) * worldClock.MonthsPerYear * worldClock.DaysPerMonth)
                + ((worldClock.Month - 1) * worldClock.DaysPerMonth)
                + (worldClock.Day - 1);
        }
    }
}

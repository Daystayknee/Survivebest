using UnityEngine;
using Survivebest.Appearance;
using Survivebest.Core;
using Survivebest.Social;

namespace Survivebest.Tasks
{
    public class TaskStateUpdater : MonoBehaviour
    {
        [SerializeField] private RelationshipMemorySystem relationshipMemorySystem;

        public void ApplyAppearanceResult(TaskResultDefinition result, CharacterCore actor)
        {
            if (result == null || actor == null || string.IsNullOrWhiteSpace(result.AppearanceActionId))
            {
                return;
            }

            AppearanceManager appearance = actor.GetComponent<AppearanceManager>();
            if (appearance == null)
            {
                return;
            }

            if (result.AppearanceActionId == "WashHair")
            {
                HairProfile hair = appearance.ScalpHairProfile;
                hair.IsWet = true;
                hair.IsMessy = false;
                appearance.SetHairProfile(hair);
                return;
            }

            if (result.AppearanceActionId == "CombHair")
            {
                HairProfile hair = appearance.ScalpHairProfile;
                hair.IsMessy = false;
                appearance.SetHairProfile(hair);
                return;
            }

            if (result.AppearanceActionId == "ApplyMakeup")
            {
                appearance.SetMakeupColor(new Color(0.95f, 0.7f, 0.8f, 0.55f));
            }
        }

        public void ApplyWorldStateResult(TaskResultDefinition result)
        {
            if (result == null)
            {
                return;
            }

            // Placeholder hook for cleanliness/repair/planted-state world models.
        }

        public void ApplyRelationshipServiceResult(TaskResultDefinition result, CharacterCore actor)
        {
            if (result == null || actor == null || relationshipMemorySystem == null || string.IsNullOrWhiteSpace(result.RelationshipTargetId))
            {
                return;
            }

            relationshipMemorySystem.RecordEvent(
                actor.CharacterId,
                result.RelationshipTargetId,
                string.IsNullOrWhiteSpace(result.CompletionText) ? "service completed" : result.CompletionText,
                result.RelationshipDelta,
                true,
                "task_service");
        }
    }
}

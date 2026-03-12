using UnityEngine;

namespace Survivebest.UI
{
    public class FurniturePlaceable : MonoBehaviour
    {
        [SerializeField] private Bounds movementBounds = new(new Vector3(0f, 0f, 0f), new Vector3(20f, 12f, 0f));
        [SerializeField] private bool lockWhileNotInBuildMode = true;

        public bool TryMoveTo(Vector3 target)
        {
            float x = Mathf.Clamp(target.x, movementBounds.min.x, movementBounds.max.x);
            float y = Mathf.Clamp(target.y, movementBounds.min.y, movementBounds.max.y);
            transform.position = new Vector3(x, y, transform.position.z);
            return true;
        }

        public bool CanMove(bool isBuildModeEnabled)
        {
            return !lockWhileNotInBuildMode || isBuildModeEnabled;
        }
    }
}

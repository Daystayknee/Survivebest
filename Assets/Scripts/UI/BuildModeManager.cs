using UnityEngine;
using Survivebest.Events;

namespace Survivebest.UI
{
    public class BuildModeManager : MonoBehaviour
    {
        [SerializeField] private Camera gameplayCamera;
        [SerializeField] private LayerMask placeableLayerMask = ~0;
        [SerializeField] private GameEventHub gameEventHub;
        [SerializeField] private bool buildModeEnabled;

        private FurniturePlaceable selected;

        public bool IsBuildModeEnabled => buildModeEnabled;

        private void Update()
        {
            if (!buildModeEnabled || gameplayCamera == null)
            {
                return;
            }

            if (Input.GetMouseButtonDown(0))
            {
                Vector3 world = GetWorldPoint(Input.mousePosition);
                RaycastHit2D hit = Physics2D.Raycast(world, Vector2.zero, Mathf.Infinity, placeableLayerMask);
                selected = hit.collider != null ? hit.collider.GetComponent<FurniturePlaceable>() : null;
            }

            if (Input.GetMouseButton(0) && selected != null)
            {
                selected.TryMoveTo(GetWorldPoint(Input.mousePosition));
            }

            if (Input.GetMouseButtonUp(0))
            {
                selected = null;
            }
        }

        public void ToggleBuildMode()
        {
            SetBuildMode(!buildModeEnabled);
        }

        public void SetBuildMode(bool enabled)
        {
            if (buildModeEnabled == enabled)
            {
                return;
            }

            buildModeEnabled = enabled;
            PublishBuildModeEvent(enabled ? "BuildModeOn" : "BuildModeOff", enabled ? 1f : 0f);
        }

        private void PublishBuildModeEvent(string change, float magnitude)
        {
            (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
            {
                Type = SimulationEventType.BuildModeChanged,
                Severity = SimulationEventSeverity.Info,
                SystemName = nameof(BuildModeManager),
                ChangeKey = change,
                Reason = $"Build mode set to {buildModeEnabled}",
                Magnitude = magnitude
            });
        }

        private Vector3 GetWorldPoint(Vector3 screen)
        {
            screen.z = Mathf.Abs(gameplayCamera.transform.position.z);
            Vector3 world = gameplayCamera.ScreenToWorldPoint(screen);
            world.z = 0f;
            return world;
        }
    }
}

using UnityEngine;
using Survivebest.Core;
using Survivebest.Economy;

namespace Survivebest.Tasks
{
    public class TaskResultSpawner : MonoBehaviour
    {
        [SerializeField] private EconomyInventorySystem economyInventorySystem;

        public void SpawnInventoryResult(TaskResultDefinition result, CharacterCore actor)
        {
            if (result == null || string.IsNullOrWhiteSpace(result.ItemId) || economyInventorySystem == null)
            {
                return;
            }

            economyInventorySystem.AddItem(result.ItemId, Mathf.Max(1, result.ItemAmount), $"Task result: {result.ItemId}");
        }

        public void SpawnHeldPropResult(TaskResultDefinition result, CharacterCore actor)
        {
            if (result == null || actor == null)
            {
                return;
            }

            // Placeholder hook for equipping props in hand slots once hand-prop runtime is wired.
        }
    }
}

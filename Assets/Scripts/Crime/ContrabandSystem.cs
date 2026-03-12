using System;
using System.Collections.Generic;
using UnityEngine;
using Survivebest.Core;
using Survivebest.Events;

namespace Survivebest.Crime
{
    public enum ContrabandCategory
    {
        Drugs,
        Tools,
        Luxury,
        Communication
    }

    [Serializable]
    public class ContrabandItem
    {
        public string ItemId;
        public ContrabandCategory Category;
        [Range(0f, 1f)] public float Risk = 0.3f;
        [Min(1)] public int Value = 10;
    }

    [Serializable]
    public class InmateContrabandInventory
    {
        public string CharacterId;
        public List<ContrabandItem> Items = new();
    }

    public class ContrabandSystem : MonoBehaviour
    {
        [SerializeField] private GameEventHub gameEventHub;
        [SerializeField] private List<InmateContrabandInventory> inventories = new();

        public event Action<string, ContrabandItem> OnContrabandAdded;
        public event Action<string, ContrabandItem> OnContrabandConfiscated;

        public IReadOnlyList<InmateContrabandInventory> Inventories => inventories;

        public void AddContraband(CharacterCore actor, ContrabandItem item)
        {
            if (actor == null || item == null)
            {
                return;
            }

            InmateContrabandInventory inventory = GetOrCreateInventory(actor.CharacterId);
            inventory.Items.Add(item);
            OnContrabandAdded?.Invoke(actor.CharacterId, item);
            Publish(actor.CharacterId, "ContrabandAdded", $"Acquired {item.ItemId}", item.Risk);
        }

        public bool TryConfiscateRandom(CharacterCore actor)
        {
            if (actor == null)
            {
                return false;
            }

            InmateContrabandInventory inventory = inventories.Find(x => x != null && x.CharacterId == actor.CharacterId);
            if (inventory == null || inventory.Items.Count == 0)
            {
                return false;
            }

            int index = UnityEngine.Random.Range(0, inventory.Items.Count);
            ContrabandItem item = inventory.Items[index];
            inventory.Items.RemoveAt(index);
            OnContrabandConfiscated?.Invoke(actor.CharacterId, item);
            Publish(actor.CharacterId, "ContrabandConfiscated", $"Confiscated {item.ItemId}", item != null ? item.Risk : 0.2f);
            return true;
        }

        public int CountItems(string characterId)
        {
            InmateContrabandInventory inventory = inventories.Find(x => x != null && x.CharacterId == characterId);
            return inventory != null && inventory.Items != null ? inventory.Items.Count : 0;
        }

        private InmateContrabandInventory GetOrCreateInventory(string characterId)
        {
            InmateContrabandInventory inventory = inventories.Find(x => x != null && x.CharacterId == characterId);
            if (inventory != null)
            {
                return inventory;
            }

            inventory = new InmateContrabandInventory { CharacterId = characterId, Items = new List<ContrabandItem>() };
            inventories.Add(inventory);
            return inventory;
        }

        private void Publish(string characterId, string key, string reason, float magnitude)
        {
            (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
            {
                Type = SimulationEventType.ActivityCompleted,
                Severity = SimulationEventSeverity.Warning,
                SystemName = nameof(ContrabandSystem),
                SourceCharacterId = characterId,
                ChangeKey = key,
                Reason = reason,
                Magnitude = magnitude
            });
        }
    }
}

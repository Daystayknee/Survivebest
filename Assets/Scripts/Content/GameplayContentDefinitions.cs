using System;
using System.Collections.Generic;
using UnityEngine;
using Survivebest.Core;
using Survivebest.Location;

namespace Survivebest.Content
{
    public abstract class ContentDefinitionAsset : ScriptableObject
    {
        [SerializeField] private string contentId;
        [SerializeField] private string displayName;

        public string ContentId => contentId;
        public string DisplayName => displayName;
    }

    [Serializable]
    public sealed class EconomyItemContentData
    {
        public string Category;
        public int BaseValue;
    }

    [CreateAssetMenu(menuName = "Survivebest/Content/Item Definition")]
    public sealed class ItemDefinitionAsset : ContentDefinitionAsset
    {
        [SerializeField] private EconomyItemContentData runtimeData = new();
        public EconomyItemContentData RuntimeData => runtimeData;
    }

    [Serializable]
    public sealed class ActivityContentData
    {
        public string ActivityId;
        public string PrimaryNeed;
        public List<string> Tags = new();
    }

    [CreateAssetMenu(menuName = "Survivebest/Content/Activity Definition")]
    public sealed class ActivityDefinitionAsset : ContentDefinitionAsset
    {
        [SerializeField] private ActivityContentData runtimeData = new();
        public ActivityContentData RuntimeData => runtimeData;
    }

    [CreateAssetMenu(menuName = "Survivebest/Content/District Template")]
    public sealed class DistrictTemplateAsset : ContentDefinitionAsset
    {
        [SerializeField] private DistrictDefinition runtimeData = new();
        public DistrictDefinition RuntimeData => runtimeData;
    }

    [CreateAssetMenu(menuName = "Survivebest/Content/Status Effect Definition")]
    public sealed class StatusEffectDefinitionAsset : ContentDefinitionAsset
    {
        [SerializeField] private string statusId;
        [SerializeField] private bool isNegative;
        [SerializeField] private int defaultDurationHours = 4;

        public string StatusId => statusId;
        public bool IsNegative => isNegative;
        public int DefaultDurationHours => defaultDurationHours;
    }

    [CreateAssetMenu(menuName = "Survivebest/Content/Contract Definition")]
    public sealed class ContractDefinitionAsset : ContentDefinitionAsset
    {
        [SerializeField] private string zoneName;
        [SerializeField] private int reward;

        public string ZoneName => zoneName;
        public int Reward => reward;
    }
}

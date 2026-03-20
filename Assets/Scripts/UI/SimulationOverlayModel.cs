using System;
using System.Collections.Generic;
using UnityEngine;

namespace Survivebest.UI
{
    public enum SimulationOverlayMetric
    {
        Activity,
        Population,
        Pressure,
        Safety,
        Wealth,
        CommunityEvents,
        StoryIncidents
    }

    [Serializable]
    public class SimulationOverlayEntry
    {
        public string EntryId;
        public string DisplayName;
        public SimulationOverlayMetric Metric;
        public float Value;
        public List<string> Tags = new();
        public string Status;
        public bool Highlighted;
    }

    [Serializable]
    public class SimulationOverlayFilterState
    {
        public SimulationOverlayMetric Metric = SimulationOverlayMetric.Activity;
        public string DistrictId;
        public bool HighlightOnly;
        public List<string> RequiredTags = new();
    }
}

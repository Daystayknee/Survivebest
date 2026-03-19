using UnityEngine;
using Survivebest.NPC;

namespace Survivebest.World
{
    [AddComponentMenu("Survivebest/World/Unity Presentation State Hookup")]
    public class UnityPresentationStateHookup : MonoBehaviour
    {
        [Header("Core References")]
        [Tooltip("GeneticsSystem on this character or prefab. If left empty, the component tries GetComponent<GeneticsSystem>() when resolving.")]
        [SerializeField] private GeneticsSystem geneticsSystem;
        [Tooltip("MasterAssetMatrix asset used to resolve trait/state bands into concrete art keys.")]
        [SerializeField] private MasterAssetMatrix masterAssetMatrix;

        [Header("NPC Mode")]
        [Tooltip("Enable this when the object represents a background NPC driven by NpcScheduleSystem instead of the active household character flow.")]
        [SerializeField] private bool useNpcProfile;
        [Tooltip("Scene reference to the NpcScheduleSystem that owns the NPC profile data.")]
        [SerializeField] private NpcScheduleSystem npcScheduleSystem;
        [Tooltip("NpcProfile id to pull stress/health/activity/reputation from when NPC mode is enabled.")]
        [SerializeField] private string npcId;

        [Header("Auto Refresh")]
        [Tooltip("Resolve once automatically when the object becomes enabled.")]
        [SerializeField] private bool resolveOnEnable = true;
        [Tooltip("Continuously re-resolve on a timer while the object is active.")]
        [SerializeField] private bool autoRefresh;
        [Tooltip("Seconds between automatic resolve passes when Auto Refresh is enabled.")]
        [SerializeField, Min(0.1f)] private float refreshInterval = 5f;

        private float refreshTimer;

        public GeneticsSystem GeneticsSystem => geneticsSystem;
        public MasterAssetMatrix MasterAssetMatrix => masterAssetMatrix;

        private void Reset()
        {
            geneticsSystem = GetComponent<GeneticsSystem>();
        }

        private void OnEnable()
        {
            refreshTimer = 0f;
            if (resolveOnEnable)
            {
                ResolveNow();
            }
        }

        private void Update()
        {
            if (!autoRefresh)
            {
                return;
            }

            refreshTimer -= Time.deltaTime;
            if (refreshTimer > 0f)
            {
                return;
            }

            refreshTimer = refreshInterval;
            ResolveNow();
        }

        [ContextMenu("Resolve Presentation State Now")]
        public void ResolveNow()
        {
            geneticsSystem ??= GetComponent<GeneticsSystem>();
            if (geneticsSystem == null || geneticsSystem.Phenotype == null)
            {
                return;
            }

            if (useNpcProfile)
            {
                NpcProfile npc = npcScheduleSystem != null && !string.IsNullOrWhiteSpace(npcId)
                    ? npcScheduleSystem.GetNpcProfile(npcId)
                    : null;
                AvatarPresentationState state = AvatarPresentationStateResolver.ResolveNpcState(geneticsSystem.Phenotype, npc);
                AvatarPresentationStateResolver.ApplyResolvedState(geneticsSystem.Phenotype, state);
                return;
            }

            geneticsSystem.ApplyDynamicPresentationState();
        }

        public AssetMatrixEntry ResolveAssetEntry(string traitOrState, float normalizedValue)
        {
            if (masterAssetMatrix == null || geneticsSystem == null || geneticsSystem.Phenotype == null)
            {
                return null;
            }

            return masterAssetMatrix.FindBestEntry(traitOrState, normalizedValue, geneticsSystem.Phenotype.LifeStage.ToString());
        }
    }
}

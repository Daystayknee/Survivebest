using UnityEngine;
using Survivebest.NPC;

namespace Survivebest.World
{
    [AddComponentMenu("Survivebest/World/Unity Presentation State Hookup")]
    public class UnityPresentationStateHookup : MonoBehaviour
    {
        [Header("Core References")]
        [SerializeField] private GeneticsSystem geneticsSystem;
        [SerializeField] private MasterAssetMatrix masterAssetMatrix;

        [Header("NPC Mode")]
        [SerializeField] private bool useNpcProfile;
        [SerializeField] private NpcScheduleSystem npcScheduleSystem;
        [SerializeField] private string npcId;

        [Header("Auto Refresh")]
        [SerializeField] private bool resolveOnEnable = true;
        [SerializeField] private bool autoRefresh;
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

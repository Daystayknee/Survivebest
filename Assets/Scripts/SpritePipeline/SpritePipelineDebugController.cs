using UnityEngine;
using Survivebest.Core;
using Survivebest.World;

namespace Survivebest.SpritePipeline
{
    public class SpritePipelineDebugController : MonoBehaviour
    {
        [SerializeField] private SpriteAssetRegistry registry;
        [SerializeField] private int seed = 10482;
        [SerializeField] private LifeStage lifeStage = LifeStage.YoungAdult;
        [SerializeField] private AvatarPresentationInput input = new AvatarPresentationInput
        {
            Stress = 0.4f,
            Anger = 0.15f,
            Affection = 0.5f,
            Energy = 0.6f,
            IllnessPressure = 0.2f,
            Confidence = 0.55f,
            SocialPressure = 0.4f,
            Grooming = 0.7f,
            SafetyUrgency = 0.2f
        };
        [SerializeField] private string globalFallbackKey = "default_portrait";
        [SerializeField] private bool runOnStart;

        public SpriteValidationReport LastReport { get; private set; }

        private void Start()
        {
            if (runOnStart)
            {
                RunValidation();
            }
        }

        [ContextMenu("Run Sprite Pipeline Validation")]
        public void RunValidation()
        {
            SpriteLookupService lookup = new SpriteLookupService(registry);
            LastReport = SpriteResolverValidator.ValidateSeed(lookup, seed, lifeStage, input, globalFallbackKey);
            Debug.Log($"[SpritePipeline] {LastReport.ToMultilineString()}", this);
        }
    }
}

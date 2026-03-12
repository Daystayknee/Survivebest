using System.Text;
using UnityEngine;
using UnityEngine.UI;
using Survivebest.Core;
using Survivebest.Health;
using Survivebest.World;

namespace Survivebest.UI
{
    public class CharacterScreenController : MonoBehaviour
    {
        [SerializeField] private HouseholdManager householdManager;

        [Header("Panels")]
        [SerializeField] private CharacterPortraitRenderer portraitRenderer;
        [SerializeField] private Text identityText;
        [SerializeField] private Text bodyStatsText;
        [SerializeField] private Text geneticsText;
        [SerializeField] private Text healthText;

        [Header("Trait Pills")]
        [SerializeField] private TraitPillTagView pillPrefab;
        [SerializeField] private Transform pillsContainer;
        [SerializeField] private Color positiveTraitColor = new(0.2f, 0.75f, 0.35f, 1f);
        [SerializeField] private Color cautionTraitColor = new(0.95f, 0.65f, 0.2f, 1f);
        [SerializeField] private Color ailmentColor = new(0.9f, 0.35f, 0.35f, 1f);

        private readonly StringBuilder builder = new();

        private void OnEnable()
        {
            if (householdManager != null)
            {
                householdManager.OnActiveCharacterChanged += HandleCharacterChanged;
                HandleCharacterChanged(householdManager.ActiveCharacter);
            }
        }

        private void OnDisable()
        {
            if (householdManager != null)
            {
                householdManager.OnActiveCharacterChanged -= HandleCharacterChanged;
            }
        }

        private void HandleCharacterChanged(CharacterCore character)
        {
            Refresh(character);
        }

        public void Refresh(CharacterCore character)
        {
            if (character == null)
            {
                SetMissing();
                return;
            }

            Appearance.AppearanceManager appearance = character.GetComponent<Appearance.AppearanceManager>();
            if (portraitRenderer != null)
            {
                portraitRenderer.SetTargetCharacter(character, appearance);
            }

            RenderIdentity(character);
            RenderBodyStats(character.GetComponent<BodyCompositionSystem>());
            RenderGenetics(character.GetComponent<GeneticsSystem>(), character.GetComponent<VisualGenome>());
            RenderHealth(character.GetComponent<HealthSystem>(), character.GetComponent<MedicalConditionSystem>());
            RenderTraitPills(character);
        }

        private void RenderIdentity(CharacterCore character)
        {
            if (identityText == null)
            {
                return;
            }

            identityText.text = $"{character.DisplayName}\nLife Stage: {character.CurrentLifeStage}\nBirthday: Y{character.BirthYear} M{character.BirthMonth} D{character.BirthDay}";
        }

        private void RenderBodyStats(BodyCompositionSystem body)
        {
            if (bodyStatsText == null)
            {
                return;
            }

            if (body == null)
            {
                bodyStatsText.text = "Body data unavailable.";
                return;
            }

            bodyStatsText.text = $"Height: {body.HeightCm:0.#} cm\nWeight: {body.WeightKg:0.#} kg\nBody Fat: {body.BodyFat * 100f:0.#}%\nMuscle Tone: {body.MuscleTone * 100f:0.#}%";
        }

        private void RenderGenetics(GeneticsSystem geneticsSystem, VisualGenome visualGenome)
        {
            if (geneticsText == null)
            {
                return;
            }

            if (visualGenome == null && geneticsSystem == null)
            {
                geneticsText.text = "Genetics/visual DNA unavailable.";
                return;
            }

            builder.Clear();
            builder.AppendLine("Genetics / Physical DNA");

            if (geneticsSystem != null && geneticsSystem.Phenotype != null)
            {
                PhenotypeProfile phenotype = geneticsSystem.Phenotype;
                builder.AppendLine($"Schema: {phenotype.BodySchema}");
                builder.AppendLine($"Skin Tone: {phenotype.Skin.Tone:0.00}");
                builder.AppendLine($"Freckles: {phenotype.Skin.Overlays.Freckles:0.00}");
                builder.AppendLine($"Vitiligo: {phenotype.Skin.Overlays.Vitiligo:0.00}");
                builder.AppendLine($"Jaw Width: {phenotype.Face.JawWidth:0.00}");
                builder.AppendLine($"Lip Fullness: {phenotype.Face.LipFullness:0.00}");
            }

            if (visualGenome != null)
            {
                PhysicalTraits t = visualGenome.CurrentTraits;
                builder.AppendLine($"Neck: {t.NeckLength:0.00}");
                builder.AppendLine($"Shoulders: {t.ShoulderWidth:0.00}");
                builder.AppendLine($"Bust: {t.BustSize:0.00}");
                builder.AppendLine($"Hips: {t.HipWidth:0.00}");
                builder.AppendLine($"Height Gene: {t.Height:0.00}");
            }

            geneticsText.text = builder.ToString().TrimEnd();
        }

        private void RenderHealth(HealthSystem health, MedicalConditionSystem medical)
        {
            if (healthText == null)
            {
                return;
            }

            builder.Clear();
            builder.AppendLine($"Vitality: {(health != null ? health.Vitality : 0f):0.#}/100");

            if (medical == null || medical.ActiveConditions == null || medical.ActiveConditions.Count == 0)
            {
                builder.AppendLine("Ailments: None");
            }
            else
            {
                builder.AppendLine("Ailments:");
                for (int i = 0; i < medical.ActiveConditions.Count; i++)
                {
                    MedicalCondition c = medical.ActiveConditions[i];
                    string name = c.IsIllness ? c.IllnessType.ToString() : c.InjuryType.ToString();
                    builder.AppendLine($"- {name} ({c.Severity}) - {c.RemainingHours}h");
                }
            }

            healthText.text = builder.ToString().TrimEnd();
        }

        private void RenderTraitPills(CharacterCore character)
        {
            if (pillPrefab == null || pillsContainer == null)
            {
                return;
            }

            for (int i = pillsContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(pillsContainer.GetChild(i).gameObject);
            }

            CreatePill($"Face: {character.FaceShape}", positiveTraitColor);
            CreatePill($"Eyes: {character.EyeShape}", positiveTraitColor);
            CreatePill($"Build: {character.CurrentBodyType}", positiveTraitColor);
            CreatePill($"Style: {character.ClothingStyle}", positiveTraitColor);

            if (character.Talents != null)
            {
                for (int i = 0; i < character.Talents.Count; i++)
                {
                    CreatePill(character.Talents[i].ToString(), cautionTraitColor);
                }
            }

            MedicalConditionSystem med = character.GetComponent<MedicalConditionSystem>();
            if (med != null && med.ActiveConditions != null)
            {
                for (int i = 0; i < med.ActiveConditions.Count; i++)
                {
                    MedicalCondition c = med.ActiveConditions[i];
                    string label = c.IsIllness ? c.IllnessType.ToString() : c.InjuryType.ToString();
                    CreatePill(label, ailmentColor);
                }
            }
        }

        private void CreatePill(string label, Color color)
        {
            TraitPillTagView pill = Instantiate(pillPrefab, pillsContainer);
            pill.Bind(label, color);
        }

        private void SetMissing()
        {
            if (identityText != null) identityText.text = "No active character.";
            if (bodyStatsText != null) bodyStatsText.text = string.Empty;
            if (geneticsText != null) geneticsText.text = string.Empty;
            if (healthText != null) healthText.text = string.Empty;
        }
    }
}

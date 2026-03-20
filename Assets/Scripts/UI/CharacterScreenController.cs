using System.Text;
using UnityEngine;
using UnityEngine.UI;
using Survivebest.Core;
using Survivebest.Health;
using Survivebest.World;
using Survivebest.Catalog;

namespace Survivebest.UI
{
    public class CharacterScreenController : MonoBehaviour
    {
        [SerializeField] private HouseholdManager householdManager;
        [SerializeField] private PersonalityMatrixSystem personalityMatrixSystem;
        [SerializeField] private SupplyCatalog supplyCatalog;

        [Header("Panels")]
        [SerializeField] private CharacterPortraitRenderer portraitRenderer;
        [SerializeField] private Text identityText;
        [SerializeField] private Text bodyStatsText;
        [SerializeField] private Text geneticsText;
        [SerializeField] private Text healthText;
        [SerializeField] private Text personalityText;

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
            RenderPersonality(character);
            RenderTraitPills(character);
        }

        private void RenderIdentity(CharacterCore character)
        {
            if (identityText == null)
            {
                return;
            }

            builder.Clear();
            builder.AppendLine(character.DisplayName);
            builder.AppendLine($"Life Stage: {character.CurrentLifeStage}");
            builder.AppendLine($"Birthday: Y{character.BirthYear} M{character.BirthMonth} D{character.BirthDay}");

            if (supplyCatalog != null)
            {
                var groups = supplyCatalog.GetPriorityGroupsForCharacter(character);
                var items = supplyCatalog.GetSuggestedSuppliesForCharacter(character, 3);
                if (groups.Count > 0)
                {
                    builder.AppendLine($"Retail Focus: {string.Join(", ", groups.GetRange(0, Mathf.Min(3, groups.Count)))}");
                }

                if (items.Count > 0)
                {
                    builder.AppendLine($"Starter Buys: {string.Join(", ", items.ConvertAll(x => x.Name).GetRange(0, Mathf.Min(3, items.Count)))}");
                }
            }

            identityText.text = builder.ToString().TrimEnd();
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
                builder.AppendLine($"Skin Tone: {phenotype.Skin.Tone:0.00}  Undertone: {phenotype.Skin.Undertone:0.00}");
                builder.AppendLine($"Freckles: {phenotype.Skin.Overlays.Freckles:0.00}  Beauty Marks: {phenotype.Skin.Overlays.BeautyMarks:0.00}");
                builder.AppendLine($"Vitiligo: {phenotype.Skin.Overlays.Vitiligo:0.00}  Acne: {phenotype.Skin.Overlays.Acne:0.00}  Wrinkles: {phenotype.Skin.Overlays.Wrinkles:0.00}");
                builder.AppendLine($"Jaw: {phenotype.Face.JawWidth:0.00}  Chin: {phenotype.Face.ChinProminence:0.00}  Cheeks: {phenotype.Face.CheekFullness:0.00}");
                builder.AppendLine($"Eyes: {phenotype.Face.EyeSize:0.00}/{phenotype.Face.EyeSpacing:0.00}  Nose: {phenotype.Face.NoseBridgeHeight:0.00}/{phenotype.Face.NostrilWidth:0.00}  Lips: {phenotype.Face.LipFullness:0.00}");
                builder.AppendLine($"Hair Curl: {phenotype.Hair.Curl:0.00}  Density: {phenotype.Hair.Density:0.00}  Front/Side/Back: {phenotype.Hair.FrontPieceDensity:0.00}/{phenotype.Hair.SidePieceDensity:0.00}/{phenotype.Hair.BackPieceDensity:0.00}");
                builder.AppendLine($"Body Regions N/C/W/H/T/C: {phenotype.Body.Neck:0.00}/{phenotype.Body.ChestBustPresentation:0.00}/{phenotype.Body.Waist:0.00}/{phenotype.Body.Hips:0.00}/{phenotype.Body.Thighs:0.00}/{phenotype.Body.Calves:0.00}");
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


        private void RenderPersonality(CharacterCore character)
        {
            if (personalityText == null)
            {
                return;
            }

            if (character == null || personalityMatrixSystem == null)
            {
                personalityText.text = "Personality data unavailable.";
                return;
            }

            personalityText.text = personalityMatrixSystem.BuildCompactSummary(character.CharacterId);
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
            if (personalityText != null) personalityText.text = string.Empty;
        }
    }
}

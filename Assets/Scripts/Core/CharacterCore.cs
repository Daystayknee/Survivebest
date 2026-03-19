using System;
using System.Collections.Generic;
using UnityEngine;
using Survivebest.Events;
using Survivebest.Appearance;

namespace Survivebest.Core
{
    public enum LifeStage
    {
        Baby,
        Infant,
        Toddler,
        Child,
        Preteen,
        Teen,
        YoungAdult,
        Adult,
        OlderAdult,
        Elder
    }

    public enum CharacterSpecies
    {
        Human,
        Vampire
    }

    public enum CharacterTalent
    {
        None,
        Artistic,
        Athletic,
        Social,
        Academic,
        Musical,
        Technical,
        Culinary,
        Entrepreneurial,
        Caregiving,
        Performer
    }

    public enum FaceShapeType
    {
        Oval,
        Round,
        Square,
        Heart,
        Diamond,
        Rectangle,
        Triangle,
        LongOval,
        SoftPear,
        BroadOval,
        NarrowAngular
    }

    public enum EyeShapeType
    {
        Almond,
        Round,
        Hooded,
        Monolid,
        Upturned,
        Downturned,
        DeepSet,
        Protruding,
        CloseSet,
        WideSet,
        Doe,
        CatEye
    }

    public enum BodyType
    {
        Petite,
        Slim,
        Lean,
        Average,
        Soft,
        Curvy,
        Athletic,
        Muscular,
        Broad,
        TallLean,
        PlusSize,
        Heavy
    }

    public enum JawShapeType
    {
        Soft,
        Balanced,
        Defined,
        Angular
    }

    public enum NoseShapeType
    {
        Petite,
        Straight,
        Aquiline,
        Button,
        Broad,
        Roman,
        Snub,
        Nubian,
        Hooked,
        FlatBridge,
        WideBridge,
        LongNarrow,
        Rounded
    }

    public enum LipShapeType
    {
        Thin,
        Balanced,
        Full,
        Heart,
        CupidBow,
        WideFull,
        Downturned,
        SoftRound
    }

    public enum ClothingStyleType
    {
        Casual,
        Work,
        Sport,
        Formal,
        Medical,
        Outdoor,
        Streetwear,
        Traditional,
        Loungewear,
        Festival,
        Evening,
        Utility
    }

    public class CharacterCore : MonoBehaviour
    {
        [SerializeField] private string characterId;
        [SerializeField] private string displayName = "New Character";
        [SerializeField] private LifeStage lifeStage = LifeStage.YoungAdult;
        [SerializeField] private bool isPlayerControlled;
        [SerializeField] private bool isDead;
        [SerializeField] private CharacterSpecies species = CharacterSpecies.Human;
        [SerializeField] private List<CharacterTalent> talents = new();
        [SerializeField] private GameEventHub gameEventHub;

        [Header("Portrait Data")]
        [SerializeField] private FaceShapeType faceShape = FaceShapeType.Oval;
        [SerializeField] private EyeShapeType eyeShape = EyeShapeType.Almond;
        [SerializeField] private BodyType bodyType = BodyType.Average;
        [SerializeField] private JawShapeType jawShape = JawShapeType.Balanced;
        [SerializeField] private NoseShapeType noseShape = NoseShapeType.Straight;
        [SerializeField] private LipShapeType lipShape = LipShapeType.Balanced;
        [SerializeField] private ClothingStyleType clothingStyle = ClothingStyleType.Casual;
        [Header("Feature Expression")]
        [SerializeField, Range(0f, 1f)] private float feminineExpression = 0.5f;
        [SerializeField, Range(0f, 1f)] private float masculineExpression = 0.5f;
        [SerializeField, Range(0f, 1f)] private float androgynyExpression = 0.5f;

        [Header("Birth Date")]
        [SerializeField, Min(1)] private int birthYear = 1;
        [SerializeField, Range(1, 12)] private int birthMonth = 1;
        [SerializeField, Range(1, 31)] private int birthDay = 1;

        public event Action<CharacterCore> OnCharacterDied;

        public string CharacterId => characterId;
        public string DisplayName => displayName;
        public LifeStage CurrentLifeStage => lifeStage;
        public bool IsPlayerControlled => isPlayerControlled;
        public bool IsDead => isDead;
        public CharacterSpecies Species => species;
        public bool IsHuman => species == CharacterSpecies.Human;
        public bool IsVampire => species == CharacterSpecies.Vampire;
        public IReadOnlyList<CharacterTalent> Talents => talents;
        public int BirthYear => birthYear;
        public int BirthMonth => birthMonth;
        public int BirthDay => birthDay;
        public FaceShapeType FaceShape => faceShape;
        public EyeShapeType EyeShape => eyeShape;
        public BodyType CurrentBodyType => bodyType;
        public JawShapeType JawShape => jawShape;
        public NoseShapeType NoseShape => noseShape;
        public LipShapeType LipShape => lipShape;
        public ClothingStyleType ClothingStyle => clothingStyle;
        public float FeminineExpression => feminineExpression;
        public float MasculineExpression => masculineExpression;
        public float AndrogynyExpression => androgynyExpression;

        public void Initialize(string id, string name, LifeStage stage, CharacterSpecies newSpecies = CharacterSpecies.Human)
        {
            characterId = id;
            displayName = name;
            lifeStage = stage;
            species = newSpecies;
        }

        public void SetBirthDate(int year, int month, int day)
        {
            birthYear = Mathf.Max(1, year);
            birthMonth = Mathf.Clamp(month, 1, 12);
            birthDay = Mathf.Clamp(day, 1, 31);
        }

        public bool IsBirthday(int currentMonth, int currentDay)
        {
            return birthMonth == currentMonth && birthDay == currentDay;
        }

        public void SetDisplayName(string value)
        {
            displayName = value;
        }

        public void SetLifeStage(LifeStage stage)
        {
            lifeStage = stage;
        }

        public void SetSpecies(CharacterSpecies value)
        {
            species = value;
        }

        public string GetSpeciesKey()
        {
            return species.ToString().ToLowerInvariant();
        }

        public float GetSpeciesAgingRateMultiplier()
        {
            return species switch
            {
                CharacterSpecies.Vampire when lifeStage is LifeStage.Baby or LifeStage.Infant or LifeStage.Toddler or LifeStage.Child or LifeStage.Preteen or LifeStage.Teen => 1f,
                CharacterSpecies.Vampire when lifeStage == LifeStage.YoungAdult => 0.35f,
                CharacterSpecies.Vampire => 0.08f,
                _ => 1f
            };
        }

        public bool CanFeedOnBlood() => species == CharacterSpecies.Vampire;

        public bool CanCompelTargets() => species == CharacterSpecies.Vampire && lifeStage >= LifeStage.Teen;

        public bool HasNightAdvantage() => species == CharacterSpecies.Vampire;

        public string GetSpeciesTraitSummary()
        {
            return species switch
            {
                CharacterSpecies.Vampire => "Night-active, blood-feeding, sunlight-vulnerable, and slow-aging after maturity.",
                _ => "Day-active human baseline with standard aging and illness rules."
            };
        }


        public void SetTalents(List<CharacterTalent> values)
        {
            talents = values ?? new List<CharacterTalent>();
        }

        public void SetPortraitData(FaceShapeType newFaceShape, EyeShapeType newEyeShape, BodyType newBodyType, ClothingStyleType newClothingStyle)
        {
            faceShape = newFaceShape;
            eyeShape = newEyeShape;
            bodyType = newBodyType;
            clothingStyle = newClothingStyle;
        }

        public void SetFacialFeatureData(JawShapeType newJawShape, NoseShapeType newNoseShape, LipShapeType newLipShape)
        {
            jawShape = newJawShape;
            noseShape = newNoseShape;
            lipShape = newLipShape;
        }

        public void RandomizePortraitData()
        {
            faceShape = RandomEnum<FaceShapeType>();
            eyeShape = RandomEnum<EyeShapeType>();
            bodyType = RandomEnum<BodyType>();
            jawShape = RandomEnum<JawShapeType>();
            noseShape = RandomEnum<NoseShapeType>();
            lipShape = RandomEnum<LipShapeType>();
            clothingStyle = RandomEnum<ClothingStyleType>();
        }

        public void SyncPortraitDataFromAppearance(AppearanceManager appearanceManager)
        {
            if (appearanceManager == null || appearanceManager.CurrentProfile == null)
            {
                return;
            }

            AppearanceProfile profile = appearanceManager.CurrentProfile;
            eyeShape = profile.EyeColor switch
            {
                EyeColorType.Brown => EyeShapeType.Almond,
                EyeColorType.Hazel => EyeShapeType.Round,
                EyeColorType.Green => EyeShapeType.Upturned,
                EyeColorType.Blue => EyeShapeType.Hooded,
                EyeColorType.Gray => EyeShapeType.Downturned,
                EyeColorType.Teal or EyeColorType.Violet => EyeShapeType.Upturned,
                EyeColorType.DarkBrown or EyeColorType.LightBrown or EyeColorType.Honey => EyeShapeType.Almond,
                _ => EyeShapeType.Monolid
            };

            clothingStyle = profile.SkinIssue switch
            {
                SkinIssueType.None => ClothingStyleType.Casual,
                SkinIssueType.Acne or SkinIssueType.Rosacea => ClothingStyleType.Work,
                SkinIssueType.Vitiligo => ClothingStyleType.Formal,
                _ => clothingStyle
            };
        }

        public float GetSkillMultiplier(string skillName)
        {
            if (talents == null || talents.Count == 0)
            {
                return 1f;
            }

            if (skillName == "Art" && talents.Contains(CharacterTalent.Artistic))
            {
                return 1.5f;
            }

            if (skillName == "Fitness" && talents.Contains(CharacterTalent.Athletic))
            {
                return 1.25f;
            }

            if (skillName == "Social" && talents.Contains(CharacterTalent.Social))
            {
                return 1.25f;
            }

            return 1f;
        }

        public void SetFeatureExpression(float feminine, float masculine, float androgyny)
        {
            feminineExpression = Mathf.Clamp01(feminine);
            masculineExpression = Mathf.Clamp01(masculine);
            androgynyExpression = Mathf.Clamp01(androgyny);
        }

        public void SetPlayerControlled(bool value)
        {
            isPlayerControlled = value;
        }

        public void Die()
        {
            if (isDead)
            {
                return;
            }

            isDead = true;
            isPlayerControlled = false;
            OnCharacterDied?.Invoke(this);

            (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
            {
                Type = SimulationEventType.CharacterDied,
                Severity = SimulationEventSeverity.Critical,
                SystemName = nameof(CharacterCore),
                SourceCharacterId = characterId,
                ChangeKey = nameof(isDead),
                Reason = "Vitality or game system death trigger",
                Magnitude = 100f
            });
        }

        private static T RandomEnum<T>() where T : Enum
        {
            Array values = Enum.GetValues(typeof(T));
            return (T)values.GetValue(UnityEngine.Random.Range(0, values.Length));
        }
    }
}

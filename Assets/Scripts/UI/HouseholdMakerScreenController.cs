using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Survivebest.Core;
using Survivebest.Events;
using Survivebest.World;

namespace Survivebest.UI
{
    public enum HouseholdMakerTab
    {
        Appearance,
        Genetics,
        FamilyGenetics,
        Clothing,
        Shoes,
        Hats,
        Accessories,
        Makeup,
        Traits,
        Skills,
        Relationships,
        Household
    }

    public enum HouseholdOriginFocus
    {
        LocalRoots,
        NewArrivals,
        CivicFamily,
        Survivalist,
        Multigenerational
    }

    public enum FamilyPlanningPriority
    {
        Stability,
        Growth,
        Romance,
        Legacy,
        Community
    }

    public enum HouseholdLotType
    {
        StarterApartment,
        SuburbanHome,
        RuralFarmhouse,
        DowntownPenthouse,
        Waterfront,
        OffGridLot
    }

    public enum HouseholdBudgetTier
    {
        Shoestring,
        Starter,
        Comfortable,
        Affluent,
        Luxury
    }

    public enum HouseholdVibe
    {
        Cozy,
        Structured,
        ChaoticFun,
        CreativeLoft,
        Prestige,
        NatureFocused
    }

    public enum FamilyConflictApproach
    {
        TalkItOut,
        FirmBoundaries,
        HumorFirst,
        Competitive,
        Avoidant,
        Mediated
    }

    public enum HouseholdArchetype
    {
        LegacyBuilders,
        AmbitiousRoommates,
        FamilyFirst,
        CreativeCollective,
        QuietRetreat,
        PartyHouse
    }

    public enum HouseholdRoomType
    {
        Bedroom,
        Nursery,
        HomeOffice,
        Kitchen,
        Dining,
        LivingRoom,
        SkillStudio,
        FitnessSpace,
        Garden,
        RecreationRoom
    }

    public enum RoomStyleDirection
    {
        Minimal,
        Contemporary,
        CozyRustic,
        Industrial,
        Luxury,
        Eclectic
    }

    [Serializable]
    public class HouseholdRoomPlan
    {
        public HouseholdRoomType RoomType = HouseholdRoomType.Bedroom;
        public RoomStyleDirection StyleDirection = RoomStyleDirection.Contemporary;
        [Range(1, 5)] public int Priority = 3;
        [TextArea] public string Notes = "Describe the purpose and mood for this room.";
    }

    [Serializable]
    public class HouseholdMakerTabPanel
    {
        public HouseholdMakerTab Tab;
        public GameObject Root;
    }

    [Serializable]
    public class HouseholdDraftMemberSnapshot
    {
        public string CharacterId;
        public string DisplayName;
        public string LifeStage;
        public bool Locked;
    }

    [Serializable]
    public class HouseholdDraftPetSnapshot
    {
        public string PetId;
        public string DisplayName;
        public string Species;
        public string Breed;
    }

    [Serializable]
    public class AdoptionIntentSnapshot
    {
        public string IntentId;
        public string CandidateName;
        public string Category;
        public string Detail;
    }

    public enum HouseholdPetTemperament
    {
        Gentle,
        Playful,
        Protective,
        Independent,
        Shy,
        Energetic
    }

    [Serializable]
    public class HouseholdDraftSnapshot
    {
        public string ActiveCharacterId;
        public string ActiveTab;
        public bool FamilyLocked;
        public string FamilySurname;
        public string HomeDistrict;
        public string HouseholdStoryPrompt;
        public string OriginFocus;
        public string PlanningPriority;
        public string LotType;
        public string BudgetTier;
        public string HouseholdVibe;
        public string ConflictApproach;
        public bool WantsChildrenSoon;
        public bool IncludesPets;
        public bool PrioritizesCareerMobility;
        public string GenerationalGoal;
        public string HouseholdArchetype;
        public string CommuteStrategy;
        public string DailyRhythm;
        public List<HouseholdRoomPlan> RoomPlans = new();
        public List<HouseholdDraftMemberSnapshot> Members = new();
        public List<HouseholdDraftPetSnapshot> Pets = new();
        public List<AdoptionIntentSnapshot> AdoptionIntents = new();
        public string PetCreatorSpecies;
        public string PetCreatorBreed;
        public string PetCreatorName;
        public string PetCreatorTemperament;
    }

    public class HouseholdMakerScreenController : MonoBehaviour
    {
        [SerializeField] private MainMenuFlowController menuFlowController;
        [SerializeField] private HouseholdManager householdManager;
        [SerializeField] private Camera characterCamera;
        [SerializeField] private Transform characterPivot;
        [SerializeField] private GameEventHub gameEventHub;

        [Header("Optional UI")]
        [SerializeField] private Text tabText;
        [SerializeField] private Text characterNameText;
        [SerializeField] private Text creatorSummaryText;
        [SerializeField] private Text familyLockStateText;
        [SerializeField] private Text familyVisionText;
        [SerializeField] private List<HouseholdMakerTabPanel> tabPanels = new();
        [SerializeField] private bool wrapTabs = true;

        [Header("Family Planning")]
        [SerializeField] private string familySurname = "Rivera";
        [SerializeField] private string homeDistrict = "Founders Row";
        [SerializeField, TextArea] private string householdStoryPrompt = "A household trying to build a stable life while balancing work, care, and community expectations.";
        [SerializeField] private HouseholdOriginFocus originFocus = HouseholdOriginFocus.LocalRoots;
        [SerializeField] private FamilyPlanningPriority planningPriority = FamilyPlanningPriority.Stability;
        [SerializeField] private HouseholdLotType lotType = HouseholdLotType.StarterApartment;
        [SerializeField] private HouseholdBudgetTier budgetTier = HouseholdBudgetTier.Starter;
        [SerializeField] private HouseholdVibe householdVibe = HouseholdVibe.Cozy;
        [SerializeField] private FamilyConflictApproach conflictApproach = FamilyConflictApproach.TalkItOut;
        [SerializeField] private bool wantsChildrenSoon;
        [SerializeField] private bool includesPets;
        [SerializeField] private bool prioritizesCareerMobility = true;
        [SerializeField] private bool supportsAdoption = true;
        [SerializeField, Min(1)] private int adoptionPoolSize = 36;
        [SerializeField] private List<string> petCreatorSpeciesCatalog = new()
        {
            "Dog","Cat","Rabbit","Bird","Hamster","Guinea Pig","Ferret","Turtle","Lizard","Mini Pig","Goat","Horse",
            "Parrot","Chinchilla","Gecko","Iguana","Snake","Duck","Chicken","Hedgehog","Rat","Mouse","Axolotl","Pony"
        };
        [SerializeField] private List<string> petCreatorBreedCatalog = new()
        {
            "Mixed Rescue","Labrador","Golden Retriever","Shiba Inu","Corgi","German Shepherd","Maine Coon","Ragdoll","Siamese","Domestic Shorthair",
            "Holland Lop","Mini Rex","Cockatiel","Parakeet","Syrian Hamster","Abyssinian Guinea Pig","Standard Ferret","Painted Turtle","Leopard Gecko","Miniature Pig",
            "Border Collie","Australian Shepherd","Dachshund","Pomeranian","Bengal","Sphynx","Persian","Norwegian Forest","Angora Rabbit","Lionhead Rabbit",
            "African Grey","Macaw","Budgie","Canary","Crested Gecko","Bearded Dragon","Corn Snake","Ball Python","Silkie Chicken","Pekin Duck",
            "Mini Lop","Rex Rat","Fancy Mouse","African Pygmy Hedgehog","American Pony","Icelandic Horse","Mini Donkey","Cockapoo","Cavapoo","Street Mix"
        };
        [SerializeField] private int petCreatorSpeciesIndex;
        [SerializeField] private int petCreatorBreedIndex;
        [SerializeField] private string petCreatorName = "Paws";
        [SerializeField] private HouseholdPetTemperament petCreatorTemperament = HouseholdPetTemperament.Playful;
        [SerializeField] private string generationalGoal = "Build a secure first generation foundation with enough savings to launch the next chapter.";
        [SerializeField] private HouseholdArchetype householdArchetype = HouseholdArchetype.LegacyBuilders;
        [SerializeField] private string commuteStrategy = "Live near transit and jobs to reduce stress and keep routines predictable.";
        [SerializeField] private string dailyRhythm = "Early mornings, shared dinner, and focused skill-building at night.";
        [SerializeField] private List<HouseholdRoomPlan> roomPlans = new()
        {
            new HouseholdRoomPlan
            {
                RoomType = HouseholdRoomType.Bedroom,
                StyleDirection = RoomStyleDirection.CozyRustic,
                Priority = 5,
                Notes = "Comfort-focused sleeping space for routine stability."
            },
            new HouseholdRoomPlan
            {
                RoomType = HouseholdRoomType.Kitchen,
                StyleDirection = RoomStyleDirection.Contemporary,
                Priority = 4,
                Notes = "Central social hub with enough prep space for family meals."
            },
            new HouseholdRoomPlan
            {
                RoomType = HouseholdRoomType.HomeOffice,
                StyleDirection = RoomStyleDirection.Minimal,
                Priority = 3,
                Notes = "Career and study station that supports focused progression."
            }
        };

        [Header("Multi-Asset Character Preview")]
        [SerializeField] private List<Transform> characterArtPivots = new();
        [SerializeField] private bool rotateAllArtPivots = true;
        [SerializeField] private int activeArtPivotIndex;

        [Header("Camera Controls")]
        [SerializeField] private float rotateSpeed = 60f;
        [SerializeField] private float zoomSpeed = 2f;
        [SerializeField] private float minZoom = 2f;
        [SerializeField] private float maxZoom = 8f;

        public HouseholdMakerTab CurrentTab { get; private set; }
        public int ActiveArtPivotIndex => Mathf.Clamp(activeArtPivotIndex, 0, Mathf.Max(0, characterArtPivots.Count - 1));

        private readonly HashSet<string> lockedCharacterIds = new();
        private readonly List<AdoptionIntentSnapshot> adoptionIntents = new();
        private bool familyDraftLocked;

        private void OnEnable()
        {
            if (householdManager != null)
            {
                householdManager.OnActiveCharacterChanged += HandleActiveCharacterChanged;
                HandleActiveCharacterChanged(householdManager.ActiveCharacter);
            }

            RefreshTabUi();
            EnsureAdoptionPoolSeeded();
            RefreshCreatorSummary();
        }

        private void OnDisable()
        {
            if (householdManager != null)
            {
                householdManager.OnActiveCharacterChanged -= HandleActiveCharacterChanged;
            }
        }

        public void SetTab(int tab)
        {
            int tabCount = Enum.GetValues(typeof(HouseholdMakerTab)).Length;
            int resolved = wrapTabs
                ? ((tab % tabCount) + tabCount) % tabCount
                : Mathf.Clamp(tab, 0, tabCount - 1);

            CurrentTab = (HouseholdMakerTab)resolved;
            RefreshTabUi();
            PublishTabEvent();
        }

        public void NextTab()
        {
            SetTab((int)CurrentTab + 1);
        }

        public void PreviousTab()
        {
            SetTab((int)CurrentTab - 1);
        }

        public void OpenFamilyGeneticsSection()
        {
            SetTab((int)HouseholdMakerTab.FamilyGenetics);
        }

        public void SetFamilySurname(string value)
        {
            familySurname = string.IsNullOrWhiteSpace(value) ? familySurname : value.Trim();
            RefreshCreatorSummary();
        }

        public void SetHomeDistrict(string value)
        {
            homeDistrict = string.IsNullOrWhiteSpace(value) ? homeDistrict : value.Trim();
            RefreshCreatorSummary();
        }

        public void SetHouseholdStoryPrompt(string value)
        {
            householdStoryPrompt = string.IsNullOrWhiteSpace(value) ? householdStoryPrompt : value.Trim();
            RefreshCreatorSummary();
        }

        public void SetOriginFocus(int index)
        {
            originFocus = (HouseholdOriginFocus)Mathf.Clamp(index, 0, Enum.GetValues(typeof(HouseholdOriginFocus)).Length - 1);
            RefreshCreatorSummary();
        }

        public void SetPlanningPriority(int index)
        {
            planningPriority = (FamilyPlanningPriority)Mathf.Clamp(index, 0, Enum.GetValues(typeof(FamilyPlanningPriority)).Length - 1);
            RefreshCreatorSummary();
        }

        public void SetLotType(int index)
        {
            lotType = (HouseholdLotType)Mathf.Clamp(index, 0, Enum.GetValues(typeof(HouseholdLotType)).Length - 1);
            RefreshCreatorSummary();
        }

        public void SetBudgetTier(int index)
        {
            budgetTier = (HouseholdBudgetTier)Mathf.Clamp(index, 0, Enum.GetValues(typeof(HouseholdBudgetTier)).Length - 1);
            RefreshCreatorSummary();
        }

        public void SetHouseholdVibe(int index)
        {
            householdVibe = (HouseholdVibe)Mathf.Clamp(index, 0, Enum.GetValues(typeof(HouseholdVibe)).Length - 1);
            RefreshCreatorSummary();
        }

        public void SetConflictApproach(int index)
        {
            conflictApproach = (FamilyConflictApproach)Mathf.Clamp(index, 0, Enum.GetValues(typeof(FamilyConflictApproach)).Length - 1);
            RefreshCreatorSummary();
        }

        public void SetWantsChildrenSoon(bool value)
        {
            wantsChildrenSoon = value;
            RefreshCreatorSummary();
        }

        public void SetIncludesPets(bool value)
        {
            includesPets = value;
            RefreshCreatorSummary();
        }

        public void SetSupportsAdoption(bool value)
        {
            supportsAdoption = value;
            RefreshCreatorSummary();
        }

        public void SetPetCreatorSpecies(int index)
        {
            if (petCreatorSpeciesCatalog.Count == 0) return;
            petCreatorSpeciesIndex = Mathf.Clamp(index, 0, petCreatorSpeciesCatalog.Count - 1);
            RefreshCreatorSummary();
        }

        public void SetPetCreatorBreed(int index)
        {
            if (petCreatorBreedCatalog.Count == 0) return;
            petCreatorBreedIndex = Mathf.Clamp(index, 0, petCreatorBreedCatalog.Count - 1);
            RefreshCreatorSummary();
        }

        public void SetPetCreatorName(string value)
        {
            petCreatorName = string.IsNullOrWhiteSpace(value) ? petCreatorName : value.Trim();
            RefreshCreatorSummary();
        }

        public void SetPetCreatorTemperament(int index)
        {
            petCreatorTemperament = (HouseholdPetTemperament)Mathf.Clamp(index, 0, Enum.GetValues(typeof(HouseholdPetTemperament)).Length - 1);
            RefreshCreatorSummary();
        }

        public void CreatePetFromCreator()
        {
            if (householdManager == null || petCreatorSpeciesCatalog.Count == 0 || petCreatorBreedCatalog.Count == 0)
            {
                return;
            }

            string species = petCreatorSpeciesCatalog[Mathf.Clamp(petCreatorSpeciesIndex, 0, petCreatorSpeciesCatalog.Count - 1)];
            string breed = petCreatorBreedCatalog[Mathf.Clamp(petCreatorBreedIndex, 0, petCreatorBreedCatalog.Count - 1)];
            string name = string.IsNullOrWhiteSpace(petCreatorName) ? BuildRandomPetName() : petCreatorName.Trim();
            string petId = $"pet_creator_{Guid.NewGuid():N}".Substring(0, 20);
            householdManager.RegisterPet(petId, name, species, breed);
            includesPets = true;

            adoptionIntents.Add(new AdoptionIntentSnapshot
            {
                IntentId = $"intent_petmaker_{Guid.NewGuid():N}".Substring(0, 20),
                CandidateName = name,
                Category = "Pet Creator",
                Detail = $"{species} • {breed} • {petCreatorTemperament}"
            });

            PublishAdoptionEvent($"Created pet {name} ({species}, {breed}) in Pet Creator.");
            RefreshCreatorSummary();
        }

        public void SetPrioritizesCareerMobility(bool value)
        {
            prioritizesCareerMobility = value;
            RefreshCreatorSummary();
        }

        public void SetGenerationalGoal(string value)
        {
            generationalGoal = string.IsNullOrWhiteSpace(value) ? generationalGoal : value.Trim();
            RefreshCreatorSummary();
        }

        public void SetHouseholdArchetype(int index)
        {
            householdArchetype = (HouseholdArchetype)Mathf.Clamp(index, 0, Enum.GetValues(typeof(HouseholdArchetype)).Length - 1);
            RefreshCreatorSummary();
        }

        public void SetCommuteStrategy(string value)
        {
            commuteStrategy = string.IsNullOrWhiteSpace(value) ? commuteStrategy : value.Trim();
            RefreshCreatorSummary();
        }

        public void SetDailyRhythm(string value)
        {
            dailyRhythm = string.IsNullOrWhiteSpace(value) ? dailyRhythm : value.Trim();
            RefreshCreatorSummary();
        }

        public void AddRoomPlan(int roomType, int styleDirection, int priority, string notes)
        {
            roomPlans ??= new List<HouseholdRoomPlan>();
            roomPlans.Add(new HouseholdRoomPlan
            {
                RoomType = (HouseholdRoomType)Mathf.Clamp(roomType, 0, Enum.GetValues(typeof(HouseholdRoomType)).Length - 1),
                StyleDirection = (RoomStyleDirection)Mathf.Clamp(styleDirection, 0, Enum.GetValues(typeof(RoomStyleDirection)).Length - 1),
                Priority = Mathf.Clamp(priority, 1, 5),
                Notes = string.IsNullOrWhiteSpace(notes) ? "Custom room plan." : notes.Trim()
            });
            RefreshCreatorSummary();
        }

        public void RemoveLastRoomPlan()
        {
            if (roomPlans == null || roomPlans.Count == 0)
            {
                return;
            }

            roomPlans.RemoveAt(roomPlans.Count - 1);
            RefreshCreatorSummary();
        }

        public void AdoptRandomChild()
        {
            if (!supportsAdoption || householdManager == null)
            {
                return;
            }

            string childName = BuildRandomChildName();
            string characterId = $"adopted_child_{Guid.NewGuid():N}".Substring(0, 22);
            LifeStage stage = PickRandomChildLifeStage();
            CharacterTalent talent = PickRandomTalent();

            GameObject childObject = new($"Adopted_{childName.Replace(" ", string.Empty)}");
            CharacterCore child = childObject.AddComponent<CharacterCore>();
            child.Initialize(characterId, childName, stage, CharacterSpecies.Human);
            child.SetTalents(new List<CharacterTalent> { talent });
            child.SetBirthDate(1, UnityEngine.Random.Range(1, 12), UnityEngine.Random.Range(1, 28));
            householdManager.AddMember(child);

            adoptionIntents.Add(new AdoptionIntentSnapshot
            {
                IntentId = $"intent_child_{Guid.NewGuid():N}".Substring(0, 20),
                CandidateName = childName,
                Category = "Child Adoption",
                Detail = $"{stage} • Talent: {talent}"
            });

            PublishAdoptionEvent($"Adopted child {childName} ({stage}).");
            RefreshCreatorSummary();
        }

        public void AdoptRandomPet()
        {
            if (!supportsAdoption || householdManager == null)
            {
                return;
            }

            (string species, string breed) = PickRandomPetBreed();
            string petName = BuildRandomPetName();
            string petId = $"adopted_pet_{Guid.NewGuid():N}".Substring(0, 20);
            householdManager.RegisterPet(petId, petName, species, breed);
            includesPets = true;

            adoptionIntents.Add(new AdoptionIntentSnapshot
            {
                IntentId = $"intent_pet_{Guid.NewGuid():N}".Substring(0, 20),
                CandidateName = petName,
                Category = "Pet Adoption",
                Detail = $"{species} • {breed}"
            });

            PublishAdoptionEvent($"Adopted pet {petName} ({species}, {breed}).");
            RefreshCreatorSummary();
        }

        public void NextHouseholdCharacter()
        {
            householdManager?.CycleToNextCharacter();
            RefreshCreatorSummary();
        }

        public void PreviousHouseholdCharacter()
        {
            if (householdManager == null || householdManager.Members.Count == 0)
            {
                return;
            }

            IReadOnlyList<CharacterCore> members = householdManager.Members;
            CharacterCore active = householdManager.ActiveCharacter;
            int activeIndex = active != null ? IndexOfMember(members, active) : 0;
            if (activeIndex < 0)
            {
                householdManager.SetActiveCharacter(members[0]);
                RefreshCreatorSummary();
                return;
            }

            int previous = (activeIndex - 1 + members.Count) % members.Count;
            householdManager.SetActiveCharacter(members[previous]);
            RefreshCreatorSummary();
        }

        public void SetActiveArtPivot(int index)
        {
            if (characterArtPivots.Count == 0)
            {
                activeArtPivotIndex = 0;
                return;
            }

            activeArtPivotIndex = ((index % characterArtPivots.Count) + characterArtPivots.Count) % characterArtPivots.Count;
        }

        public void NextArtPivot()
        {
            SetActiveArtPivot(activeArtPivotIndex + 1);
        }

        public void PreviousArtPivot()
        {
            SetActiveArtPivot(activeArtPivotIndex - 1);
        }

        public void RotateCharacter(float direction)
        {
            if (Mathf.Approximately(direction, 0f))
            {
                return;
            }

            float step = Time.deltaTime > 0f ? Time.deltaTime : (1f / 60f);
            float rotationAmount = direction * rotateSpeed * step;
            bool rotatedAny = false;

            if (rotateAllArtPivots && characterArtPivots.Count > 0)
            {
                for (int i = 0; i < characterArtPivots.Count; i++)
                {
                    Transform pivot = characterArtPivots[i];
                    if (pivot == null)
                    {
                        continue;
                    }

                    pivot.Rotate(Vector3.up, rotationAmount, Space.World);
                    rotatedAny = true;
                }
            }
            else
            {
                Transform selected = ResolveActivePivot();
                if (selected != null)
                {
                    selected.Rotate(Vector3.up, rotationAmount, Space.World);
                    rotatedAny = true;
                }
            }

            if (!rotatedAny && characterPivot != null)
            {
                characterPivot.Rotate(Vector3.up, rotationAmount, Space.World);
            }
        }

        public void ZoomCamera(float delta)
        {
            if (characterCamera == null)
            {
                return;
            }

            if (characterCamera.orthographic)
            {
                characterCamera.orthographicSize = Mathf.Clamp(characterCamera.orthographicSize - delta * zoomSpeed * Time.deltaTime, minZoom, maxZoom);
            }
            else
            {
                characterCamera.fieldOfView = Mathf.Clamp(characterCamera.fieldOfView - delta * zoomSpeed * 5f * Time.deltaTime, 25f, 80f);
            }
        }

        public void ValidateHouseholdGenetics()
        {
            if (householdManager == null)
            {
                return;
            }

            int repaired = 0;
            for (int i = 0; i < householdManager.Members.Count; i++)
            {
                CharacterCore member = householdManager.Members[i];
                if (member == null)
                {
                    continue;
                }

                GeneticsSystem genetics = member.GetComponent<GeneticsSystem>();
                if (genetics == null)
                {
                    continue;
                }

                if (genetics.ValidateGeneticsConsistency())
                {
                    repaired++;
                }
            }

            (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
            {
                Type = SimulationEventType.MenuScreenChanged,
                Severity = SimulationEventSeverity.Info,
                SystemName = nameof(HouseholdMakerScreenController),
                ChangeKey = "GeneticsValidation",
                Reason = $"Validated household genetics. Repaired {repaired} character profiles.",
                Magnitude = repaired
            });

            RefreshCreatorSummary();
        }

        public void Back()
        {
            menuFlowController?.Back();
        }

        public void StartGame()
        {
            menuFlowController?.ContinueFromHousehold();
        }

        public void ToggleLockActiveCharacter()
        {
            CharacterCore active = householdManager != null ? householdManager.ActiveCharacter : null;
            if (active == null || string.IsNullOrWhiteSpace(active.CharacterId))
            {
                return;
            }

            if (!lockedCharacterIds.Add(active.CharacterId))
            {
                lockedCharacterIds.Remove(active.CharacterId);
            }

            RefreshCreatorSummary();
        }

        public void LockFamilyDraft()
        {
            familyDraftLocked = true;
            RefreshCreatorSummary();
        }

        public void UnlockFamilyDraft()
        {
            familyDraftLocked = false;
            RefreshCreatorSummary();
        }

        public void SaveFamilyDraft(string slotId)
        {
            if (householdManager == null || string.IsNullOrWhiteSpace(slotId))
            {
                return;
            }

            HouseholdDraftSnapshot snapshot = new HouseholdDraftSnapshot
            {
                ActiveCharacterId = householdManager.ActiveCharacter != null ? householdManager.ActiveCharacter.CharacterId : null,
                ActiveTab = CurrentTab.ToString(),
                FamilyLocked = familyDraftLocked,
                FamilySurname = familySurname,
                HomeDistrict = homeDistrict,
                HouseholdStoryPrompt = householdStoryPrompt,
                OriginFocus = originFocus.ToString(),
                PlanningPriority = planningPriority.ToString(),
                LotType = lotType.ToString(),
                BudgetTier = budgetTier.ToString(),
                HouseholdVibe = householdVibe.ToString(),
                ConflictApproach = conflictApproach.ToString(),
                WantsChildrenSoon = wantsChildrenSoon,
                IncludesPets = includesPets,
                PrioritizesCareerMobility = prioritizesCareerMobility,
                GenerationalGoal = generationalGoal,
                HouseholdArchetype = householdArchetype.ToString(),
                CommuteStrategy = commuteStrategy,
                DailyRhythm = dailyRhythm
            };

            for (int i = 0; i < householdManager.Members.Count; i++)
            {
                CharacterCore member = householdManager.Members[i];
                if (member == null)
                {
                    continue;
                }

                snapshot.Members.Add(new HouseholdDraftMemberSnapshot
                {
                    CharacterId = member.CharacterId,
                    DisplayName = member.DisplayName,
                    LifeStage = member.CurrentLifeStage.ToString(),
                    Locked = !string.IsNullOrWhiteSpace(member.CharacterId) && lockedCharacterIds.Contains(member.CharacterId)
                });
            }

            if (roomPlans != null)
            {
                snapshot.RoomPlans.AddRange(roomPlans);
            }

            if (householdManager.Pets != null)
            {
                for (int i = 0; i < householdManager.Pets.Count; i++)
                {
                    HouseholdPetProfile pet = householdManager.Pets[i];
                    if (pet == null)
                    {
                        continue;
                    }

                    snapshot.Pets.Add(new HouseholdDraftPetSnapshot
                    {
                        PetId = pet.PetId,
                        DisplayName = pet.Name,
                        Species = pet.Species,
                        Breed = pet.Breed
                    });
                }
            }

            snapshot.AdoptionIntents = new List<AdoptionIntentSnapshot>(adoptionIntents);
            snapshot.PetCreatorSpecies = petCreatorSpeciesCatalog.Count > 0 ? petCreatorSpeciesCatalog[Mathf.Clamp(petCreatorSpeciesIndex, 0, petCreatorSpeciesCatalog.Count - 1)] : null;
            snapshot.PetCreatorBreed = petCreatorBreedCatalog.Count > 0 ? petCreatorBreedCatalog[Mathf.Clamp(petCreatorBreedIndex, 0, petCreatorBreedCatalog.Count - 1)] : null;
            snapshot.PetCreatorName = petCreatorName;
            snapshot.PetCreatorTemperament = petCreatorTemperament.ToString();

            PlayerPrefs.SetString(BuildHouseholdDraftKey(slotId), JsonUtility.ToJson(snapshot));
            PlayerPrefs.Save();
            RefreshCreatorSummary();
        }

        public bool LoadFamilyDraft(string slotId)
        {
            if (string.IsNullOrWhiteSpace(slotId))
            {
                return false;
            }

            string key = BuildHouseholdDraftKey(slotId);
            if (!PlayerPrefs.HasKey(key))
            {
                return false;
            }

            HouseholdDraftSnapshot snapshot = JsonUtility.FromJson<HouseholdDraftSnapshot>(PlayerPrefs.GetString(key));
            if (snapshot == null)
            {
                return false;
            }

            familyDraftLocked = snapshot.FamilyLocked;
            familySurname = string.IsNullOrWhiteSpace(snapshot.FamilySurname) ? familySurname : snapshot.FamilySurname;
            homeDistrict = string.IsNullOrWhiteSpace(snapshot.HomeDistrict) ? homeDistrict : snapshot.HomeDistrict;
            householdStoryPrompt = string.IsNullOrWhiteSpace(snapshot.HouseholdStoryPrompt) ? householdStoryPrompt : snapshot.HouseholdStoryPrompt;

            if (Enum.TryParse(snapshot.OriginFocus, out HouseholdOriginFocus loadedOrigin))
            {
                originFocus = loadedOrigin;
            }

            if (Enum.TryParse(snapshot.PlanningPriority, out FamilyPlanningPriority loadedPriority))
            {
                planningPriority = loadedPriority;
            }

            if (Enum.TryParse(snapshot.LotType, out HouseholdLotType loadedLotType))
            {
                lotType = loadedLotType;
            }

            if (Enum.TryParse(snapshot.BudgetTier, out HouseholdBudgetTier loadedBudgetTier))
            {
                budgetTier = loadedBudgetTier;
            }

            if (Enum.TryParse(snapshot.HouseholdVibe, out HouseholdVibe loadedVibe))
            {
                householdVibe = loadedVibe;
            }

            if (Enum.TryParse(snapshot.ConflictApproach, out FamilyConflictApproach loadedConflictApproach))
            {
                conflictApproach = loadedConflictApproach;
            }

            wantsChildrenSoon = snapshot.WantsChildrenSoon;
            includesPets = snapshot.IncludesPets;
            prioritizesCareerMobility = snapshot.PrioritizesCareerMobility;
            generationalGoal = string.IsNullOrWhiteSpace(snapshot.GenerationalGoal) ? generationalGoal : snapshot.GenerationalGoal;
            commuteStrategy = string.IsNullOrWhiteSpace(snapshot.CommuteStrategy) ? commuteStrategy : snapshot.CommuteStrategy;
            dailyRhythm = string.IsNullOrWhiteSpace(snapshot.DailyRhythm) ? dailyRhythm : snapshot.DailyRhythm;

            if (Enum.TryParse(snapshot.HouseholdArchetype, out HouseholdArchetype loadedArchetype))
            {
                householdArchetype = loadedArchetype;
            }

            roomPlans = snapshot.RoomPlans != null ? new List<HouseholdRoomPlan>(snapshot.RoomPlans) : new List<HouseholdRoomPlan>();
            adoptionIntents.Clear();
            if (snapshot.AdoptionIntents != null)
            {
                adoptionIntents.AddRange(snapshot.AdoptionIntents);
            }

            ApplyPetCreatorSnapshot(snapshot);

            if (snapshot.Pets != null && householdManager != null)
            {
                for (int i = 0; i < snapshot.Pets.Count; i++)
                {
                    HouseholdDraftPetSnapshot pet = snapshot.Pets[i];
                    if (pet == null || string.IsNullOrWhiteSpace(pet.PetId))
                    {
                        continue;
                    }

                    householdManager.RegisterPet(pet.PetId, pet.DisplayName, pet.Species, pet.Breed);
                }
            }

            lockedCharacterIds.Clear();
            for (int i = 0; i < snapshot.Members.Count; i++)
            {
                HouseholdDraftMemberSnapshot member = snapshot.Members[i];
                if (member != null && member.Locked && !string.IsNullOrWhiteSpace(member.CharacterId))
                {
                    lockedCharacterIds.Add(member.CharacterId);
                }
            }

            if (Enum.TryParse(snapshot.ActiveTab, out HouseholdMakerTab tab))
            {
                SetTab((int)tab);
            }

            if (householdManager != null && !string.IsNullOrWhiteSpace(snapshot.ActiveCharacterId))
            {
                for (int i = 0; i < householdManager.Members.Count; i++)
                {
                    CharacterCore member = householdManager.Members[i];
                    if (member != null && member.CharacterId == snapshot.ActiveCharacterId)
                    {
                        householdManager.SetActiveCharacter(member);
                        break;
                    }
                }
            }

            RefreshCreatorSummary();
            return true;
        }

        private void HandleActiveCharacterChanged(CharacterCore character)
        {
            if (characterNameText != null)
            {
                characterNameText.text = character != null ? character.DisplayName : "No Active Character";
            }

            RefreshCreatorSummary();
        }

        private void RefreshTabUi()
        {
            if (tabText != null)
            {
                tabText.text = CurrentTab.ToString();
            }

            for (int i = 0; i < tabPanels.Count; i++)
            {
                HouseholdMakerTabPanel panel = tabPanels[i];
                if (panel?.Root == null)
                {
                    continue;
                }

                panel.Root.SetActive(panel.Tab == CurrentTab);
            }
        }

        private void RefreshCreatorSummary()
        {
            if (creatorSummaryText == null)
            {
                return;
            }

            int memberCount = householdManager != null ? householdManager.Members.Count : 0;
            string activeName = householdManager != null && householdManager.ActiveCharacter != null
                ? householdManager.ActiveCharacter.DisplayName
                : "None";

            creatorSummaryText.text =
                $"Tab: {CurrentTab}\n" +
                $"Members: {memberCount}\n" +
                $"Active: {activeName}\n" +
                $"Surname: {familySurname}\n" +
                $"Home District: {homeDistrict}\n" +
                $"Lot Type: {lotType}\n" +
                $"Budget Tier: {budgetTier}\n" +
                $"Household Vibe: {householdVibe}\n" +
                $"Origin Focus: {originFocus}\n" +
                $"Planning Priority: {planningPriority}\n" +
                $"Conflict Approach: {conflictApproach}\n" +
                $"Children Plan: {(wantsChildrenSoon ? "Trying Soon" : "Not Soon")}\n" +
                $"Pet Plan: {(includesPets ? "Pet Friendly" : "No Pets Planned")}\n" +
                $"Adoption: {(supportsAdoption ? "Enabled" : "Disabled")}\n" +
                $"Pet Creator: {BuildPetCreatorSummary()}\n" +
                $"Career Mobility: {(prioritizesCareerMobility ? "High Priority" : "Balanced")}\n" +
                $"Household Archetype: {householdArchetype}\n" +
                $"Locked Characters: {lockedCharacterIds.Count}\n" +
                $"Family Draft Locked: {(familyDraftLocked ? "Yes" : "No")}\n" +
                $"Preview Assets: {characterArtPivots.Count}\n" +
                $"Pivot Mode: {(rotateAllArtPivots ? "Rotate All" : "Rotate Active")}\n" +
                $"Generational Goal: {generationalGoal}\n" +
                $"Commute Strategy: {commuteStrategy}\n" +
                $"Daily Rhythm: {dailyRhythm}\n" +
                $"Room Blueprint:\n{BuildRoomPlanSummary()}\n" +
                $"Household Composition:\n{BuildMemberCompositionSummary()}\n" +
                $"Household Pets:\n{BuildPetCompositionSummary()}\n" +
                $"Adoption Log:\n{BuildAdoptionIntentSummary()}";

            if (familyLockStateText != null)
            {
                familyLockStateText.text = familyDraftLocked ? "Family Locked In" : "Family Draft Editable";
            }

            if (familyVisionText != null)
            {
                familyVisionText.text =
                    $"{familySurname} • {homeDistrict} • {lotType}\n" +
                    $"{originFocus} / {planningPriority} / {householdVibe} / {householdArchetype}\n" +
                    $"{householdStoryPrompt}\n" +
                    $"Conflict: {conflictApproach} • Budget: {budgetTier} • Next Gen: {(wantsChildrenSoon ? "Soon" : "Later")} • Adoption: {(supportsAdoption ? "Ready" : "Off")} • Pet Creator: {BuildPetCreatorSummary()}";
            }
        }

        private string BuildRoomPlanSummary()
        {
            if (roomPlans == null || roomPlans.Count == 0)
            {
                return "- No room plans";
            }

            var lines = new List<string>();
            for (int i = 0; i < roomPlans.Count; i++)
            {
                HouseholdRoomPlan plan = roomPlans[i];
                if (plan == null)
                {
                    continue;
                }

                string notes = string.IsNullOrWhiteSpace(plan.Notes) ? "No notes" : plan.Notes;
                lines.Add($"- {plan.RoomType} | Style: {plan.StyleDirection} | Priority: {plan.Priority}/5 | {notes}");
            }

            return lines.Count > 0 ? string.Join("\n", lines) : "- No room plans";
        }

        private string BuildMemberCompositionSummary()
        {
            if (householdManager == null || householdManager.Members.Count == 0)
            {
                return "- No members";
            }

            var lines = new List<string>();
            for (int i = 0; i < householdManager.Members.Count; i++)
            {
                CharacterCore member = householdManager.Members[i];
                if (member == null)
                {
                    continue;
                }

                string lockTag = !string.IsNullOrWhiteSpace(member.CharacterId) && lockedCharacterIds.Contains(member.CharacterId) ? "Locked" : "Editable";
                string talent = member.Talents.Count > 0 ? member.Talents[0].ToString() : "None";
                lines.Add($"- {member.DisplayName} ({member.CurrentLifeStage}, {member.Species}) • Talent: {talent} • {lockTag}");
            }

            return lines.Count > 0 ? string.Join("\n", lines) : "- No members";
        }

        private string BuildPetCompositionSummary()
        {
            if (householdManager == null || householdManager.Pets == null || householdManager.Pets.Count == 0)
            {
                return "- No pets";
            }

            var lines = new List<string>();
            for (int i = 0; i < householdManager.Pets.Count; i++)
            {
                HouseholdPetProfile pet = householdManager.Pets[i];
                if (pet == null)
                {
                    continue;
                }

                string breed = string.IsNullOrWhiteSpace(pet.Breed) ? "Mixed" : pet.Breed;
                lines.Add($"- {pet.Name} ({pet.Species}, {breed}) • Bond {pet.BondLevel:0}% • Happiness {pet.Happiness:0}%");
            }

            return lines.Count > 0 ? string.Join("\n", lines) : "- No pets";
        }

        private string BuildAdoptionIntentSummary()
        {
            if (adoptionIntents.Count == 0)
            {
                return "- No adoption activity";
            }

            var lines = new List<string>();
            for (int i = 0; i < adoptionIntents.Count; i++)
            {
                AdoptionIntentSnapshot intent = adoptionIntents[i];
                if (intent == null)
                {
                    continue;
                }

                lines.Add($"- {intent.Category}: {intent.CandidateName} • {intent.Detail}");
            }

            return lines.Count > 0 ? string.Join("\n", lines) : "- No adoption activity";
        }

        private string BuildPetCreatorSummary()
        {
            string species = petCreatorSpeciesCatalog.Count > 0 ? petCreatorSpeciesCatalog[Mathf.Clamp(petCreatorSpeciesIndex, 0, petCreatorSpeciesCatalog.Count - 1)] : "Species";
            string breed = petCreatorBreedCatalog.Count > 0 ? petCreatorBreedCatalog[Mathf.Clamp(petCreatorBreedIndex, 0, petCreatorBreedCatalog.Count - 1)] : "Breed";
            string name = string.IsNullOrWhiteSpace(petCreatorName) ? "Custom Name" : petCreatorName;
            return $"{name} ({species}, {breed}, {petCreatorTemperament})";
        }

        private void EnsureAdoptionPoolSeeded()
        {
            adoptionPoolSize = Mathf.Max(1, adoptionPoolSize);
            petCreatorSpeciesIndex = Mathf.Clamp(petCreatorSpeciesIndex, 0, Mathf.Max(0, petCreatorSpeciesCatalog.Count - 1));
            petCreatorBreedIndex = Mathf.Clamp(petCreatorBreedIndex, 0, Mathf.Max(0, petCreatorBreedCatalog.Count - 1));
        }

        private void ApplyPetCreatorSnapshot(HouseholdDraftSnapshot snapshot)
        {
            if (snapshot == null)
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(snapshot.PetCreatorName))
            {
                petCreatorName = snapshot.PetCreatorName;
            }

            if (!string.IsNullOrWhiteSpace(snapshot.PetCreatorTemperament) &&
                Enum.TryParse(snapshot.PetCreatorTemperament, out HouseholdPetTemperament loadedTemperament))
            {
                petCreatorTemperament = loadedTemperament;
            }

            if (!string.IsNullOrWhiteSpace(snapshot.PetCreatorSpecies) && petCreatorSpeciesCatalog != null)
            {
                int idx = petCreatorSpeciesCatalog.FindIndex(x => string.Equals(x, snapshot.PetCreatorSpecies, StringComparison.OrdinalIgnoreCase));
                if (idx >= 0) petCreatorSpeciesIndex = idx;
            }

            if (!string.IsNullOrWhiteSpace(snapshot.PetCreatorBreed) && petCreatorBreedCatalog != null)
            {
                int idx = petCreatorBreedCatalog.FindIndex(x => string.Equals(x, snapshot.PetCreatorBreed, StringComparison.OrdinalIgnoreCase));
                if (idx >= 0) petCreatorBreedIndex = idx;
            }
        }

        private static LifeStage PickRandomChildLifeStage()
        {
            LifeStage[] stages = { LifeStage.Baby, LifeStage.Infant, LifeStage.Toddler, LifeStage.Child, LifeStage.Preteen, LifeStage.Teen };
            return stages[UnityEngine.Random.Range(0, stages.Length)];
        }

        private static CharacterTalent PickRandomTalent()
        {
            CharacterTalent[] talents = { CharacterTalent.Artistic, CharacterTalent.Athletic, CharacterTalent.Social, CharacterTalent.Academic, CharacterTalent.Musical, CharacterTalent.Technical, CharacterTalent.Culinary, CharacterTalent.Caregiving, CharacterTalent.Performer };
            return talents[UnityEngine.Random.Range(0, talents.Length)];
        }

        private static string BuildRandomChildName()
        {
            string[] first = { "Avery", "Mila", "Noah", "Ezra", "Zoe", "Kai", "Luca", "Lina", "Ivy", "Mason", "Nia", "Owen", "Ruby", "Theo", "June", "Aria", "Nova", "Remy", "Skye", "Piper", "Rory", "Ellis", "Milo", "Sage", "Kira", "Quinn", "Rylan", "Lilah", "Cora", "Jules" };
            string[] last = { "Rivera", "Nguyen", "Patel", "Jackson", "Diaz", "Kim", "Singh", "Morgan", "Parker", "Reed", "Bennett", "Flores", "Walker", "Young", "Santos", "Howard", "Bailey", "Mitchell", "Campbell", "Brooks" };
            return $"{first[UnityEngine.Random.Range(0, first.Length)]} {last[UnityEngine.Random.Range(0, last.Length)]}";
        }

        private static string BuildRandomPetName()
        {
            string[] names = { "Mochi", "Biscuit", "Pepper", "Comet", "Noodle", "Maple", "Scout", "Luna", "Sparky", "Pudding", "Miso", "Pickles", "Juno", "Nori", "Otis", "Clover", "Waffles", "Ziggy", "Sunny", "Tofu", "Bean", "Echo", "Poppy", "Cosmo", "Pebble", "Mocha", "Bandit", "Pico", "Mango", "Nova" };
            return names[UnityEngine.Random.Range(0, names.Length)];
        }

        private static (string species, string breed) PickRandomPetBreed()
        {
            (string species, string breed)[] options =
            {
                ("Dog", "Labrador"), ("Dog", "Shiba Inu"), ("Dog", "Corgi"), ("Dog", "Mixed Rescue"), ("Dog", "German Shepherd"),
                ("Cat", "Domestic Shorthair"), ("Cat", "Maine Coon"), ("Cat", "Siamese"), ("Cat", "Ragdoll"), ("Cat", "British Shorthair"),
                ("Rabbit", "Holland Lop"), ("Rabbit", "Mini Rex"), ("Bird", "Cockatiel"), ("Bird", "Parakeet"), ("Hamster", "Syrian"),
                ("Guinea Pig", "Abyssinian"), ("Ferret", "Standard"), ("Turtle", "Painted Turtle"), ("Dog", "Poodle"), ("Cat", "Bengal"),
                ("Dog", "Beagle"), ("Cat", "Sphynx"), ("Rabbit", "Lionhead"), ("Bird", "Canary"), ("Dog", "Husky"),
                ("Dog", "Golden Retriever"), ("Cat", "Persian"), ("Dog", "Pit Mix"), ("Cat", "Tabby"), ("Dog", "Boxer")
            };

            return options[UnityEngine.Random.Range(0, options.Length)];
        }

        private void PublishAdoptionEvent(string reason)
        {
            (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
            {
                Type = SimulationEventType.ActivityCompleted,
                Severity = SimulationEventSeverity.Info,
                SystemName = nameof(HouseholdMakerScreenController),
                ChangeKey = "Adoption",
                Reason = reason,
                Magnitude = adoptionIntents.Count
            });
        }

        private void PublishTabEvent()
        {
            (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
            {
                Type = SimulationEventType.MenuScreenChanged,
                Severity = SimulationEventSeverity.Info,
                SystemName = nameof(HouseholdMakerScreenController),
                ChangeKey = CurrentTab.ToString(),
                Reason = "Household maker tab changed",
                Magnitude = (int)CurrentTab
            });
        }

        private Transform ResolveActivePivot()
        {
            if (characterArtPivots.Count > 0)
            {
                int idx = ActiveArtPivotIndex;
                return idx >= 0 && idx < characterArtPivots.Count ? characterArtPivots[idx] : null;
            }

            return characterPivot;
        }

        private static int IndexOfMember(IReadOnlyList<CharacterCore> members, CharacterCore target)
        {
            for (int i = 0; i < members.Count; i++)
            {
                if (members[i] == target)
                {
                    return i;
                }
            }

            return -1;
        }

        private static string BuildHouseholdDraftKey(string slotId)
        {
            return $"household_draft_{slotId}";
        }
    }
}

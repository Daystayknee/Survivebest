using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using Survivebest.Commerce;
using Survivebest.Core;
using Survivebest.Events;
using Survivebest.Health;
using Survivebest.Location;
using Survivebest.Needs;
using Survivebest.Status;
using Survivebest.Minigames;
using Survivebest.Quest;
using Survivebest.World;
using Survivebest.NPC;

namespace Survivebest.UI
{
    [Serializable]
    public class AnimalSightingEncounter
    {
        public string SightingName;
        public string AnimalName;
        public string AnimalSpecies;
        [TextArea] public string Description;
        [Range(0f, 1f)] public float Difficulty = 0.4f;
        [Min(0)] public int Payment = 35;
        [Range(-20f, 20f)] public float EnergyDelta = -6f;
        [Range(-20f, 20f)] public float HygieneDelta = -4f;
        [Range(-20f, 20f)] public float MoodDelta = 8f;
        [Range(-20f, 20f)] public float HydrationDelta = -2f;
        public string RewardStatusId = "status_020";
        public string PenaltyStatusId = "status_200";
        public Sprite AnimalPreview;
    }

    public class ActionPopupController : MonoBehaviour
    {
        [SerializeField] private SidebarContextMenu sidebarContextMenu;
        [SerializeField] private HouseholdManager householdManager;
        [SerializeField] private LocationManager locationManager;
        [SerializeField] private GrocerySystem grocerySystem;
        [SerializeField] private OrderingSystem orderingSystem;
        [SerializeField] private RecipeSystem recipeSystem;
        [SerializeField] private GameEventHub gameEventHub;
        [SerializeField] private MinigameManager minigameManager;
        [SerializeField] private QuestOpportunitySystem questOpportunitySystem;
        [SerializeField] private WorldGuideAISystem worldGuideAISystem;
        [SerializeField] private NpcLifeAIGuideSystem npcLifeAIGuideSystem;
        [SerializeField] private GameplayVisionSystem gameplayVisionSystem;
        [SerializeField] private HealthcareGameplaySystem healthcareGameplaySystem;

        [Header("Popup UI")]
        [SerializeField] private GameObject popupRoot;
        [SerializeField] private Text titleText;
        [SerializeField] private Text bodyText;
        [SerializeField] private Text optionsText;
        [SerializeField] private Image animalPreviewImage;
        [SerializeField] private Text animalPreviewLabel;

        [Header("Animal Sightings")]
        [SerializeField] private List<AnimalSightingEncounter> animalSightingEncounters = new()
        {
            new AnimalSightingEncounter
            {
                SightingName = "Dawn Deer Crossing",
                AnimalName = "White-tail Doe",
                AnimalSpecies = "Deer",
                Description = "Track hoof marks near the creek and quietly observe the herd at sunrise.",
                Difficulty = 0.25f,
                Payment = 30,
                EnergyDelta = -3f,
                HygieneDelta = -1f,
                MoodDelta = 8f,
                HydrationDelta = -1f,
                RewardStatusId = "status_020",
                PenaltyStatusId = "status_205"
            },
            new AnimalSightingEncounter
            {
                SightingName = "Night Owl Watch",
                AnimalName = "Barn Owl",
                AnimalSpecies = "Bird",
                Description = "Stay quiet near the old mill and capture a clean sighting at dusk.",
                Difficulty = 0.45f,
                Payment = 44,
                EnergyDelta = -6f,
                HygieneDelta = -2f,
                MoodDelta = 10f,
                HydrationDelta = -2f,
                RewardStatusId = "status_060",
                PenaltyStatusId = "status_210"
            },
            new AnimalSightingEncounter
            {
                SightingName = "Wetland Croc Survey",
                AnimalName = "Marsh Crocodile",
                AnimalSpecies = "Reptile",
                Description = "Log movement patterns from a safe ridge without startling the animal.",
                Difficulty = 0.62f,
                Payment = 62,
                EnergyDelta = -7f,
                HygieneDelta = -3f,
                MoodDelta = 9f,
                HydrationDelta = -2f,
                RewardStatusId = "status_080",
                PenaltyStatusId = "status_215"
            },
            new AnimalSightingEncounter
            {
                SightingName = "Mountain Fox Trail",
                AnimalName = "Silver Fox",
                AnimalSpecies = "Mammal",
                Description = "Follow tracks in the highlands and report a verified visual sighting.",
                Difficulty = 0.55f,
                Payment = 57,
                EnergyDelta = -9f,
                HygieneDelta = -4f,
                MoodDelta = 11f,
                HydrationDelta = -3f,
                RewardStatusId = "status_100",
                PenaltyStatusId = "status_220"
            }
        };

        private string currentActionKey;
        private AnimalSightingEncounter currentSighting;

        public event Action<string, string, float> OnActionResolved;
        private readonly StringBuilder builder = new();

        private void OnEnable()
        {
            if (sidebarContextMenu != null)
            {
                sidebarContextMenu.OnSidebarOptionSelected += HandleSidebarOption;
            }

            SetPopupVisible(false);
        }

        private void OnDisable()
        {
            if (sidebarContextMenu != null)
            {
                sidebarContextMenu.OnSidebarOptionSelected -= HandleSidebarOption;
            }
        }

        private void HandleSidebarOption(string actionKey)
        {
            currentActionKey = actionKey;
            currentSighting = actionKey == "animal_sight" ? PickSighting() : null;
            SetPopupVisible(true);
            RefreshPopupContent(actionKey);
        }

        public void ConfirmAction()
        {
            if (string.IsNullOrWhiteSpace(currentActionKey))
            {
                return;
            }

            CharacterCore active = householdManager != null ? householdManager.ActiveCharacter : null;
            string reason;
            float magnitude = 1f;

            switch (currentActionKey)
            {
                case "buy":
                    reason = DoBuy();
                    break;
                case "sell":
                    reason = DoSell();
                    break;
                case "trade":
                    reason = "Trade completed. Reputation changed slightly.";
                    magnitude = 2f;
                    break;
                case "get_meds":
                    reason = ApplyMedicalAid(active);
                    magnitude = 4f;
                    break;
                case "see_doctor":
                    reason = DoSeeDoctor(active);
                    magnitude = 4f;
                    break;
                case "forage":
                    reason = DoForage();
                    magnitude = 2f;
                    break;
                case "camp":
                    reason = "Camp setup complete. Energy recovery boosted tonight.";
                    magnitude = 2f;
                    break;
                case "fish":
                    reason = DoFishing(active);
                    magnitude = 3f;
                    break;
                case "watch_tv":
                    reason = DoWatchTv(active);
                    magnitude = 2f;
                    break;
                case "watch_movie":
                case "go_movies":
                    reason = DoWatchMovie(active);
                    magnitude = 3f;
                    break;
                case "read_book":
                    reason = DoReadBook(active);
                    magnitude = 2f;
                    break;
                case "sing":
                    reason = DoSing(active);
                    magnitude = 2f;
                    break;
                case "cook_meal":
                    reason = DoCookMeal(active);
                    magnitude = 3f;
                    break;
                case "bake_food":
                    reason = DoBakeFood(active);
                    magnitude = 3f;
                    break;
                case "make_drink":
                    reason = DoMakeDrink(active);
                    magnitude = 2f;
                    break;
                case "use_bathroom":
                    reason = DoUseBathroom(active);
                    magnitude = 2f;
                    break;
                case "take_shower":
                    reason = DoTakeShower(active);
                    magnitude = 3f;
                    break;
                case "dry_off_towel":
                    reason = DoDryOffWithTowel(active);
                    magnitude = 1.5f;
                    break;
                case "clothing_store":
                    reason = DoClothingStore(active);
                    magnitude = 3f;
                    break;
                case "practice_skill":
                    reason = PracticeSkill(active, "Cooking", 4f);
                    magnitude = 4f;
                    break;
                case "train_skill":
                    reason = PracticeSkill(active, "Survival skills", 5f);
                    magnitude = 5f;
                    break;
                case "accept_local_opportunity":
                    reason = AcceptLocalOpportunity(active);
                    magnitude = 5f;
                    break;
                case "continue_local_opportunity":
                    reason = ContinueLocalOpportunity(active, out magnitude);
                    break;
                case "review_local_pulse":
                    reason = ReviewLocalPulse();
                    magnitude = 1.5f;
                    break;
                case "ask_world_ai":
                    reason = AskWorldAi();
                    magnitude = 1f;
                    break;
                case "npc_chat":
                    reason = ResolveNpcInteraction(false);
                    magnitude = 1.5f;
                    break;
                case "npc_text":
                    reason = ResolveNpcInteraction(true);
                    magnitude = 1.2f;
                    break;
                case "animal_sight":
                    reason = ResolveAnimalSighting(active, out magnitude);
                    break;
                case "talk_coworkers":
                    reason = "Coworker social interaction finished.";
                    magnitude = 1f;
                    break;
                case "schmooze_boss":
                    reason = "Boss relationship attempt performed.";
                    magnitude = 2f;
                    break;
                default:
                    reason = "Action completed.";
                    break;
            }

            PublishActionEvent(reason, magnitude);
            OnActionResolved?.Invoke(currentActionKey, reason, magnitude);
            SetPopupVisible(false);
        }

        public void CancelAction()
        {
            SetPopupVisible(false);
        }

        private void RefreshPopupContent(string actionKey)
        {
            if (titleText != null)
            {
                titleText.text = BuildTitle(actionKey);
            }

            if (bodyText != null)
            {
                bodyText.text = BuildVisionAwareDescription(actionKey);
            }

            if (optionsText != null)
            {
                optionsText.text = BuildVisionAwareOptions(actionKey);
            }

            RefreshAnimalPreview();
        }

        private string BuildTitle(string actionKey)
        {
            return actionKey switch
            {
                "buy" => "Store: Buy Items",
                "sell" => "Store: Sell Items",
                "trade" => "Trade Interaction",
                "get_meds" => "Medical: Get Meds",
                "see_doctor" => "Medical: See Doctor",
                "forage" => "Nature: Forage",
                "fish" => "Nature: Fishing",
                "camp" => "Nature: Camp",
                "watch_tv" => "Home: Watch TV",
                "watch_movie" => "Home: Movie Night",
                "go_movies" => "Outing: Go to Movies",
                "read_book" => "Home: Read Book",
                "sing" => "Home: Singing Session",
                "cook_meal" => "Kitchen: Cook Meal",
                "bake_food" => "Kitchen: Bake Food",
                "make_drink" => "Kitchen: Make Drink",
                "use_bathroom" => "Bathroom: Toilet",
                "take_shower" => "Bathroom: Shower",
                "dry_off_towel" => "Bathroom: Dry Off",
                "clothing_store" => "Store: Clothing Store",
                "animal_sight" => currentSighting != null ? $"Animal Sighting: {currentSighting.SightingName}" : "Animal Sighting",
                "practice_skill" => "Skill Practice",
                "train_skill" => "Skill Training",
                "accept_local_opportunity" => "Local Opportunity",
                "continue_local_opportunity" => "Continue Opportunity",
                "review_local_pulse" => "District Pulse",
                "ask_world_ai" => "World AI Guidance",
                "npc_chat" => "NPC Conversation",
                "npc_text" => "NPC Text Message",
                _ => "Action"
            };
        }

        private string BuildDescription(string actionKey)
        {
            return actionKey switch
            {
                "buy" => "Purchase food/supplies and add them to your pantry.",
                "sell" => "Sell selected pantry goods for money.",
                "trade" => "Exchange goods/social favor with NPC vendors.",
                "get_meds" => "Apply the right medicine, dressing, and at-home care loop based on the exact condition.",
                "see_doctor" => "Run a fuller clinic workflow with triage, wound care, casting, surgery prep, dermatology, or animal-care follow-up.",
                "forage" => "Explore wild zones and gather random ingredients.",
                "fish" => "Cast lines in rivers/lakes and bring home fish for meals or sale.",
                "camp" => "Set camp to restore comfort and safety overnight.",
                "watch_tv" => "Pick a show genre and decompress while recovering mood.",
                "watch_movie" => "Choose a movie vibe and enjoy a full focused entertainment block.",
                "go_movies" => "Travel out for a theater outing with stronger social/mood impact.",
                "read_book" => "Read fiction/non-fiction to unwind and build knowledge.",
                "sing" => "Run a singing session to raise confidence and expression.",
                "cook_meal" => "Cook an actual meal in the kitchen for hunger recovery and skill XP.",
                "bake_food" => "Bake snacks/desserts for comfort and cooking growth.",
                "make_drink" => "Prepare tea/coffee/smoothies for hydration and light mood boost.",
                "use_bathroom" => "Use the toilet to reset bladder pressure and avoid discomfort accidents.",
                "take_shower" => "Take a shower to recover hygiene, grooming, and confidence.",
                "dry_off_towel" => "Use a towel after shower to finish your routine and feel fresh.",
                "clothing_store" => "Buy a new outfit per character and improve appearance/mood.",
                "animal_sight" => BuildAnimalSightingDescription(),
                "practice_skill" => "Spend time to gain XP in an applied skill.",
                "train_skill" => "Focused training to gain bigger XP rewards.",
                "accept_local_opportunity" => "Accept the best currently available opportunity tied to this location.",
                "continue_local_opportunity" => "Advance an active local opportunity and cash in progress if possible.",
                "review_local_pulse" => "Pause and read the district pulse before committing your next move.",
                "ask_world_ai" => "Ask the world AI to synthesize local danger, opportunity, and routing advice.",
                "npc_chat" => "Talk to a nearby NPC using personality-aware conversation guidance.",
                "npc_text" => "Send a personality-aware text message to a nearby NPC contact.",
                _ => "Confirm to execute this action."
            };
        }

        private string BuildVisionAwareDescription(string actionKey)
        {
            Room room = locationManager != null ? locationManager.CurrentRoom : null;
            string core = BuildDescription(actionKey);
            if (gameplayVisionSystem == null)
            {
                return core;
            }

            return $"{gameplayVisionSystem.BuildVisionStatement(actionKey, room)}\n\n{core}";
        }

        private string BuildAnimalSightingDescription()
        {
            if (currentSighting == null)
            {
                return "Track a wildlife encounter for finder payout, mood boosts, and skill growth.";
            }

            return $"Care for {currentSighting.AnimalName} ({currentSighting.AnimalSpecies}). {currentSighting.Description}\nFinder payout: ${currentSighting.Payment}.";
        }

        private string BuildOptionsPreview(string actionKey)
        {
            builder.Clear();
            switch (actionKey)
            {
                case "buy":
                    builder.AppendLine("Preview Items:");
                    AppendTopCatalogItems(builder, 5);
                    break;
                case "sell":
                    builder.AppendLine("Pantry Items:");
                    AppendPantryItems(builder, 5);
                    break;
                case "animal_sight":
                    AppendAnimalSightingPreview(builder);
                    break;
                case "get_meds":
                case "see_doctor":
                    AppendMedicalPreview(builder, actionKey);
                    break;
                case "accept_local_opportunity":
                case "continue_local_opportunity":
                    AppendOpportunityPreview(builder, actionKey);
                    break;
                case "review_local_pulse":
                    builder.AppendLine(ReviewLocalPulse());
                    break;
                case "ask_world_ai":
                    builder.AppendLine(AskWorldAi());
                    break;
                case "npc_chat":
                    AppendNpcConversationPreview(builder, false);
                    break;
                case "npc_text":
                    AppendNpcConversationPreview(builder, true);
                    break;
                case "fish":
                    builder.AppendLine("Fishing options:");
                    builder.AppendLine("• River cast");
                    builder.AppendLine("• Lake shore cast");
                    builder.AppendLine("• Bait setup");
                    break;
                case "watch_tv":
                    builder.AppendLine("TV Genres:");
                    builder.AppendLine("• Sitcom • Drama • Documentary • Sports • Anime");
                    break;
                case "watch_movie":
                case "go_movies":
                    builder.AppendLine("Movie choices:");
                    builder.AppendLine("• Comedy • Action • Romance • Horror • Family");
                    break;
                case "read_book":
                    builder.AppendLine("Book shelf:");
                    builder.AppendLine("• Fantasy • Mystery • Sci-fi • Biography • Self-help");
                    break;
                case "sing":
                    builder.AppendLine("Singing set:");
                    builder.AppendLine("• Pop • R&B • Rock • Jazz • Acoustic");
                    break;
                case "cook_meal":
                case "bake_food":
                case "make_drink":
                    builder.AppendLine("Kitchen actions:");
                    builder.AppendLine("• Prep ingredients");
                    builder.AppendLine("• Time heat and finish");
                    builder.AppendLine("• Clean station");
                    break;
                case "use_bathroom":
                case "take_shower":
                case "dry_off_towel":
                    builder.AppendLine("Bathroom routine:");
                    builder.AppendLine("• Toilet → Shower → Towel");
                    builder.AppendLine("• Keeps hygiene + bladder in healthy range");
                    break;
                case "clothing_store":
                    builder.AppendLine("Outfit options:");
                    builder.AppendLine("• Casual • Workwear • Formal • Sport • Cozy");
                    builder.AppendLine("Costs funds, improves appearance/mood");
                    break;
                case "practice_skill":
                case "train_skill":
                    builder.AppendLine("Skill Actions:");
                    builder.AppendLine("• Cooking");
                    builder.AppendLine("• Survival skills");
                    builder.AppendLine("• Negotiation");
                    if (recipeSystem != null && recipeSystem.Recipes != null && recipeSystem.Recipes.Count > 0)
                    {
                        builder.AppendLine($"• Recipe Focus: {recipeSystem.Recipes[0].RecipeName}");
                    }
                    break;
                default:
                    builder.AppendLine("Press Confirm to continue.");
                    break;
            }

            return builder.ToString().TrimEnd();
        }

        private string BuildVisionAwareOptions(string actionKey)
        {
            string preview = BuildOptionsPreview(actionKey);
            if (gameplayVisionSystem == null)
            {
                return preview;
            }

            Room room = locationManager != null ? locationManager.CurrentRoom : null;
            List<string> tabs = gameplayVisionSystem.BuildTabsForContext(actionKey, room);
            if (tabs.Count == 0)
            {
                return preview;
            }

            builder.Clear();
            builder.AppendLine($"Section tabs: {string.Join(" • ", tabs)}");
            if (!string.IsNullOrWhiteSpace(preview))
            {
                builder.AppendLine();
                builder.Append(preview);
            }

            return builder.ToString().TrimEnd();
        }

        private void AppendOpportunityPreview(StringBuilder sb, string actionKey)
        {
            ActiveOpportunity opportunity = actionKey == "continue_local_opportunity"
                ? GetFirstAcceptedOpportunityForCurrentLocation()
                : GetFirstAvailableOpportunityForCurrentLocation();

            if (opportunity == null || opportunity.Definition == null)
            {
                sb.AppendLine("• No matching local opportunity is available.");
                return;
            }

            sb.AppendLine($"• {opportunity.Definition.Title}");
            sb.AppendLine($"• Reward: ${opportunity.Definition.RewardFunds}");
            sb.AppendLine($"• Deadline: {opportunity.DeadlineHour}h");
            sb.AppendLine($"• Objectives: {opportunity.Definition.Objectives.Count}");
        }

        private void AppendAnimalSightingPreview(StringBuilder sb)
        {
            if (currentSighting == null)
            {
                sb.AppendLine("• No gig selected.");
                return;
            }

            sb.AppendLine($"Target: {currentSighting.AnimalName} ({currentSighting.AnimalSpecies})");
            sb.AppendLine($"Difficulty: {(int)(currentSighting.Difficulty * 100f)}%");
            sb.AppendLine($"Payout: ${currentSighting.Payment}");
            sb.AppendLine($"Need impact: Energy {currentSighting.EnergyDelta:+0;-0;0}, Hygiene {currentSighting.HygieneDelta:+0;-0;0}, Mood {currentSighting.MoodDelta:+0;-0;0}");
            sb.AppendLine("Confirm to attempt the wildlife sighting.");
        }

        private void RefreshAnimalPreview()
        {
            bool show = currentActionKey == "animal_sight" && currentSighting != null;

            if (animalPreviewImage != null)
            {
                animalPreviewImage.gameObject.SetActive(show);
                if (show)
                {
                    animalPreviewImage.sprite = currentSighting.AnimalPreview;
                    animalPreviewImage.color = currentSighting.AnimalPreview != null ? Color.white : new Color(1f, 1f, 1f, 0.15f);
                }
            }

            if (animalPreviewLabel != null)
            {
                animalPreviewLabel.gameObject.SetActive(show);
                if (show)
                {
                    animalPreviewLabel.text = currentSighting.AnimalPreview != null
                        ? $"{currentSighting.AnimalName} • {currentSighting.AnimalSpecies}"
                        : $"{currentSighting.AnimalName} • {currentSighting.AnimalSpecies} (assign preview image)";
                }
            }
        }

        private ActiveOpportunity GetFirstAvailableOpportunityForCurrentLocation()
        {
            string locationId = locationManager != null && locationManager.CurrentRoom != null ? locationManager.CurrentRoom.RoomName : null;
            if (questOpportunitySystem == null || string.IsNullOrWhiteSpace(locationId))
            {
                return null;
            }

            IReadOnlyList<ActiveOpportunity> opportunities = questOpportunitySystem.GetAvailableOpportunitiesForLocation(locationId);
            return opportunities.Count > 0 ? opportunities[0] : null;
        }

        private ActiveOpportunity GetFirstAcceptedOpportunityForCurrentLocation()
        {
            string locationId = locationManager != null && locationManager.CurrentRoom != null ? locationManager.CurrentRoom.RoomName : null;
            if (questOpportunitySystem == null || string.IsNullOrWhiteSpace(locationId))
            {
                return null;
            }

            IReadOnlyList<ActiveOpportunity> opportunities = questOpportunitySystem.GetAcceptedOpportunitiesForLocation(locationId);
            return opportunities.Count > 0 ? opportunities[0] : null;
        }

        private string AcceptLocalOpportunity(CharacterCore active)
        {
            ActiveOpportunity opportunity = GetFirstAvailableOpportunityForCurrentLocation();
            if (opportunity == null || opportunity.Definition == null)
            {
                return "No local opportunities are available right now.";
            }

            bool accepted = questOpportunitySystem.AcceptOpportunity(opportunity.Definition.OpportunityId, active);
            return accepted
                ? $"Accepted local opportunity: {opportunity.Definition.Title}."
                : "Could not accept the local opportunity.";
        }

        private string ContinueLocalOpportunity(CharacterCore active, out float magnitude)
        {
            magnitude = 2f;
            ActiveOpportunity opportunity = GetFirstAcceptedOpportunityForCurrentLocation();
            if (opportunity == null || opportunity.Definition == null)
            {
                magnitude = 0f;
                return "No active local opportunity is ready to progress.";
            }

            if (opportunity.Definition.Objectives == null || opportunity.Definition.Objectives.Count == 0)
            {
                questOpportunitySystem.ResolveOpportunity(opportunity, true, "Location follow-through completed");
                magnitude = opportunity.Definition.RewardFunds;
                return $"Resolved {opportunity.Definition.Title}.";
            }

            OpportunityObjective objective = opportunity.Definition.Objectives[0];
            bool progressed = questOpportunitySystem.ProgressObjective(opportunity.Definition.OpportunityId, objective.ObjectiveId, 1);
            magnitude = progressed ? Mathf.Max(3f, opportunity.Definition.RewardFunds) : 0f;
            return progressed
                ? $"Advanced local opportunity: {opportunity.Definition.Title}."
                : $"Could not advance {opportunity.Definition.Title}.";
        }

        private string AskWorldAi()
        {
            Room room = locationManager != null ? locationManager.CurrentRoom : null;
            return worldGuideAISystem != null ? worldGuideAISystem.BuildGuidanceForRoom(room) : "World AI is offline.";
        }

        private string ResolveNpcInteraction(bool isTextMessage)
        {
            Room room = locationManager != null ? locationManager.CurrentRoom : null;
            return npcLifeAIGuideSystem != null ? npcLifeAIGuideSystem.BuildInteractionSummary(room, isTextMessage) : "NPC AI is offline.";
        }

        private void AppendNpcConversationPreview(StringBuilder sb, bool isTextMessage)
        {
            Room room = locationManager != null ? locationManager.CurrentRoom : null;
            if (npcLifeAIGuideSystem == null)
            {
                sb.AppendLine("• NPC AI is offline.");
                return;
            }

            List<NpcChatSuggestion> suggestions = npcLifeAIGuideSystem.BuildChatSuggestions(room, isTextMessage);
            if (suggestions.Count == 0)
            {
                sb.AppendLine(isTextMessage ? "• No one is available to text here." : "• No conversational target is available here.");
                return;
            }

            for (int i = 0; i < suggestions.Count; i++)
            {
                sb.AppendLine($"• {suggestions[i].Label}: {suggestions[i].PreviewText}");
            }
        }

        private string ReviewLocalPulse()
        {
            if (locationManager == null || locationManager.CurrentRoom == null)
            {
                return "No local district pulse is available.";
            }

            ActiveOpportunity available = GetFirstAvailableOpportunityForCurrentLocation();
            ActiveOpportunity accepted = GetFirstAcceptedOpportunityForCurrentLocation();
            if (available != null && available.Definition != null)
            {
                return $"Pulse: {locationManager.CurrentRoom.RoomName} has an available lead — {available.Definition.Title}.";
            }

            if (accepted != null && accepted.Definition != null)
            {
                return $"Pulse: Stay on task in {locationManager.CurrentRoom.RoomName} — {accepted.Definition.Title} is active.";
            }

            return $"Pulse: {locationManager.CurrentRoom.RoomName} feels quiet right now. Push skills, needs, or scouting.";
        }

        private AnimalSightingEncounter PickSighting()
        {
            if (animalSightingEncounters == null || animalSightingEncounters.Count == 0)
            {
                return null;
            }

            int index = UnityEngine.Random.Range(0, animalSightingEncounters.Count);
            return animalSightingEncounters[index];
        }

        private string ResolveAnimalSighting(CharacterCore active, out float magnitude)
        {
            magnitude = 2f;
            if (active == null)
            {
                return "No active character available for animal sighting.";
            }

            AnimalSightingEncounter gig = currentSighting ?? PickSighting();
            if (gig == null)
            {
                return "No animal sighting encounters are configured.";
            }

            NeedsSystem needs = active.GetComponent<NeedsSystem>();
            SkillSystem skills = active.GetComponent<SkillSystem>();
            StatusEffectSystem status = active.GetComponent<StatusEffectSystem>();

            float handlingSkill = 0f;
            if (skills != null && skills.SkillLevels.TryGetValue("Animal care", out float value))
            {
                handlingSkill = Mathf.Clamp01(value / 100f);
            }

            float moodBonus = needs != null ? Mathf.Clamp01(needs.Mood / 100f) * 0.08f : 0f;
            float successChance = Mathf.Clamp01(0.52f + handlingSkill * 0.35f + moodBonus - gig.Difficulty * 0.28f);
            bool success = UnityEngine.Random.value <= successChance;

            if (needs != null)
            {
                needs.ModifyEnergy(gig.EnergyDelta);
                needs.ModifyHygiene(gig.HygieneDelta);
                needs.ModifyMood(success ? gig.MoodDelta : -Mathf.Abs(gig.MoodDelta) * 0.6f);
                needs.RestoreHydration(gig.HydrationDelta);
            }

            if (skills != null)
            {
                skills.AddExperience("Animal care", success ? 5f : 2f);
                skills.AddExperience("Survival skills", success ? 2f : 1f);
            }

            if (orderingSystem != null && success)
            {
                orderingSystem.AddFunds(gig.Payment);
            }

            if (status != null)
            {
                if (success && !string.IsNullOrWhiteSpace(gig.RewardStatusId))
                {
                    status.ApplyStatusById(gig.RewardStatusId, 6);
                }
                else if (!success && !string.IsNullOrWhiteSpace(gig.PenaltyStatusId))
                {
                    status.ApplyStatusById(gig.PenaltyStatusId, 6);
                }
            }

            magnitude = success ? gig.Payment : -gig.Difficulty * 10f;
            return success
                ? $"Animal sighting success: {gig.AnimalName} is happy. Earned ${gig.Payment}."
                : $"Animal sighting failed: {gig.AnimalName} became stressed. Partial progress only.";
        }

        private void AppendTopCatalogItems(StringBuilder sb, int count)
        {
            string[] defaults = { "Bread", "Rice", "Chicken", "Tomato", "Milk" };
            for (int i = 0; i < count; i++)
            {
                string name = defaults[i % defaults.Length];
                sb.AppendLine($"• {name}");
            }
        }

        private void AppendPantryItems(StringBuilder sb, int count)
        {
            if (grocerySystem == null || grocerySystem.Pantry == null || grocerySystem.Pantry.Count == 0)
            {
                sb.AppendLine("• No pantry items yet");
                return;
            }

            int max = Mathf.Min(count, grocerySystem.Pantry.Count);
            for (int i = 0; i < max; i++)
            {
                InventoryEntry entry = grocerySystem.Pantry[i];
                if (entry != null)
                {
                    sb.AppendLine($"• {entry.ItemName} x{entry.Quantity}");
                }
            }
        }

        private string DoBuy()
        {
            if (grocerySystem == null)
            {
                return "No grocery system connected.";
            }

            string[] staples = { "Bread", "Rice", "Chicken", "Tomato", "Onion" };
            string selected = staples[UnityEngine.Random.Range(0, staples.Length)];
            grocerySystem.BuyIngredient(selected, UnityEngine.Random.Range(1, 4));
            bool paid = orderingSystem == null || orderingSystem.SpendFunds(5f);
            return paid
                ? $"Bought {selected} and added to pantry."
                : "Not enough money to buy right now.";
        }

        private string DoSell()
        {
            if (grocerySystem == null || grocerySystem.Pantry == null || grocerySystem.Pantry.Count == 0)
            {
                return "Nothing to sell.";
            }

            InventoryEntry item = grocerySystem.Pantry.FirstOrDefault(x => x != null && x.Quantity > 0);
            if (item == null)
            {
                return "No valid pantry item to sell.";
            }

            grocerySystem.ConsumeIngredient(item.ItemName, 1);
            orderingSystem?.AddFunds(8f);
            return $"Sold 1x {item.ItemName}.";
        }


        private string DoFishing(CharacterCore active)
        {
            minigameManager ??= MinigameManager.Instance;
            minigameManager?.StartMinigame(MinigameType.Fishing, active, _ => { });

            SkillSystem skills = active != null ? active.GetComponent<SkillSystem>() : null;
            skills?.AddExperience("Fishing", 4f);
            grocerySystem?.BuyIngredient("Fresh Fish", UnityEngine.Random.Range(1, 3));
            return "Fishing session complete. You brought back fresh fish.";
        }

        private string DoWatchTv(CharacterCore active)
        {
            string picked = LifeActivityCatalog.PickTvGenre();
            minigameManager ??= MinigameManager.Instance;
            minigameManager?.StartMinigame(MinigameType.TVMarathon, active, _ => { });
            NeedsSystem needs = active != null ? active.GetComponent<NeedsSystem>() : null;
            needs?.ModifyMood(6f);
            needs?.ModifyEnergy(2f);
            return $"Watched a {picked} TV block and relaxed.";
        }

        private string DoWatchMovie(CharacterCore active)
        {
            string picked = LifeActivityCatalog.PickMovieGenre();
            minigameManager ??= MinigameManager.Instance;
            minigameManager?.StartMinigame(MinigameType.MovieNight, active, _ => { });
            NeedsSystem needs = active != null ? active.GetComponent<NeedsSystem>() : null;
            needs?.ModifyMood(8f);
            needs?.ModifyEnergy(3f);
            return $"Movie night: enjoyed a {picked} feature.";
        }

        private string DoReadBook(CharacterCore active)
        {
            string picked = LifeActivityCatalog.PickBookGenre();
            minigameManager ??= MinigameManager.Instance;
            minigameManager?.StartMinigame(MinigameType.BookReading, active, _ => { });
            SkillSystem skills = active != null ? active.GetComponent<SkillSystem>() : null;
            skills?.AddExperience("Writing", 2f);
            return $"Read a {picked} book session.";
        }

        private string DoSing(CharacterCore active)
        {
            string style = LifeActivityCatalog.PickSingingStyle();
            minigameManager ??= MinigameManager.Instance;
            minigameManager?.StartMinigame(MinigameType.SingingSession, active, _ => { });
            SkillSystem skills = active != null ? active.GetComponent<SkillSystem>() : null;
            skills?.AddExperience("Singing", 3f);
            NeedsSystem needs = active != null ? active.GetComponent<NeedsSystem>() : null;
            needs?.ModifyMood(5f);
            return $"Singing session finished with {style} vocal practice.";
        }

        private string DoCookMeal(CharacterCore active)
        {
            minigameManager ??= MinigameManager.Instance;
            minigameManager?.StartMinigame(MinigameType.Cooking, active, _ => { });
            return PracticeSkill(active, "Cooking", 5f);
        }

        private string DoBakeFood(CharacterCore active)
        {
            minigameManager ??= MinigameManager.Instance;
            minigameManager?.StartMinigame(MinigameType.Baking, active, _ => { });
            return PracticeSkill(active, "Cooking", 4f);
        }

        private string DoMakeDrink(CharacterCore active)
        {
            minigameManager ??= MinigameManager.Instance;
            minigameManager?.StartMinigame(MinigameType.DrinkMixing, active, _ => { });
            SkillSystem skill = active != null ? active.GetComponent<SkillSystem>() : null;
            skill?.AddExperience("Cooking", 2f);
            NeedsSystem needs = active != null ? active.GetComponent<NeedsSystem>() : null;
            needs?.RestoreHydration(12f);
            return "Prepared a drink and recovered hydration.";
        }


        private string DoUseBathroom(CharacterCore active)
        {
            NeedsSystem needs = active != null ? active.GetComponent<NeedsSystem>() : null;
            if (needs == null)
            {
                return "No active character for bathroom action.";
            }

            needs.ResetBladder();
            needs.ModifyMood(2f);
            return "Bathroom break complete.";
        }

        private string DoTakeShower(CharacterCore active)
        {
            NeedsSystem needs = active != null ? active.GetComponent<NeedsSystem>() : null;
            if (needs == null)
            {
                return "No active character for shower action.";
            }

            needs.ModifyHygiene(28f);
            needs.ModifyGrooming(6f);
            needs.ModifyAppearance(4f);
            needs.ModifyMood(5f);
            return "Shower complete. You should dry off with a towel.";
        }

        private string DoDryOffWithTowel(CharacterCore active)
        {
            NeedsSystem needs = active != null ? active.GetComponent<NeedsSystem>() : null;
            if (needs == null)
            {
                return "No active character for towel action.";
            }

            needs.ModifyHygiene(2f);
            needs.ModifyAppearance(2f);
            needs.ModifyMood(2f);
            return "Dried off and finished bathroom routine.";
        }

        private string DoClothingStore(CharacterCore active)
        {
            if (active == null)
            {
                return "No active character for clothing store action.";
            }

            bool paid = orderingSystem == null || orderingSystem.SpendFunds(22f);
            if (!paid)
            {
                return "Not enough money for new clothing right now.";
            }

            NeedsSystem needs = active.GetComponent<NeedsSystem>();
            needs?.ModifyAppearance(12f);
            needs?.ModifyMood(6f);
            needs?.ModifyGrooming(4f);

            string style = LifeActivityCatalog.PickRandomOutfitStyle(active.CurrentLifeStage);
            return $"Bought a {style} outfit and updated your closet.";
        }

        private string DoForage()
        {
            if (grocerySystem == null)
            {
                return "No inventory available for forage rewards.";
            }

            string[] forageItems = { "Mushroom", "Berry", "Herbs", "Wild onion", "Chestnut" };
            string gathered = forageItems[UnityEngine.Random.Range(0, forageItems.Length)];
            grocerySystem.BuyIngredient(gathered, UnityEngine.Random.Range(1, 3));
            return $"Foraged {gathered}.";
        }

        private string ApplyMedicalAid(CharacterCore active)
        {
            if (active == null)
            {
                return "No active character for medical aid.";
            }

            HealthSystem health = active.GetComponent<HealthSystem>();
            health?.Heal(4f);

            MedicalConditionSystem med = active.GetComponent<MedicalConditionSystem>();
            if (med != null && med.ActiveConditions.Count > 0)
            {
                MedicalCondition condition = med.ActiveConditions[0];
                med.AdministerMedication(condition.RecommendedMedication);
                med.HealCondition(condition.Id, 8);
                return $"Applied {condition.RecommendedMedication} support for {condition.DisplayName}.";
            }

            return "Applied preventive medicine. No active ailment found.";
        }

        private string DoSeeDoctor(CharacterCore active)
        {
            if (active == null)
            {
                return "No active character for doctor visit.";
            }

            MedicalConditionSystem med = active.GetComponent<MedicalConditionSystem>();
            if (med == null || med.ActiveConditions.Count == 0)
            {
                minigameManager?.StartMinigame(MinigameType.Triage, active, _ => { });
                return "Doctor visit completed with preventive triage and baseline wellness advice.";
            }

            MedicalCondition primary = med.ActiveConditions[0];
            HealthcareEncounterSession session = healthcareGameplaySystem != null ? healthcareGameplaySystem.BuildEncounterSession(primary) : null;
            HealthcareEncounterPlan plan = session != null ? session.Plan : healthcareGameplaySystem != null ? healthcareGameplaySystem.BuildPlan(primary) : null;
            if (plan != null && plan.Directives.Count > 0)
            {
                minigameManager?.StartMinigame(plan.Directives[0].InteractiveMinigame, active, _ => { });
            }

            med.HealCondition(primary.Id, plan != null && plan.NeedsHospitalization ? 6 : 10);
            return session != null && session.Providers.Count > 0 && session.Bookings.Count > 0
                ? $"Doctor visit completed: {plan.Directives[0].Title} with {session.Providers[0].DisplayName} in {session.Bookings[0].DisplayName} ({session.Bookings[0].RoomLabel}). {plan.FollowUpSummary}"
                : plan != null
                ? $"Doctor visit completed: {plan.Directives[0].Title}. {plan.FollowUpSummary}"
                : $"Doctor consult completed for {primary.DisplayName}.";
        }

        private void AppendMedicalPreview(StringBuilder stringBuilder, string actionKey)
        {
            CharacterCore active = householdManager != null ? householdManager.ActiveCharacter : null;
            MedicalConditionSystem medical = active != null ? active.GetComponent<MedicalConditionSystem>() : null;
            if (medical == null || medical.ActiveConditions.Count == 0)
            {
                stringBuilder.AppendLine("No active conditions. Preventive care only.");
                return;
            }

            MedicalCondition primary = medical.ActiveConditions[0];
            HealthcareEncounterPlan plan = healthcareGameplaySystem != null ? healthcareGameplaySystem.BuildPlan(primary) : null;
            stringBuilder.AppendLine($"Primary case: {primary.DisplayName}");
            stringBuilder.AppendLine(primary.DetailSummary);

            if (plan == null)
            {
                stringBuilder.AppendLine("No healthcare plan system connected.");
                return;
            }

            stringBuilder.AppendLine($"Triage: {plan.TriagePriority}");
            stringBuilder.AppendLine(plan.MedicationSummary);
            stringBuilder.AppendLine(plan.ReprocessingSummary);
            if (actionKey == "see_doctor")
            {
                stringBuilder.AppendLine(healthcareGameplaySystem.BuildProviderCoverageSummary(false));
                for (int i = 0; i < Mathf.Min(3, plan.Directives.Count); i++)
                {
                    TreatmentDirective directive = plan.Directives[i];
                    stringBuilder.AppendLine($"• {directive.Title} [{directive.InteractiveMinigame}]");
                }
            }
        }

        private static string PracticeSkill(CharacterCore active, string skillName, float xp)
        {
            if (active == null)
            {
                return "No active character for skill practice.";
            }

            SkillSystem skill = active.GetComponent<SkillSystem>();
            if (skill == null)
            {
                return "No skill system found on active character.";
            }

            skill.AddExperience(skillName, xp);
            return $"{skillName} improved by practice.";
        }

        private void PublishActionEvent(string reason, float magnitude)
        {
            (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
            {
                Type = SimulationEventType.ActivityCompleted,
                Severity = magnitude < 0f ? SimulationEventSeverity.Warning : SimulationEventSeverity.Info,
                SystemName = nameof(ActionPopupController),
                SourceCharacterId = householdManager != null && householdManager.ActiveCharacter != null
                    ? householdManager.ActiveCharacter.CharacterId
                    : null,
                ChangeKey = currentActionKey,
                Reason = reason,
                Magnitude = magnitude
            });
        }

        private void SetPopupVisible(bool visible)
        {
            if (popupRoot != null)
            {
                popupRoot.SetActive(visible);
            }
        }
    }
}

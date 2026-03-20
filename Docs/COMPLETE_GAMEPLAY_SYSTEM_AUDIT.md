# Complete Gameplay System Audit

This document is a repository-wide gameplay feature index assembled from the Unity source tree, existing architecture docs, and EditMode test coverage. It is meant to be the most exhaustive single-list reference currently derivable from the codebase.

## Scope and reading rules

- Coverage includes runtime systems, UI-facing gameplay routing, content catalogs, support simulation, and the tests that imply intended behavior.
- For each script, the list focuses on player-impacting behavior, simulation responsibilities, public API surfaces, and any implied sub-features visible from names and summaries.
- When a file exposes a broad manager but not every private rule inline, this audit lists the externally visible contract rather than inventing hidden implementation details.

## Core Simulation

### `Assets/Scripts/Core/AIDirectorDramaManager.cs`
- Public classes/structs: AIDirectorDramaManager.
- Public methods / feature hooks:
  - `RegisterMeaningfulBeat`: supports register meaningful beat.
  - `RegisterCalmBeat`: supports register calm beat.
  - `EvaluateAndInject`: supports evaluate and inject.

### `Assets/Scripts/Core/AchievementSystem.cs`
- Public classes/structs: AchievementDefinition, AchievementSystem.
- Public enums: AchievementTriggerType.
- Public methods / feature hooks: none exposed beyond data/state holders.

### `Assets/Scripts/Core/AdaptiveLifeEventsDirector.cs`
- Public classes/structs: DirectedLifeBeat, AdaptiveLifeEventsDirector.
- Public enums: DirectedLifeBeatType.
- Declared purpose: Adaptive director that injects context-aware beats so long-running play stays human and reactive..
- Public methods / feature hooks:
  - `DirectBeatForActiveCharacter`: supports direct beat for active character.

### `Assets/Scripts/Core/BodyCompositionSystem.cs`
- Public classes/structs: BodyGenetics, BodyCompositionSystem.
- Public methods / feature hooks:
  - `ApplyGenetics`: supports apply genetics.
  - `RecalculateForLifeStage`: supports recalculate for life stage.
  - `ModifyBodyComposition`: supports modify body composition.

### `Assets/Scripts/Core/CharacterCore.cs`
- Public classes/structs: CharacterCore.
- Public enums: LifeStage, CharacterSpecies, CharacterTalent, FaceShapeType, EyeShapeType, BodyType, JawShapeType, NoseShapeType, LipShapeType, ClothingStyleType.
- Public methods / feature hooks:
  - `Initialize`: supports initialize.
  - `SetBirthDate`: supports set birth date.
  - `IsBirthday`: supports is birthday.
  - `SetDisplayName`: supports set display name.
  - `SetLifeStage`: supports set life stage.
  - `SetSpecies`: supports set species.
  - `GetSpeciesKey`: supports get species key.
  - `GetSpeciesAgingRateMultiplier`: supports get species aging rate multiplier.
  - `CanFeedOnBlood`: supports can feed on blood.
  - `CanCompelTargets`: supports can compel targets.
  - `HasNightAdvantage`: supports has night advantage.
  - `GetSpeciesTraitSummary`: supports get species trait summary.
  - `SetTalents`: supports set talents.
  - `SetPortraitData`: supports set portrait data.
  - `SetFacialFeatureData`: supports set facial feature data.
  - `RandomizePortraitData`: supports randomize portrait data.
  - `SyncPortraitDataFromAppearance`: supports sync portrait data from appearance.
  - `GetSkillMultiplier`: supports get skill multiplier.
  - `SetFeatureExpression`: supports set feature expression.
  - `SetPlayerControlled`: supports set player controlled.
  - `Die`: supports die.

### `Assets/Scripts/Core/DaySliceManager.cs`
- Public classes/structs: DaySliceManager.
- Public enums: DaySliceStage.
- Public methods / feature hooks:
  - `AdvanceStage`: supports advance stage.
  - `JumpToStage`: supports jump to stage.

### `Assets/Scripts/Core/ExperiencePacingOrchestrator.cs`
- Public classes/structs: ExperiencePacingOrchestrator.
- Public enums: ExperiencePillar.
- Public methods / feature hooks:
  - `RegisterPillarBeat`: supports register pillar beat.

### `Assets/Scripts/Core/FamilyDynamicsSystem.cs`
- Public classes/structs: FamilyNode, FamilyBondProfile, HouseholdClimate, FamilyMemoryEntry, FamilyDynamicsSystem.
- Public enums: ParentingStyle, FamilyRole, HouseholdMoodState.
- Public methods / feature hooks:
  - `RegisterSpouse`: supports register spouse.
  - `RegisterGuardian`: supports register guardian.
  - `GetBond`: supports get bond.
  - `GetHouseholdClimate`: supports get household climate.
  - `ApplyFinancialStressToHousehold`: supports apply financial stress to household.
  - `ApplyFamilySupport`: supports apply family support.
  - `ApplyFamilyConflict`: supports apply family conflict.

### `Assets/Scripts/Core/FamilyManager.cs`
- Public classes/structs: FamilyManager.
- Public methods / feature hooks:
  - `CreateRoommate`: supports create roommate.
  - `HaveBaby`: supports have baby.
  - `BuildOffspringPreview`: supports build offspring preview.
  - `BuildOffspringAIGuidance`: supports build offspring aiguidance.

### `Assets/Scripts/Core/GameBalanceManager.cs`
- Public classes/structs: BalanceTelemetrySnapshot, BalanceTargetBand, BalanceEvaluationReport, GameBalanceManager.
- Public enums: BalanceExperienceMode.
- Public methods / feature hooks:
  - `Evaluate`: supports evaluate.
  - `IsOutOfBand`: supports is out of band.
  - `ScaleNeedDecay`: supports scale need decay.
  - `ScaleSocialChange`: supports scale social change.
  - `ScaleEmotionalDelta`: supports scale emotional delta.
  - `ScalePrice`: supports scale price.
  - `ScaleWage`: supports scale wage.
  - `ScaleQuestReward`: supports scale quest reward.
  - `ScaleFineAmount`: supports scale fine amount.
  - `ScaleJailHours`: supports scale jail hours.
  - `ScaleWeatherPenalty`: supports scale weather penalty.
  - `ScaleSkillXp`: supports scale skill xp.
  - `ScaleCrimeRisk`: supports scale crime risk.
  - `ApplyExperienceMode`: supports apply experience mode.
  - `CaptureTelemetry`: supports capture telemetry.
  - `EvaluateLatestTelemetry`: supports evaluate latest telemetry.
  - `EvaluateTelemetry`: supports evaluate telemetry.

### `Assets/Scripts/Core/GameplayLifeLoopOrchestrator.cs`
- Public classes/structs: LifeLoopStepRecord, LifeLoopExperienceSnapshot, GameplayLifeLoopOrchestrator.
- Declared purpose: Final bridge that keeps the core loop always moving: needs check -> decision -> world simulation -> interaction feedback -> reflection..
- Public methods / feature hooks:
  - `ExecuteManualLifeLoopTick`: supports execute manual life loop tick.

### `Assets/Scripts/Core/HouseholdManager.cs`
- Public classes/structs: HouseholdAutonomyNote, HouseholdPetProfile, HouseholdManager.
- Public enums: HouseholdControlMode.
- Public methods / feature hooks:
  - `AddMember`: supports add member.
  - `RemoveMember`: supports remove member.
  - `SetControlMode`: supports set control mode.
  - `SetActiveCharacter`: supports set active character.
  - `CycleToNextCharacter`: supports cycle to next character.
  - `RegisterAutonomyIntent`: supports register autonomy intent.
  - `GetLatestIntentForCharacter`: supports get latest intent for character.
  - `RegisterPet`: supports register pet.
  - `InteractWithPet`: supports interact with pet.

### `Assets/Scripts/Core/HumanLifeExperienceLayerSystem.cs`
- Public classes/structs: DailyRoutineAction, ThoughtMessage, PlaceAttachmentState, ProceduralLifeMoment, LifeTimelineEntry, HumanLifeExperienceLayerSystem.
- Public enums: LifeReflectionType.
- Declared purpose: Lightweight orchestration layer for dashboard/portrait-first life simulation. Tracks routine identity signals, embodiment prompts, thought messages, place attachment, and procedural day-to-day life moments to keep everyday simulation rich and continuous..
- Public methods / feature hooks:
  - `LogRoutineCompletion`: supports log routine completion.
  - `RecordEmbodimentSignal`: supports record embodiment signal.
  - `RegisterPlaceVisit`: supports register place visit.
  - `LogReflection`: supports log reflection.
  - `SimulateHourPulse`: supports simulate hour pulse.
  - `GenerateProceduralLifeMoments`: supports generate procedural life moments.
  - `RecordLifeTimelineEvent`: supports record life timeline event.
  - `GetTimelineForCharacter`: supports get timeline for character.
  - `SimulateDailyLifeLoop`: supports simulate daily life loop.
  - `GetRoutineForHour`: supports get routine for hour.
  - `GetRecentThoughts`: supports get recent thoughts.
  - `GetStrongestAttachment`: supports get strongest attachment.
  - `BuildLifePulseSummary`: supports build life pulse summary.

### `Assets/Scripts/Core/LifeActivityCatalog.cs`
- Public methods / feature hooks:
  - `PickTvGenre`: supports pick tv genre.
  - `PickMovieGenre`: supports pick movie genre.
  - `PickBookGenre`: supports pick book genre.
  - `PickSingingStyle`: supports pick singing style.
  - `PickRandomOutfitStyle`: supports pick random outfit style.
  - `PickHobbyActivity`: supports pick hobby activity.
  - `PickFamilyMoment`: supports pick family moment.
  - `PickDatingActivity`: supports pick dating activity.
  - `PickNightlifeActivity`: supports pick nightlife activity.
  - `PickCreatorEconomyActivity`: supports pick creator economy activity.
  - `PickSelfCareActivity`: supports pick self care activity.
  - `PickAdultErrand`: supports pick adult errand.
  - `PickGigWorkActivity`: supports pick gig work activity.
  - `PickSocialFeedActivity`: supports pick social feed activity.
  - `PickHomeUpgradeProject`: supports pick home upgrade project.
  - `PickAmbitionFocus`: supports pick ambition focus.

### `Assets/Scripts/Core/LifeMilestonesEngine.cs`
- Public classes/structs: LifeMilestone, LifeMilestonesEngine.
- Public enums: LifeMilestoneType.
- Public methods / feature hooks:
  - `TriggerMilestone`: supports trigger milestone.
  - `EvaluateAnnualMilestones`: supports evaluate annual milestones.

### `Assets/Scripts/Core/LifestyleBehaviorSystem.cs`
- Public classes/structs: HabitEntry, PreferenceEntry, PersonalGoal, IdentityTrack, LifestyleBehaviorSystem.
- Public enums: FinanceBehaviorType, PersonalValueType, PersonalGoalArchetype.
- Public methods / feature hooks:
  - `GetPreference`: supports get preference.
  - `ShouldAllowDiscretionarySpend`: supports should allow discretionary spend.
  - `EnsureModernAdultGoals`: supports ensure modern adult goals.
  - `BuildLifestyleHooks`: supports build lifestyle hooks.
  - `BuildLifestyleDashboard`: supports build lifestyle dashboard.
  - `PinGoal`: supports pin goal.
  - `RefreshDynamicGoals`: supports refresh dynamic goals.

### `Assets/Scripts/Core/LongTermProgressionSystem.cs`
- Public classes/structs: AspirationGoal, ProgressionMilestone, LegacyProfile, LongTermProgressionSystem.
- Public enums: SocialClassTier.
- Public methods / feature hooks:
  - `ProgressGoal`: supports progress goal.
  - `AddFame`: supports add fame.
  - `AddInfamy`: supports add infamy.
  - `AddHousePrestige`: supports add house prestige.
  - `AddHobbyMastery`: supports add hobby mastery.
  - `HasPerk`: supports has perk.

### `Assets/Scripts/Core/MoralValueSystem.cs`
- Public classes/structs: MoralValueProfile, MoralValueSystem.
- Public methods / feature hooks:
  - `GetOrCreateProfile`: supports get or create profile.
  - `EvaluateCrimeResistance`: supports evaluate crime resistance.

### `Assets/Scripts/Core/PersonalityArchetypeSystem.cs`
- Public classes/structs: PersonalityArchetypeProfile, PersonalityArchetypeSystem.
- Public enums: PersonalityArchetype, EmotionalProcessingStyle.
- Public methods / feature hooks:
  - `GetOrCreateProfile`: supports get or create profile.
  - `GetActionBias`: supports get action bias.
  - `ApplyLifeEventShift`: supports apply life event shift.

### `Assets/Scripts/Core/PersonalityDecisionSystem.cs`
- Public classes/structs: PersonalityProfile, SocialCompatibility, JobFitScore, ProceduralDecisionOption, PersonalityDecisionSystem.
- Public enums: PersonalityTrait, AutonomousActionType.
- Public methods / feature hooks:
  - `GetOrCreateProfile`: supports get or create profile.
  - `GetFightEscalationChance`: supports get fight escalation chance.
  - `DecideNextAction`: supports decide next action.
  - `GenerateDecisionSpace`: supports generate decision space.
  - `GenerateProceduralDecisionLoop`: supports generate procedural decision loop.
  - `ComputeSocialCompatibility`: supports compute social compatibility.
  - `ComputeJobFit`: supports compute job fit.

### `Assets/Scripts/Core/PersonalityMatrixSystem.cs`
- Public classes/structs: PersonalityTraitValue, PersonalityDomainCollection, PersonalityMatrixProfile, PersonalityMatrixSystem.
- Public enums: PersonalityDomain.
- Public methods / feature hooks:
  - `CountTotalTraits`: supports count total traits.
  - `GetOrCreateProfile`: supports get or create profile.
  - `GetActionAdjustment`: supports get action adjustment.
  - `GetFightEscalationBias`: supports get fight escalation bias.
  - `ComputeCompatibility`: supports compute compatibility.
  - `ApplyLifeEventShift`: supports apply life event shift.
  - `GetTrait`: supports get trait.
  - `BuildCompactSummary`: supports build compact summary.

### `Assets/Scripts/Core/PreferenceSystem.cs`
- Public classes/structs: PreferenceProfile, PreferenceSystem.
- Public methods / feature hooks:
  - `GetOrCreateProfile`: supports get or create profile.
  - `GetMoodModifierForWeather`: supports get mood modifier for weather.
  - `PublishPreferenceHit`: supports publish preference hit.

### `Assets/Scripts/Core/Procedural/Generators/ScenarioGenerators.cs`
- Public classes/structs: WeatherGenerationResult, HouseholdGenerationResult, TownGenerationResult, NpcGenerationResult, IncidentGenerationResult, WeatherGenerator, HouseholdGenerator, TownGenerator, NpcGenerator, IncidentGenerator.
- Public methods / feature hooks:
  - `Generate`: supports generate.

### `Assets/Scripts/Core/Procedural/Harness/ScenarioHarness.cs`
- Public classes/structs: ScenarioDefinition, ScenarioOutcomeReport, ScenarioHarness.
- Public methods / feature hooks:
  - `Run`: supports run.

### `Assets/Scripts/Core/Procedural/IRandomService.cs`
- Public methods / feature hooks: none exposed beyond data/state holders.

### `Assets/Scripts/Core/Procedural/ProceduralRunContext.cs`
- Public classes/structs: ProceduralRunContext.
- Public methods / feature hooks:
  - `SetMasterSeed`: supports set master seed.
  - `GetRandom`: supports get random.
  - `BuildInitialContext`: supports build initial context.

### `Assets/Scripts/Core/Procedural/RunSeed.cs`
- Public classes/structs: RunSeed.
- Public methods / feature hooks:
  - `Derive`: supports derive.

### `Assets/Scripts/Core/Procedural/ScenarioContext.cs`
- Public classes/structs: ScenarioContext.
- Public methods / feature hooks: none exposed beyond data/state holders.

### `Assets/Scripts/Core/Procedural/SeededRandomService.cs`
- Public classes/structs: SeededRandomService.
- Public methods / feature hooks:
  - `NextInt`: supports next int.
  - `NextFloat`: supports next float.
  - `Roll`: supports roll.

### `Assets/Scripts/Core/Procedural/SimulationProfile.cs`
- Public classes/structs: SimulationProfile.
- Public enums: SimulationProfileType.
- Public methods / feature hooks:
  - `CreatePreset`: supports create preset.

### `Assets/Scripts/Core/Procedural/WeightedTable.cs`
- Public classes/structs: WeightedOption, WeightedTable.
- Public methods / feature hooks:
  - `AddOption`: supports add option.
  - `Pick`: supports pick.

### `Assets/Scripts/Core/PsychologicalGrowthMentalHealthEngine.cs`
- Public classes/structs: MentalHealthProfile, PsychologicalGrowthMentalHealthEngine.
- Public enums: MentalHealthEventType.
- Public methods / feature hooks:
  - `GetOrCreateProfile`: supports get or create profile.
  - `RecordLifeEvent`: supports record life event.
  - `AttendTherapySession`: supports attend therapy session.
  - `GetDecisionMakingModifier`: supports get decision making modifier.
  - `GetRelationshipStabilityModifier`: supports get relationship stability modifier.
  - `GetAddictionVulnerabilityModifier`: supports get addiction vulnerability modifier.
  - `GetWorkPerformanceModifier`: supports get work performance modifier.
  - `GetConflictBehaviorModifier`: supports get conflict behavior modifier.
  - `GetLifeSatisfactionIndex`: supports get life satisfaction index.
  - `GetMentalHealthRiskFlags`: supports get mental health risk flags.
  - `GetPersonalityEvolutionMomentum`: supports get personality evolution momentum.

### `Assets/Scripts/Core/SaveGameManager.cs`
- Public classes/structs: SaveSnapshot, WorldSnapshot, CharacterSnapshot, SaveSlotPayload, SaveGameManager.
- Public methods / feature hooks:
  - `SaveToSlot`: supports save to slot.
  - `LoadFromSlot`: supports load from slot.
  - `DeleteSlot`: supports delete slot.

### `Assets/Scripts/Core/SkillSystem.cs`
- Public classes/structs: SkillEntry, SkillSaveData, SkillSystem.
- Public methods / feature hooks:
  - `CaptureSnapshot`: supports capture snapshot.
  - `ApplySnapshot`: supports apply snapshot.
  - `AddExperience`: supports add experience.
  - `GetSkillLevel`: supports get skill level.
  - `SaveToJson`: supports save to json.
  - `LoadFromJson`: supports load from json.

### `Assets/Scripts/Core/SkillTreeSystem.cs`
- Public classes/structs: SkillTreeNode, SkillTreeSystem.
- Public methods / feature hooks:
  - `TryUnlockNode`: supports try unlock node.

### `Assets/Scripts/Core/WorldCultureSocietyEngine.cs`
- Public classes/structs: CulturalValueWeight, SocialNorm, CulturalTradition, CareerPrestigeEntry, EducationCultureProfile, CultureProfile, CulturalIdentityState, WorldCultureSocietyEngine.
- Public enums: EconomicClassTier.
- Public methods / feature hooks:
  - `GetOrCreateCulture`: supports get or create culture.
  - `GetOrCreateIdentity`: supports get or create identity.
  - `EvaluateNormReaction`: supports evaluate norm reaction.
  - `RegisterTraditionParticipation`: supports register tradition participation.
  - `RegisterMigration`: supports register migration.
  - `ComputeCareerPrestige`: supports compute career prestige.
  - `ComputeClassOpportunityModifier`: supports compute class opportunity modifier.
  - `TickSocietalEvolution`: supports tick societal evolution.

## World & Presentation

### `Assets/Scripts/World/AvatarPresentationStateResolver.cs`
- Public classes/structs: AvatarPresentationInput, AvatarPresentationState.
- Public methods / feature hooks:
  - `ApplyDynamicState`: supports apply dynamic state.
  - `ResolveDynamicState`: supports resolve dynamic state.
  - `ResolveNpcState`: supports resolve npc state.
  - `ApplyResolvedState`: supports apply resolved state.

### `Assets/Scripts/World/BirthdayManager.cs`
- Public classes/structs: BirthdayManager.
- Public methods / feature hooks: none exposed beyond data/state holders.

### `Assets/Scripts/World/BloodlineInheritanceResolver.cs`
- Public classes/structs: TraitAnchorDescriptor, OffspringPreviewEntry, OffspringPreviewCollection.
- Public enums: FamilyResemblanceMode.
- Public methods / feature hooks:
  - `BuildPreviewSet`: supports build preview set.
  - `BuildChildPreview`: supports build child preview.

### `Assets/Scripts/World/GeneticProfile.cs`
- Public classes/structs: AlleleDefinition, MutationFlag, GeneExpressionRule, Gene, ChromosomePair, GenomeRegionProfile, GeneticLineageRecord, PopulationGenePoolReference, EpigeneticMarkerProfile, MutationProfile, PsychologicalGeneticsProfile, TalentGeneticsProfile, IdentityGeneticsProfile, HormoneRegulationProfile, MicroDetailGenomeProfile, ReproductiveGenomeProfile, FaceStructureGenomeProfile, EyeGenomeProfile, NoseGenomeProfile, MouthGenomeProfile, SkinGenomeProfile, HairGenomeProfile, BodyGenomeProfile, BiologyGenomeProfile, TemperamentGenomeProfile, BloodGeneticsProfile, GeneticProfile.
- Public enums: BodySchema, TraitExpressionMode, GeneCategory, MutationOrigin, AboAllele, RhAllele, BloodType, CreatorGeneticsMode.
- Public methods / feature hooks:
  - `ResolveBloodType`: supports resolve blood type.
  - `ToDisplayString`: supports to display string.
  - `ClampToNormalizedRange`: supports clamp to normalized range.
  - `SynchronizeDetailedGenomeFromScalarCache`: supports synchronize detailed genome from scalar cache.
  - `RebuildDerivedTraitsFromGenome`: supports rebuild derived traits from genome.
  - `EvaluateGene`: supports evaluate gene.
  - `FindGene`: supports find gene.

### `Assets/Scripts/World/GeneticTraitCatalog.cs`
- Public classes/structs: GeneticTraitRule, GeneticTraitSnapshot.
- Public enums: GeneticTraitBucket, GeneticInheritanceRule, GeneticTraitCluster.
- Public methods / feature hooks:
  - `GetCoreRules`: supports get core rules.
  - `GetRulesForCluster`: supports get rules for cluster.
  - `BuildPreviewSummary`: supports build preview summary.
  - `CaptureTraitSnapshots`: supports capture trait snapshots.

### `Assets/Scripts/World/GeneticsGuideAISystem.cs`
- Public classes/structs: GeneticsGuideAISystem.
- Public methods / feature hooks:
  - `BuildProfileGuidance`: supports build profile guidance.
  - `BuildOffspringGuidance`: supports build offspring guidance.

### `Assets/Scripts/World/GeneticsSystem.cs`
- Public classes/structs: GeneticsSystem.
- Public methods / feature hooks:
  - `GenerateFounderGenes`: supports generate founder genes.
  - `InheritFromParents`: supports inherit from parents.
  - `SetParentReferences`: supports set parent references.
  - `ValidateGeneticsConsistency`: supports validate genetics consistency.
  - `ApplyGeneticsToSystems`: supports apply genetics to systems.
  - `SetEnvironmentPressure`: supports set environment pressure.
  - `OverrideGenetics`: supports override genetics.
  - `ApplyEpigeneticPressure`: supports apply epigenetic pressure.
  - `ApplyTargetedGeneEdit`: supports apply targeted gene edit.
  - `RollSpontaneousMutation`: supports roll spontaneous mutation.
  - `BuildFamilyResemblanceReport`: supports build family resemblance report.
  - `AdvanceDevelopmentalYear`: supports advance developmental year.
  - `BuildReproductionForecast`: supports build reproduction forecast.
  - `SetCreatorMode`: supports set creator mode.
  - `ApplyVisualSculptToGenome`: supports apply visual sculpt to genome.
  - `ApplyPopulationTemplate`: supports apply population template.
  - `ApplyDynamicPresentationState`: supports apply dynamic presentation state.
  - `BuildAvatarLayerContract`: supports build avatar layer contract.

### `Assets/Scripts/World/InheritanceResolver.cs`
- Public methods / feature hooks:
  - `BuildFounder`: supports build founder.
  - `Inherit`: supports inherit.

### `Assets/Scripts/World/LifeStageMorphResolver.cs`
- Public methods / feature hooks:
  - `ApplyLifeStageMorph`: supports apply life stage morph.

### `Assets/Scripts/World/MasterAssetMatrix.cs`
- Public classes/structs: AssetMatrixEntry, MasterAssetMatrix.
- Public enums: AssetMatrixSystem.
- Public methods / feature hooks:
  - `Matches`: supports matches.
  - `FindBestEntry`: supports find best entry.
  - `GetEntriesForSystem`: supports get entries for system.

### `Assets/Scripts/World/PhenotypeProfiles.cs`
- Public classes/structs: ConditionOverlayProfile, FaceMorphProfile, BodyMorphProfile, SkinProfile, HairProfile, HealthPredispositionProfile, AvatarLayerProfile, PortraitBehaviorProfile, BackgroundPresentationProfile, FamilyResemblanceProfile, PhenotypeProfile.
- Public enums: LayerPieceFamily, EyeExpressionSet, MouthExpressionSet, LifeStageArtMode, BodySilhouetteArchetype.
- Public methods / feature hooks: none exposed beyond data/state holders.

### `Assets/Scripts/World/PhenotypeResolver.cs`
- Public methods / feature hooks:
  - `Resolve`: supports resolve.

### `Assets/Scripts/World/UnityPresentationStateHookup.cs`
- Public classes/structs: UnityPresentationStateHookup.
- Public methods / feature hooks:
  - `ResolveNow`: supports resolve now.
  - `ResolveAssetEntry`: supports resolve asset entry.

### `Assets/Scripts/World/VisualGenome.cs`
- Public classes/structs: VisualGenome.
- Public methods / feature hooks:
  - `ApplyPhysicalTraits`: supports apply physical traits.
  - `GenerateRandomDNA`: supports generate random dna.
  - `InheritTraits`: supports inherit traits.

### `Assets/Scripts/World/WeatherEffectSystem.cs`
- Public classes/structs: WeatherEffectSystem.
- Public methods / feature hooks: none exposed beyond data/state holders.

### `Assets/Scripts/World/WeatherManager.cs`
- Public classes/structs: WeatherManager.
- Public enums: WeatherState.
- Public methods / feature hooks: none exposed beyond data/state holders.

### `Assets/Scripts/World/WorldClock.cs`
- Public classes/structs: HolidayDefinition, SeasonalHolidayDefinition, WorldClock.
- Public enums: Season.
- Public methods / feature hooks:
  - `SetDateTime`: supports set date time.
  - `IsDate`: supports is date.

### `Assets/Scripts/World/WorldCreatorManager.cs`
- Public classes/structs: WorldAreaTemplate, WorldGenerationSummary, WorldCreatorManager.
- Public methods / feature hooks:
  - `SetWorldMetadata`: supports set world metadata.
  - `GenerateWorldWithDefaults`: supports generate world with defaults.
  - `BuildWorldFromTemplates`: supports build world from templates.
  - `VoteLaw`: supports vote law.

### `Assets/Scripts/World/WorldEventDirector.cs`
- Public classes/structs: WorldAmbientEventDefinition, WorldEventDirector.
- Public enums: WorldAmbientEventType.
- Public methods / feature hooks: none exposed beyond data/state holders.

### `Assets/Scripts/World/WorldGuideAISystem.cs`
- Public classes/structs: WorldGuideAISystem.
- Public methods / feature hooks:
  - `BuildGuidanceForRoom`: supports build guidance for room.

### `Assets/Scripts/World/WorldPersistenceCullingSystem.cs`
- Public classes/structs: LotSimulationState, RemoteNpcSnapshot, WorldPersistenceCullingSystem.
- Public methods / feature hooks:
  - `SetLotActive`: supports set lot active.
  - `RegisterRemoteNpc`: supports register remote npc.
  - `SimulateOffscreenHours`: supports simulate offscreen hours.
  - `ApplyCatchUpForNpc`: supports apply catch up for npc.

## Needs Health Emotion

### `Assets/Scripts/Emotion/ConflictSystem.cs`
- Public classes/structs: CombatRoundResult, ConflictSystem.
- Public enums: ViolenceType, CombatOption, CombatRange, ConflictEscalationStage.
- Public methods / feature hooks:
  - `TryStartFight`: supports try start fight.
  - `TryStartViolence`: supports try start violence.
  - `BuildCombatActionCatalog`: supports build combat action catalog.
  - `ResolveCombatRound`: supports resolve combat round.
  - `TryDeescalate`: supports try deescalate.
  - `GetEscalationStage`: supports get escalation stage.
  - `RaiseConflictStage`: supports raise conflict stage.

### `Assets/Scripts/Emotion/EmotionSystem.cs`
- Public classes/structs: EmotionSystem.
- Public methods / feature hooks:
  - `ModifyAnger`: supports modify anger.
  - `ModifyAffection`: supports modify affection.
  - `ModifyStress`: supports modify stress.
  - `IsReadyToFight`: supports is ready to fight.
  - `IsInLoveState`: supports is in love state.
  - `ApplySocialInteraction`: supports apply social interaction.
  - `RecoverSocialEnergy`: supports recover social energy.

### `Assets/Scripts/Health/AdvancedHealthRecoverySystem.cs`
- Public classes/structs: RegionInjury, MedicationPlan, AdvancedHealthRecoverySystem.
- Public enums: BodyRegion.
- Public methods / feature hooks:
  - `AddInjury`: supports add injury.
  - `ApplyTreatment`: supports apply treatment.
  - `ComputeRecoveryQuality`: supports compute recovery quality.

### `Assets/Scripts/Health/HealthSystem.cs`
- Public classes/structs: HealthSystem.
- Public methods / feature hooks:
  - `CaptureVitality`: supports capture vitality.
  - `ApplyVitality`: supports apply vitality.
  - `Damage`: supports damage.
  - `Heal`: supports heal.
  - `ApplySunlightExposure`: supports apply sunlight exposure.
  - `ApplyRestorativeDarkness`: supports apply restorative darkness.

### `Assets/Scripts/Health/HealthcareGameplaySystem.cs`
- Public classes/structs: TreatmentDirective, HealthcareEncounterPlan, HealthcareGameplaySystem.
- Public enums: CareProviderRole, CareFacilityType, TriagePriority.
- Public methods / feature hooks:
  - `BuildPlansForCharacter`: supports build plans for character.
  - `BuildPlan`: supports build plan.
  - `BuildProviderCoverageSummary`: supports build provider coverage summary.
  - `BuildImmersiveProcedureBoard`: supports build immersive procedure board.

### `Assets/Scripts/Health/InjuryRecoverySystem.cs`
- Public classes/structs: InjuryRecord, InjuryRecoverySystem.
- Public enums: InjurySeverity.
- Public methods / feature hooks:
  - `AddInjury`: supports add injury.
  - `TreatInjury`: supports treat injury.

### `Assets/Scripts/Health/MedicalConditionSystem.cs`
- Public classes/structs: MedicalCondition, MedicalConditionSystem.
- Public enums: ConditionSeverity, IllnessType, InjuryType, BodyLocation, WoundType, FractureType, TissueLayerDepth, InternalComplicationType, SkinConditionType, MedicationType.
- Public methods / feature hooks:
  - `AddIllness`: supports add illness.
  - `AddInjury`: supports add injury.
  - `AddDetailedInjury`: supports add detailed injury.
  - `AdministerMedication`: supports administer medication.
  - `HealCondition`: supports heal condition.
  - `RollRandomCondition`: supports roll random condition.
  - `ApplyRadiationExposure`: supports apply radiation exposure.

### `Assets/Scripts/Health/SeasonalAllergySystem.cs`
- Public classes/structs: SeasonalAllergySystem.
- Public methods / feature hooks: none exposed beyond data/state holders.

### `Assets/Scripts/Needs/NeedsSystem.cs`
- Public classes/structs: NeedsSnapshot, NeedsSystem.
- Public enums: BurnoutStage, CravingType.
- Public methods / feature hooks:
  - `CaptureSnapshot`: supports capture snapshot.
  - `ApplySnapshot`: supports apply snapshot.
  - `RestoreHunger`: supports restore hunger.
  - `RestoreHydration`: supports restore hydration.
  - `ResetBladder`: supports reset bladder.
  - `ModifyEnergy`: supports modify energy.
  - `ModifyHygiene`: supports modify hygiene.
  - `ModifyMood`: supports modify mood.
  - `ModifyGrooming`: supports modify grooming.
  - `ModifyAppearance`: supports modify appearance.
  - `ModifyBoredom`: supports modify boredom.
  - `ModifyMentalFatigue`: supports modify mental fatigue.
  - `ModifyMotivation`: supports modify motivation.
  - `ApplyActivityStimulation`: supports apply activity stimulation.
  - `SetActiveCraving`: supports set active craving.
  - `ResolveCraving`: supports resolve craving.
  - `ApplyFoodEffects`: supports apply food effects.
  - `ApplyDrinkEffects`: supports apply drink effects.
  - `GetSocialFailureModifier`: supports get social failure modifier.
  - `IncreaseBladder`: supports increase bladder.

## Social Dialogue Story

### `Assets/Scripts/Dialogue/DialogueSystem.cs`
- Public classes/structs: DialogueReplyOption, DialogueLine, DialogueGeneratedLine, DialoguePresentationPayload, DialogueContext, DialogueSystem.
- Public enums: DialogueReplyTone, DialogueIntent.
- Public methods / feature hooks:
  - `PerformDialogue`: supports perform dialogue.
  - `BuildReplyOptions`: supports build reply options.
  - `PerformDialogueReply`: supports perform dialogue reply.
  - `PerformServiceInteractionDialogue`: supports perform service interaction dialogue.
  - `PerformPetInteractionDialogue`: supports perform pet interaction dialogue.
  - `GetOptionsForMood`: supports get options for mood.
  - `GetOptionsForSituation`: supports get options for situation.
  - `GetOptionsForMemory`: supports get options for memory.
  - `GetPetInteractionLines`: supports get pet interaction lines.

### `Assets/Scripts/Dialogue/InteractionDialogueBridge.cs`
- Public classes/structs: InteractionDialogueBridge.
- Declared purpose: Bridges click interactions into dialogue intents so character clicking immediately produces conversational feedback suitable for overlay presentation..
- Public methods / feature hooks:
  - `ResolveIntentForActor`: supports resolve intent for actor.
  - `ResolveSituationTag`: supports resolve situation tag.

### `Assets/Scripts/Dialogue/NarrativePromptSystem.cs`
- Public classes/structs: NarrativePromptSystem.
- Public methods / feature hooks: none exposed beyond data/state holders.

### `Assets/Scripts/Social/LoveLanguageSystem.cs`
- Public classes/structs: LoveLanguageProfile, LoveLanguageSystem.
- Public enums: LoveLanguageType.
- Public methods / feature hooks:
  - `GetOrCreateProfile`: supports get or create profile.
  - `ApplyAffectionAction`: supports apply affection action.

### `Assets/Scripts/Social/RelationshipCompatibilityEngine.cs`
- Public classes/structs: RelationshipCompatibilityProfile, RelationshipCompatibilityEngine.
- Public enums: FriendshipStage, RomanticPhase, RivalryState.
- Public methods / feature hooks:
  - `Matches`: supports matches.
  - `GetOrCreateProfile`: supports get or create profile.
  - `EvaluateInitialCompatibility`: supports evaluate initial compatibility.
  - `ApplyInteraction`: supports apply interaction.
  - `ApplyAppearanceChangeReaction`: supports apply appearance change reaction.
  - `BuildCompatibilityDashboard`: supports build compatibility dashboard.

### `Assets/Scripts/Social/RelationshipMemorySystem.cs`
- Public classes/structs: RelationshipMemory, RelationshipProfile, ReputationEntry, RelationshipMemorySystem.
- Public enums: ReputationScope, PersonalMemoryKind.
- Public methods / feature hooks:
  - `RecordEvent`: supports record event.
  - `RecordPersonalMemory`: supports record personal memory.
  - `GetOrCreateProfile`: supports get or create profile.
  - `ApplyFamilyReputationConsequences`: supports apply family reputation consequences.
  - `GetReputation`: supports get reputation.
  - `AdjustReputation`: supports adjust reputation.
  - `SpreadNeighborhoodGossip`: supports spread neighborhood gossip.

### `Assets/Scripts/Social/SocialDramaEngine.cs`
- Public classes/structs: SocialEventSignal, SecretEntry, ScandalEvent, ReputationLayerProfile, RumorPacket, SocialDramaEngine.
- Public enums: SocialEventType, TownAwarenessLevel.
- Public methods / feature hooks:
  - `RegisterSocialEvent`: supports register social event.
  - `RegisterSecret`: supports register secret.
  - `TryExposeSecret`: supports try expose secret.
  - `SpreadRumor`: supports spread rumor.
  - `RetellRumor`: supports retell rumor.
  - `GetOrCreateReputation`: supports get or create reputation.
  - `TriggerScandal`: supports trigger scandal.

### `Assets/Scripts/Social/SocialSystem.cs`
- Public classes/structs: Relationship, SocialSystem.
- Public enums: RelationshipType.
- Public methods / feature hooks:
  - `GetRelationshipValue`: supports get relationship value.
  - `GetRelationshipType`: supports get relationship type.
  - `ApplyDailyRelationshipDrift`: supports apply daily relationship drift.
  - `AddHouseholdMember`: supports add household member.
  - `UpdateRelationship`: supports update relationship.

### `Assets/Scripts/Story/AutonomousStoryGenerator.cs`
- Public classes/structs: StoryIncidentDefinition, StoryIncidentRecord, LocalNewsEntry, AutonomousStoryGenerator.
- Public enums: StoryVibePreset, StoryIncidentType.
- Public methods / feature hooks:
  - `ForceGenerateIncident`: supports force generate incident.
  - `TryGenerateIncident`: supports try generate incident.
  - `GetVibeMultiplier`: supports get vibe multiplier.

## Activities Tasks Minigames

### `Assets/Scripts/Activity/ActivitySystem.cs`
- Public classes/structs: ActivitySystem.
- Public enums: ActivityType.
- Public methods / feature hooks:
  - `PerformActivity`: supports perform activity.
  - `IsActivityAllowedForLifeStage`: supports is activity allowed for life stage.

### `Assets/Scripts/Activity/DailyRoutineSystem.cs`
- Public classes/structs: DailyRoutineSystem.
- Public methods / feature hooks: none exposed beyond data/state holders.

### `Assets/Scripts/Minigames/MinigameManager.cs`
- Public classes/structs: MinigameSceneProfile, MinigameManager.
- Public enums: MinigameType.
- Public methods / feature hooks:
  - `StartMinigame`: supports start minigame.
  - `StartCareerMinigame`: supports start career minigame.
  - `ResolveProfessionMinigame`: supports resolve profession minigame.
  - `GetAvailableMinigameTypes`: supports get available minigame types.
  - `GetSceneProfile`: supports get scene profile.

### `Assets/Scripts/Tasking/AutoTaskRunner.cs`
- Public classes/structs: AutoTaskRunner.
- Public methods / feature hooks:
  - `RunAuto`: supports run auto.

### `Assets/Scripts/Tasking/InteractiveTaskRunner.cs`
- Public classes/structs: InteractiveTaskRunner.
- Public methods / feature hooks:
  - `RunInteractive`: supports run interactive.

### `Assets/Scripts/Tasking/TaskDatabase.cs`
- Public classes/structs: TaskDatabase.
- Public methods / feature hooks:
  - `FindById`: supports find by id.
  - `FindByCategory`: supports find by category.
  - `FindByStation`: supports find by station.

### `Assets/Scripts/Tasking/TaskInteractionManager.cs`
- Public classes/structs: TaskInteractionManager.
- Public methods / feature hooks:
  - `StartTask`: supports start task.
  - `StartTaskAuto`: supports start task auto.
  - `StartTaskInteractive`: supports start task interactive.

### `Assets/Scripts/Tasking/TaskModels.cs`
- Public classes/structs: TaskStepDefinition, TaskResultDefinition, TaskDefinition, ActiveTaskSession.
- Public enums: TaskMode, TaskCategory, TaskResultType.
- Public methods / feature hooks: none exposed beyond data/state holders.

### `Assets/Scripts/Tasking/TaskResultSpawner.cs`
- Public classes/structs: TaskResultSpawner.
- Public methods / feature hooks:
  - `SpawnInventoryResult`: supports spawn inventory result.
  - `SpawnHeldPropResult`: supports spawn held prop result.

### `Assets/Scripts/Tasking/TaskStateUpdater.cs`
- Public classes/structs: TaskStateUpdater.
- Public methods / feature hooks:
  - `ApplyAppearanceResult`: supports apply appearance result.
  - `ApplyWorldStateResult`: supports apply world state result.
  - `ApplyRelationshipServiceResult`: supports apply relationship service result.

## Commerce Economy Crafting

### `Assets/Scripts/Catalog/IngredientCatalog.cs`
- Public classes/structs: IngredientItem, IngredientCatalog.
- Public enums: IngredientGroup, IngredientCategory, IngredientLifecycleState, IngredientPurpose.
- Public methods / feature hooks:
  - `GetByGroup`: supports get by group.
  - `GetIngredient`: supports get ingredient.
  - `HasTag`: supports has tag.
  - `GetRandomByCategory`: supports get random by category.

### `Assets/Scripts/Catalog/SupplyCatalog.cs`
- Public classes/structs: SupplyItem, SupplyCatalog.
- Public enums: SupplyGroup.
- Public methods / feature hooks:
  - `HasSupply`: supports has supply.
  - `GetByGroup`: supports get by group.
  - `GetRandomByGroup`: supports get random by group.

### `Assets/Scripts/Commerce/CraftingProfessionSystem.cs`
- Public classes/structs: CraftingStation, CraftingBlueprint, CraftingProfessionSystem.
- Public enums: ProfessionCategory.
- Public methods / feature hooks:
  - `UnlockBlueprint`: supports unlock blueprint.
  - `Craft`: supports craft.
  - `UnlockStation`: supports unlock station.

### `Assets/Scripts/Commerce/GrocerySystem.cs`
- Public classes/structs: InventoryEntry, GrocerySystem.
- Public methods / feature hooks:
  - `BuyIngredient`: supports buy ingredient.
  - `BulkRestockForWeek`: supports bulk restock for week.
  - `ConsumeIngredient`: supports consume ingredient.
  - `HasIngredient`: supports has ingredient.
  - `GetIngredientQuantity`: supports get ingredient quantity.

### `Assets/Scripts/Commerce/OrderingSystem.cs`
- Public classes/structs: MenuItem, FastFoodLocation, PendingOrder, OrderingSystem.
- Public methods / feature hooks:
  - `BuildProceduralDailyMenu`: supports build procedural daily menu.
  - `GetRecommendations`: supports get recommendations.
  - `AddFunds`: supports add funds.
  - `SpendFunds`: supports spend funds.
  - `OrderOut`: supports order out.
  - `OrderFastFood`: supports order fast food.

### `Assets/Scripts/Commerce/RecipeSystem.cs`
- Public classes/structs: RecipeIngredient, Recipe, RecipeSystem.
- Public methods / feature hooks:
  - `CookRecipe`: supports cook recipe.
  - `GenerateDailySpecialMenu`: supports generate daily special menu.
  - `GetSuggestedRecipesByPantry`: supports get suggested recipes by pantry.
  - `DiscoverRecipeFromIngredients`: supports discover recipe from ingredients.
  - `RefreshRecipeUnlocks`: supports refresh recipe unlocks.

### `Assets/Scripts/Economy/EconomyInventorySystem.cs`
- Public classes/structs: SharedInventoryEntry, EconomyItemDefinition, EconomyItemInstance, EconomySnapshot, EconomyInventorySystem.
- Public enums: InventoryScope.
- Public methods / feature hooks:
  - `AddFunds`: supports add funds.
  - `TrySpend`: supports try spend.
  - `GetQuantity`: supports get quantity.
  - `HasItem`: supports has item.
  - `CaptureSnapshot`: supports capture snapshot.
  - `ApplySnapshot`: supports apply snapshot.
  - `AddItem`: supports add item.
  - `RemoveItem`: supports remove item.
  - `AddItemInstance`: supports add item instance.
  - `ReserveForRecipe`: supports reserve for recipe.
  - `ReleaseReservation`: supports release reservation.
  - `EquipItem`: supports equip item.
  - `UnequipItem`: supports unequip item.
  - `GetOwnedItems`: supports get owned items.
  - `EvaluateSellValue`: supports evaluate sell value.
  - `RegisterDefinition`: supports register definition.
  - `SimulateHourTick`: supports simulate hour tick.
  - `SetRefrigeration`: supports set refrigeration.

### `Assets/Scripts/Economy/EconomyManager.cs`
- Public classes/structs: EconomyAccount, EconomyTransactionRecord, PricingModifier, EconomyManager.
- Public enums: EconomyTransactionType.
- Public methods / feature hooks:
  - `EnsureAccount`: supports ensure account.
  - `GetBalance`: supports get balance.
  - `Deposit`: supports deposit.
  - `TryCharge`: supports try charge.
  - `Transfer`: supports transfer.
  - `RecordPurchase`: supports record purchase.
  - `ApplyFine`: supports apply fine.
  - `IssuePaycheck`: supports issue paycheck.
  - `ProcessUtilityBill`: supports process utility bill.
  - `SetPricingModifier`: supports set pricing modifier.
  - `GetModifiedPrice`: supports get modified price.

### `Assets/Scripts/Economy/InventoryManager.cs`
- Public classes/structs: InventoryContainer, InventoryTransferRecord, ReservedIngredientEntry, FoodStackState, InventoryManager.
- Public enums: FoodFreshnessState.
- Public methods / feature hooks:
  - `EnsureContainer`: supports ensure container.
  - `GetStackQuantity`: supports get stack quantity.
  - `AddStack`: supports add stack.
  - `RemoveStack`: supports remove stack.
  - `TransferStack`: supports transfer stack.
  - `AddItemInstance`: supports add item instance.
  - `ReserveIngredient`: supports reserve ingredient.
  - `SetRefrigerated`: supports set refrigerated.
  - `GetFreshnessState`: supports get freshness state.
  - `ReleaseReservation`: supports release reservation.
  - `GetReservedQuantity`: supports get reserved quantity.
  - `EquipItem`: supports equip item.
  - `UnequipItem`: supports unequip item.
  - `EvaluateSellValue`: supports evaluate sell value.

### `Assets/Scripts/Food/DrinkDatabase.cs`
- Public classes/structs: DrinkItem, DrinkDatabase.
- Public enums: DrinkCategory.
- Public methods / feature hooks:
  - `GetRandomDrink`: supports get random drink.
  - `GetRandomByCategory`: supports get random by category.

### `Assets/Scripts/Food/FoodDatabase.cs`
- Public classes/structs: FoodNutrition, FoodRecipeDefinition, FoodItem, FoodDatabase.
- Public enums: FoodCategory, CookingMethod, KitchenEquipment, CuisineType, FoodQuality, ServingTemperature, MealPurpose.
- Public methods / feature hooks:
  - `GetRandomFood`: supports get random food.
  - `GetRecipe`: supports get recipe.
  - `GetFood`: supports get food.
  - `GetRandomByCategory`: supports get random by category.

## NPC World Simulation

### `Assets/Scripts/Location/HouseholdChoreSystem.cs`
- Public classes/structs: HouseholdChore, HouseholdChoreSystem.
- Public enums: HouseholdChoreType.
- Public methods / feature hooks:
  - `GenerateDailyChores`: supports generate daily chores.
  - `CompleteChore`: supports complete chore.
  - `TryCompleteHighestPriorityChore`: supports try complete highest priority chore.

### `Assets/Scripts/Location/HousingPropertySystem.cs`
- Public classes/structs: PropertyRecord, RepairRequest, HousingPropertySystem.
- Public enums: WasteItemState, LaundryState.
- Public methods / feature hooks:
  - `GetProperty`: supports get property.
  - `TransferOwnership`: supports transfer ownership.
  - `ApplyRoomMaintenance`: supports apply room maintenance.
  - `RegisterWaste`: supports register waste.
  - `AddDishStack`: supports add dish stack.
  - `AddLaundry`: supports add laundry.
  - `ProcessBinDisposal`: supports process bin disposal.
  - `ProcessLaundry`: supports process laundry.
  - `ProcessDishes`: supports process dishes.
  - `RegisterUtilityUsage`: supports register utility usage.
  - `TryAddStorageUsage`: supports try add storage usage.
  - `SubmitRepairRequest`: supports submit repair request.
  - `ResolveRepairRequest`: supports resolve repair request.

### `Assets/Scripts/Location/LivingWorldInfrastructureEngine.cs`
- Public classes/structs: DistrictInfrastructureProfile, BusinessInfrastructureProfile, HousingInfrastructureProfile, PublicServiceInfrastructureState, TransportInfrastructureState, SupplyItemState, EncounterRecord, LivingWorldInfrastructureEngine.
- Public enums: DistrictType, TransportMode, BusinessIndustry, SupplyItemType.
- Public methods / feature hooks:
  - `EnsureSeededDefaults`: supports ensure seeded defaults.
  - `SimulateInfrastructureHour`: supports simulate infrastructure hour.
  - `SimulateInfrastructureDay`: supports simulate infrastructure day.
  - `GetDistrictActivityScore`: supports get district activity score.
  - `GetItemAvailability`: supports get item availability.

### `Assets/Scripts/Location/LocationManager.cs`
- Public classes/structs: Room, LocationManager.
- Public enums: LocationTheme.
- Public methods / feature hooks:
  - `SetRooms`: supports set rooms.
  - `FindRoom`: supports find room.
  - `NavigateToRoom`: supports navigate to room.

### `Assets/Scripts/Location/TownSimulationManager.cs`
- Public classes/structs: LotPopulationSnapshot, DistrictActivitySnapshot, TownOffscreenState, CommunityEventRecord, TownSimulationManager.
- Public methods / feature hooks:
  - `GetTownPressureScore`: supports get town pressure score.
  - `RecomputeTownState`: supports recompute town state.

### `Assets/Scripts/Location/TownSimulationSystem.cs`
- Public classes/structs: LotDefinition, DistrictDefinition, RouteEdge, TownSimulationSystem.
- Public enums: ZoneType.
- Public methods / feature hooks:
  - `GetLot`: supports get lot.
  - `GetDistrict`: supports get district.
  - `IsLotOpen`: supports is lot open.
  - `GetRouteCost`: supports get route cost.
  - `GetLocalDanger`: supports get local danger.
  - `GetLocalWealth`: supports get local wealth.
  - `GetOpenLotsByZone`: supports get open lots by zone.
  - `SetTownLayout`: supports set town layout.

### `Assets/Scripts/NPC/NPCAutonomyController.cs`
- Public classes/structs: NPCAutonomyController.
- Public methods / feature hooks:
  - `EvaluateAutonomy`: supports evaluate autonomy.

### `Assets/Scripts/NPC/NpcCareerSystem.cs`
- Public classes/structs: CareerRoleDefinition, NpcCareerRecord, NpcCareerSystem.
- Public enums: ProfessionType.
- Public methods / feature hooks:
  - `AssignCareer`: supports assign career.
  - `IsServiceAvailable`: supports is service available.
  - `CountOnDuty`: supports count on duty.
  - `EvaluatePromotion`: supports evaluate promotion.

### `Assets/Scripts/NPC/NpcLifeAIGuideSystem.cs`
- Public classes/structs: NpcChatSuggestion, NpcLifeAIGuideSystem.
- Public methods / feature hooks:
  - `BuildGuidance`: supports build guidance.
  - `BuildChatSuggestions`: supports build chat suggestions.
  - `BuildEndlessChatSuggestions`: supports build endless chat suggestions.
  - `BuildReplayabilitySummary`: supports build replayability summary.
  - `BuildInteractionSummary`: supports build interaction summary.

### `Assets/Scripts/NPC/NpcScheduleSystem.cs`
- Public classes/structs: NpcScheduleBlock, NpcMemoryEntry, NpcProfile, NpcScheduleSystem.
- Public enums: NpcJobType, NpcActivityState, NpcMemoryKind, NpcKnowledgeSensitivity.
- Public methods / feature hooks:
  - `RegisterNpc`: supports register npc.
  - `RememberInteraction`: supports remember interaction.
  - `RememberFirstImpression`: supports remember first impression.
  - `RememberGrudge`: supports remember grudge.
  - `RememberRumor`: supports remember rumor.
  - `RememberSecret`: supports remember secret.
  - `RememberLegacy`: supports remember legacy.
  - `GetNpcProfile`: supports get npc profile.
  - `GetStrongestMemory`: supports get strongest memory.
  - `ForceNpcState`: supports force npc state.
  - `ForceNpcLot`: supports force npc lot.
  - `SetNpcHealth`: supports set npc health.

### `Assets/Scripts/NPC/ScheduleSystem.cs`
- Public classes/structs: ScheduleBlock, NpcSchedulePlan, ScheduleSystem.
- Public enums: ScheduleBlockType.
- Public methods / feature hooks:
  - `GetOrCreatePlan`: supports get or create plan.
  - `ResolveCurrentBlock`: supports resolve current block.
  - `ResolveBestLotForBlock`: supports resolve best lot for block.
  - `ToActivityState`: supports to activity state.
  - `MarkHoliday`: supports mark holiday.

## Crime Law Society

### `Assets/Scripts/Crime/AddictionLifecycleSystem.cs`
- Public classes/structs: AddictionLifecycleSystem.
- Public enums: AddictionStage.
- Public methods / feature hooks:
  - `ApplyRehabilitationProgress`: supports apply rehabilitation progress.

### `Assets/Scripts/Crime/ContrabandSystem.cs`
- Public classes/structs: ContrabandItem, InmateContrabandInventory, ContrabandSystem.
- Public enums: ContrabandCategory.
- Public methods / feature hooks:
  - `AddContraband`: supports add contraband.
  - `TryConfiscateRandom`: supports try confiscate random.
  - `CountItems`: supports count items.

### `Assets/Scripts/Crime/CravingSystem.cs`
- Public classes/structs: CravingSystem.
- Public methods / feature hooks:
  - `RelieveCraving`: supports relieve craving.

### `Assets/Scripts/Crime/CrimeSystem.cs`
- Public classes/structs: CrimeRecord, PendingInvestigation, CrimeSystem.
- Public enums: CrimeType, CrimeCategory.
- Public methods / feature hooks:
  - `CommitCrime`: supports commit crime.

### `Assets/Scripts/Crime/CriminalReputationSystem.cs`
- Public classes/structs: CriminalReputationState, CriminalReputationSystem.
- Public enums: CriminalReputationTier.
- Public methods / feature hooks:
  - `GetState`: supports get state.

### `Assets/Scripts/Crime/DisciplineSystem.cs`
- Public classes/structs: DisciplineRecord, DisciplineSystem.
- Public enums: DisciplineOffenseType, DisciplinePunishmentType.
- Public methods / feature hooks:
  - `ApplyOffense`: supports apply offense.

### `Assets/Scripts/Crime/GuardAlertSystem.cs`
- Public classes/structs: GuardAlertSystem.
- Public enums: GuardAlertLevel.
- Public methods / feature hooks:
  - `RaiseAlert`: supports raise alert.
  - `ReduceAlert`: supports reduce alert.

### `Assets/Scripts/Crime/JusticeSystem.cs`
- Public classes/structs: JusticeOutcome, ActiveSentence, JusticeSystem.
- Public enums: JusticeOutcomeType, LegalProcessStage.
- Public methods / feature hooks:
  - `IsIncarcerated`: supports is incarcerated.
  - `CanVote`: supports can vote.
  - `ProcessCrime`: supports process crime.

### `Assets/Scripts/Crime/ParoleEvaluationSystem.cs`
- Public classes/structs: ParoleEvaluationSystem.
- Public methods / feature hooks: none exposed beyond data/state holders.

### `Assets/Scripts/Crime/PrisonEconomySystem.cs`
- Public classes/structs: CommissaryItem, InmateWallet, PrisonEconomySystem.
- Public methods / feature hooks:
  - `GetBalance`: supports get balance.
  - `Deposit`: supports deposit.
  - `BuyCommissaryItem`: supports buy commissary item.

### `Assets/Scripts/Crime/PrisonInteractionSystem.cs`
- Public classes/structs: PrisonInteractionDefinition, PrisonInteractionSystem.
- Public enums: PrisonInteractionType.
- Public methods / feature hooks:
  - `PerformInteraction`: supports perform interaction.

### `Assets/Scripts/Crime/PrisonRoutineSystem.cs`
- Public classes/structs: PrisonScheduleSlot, InmateRoutineState, PrisonRoutineSystem.
- Public enums: PrisonRoutineActivity.
- Public methods / feature hooks:
  - `GetState`: supports get state.

### `Assets/Scripts/Crime/RehabilitationSystem.cs`
- Public classes/structs: RehabilitationCenterProfile, RehabilitationSystem.
- Public enums: RehabilitationProgramType.
- Public methods / feature hooks:
  - `StartProgram`: supports start program.
  - `AdmitToBestCenter`: supports admit to best center.
  - `GetBestCenter`: supports get best center.

### `Assets/Scripts/Crime/SubstanceSystem.cs`
- Public classes/structs: SubstanceProfile, ActiveSubstanceEffect, SubstanceSystem.
- Public enums: SubstanceRiskTier.
- Public methods / feature hooks:
  - `ModifyDependency`: supports modify dependency.
  - `AdjustRiskPressure`: supports adjust risk pressure.
  - `GetSubstanceProfile`: supports get substance profile.
  - `GetProfilesForCategory`: supports get profiles for category.
  - `UseSubstance`: supports use substance.

### `Assets/Scripts/Society/ElectionCycleSystem.cs`
- Public classes/structs: ElectionPolicyOutcome, ElectionCycleSystem.
- Public enums: ElectionPhase.
- Public methods / feature hooks:
  - `CanCharacterVote`: supports can character vote.
  - `RegisterPlayerVote`: supports register player vote.

### `Assets/Scripts/Society/LawSystem.cs`
- Public classes/structs: SubstanceLaw, AreaLawProfile, LawSystem.
- Public enums: LawSeverity, SubstanceType.
- Public methods / feature hooks:
  - `SetAreaProfiles`: supports set area profiles.
  - `SetCurrentArea`: supports set current area.
  - `GetSubstanceSeverity`: supports get substance severity.
  - `GetEnforcementForCrime`: supports get enforcement for crime.
  - `VoteOnSubstanceLaw`: supports vote on substance law.
  - `ApplyPolicyShift`: supports apply policy shift.

## Appearance Identity View

### `Assets/Scripts/Appearance/AppearanceManager.cs`
- Public classes/structs: AppearanceProfile, HairStyleVariant, FaceFeatureSet, AppearanceManager.
- Public enums: HairStyleType, EyeColorType, SkinToneType, SkinIssueType.
- Public methods / feature hooks:
  - `AutoBindRenderersByName`: supports auto bind renderers by name.
  - `ValidatePortraitLayerSetup`: supports validate portrait layer setup.
  - `ConfigureDefaultLayerOrder`: supports configure default layer order.
  - `ApplyFaceFeatureSet`: supports apply face feature set.
  - `RandomizeAppearance`: supports randomize appearance.
  - `ApplyAppearance`: supports apply appearance.
  - `SetHairStyle`: supports set hair style.
  - `SetHairColor`: supports set hair color.
  - `SetNaturalHairColor`: supports set natural hair color.
  - `SetDyedHairColor`: supports set dyed hair color.
  - `SetUseDyedHairColor`: supports set use dyed hair color.
  - `SetHairColorChannels`: supports set hair color channels.
  - `SetEyeColor`: supports set eye color.
  - `SetSkinTone`: supports set skin tone.
  - `SetSkinIssue`: supports set skin issue.
  - `SetBeautyMark`: supports set beauty mark.
  - `SetMakeupColor`: supports set makeup color.
  - `TryApplyHairstyleById`: supports try apply hairstyle by id.
  - `GetHairstylesByFilter`: supports get hairstyles by filter.
  - `SetHairProfile`: supports set hair profile.
  - `SetFacialHairProfile`: supports set facial hair profile.
  - `SetBodyHairProfile`: supports set body hair profile.
  - `BuildHairRenderContract`: supports build hair render contract.
  - `ApplyLayeredHairContract`: supports apply layered hair contract.

### `Assets/Scripts/Appearance/CharacterAppearanceEditor.cs`
- Public classes/structs: CharacterAppearanceEditor.
- Public methods / feature hooks:
  - `TrySetHairDyeColor`: supports try set hair dye color.
  - `TrySetNaturalHairColor`: supports try set natural hair color.
  - `AddTattoo`: supports add tattoo.
  - `SetStyleIdentity`: supports set style identity.
  - `EquipFashionItem`: supports equip fashion item.

### `Assets/Scripts/Appearance/FashionSystem.cs`
- Public classes/structs: ClothingItem, FashionSystem.
- Public methods / feature hooks:
  - `Equip`: supports equip.
  - `EvaluateStyleFit`: supports evaluate style fit.

### `Assets/Scripts/Appearance/HairGroomingSystem.cs`
- Public classes/structs: HairGroomingSystem.
- Public methods / feature hooks:
  - `TrimHairToStage`: supports trim hair to stage.
  - `ShaveFacialHair`: supports shave facial hair.
  - `ShaveBodyHair`: supports shave body hair.

### `Assets/Scripts/Appearance/HairLayerSystem.cs`
- Public classes/structs: HairProfile, FacialHairProfile, BodyHairProfile, HairGrowthRules, HairPieceDefinition, HairstyleDefinition, HairRenderPiece, HairRenderContract.
- Public enums: HairSlotType, HairTextureFamily, HairStyleFamily, HairGrowthStage, BeardGrowthStage, HairLengthStage, HairVolumeClass.
- Public methods / feature hooks:
  - `GetEffectiveBaseColor`: supports get effective base color.
  - `BuildContract`: supports build contract.
  - `ResolveHairGrowthStage`: supports resolve hair growth stage.
  - `ResolveBeardGrowthStage`: supports resolve beard growth stage.

### `Assets/Scripts/Appearance/StyleIdentitySystem.cs`
- Public classes/structs: StyleIdentityProfile, StyleIdentitySystem.
- Public enums: StyleIdentityType.
- Public methods / feature hooks:
  - `GetOrCreateProfile`: supports get or create profile.
  - `GetSocialModifier`: supports get social modifier.

### `Assets/Scripts/Appearance/TattooSystem.cs`
- Public classes/structs: TattooEntry, CharacterTattooProfile, TattooSystem.
- Public methods / feature hooks:
  - `AddTattoo`: supports add tattoo.
  - `RecordTattooNotice`: supports record tattoo notice.

### `Assets/Scripts/View/ViewManager.cs`
- Public classes/structs: ViewManager.
- Public methods / feature hooks:
  - `ToggleViewMode`: supports toggle view mode.
  - `SetPortraitMode`: supports set portrait mode.
  - `SetFullBodyMode`: supports set full body mode.

## Interaction Transport UI

### `Assets/Scripts/Interaction/HomeInteractionHotspot.cs`
- Public classes/structs: CharacterClosetProfile, HomeInteractionHotspot.
- Public enums: HomeHotspotType.
- Public methods / feature hooks:
  - `Execute`: supports execute.

### `Assets/Scripts/Interaction/Interactable.cs`
- Public classes/structs: Interactable.
- Public enums: InteractableType.
- Public methods / feature hooks: none exposed beyond data/state holders.

### `Assets/Scripts/Interaction/InteractionController.cs`
- Public classes/structs: InteractionController.
- Public methods / feature hooks: none exposed beyond data/state holders.

### `Assets/Scripts/Transport/CarSystem.cs`
- Public classes/structs: Car, CarSystem.
- Public enums: CarType.
- Public methods / feature hooks:
  - `DriveToRoom`: supports drive to room.
  - `RefuelCar`: supports refuel car.
  - `RepairCar`: supports repair car.
  - `CleanCar`: supports clean car.

### `Assets/Scripts/UI/ActionPopupController.cs`
- Public classes/structs: AnimalSightingEncounter, ActionPopupController.
- Public methods / feature hooks:
  - `ConfirmAction`: supports confirm action.
  - `CancelAction`: supports cancel action.

### `Assets/Scripts/UI/AnimationFeedbackJuiceSystem.cs`
- Public classes/structs: FeedbackCue, AnimationFeedbackJuiceSystem.
- Public methods / feature hooks:
  - `BuildCue`: supports build cue.

### `Assets/Scripts/UI/BuildModeManager.cs`
- Public classes/structs: BuildModeManager.
- Public methods / feature hooks:
  - `ToggleBuildMode`: supports toggle build mode.
  - `SetBuildMode`: supports set build mode.

### `Assets/Scripts/UI/CharacterCreatorDashboardController.cs`
- Public classes/structs: CharacterCreatorBackgroundView, CharacterCreatorDraftSnapshot, CreatorTabPanel, StyleVariantCardView, CharacterCreatorDashboardController.
- Public enums: CharacterCreatorDashboardTab, FacialHairFilterMode, CharacterCreatorPreviewFocus, CharacterCreatorBackgroundOption.
- Public methods / feature hooks:
  - `SetTab`: supports set tab.
  - `SetPreviewBackground`: supports set preview background.
  - `SetPreviewFocus`: supports set preview focus.
  - `FocusFullBody`: supports focus full body.
  - `FocusFaceClose`: supports focus face close.
  - `FocusBodyClose`: supports focus body close.
  - `FocusGenetics`: supports focus genetics.
  - `FocusAreaView`: supports focus area view.
  - `SetHairTextureFilter`: supports set hair texture filter.
  - `SetFaceShape`: supports set face shape.
  - `SetBodyType`: supports set body type.
  - `SetJawShape`: supports set jaw shape.
  - `SetNoseShape`: supports set nose shape.
  - `SetLipShape`: supports set lip shape.
  - `SetEyeColor`: supports set eye color.
  - `SetSkinTone`: supports set skin tone.
  - `UseRandomPopulationMode`: supports use random population mode.
  - `UseDnaEditMode`: supports use dna edit mode.
  - `UseVisualSculptMode`: supports use visual sculpt mode.
  - `SetPopulationRegionTemplate`: supports set population region template.
  - `SetGenomeMelanin`: supports set genome melanin.
  - `SetGenomeHeight`: supports set genome height.
  - `SetGenomeBodyFat`: supports set genome body fat.
  - `SetGenomeMuscle`: supports set genome muscle.
  - `SetGenomeCognition`: supports set genome cognition.
  - `SetGenomeHormoneBalance`: supports set genome hormone balance.
  - `SetGenomeHairThickness`: supports set genome hair thickness.
  - `SimulateDevelopmentalYear`: supports simulate developmental year.
  - `SetGenomeStressEpigenetics`: supports set genome stress epigenetics.
  - `SetGenomeDietEpigenetics`: supports set genome diet epigenetics.
  - `RollGenomeMutation`: supports roll genome mutation.
  - `NextSection`: supports next section.
  - `PreviousSection`: supports previous section.
  - `SetHairLengthFilter`: supports set hair length filter.
  - `SetFacialHairFilter`: supports set facial hair filter.
  - `SetHairColorSwatch`: supports set hair color swatch.
  - `SetUseDyedHair`: supports set use dyed hair.
  - `SetHairBaseR`: supports set hair base r.
  - `SetHairBaseG`: supports set hair base g.
  - `SetHairBaseB`: supports set hair base b.
  - `SetHairHighlightIntensity`: supports set hair highlight intensity.
  - `SetHairRootDepth`: supports set hair root depth.
  - `SetHairOmbreAmount`: supports set hair ombre amount.
  - `SetFrontHairSlider`: supports set front hair slider.
  - `SetSidesHairSlider`: supports set sides hair slider.
  - `SetBackHairSlider`: supports set back hair slider.
  - `SetFacialHairSlider`: supports set facial hair slider.
  - `SelectStyleByCardIndex`: supports select style by card index.
  - `RandomizeCharacterDashboard`: supports randomize character dashboard.
  - `SaveAppearancePreset`: supports save appearance preset.
  - `LoadAppearancePreset`: supports load appearance preset.
  - `LockActiveCharacterDesign`: supports lock active character design.
  - `UnlockActiveCharacterDesign`: supports unlock active character design.
  - `IsActiveCharacterLocked`: supports is active character locked.
  - `SaveCharacterDraft`: supports save character draft.
  - `LoadCharacterDraft`: supports load character draft.
  - `BeginPreviewDrag`: supports begin preview drag.
  - `EndPreviewDrag`: supports end preview drag.
  - `DragRotatePreview`: supports drag rotate preview.
  - `RotatePreview`: supports rotate preview.
  - `ZoomPreview`: supports zoom preview.
  - `StartTaskAutoFromDashboard`: supports start task auto from dashboard.
  - `StartTaskInteractiveFromDashboard`: supports start task interactive from dashboard.
  - `Back`: supports back.
  - `Next`: supports next.
  - `CaptureViewModel`: supports capture view model.

### `Assets/Scripts/UI/CharacterPortraitRenderer.cs`
- Public classes/structs: FaceShapeSpriteEntry, EyeShapeSpriteEntry, BodyTypeSpriteEntry, ClothingSpriteEntry, HairStyleSpriteEntry, CharacterPortraitRenderer.
- Public methods / feature hooks:
  - `SetTargetCharacter`: supports set target character.
  - `EstimateUniquePortraitCombinationCount`: supports estimate unique portrait combination count.
  - `MeetsLargeSpriteRosterRequirement`: supports meets large sprite roster requirement.
  - `BuildPortraitVariationSummary`: supports build portrait variation summary.
  - `RefreshPortrait`: supports refresh portrait.

### `Assets/Scripts/UI/CharacterRosterHUD.cs`
- Public classes/structs: CharacterRosterHUD.
- Public methods / feature hooks: none exposed beyond data/state holders.

### `Assets/Scripts/UI/CharacterScreenController.cs`
- Public classes/structs: CharacterScreenController.
- Public methods / feature hooks:
  - `Refresh`: supports refresh.

### `Assets/Scripts/UI/DialogueOverlayController.cs`
- Public classes/structs: DialogueOverlayController.
- Public methods / feature hooks:
  - `CloseOverlay`: supports close overlay.
  - `ShowCharacterOverlay`: supports show character overlay.

### `Assets/Scripts/UI/FurniturePlaceable.cs`
- Public classes/structs: FurniturePlaceable.
- Public methods / feature hooks:
  - `TryMoveTo`: supports try move to.
  - `CanMove`: supports can move.

### `Assets/Scripts/UI/FurnitureStoreController.cs`
- Public classes/structs: FurnitureStoreItem, FurnitureStoreController.
- Public methods / feature hooks:
  - `OpenStore`: supports open store.
  - `CloseStore`: supports close store.
  - `BuyItemByIndex`: supports buy item by index.

### `Assets/Scripts/UI/GameHUD.cs`
- Public classes/structs: GameHUD.
- Public methods / feature hooks:
  - `BuildHudLoopDigest`: supports build hud loop digest.

### `Assets/Scripts/UI/GameplayInteractionPresentationLayer.cs`
- Public classes/structs: WorldPanelSnapshot, CharacterPanelSnapshot, ActionFeedbackPulse, MapTravelOption, LifeTimelinePreview, HotspotActionPack, GameplayInteractionPresentationLayer.
- Public methods / feature hooks:
  - `RefreshSnapshots`: supports refresh snapshots.
  - `BuildMapTravelOptions`: supports build map travel options.
  - `BuildTravelMapPrompt`: supports build travel map prompt.
  - `TryTravelToDistrict`: supports try travel to district.
  - `BuildHotspotsForCurrentLocation`: supports build hotspots for current location.
  - `BuildSectionTabsForCurrentLocation`: supports build section tabs for current location.
  - `BuildScreenMoodSummary`: supports build screen mood summary.
  - `BuildContextActionSuggestions`: supports build context action suggestions.
  - `BuildDailyLifeFlowSuggestions`: supports build daily life flow suggestions.
  - `SyncTimelinePreview`: supports sync timeline preview.
  - `RegisterManualChoiceResult`: supports register manual choice result.

### `Assets/Scripts/UI/GameplayPresentationStateCoordinator.cs`
- Public classes/structs: GameplayPresentationStateCoordinator.
- Declared purpose: Bridges gameplay context into a single presentation-facing state object. It does not own gameplay rules; it only resolves room/action/snapshot context into structured section identity that HUDs, tabs, popups, and portrait panels can bind to..
- Public methods / feature hooks:
  - `SetFocusedAction`: supports set focused action.
  - `RefreshState`: supports refresh state.

### `Assets/Scripts/UI/GameplayScreenController.cs`
- Public classes/structs: GameplayResourceStat, GameplayScreenController.
- Public methods / feature hooks:
  - `NavigateTo`: supports navigate to.
  - `QuickSaveSlot1`: supports quick save slot1.
  - `QuickSaveSlot2`: supports quick save slot2.
  - `QuickSaveSlot3`: supports quick save slot3.

### `Assets/Scripts/UI/GameplayVisionSystem.cs`
- Public classes/structs: GameplaySectionVision, GameplayVisionSystem.
- Public enums: GameplaySectionType.
- Public methods / feature hooks:
  - `ResolveVision`: supports resolve vision.
  - `BuildTabsForContext`: supports build tabs for context.
  - `BuildVisionStatement`: supports build vision statement.

### `Assets/Scripts/UI/HouseholdMakerScreenController.cs`
- Public classes/structs: HouseholdMakerTabPanel, HouseholdDraftMemberSnapshot, HouseholdDraftSnapshot, HouseholdMakerScreenController.
- Public enums: HouseholdMakerTab, HouseholdOriginFocus, FamilyPlanningPriority.
- Public methods / feature hooks:
  - `SetTab`: supports set tab.
  - `NextTab`: supports next tab.
  - `PreviousTab`: supports previous tab.
  - `OpenFamilyGeneticsSection`: supports open family genetics section.
  - `SetFamilySurname`: supports set family surname.
  - `SetHomeDistrict`: supports set home district.
  - `SetHouseholdStoryPrompt`: supports set household story prompt.
  - `SetOriginFocus`: supports set origin focus.
  - `SetPlanningPriority`: supports set planning priority.
  - `NextHouseholdCharacter`: supports next household character.
  - `PreviousHouseholdCharacter`: supports previous household character.
  - `SetActiveArtPivot`: supports set active art pivot.
  - `NextArtPivot`: supports next art pivot.
  - `PreviousArtPivot`: supports previous art pivot.
  - `RotateCharacter`: supports rotate character.
  - `ZoomCamera`: supports zoom camera.
  - `ValidateHouseholdGenetics`: supports validate household genetics.
  - `Back`: supports back.
  - `StartGame`: supports start game.
  - `ToggleLockActiveCharacter`: supports toggle lock active character.
  - `LockFamilyDraft`: supports lock family draft.
  - `UnlockFamilyDraft`: supports unlock family draft.
  - `SaveFamilyDraft`: supports save family draft.
  - `LoadFamilyDraft`: supports load family draft.

### `Assets/Scripts/UI/JournalCardView.cs`
- Public classes/structs: JournalCardView.
- Public methods / feature hooks:
  - `Bind`: supports bind.

### `Assets/Scripts/UI/JournalFeedUI.cs`
- Public classes/structs: JournalFeedUI.
- Public methods / feature hooks:
  - `BuildDailyDigest`: supports build daily digest.

### `Assets/Scripts/UI/LoadGameScreenController.cs`
- Public classes/structs: SaveSlotMeta, LoadGameScreenController.
- Public methods / feature hooks:
  - `SelectSlot`: supports select slot.
  - `Back`: supports back.
  - `RefreshSlots`: supports refresh slots.

### `Assets/Scripts/UI/MainMenuFlowController.cs`
- Public classes/structs: ScreenBinding, MainMenuFlowController.
- Public enums: MainMenuScreen.
- Public methods / feature hooks:
  - `OpenMainMenu`: supports open main menu.
  - `OpenSettings`: supports open settings.
  - `OpenLoadGame`: supports open load game.
  - `OpenCharacterScreen`: supports open character screen.
  - `OpenWorldCreator`: supports open world creator.
  - `OpenCharacterCreator`: supports open character creator.
  - `OpenHouseholdMaker`: supports open household maker.
  - `StartGameplay`: supports start gameplay.
  - `StartNewGame`: supports start new game.
  - `ContinueFromWorldCreator`: supports continue from world creator.
  - `ContinueFromCharacterCreator`: supports continue from character creator.
  - `ContinueFromHousehold`: supports continue from household.
  - `Back`: supports back.

### `Assets/Scripts/UI/RpgCelebrationPopupController.cs`
- Public classes/structs: RpgCelebrationPopupController.
- Public methods / feature hooks: none exposed beyond data/state holders.

### `Assets/Scripts/UI/SettingsPageController.cs`
- Public classes/structs: GameplaySettingsData, SettingsPageController.
- Public methods / feature hooks:
  - `SetMasterVolume`: supports set master volume.
  - `SetMusicVolume`: supports set music volume.
  - `SetSfxVolume`: supports set sfx volume.
  - `SetFullscreen`: supports set fullscreen.
  - `SetShowSubtitles`: supports set show subtitles.
  - `SetPauseOnFocusLoss`: supports set pause on focus loss.
  - `SetUiScale`: supports set ui scale.
  - `SetPrimaryColor`: supports set primary color.
  - `SetSecondaryColor`: supports set secondary color.
  - `SetBackgroundColor`: supports set background color.
  - `SetTraitPillColor`: supports set trait pill color.
  - `ResetDefaults`: supports reset defaults.

### `Assets/Scripts/UI/SettingsTabsController.cs`
- Public classes/structs: SettingsTabPanel, SettingsTabsController.
- Public enums: SettingsTab.
- Public methods / feature hooks:
  - `SetTab`: supports set tab.

### `Assets/Scripts/UI/SidebarContextMenu.cs`
- Public classes/structs: SidebarOption, SidebarContextMenu.
- Public methods / feature hooks:
  - `ExecuteOptionByIndex`: supports execute option by index.

### `Assets/Scripts/UI/SplashScreenController.cs`
- Public classes/structs: SplashScreenController.
- Public methods / feature hooks:
  - `SkipSplash`: supports skip splash.

### `Assets/Scripts/UI/SuccessionUI.cs`
- Public classes/structs: SuccessionUI.
- Public methods / feature hooks: none exposed beyond data/state holders.

### `Assets/Scripts/UI/TraitPillTagView.cs`
- Public classes/structs: TraitPillTagView.
- Public methods / feature hooks:
  - `Bind`: supports bind.

### `Assets/Scripts/UI/UIEventFeedbackRouter.cs`
- Public classes/structs: UIEventFeedbackRouter.
- Public methods / feature hooks: none exposed beyond data/state holders.

### `Assets/Scripts/UI/UIGlassStyleController.cs`
- Public classes/structs: UIGlassStyleController.
- Public methods / feature hooks:
  - `ApplyStyle`: supports apply style.
  - `SetGlassTint`: supports set glass tint.
  - `SetGlowTint`: supports set glow tint.

### `Assets/Scripts/UI/ViewModels/UiViewModels.cs`
- Public classes/structs: SplashScreenViewModel, MainMenuViewModel, LoadSlotViewModel, SettingsViewModel, WorldCreatorViewModel, CharacterCreatorViewModel, CharacterCreatorDashboardViewModel, HouseholdMakerViewModel, GameplayHudViewModel, JournalCardViewModel, ActionPopupViewModel, SidebarActionViewModel, ZoneScenePanelViewModel, CharacterPortraitViewModel, CharacterRosterItemViewModel, PresentationSectionViewModel.
- Public methods / feature hooks: none exposed beyond data/state holders.

### `Assets/Scripts/UI/WorldCreatorScreenController.cs`
- Public classes/structs: WorldCreatorSettings, WorldCreatorScreenController.
- Public enums: WorldCreatorTab, StartingOrigin, SettlementDensity, EconomyFocus, GovernmentStyle.
- Public methods / feature hooks:
  - `SetTab`: supports set tab.
  - `Next`: supports next.
  - `Back`: supports back.
  - `GenerateWorld`: supports generate world.
  - `SetSandboxExperience`: supports set sandbox experience.

### `Assets/Scripts/UI/ZoneScenePanel.cs`
- Public classes/structs: ZoneThemeContent, ZoneScenePanel.
- Public methods / feature hooks: none exposed beyond data/state holders.

## Support Tools

### `Assets/Scripts/Rendering/CharacterSortingGroupBinder.cs`
- Public classes/structs: CharacterSortingGroupBinder.
- Public methods / feature hooks:
  - `BindToSortingGroup`: supports bind to sorting group.

### `Assets/Scripts/Utility/AssetReadinessReporter.cs`
- Public classes/structs: AssetReadinessReporter.
- Public methods / feature hooks:
  - `ReportMissingAssetReferences`: supports report missing asset references.
  - `AutoWireKnownReferences`: supports auto wire known references.
  - `ReportRuntimeVisionCoverage`: supports report runtime vision coverage.
  - `RunIntegrationBalanceDryRun`: supports run integration balance dry run.
  - `ReportOptionalUiCoverage`: supports report optional ui coverage.
  - `RunHeadlessPendingWorkAudit`: supports run headless pending work audit.

### `Assets/Scripts/Utility/BalanceTuningAdvisor.cs`
- Public classes/structs: BalanceAuditSummary.
- Public methods / feature hooks:
  - `BuildSummary`: supports build summary.

### `Assets/Scripts/Utility/IntegrationDryRunService.cs`
- Public classes/structs: IntegrationDryRunResult.
- Public methods / feature hooks:
  - `RunScenarioBalanceDryRun`: supports run scenario balance dry run.

### `Assets/Scripts/Utility/PlaceholderGenerator.cs`
- Public classes/structs: PlaceholderGenerator.
- Public methods / feature hooks:
  - `AssignPlaceholders`: supports assign placeholders.

### `Assets/Scripts/Utility/SimulationSceneBootstrapper.cs`
- Public classes/structs: SimulationSceneBootstrapper.
- Public methods / feature hooks:
  - `BootstrapScene`: supports bootstrap scene.

### `Assets/Scripts/Utility/SimulationStabilityMonitor.cs`
- Public classes/structs: SimulationStabilityMonitor.
- Public methods / feature hooks: none exposed beyond data/state holders.

## Other

### `Assets/Scripts/Events/GameEventHub.cs`
- Public classes/structs: SimulationEvent, GameEventHub.
- Public enums: SimulationEventType, SimulationEventSeverity.
- Public methods / feature hooks:
  - `Publish`: supports publish.
  - `GetRecentEventsByType`: supports get recent events by type.
  - `CountEventsInRange`: supports count events in range.
  - `PublishFromAnywhere`: supports publish from anywhere.

### `Assets/Scripts/Legacy/LegacyManager.cs`
- Public classes/structs: LegacyBeat, SuccessorScoreCard, LegacyManager.
- Public methods / feature hooks:
  - `RecordLegacyBeat`: supports record legacy beat.
  - `BuildSuccessorScoreCards`: supports build successor score cards.
  - `ChooseSuccessor`: supports choose successor.

### `Assets/Scripts/LifeStage/LifeStageManager.cs`
- Public classes/structs: LifeStageManager.
- Public methods / feature hooks:
  - `AgeUp`: supports age up.

### `Assets/Scripts/Quest/ContractBoardSystem.cs`
- Public classes/structs: AnimalSightingContract, ContractBoardSystem.
- Public enums: ContractState.
- Public methods / feature hooks:
  - `EnsureContractPool`: supports ensure contract pool.
  - `AcceptContract`: supports accept contract.
  - `CompleteContract`: supports complete contract.

### `Assets/Scripts/Quest/QuestOpportunitySystem.cs`
- Public classes/structs: OpportunityObjective, OpportunityDefinition, ActiveOpportunity, QuestOpportunitySystem.
- Public enums: OpportunityState, ObjectiveType.
- Public methods / feature hooks:
  - `GetAvailableOpportunitiesForLocation`: supports get available opportunities for location.
  - `GetAcceptedOpportunitiesForLocation`: supports get accepted opportunities for location.
  - `PublishOpportunity`: supports publish opportunity.
  - `AcceptOpportunity`: supports accept opportunity.
  - `ProgressObjective`: supports progress objective.
  - `ResolveOpportunity`: supports resolve opportunity.
  - `GenerateEmergencyOpportunity`: supports generate emergency opportunity.

### `Assets/Scripts/Status/StatusEffectSystem.cs`
- Public classes/structs: StatusEffectDefinition, ActiveStatusEffect, StatusEffectSystem.
- Public methods / feature hooks:
  - `FromDefinition`: supports from definition.
  - `CaptureSnapshot`: supports capture snapshot.
  - `ApplySnapshot`: supports apply snapshot.
  - `ApplyStatusById`: supports apply status by id.
  - `ApplyRandomStatus`: supports apply random status.

## EditMode test coverage index

These tests are an important second source of truth because each file name advertises a validated or intended gameplay slice.

- `Assets/Tests/EditMode/AIDirectorDramaManagerTests.cs` -> validates or documents aidirector drama manager.
- `Assets/Tests/EditMode/AchievementSystemTests.cs` -> validates or documents achievement system.
- `Assets/Tests/EditMode/ActionPopupVisionTests.cs` -> validates or documents action popup vision.
- `Assets/Tests/EditMode/AdaptiveLifeEventsDirectorTests.cs` -> validates or documents adaptive life events director.
- `Assets/Tests/EditMode/AnimationFeedbackJuiceSystemTests.cs` -> validates or documents animation feedback juice system.
- `Assets/Tests/EditMode/AutonomousStoryGeneratorTests.cs` -> validates or documents autonomous story generator.
- `Assets/Tests/EditMode/AvatarPresentationStateResolverTests.cs` -> validates or documents avatar presentation state resolver.
- `Assets/Tests/EditMode/BalanceTuningAdvisorTests.cs` -> validates or documents balance tuning advisor.
- `Assets/Tests/EditMode/BalancingIntegrationTests.cs` -> validates or documents balancing integration.
- `Assets/Tests/EditMode/BehaviorAndSocialExtensionsTests.cs` -> validates or documents behavior and social extensions.
- `Assets/Tests/EditMode/BloodlineInheritanceResolverTests.cs` -> validates or documents bloodline inheritance resolver.
- `Assets/Tests/EditMode/CharacterAppearanceDiversityTests.cs` -> validates or documents character appearance diversity.
- `Assets/Tests/EditMode/ContextualGameplaySystemsTests.cs` -> validates or documents contextual gameplay systems.
- `Assets/Tests/EditMode/DashboardPresentationTests.cs` -> validates or documents dashboard presentation.
- `Assets/Tests/EditMode/DialogueAndLegacyGameplayTests.cs` -> validates or documents dialogue and legacy gameplay.
- `Assets/Tests/EditMode/DialogueContextSelectionTests.cs` -> validates or documents dialogue context selection.
- `Assets/Tests/EditMode/DialogueDepthCoverageTests.cs` -> validates or documents dialogue depth coverage.
- `Assets/Tests/EditMode/DialogueOverlayFlowTests.cs` -> validates or documents dialogue overlay flow.
- `Assets/Tests/EditMode/EconomyInventorySystemTests.cs` -> validates or documents economy inventory system.
- `Assets/Tests/EditMode/EconomyManagerTests.cs` -> validates or documents economy manager.
- `Assets/Tests/EditMode/FamilyDynamicsSystemTests.cs` -> validates or documents family dynamics system.
- `Assets/Tests/EditMode/FoodSystemExpansionTests.cs` -> validates or documents food system expansion.
- `Assets/Tests/EditMode/GameBalanceManagerTests.cs` -> validates or documents game balance manager.
- `Assets/Tests/EditMode/GameEventHubSpamGateTests.cs` -> validates or documents game event hub spam gate.
- `Assets/Tests/EditMode/GameplayInteractionPresentationLayerTests.cs` -> validates or documents gameplay interaction presentation layer.
- `Assets/Tests/EditMode/GameplayLifeLoopOrchestratorTests.cs` -> validates or documents gameplay life loop orchestrator.
- `Assets/Tests/EditMode/GameplayPresentationStateCoordinatorTests.cs` -> validates or documents gameplay presentation state coordinator.
- `Assets/Tests/EditMode/HealthAndCombatRealismTests.cs` -> validates or documents health and combat realism.
- `Assets/Tests/EditMode/HomeLifeExtensionsTests.cs` -> validates or documents home life extensions.
- `Assets/Tests/EditMode/HouseholdChoreSystemTests.cs` -> validates or documents household chore system.
- `Assets/Tests/EditMode/HouseholdMakerScreenControllerTests.cs` -> validates or documents household maker screen controller.
- `Assets/Tests/EditMode/HousingPropertySystemTests.cs` -> validates or documents housing property system.
- `Assets/Tests/EditMode/HumanLifeExperienceLayerSystemTests.cs` -> validates or documents human life experience layer system.
- `Assets/Tests/EditMode/IntegrationDryRunServiceTests.cs` -> validates or documents integration dry run service.
- `Assets/Tests/EditMode/InteractionDialogueBridgeTests.cs` -> validates or documents interaction dialogue bridge.
- `Assets/Tests/EditMode/InventoryManagerTests.cs` -> validates or documents inventory manager.
- `Assets/Tests/EditMode/LifeActivityCatalogTests.cs` -> validates or documents life activity catalog.
- `Assets/Tests/EditMode/LifeMilestonesEngineTests.cs` -> validates or documents life milestones engine.
- `Assets/Tests/EditMode/LivingWorldInfrastructureEngineTests.cs` -> validates or documents living world infrastructure engine.
- `Assets/Tests/EditMode/LongTermProgressionSystemTests.cs` -> validates or documents long term progression system.
- `Assets/Tests/EditMode/MasterAssetMatrixTests.cs` -> validates or documents master asset matrix.
- `Assets/Tests/EditMode/MinigameManagerTests.cs` -> validates or documents minigame manager.
- `Assets/Tests/EditMode/ModernLifeFeatureTests.cs` -> validates or documents modern life feature.
- `Assets/Tests/EditMode/NPCAutonomyControllerTests.cs` -> validates or documents npcautonomy controller.
- `Assets/Tests/EditMode/NpcCareerSystemTests.cs` -> validates or documents npc career system.
- `Assets/Tests/EditMode/PersonalityDecisionSystemTests.cs` -> validates or documents personality decision system.
- `Assets/Tests/EditMode/PersonalityMatrixSystemTests.cs` -> validates or documents personality matrix system.
- `Assets/Tests/EditMode/PhenotypeLayeringPipelineTests.cs` -> validates or documents phenotype layering pipeline.
- `Assets/Tests/EditMode/ProceduralFoundationTests.cs` -> validates or documents procedural foundation.
- `Assets/Tests/EditMode/PsychologicalGrowthMentalHealthEngineTests.cs` -> validates or documents psychological growth mental health engine.
- `Assets/Tests/EditMode/QuestOpportunitySystemTests.cs` -> validates or documents quest opportunity system.
- `Assets/Tests/EditMode/RelationshipCompatibilityEngineTests.cs` -> validates or documents relationship compatibility engine.
- `Assets/Tests/EditMode/RelationshipMemorySystemTests.cs` -> validates or documents relationship memory system.
- `Assets/Tests/EditMode/SaveSchemaMigrationTests.cs` -> validates or documents save schema migration.
- `Assets/Tests/EditMode/ScenarioHarnessTests.cs` -> validates or documents scenario harness.
- `Assets/Tests/EditMode/ScheduleSystemTests.cs` -> validates or documents schedule system.
- `Assets/Tests/EditMode/SimulationStabilityMonitorTests.cs` -> validates or documents simulation stability monitor.
- `Assets/Tests/EditMode/SkillSystemTests.cs` -> validates or documents skill system.
- `Assets/Tests/EditMode/SocialDramaEngineTests.cs` -> validates or documents social drama engine.
- `Assets/Tests/EditMode/SubstanceSystemExpansionTests.cs` -> validates or documents substance system expansion.
- `Assets/Tests/EditMode/TownSimulationManagerTests.cs` -> validates or documents town simulation manager.
- `Assets/Tests/EditMode/TownSimulationSystemTests.cs` -> validates or documents town simulation system.
- `Assets/Tests/EditMode/UnityPresentationStateHookupTests.cs` -> validates or documents unity presentation state hookup.
- `Assets/Tests/EditMode/WorldClockSystemsTests.cs` -> validates or documents world clock systems.
- `Assets/Tests/EditMode/WorldCreatorScreenControllerTests.cs` -> validates or documents world creator screen controller.
- `Assets/Tests/EditMode/WorldCultureSocietyEngineTests.cs` -> validates or documents world culture society engine.
- `Assets/Tests/EditMode/WorldEssentialsExpansionTests.cs` -> validates or documents world essentials expansion.
- `Assets/Tests/EditMode/WorldEventDirectorTests.cs` -> validates or documents world event director.
- `Assets/Tests/EditMode/WorldPersistenceCullingSystemTests.cs` -> validates or documents world persistence culling system.
- `Assets/Tests/EditMode/WorldVibeSystemsTests.cs` -> validates or documents world vibe systems.

## Cross-system full-picture gameplay flow

1. World generation establishes templates, laws, experience mode, and baseline infrastructure.
2. Character creation and household creation define body/portrait/genetics/family setup.
3. The world clock advances time; weather, birthdays, holidays, aging, routines, and offscreen simulation react.
4. Needs, emotions, health, skills, economy, and social memory are continuously modified by activities, dialogue, and environment.
5. Interaction, tasking, minigames, and contextual UI convert clicks or automation into gameplay outcomes.
6. NPC autonomy, careers, schedules, town systems, and story/drama engines keep the world moving around the player.
7. Crime, law, justice, addiction, and rehabilitation layers create consequences and alternative life loops.
8. Progression, achievements, legacy, milestones, and save/load preserve long-term household history.

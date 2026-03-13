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

    [Serializable]
    public class HouseholdMakerTabPanel
    {
        public HouseholdMakerTab Tab;
        public GameObject Root;
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
        [SerializeField] private List<HouseholdMakerTabPanel> tabPanels = new();
        [SerializeField] private bool wrapTabs = true;

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

        private void OnEnable()
        {
            if (householdManager != null)
            {
                householdManager.OnActiveCharacterChanged += HandleActiveCharacterChanged;
                HandleActiveCharacterChanged(householdManager.ActiveCharacter);
            }

            RefreshTabUi();
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
                $"Preview Assets: {characterArtPivots.Count}\n" +
                $"Pivot Mode: {(rotateAllArtPivots ? "Rotate All" : "Rotate Active")}";
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
    }
}

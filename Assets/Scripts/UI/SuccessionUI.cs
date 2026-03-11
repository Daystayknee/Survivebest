using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Survivebest.Core;
using Survivebest.Legacy;

namespace Survivebest.UI
{
    public class SuccessionUI : MonoBehaviour
    {
        [SerializeField] private LegacyManager legacyManager;
        [SerializeField] private GameObject rootPanel;
        [SerializeField] private Transform cardContainer;
        [SerializeField] private Button cardButtonPrefab;

        private readonly List<Button> spawnedButtons = new();

        private void Awake()
        {
            if (rootPanel != null)
            {
                rootPanel.SetActive(false);
            }
        }

        private void OnEnable()
        {
            if (legacyManager == null)
            {
                Debug.LogWarning("SuccessionUI missing LegacyManager reference.");
                return;
            }

            legacyManager.OnPlayerDeath += HandlePlayerDeath;
            legacyManager.OnSuccessorChosen += HandleSuccessorChosen;
            legacyManager.OnGameOver += HandleGameOver;
        }

        private void OnDisable()
        {
            if (legacyManager == null)
            {
                return;
            }

            legacyManager.OnPlayerDeath -= HandlePlayerDeath;
            legacyManager.OnSuccessorChosen -= HandleSuccessorChosen;
            legacyManager.OnGameOver -= HandleGameOver;
        }

        private void HandlePlayerDeath(CharacterCore deceased, IReadOnlyList<CharacterCore> survivors)
        {
            if (rootPanel != null)
            {
                rootPanel.SetActive(true);
            }

            RebuildCards(survivors);
            Time.timeScale = 0f;
        }

        private void HandleSuccessorChosen(CharacterCore successor)
        {
            CloseUI();
        }

        private void HandleGameOver()
        {
            Debug.Log("Game Over: no survivors left.");
            CloseUI();
        }

        private void RebuildCards(IReadOnlyList<CharacterCore> survivors)
        {
            foreach (Button button in spawnedButtons)
            {
                if (button != null)
                {
                    Destroy(button.gameObject);
                }
            }

            spawnedButtons.Clear();

            if (cardContainer == null || cardButtonPrefab == null)
            {
                Debug.LogWarning("SuccessionUI missing cardContainer or cardButtonPrefab.");
                return;
            }

            foreach (CharacterCore survivor in survivors)
            {
                Button button = Instantiate(cardButtonPrefab, cardContainer);
                Text label = button.GetComponentInChildren<Text>();
                if (label != null)
                {
                    label.text = $"{survivor.DisplayName} ({survivor.CurrentLifeStage})";
                }

                CharacterCore selected = survivor;
                button.onClick.AddListener(() => legacyManager.ChooseSuccessor(selected));
                spawnedButtons.Add(button);
            }
        }

        private void CloseUI()
        {
            Time.timeScale = 1f;
            if (rootPanel != null)
            {
                rootPanel.SetActive(false);
            }
        }
    }
}

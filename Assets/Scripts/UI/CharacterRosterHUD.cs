using System.Text;
using UnityEngine;
using UnityEngine.UI;
using Survivebest.Core;

namespace Survivebest.UI
{
    public class CharacterRosterHUD : MonoBehaviour
    {
        [SerializeField] private HouseholdManager householdManager;
        [SerializeField] private Text rosterText;

        private readonly StringBuilder builder = new();

        private void OnEnable()
        {
            if (householdManager != null)
            {
                householdManager.OnMemberAdded += HandleRosterChanged;
                householdManager.OnMemberRemoved += HandleRosterChanged;
                householdManager.OnActiveCharacterChanged += HandleActiveChanged;
            }

            Refresh();
        }

        private void OnDisable()
        {
            if (householdManager != null)
            {
                householdManager.OnMemberAdded -= HandleRosterChanged;
                householdManager.OnMemberRemoved -= HandleRosterChanged;
                householdManager.OnActiveCharacterChanged -= HandleActiveChanged;
            }
        }

        private void HandleRosterChanged(CharacterCore character)
        {
            Refresh();
        }

        private void HandleActiveChanged(CharacterCore active)
        {
            Refresh();
        }

        private void Refresh()
        {
            if (rosterText == null)
            {
                return;
            }

            if (householdManager == null || householdManager.Members == null || householdManager.Members.Count == 0)
            {
                rosterText.text = "No household characters.";
                return;
            }

            builder.Clear();
            builder.AppendLine("Household:");
            foreach (CharacterCore member in householdManager.Members)
            {
                if (member == null)
                {
                    continue;
                }

                bool active = householdManager.ActiveCharacter == member;
                builder.Append(active ? "▶ " : "• ");
                builder.Append(member.DisplayName);
                builder.Append(" (");
                builder.Append(member.CurrentLifeStage);
                builder.AppendLine(")");
            }

            rosterText.text = builder.ToString().TrimEnd();
        }
    }
}

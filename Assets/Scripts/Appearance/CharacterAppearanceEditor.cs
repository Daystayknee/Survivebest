using UnityEngine;
using Survivebest.Core;

namespace Survivebest.Appearance
{
    public class CharacterAppearanceEditor : MonoBehaviour
    {
        [SerializeField] private CharacterCore targetCharacter;
        [SerializeField] private AppearanceManager appearanceManager;
        [SerializeField] private StyleIdentitySystem styleIdentitySystem;
        [SerializeField] private TattooSystem tattooSystem;
        [SerializeField] private FashionSystem fashionSystem;

        public bool TrySetHairDyeColor(Color dye)
        {
            if (appearanceManager == null)
            {
                return false;
            }

            HairProfile profile = appearanceManager.ScalpHairProfile;
            profile.UseDyedColor = true;
            profile.DyedHairColor = dye;
            appearanceManager.SetHairProfile(profile);
            return true;
        }

        public bool TrySetNaturalHairColor(Color _)
        {
            // Genetics-locked: natural hair color cannot be edited post-creation.
            return false;
        }

        public bool AddTattoo(string tattooId, string meaning)
        {
            if (targetCharacter == null || tattooSystem == null)
            {
                return false;
            }

            tattooSystem.AddTattoo(targetCharacter.CharacterId, tattooId, meaning);
            return true;
        }

        public bool SetStyleIdentity(StyleIdentityType styleType)
        {
            if (targetCharacter == null || styleIdentitySystem == null)
            {
                return false;
            }

            StyleIdentityProfile profile = styleIdentitySystem.GetOrCreateProfile(targetCharacter.CharacterId);
            profile.PrimaryStyle = styleType;
            return true;
        }

        public bool EquipFashionItem(ClothingItem item)
        {
            if (targetCharacter == null || fashionSystem == null || item == null)
            {
                return false;
            }

            fashionSystem.Equip(targetCharacter, item);
            return true;
        }
    }
}

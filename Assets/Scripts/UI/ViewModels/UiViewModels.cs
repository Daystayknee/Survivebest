using System;
using System.Collections.Generic;

namespace Survivebest.UI.ViewModels
{
    [Serializable] public class SplashScreenViewModel { public string BuildVersion; public string StatusText; public float Progress01; }
    [Serializable] public class MainMenuViewModel { public bool CanContinue; public string SelectedProfileName; }
    [Serializable] public class LoadSlotViewModel { public int SlotIndex; public string SaveName; public string Timestamp; public string Summary; public bool IsCorrupt; }
    [Serializable] public class SettingsViewModel { public float MasterVolume; public float MusicVolume; public float SfxVolume; public bool Fullscreen; public string Resolution; }
    [Serializable] public class WorldCreatorViewModel { public int MasterSeed; public string ProfileType; public string WorldName; }
    [Serializable] public class CharacterCreatorViewModel { public string DisplayName; public int Age; public string TraitSummary; }
    [Serializable] public class CharacterCreatorDashboardViewModel
    {
        public string ActiveTab;
        public string PreviewMode;
        public string PreviewBackground;
        public string HairTextureFilter;
        public string HairLengthFilter;
        public string FacialHairFilter;
        public int AvailableStyles;
        public int SavedPresetCount;
        public bool UseDyedHair;
        public string NaturalHairHex;
        public string DyedHairHex;
        public float OmbreAmount;
        public float HighlightIntensity;
    }

    [Serializable] public class HouseholdMakerViewModel { public string HouseholdName; public int MemberCount; public int StartingFunds; }
    [Serializable] public class GameplayHudViewModel { public int Day; public string Time; public string Weather; public int Funds; public float TownPressure; }
    [Serializable] public class JournalCardViewModel { public string Title; public string Body; public string DistrictId; public string Timestamp; public string Severity; }
    [Serializable] public class ActionPopupViewModel { public string Title; public string Description; public List<string> Options = new(); public bool CanConfirm; }
    [Serializable] public class SidebarActionViewModel { public string ActionId; public string Label; public bool Enabled; }
    [Serializable] public class ZoneScenePanelViewModel { public string ZoneName; public int Population; public int OpenVenues; public float Danger; }
    [Serializable] public class CharacterPortraitViewModel { public string CharacterId; public string DisplayName; public string MoodLabel; public string PortraitKey; }
    [Serializable] public class CharacterRosterItemViewModel { public string CharacterId; public string DisplayName; public string Role; public float Health; public float Energy; }
}

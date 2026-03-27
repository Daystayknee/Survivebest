using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using Survivebest.Events;
using Survivebest.World;
using Survivebest.Appearance;

namespace Survivebest.UI
{
    [Serializable]
    public class GameplaySettingsData
    {
        [Range(0f, 1f)] public float MasterVolume = 1f;
        [Range(0f, 1f)] public float MusicVolume = 0.7f;
        [Range(0f, 1f)] public float SfxVolume = 0.8f;
        public bool Fullscreen = true;
        public bool ShowSubtitles = true;
        public bool PauseOnFocusLoss = true;
        [Range(0.5f, 3f)] public float UiScale = 1f;
        [Min(6f)] public float RealSecondsPerGameHour = 300f;
        [Min(2.4f)] public float RealSecondsPerGameDay = 120f;
        public bool AutoHairGrowthEnabled = true;
        public bool AutoShavingAndCuttingEnabled = false;
        [Range(1, 30)] public int AutoShavingAndCuttingIntervalDays = 7;

        public Color PrimaryColor = new(0.13f, 0.7f, 1f, 1f);
        public Color SecondaryColor = new(0.92f, 0.52f, 0.2f, 1f);
        public Color BackgroundColor = new(0.08f, 0.08f, 0.1f, 1f);
        public Color TraitPillColor = new(0.2f, 0.75f, 0.35f, 1f);
    }

    public class SettingsPageController : MonoBehaviour
    {
        [SerializeField] private AudioMixer audioMixer;
        [SerializeField] private GameplaySettingsData settings = new();
        [SerializeField] private GameEventHub gameEventHub;
        [SerializeField] private WorldClock worldClock;
        [SerializeField] private HairGroomingSystem hairGroomingSystem;

        [Header("Optional UI Color Targets")]
        [SerializeField] private List<Graphic> primaryColorTargets = new();
        [SerializeField] private List<Graphic> secondaryColorTargets = new();
        [SerializeField] private List<Graphic> backgroundColorTargets = new();
        [SerializeField] private List<Graphic> traitPillColorTargets = new();

        public GameplaySettingsData CurrentSettings => settings;

        private const string MasterVolumeKey = "MasterVolume";
        private const string MusicVolumeKey = "MusicVolume";
        private const string SfxVolumeKey = "SfxVolume";
        private const string FullscreenKey = "Fullscreen";
        private const string SubtitlesKey = "Subtitles";
        private const string PauseOnFocusLossKey = "PauseOnFocusLoss";
        private const string UiScaleKey = "UiScale";
        private const string RealSecondsPerGameHourKey = "RealSecondsPerGameHour";
        private const string RealSecondsPerGameDayKey = "RealSecondsPerGameDay";
        private const string AutoHairGrowthEnabledKey = "AutoHairGrowthEnabled";
        private const string AutoShavingAndCuttingEnabledKey = "AutoShavingAndCuttingEnabled";
        private const string AutoShavingAndCuttingIntervalDaysKey = "AutoShavingAndCuttingIntervalDays";

        private const string PrimaryColorRKey = "PrimaryColorR";
        private const string PrimaryColorGKey = "PrimaryColorG";
        private const string PrimaryColorBKey = "PrimaryColorB";
        private const string SecondaryColorRKey = "SecondaryColorR";
        private const string SecondaryColorGKey = "SecondaryColorG";
        private const string SecondaryColorBKey = "SecondaryColorB";
        private const string BackgroundColorRKey = "BackgroundColorR";
        private const string BackgroundColorGKey = "BackgroundColorG";
        private const string BackgroundColorBKey = "BackgroundColorB";
        private const string TraitPillColorRKey = "TraitPillColorR";
        private const string TraitPillColorGKey = "TraitPillColorG";
        private const string TraitPillColorBKey = "TraitPillColorB";

        private void Awake()
        {
            LoadSettings();
            ApplySettings();
        }

        public void SetMasterVolume(float value) { settings.MasterVolume = Mathf.Clamp01(value); ApplySettings(); SaveSettings(); }
        public void SetMusicVolume(float value) { settings.MusicVolume = Mathf.Clamp01(value); ApplySettings(); SaveSettings(); }
        public void SetSfxVolume(float value) { settings.SfxVolume = Mathf.Clamp01(value); ApplySettings(); SaveSettings(); }

        public void SetFullscreen(bool value)
        {
            settings.Fullscreen = value;
            ApplySettings();
            SaveSettings();
        }

        public void SetShowSubtitles(bool value)
        {
            settings.ShowSubtitles = value;
            PublishSettingsEvent(nameof(settings.ShowSubtitles), value ? 1f : 0f);
            SaveSettings();
        }

        public void SetPauseOnFocusLoss(bool value)
        {
            settings.PauseOnFocusLoss = value;
            PublishSettingsEvent(nameof(settings.PauseOnFocusLoss), value ? 1f : 0f);
            SaveSettings();
        }

        public void SetUiScale(float value)
        {
            settings.UiScale = Mathf.Clamp(value, 0.5f, 3f);
            PublishSettingsEvent(nameof(settings.UiScale), settings.UiScale);
            SaveSettings();
        }

        public void SetRealSecondsPerGameHour(float value)
        {
            settings.RealSecondsPerGameHour = Mathf.Max(6f, value);
            settings.RealSecondsPerGameDay = settings.RealSecondsPerGameHour * 24f;
            ApplySettings();
            SaveSettings();
        }

        public void SetRealSecondsPerGameDay(float value)
        {
            settings.RealSecondsPerGameDay = Mathf.Max(2.4f, value);
            settings.RealSecondsPerGameHour = Mathf.Max(6f, settings.RealSecondsPerGameDay / 24f);
            ApplySettings();
            SaveSettings();
        }

        public void SetAutoHairGrowthEnabled(bool value)
        {
            settings.AutoHairGrowthEnabled = value;
            ApplySettings();
            SaveSettings();
        }

        public void SetAutoShavingAndCuttingEnabled(bool value)
        {
            settings.AutoShavingAndCuttingEnabled = value;
            ApplySettings();
            SaveSettings();
        }

        public void SetAutoShavingAndCuttingIntervalDays(int value)
        {
            settings.AutoShavingAndCuttingIntervalDays = Mathf.Clamp(value, 1, 30);
            ApplySettings();
            SaveSettings();
        }

        public void SetPrimaryColor(float r, float g, float b)
        {
            settings.PrimaryColor = new Color(Mathf.Clamp01(r), Mathf.Clamp01(g), Mathf.Clamp01(b), 1f);
            ApplyColors();
            SaveSettings();
        }

        public void SetSecondaryColor(float r, float g, float b)
        {
            settings.SecondaryColor = new Color(Mathf.Clamp01(r), Mathf.Clamp01(g), Mathf.Clamp01(b), 1f);
            ApplyColors();
            SaveSettings();
        }

        public void SetBackgroundColor(float r, float g, float b)
        {
            settings.BackgroundColor = new Color(Mathf.Clamp01(r), Mathf.Clamp01(g), Mathf.Clamp01(b), 1f);
            ApplyColors();
            SaveSettings();
        }

        public void SetTraitPillColor(float r, float g, float b)
        {
            settings.TraitPillColor = new Color(Mathf.Clamp01(r), Mathf.Clamp01(g), Mathf.Clamp01(b), 1f);
            ApplyColors();
            SaveSettings();
        }

        public void ResetDefaults()
        {
            settings = new GameplaySettingsData();
            ApplySettings();
            SaveSettings();
        }

        private void ApplySettings()
        {
            AudioSet("MasterVolume", settings.MasterVolume);
            AudioSet("MusicVolume", settings.MusicVolume);
            AudioSet("SfxVolume", settings.SfxVolume);
            Screen.fullScreen = settings.Fullscreen;
            worldClock ??= FindObjectOfType<WorldClock>();
            hairGroomingSystem ??= FindObjectOfType<HairGroomingSystem>();

            if (worldClock != null)
            {
                worldClock.SetRealSecondsPerGameHour(settings.RealSecondsPerGameHour);
                settings.RealSecondsPerGameDay = settings.RealSecondsPerGameHour * 24f;
            }

            if (hairGroomingSystem != null)
            {
                hairGroomingSystem.SetAutoHairGrowthEnabled(settings.AutoHairGrowthEnabled);
                hairGroomingSystem.SetAutoShavingAndCuttingEnabled(settings.AutoShavingAndCuttingEnabled);
                hairGroomingSystem.SetAutoShavingAndCuttingIntervalDays(settings.AutoShavingAndCuttingIntervalDays);
            }
            ApplyColors();

            PublishSettingsEvent("ApplySettings", settings.MasterVolume + settings.MusicVolume + settings.SfxVolume);
        }

        private void ApplyColors()
        {
            ApplyColorList(primaryColorTargets, settings.PrimaryColor);
            ApplyColorList(secondaryColorTargets, settings.SecondaryColor);
            ApplyColorList(backgroundColorTargets, settings.BackgroundColor);
            ApplyColorList(traitPillColorTargets, settings.TraitPillColor);
            PublishSettingsEvent("ThemeColors", settings.PrimaryColor.r + settings.SecondaryColor.g + settings.BackgroundColor.b);
        }

        private static void ApplyColorList(List<Graphic> graphics, Color color)
        {
            if (graphics == null)
            {
                return;
            }

            for (int i = 0; i < graphics.Count; i++)
            {
                if (graphics[i] != null)
                {
                    graphics[i].color = color;
                }
            }
        }

        private void AudioSet(string parameter, float normalized)
        {
            if (audioMixer == null)
            {
                return;
            }

            float db = Mathf.Log10(Mathf.Max(0.0001f, normalized)) * 20f;
            audioMixer.SetFloat(parameter, db);
        }

        private void SaveSettings()
        {
            PlayerPrefs.SetFloat(MasterVolumeKey, settings.MasterVolume);
            PlayerPrefs.SetFloat(MusicVolumeKey, settings.MusicVolume);
            PlayerPrefs.SetFloat(SfxVolumeKey, settings.SfxVolume);
            PlayerPrefs.SetInt(FullscreenKey, settings.Fullscreen ? 1 : 0);
            PlayerPrefs.SetInt(SubtitlesKey, settings.ShowSubtitles ? 1 : 0);
            PlayerPrefs.SetInt(PauseOnFocusLossKey, settings.PauseOnFocusLoss ? 1 : 0);
            PlayerPrefs.SetFloat(UiScaleKey, settings.UiScale);
            PlayerPrefs.SetFloat(RealSecondsPerGameHourKey, settings.RealSecondsPerGameHour);
            PlayerPrefs.SetFloat(RealSecondsPerGameDayKey, settings.RealSecondsPerGameDay);
            PlayerPrefs.SetInt(AutoHairGrowthEnabledKey, settings.AutoHairGrowthEnabled ? 1 : 0);
            PlayerPrefs.SetInt(AutoShavingAndCuttingEnabledKey, settings.AutoShavingAndCuttingEnabled ? 1 : 0);
            PlayerPrefs.SetInt(AutoShavingAndCuttingIntervalDaysKey, settings.AutoShavingAndCuttingIntervalDays);

            SaveColor(PrimaryColorRKey, PrimaryColorGKey, PrimaryColorBKey, settings.PrimaryColor);
            SaveColor(SecondaryColorRKey, SecondaryColorGKey, SecondaryColorBKey, settings.SecondaryColor);
            SaveColor(BackgroundColorRKey, BackgroundColorGKey, BackgroundColorBKey, settings.BackgroundColor);
            SaveColor(TraitPillColorRKey, TraitPillColorGKey, TraitPillColorBKey, settings.TraitPillColor);

            PlayerPrefs.Save();
        }

        private void LoadSettings()
        {
            settings.MasterVolume = PlayerPrefs.GetFloat(MasterVolumeKey, settings.MasterVolume);
            settings.MusicVolume = PlayerPrefs.GetFloat(MusicVolumeKey, settings.MusicVolume);
            settings.SfxVolume = PlayerPrefs.GetFloat(SfxVolumeKey, settings.SfxVolume);
            settings.Fullscreen = PlayerPrefs.GetInt(FullscreenKey, settings.Fullscreen ? 1 : 0) == 1;
            settings.ShowSubtitles = PlayerPrefs.GetInt(SubtitlesKey, settings.ShowSubtitles ? 1 : 0) == 1;
            settings.PauseOnFocusLoss = PlayerPrefs.GetInt(PauseOnFocusLossKey, settings.PauseOnFocusLoss ? 1 : 0) == 1;
            settings.UiScale = PlayerPrefs.GetFloat(UiScaleKey, settings.UiScale);
            settings.RealSecondsPerGameHour = PlayerPrefs.GetFloat(RealSecondsPerGameHourKey, settings.RealSecondsPerGameHour);
            settings.RealSecondsPerGameDay = PlayerPrefs.GetFloat(RealSecondsPerGameDayKey, settings.RealSecondsPerGameDay);
            settings.AutoHairGrowthEnabled = PlayerPrefs.GetInt(AutoHairGrowthEnabledKey, settings.AutoHairGrowthEnabled ? 1 : 0) == 1;
            settings.AutoShavingAndCuttingEnabled = PlayerPrefs.GetInt(AutoShavingAndCuttingEnabledKey, settings.AutoShavingAndCuttingEnabled ? 1 : 0) == 1;
            settings.AutoShavingAndCuttingIntervalDays = PlayerPrefs.GetInt(AutoShavingAndCuttingIntervalDaysKey, settings.AutoShavingAndCuttingIntervalDays);
            settings.RealSecondsPerGameHour = Mathf.Max(6f, settings.RealSecondsPerGameHour);
            settings.RealSecondsPerGameDay = Mathf.Max(2.4f, settings.RealSecondsPerGameDay);
            settings.AutoShavingAndCuttingIntervalDays = Mathf.Clamp(settings.AutoShavingAndCuttingIntervalDays, 1, 30);

            settings.PrimaryColor = LoadColor(PrimaryColorRKey, PrimaryColorGKey, PrimaryColorBKey, settings.PrimaryColor);
            settings.SecondaryColor = LoadColor(SecondaryColorRKey, SecondaryColorGKey, SecondaryColorBKey, settings.SecondaryColor);
            settings.BackgroundColor = LoadColor(BackgroundColorRKey, BackgroundColorGKey, BackgroundColorBKey, settings.BackgroundColor);
            settings.TraitPillColor = LoadColor(TraitPillColorRKey, TraitPillColorGKey, TraitPillColorBKey, settings.TraitPillColor);
        }

        private static void SaveColor(string rKey, string gKey, string bKey, Color c)
        {
            PlayerPrefs.SetFloat(rKey, c.r);
            PlayerPrefs.SetFloat(gKey, c.g);
            PlayerPrefs.SetFloat(bKey, c.b);
        }

        private static Color LoadColor(string rKey, string gKey, string bKey, Color fallback)
        {
            return new Color(
                PlayerPrefs.GetFloat(rKey, fallback.r),
                PlayerPrefs.GetFloat(gKey, fallback.g),
                PlayerPrefs.GetFloat(bKey, fallback.b),
                1f);
        }

        private void PublishSettingsEvent(string change, float magnitude)
        {
            (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
            {
                Type = SimulationEventType.SettingsChanged,
                Severity = SimulationEventSeverity.Info,
                SystemName = nameof(SettingsPageController),
                ChangeKey = change,
                Reason = $"Settings updated: {change}",
                Magnitude = magnitude
            });
        }
    }
}

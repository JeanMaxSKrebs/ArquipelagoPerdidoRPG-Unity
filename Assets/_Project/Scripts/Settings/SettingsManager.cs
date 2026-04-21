using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace ArquipelagoPerdidoRPG.Settings
{
    public class SettingsManager : ArquipelagoPerdidoRPG.Core.SingletonBehaviour<SettingsManager>
    {
        private const string MasterVolumeKey = "settings.audio.master";
        private const string MusicVolumeKey = "settings.audio.music";
        private const string SfxVolumeKey = "settings.audio.sfx";
        private const string MuteKey = "settings.audio.mute";
        private const string ShowFpsKey = "settings.fps.show";
        private const string FpsLimitKey = "settings.fps.limit";
        private const string FullscreenKey = "settings.video.fullscreen";
        private const string ResolutionIndexKey = "settings.video.resolutionIndex";
        private const string QualityLevelKey = "settings.video.qualityLevel";
        private const string MouseSensitivityKey = "settings.input.mouseSensitivity";

        [Header("Audio Mixer")]
        [SerializeField] private AudioMixer audioMixer;
        [SerializeField] private string masterVolumeParam = "MasterVolume";
        [SerializeField] private string musicVolumeParam = "MusicVolume";
        [SerializeField] private string sfxVolumeParam = "SfxVolume";

        [Header("FPS")]
        [SerializeField] private int defaultFpsLimit = 60;

        [Header("Input")]
        [SerializeField] private float defaultMouseSensitivity = 1f;
        [SerializeField] private float minMouseSensitivity = 0.1f;
        [SerializeField] private float maxMouseSensitivity = 5f;

        public bool ShowFps { get; private set; }
        public int FpsLimit { get; private set; }
        public bool Fullscreen { get; private set; }
        public int ResolutionIndex { get; private set; }
        public float MasterVolume { get; private set; }
        public float MusicVolume { get; private set; }
        public float SfxVolume { get; private set; }
        public bool IsMuted { get; private set; }
        public int QualityLevel { get; private set; }
        public float MouseSensitivity { get; private set; }

        public IReadOnlyList<Resolution> AvailableResolutions => _availableResolutions;

        public event Action OnSettingsApplied;

        private readonly List<Resolution> _availableResolutions = new List<Resolution>();

        protected override void Awake()
        {
            base.Awake();
            BuildResolutionList();
            LoadSettings();
            ApplyAll();
        }

        private void BuildResolutionList()
        {
            _availableResolutions.Clear();
            Resolution[] raw = Screen.resolutions;

            foreach (Resolution resolution in raw)
            {
                bool alreadyExists = false;
                for (int i = 0; i < _availableResolutions.Count; i++)
                {
                    if (_availableResolutions[i].width == resolution.width && _availableResolutions[i].height == resolution.height)
                    {
                        alreadyExists = true;
                        break;
                    }
                }

                if (!alreadyExists)
                {
                    _availableResolutions.Add(resolution);
                }
            }

            if (_availableResolutions.Count == 0)
            {
                _availableResolutions.Add(Screen.currentResolution);
            }
        }

        private void LoadSettings()
        {
            MasterVolume = PlayerPrefs.GetFloat(MasterVolumeKey, 1f);
            MusicVolume = PlayerPrefs.GetFloat(MusicVolumeKey, 1f);
            SfxVolume = PlayerPrefs.GetFloat(SfxVolumeKey, 1f);
            IsMuted = PlayerPrefs.GetInt(MuteKey, 0) == 1;
            ShowFps = PlayerPrefs.GetInt(ShowFpsKey, 1) == 1;
            FpsLimit = PlayerPrefs.GetInt(FpsLimitKey, defaultFpsLimit);
            Fullscreen = PlayerPrefs.GetInt(FullscreenKey, 1) == 1;
            QualityLevel = PlayerPrefs.GetInt(QualityLevelKey, QualitySettings.GetQualityLevel());
            MouseSensitivity = PlayerPrefs.GetFloat(MouseSensitivityKey, defaultMouseSensitivity);

            int currentIndex = FindResolutionIndex(Screen.width, Screen.height);
            int loadedIndex = PlayerPrefs.GetInt(ResolutionIndexKey, currentIndex);
            ResolutionIndex = Mathf.Clamp(loadedIndex, 0, _availableResolutions.Count - 1);

            if (QualitySettings.names != null && QualitySettings.names.Length > 0)
            {
                QualityLevel = Mathf.Clamp(QualityLevel, 0, QualitySettings.names.Length - 1);
            }
            else
            {
                QualityLevel = 0;
            }

            MouseSensitivity = Mathf.Clamp(MouseSensitivity, minMouseSensitivity, maxMouseSensitivity);
        }

        public void SetMasterVolume(float value)
        {
            MasterVolume = Mathf.Clamp01(value);
            PlayerPrefs.SetFloat(MasterVolumeKey, MasterVolume);
            ApplyAudio();
        }

        public void SetMusicVolume(float value)
        {
            MusicVolume = Mathf.Clamp01(value);
            PlayerPrefs.SetFloat(MusicVolumeKey, MusicVolume);
            ApplyAudio();
        }

        public void SetSfxVolume(float value)
        {
            SfxVolume = Mathf.Clamp01(value);
            PlayerPrefs.SetFloat(SfxVolumeKey, SfxVolume);
            ApplyAudio();
        }

        public void SetMute(bool value)
        {
            IsMuted = value;
            PlayerPrefs.SetInt(MuteKey, IsMuted ? 1 : 0);
            ApplyAudio();
        }

        public void SetShowFps(bool value)
        {
            ShowFps = value;
            PlayerPrefs.SetInt(ShowFpsKey, ShowFps ? 1 : 0);
            OnSettingsApplied?.Invoke();
        }

        public void SetFpsLimit(int value)
        {
            FpsLimit = Mathf.Clamp(value, 30, 240);
            PlayerPrefs.SetInt(FpsLimitKey, FpsLimit);
            ApplyFps();
        }

        public void SetFullscreen(bool value)
        {
            Fullscreen = value;
            PlayerPrefs.SetInt(FullscreenKey, Fullscreen ? 1 : 0);
            ApplyVideo();
        }

        public void SetResolutionIndex(int index)
        {
            ResolutionIndex = Mathf.Clamp(index, 0, _availableResolutions.Count - 1);
            PlayerPrefs.SetInt(ResolutionIndexKey, ResolutionIndex);
            ApplyVideo();
        }

        public void SetQualityLevel(int level)
        {
            if (QualitySettings.names == null || QualitySettings.names.Length == 0)
            {
                return;
            }

            QualityLevel = Mathf.Clamp(level, 0, QualitySettings.names.Length - 1);
            PlayerPrefs.SetInt(QualityLevelKey, QualityLevel);
            ApplyVideo();
        }

        public void SetMouseSensitivity(float value)
        {
            MouseSensitivity = Mathf.Clamp(value, minMouseSensitivity, maxMouseSensitivity);
            PlayerPrefs.SetFloat(MouseSensitivityKey, MouseSensitivity);
            OnSettingsApplied?.Invoke();
        }

        public float GetMinMouseSensitivity()
        {
            return minMouseSensitivity;
        }

        public float GetMaxMouseSensitivity()
        {
            return maxMouseSensitivity;
        }

        public void ApplyAll()
        {
            ApplyAudio();
            ApplyFps();
            ApplyVideo();
            OnSettingsApplied?.Invoke();
        }

        private void ApplyAudio()
        {
            float muteMultiplier = IsMuted ? 0f : 1f;

            if (audioMixer != null)
            {
                audioMixer.SetFloat(masterVolumeParam, ToDecibels(MasterVolume * muteMultiplier));
                audioMixer.SetFloat(musicVolumeParam, ToDecibels(MusicVolume * muteMultiplier));
                audioMixer.SetFloat(sfxVolumeParam, ToDecibels(SfxVolume * muteMultiplier));
            }
            else
            {
                AudioListener.volume = MasterVolume * muteMultiplier;
            }

            OnSettingsApplied?.Invoke();
        }

        private void ApplyFps()
        {
            Application.targetFrameRate = FpsLimit;
            OnSettingsApplied?.Invoke();
        }

        private void ApplyVideo()
        {
            Resolution resolution = _availableResolutions[ResolutionIndex];
            Screen.SetResolution(resolution.width, resolution.height, Fullscreen);

            if (QualitySettings.names != null && QualitySettings.names.Length > 0)
            {
                QualitySettings.SetQualityLevel(QualityLevel);
            }

            OnSettingsApplied?.Invoke();
        }

        private static float ToDecibels(float linear)
        {
            float clamped = Mathf.Max(linear, 0.0001f);
            return Mathf.Log10(clamped) * 20f;
        }

        private int FindResolutionIndex(int width, int height)
        {
            for (int i = 0; i < _availableResolutions.Count; i++)
            {
                if (_availableResolutions[i].width == width && _availableResolutions[i].height == height)
                {
                    return i;
                }
            }

            return _availableResolutions.Count - 1;
        }
    }
}

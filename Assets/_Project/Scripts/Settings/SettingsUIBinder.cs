using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ArquipelagoPerdidoRPG.Settings
{
    public class SettingsUIBinder : MonoBehaviour
    {
        [Header("Video")]
        [SerializeField] private Dropdown resolutionDropdown;
        [SerializeField] private Toggle fullscreenToggle;

        [Header("FPS")]
        [SerializeField] private Toggle showFpsToggle;
        [SerializeField] private Slider fpsLimitSlider;
        [SerializeField] private Text fpsLimitLabel;

        [Header("Audio")]
        [SerializeField] private Slider masterVolumeSlider;
        [SerializeField] private Slider musicVolumeSlider;
        [SerializeField] private Slider sfxVolumeSlider;

        private bool _isBinding;

        private void OnEnable()
        {
            BuildUiFromSettings();
            Subscribe();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void BuildUiFromSettings()
        {
            SettingsManager settings = SettingsManager.Instance;
            if (settings == null)
            {
                return;
            }

            _isBinding = true;

            if (resolutionDropdown != null)
            {
                List<string> options = new List<string>();
                var resolutions = settings.AvailableResolutions;
                for (int i = 0; i < resolutions.Count; i++)
                {
                    options.Add($"{resolutions[i].width} x {resolutions[i].height}");
                }

                resolutionDropdown.ClearOptions();
                resolutionDropdown.AddOptions(options);
                resolutionDropdown.value = settings.ResolutionIndex;
                resolutionDropdown.RefreshShownValue();
            }

            if (fullscreenToggle != null)
            {
                fullscreenToggle.isOn = settings.Fullscreen;
            }

            if (showFpsToggle != null)
            {
                showFpsToggle.isOn = settings.ShowFps;
            }

            if (fpsLimitSlider != null)
            {
                fpsLimitSlider.minValue = 30;
                fpsLimitSlider.maxValue = 240;
                fpsLimitSlider.wholeNumbers = true;
                fpsLimitSlider.value = settings.FpsLimit;
                UpdateFpsLabel(settings.FpsLimit);
            }

            if (masterVolumeSlider != null)
            {
                masterVolumeSlider.value = settings.MasterVolume;
            }

            if (musicVolumeSlider != null)
            {
                musicVolumeSlider.value = settings.MusicVolume;
            }

            if (sfxVolumeSlider != null)
            {
                sfxVolumeSlider.value = settings.SfxVolume;
            }

            _isBinding = false;
        }

        private void Subscribe()
        {
            if (resolutionDropdown != null)
            {
                resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
            }

            if (fullscreenToggle != null)
            {
                fullscreenToggle.onValueChanged.AddListener(OnFullscreenChanged);
            }

            if (showFpsToggle != null)
            {
                showFpsToggle.onValueChanged.AddListener(OnShowFpsChanged);
            }

            if (fpsLimitSlider != null)
            {
                fpsLimitSlider.onValueChanged.AddListener(OnFpsLimitChanged);
            }

            if (masterVolumeSlider != null)
            {
                masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
            }

            if (musicVolumeSlider != null)
            {
                musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
            }

            if (sfxVolumeSlider != null)
            {
                sfxVolumeSlider.onValueChanged.AddListener(OnSfxVolumeChanged);
            }
        }

        private void Unsubscribe()
        {
            if (resolutionDropdown != null)
            {
                resolutionDropdown.onValueChanged.RemoveListener(OnResolutionChanged);
            }

            if (fullscreenToggle != null)
            {
                fullscreenToggle.onValueChanged.RemoveListener(OnFullscreenChanged);
            }

            if (showFpsToggle != null)
            {
                showFpsToggle.onValueChanged.RemoveListener(OnShowFpsChanged);
            }

            if (fpsLimitSlider != null)
            {
                fpsLimitSlider.onValueChanged.RemoveListener(OnFpsLimitChanged);
            }

            if (masterVolumeSlider != null)
            {
                masterVolumeSlider.onValueChanged.RemoveListener(OnMasterVolumeChanged);
            }

            if (musicVolumeSlider != null)
            {
                musicVolumeSlider.onValueChanged.RemoveListener(OnMusicVolumeChanged);
            }

            if (sfxVolumeSlider != null)
            {
                sfxVolumeSlider.onValueChanged.RemoveListener(OnSfxVolumeChanged);
            }
        }

        private void OnResolutionChanged(int index)
        {
            if (_isBinding)
            {
                return;
            }

            SettingsManager.Instance.SetResolutionIndex(index);
        }

        private void OnFullscreenChanged(bool value)
        {
            if (_isBinding)
            {
                return;
            }

            SettingsManager.Instance.SetFullscreen(value);
        }

        private void OnShowFpsChanged(bool value)
        {
            if (_isBinding)
            {
                return;
            }

            SettingsManager.Instance.SetShowFps(value);
        }

        private void OnFpsLimitChanged(float value)
        {
            if (_isBinding)
            {
                return;
            }

            int intValue = Mathf.RoundToInt(value);
            SettingsManager.Instance.SetFpsLimit(intValue);
            UpdateFpsLabel(intValue);
        }

        private void OnMasterVolumeChanged(float value)
        {
            if (_isBinding)
            {
                return;
            }

            SettingsManager.Instance.SetMasterVolume(value);
        }

        private void OnMusicVolumeChanged(float value)
        {
            if (_isBinding)
            {
                return;
            }

            SettingsManager.Instance.SetMusicVolume(value);
        }

        private void OnSfxVolumeChanged(float value)
        {
            if (_isBinding)
            {
                return;
            }

            SettingsManager.Instance.SetSfxVolume(value);
        }

        private void UpdateFpsLabel(int value)
        {
            if (fpsLimitLabel != null)
            {
                fpsLimitLabel.text = $"FPS Limit: {value}";
            }
        }
    }
}

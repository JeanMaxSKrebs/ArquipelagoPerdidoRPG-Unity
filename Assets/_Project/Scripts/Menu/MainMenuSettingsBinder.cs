using System.Collections.Generic;
using ArquipelagoPerdidoRPG.Settings;
using UnityEngine;
using UnityEngine.UI;

namespace ArquipelagoPerdidoRPG.Menu
{
    public class MainMenuSettingsBinder : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject optionsPanel;
        [SerializeField] private GameObject languagePanel;

        [Header("Main Buttons")]
        [SerializeField] private Text playButtonText;
        [SerializeField] private Text optionsButtonText;
        [SerializeField] private Text languageButtonText;
        [SerializeField] private Text quitButtonText;

        [Header("Titles")]
        [SerializeField] private Text titleText;
        [SerializeField] private Text subtitleText;
        [SerializeField] private Text optionsTitleText;
        [SerializeField] private Text languageTitleText;

        [Header("Language Buttons")]
        [SerializeField] private Button languagePtBrButton;
        [SerializeField] private Button languageEnButton;
        [SerializeField] private Text languagePtBrLabel;
        [SerializeField] private Text languageEnLabel;

        [Header("Video")]
        [SerializeField] private Dropdown resolutionDropdown;
        [SerializeField] private Toggle fullscreenToggle;
        [SerializeField] private Toggle showFpsToggle;
        [SerializeField] private Slider fpsLimitSlider;
        [SerializeField] private Text fpsLimitLabel;
        [SerializeField] private Dropdown qualityDropdown;

        [Header("Audio")]
        [SerializeField] private Slider masterVolumeSlider;
        [SerializeField] private Slider musicVolumeSlider;
        [SerializeField] private Slider sfxVolumeSlider;
        [SerializeField] private Toggle muteToggle;

        [Header("Other")]
        [SerializeField] private Slider mouseSensitivitySlider;
        [SerializeField] private Text mouseSensitivityLabel;
        [SerializeField] private Dropdown languageMirrorDropdown;

        [Header("Option Tabs")]
        [SerializeField] private Button videoTabButton;
        [SerializeField] private Button audioTabButton;
        [SerializeField] private Button otherTabButton;
        [SerializeField] private Text videoTabText;
        [SerializeField] private Text audioTabText;
        [SerializeField] private Text otherTabText;
        [SerializeField] private GameObject videoSection;
        [SerializeField] private GameObject audioSection;
        [SerializeField] private GameObject otherSection;
        [SerializeField] private Text closeOptionsButtonText;
        [SerializeField] private Text closeLanguageButtonText;

        [Header("Option Labels")]
        [SerializeField] private Text resolutionLabel;
        [SerializeField] private Text fullscreenLabel;
        [SerializeField] private Text showFpsLabel;
        [SerializeField] private Text qualityLabel;
        [SerializeField] private Text masterVolumeLabel;
        [SerializeField] private Text musicVolumeLabel;
        [SerializeField] private Text sfxVolumeLabel;
        [SerializeField] private Text muteLabel;
        [SerializeField] private Text mouseSensitivityTitleLabel;
        [SerializeField] private Text languageMirrorLabel;
        [SerializeField] private Text videoSectionTitle;
        [SerializeField] private Text audioSectionTitle;
        [SerializeField] private Text otherSectionTitle;

        private bool _isBinding;

        private void Awake()
        {
            _ = SettingsManager.Instance;
            _ = LanguageManager.Instance;
            AutoResolveReferences();
        }

        private void OnEnable()
        {
            AutoResolveReferences();
            BuildUiFromSettings();
            Subscribe();
            RefreshLocalizedText();
            ShowVideoTab();

            if (LanguageManager.Instance != null)
            {
                LanguageManager.Instance.OnLanguageChanged += OnLanguageChanged;
            }
        }

        private void OnDisable()
        {
            Unsubscribe();

            if (LanguageManager.Instance != null)
            {
                LanguageManager.Instance.OnLanguageChanged -= OnLanguageChanged;
            }
        }

        public void SetLanguagePtBr()
        {
            LanguageManager.Instance?.SetLanguage(SupportedLanguage.PtBr);
        }

        public void SetLanguageEn()
        {
            LanguageManager.Instance?.SetLanguage(SupportedLanguage.En);
        }

        public void ShowVideoTab()
        {
            SetActiveTab(videoSection);
        }

        public void ShowAudioTab()
        {
            SetActiveTab(audioSection);
        }

        public void ShowOtherTab()
        {
            SetActiveTab(otherSection);
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
                var options = new List<string>();
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

            if (qualityDropdown != null)
            {
                qualityDropdown.ClearOptions();
                var qualityNames = QualitySettings.names;
                var options = new List<string>(qualityNames);
                qualityDropdown.AddOptions(options);
                qualityDropdown.value = settings.QualityLevel;
                qualityDropdown.RefreshShownValue();
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

            if (muteToggle != null)
            {
                muteToggle.isOn = settings.IsMuted;
            }

            if (mouseSensitivitySlider != null)
            {
                mouseSensitivitySlider.minValue = settings.GetMinMouseSensitivity();
                mouseSensitivitySlider.maxValue = settings.GetMaxMouseSensitivity();
                mouseSensitivitySlider.value = settings.MouseSensitivity;
                UpdateSensitivityLabel(settings.MouseSensitivity);
            }

            if (languageMirrorDropdown != null)
            {
                languageMirrorDropdown.ClearOptions();
                languageMirrorDropdown.AddOptions(new List<string> { "PT-BR", "EN" });
                languageMirrorDropdown.value = LanguageManager.Instance != null && LanguageManager.Instance.CurrentLanguage == SupportedLanguage.En ? 1 : 0;
                languageMirrorDropdown.RefreshShownValue();
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

            if (qualityDropdown != null)
            {
                qualityDropdown.onValueChanged.AddListener(OnQualityChanged);
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

            if (muteToggle != null)
            {
                muteToggle.onValueChanged.AddListener(OnMuteChanged);
            }

            if (mouseSensitivitySlider != null)
            {
                mouseSensitivitySlider.onValueChanged.AddListener(OnMouseSensitivityChanged);
            }

            if (languageMirrorDropdown != null)
            {
                languageMirrorDropdown.onValueChanged.AddListener(OnLanguageMirrorChanged);
            }

            if (languagePtBrButton != null)
            {
                languagePtBrButton.onClick.AddListener(SetLanguagePtBr);
            }

            if (languageEnButton != null)
            {
                languageEnButton.onClick.AddListener(SetLanguageEn);
            }

            if (videoTabButton != null)
            {
                videoTabButton.onClick.AddListener(ShowVideoTab);
            }

            if (audioTabButton != null)
            {
                audioTabButton.onClick.AddListener(ShowAudioTab);
            }

            if (otherTabButton != null)
            {
                otherTabButton.onClick.AddListener(ShowOtherTab);
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

            if (qualityDropdown != null)
            {
                qualityDropdown.onValueChanged.RemoveListener(OnQualityChanged);
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

            if (muteToggle != null)
            {
                muteToggle.onValueChanged.RemoveListener(OnMuteChanged);
            }

            if (mouseSensitivitySlider != null)
            {
                mouseSensitivitySlider.onValueChanged.RemoveListener(OnMouseSensitivityChanged);
            }

            if (languageMirrorDropdown != null)
            {
                languageMirrorDropdown.onValueChanged.RemoveListener(OnLanguageMirrorChanged);
            }

            if (languagePtBrButton != null)
            {
                languagePtBrButton.onClick.RemoveListener(SetLanguagePtBr);
            }

            if (languageEnButton != null)
            {
                languageEnButton.onClick.RemoveListener(SetLanguageEn);
            }

            if (videoTabButton != null)
            {
                videoTabButton.onClick.RemoveListener(ShowVideoTab);
            }

            if (audioTabButton != null)
            {
                audioTabButton.onClick.RemoveListener(ShowAudioTab);
            }

            if (otherTabButton != null)
            {
                otherTabButton.onClick.RemoveListener(ShowOtherTab);
            }
        }

        private void OnResolutionChanged(int value)
        {
            if (_isBinding || SettingsManager.Instance == null)
            {
                return;
            }

            SettingsManager.Instance.SetResolutionIndex(value);
        }

        private void OnFullscreenChanged(bool value)
        {
            if (_isBinding || SettingsManager.Instance == null)
            {
                return;
            }

            SettingsManager.Instance.SetFullscreen(value);
        }

        private void OnShowFpsChanged(bool value)
        {
            if (_isBinding || SettingsManager.Instance == null)
            {
                return;
            }

            SettingsManager.Instance.SetShowFps(value);
        }

        private void OnFpsLimitChanged(float value)
        {
            if (_isBinding || SettingsManager.Instance == null)
            {
                return;
            }

            int intValue = Mathf.RoundToInt(value);
            SettingsManager.Instance.SetFpsLimit(intValue);
            UpdateFpsLabel(intValue);
        }

        private void OnQualityChanged(int value)
        {
            if (_isBinding || SettingsManager.Instance == null)
            {
                return;
            }

            SettingsManager.Instance.SetQualityLevel(value);
        }

        private void OnMasterVolumeChanged(float value)
        {
            if (_isBinding || SettingsManager.Instance == null)
            {
                return;
            }

            SettingsManager.Instance.SetMasterVolume(value);
        }

        private void OnMusicVolumeChanged(float value)
        {
            if (_isBinding || SettingsManager.Instance == null)
            {
                return;
            }

            SettingsManager.Instance.SetMusicVolume(value);
        }

        private void OnSfxVolumeChanged(float value)
        {
            if (_isBinding || SettingsManager.Instance == null)
            {
                return;
            }

            SettingsManager.Instance.SetSfxVolume(value);
        }

        private void OnMuteChanged(bool value)
        {
            if (_isBinding || SettingsManager.Instance == null)
            {
                return;
            }

            SettingsManager.Instance.SetMute(value);
        }

        private void OnMouseSensitivityChanged(float value)
        {
            if (_isBinding || SettingsManager.Instance == null)
            {
                return;
            }

            SettingsManager.Instance.SetMouseSensitivity(value);
            UpdateSensitivityLabel(value);
        }

        private void OnLanguageMirrorChanged(int value)
        {
            if (_isBinding || LanguageManager.Instance == null)
            {
                return;
            }

            LanguageManager.Instance.SetLanguage(value == 0 ? SupportedLanguage.PtBr : SupportedLanguage.En);
        }

        private void OnLanguageChanged(SupportedLanguage language)
        {
            if (languageMirrorDropdown != null)
            {
                _isBinding = true;
                languageMirrorDropdown.value = language == SupportedLanguage.En ? 1 : 0;
                languageMirrorDropdown.RefreshShownValue();
                _isBinding = false;
            }

            RefreshLocalizedText();
        }

        private void RefreshLocalizedText()
        {
            bool isEnglish = LanguageManager.Instance != null && LanguageManager.Instance.CurrentLanguage == SupportedLanguage.En;

            SetText(playButtonText, isEnglish ? "Play" : "Jogar");
            SetText(optionsButtonText, isEnglish ? "Options" : "Opções");
            SetText(languageButtonText, isEnglish ? "Language" : "Idioma");
            SetText(quitButtonText, isEnglish ? "Quit" : "Sair");
            SetText(titleText, "Arquipelago Perdido RPG");
            SetText(subtitleText, isEnglish ? "Explore lost islands. Survive. Evolve." : "Explore ilhas perdidas. Sobreviva. Evolua.");
            SetText(optionsTitleText, isEnglish ? "Options" : "Opções");
            SetText(languageTitleText, isEnglish ? "Language" : "Idioma");
            SetText(languagePtBrLabel, "PT-BR");
            SetText(languageEnLabel, "EN");
            SetText(videoTabText, isEnglish ? "Video" : "Video");
            SetText(audioTabText, isEnglish ? "Audio" : "Som");
            SetText(otherTabText, isEnglish ? "Other" : "Outros");
            SetText(closeOptionsButtonText, isEnglish ? "Close Options" : "Fechar Opções");
            SetText(closeLanguageButtonText, isEnglish ? "Close" : "Fechar");

            SetText(videoSectionTitle, isEnglish ? "VIDEO" : "VIDEO");
            SetText(audioSectionTitle, isEnglish ? "AUDIO" : "SOM");
            SetText(otherSectionTitle, isEnglish ? "OTHER" : "OUTROS");
            SetText(resolutionLabel, isEnglish ? "Resolution" : "Resolucao");
            SetText(fullscreenLabel, isEnglish ? "Fullscreen" : "Tela Cheia");
            SetText(showFpsLabel, isEnglish ? "Show FPS" : "Mostrar FPS");
            SetText(qualityLabel, isEnglish ? "Graphics Quality" : "Qualidade Grafica");
            SetText(masterVolumeLabel, isEnglish ? "Master Volume" : "Volume Master");
            SetText(musicVolumeLabel, isEnglish ? "Music Volume" : "Volume Musica");
            SetText(sfxVolumeLabel, isEnglish ? "SFX Volume" : "Volume Efeitos");
            SetText(muteLabel, isEnglish ? "Mute" : "Mudo");
            SetText(mouseSensitivityTitleLabel, isEnglish ? "Mouse Sensitivity" : "Sensibilidade do Mouse");
            SetText(languageMirrorLabel, isEnglish ? "Language" : "Idioma");

            if (fpsLimitLabel != null && SettingsManager.Instance != null)
            {
                UpdateFpsLabel(SettingsManager.Instance.FpsLimit);
            }

            if (mouseSensitivityLabel != null && SettingsManager.Instance != null)
            {
                UpdateSensitivityLabel(SettingsManager.Instance.MouseSensitivity);
            }
        }

        private void SetText(Text target, string value)
        {
            if (target != null)
            {
                target.text = value;
            }
        }

        private void UpdateFpsLabel(int value)
        {
            bool isEnglish = LanguageManager.Instance != null && LanguageManager.Instance.CurrentLanguage == SupportedLanguage.En;
            SetText(fpsLimitLabel, isEnglish ? $"FPS Limit: {value}" : $"Limite FPS: {value}");
        }

        private void UpdateSensitivityLabel(float value)
        {
            bool isEnglish = LanguageManager.Instance != null && LanguageManager.Instance.CurrentLanguage == SupportedLanguage.En;
            string numeric = value.ToString("0.00");
            SetText(mouseSensitivityLabel, isEnglish ? $"Mouse Sensitivity: {numeric}" : $"Sensibilidade do Mouse: {numeric}");
        }

        private void SetActiveTab(GameObject activeSection)
        {
            if (videoSection != null)
            {
                videoSection.SetActive(videoSection == activeSection);
            }

            if (audioSection != null)
            {
                audioSection.SetActive(audioSection == activeSection);
            }

            if (otherSection != null)
            {
                otherSection.SetActive(otherSection == activeSection);
            }

            UpdateTabVisual(videoTabButton, videoSection == activeSection);
            UpdateTabVisual(audioTabButton, audioSection == activeSection);
            UpdateTabVisual(otherTabButton, otherSection == activeSection);
        }

        private void UpdateTabVisual(Button button, bool isActive)
        {
            if (button == null)
            {
                return;
            }

            ColorBlock colors = button.colors;
            if (isActive)
            {
                colors.normalColor = new Color(0.10f, 0.52f, 0.72f, 1f);
            }
            else
            {
                colors.normalColor = new Color(0.07f, 0.40f, 0.55f, 0.98f);
            }

            button.colors = colors;
        }

        private void AutoResolveReferences()
        {
            playButtonText ??= FindByNamePath<Text>("Panel_Main/Buttons_Container/Button_Play/Text");
            optionsButtonText ??= FindByNamePath<Text>("Panel_Main/Buttons_Container/Button_Options/Text");
            languageButtonText ??= FindByNamePath<Text>("Panel_Main/Buttons_Container/Button_Language/Text");
            quitButtonText ??= FindByNamePath<Text>("Panel_Main/Buttons_Container/Button_Quit/Text");

            if (optionsPanel != null)
            {
                videoSection ??= FindObjectByPath(optionsPanel.transform, "Options_Content/Section_Video");
                audioSection ??= FindObjectByPath(optionsPanel.transform, "Options_Content/Section_Audio");
                otherSection ??= FindObjectByPath(optionsPanel.transform, "Options_Content/Section_Other");

                videoTabButton ??= FindByPath<Button>(optionsPanel.transform, "Options_Tabs/Button_TabVideo");
                audioTabButton ??= FindByPath<Button>(optionsPanel.transform, "Options_Tabs/Button_TabAudio");
                otherTabButton ??= FindByPath<Button>(optionsPanel.transform, "Options_Tabs/Button_TabOther");

                videoTabText ??= FindByPath<Text>(optionsPanel.transform, "Options_Tabs/Button_TabVideo/Text");
                audioTabText ??= FindByPath<Text>(optionsPanel.transform, "Options_Tabs/Button_TabAudio/Text");
                otherTabText ??= FindByPath<Text>(optionsPanel.transform, "Options_Tabs/Button_TabOther/Text");

                closeOptionsButtonText ??= FindByPath<Text>(optionsPanel.transform, "Button_CloseOptions/Text");
                resolutionLabel ??= FindByPath<Text>(optionsPanel.transform, "Options_Content/Section_Video/Row_Resolution/Text_Label");
                fullscreenLabel ??= FindByPath<Text>(optionsPanel.transform, "Options_Content/Section_Video/Row_Fullscreen/Text_Label");
                showFpsLabel ??= FindByPath<Text>(optionsPanel.transform, "Options_Content/Section_Video/Row_ShowFPS/Text_Label");
                qualityLabel ??= FindByPath<Text>(optionsPanel.transform, "Options_Content/Section_Video/Row_Quality/Text_Label");
                masterVolumeLabel ??= FindByPath<Text>(optionsPanel.transform, "Options_Content/Section_Audio/Row_MasterVolume/Text_Label");
                musicVolumeLabel ??= FindByPath<Text>(optionsPanel.transform, "Options_Content/Section_Audio/Row_MusicVolume/Text_Label");
                sfxVolumeLabel ??= FindByPath<Text>(optionsPanel.transform, "Options_Content/Section_Audio/Row_SfxVolume/Text_Label");
                muteLabel ??= FindByPath<Text>(optionsPanel.transform, "Options_Content/Section_Audio/Row_Mute/Text_Label");
                mouseSensitivityTitleLabel ??= FindByPath<Text>(optionsPanel.transform, "Options_Content/Section_Other/Row_Sensitivity/Text_Label");
                languageMirrorLabel ??= FindByPath<Text>(optionsPanel.transform, "Options_Content/Section_Other/Row_LanguageMirror/Text_Label");
                videoSectionTitle ??= FindByPath<Text>(optionsPanel.transform, "Options_Content/Section_Video/Text_SectionTitle");
                audioSectionTitle ??= FindByPath<Text>(optionsPanel.transform, "Options_Content/Section_Audio/Text_SectionTitle");
                otherSectionTitle ??= FindByPath<Text>(optionsPanel.transform, "Options_Content/Section_Other/Text_SectionTitle");
            }

            if (languagePanel != null)
            {
                closeLanguageButtonText ??= FindByPath<Text>(languagePanel.transform, "Button_CloseLanguage/Text");
            }
        }

        private T FindByPath<T>(Transform root, string path) where T : Component
        {
            if (root == null)
            {
                return null;
            }

            Transform found = root.Find(path);
            if (found == null)
            {
                return null;
            }

            return found.GetComponent<T>();
        }

        private T FindByNamePath<T>(string path) where T : Component
        {
            Transform root = transform;
            Transform found = root.Find(path);
            if (found == null)
            {
                return null;
            }

            return found.GetComponent<T>();
        }

        private GameObject FindObjectByPath(Transform root, string path)
        {
            if (root == null)
            {
                return null;
            }

            Transform found = root.Find(path);
            return found != null ? found.gameObject : null;
        }
    }
}

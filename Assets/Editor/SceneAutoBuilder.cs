#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using ArquipelagoPerdidoRPG.Core;
using ArquipelagoPerdidoRPG.Menu;
using ArquipelagoPerdidoRPG.Player;
using ArquipelagoPerdidoRPG.UI;
using ArquipelagoPerdidoRPG.World;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ArquipelagoPerdidoRPG.Editor
{
    public static class SceneAutoBuilder
    {
        private const int TerrainCountExpected = 14;
        private const int TerrainWidth = 500;
        private const int TerrainLength = 500;
        private const string FixedPlayerPrefabPath = "Assets/StarterAssets/FirstPersonController/Prefabs/NestedParent_Unpack.prefab";

        [MenuItem("Tools/Arquipelago/Create MainMenu UI")]
        public static void CreateMainMenuUI()
        {
            EnsureScenesInBuildSettings();
            EnsureAppBootstrap();

            Canvas canvas = EnsureCanvas("Canvas_MainMenu");
            SetupMainMenuBackground(canvas.transform);

            GameObject panelMain = EnsurePanel("Panel_Main", canvas.transform, true);
            GameObject panelOptions = EnsurePanel("Panel_Options", canvas.transform, false);
            GameObject panelLanguage = EnsurePanel("Panel_Language", canvas.transform, false);

            RemoveLegacyChild(panelOptions.transform, "Options_Sections");

            RectTransform panelMainRect = EnsureRectTransform(panelMain);
            panelMainRect.sizeDelta = new Vector2(700f, 560f);
            EnsureComponent<Image>(panelMain).color = new Color(0.02f, 0.08f, 0.14f, 0.86f);
            RemoveAutoLayout(panelMain);
            AddVerticalLayout(panelMain, 18f, TextAnchor.UpperCenter, new RectOffset(44, 44, 48, 48));

            RectTransform optionsRect = EnsureRectTransform(panelOptions);
            optionsRect.sizeDelta = new Vector2(1040f, 860f);
            EnsureComponent<Image>(panelOptions).color = new Color(0.03f, 0.09f, 0.15f, 0.97f);
            RemoveAutoLayout(panelOptions);

            RectTransform languageRect = EnsureRectTransform(panelLanguage);
            languageRect.sizeDelta = new Vector2(560f, 360f);
            EnsureComponent<Image>(panelLanguage).color = new Color(0.03f, 0.09f, 0.15f, 0.97f);
            RemoveAutoLayout(panelLanguage);

            GameObject title = EnsureTextObject("Text_Title", panelMain.transform, "Arquipelago Perdido RPG", 56, TextAnchor.MiddleCenter);
            GameObject subtitle = EnsureTextObject("Text_Subtitle", panelMain.transform, "Explore ilhas perdidas. Sobreviva. Evolua.", 22, TextAnchor.MiddleCenter);
            StyleText(title.GetComponent<Text>(), new Color(0.90f, 0.98f, 1f, 1f), FontStyle.Bold);
            StyleText(subtitle.GetComponent<Text>(), new Color(0.65f, 0.80f, 0.88f, 1f), FontStyle.Italic);

            GameObject buttonsContainer = EnsureChildObject("Buttons_Container", panelMain.transform);
            RectTransform buttonsRect = EnsureRectTransform(buttonsContainer);
            buttonsRect.sizeDelta = new Vector2(500f, 280f);
            AddVerticalLayout(buttonsContainer, 16f, TextAnchor.UpperCenter, new RectOffset(0, 0, 0, 0));

            Button playButton = EnsureButton("Button_Play", buttonsContainer.transform, "Play");
            Button optionsButton = EnsureButton("Button_Options", buttonsContainer.transform, "Opções");
            Button languageButton = EnsureButton("Button_Language", buttonsContainer.transform, "Language");
            Button quitButton = EnsureButton("Button_Quit", buttonsContainer.transform, "Quit");
            Button closeOptionsButton = EnsureButton("Button_CloseOptions", panelOptions.transform, "Fechar Opções");
            Button closeLanguageButton = EnsureButton("Button_CloseLanguage", panelLanguage.transform, "Close");

            EnsureParent(playButton.gameObject, buttonsContainer.transform);
            EnsureParent(optionsButton.gameObject, buttonsContainer.transform);
            EnsureParent(languageButton.gameObject, buttonsContainer.transform);
            EnsureParent(quitButton.gameObject, buttonsContainer.transform);

            playButton.transform.SetSiblingIndex(0);
            optionsButton.transform.SetSiblingIndex(1);
            languageButton.transform.SetSiblingIndex(2);
            quitButton.transform.SetSiblingIndex(3);

            StyleMainMenuButton(playButton);
            StyleMainMenuButton(optionsButton);
            StyleMainMenuButton(languageButton);
            StyleMainMenuButton(quitButton);
            StyleMainMenuButton(closeOptionsButton);
            StyleMainMenuButton(closeLanguageButton);

            EnsureLayoutElementHeight(title, 120f);
            EnsureLayoutElementHeight(subtitle, 60f);
            EnsureLayoutElementHeight(buttonsContainer, 380f);

            title.transform.SetSiblingIndex(0);
            subtitle.transform.SetSiblingIndex(1);
            buttonsContainer.transform.SetSiblingIndex(2);

            GameObject optionsTitle = EnsureTextObject("Text_OptionsTitle", panelOptions.transform, "Opções", 38, TextAnchor.MiddleCenter);
            StyleText(optionsTitle.GetComponent<Text>(), new Color(0.92f, 0.98f, 1f, 1f), FontStyle.Bold);
            SetRect(optionsTitle, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -46f), new Vector2(540f, 56f));

            GameObject optionsTabs = EnsureChildObject("Options_Tabs", panelOptions.transform);
            AddHorizontalLayout(optionsTabs, 12f, TextAnchor.MiddleCenter, new RectOffset(0, 0, 0, 0));
            RemoveContentSizeFitter(optionsTabs);
            SetRect(optionsTabs, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -118f), new Vector2(900f, 72f));
            Button tabVideoButton = EnsureButton("Button_TabVideo", optionsTabs.transform, "Video");
            Button tabAudioButton = EnsureButton("Button_TabAudio", optionsTabs.transform, "Som");
            Button tabOtherButton = EnsureButton("Button_TabOther", optionsTabs.transform, "Outros");
            EnsureParent(tabVideoButton.gameObject, optionsTabs.transform);
            EnsureParent(tabAudioButton.gameObject, optionsTabs.transform);
            EnsureParent(tabOtherButton.gameObject, optionsTabs.transform);
            SetTabButtonSize(tabVideoButton);
            SetTabButtonSize(tabAudioButton);
            SetTabButtonSize(tabOtherButton);
            RemoveAutoLayout(optionsTabs);
            SetRect(tabVideoButton.gameObject, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0f), new Vector2(280f, 60f));
            SetRect(tabAudioButton.gameObject, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 0f), new Vector2(280f, 60f));
            SetRect(tabOtherButton.gameObject, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(0f, 0f), new Vector2(280f, 60f));
            StyleMainMenuButton(tabVideoButton);
            StyleMainMenuButton(tabAudioButton);
            StyleMainMenuButton(tabOtherButton);

            GameObject optionsContent = EnsureChildObject("Options_Content", panelOptions.transform);
            RectTransform optionsContentRect = EnsureRectTransform(optionsContent);
            optionsContentRect.anchorMin = new Vector2(0.05f, 0.14f);
            optionsContentRect.anchorMax = new Vector2(0.95f, 0.74f);
            optionsContentRect.pivot = new Vector2(0.5f, 0.5f);
            optionsContentRect.offsetMin = Vector2.zero;
            optionsContentRect.offsetMax = Vector2.zero;
            RemoveAutoLayout(optionsContent);
            EnsureComponent<Image>(optionsContent).color = new Color(0.04f, 0.11f, 0.18f, 1f);

            GameObject optionsViewport = EnsureChildObject("Viewport", optionsContent.transform);
            RectTransform optionsViewportRect = EnsureRectTransform(optionsViewport);
            StretchRect(optionsViewportRect);
            Image viewportImage = EnsureComponent<Image>(optionsViewport);
            viewportImage.color = new Color(1f, 1f, 1f, 0.01f);
            Mask viewportMask = EnsureComponent<Mask>(optionsViewport);
            viewportMask.showMaskGraphic = false;

            GameObject optionsScrollContent = EnsureChildObject("Content", optionsViewport.transform);
            RectTransform optionsScrollContentRect = EnsureRectTransform(optionsScrollContent);
            optionsScrollContentRect.anchorMin = new Vector2(0f, 1f);
            optionsScrollContentRect.anchorMax = new Vector2(1f, 1f);
            optionsScrollContentRect.pivot = new Vector2(0.5f, 1f);
            optionsScrollContentRect.anchoredPosition = Vector2.zero;
            optionsScrollContentRect.sizeDelta = new Vector2(0f, 820f);

            ScrollRect optionsScrollRect = EnsureComponent<ScrollRect>(optionsContent);
            optionsScrollRect.viewport = optionsViewportRect;
            optionsScrollRect.content = optionsScrollContentRect;
            optionsScrollRect.horizontal = false;
            optionsScrollRect.vertical = true;
            optionsScrollRect.movementType = ScrollRect.MovementType.Clamped;
            optionsScrollRect.scrollSensitivity = 30f;

            BuildOptionsSection(optionsScrollContent.transform, "Section_Video", "VIDEO", out GameObject sectionVideo, out Dropdown resolutionDropdown, out Toggle fullscreenToggle, out Toggle showFpsToggle, out Slider fpsLimitSlider, out Text fpsLimitLabel, out Dropdown qualityDropdown);
            BuildAudioSection(optionsScrollContent.transform, out GameObject sectionAudio, out Slider masterSlider, out Slider musicSlider, out Slider sfxSlider, out Toggle muteToggle);
            BuildOtherSection(optionsScrollContent.transform, out GameObject sectionOther, out Slider mouseSensitivitySlider, out Text mouseSensitivityLabel, out Dropdown languageMirrorDropdown);
            EnsureParent(sectionVideo, optionsScrollContent.transform);
            EnsureParent(sectionAudio, optionsScrollContent.transform);
            EnsureParent(sectionOther, optionsScrollContent.transform);

            sectionVideo.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            sectionAudio.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            sectionOther.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

            sectionVideo.SetActive(true);
            sectionAudio.SetActive(false);
            sectionOther.SetActive(false);

            SetRect(closeOptionsButton.gameObject, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 34f), new Vector2(320f, 64f));
            optionsTitle.transform.SetSiblingIndex(0);
            optionsContent.transform.SetSiblingIndex(1);
            closeOptionsButton.transform.SetSiblingIndex(2);
            optionsTabs.transform.SetSiblingIndex(3);

            GameObject languageTitle = EnsureTextObject("Text_LanguageTitle", panelLanguage.transform, "Idioma", 34, TextAnchor.MiddleCenter);
            StyleText(languageTitle.GetComponent<Text>(), new Color(0.92f, 0.98f, 1f, 1f), FontStyle.Bold);
            SetRect(languageTitle, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -44f), new Vector2(380f, 56f));

            GameObject languageButtons = EnsureChildObject("Language_Buttons", panelLanguage.transform);
            AddVerticalLayout(languageButtons, 14f, TextAnchor.MiddleCenter, new RectOffset(30, 30, 10, 10));
            RemoveContentSizeFitter(languageButtons);
            SetRect(languageButtons, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -8f), new Vector2(420f, 170f));
            Button ptBrButton = EnsureButton("Button_PTBR", languageButtons.transform, "PT-BR");
            Button enButton = EnsureButton("Button_EN", languageButtons.transform, "EN");
            StyleMainMenuButton(ptBrButton);
            StyleMainMenuButton(enButton);
            SetRect(closeLanguageButton.gameObject, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 30f), new Vector2(260f, 58f));
            closeLanguageButton.transform.SetAsLastSibling();

            MainMenuUI menuUi = EnsureComponent<MainMenuUI>(canvas.gameObject);
            SetSerializedReference(menuUi, "optionsPanel", panelOptions);
            SetSerializedReference(menuUi, "languagePanel", panelLanguage);

            BindButton(playButton, menuUi.OnPlayButtonPressed);
            BindButton(optionsButton, menuUi.OnOptionsButtonPressed);
            BindButton(languageButton, menuUi.OnLanguageButtonPressed);
            BindButton(quitButton, menuUi.OnQuitButtonPressed);
            BindButton(closeOptionsButton, menuUi.OnCloseOptionsButtonPressed);
            BindButton(closeLanguageButton, menuUi.OnCloseLanguageButtonPressed);

            MainMenuSettingsBinder settingsBinder = EnsureComponent<MainMenuSettingsBinder>(canvas.gameObject);
            SetSerializedReference(settingsBinder, "optionsPanel", panelOptions);
            SetSerializedReference(settingsBinder, "languagePanel", panelLanguage);
            SetSerializedReference(settingsBinder, "playButtonText", FindChildText(playButton.transform, "Text"));
            SetSerializedReference(settingsBinder, "optionsButtonText", FindChildText(optionsButton.transform, "Text"));
            SetSerializedReference(settingsBinder, "languageButtonText", FindChildText(languageButton.transform, "Text"));
            SetSerializedReference(settingsBinder, "quitButtonText", FindChildText(quitButton.transform, "Text"));
            SetSerializedReference(settingsBinder, "titleText", title.GetComponent<Text>());
            SetSerializedReference(settingsBinder, "subtitleText", subtitle.GetComponent<Text>());
            SetSerializedReference(settingsBinder, "optionsTitleText", optionsTitle.GetComponent<Text>());
            SetSerializedReference(settingsBinder, "languageTitleText", languageTitle.GetComponent<Text>());
            SetSerializedReference(settingsBinder, "languagePtBrButton", ptBrButton);
            SetSerializedReference(settingsBinder, "languageEnButton", enButton);
            SetSerializedReference(settingsBinder, "languagePtBrLabel", FindChildText(ptBrButton.transform, "Text"));
            SetSerializedReference(settingsBinder, "languageEnLabel", FindChildText(enButton.transform, "Text"));
            SetSerializedReference(settingsBinder, "resolutionDropdown", resolutionDropdown);
            SetSerializedReference(settingsBinder, "fullscreenToggle", fullscreenToggle);
            SetSerializedReference(settingsBinder, "showFpsToggle", showFpsToggle);
            SetSerializedReference(settingsBinder, "fpsLimitSlider", fpsLimitSlider);
            SetSerializedReference(settingsBinder, "fpsLimitLabel", fpsLimitLabel);
            SetSerializedReference(settingsBinder, "qualityDropdown", qualityDropdown);
            SetSerializedReference(settingsBinder, "masterVolumeSlider", masterSlider);
            SetSerializedReference(settingsBinder, "musicVolumeSlider", musicSlider);
            SetSerializedReference(settingsBinder, "sfxVolumeSlider", sfxSlider);
            SetSerializedReference(settingsBinder, "muteToggle", muteToggle);
            SetSerializedReference(settingsBinder, "mouseSensitivitySlider", mouseSensitivitySlider);
            SetSerializedReference(settingsBinder, "mouseSensitivityLabel", mouseSensitivityLabel);
            SetSerializedReference(settingsBinder, "languageMirrorDropdown", languageMirrorDropdown);
            SetSerializedReference(settingsBinder, "videoTabButton", tabVideoButton);
            SetSerializedReference(settingsBinder, "audioTabButton", tabAudioButton);
            SetSerializedReference(settingsBinder, "otherTabButton", tabOtherButton);
            SetSerializedReference(settingsBinder, "videoSection", sectionVideo);
            SetSerializedReference(settingsBinder, "audioSection", sectionAudio);
            SetSerializedReference(settingsBinder, "otherSection", sectionOther);
            SetSerializedReference(settingsBinder, "videoTabText", FindChildText(tabVideoButton.transform, "Text"));
            SetSerializedReference(settingsBinder, "audioTabText", FindChildText(tabAudioButton.transform, "Text"));
            SetSerializedReference(settingsBinder, "otherTabText", FindChildText(tabOtherButton.transform, "Text"));
            SetSerializedReference(settingsBinder, "closeOptionsButtonText", FindChildText(closeOptionsButton.transform, "Text"));
            SetSerializedReference(settingsBinder, "closeLanguageButtonText", FindChildText(closeLanguageButton.transform, "Text"));

            EnsureEventSystem();
            ValidateMainMenuState(playButton, menuUi);
            MarkSceneDirty("MainMenu UI montada/atualizada com sucesso.");
        }

        [MenuItem("Tools/Arquipelago/Create Open World UI")]
        public static void CreateOpenWorldUI()
        {
            BuildOpenWorld();
        }

        [MenuItem("Tools/Arquipelago/Build Open World")]
        public static void BuildOpenWorld()
        {
            EnsureScenesInBuildSettings();
            EnsureAppBootstrap();

            GameObject world = EnsureRootObject("World");
            TerrainGenerator terrainGenerator = EnsureComponent<TerrainGenerator>(world);
            ConfigureTerrainGenerator(terrainGenerator, world.transform);
            GenerateTerrainsNow(terrainGenerator, world.transform);

            GameObject player = EnsurePlayerForGameScene();
            TrySetupFirstPersonBridge(player);

            Canvas canvas = EnsureCanvas("Canvas_Game");
            GameObject panelHud = EnsurePanel("Panel_HUD", canvas.transform, true, stretch: true, tint: new Color(0f, 0f, 0f, 0f));
            GameObject panelPause = EnsurePanel("Panel_Pause", canvas.transform, false);
            GameObject panelInventory = EnsurePanel("Panel_Inventory", canvas.transform, false);
            GameObject panelOptions = EnsurePanel("Panel_Options", canvas.transform, false);

            AddVerticalLayout(panelPause, 12f, TextAnchor.MiddleCenter, new RectOffset(30, 30, 30, 30));
            AddVerticalLayout(panelOptions, 12f, TextAnchor.MiddleCenter, new RectOffset(30, 30, 30, 30));

            Button resumeButton = EnsureButton("Button_Resume", panelPause.transform, "Resume");
            Button optionsButton = EnsureButton("Button_Options", panelPause.transform, "Options");
            Button backToMenuButton = EnsureButton("Button_BackToMenu", panelPause.transform, "Back To Menu");
            Button closeOptionsButton = EnsureButton("Button_CloseOptions", panelOptions.transform, "Close Options");

            Text fpsText = EnsureText("Text_FPS", panelHud.transform, "FPS: 0", 16, TextAnchor.UpperLeft);
            RectTransform fpsRect = fpsText.GetComponent<RectTransform>();
            fpsRect.anchorMin = new Vector2(0f, 1f);
            fpsRect.anchorMax = new Vector2(0f, 1f);
            fpsRect.pivot = new Vector2(0f, 1f);
            fpsRect.anchoredPosition = new Vector2(15f, -15f);
            fpsRect.sizeDelta = new Vector2(180f, 30f);

            UIManager uiManager = EnsureComponent<UIManager>(canvas.gameObject);
            SetSerializedReference(uiManager, "gameplayHudPanel", panelHud);
            SetSerializedReference(uiManager, "pausePanel", panelPause);
            SetSerializedReference(uiManager, "inventoryPanel", panelInventory);
            SetSerializedReference(uiManager, "optionsPanel", panelOptions);

            FPSDisplay fpsDisplay = EnsureComponent<FPSDisplay>(fpsText.gameObject);
            SetSerializedReference(fpsDisplay, "fpsText", fpsText);

            BindButton(resumeButton, uiManager.OnResumeButtonPressed);
            BindButton(optionsButton, uiManager.OnOpenOptionsButtonPressed);
            BindButton(backToMenuButton, uiManager.OnBackToMenuButtonPressed);
            BindButton(closeOptionsButton, uiManager.OnCloseOptionsButtonPressed);

            EnsureEventSystem();
            ValidateGameSceneState(world, canvas, player);
            MarkSceneDirty("Open World montado/atualizado com sucesso.");
        }

        [MenuItem("Tools/Arquipelago/Build Game Scene")]
        public static void BuildGameSceneCompatibility()
        {
            BuildOpenWorld();
        }

        [MenuItem("Tools/Arquipelago/Setup Full Scene Architecture")]
        [MenuItem("Tools/Arquipelago/Architecture/Setup Full Scene Architecture (Alias)")]
        public static void SetupFullSceneArchitectureAlias()
        {
            SceneArchitectureBuilder.SetupFullSceneArchitecture();
        }

        private static void EnsureAppBootstrap()
        {
            GameObject appBootstrap = EnsureRootObject("AppBootstrap");
            EnsureComponent<AppManagersBootstrap>(appBootstrap);
        }

        private static Canvas EnsureCanvas(string canvasName)
        {
            GameObject canvasObject = EnsureRootObject(canvasName);
            Canvas canvas = EnsureComponent<Canvas>(canvasObject);
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = EnsureComponent<CanvasScaler>(canvasObject);
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            EnsureComponent<GraphicRaycaster>(canvasObject);
            return canvas;
        }

        private static GameObject EnsurePanel(string panelName, Transform parent, bool activeByDefault, bool stretch = false, Color? tint = null)
        {
            GameObject panel = EnsureChildObject(panelName, parent);
            RectTransform rect = EnsureRectTransform(panel);

            if (stretch)
            {
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
                rect.pivot = new Vector2(0.5f, 0.5f);
            }
            else
            {
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = Vector2.zero;
                rect.sizeDelta = new Vector2(450f, 350f);
            }

            Image image = EnsureComponent<Image>(panel);
            image.color = tint ?? new Color(0f, 0f, 0f, 0.75f);

            panel.SetActive(activeByDefault);
            return panel;
        }

        private static void SetupMainMenuBackground(Transform canvasTransform)
        {
            GameObject background = EnsurePanel("Background", canvasTransform, true, stretch: true, tint: new Color(0.03f, 0.07f, 0.12f, 1f));
            background.transform.SetSiblingIndex(0);

            GameObject vignette = EnsurePanel("Background_Vignette", canvasTransform, true, stretch: true, tint: new Color(0f, 0f, 0f, 0.35f));
            vignette.transform.SetSiblingIndex(1);

            GameObject panelMain = EnsureChildObject("Panel_Main", canvasTransform);
            panelMain.transform.SetSiblingIndex(2);

            GameObject panelOptions = EnsureChildObject("Panel_Options", canvasTransform);
            panelOptions.transform.SetSiblingIndex(3);

            GameObject panelLanguage = EnsureChildObject("Panel_Language", canvasTransform);
            panelLanguage.transform.SetSiblingIndex(4);
        }

        private static void BuildOptionsSection(
            Transform parent,
            string sectionName,
            string sectionTitle,
            out GameObject sectionObject,
            out Dropdown resolutionDropdown,
            out Toggle fullscreenToggle,
            out Toggle showFpsToggle,
            out Slider fpsLimitSlider,
            out Text fpsLimitLabel,
            out Dropdown qualityDropdown)
        {
            GameObject section = EnsureSection(parent, sectionName, sectionTitle);
            sectionObject = section;
            ConfigureScrollSectionRect(section);

            resolutionDropdown = EnsureLabeledDropdown(section.transform, "Row_Resolution", "Resolucao");
            fullscreenToggle = EnsureLabeledToggle(section.transform, "Row_Fullscreen", "Fullscreen");
            showFpsToggle = EnsureLabeledToggle(section.transform, "Row_ShowFPS", "Mostrar FPS");
            fpsLimitSlider = EnsureLabeledSlider(section.transform, "Row_FPSLimit", "Limite FPS", out fpsLimitLabel, wholeNumbers: true, 30f, 240f);
            qualityDropdown = EnsureLabeledDropdown(section.transform, "Row_Quality", "Qualidade Grafica");
        }

        private static void BuildAudioSection(
            Transform parent,
            out GameObject sectionObject,
            out Slider masterSlider,
            out Slider musicSlider,
            out Slider sfxSlider,
            out Toggle muteToggle)
        {
            GameObject section = EnsureSection(parent, "Section_Audio", "SOM");
            sectionObject = section;
            ConfigureScrollSectionRect(section);

            masterSlider = EnsureLabeledSlider(section.transform, "Row_MasterVolume", "Volume Master", out _, wholeNumbers: false, 0f, 1f);
            musicSlider = EnsureLabeledSlider(section.transform, "Row_MusicVolume", "Volume Musica", out _, wholeNumbers: false, 0f, 1f);
            sfxSlider = EnsureLabeledSlider(section.transform, "Row_SfxVolume", "Volume Efeitos", out _, wholeNumbers: false, 0f, 1f);
            muteToggle = EnsureLabeledToggle(section.transform, "Row_Mute", "Mute Geral");
        }

        private static void BuildOtherSection(
            Transform parent,
            out GameObject sectionObject,
            out Slider sensitivitySlider,
            out Text sensitivityLabel,
            out Dropdown languageMirrorDropdown)
        {
            GameObject section = EnsureSection(parent, "Section_Other", "OUTROS");
            sectionObject = section;
            ConfigureScrollSectionRect(section);

            sensitivitySlider = EnsureLabeledSlider(section.transform, "Row_Sensitivity", "Sensibilidade Mouse", out sensitivityLabel, wholeNumbers: false, 0.1f, 5f);
            languageMirrorDropdown = EnsureLabeledDropdown(section.transform, "Row_LanguageMirror", "Idioma");
        }

        private static GameObject EnsureSection(Transform parent, string sectionName, string title)
        {
            GameObject section = EnsureChildObject(sectionName, parent);
            EnsureComponent<Image>(section).color = new Color(0.05f, 0.14f, 0.22f, 1f);
            AddVerticalLayout(section, 8f, TextAnchor.UpperCenter, new RectOffset(16, 16, 12, 12));
            EnsureLayoutElementHeight(section, 160f);

            GameObject titleObj = EnsureTextObject("Text_SectionTitle", section.transform, title, 20, TextAnchor.MiddleLeft);
            StyleText(titleObj.GetComponent<Text>(), new Color(0.85f, 0.95f, 1f, 1f), FontStyle.Bold);
            EnsureLayoutElementHeight(titleObj, 34f);

            return section;
        }

        private static void ConfigureScrollSectionRect(GameObject section)
        {
            RectTransform rect = EnsureRectTransform(section);
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0f, 0f);
            rect.sizeDelta = new Vector2(0f, 700f);

            EnsureLayoutElementHeight(section, 700f);
        }

        private static Dropdown EnsureLabeledDropdown(Transform parent, string rowName, string labelText)
        {
            GameObject row = EnsureRowContainer(parent, rowName);
            EnsureRowLabel(row.transform, "Text_Label", labelText);

            GameObject dropdownGo = EnsureChildObject("Dropdown", row.transform);
            EnsureRectTransform(dropdownGo).sizeDelta = new Vector2(250f, 34f);
            EnsureComponent<Image>(dropdownGo).color = new Color(0.96f, 0.98f, 1f, 1f);
            Dropdown dropdown = EnsureComponent<Dropdown>(dropdownGo);

            Text caption = EnsureText("Label", dropdownGo.transform, "-", 16, TextAnchor.MiddleLeft);
            caption.color = new Color(0.1f, 0.14f, 0.2f, 1f);
            RectTransform captionRect = EnsureRectTransform(caption.gameObject);
            captionRect.anchorMin = new Vector2(0f, 0f);
            captionRect.anchorMax = new Vector2(1f, 1f);
            captionRect.offsetMin = new Vector2(12f, 2f);
            captionRect.offsetMax = new Vector2(-26f, -2f);

            GameObject arrowObj = EnsureTextObject("Arrow", dropdownGo.transform, "v", 18, TextAnchor.MiddleCenter);
            Text arrowText = arrowObj.GetComponent<Text>();
            arrowText.color = new Color(0.08f, 0.12f, 0.16f, 1f);
            RectTransform arrowRect = EnsureRectTransform(arrowObj);
            arrowRect.anchorMin = new Vector2(1f, 0.5f);
            arrowRect.anchorMax = new Vector2(1f, 0.5f);
            arrowRect.pivot = new Vector2(1f, 0.5f);
            arrowRect.sizeDelta = new Vector2(24f, 24f);
            arrowRect.anchoredPosition = new Vector2(-8f, 0f);

            GameObject template = EnsureChildObject("Template", dropdownGo.transform);
            template.SetActive(false);
            EnsureComponent<Image>(template).color = new Color(0.95f, 0.97f, 1f, 1f);
            ScrollRect scrollRect = EnsureComponent<ScrollRect>(template);
            RectTransform templateRect = EnsureRectTransform(template);
            templateRect.anchorMin = new Vector2(0f, 0f);
            templateRect.anchorMax = new Vector2(1f, 0f);
            templateRect.pivot = new Vector2(0.5f, 1f);
            templateRect.sizeDelta = new Vector2(0f, 160f);
            templateRect.anchoredPosition = new Vector2(0f, -38f);

            GameObject viewport = EnsureChildObject("Viewport", template.transform);
            Mask mask = EnsureComponent<Mask>(viewport);
            mask.showMaskGraphic = false;
            EnsureComponent<Image>(viewport).color = new Color(1f, 1f, 1f, 0.02f);
            RectTransform viewportRect = EnsureRectTransform(viewport);
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.offsetMin = Vector2.zero;
            viewportRect.offsetMax = Vector2.zero;

            GameObject content = EnsureChildObject("Content", viewport.transform);
            RectTransform contentRect = EnsureRectTransform(content);
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot = new Vector2(0.5f, 1f);
            contentRect.sizeDelta = new Vector2(0f, 28f);
            contentRect.anchoredPosition = Vector2.zero;

            GameObject item = EnsureChildObject("Item", content.transform);
            Toggle itemToggle = EnsureComponent<Toggle>(item);
            EnsureComponent<Image>(item).color = new Color(1f, 1f, 1f, 1f);
            RectTransform itemRect = EnsureRectTransform(item);
            itemRect.anchorMin = new Vector2(0f, 1f);
            itemRect.anchorMax = new Vector2(1f, 1f);
            itemRect.pivot = new Vector2(0.5f, 1f);
            itemRect.sizeDelta = new Vector2(0f, 28f);

            GameObject itemCheckmark = EnsureTextObject("Item Checkmark", item.transform, "*", 14, TextAnchor.MiddleCenter);
            RectTransform itemCheckmarkRect = EnsureRectTransform(itemCheckmark);
            itemCheckmarkRect.anchorMin = new Vector2(0f, 0.5f);
            itemCheckmarkRect.anchorMax = new Vector2(0f, 0.5f);
            itemCheckmarkRect.pivot = new Vector2(0f, 0.5f);
            itemCheckmarkRect.sizeDelta = new Vector2(18f, 18f);
            itemCheckmarkRect.anchoredPosition = new Vector2(8f, 0f);

            Text itemLabel = EnsureText("Item Label", item.transform, "Option", 14, TextAnchor.MiddleLeft);
            itemLabel.color = new Color(0.09f, 0.13f, 0.18f, 1f);
            RectTransform itemLabelRect = EnsureRectTransform(itemLabel.gameObject);
            itemLabelRect.anchorMin = new Vector2(0f, 0f);
            itemLabelRect.anchorMax = new Vector2(1f, 1f);
            itemLabelRect.offsetMin = new Vector2(28f, 2f);
            itemLabelRect.offsetMax = new Vector2(-6f, -2f);

            itemToggle.targetGraphic = item.GetComponent<Image>();
            itemToggle.graphic = itemCheckmark.GetComponent<Text>();

            scrollRect.content = contentRect;
            scrollRect.viewport = viewportRect;
            scrollRect.horizontal = false;
            scrollRect.vertical = true;

            dropdown.targetGraphic = dropdownGo.GetComponent<Image>();
            dropdown.captionText = caption;
            dropdown.template = templateRect;
            dropdown.itemText = itemLabel;

            if (dropdown.options.Count == 0)
            {
                dropdown.options.Add(new Dropdown.OptionData("Option"));
            }

            return dropdown;
        }

        private static Toggle EnsureLabeledToggle(Transform parent, string rowName, string labelText)
        {
            GameObject row = EnsureRowContainer(parent, rowName);
            EnsureRowLabel(row.transform, "Text_Label", labelText);

            GameObject toggleGo = EnsureChildObject("Toggle", row.transform);
            RectTransform toggleRect = EnsureRectTransform(toggleGo);
            toggleRect.sizeDelta = new Vector2(24f, 24f);

            Image bg = EnsureComponent<Image>(toggleGo);
            bg.color = new Color(0.92f, 0.96f, 1f, 1f);

            GameObject checkmarkObj = EnsureTextObject("Checkmark", toggleGo.transform, "X", 16, TextAnchor.MiddleCenter);
            Text checkmarkText = checkmarkObj.GetComponent<Text>();
            checkmarkText.color = new Color(0.05f, 0.30f, 0.42f, 1f);
            RectTransform checkRect = EnsureRectTransform(checkmarkObj);
            checkRect.anchorMin = Vector2.zero;
            checkRect.anchorMax = Vector2.one;
            checkRect.offsetMin = Vector2.zero;
            checkRect.offsetMax = Vector2.zero;

            Toggle toggle = EnsureComponent<Toggle>(toggleGo);
            toggle.targetGraphic = bg;
            toggle.graphic = checkmarkText;

            return toggle;
        }

        private static Slider EnsureLabeledSlider(Transform parent, string rowName, string labelText, out Text valueLabel, bool wholeNumbers, float min, float max)
        {
            GameObject row = EnsureRowContainer(parent, rowName);
            EnsureRowLabel(row.transform, "Text_Label", labelText);

            GameObject rightGroup = EnsureChildObject("RightGroup", row.transform);
            AddHorizontalLayout(rightGroup, 8f, TextAnchor.MiddleRight, new RectOffset(0, 0, 0, 0));
            RectTransform rightRect = EnsureRectTransform(rightGroup);
            rightRect.sizeDelta = new Vector2(320f, 34f);

            GameObject sliderGo = EnsureChildObject("Slider", rightGroup.transform);
            RectTransform sliderRect = EnsureRectTransform(sliderGo);
            sliderRect.sizeDelta = new Vector2(220f, 24f);
            Slider slider = EnsureComponent<Slider>(sliderGo);
            slider.wholeNumbers = wholeNumbers;
            slider.minValue = min;
            slider.maxValue = max;

            GameObject background = EnsureChildObject("Background", sliderGo.transform);
            EnsureRectTransform(background).sizeDelta = new Vector2(220f, 8f);
            EnsureComponent<Image>(background).color = new Color(0.74f, 0.84f, 0.90f, 1f);

            GameObject fillArea = EnsureChildObject("Fill Area", sliderGo.transform);
            RectTransform fillAreaRect = EnsureRectTransform(fillArea);
            fillAreaRect.anchorMin = new Vector2(0f, 0.5f);
            fillAreaRect.anchorMax = new Vector2(1f, 0.5f);
            fillAreaRect.offsetMin = new Vector2(8f, -4f);
            fillAreaRect.offsetMax = new Vector2(-8f, 4f);

            GameObject fill = EnsureChildObject("Fill", fillArea.transform);
            EnsureComponent<Image>(fill).color = new Color(0.08f, 0.45f, 0.63f, 1f);
            RectTransform fillRect = EnsureRectTransform(fill);
            fillRect.anchorMin = new Vector2(0f, 0f);
            fillRect.anchorMax = new Vector2(1f, 1f);
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;

            GameObject handleSlideArea = EnsureChildObject("Handle Slide Area", sliderGo.transform);
            RectTransform handleAreaRect = EnsureRectTransform(handleSlideArea);
            handleAreaRect.anchorMin = new Vector2(0f, 0f);
            handleAreaRect.anchorMax = new Vector2(1f, 1f);
            handleAreaRect.offsetMin = new Vector2(10f, 0f);
            handleAreaRect.offsetMax = new Vector2(-10f, 0f);

            GameObject handle = EnsureChildObject("Handle", handleSlideArea.transform);
            EnsureComponent<Image>(handle).color = new Color(0.94f, 0.98f, 1f, 1f);
            RectTransform handleRect = EnsureRectTransform(handle);
            handleRect.sizeDelta = new Vector2(18f, 18f);

            slider.targetGraphic = handle.GetComponent<Image>();
            slider.fillRect = fillRect;
            slider.handleRect = handleRect;
            slider.direction = Slider.Direction.LeftToRight;

            GameObject valueTextObj = EnsureTextObject("Text_Value", rightGroup.transform, "0", 14, TextAnchor.MiddleRight);
            valueLabel = valueTextObj.GetComponent<Text>();
            valueLabel.color = new Color(0.84f, 0.95f, 1f, 1f);
            RectTransform valueRect = EnsureRectTransform(valueTextObj);
            valueRect.sizeDelta = new Vector2(90f, 24f);

            return slider;
        }

        private static GameObject EnsureRowContainer(Transform parent, string rowName)
        {
            GameObject row = EnsureChildObject(rowName, parent);
            AddHorizontalLayout(row, 12f, TextAnchor.MiddleLeft, new RectOffset(4, 4, 4, 4));
            EnsureLayoutElementHeight(row, 38f);
            return row;
        }

        private static Text EnsureRowLabel(Transform parent, string objectName, string content)
        {
            GameObject labelObj = EnsureTextObject(objectName, parent, content, 16, TextAnchor.MiddleLeft);
            Text text = labelObj.GetComponent<Text>();
            text.color = new Color(0.88f, 0.95f, 1f, 1f);
            EnsureLayoutElementWidth(labelObj, 250f);
            return text;
        }

        private static void AddHorizontalLayout(GameObject target, float spacing, TextAnchor alignment, RectOffset padding)
        {
            HorizontalLayoutGroup group = EnsureComponent<HorizontalLayoutGroup>(target);
            group.childControlWidth = false;
            group.childControlHeight = false;
            group.childForceExpandWidth = false;
            group.childForceExpandHeight = false;
            group.spacing = spacing;
            group.childAlignment = alignment;
            group.padding = padding;
        }

        private static void EnsureLayoutElementWidth(GameObject target, float preferredWidth)
        {
            LayoutElement element = EnsureComponent<LayoutElement>(target);
            element.preferredWidth = preferredWidth;
            element.minWidth = preferredWidth;
        }

        private static void StretchRect(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.pivot = new Vector2(0.5f, 0.5f);
        }

        private static Text FindChildText(Transform parent, string childName)
        {
            if (parent == null)
            {
                return null;
            }

            Transform child = parent.Find(childName);
            if (child == null)
            {
                return null;
            }

            return child.GetComponent<Text>();
        }

        private static void EnsureParent(GameObject child, Transform parent)
        {
            if (child == null || parent == null)
            {
                return;
            }

            if (child.transform.parent != parent)
            {
                child.transform.SetParent(parent, false);
            }
        }

        private static void RemoveAutoLayout(GameObject target)
        {
            if (target == null)
            {
                return;
            }

            var v = target.GetComponent<VerticalLayoutGroup>();
            if (v != null)
            {
                UnityEngine.Object.DestroyImmediate(v);
            }

            var h = target.GetComponent<HorizontalLayoutGroup>();
            if (h != null)
            {
                UnityEngine.Object.DestroyImmediate(h);
            }

            RemoveContentSizeFitter(target);
        }

        private static void RemoveContentSizeFitter(GameObject target)
        {
            if (target == null)
            {
                return;
            }

            var fitter = target.GetComponent<ContentSizeFitter>();
            if (fitter != null)
            {
                UnityEngine.Object.DestroyImmediate(fitter);
            }
        }

        private static void SetRect(GameObject target, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPosition, Vector2 sizeDelta)
        {
            if (target == null)
            {
                return;
            }

            RectTransform rect = EnsureRectTransform(target);
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = sizeDelta;
        }

        private static void RemoveLegacyChild(Transform parent, string childName)
        {
            if (parent == null)
            {
                return;
            }

            Transform legacy = parent.Find(childName);
            if (legacy == null)
            {
                return;
            }

#if UNITY_EDITOR
            Undo.DestroyObjectImmediate(legacy.gameObject);
#else
            UnityEngine.Object.Destroy(legacy.gameObject);
#endif
        }

        private static Button EnsureButton(string buttonName, Transform parent, string buttonLabel)
        {
            GameObject buttonObject = EnsureChildObject(buttonName, parent);
            RectTransform rect = EnsureRectTransform(buttonObject);
            rect.sizeDelta = new Vector2(360f, 68f);

            Image image = EnsureComponent<Image>(buttonObject);
            image.color = new Color(0.07f, 0.40f, 0.55f, 0.98f);

            Button button = EnsureComponent<Button>(buttonObject);
            button.transition = Selectable.Transition.ColorTint;

            Text label = EnsureText("Text", buttonObject.transform, buttonLabel, 22, TextAnchor.MiddleCenter);
            StyleText(label, new Color(0.96f, 1f, 1f, 1f), FontStyle.Bold);
            RectTransform labelRect = label.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;

            return button;
        }

        private static GameObject EnsureTextObject(string objectName, Transform parent, string content, int fontSize, TextAnchor alignment)
        {
            GameObject textObject = EnsureChildObject(objectName, parent);
            Text text = EnsureComponent<Text>(textObject);
            text.text = content;
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = Color.white;
            text.font = GetBuiltInUiFont();
            EnsureRectTransform(textObject);
            return textObject;
        }

        private static void StyleText(Text text, Color color, FontStyle style)
        {
            if (text == null)
            {
                return;
            }

            text.color = color;
            text.fontStyle = style;
        }

        private static void StyleMainMenuButton(Button button)
        {
            if (button == null)
            {
                return;
            }

            ColorBlock colors = button.colors;
            colors.normalColor = new Color(0.07f, 0.40f, 0.55f, 0.98f);
            colors.highlightedColor = new Color(0.09f, 0.50f, 0.68f, 1f);
            colors.pressedColor = new Color(0.05f, 0.28f, 0.42f, 1f);
            colors.selectedColor = colors.highlightedColor;
            colors.disabledColor = new Color(0.20f, 0.20f, 0.20f, 0.80f);
            colors.colorMultiplier = 1f;
            colors.fadeDuration = 0.12f;
            button.colors = colors;
        }

        private static void SetTabButtonSize(Button button)
        {
            if (button == null)
            {
                return;
            }

            RectTransform rect = EnsureRectTransform(button.gameObject);
            rect.sizeDelta = new Vector2(220f, 56f);

            LayoutElement element = EnsureComponent<LayoutElement>(button.gameObject);
            element.preferredWidth = 220f;
            element.minWidth = 220f;
            element.preferredHeight = 56f;
            element.minHeight = 56f;
            element.flexibleWidth = 0f;
            element.flexibleHeight = 0f;
        }

        private static void EnsureLayoutElementHeight(GameObject target, float preferredHeight)
        {
            LayoutElement element = EnsureComponent<LayoutElement>(target);
            element.preferredHeight = preferredHeight;
            element.minHeight = preferredHeight;
        }

        private static Text EnsureText(string textName, Transform parent, string content, int size, TextAnchor alignment)
        {
            GameObject textObject = EnsureChildObject(textName, parent);
            Text text = EnsureComponent<Text>(textObject);
            text.text = content;
            text.fontSize = size;
            text.alignment = alignment;
            text.color = Color.white;
            text.font = GetBuiltInUiFont();

            EnsureRectTransform(textObject);
            return text;
        }

        private static Font GetBuiltInUiFont()
        {
            try
            {
                return Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            }
            catch
            {
                return Resources.GetBuiltinResource<Font>("Arial.ttf");
            }
        }

        private static void AddVerticalLayout(GameObject target, float spacing, TextAnchor alignment, RectOffset padding)
        {
            VerticalLayoutGroup group = EnsureComponent<VerticalLayoutGroup>(target);
            group.childControlWidth = true;
            group.childControlHeight = false;
            group.childForceExpandWidth = true;
            group.childForceExpandHeight = false;
            group.spacing = spacing;
            group.childAlignment = alignment;
            group.padding = padding;

            ContentSizeFitter fitter = EnsureComponent<ContentSizeFitter>(target);
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }

        private static void EnsureEventSystem()
        {
            EventSystem existing = UnityEngine.Object.FindFirstObjectByType<EventSystem>();
            if (existing != null)
            {
                return;
            }

            GameObject eventSystemObject = new GameObject("EventSystem", typeof(EventSystem));

            Type inputSystemUiType = Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
            if (inputSystemUiType != null)
            {
                eventSystemObject.AddComponent(inputSystemUiType);
            }
            else
            {
                eventSystemObject.AddComponent<StandaloneInputModule>();
            }

            Undo.RegisterCreatedObjectUndo(eventSystemObject, "Create EventSystem");
        }

        private static GameObject EnsurePlayerForGameScene()
        {
            GameObject namedPlayer = FindRootObjectByName("Player");
            if (namedPlayer != null)
            {
                EnsureCharacterController(namedPlayer);
                return namedPlayer;
            }

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(FixedPlayerPrefabPath);
            if (prefab == null)
            {
                Debug.LogError($"Prefab de player nao encontrado no caminho fixo: {FixedPlayerPrefabPath}");
                return null;
            }

            GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            if (instance != null)
            {
                Undo.RegisterCreatedObjectUndo(instance, "Instantiate Player");
                instance.name = "Player";
                instance.transform.position = new Vector3(0f, 5f, 0f);
                instance.transform.rotation = Quaternion.identity;
                EnsureCharacterController(instance);
                EditorUtility.SetDirty(instance);
                Debug.Log($"Player instanciado usando prefab fixo: {FixedPlayerPrefabPath}");
                return instance;
            }

            Debug.LogError("Falha ao instanciar o prefab de player fixo.");
            return null;
        }

        private static void EnsureCharacterController(GameObject player)
        {
            if (player == null)
            {
                return;
            }

            CharacterController controller = player.GetComponent<CharacterController>();
            if (controller == null)
            {
                Undo.AddComponent<CharacterController>(player);
                EditorUtility.SetDirty(player);
            }
        }

        private static void ConfigureTerrainGenerator(TerrainGenerator generator, Transform worldTransform)
        {
            SetSerializedInt(generator, "terrainWidth", TerrainWidth);
            SetSerializedInt(generator, "terrainLength", TerrainLength);
            SetSerializedBool(generator, "generateOnStart", true);
            SetSerializedReference(generator, "terrainParent", worldTransform);
        }

        private static void GenerateTerrainsNow(TerrainGenerator generator, Transform worldTransform)
        {
            generator.GenerateTerrainLayout();

            int count = 0;
            for (int i = 0; i < worldTransform.childCount; i++)
            {
                Transform child = worldTransform.GetChild(i);
                Terrain terrain = child.GetComponent<Terrain>();
                if (terrain == null)
                {
                    continue;
                }

                count++;
                Vector3 size = terrain.terrainData.size;
                if (Mathf.RoundToInt(size.x) != TerrainWidth || Mathf.RoundToInt(size.z) != TerrainLength)
                {
                    Debug.LogWarning($"Terrain '{child.name}' com tamanho inesperado: {size.x}x{size.z}");
                }
            }

            if (count != TerrainCountExpected)
            {
                Debug.LogWarning($"Quantidade de terrains diferente do esperado. Esperado: {TerrainCountExpected}, Atual: {count}");
            }
        }

        private static void EnsureScenesInBuildSettings()
        {
            string persistentPath = FindScenePathByName(SceneNames.PersistentSystems);
            string mainMenuPath = FindScenePathByName(SceneNames.MainMenu);
            string tutorialPath = FindScenePathByName(SceneNames.Tutorial);
            string openWorldPath = FindScenePathByName(SceneNames.OpenWorld);
            string legacyGamePath = FindScenePathByName(SceneNames.LegacyGame);

            if (string.IsNullOrEmpty(mainMenuPath))
            {
                Debug.LogWarning("Nao foi possivel localizar MainMenu em Assets. Salve as cenas para auto registrar no Build Settings.");
                return;
            }

            var existing = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            bool changed = false;

            if (!string.IsNullOrEmpty(persistentPath))
            {
                changed |= AddSceneIfMissing(existing, persistentPath);
            }

            changed |= AddSceneIfMissing(existing, mainMenuPath);

            if (!string.IsNullOrEmpty(tutorialPath))
            {
                changed |= AddSceneIfMissing(existing, tutorialPath);
            }

            if (!string.IsNullOrEmpty(openWorldPath))
            {
                changed |= AddSceneIfMissing(existing, openWorldPath);
            }
            else if (!string.IsNullOrEmpty(legacyGamePath))
            {
                changed |= AddSceneIfMissing(existing, legacyGamePath);
            }

            if (changed)
            {
                EditorBuildSettings.scenes = existing.ToArray();
                Debug.Log("Cenas base da arquitetura adicionadas ao Build Settings automaticamente.");
            }
        }

        private static string FindScenePathByName(string sceneName)
        {
            string[] guids = AssetDatabase.FindAssets($"t:Scene {sceneName}");
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                string fileName = System.IO.Path.GetFileNameWithoutExtension(path);
                if (string.Equals(fileName, sceneName, StringComparison.Ordinal))
                {
                    return path;
                }
            }

            return string.Empty;
        }

        private static bool AddSceneIfMissing(System.Collections.Generic.List<EditorBuildSettingsScene> scenes, string scenePath)
        {
            for (int i = 0; i < scenes.Count; i++)
            {
                if (string.Equals(scenes[i].path, scenePath, StringComparison.OrdinalIgnoreCase))
                {
                    if (!scenes[i].enabled)
                    {
                        scenes[i] = new EditorBuildSettingsScene(scenePath, true);
                        return true;
                    }

                    return false;
                }
            }

            scenes.Add(new EditorBuildSettingsScene(scenePath, true));
            return true;
        }

        private static void TrySetupFirstPersonBridge(GameObject playerCandidate)
        {
            if (playerCandidate == null)
            {
                return;
            }

            PlayerControllerBridge bridge = playerCandidate.GetComponent<PlayerControllerBridge>();
            if (bridge == null)
            {
                bridge = Undo.AddComponent<PlayerControllerBridge>(playerCandidate);
                EditorUtility.SetDirty(playerCandidate);
            }

            var playerInput = playerCandidate.GetComponentInChildren<UnityEngine.InputSystem.PlayerInput>(true);
            if (playerInput != null)
            {
                SetSerializedReference(bridge, "playerInput", playerInput);
                SetSerializedString(bridge, "gameplayActionMap", "Player");
                SetSerializedString(bridge, "uiActionMap", "UI");
            }
            else
            {
                Debug.LogWarning("PlayerInput nao encontrado no Player. O bridge foi adicionado, mas voce pode precisar configurar esse campo manualmente.");
            }

            Debug.Log($"PlayerControllerBridge pronto em '{playerCandidate.name}' para Starter Assets First Person.");
        }

        private static void ValidateGameSceneState(GameObject world, Canvas canvas, GameObject player)
        {
            if (world == null)
            {
                Debug.LogError("Objeto World nao encontrado apos a montagem.");
            }

            if (canvas == null)
            {
                Debug.LogError("Canvas_Game nao encontrado apos a montagem.");
            }

            if (player == null)
            {
                Debug.LogWarning("Player nao foi configurado automaticamente.");
            }

            Terrain[] terrains = world != null ? world.GetComponentsInChildren<Terrain>(true) : Array.Empty<Terrain>();
            if (terrains.Length != TerrainCountExpected)
            {
                Debug.LogWarning($"Validacao: quantidade de terrains esperada {TerrainCountExpected}, atual {terrains.Length}.");
            }

            for (int i = 0; i < terrains.Length; i++)
            {
                Vector3 size = terrains[i].terrainData.size;
                if (Mathf.RoundToInt(size.x) != TerrainWidth || Mathf.RoundToInt(size.z) != TerrainLength)
                {
                    Debug.LogWarning($"Validacao: {terrains[i].name} fora de 500x500 (atual: {size.x}x{size.z}).");
                }
            }
        }

        private static void SetSerializedReference(UnityEngine.Object target, string propertyName, UnityEngine.Object value)
        {
            SerializedObject serializedObject = new SerializedObject(target);
            SerializedProperty property = serializedObject.FindProperty(propertyName);

            if (property == null)
            {
                Debug.LogWarning($"Propriedade '{propertyName}' nao encontrada em {target.GetType().Name}.");
                return;
            }

            property.objectReferenceValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }

        private static void SetSerializedInt(UnityEngine.Object target, string propertyName, int value)
        {
            SerializedObject serializedObject = new SerializedObject(target);
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null)
            {
                Debug.LogWarning($"Propriedade '{propertyName}' nao encontrada em {target.GetType().Name}.");
                return;
            }

            property.intValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }

        private static void SetSerializedBool(UnityEngine.Object target, string propertyName, bool value)
        {
            SerializedObject serializedObject = new SerializedObject(target);
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null)
            {
                Debug.LogWarning($"Propriedade '{propertyName}' nao encontrada em {target.GetType().Name}.");
                return;
            }

            property.boolValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }

        private static void SetSerializedString(UnityEngine.Object target, string propertyName, string value)
        {
            SerializedObject serializedObject = new SerializedObject(target);
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null)
            {
                Debug.LogWarning($"Propriedade '{propertyName}' nao encontrada em {target.GetType().Name}.");
                return;
            }

            property.stringValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }

        private static void BindButton(Button button, UnityEngine.Events.UnityAction action)
        {
            Undo.RecordObject(button, "Bind Button");
            for (int i = button.onClick.GetPersistentEventCount() - 1; i >= 0; i--)
            {
                UnityEventTools.RemovePersistentListener(button.onClick, i);
            }

            UnityEventTools.AddPersistentListener(button.onClick, action);
            EditorUtility.SetDirty(button);
        }

        private static T EnsureComponent<T>(GameObject target) where T : Component
        {
            T component = target.GetComponent<T>();
            if (component != null)
            {
                return component;
            }

            component = Undo.AddComponent<T>(target);
            EditorUtility.SetDirty(target);
            return component;
        }

        private static RectTransform EnsureRectTransform(GameObject target)
        {
            RectTransform rect = target.GetComponent<RectTransform>();
            if (rect != null)
            {
                return rect;
            }

            return Undo.AddComponent<RectTransform>(target);
        }

        private static GameObject EnsureRootObject(string objectName)
        {
            Scene scene = SceneManager.GetActiveScene();
            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                if (roots[i].name == objectName)
                {
                    return roots[i];
                }
            }

            GameObject created = new GameObject(objectName);
            Undo.RegisterCreatedObjectUndo(created, "Create Root Object");
            EditorUtility.SetDirty(created);
            return created;
        }

        private static GameObject FindRootObjectByName(string objectName)
        {
            Scene scene = SceneManager.GetActiveScene();
            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                if (string.Equals(roots[i].name, objectName, StringComparison.Ordinal))
                {
                    return roots[i];
                }
            }

            return null;
        }

        private static GameObject EnsureChildObject(string objectName, Transform parent)
        {
            Transform existing = parent.Find(objectName);
            if (existing != null)
            {
                return existing.gameObject;
            }

            GameObject created = new GameObject(objectName, typeof(RectTransform));
            Undo.RegisterCreatedObjectUndo(created, "Create Child Object");
            created.transform.SetParent(parent, false);
            EditorUtility.SetDirty(created);
            return created;
        }

        private static void MarkSceneDirty(string message)
        {
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log(message);
        }

        private static void ValidateMainMenuState(Button playButton, MainMenuUI menuUi)
        {
            if (menuUi == null)
            {
                Debug.LogError("MainMenuUI nao encontrado no Canvas_MainMenu.");
                return;
            }

            if (playButton == null)
            {
                Debug.LogError("Button_Play nao encontrado no MainMenu.");
                return;
            }

            if (playButton.onClick.GetPersistentEventCount() == 0)
            {
                Debug.LogError("Button_Play esta sem listener persistente.");
                return;
            }

            bool hasPlayBinding = false;
            for (int i = 0; i < playButton.onClick.GetPersistentEventCount(); i++)
            {
                string methodName = playButton.onClick.GetPersistentMethodName(i);
                UnityEngine.Object target = playButton.onClick.GetPersistentTarget(i);
                if (target == menuUi && string.Equals(methodName, nameof(MainMenuUI.OnPlayButtonPressed), StringComparison.Ordinal))
                {
                    hasPlayBinding = true;
                    break;
                }
            }

            if (!hasPlayBinding)
            {
                Debug.LogError("Button_Play nao esta ligado ao metodo MainMenuUI.OnPlayButtonPressed.");
            }
        }
    }
}
#endif

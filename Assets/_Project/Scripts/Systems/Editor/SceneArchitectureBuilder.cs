#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using ArquipelagoPerdidoRPG.Core;
using ArquipelagoPerdidoRPG.Inventory;
using ArquipelagoPerdidoRPG.Player;
using ArquipelagoPerdidoRPG.Settings;
using ArquipelagoPerdidoRPG.Systems;
using ArquipelagoPerdidoRPG.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ArquipelagoPerdidoRPG.Editor
{
    public static class SceneArchitectureBuilder
    {
        private const string ScenesRoot = "Assets/_Project/Scenes";
        private const string PlayerPrefabPath = "Assets/StarterAssets/FirstPersonController/Prefabs/NestedParent_Unpack.prefab";

        [MenuItem("Tools/Arquipelago/Architecture/Setup Full Scene Architecture")]
        public static void SetupFullSceneArchitecture()
        {
            EnsureScenesFolder();
            TryRenameLegacyGameToOpenWorld();

            CreatePersistentSystemsScene();
            CreateOrOpenMainMenuScene();
            CreateTutorialScene();
            CreateOpenWorldScene();
            CreateEmptyScene("Dungeon_01");
            CreateEmptyScene("Dungeon_02");
            CreateEmptyScene("BossRoom_01");
            CreateEmptyScene("Interior_01");
            CreateEmptyScene("Interior_02");

            ApplyBuildSettingsOrder();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("Arquitetura de cenas configurada com sucesso.");
        }

        [MenuItem("Tools/Arquipelago/Architecture/Build Open World Scene")]
        public static void CreateOpenWorldScene()
        {
            string openWorldPath = $"{ScenesRoot}/OpenWorld.unity";

            if (!System.IO.File.Exists(openWorldPath))
            {
                Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                EditorSceneManager.SaveScene(newScene, openWorldPath);
            }

            EditorSceneManager.OpenScene(openWorldPath, OpenSceneMode.Single);
            SceneAutoBuilder.BuildOpenWorld();
            EditorSceneManager.SaveOpenScenes();
        }

        [MenuItem("Tools/Arquipelago/Build Tutorial Scene")]
        public static void BuildTutorialSceneOnly()
        {
            EnsureScenesFolder();
            CreateTutorialScene();
            EnsureSceneInBuildSettings($"{ScenesRoot}/Tutorial.unity");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Cena Tutorial criada/atualizada com sucesso.");
        }

        [MenuItem("Tools/Arquipelago/Cleanup/Remove Extra Cameras In All Scenes")]
        public static void RemoveExtraCamerasInAllScenes()
        {
            EnsureScenesFolder();

            string[] scenePaths = Directory.GetFiles(ScenesRoot, "*.unity", SearchOption.TopDirectoryOnly);
            int scenesUpdated = 0;
            int camerasRemoved = 0;
            int listenersRemoved = 0;

            for (int i = 0; i < scenePaths.Length; i++)
            {
                string scenePath = scenePaths[i].Replace('\\', '/');
                Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

                int removedInScene = RemoveExtraCamerasInActiveScene();
                int removedListenersInScene = RemoveExtraAudioListenersInActiveScene();

                if (removedInScene > 0 || removedListenersInScene > 0)
                {
                    camerasRemoved += removedInScene;
                    listenersRemoved += removedListenersInScene;
                    scenesUpdated++;
                    EditorSceneManager.MarkSceneDirty(scene);
                    EditorSceneManager.SaveOpenScenes();
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"Limpeza concluida. Cenas atualizadas: {scenesUpdated}, Cameras removidas: {camerasRemoved}, AudioListeners removidos: {listenersRemoved}.");
        }

        [MenuItem("Tools/Arquipelago/Player/Setup Player In Gameplay Scenes")]
        public static void SetupPlayerInGameplayScenes()
        {
            EnsureScenesFolder();

            string[] gameplayScenePaths =
            {
                $"{ScenesRoot}/Tutorial.unity",
                $"{ScenesRoot}/OpenWorld.unity",
                $"{ScenesRoot}/Dungeon_01.unity",
                $"{ScenesRoot}/Dungeon_02.unity",
                $"{ScenesRoot}/BossRoom_01.unity",
                $"{ScenesRoot}/Interior_01.unity",
                $"{ScenesRoot}/Interior_02.unity"
            };

            int updatedScenes = 0;
            for (int i = 0; i < gameplayScenePaths.Length; i++)
            {
                string scenePath = gameplayScenePaths[i];
                if (!System.IO.File.Exists(scenePath))
                {
                    continue;
                }

                Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                bool changed = EnsurePlayerSetupForScene(scenePath);
                if (changed)
                {
                    EditorSceneManager.MarkSceneDirty(scene);
                    EditorSceneManager.SaveOpenScenes();
                    updatedScenes++;
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"Player setup aplicado em {updatedScenes} cena(s) de gameplay.");
        }

        private static void CreatePersistentSystemsScene()
        {
            string path = $"{ScenesRoot}/PersistentSystems.unity";
            Scene scene;
            if (System.IO.File.Exists(path))
            {
                scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            }
            else
            {
                scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                EditorSceneManager.SaveScene(scene, path);
                scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            }

            GameObject root = FindOrCreateRoot("PersistentSystemsRoot");
            EnsureManagerObject<GameManager>(root.transform, "GameManager");
            EnsureManagerObject<SettingsManager>(root.transform, "SettingsManager");
            EnsureManagerObject<SceneLoader>(root.transform, "SceneLoader");
            EnsureManagerObject<AudioManager>(root.transform, "AudioManager");
            EnsureManagerObject<InventoryManager>(root.transform, "InventoryManager");
            EnsureManagerObject<SaveManager>(root.transform, "SaveManager");
            EnsureManagerObject<LanguageManager>(root.transform, "LanguageManager");
            EnsurePersistentInventoryUI(root.transform);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveOpenScenes();
        }

        private static void EnsurePersistentInventoryUI(Transform parent)
        {
            GameObject canvasObj = GameObject.Find("Canvas_Inventory");
            if (canvasObj == null)
            {
                canvasObj = new GameObject("Canvas_Inventory", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                canvasObj.transform.SetParent(parent, false);
            }

            Canvas canvas = canvasObj.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasObj.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            if (canvasObj.GetComponent<InventoryUI>() == null)
            {
                canvasObj.AddComponent<InventoryUI>();
            }
        }

        private static void CreateOrOpenMainMenuScene()
        {
            string projectMainMenuPath = $"{ScenesRoot}/MainMenu.unity";
            if (System.IO.File.Exists(projectMainMenuPath))
            {
                EditorSceneManager.OpenScene(projectMainMenuPath, OpenSceneMode.Single);
                SceneAutoBuilder.CreateMainMenuUI();
                EditorSceneManager.SaveOpenScenes();
                return;
            }

            string existingMainMenuPath = FindScenePathByName(SceneNames.MainMenu);
            if (!string.IsNullOrEmpty(existingMainMenuPath))
            {
                EditorSceneManager.OpenScene(existingMainMenuPath, OpenSceneMode.Single);
                SceneAutoBuilder.CreateMainMenuUI();
                EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), projectMainMenuPath);
                EditorSceneManager.OpenScene(projectMainMenuPath, OpenSceneMode.Single);
                SceneAutoBuilder.CreateMainMenuUI();
                EditorSceneManager.SaveOpenScenes();
                return;
            }

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            EditorSceneManager.SaveScene(scene, projectMainMenuPath);
            EditorSceneManager.OpenScene(projectMainMenuPath, OpenSceneMode.Single);
            SceneAutoBuilder.CreateMainMenuUI();
            EditorSceneManager.SaveOpenScenes();
        }

        private static void CreateTutorialScene()
        {
            string path = $"{ScenesRoot}/Tutorial.unity";
            Scene scene;

            if (System.IO.File.Exists(path))
            {
                scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            }
            else
            {
                scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
                EditorSceneManager.SaveScene(scene, path);
                scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            }

            GameObject tutorialRoot = FindOrCreateRoot("Tutorial");
            EnsureTutorialGround(tutorialRoot.transform);
            EnsureTutorialDirectionalLight();
            EnsureTutorialPlayer();
            EnsureTutorialUI();
            EnsureEventSystem();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveOpenScenes();
        }

        private static bool EnsurePlayerSetupForScene(string scenePath)
        {
            bool changed = false;
            string sceneFileName = Path.GetFileNameWithoutExtension(scenePath);

            GameObject player = GameObject.Find("Player");
            if (sceneFileName == SceneNames.Tutorial)
            {
                GameObject previousPlayer = player;
                EnsureTutorialPlayer();
                player = GameObject.Find("Player");
                if (player != previousPlayer)
                {
                    changed = true;
                }
            }

            if (player == null)
            {
                Debug.LogWarning($"Setup Player: nenhum objeto 'Player' encontrado em '{sceneFileName}'.");
                return changed;
            }

            int componentCountBefore = player.GetComponents<PlayerControllerBridge>().Length;
            EnsureFirstPersonBridge(player);
            int componentCountAfter = player.GetComponents<PlayerControllerBridge>().Length;
            if (componentCountAfter > componentCountBefore)
            {
                changed = true;
            }

            return changed;
        }

        private static void CreateEmptyScene(string sceneName)
        {
            string path = $"{ScenesRoot}/{sceneName}.unity";
            if (System.IO.File.Exists(path))
            {
                return;
            }

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            new GameObject(sceneName);
            EditorSceneManager.SaveScene(scene, path);
        }

        private static void EnsureTutorialGround(Transform parent)
        {
            Transform legacyPlane = parent.Find("TutorialGround");
            if (legacyPlane != null)
            {
                UnityEngine.Object.DestroyImmediate(legacyPlane.gameObject);
            }

            Transform existingTerrain = parent.Find("TutorialTerrain");
            GameObject terrainObject;
            if (existingTerrain != null)
            {
                terrainObject = existingTerrain.gameObject;
            }
            else
            {
                TerrainData terrainData = new TerrainData
                {
                    heightmapResolution = 129,
                    size = new Vector3(180f, 20f, 180f)
                };

                terrainObject = Terrain.CreateTerrainGameObject(terrainData);
                terrainObject.name = "TutorialTerrain";
            }

            terrainObject.transform.SetParent(parent, true);
            terrainObject.transform.position = new Vector3(-90f, 0f, -90f);
        }

        private static void EnsureTutorialDirectionalLight()
        {
            Light directional = UnityEngine.Object.FindFirstObjectByType<Light>();
            if (directional == null)
            {
                GameObject lightObj = new GameObject("Directional Light");
                directional = lightObj.AddComponent<Light>();
            }

            directional.type = LightType.Directional;
            directional.intensity = 1.15f;
            directional.color = Color.white;
            directional.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        }

        private static void EnsureTutorialPlayer()
        {
            GameObject existingPlayer = GameObject.Find("Player");
            if (existingPlayer != null)
            {
                bool hasStarterController = existingPlayer.GetComponentInChildren<StarterAssets.FirstPersonController>(true) != null;
                bool hasStarterInputs = existingPlayer.GetComponentInChildren<StarterAssets.StarterAssetsInputs>(true) != null;

                if (!hasStarterController || !hasStarterInputs)
                {
                    UnityEngine.Object.DestroyImmediate(existingPlayer);
                    existingPlayer = null;
                }
            }

            if (existingPlayer != null)
            {
                existingPlayer.transform.position = new Vector3(0f, 5f, 0f);
                existingPlayer.transform.rotation = Quaternion.identity;
                EnsureFirstPersonBridge(existingPlayer);
                return;
            }

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
            if (prefab == null)
            {
                Debug.LogWarning($"Tutorial: prefab de player nao encontrado em {PlayerPrefabPath}");
                return;
            }

            GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            if (instance == null)
            {
                return;
            }

            instance.name = "Player";
            instance.transform.position = new Vector3(0f, 5f, 0f);
            instance.transform.rotation = Quaternion.identity;

            EnsureFirstPersonBridge(instance);
        }

        private static void EnsureTutorialUI()
        {
            GameObject canvasObj = GameObject.Find("Canvas_Tutorial");
            if (canvasObj == null)
            {
                canvasObj = new GameObject("Canvas_Tutorial", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            }

            Canvas canvas = canvasObj.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasObj.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            GameObject titleObj = FindOrCreateUiChild(canvas.transform, "Text_TutorialTitle");
            Text title = titleObj.GetComponent<Text>();
            if (title == null)
            {
                title = titleObj.AddComponent<Text>();
            }

            title.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            title.text = "Tutorial";
            title.alignment = TextAnchor.UpperCenter;
            title.fontSize = 58;
            title.color = new Color(0.95f, 0.99f, 1f, 1f);

            RectTransform titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 1f);
            titleRect.anchorMax = new Vector2(0.5f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.anchoredPosition = new Vector2(0f, -18f);
            titleRect.sizeDelta = new Vector2(620f, 90f);

            GameObject panel = FindOrCreateUiChild(canvas.transform, "Panel_TutorialInfo");
            Image panelImage = panel.GetComponent<Image>();
            if (panelImage == null)
            {
                panelImage = panel.AddComponent<Image>();
            }

            panelImage.color = new Color(0.02f, 0.08f, 0.13f, 0.82f);
            RectTransform panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 1f);
            panelRect.anchorMax = new Vector2(0.5f, 1f);
            panelRect.pivot = new Vector2(0.5f, 1f);
            panelRect.anchoredPosition = new Vector2(0f, -112f);
            panelRect.sizeDelta = new Vector2(980f, 290f);

            GameObject infoTextObj = FindOrCreateUiChild(panel.transform, "Text_TutorialInfo");
            Text infoText = infoTextObj.GetComponent<Text>();
            if (infoText == null)
            {
                infoText = infoTextObj.AddComponent<Text>();
            }

            infoText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            infoText.color = Color.white;
            infoText.alignment = TextAnchor.UpperLeft;
            infoText.fontSize = 28;
            infoText.text = "Objetivo 1/5: [WASD]\nMova o personagem usando WASD.";

            RectTransform infoRect = infoTextObj.GetComponent<RectTransform>();
            infoRect.anchorMin = new Vector2(0f, 0.36f);
            infoRect.anchorMax = new Vector2(1f, 1f);
            infoRect.offsetMin = new Vector2(20f, 8f);
            infoRect.offsetMax = new Vector2(-20f, -18f);

            GameObject progressTextObj = FindOrCreateUiChild(panel.transform, "Text_TutorialProgress");
            Text progressText = progressTextObj.GetComponent<Text>();
            if (progressText == null)
            {
                progressText = progressTextObj.AddComponent<Text>();
            }

            progressText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            progressText.color = new Color(0.85f, 0.95f, 1f, 1f);
            progressText.alignment = TextAnchor.MiddleLeft;
            progressText.fontSize = 22;
            progressText.text = "Progresso: 0/5";

            RectTransform progressRect = progressTextObj.GetComponent<RectTransform>();
            progressRect.anchorMin = new Vector2(0f, 0.18f);
            progressRect.anchorMax = new Vector2(1f, 0.34f);
            progressRect.offsetMin = new Vector2(20f, 0f);
            progressRect.offsetMax = new Vector2(-20f, 0f);

            GameObject statusTextObj = FindOrCreateUiChild(panel.transform, "Text_TutorialStatus");
            Text statusText = statusTextObj.GetComponent<Text>();
            if (statusText == null)
            {
                statusText = statusTextObj.AddComponent<Text>();
            }

            statusText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            statusText.color = new Color(0.78f, 0.90f, 0.98f, 1f);
            statusText.alignment = TextAnchor.MiddleLeft;
            statusText.fontSize = 20;
            statusText.text = "Complete a etapa atual para avancar.";

            RectTransform statusRect = statusTextObj.GetComponent<RectTransform>();
            statusRect.anchorMin = new Vector2(0f, 0f);
            statusRect.anchorMax = new Vector2(1f, 0.16f);
            statusRect.offsetMin = new Vector2(20f, 8f);
            statusRect.offsetMax = new Vector2(-20f, -4f);

            GameObject buttonObj = FindOrCreateUiChild(canvas.transform, "Button_Continue");
            Image buttonImage = buttonObj.GetComponent<Image>();
            if (buttonImage == null)
            {
                buttonImage = buttonObj.AddComponent<Image>();
            }

            buttonImage.color = new Color(0.07f, 0.40f, 0.55f, 0.95f);
            Button button = buttonObj.GetComponent<Button>();
            if (button == null)
            {
                button = buttonObj.AddComponent<Button>();
            }

            button.interactable = false;

            RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(1f, 0f);
            buttonRect.anchorMax = new Vector2(1f, 0f);
            buttonRect.pivot = new Vector2(1f, 0f);
            buttonRect.anchoredPosition = new Vector2(-24f, 24f);
            buttonRect.sizeDelta = new Vector2(260f, 64f);

            GameObject buttonTextObj = FindOrCreateUiChild(buttonObj.transform, "Text");
            Text buttonText = buttonTextObj.GetComponent<Text>();
            if (buttonText == null)
            {
                buttonText = buttonTextObj.AddComponent<Text>();
            }

            buttonText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            buttonText.text = "Continuar";
            buttonText.alignment = TextAnchor.MiddleCenter;
            buttonText.fontSize = 24;
            buttonText.color = Color.white;

            RectTransform buttonTextRect = buttonTextObj.GetComponent<RectTransform>();
            buttonTextRect.anchorMin = Vector2.zero;
            buttonTextRect.anchorMax = Vector2.one;
            buttonTextRect.offsetMin = Vector2.zero;
            buttonTextRect.offsetMax = Vector2.zero;

            TutorialUIController controller = canvasObj.GetComponent<TutorialUIController>();
            if (controller == null)
            {
                controller = canvasObj.AddComponent<TutorialUIController>();
            }

            TutorialManager tutorialManager = canvasObj.GetComponent<TutorialManager>();
            if (tutorialManager == null)
            {
                tutorialManager = canvasObj.AddComponent<TutorialManager>();
            }

            InventoryUI[] tutorialInventoryUis = canvasObj.GetComponents<InventoryUI>();
            for (int i = 0; i < tutorialInventoryUis.Length; i++)
            {
                UnityEngine.Object.DestroyImmediate(tutorialInventoryUis[i]);
            }

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(controller.ContinueToOpenWorld);
        }

        private static void EnsureFirstPersonBridge(GameObject player)
        {
            if (player == null)
            {
                return;
            }

            PlayerControllerBridge bridge = player.GetComponent<PlayerControllerBridge>();
            if (bridge == null)
            {
                player.AddComponent<PlayerControllerBridge>();
            }
        }

        private static void EnsureEventSystem()
        {
            if (UnityEngine.Object.FindFirstObjectByType<EventSystem>() != null)
            {
                return;
            }

            GameObject eventSystemObj = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            eventSystemObj.hideFlags = HideFlags.None;
        }

        private static int RemoveExtraCamerasInActiveScene()
        {
            Camera[] cameras = UnityEngine.Object.FindObjectsByType<Camera>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            if (cameras == null || cameras.Length <= 1)
            {
                return 0;
            }

            Camera keepCamera = ChooseCameraToKeep(cameras);
            int removed = 0;

            for (int i = 0; i < cameras.Length; i++)
            {
                Camera camera = cameras[i];
                if (camera == null || camera == keepCamera)
                {
                    continue;
                }

                UnityEngine.Object.DestroyImmediate(camera);
                removed++;
            }

            if (keepCamera != null)
            {
                keepCamera.tag = "MainCamera";
            }

            return removed;
        }

        private static int RemoveExtraAudioListenersInActiveScene()
        {
            AudioListener[] listeners = UnityEngine.Object.FindObjectsByType<AudioListener>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            if (listeners == null || listeners.Length <= 1)
            {
                return 0;
            }

            AudioListener keepListener = null;
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                keepListener = mainCamera.GetComponent<AudioListener>();
            }

            if (keepListener == null)
            {
                keepListener = listeners[0];
            }

            int removed = 0;
            for (int i = 0; i < listeners.Length; i++)
            {
                AudioListener listener = listeners[i];
                if (listener == null || listener == keepListener)
                {
                    continue;
                }

                UnityEngine.Object.DestroyImmediate(listener);
                removed++;
            }

            return removed;
        }

        private static Camera ChooseCameraToKeep(Camera[] cameras)
        {
            for (int i = 0; i < cameras.Length; i++)
            {
                Camera camera = cameras[i];
                if (camera != null && camera.CompareTag("MainCamera") && camera.gameObject.activeInHierarchy && camera.enabled)
                {
                    return camera;
                }
            }

            for (int i = 0; i < cameras.Length; i++)
            {
                Camera camera = cameras[i];
                if (camera == null)
                {
                    continue;
                }

                Transform t = camera.transform;
                while (t != null)
                {
                    if (string.Equals(t.name, "Player", System.StringComparison.OrdinalIgnoreCase))
                    {
                        return camera;
                    }

                    t = t.parent;
                }
            }

            for (int i = 0; i < cameras.Length; i++)
            {
                Camera camera = cameras[i];
                if (camera != null && camera.gameObject.activeInHierarchy && camera.enabled)
                {
                    return camera;
                }
            }

            return cameras[0];
        }

        private static void AddManagerObject<T>(Transform parent, string objectName) where T : Component
        {
            GameObject go = new GameObject(objectName);
            go.transform.SetParent(parent, false);
            go.AddComponent<T>();
        }

        private static T EnsureManagerObject<T>(Transform parent, string objectName) where T : Component
        {
            Transform existingByName = parent.Find(objectName);
            if (existingByName != null)
            {
                T existingComponent = existingByName.GetComponent<T>();
                if (existingComponent != null)
                {
                    return existingComponent;
                }

                return existingByName.gameObject.AddComponent<T>();
            }

            T foundAnywhere = UnityEngine.Object.FindFirstObjectByType<T>();
            if (foundAnywhere != null)
            {
                return foundAnywhere;
            }

            GameObject go = new GameObject(objectName);
            go.transform.SetParent(parent, false);
            return go.AddComponent<T>();
        }

        private static void EnsureSceneInBuildSettings(string scenePath)
        {
            if (string.IsNullOrEmpty(scenePath) || !System.IO.File.Exists(scenePath))
            {
                return;
            }

            var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            for (int i = 0; i < scenes.Count; i++)
            {
                if (string.Equals(scenes[i].path, scenePath, System.StringComparison.OrdinalIgnoreCase))
                {
                    if (!scenes[i].enabled)
                    {
                        scenes[i] = new EditorBuildSettingsScene(scenePath, true);
                        EditorBuildSettings.scenes = scenes.ToArray();
                    }

                    return;
                }
            }

            scenes.Add(new EditorBuildSettingsScene(scenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        private static void ApplyBuildSettingsOrder()
        {
            string[] desiredOrder =
            {
                $"{ScenesRoot}/{SceneNames.PersistentSystems}.unity",
                FindScenePathByName(SceneNames.MainMenu),
                $"{ScenesRoot}/{SceneNames.Tutorial}.unity",
                $"{ScenesRoot}/{SceneNames.OpenWorld}.unity",
                $"{ScenesRoot}/{SceneNames.Dungeon01}.unity",
                $"{ScenesRoot}/{SceneNames.Dungeon02}.unity",
                $"{ScenesRoot}/{SceneNames.BossRoom01}.unity",
                $"{ScenesRoot}/{SceneNames.Interior01}.unity",
                $"{ScenesRoot}/{SceneNames.Interior02}.unity"
            };

            var finalScenes = new List<EditorBuildSettingsScene>();
            for (int i = 0; i < desiredOrder.Length; i++)
            {
                string path = desiredOrder[i];
                if (string.IsNullOrEmpty(path))
                {
                    continue;
                }

                if (!System.IO.File.Exists(path))
                {
                    continue;
                }

                finalScenes.Add(new EditorBuildSettingsScene(path, true));
            }

            EditorBuildSettings.scenes = finalScenes.ToArray();
            Debug.Log("Build Settings atualizados para a nova arquitetura de cenas.");
        }

        private static void TryRenameLegacyGameToOpenWorld()
        {
            string legacyPath = FindScenePathByName(SceneNames.LegacyGame);
            string openWorldPath = FindScenePathByName(SceneNames.OpenWorld);

            if (string.IsNullOrEmpty(legacyPath) || !string.IsNullOrEmpty(openWorldPath))
            {
                return;
            }

            string targetPath = $"{ScenesRoot}/{SceneNames.OpenWorld}.unity";
            if (System.IO.File.Exists(targetPath))
            {
                return;
            }

            string moveResult = AssetDatabase.MoveAsset(legacyPath, targetPath);
            if (!string.IsNullOrEmpty(moveResult))
            {
                Debug.LogWarning($"Nao foi possivel renomear Game para OpenWorld automaticamente: {moveResult}");
            }
            else
            {
                Debug.Log("Cena Game renomeada para OpenWorld.");
            }
        }

        private static void EnsureScenesFolder()
        {
            if (!AssetDatabase.IsValidFolder("Assets/_Project"))
            {
                AssetDatabase.CreateFolder("Assets", "_Project");
            }

            if (!AssetDatabase.IsValidFolder("Assets/_Project/Scenes"))
            {
                AssetDatabase.CreateFolder("Assets/_Project", "Scenes");
            }
        }

        private static string FindScenePathByName(string sceneName)
        {
            string[] guids = AssetDatabase.FindAssets($"t:Scene {sceneName}");
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                string fileName = Path.GetFileNameWithoutExtension(path);
                if (string.Equals(fileName, sceneName, System.StringComparison.Ordinal))
                {
                    return path;
                }
            }

            return string.Empty;
        }

        private static GameObject FindOrCreateRoot(string objectName)
        {
            GameObject existing = GameObject.Find(objectName);
            if (existing != null)
            {
                return existing;
            }

            return new GameObject(objectName);
        }

        private static GameObject FindOrCreateUiChild(Transform parent, string childName)
        {
            Transform existing = parent.Find(childName);
            if (existing != null)
            {
                return existing.gameObject;
            }

            GameObject child = new GameObject(childName, typeof(RectTransform));
            child.transform.SetParent(parent, false);
            return child;
        }
    }
}
#endif

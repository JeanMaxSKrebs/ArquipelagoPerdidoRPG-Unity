using System;
using ArquipelagoPerdidoRPG.Inventory;
using UnityEngine;
using UnityEngine.SceneManagement;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace ArquipelagoPerdidoRPG.Core
{
    public class GameManager : SingletonBehaviour<GameManager>
    {
        [Header("Input")]
        [SerializeField] private KeyCode pauseKey = KeyCode.P;
        [SerializeField] private KeyCode menuKey = KeyCode.Escape;

        public bool IsPaused { get; private set; }
        public bool IsGameplayInputEnabled { get; private set; } = true;

        public event Action<bool> OnPauseStateChanged;
        public event Action<bool> OnGameplayInputStateChanged;

        protected override void Awake()
        {
            base.Awake();
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                SceneManager.sceneLoaded -= OnSceneLoaded;
            }
        }

        private void Update()
        {
            if (IsMainMenuLoaded())
            {
                return;
            }

            bool isTutorial = SceneManager.GetActiveScene().name == SceneNames.Tutorial;

            if (!isTutorial && IsKeyDown(pauseKey))
            {
                TogglePause();
            }

            if (!isTutorial && IsKeyDown(menuKey))
            {
                // ESC atua como atalho de menu/pausa sem quebrar o fluxo atual.
                if (InventoryManager.Instance != null && InventoryManager.Instance.IsOpen)
                {
                    InventoryManager.Instance.CloseInventory();
                    UpdateGameplayInputState();
                }
                else
                {
                    TogglePause();
                }
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            SetPause(false);

            if (InventoryManager.Instance != null)
            {
                bool isMainMenu = scene.name == SceneNames.MainMenu;
                bool isBlockedByIndex = scene.buildIndex >= 0 && scene.buildIndex <= 1;
                bool allowInventoryByScene = !isMainMenu && !isBlockedByIndex;

                InventoryManager.Instance.SetSceneInventoryAccess(allowInventoryByScene);
                if (!allowInventoryByScene)
                {
                    InventoryManager.Instance.CloseInventory();
                }
            }

            if (scene.name == SceneNames.Tutorial)
            {
                EnsureTutorialEnvironmentFallback();
            }

            UpdateGameplayInputState();
        }

        public void TogglePause()
        {
            SetPause(!IsPaused);
        }

        public void SetPause(bool value)
        {
            if (IsPaused == value)
            {
                return;
            }

            IsPaused = value;
            Time.timeScale = IsPaused ? 0f : 1f;
            OnPauseStateChanged?.Invoke(IsPaused);
            UpdateGameplayInputState();
        }

        public void ToggleInventory()
        {
            if (InventoryManager.Instance == null)
            {
                return;
            }

            // Mesmo pausado, permite fechar o inventario se ele ja estiver aberto.
            if (IsPaused && !InventoryManager.Instance.IsOpen)
            {
                return;
            }

            InventoryManager.Instance.ToggleInventoryFromInput();
            UpdateGameplayInputState();
        }

        public void CloseAllUIAndResume()
        {
            SetPause(false);
            InventoryManager.Instance?.CloseInventory();
            UpdateGameplayInputState();
        }

        private void UpdateGameplayInputState()
        {
            bool inventoryOpen = InventoryManager.Instance != null && InventoryManager.Instance.IsOpen;
            bool shouldEnable = !IsPaused && !inventoryOpen;

            if (IsGameplayInputEnabled == shouldEnable)
            {
                return;
            }

            IsGameplayInputEnabled = shouldEnable;
            OnGameplayInputStateChanged?.Invoke(IsGameplayInputEnabled);
        }

        private static bool IsKeyDown(KeyCode keyCode)
        {
#if ENABLE_INPUT_SYSTEM
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return false;
            }

            switch (keyCode)
            {
                case KeyCode.P:
                    return keyboard.pKey.wasPressedThisFrame;
                case KeyCode.I:
                    return keyboard.iKey.wasPressedThisFrame;
                case KeyCode.Escape:
                    return keyboard.escapeKey.wasPressedThisFrame;
                default:
                    return false;
            }
#else
            return Input.GetKeyDown(keyCode);
#endif
        }

        private static bool IsMainMenuLoaded()
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene loadedScene = SceneManager.GetSceneAt(i);
                if (loadedScene.isLoaded && loadedScene.name == SceneNames.MainMenu)
                {
                    return true;
                }
            }

            return false;
        }

        private static void EnsureTutorialEnvironmentFallback()
        {
            GameObject legacyGround = GameObject.Find("TutorialGround");
            if (legacyGround != null)
            {
                UnityEngine.Object.Destroy(legacyGround);
            }

            if (GameObject.Find("TutorialTerrain") == null)
            {
                TerrainData terrainData = new TerrainData
                {
                    heightmapResolution = 129,
                    size = new Vector3(180f, 20f, 180f)
                };

                GameObject terrainObject = Terrain.CreateTerrainGameObject(terrainData);
                terrainObject.name = "TutorialTerrain";
                terrainObject.transform.position = new Vector3(-90f, 0f, -90f);
            }

            Light directional = UnityEngine.Object.FindAnyObjectByType<Light>();
            if (directional == null)
            {
                GameObject lightObj = new GameObject("Directional Light");
                directional = lightObj.AddComponent<Light>();
            }

            directional.type = LightType.Directional;
            directional.intensity = 1.15f;
            directional.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        }
    }
}

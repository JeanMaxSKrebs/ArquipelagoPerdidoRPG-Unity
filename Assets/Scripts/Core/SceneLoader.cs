using System.Collections;
using ArquipelagoPerdidoRPG.Inventory;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ArquipelagoPerdidoRPG.Core
{
    public class SceneLoader : SingletonBehaviour<SceneLoader>
    {
        private bool _isLoading;

        public void LoadMainMenu()
        {
            LoadScene(SceneNames.MainMenu);
        }

        public void LoadTutorial()
        {
            LoadScene(SceneNames.Tutorial);
        }

        public void LoadOpenWorld()
        {
            if (CanLoadScene(SceneNames.OpenWorld))
            {
                LoadScene(SceneNames.OpenWorld);
                return;
            }

            LoadScene(SceneNames.LegacyGame);
        }

        public void LoadDungeon01()
        {
            LoadScene(SceneNames.Dungeon01);
        }

        public void LoadDungeon02()
        {
            LoadScene(SceneNames.Dungeon02);
        }

        public void LoadBossRoom01()
        {
            LoadScene(SceneNames.BossRoom01);
        }

        public void LoadInterior01()
        {
            LoadScene(SceneNames.Interior01);
        }

        public void LoadInterior02()
        {
            LoadScene(SceneNames.Interior02);
        }

        public void LoadGame()
        {
            // Compatibilidade com chamadas antigas.
            if (CanLoadScene(SceneNames.OpenWorld))
            {
                LoadScene(SceneNames.OpenWorld);
                return;
            }

            LoadScene(SceneNames.LegacyGame);
        }

        public void LoadMainMenuEntry(bool startInTutorial)
        {
            if (startInTutorial)
            {
                LoadTutorial();
                return;
            }

            LoadOpenWorld();
        }

        public void LoadScene(string sceneName)
        {
            if (_isLoading)
            {
                return;
            }

            if (!CanLoadScene(sceneName))
            {
                Debug.LogError($"Nao foi possivel carregar a cena '{sceneName}'. Verifique se ela esta no Build Settings.");
                return;
            }

            PreparePersistentStateForScene(sceneName);

            StartCoroutine(LoadSceneRoutine(sceneName));
        }

        private static void PreparePersistentStateForScene(string sceneName)
        {
            if (InventoryManager.Instance == null)
            {
                return;
            }

            // Evita vazamento visual/estado entre cenas no primeiro frame de carregamento.
            InventoryManager.Instance.CloseInventory();
            InventoryManager.Instance.SelectCategory(ItemCategory.Consumables);

            if (sceneName == SceneNames.Tutorial)
            {
                InventoryManager.Instance.SetAllowOpen(false);
                return;
            }

            InventoryManager.Instance.SetAllowOpen(true);
        }

        private IEnumerator LoadSceneRoutine(string sceneName)
        {
            _isLoading = true;

            AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
            if (operation == null)
            {
                _isLoading = false;
                Debug.LogError($"SceneManager.LoadSceneAsync retornou null para '{sceneName}'.");
                yield break;
            }

            while (!operation.isDone)
            {
                yield return null;
            }

            _isLoading = false;
        }

        private static bool CanLoadScene(string sceneName)
        {
            if (string.IsNullOrWhiteSpace(sceneName))
            {
                return false;
            }

            return Application.CanStreamedLevelBeLoaded(sceneName);
        }
    }
}

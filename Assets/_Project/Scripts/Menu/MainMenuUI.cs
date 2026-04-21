using ArquipelagoPerdidoRPG.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ArquipelagoPerdidoRPG.Menu
{
    public class MainMenuUI : MonoBehaviour
    {
        [SerializeField] private bool startInTutorial = true;
        [SerializeField] private GameObject optionsPanel;
        [SerializeField] private GameObject languagePanel;

        private void Start()
        {
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (optionsPanel != null)
            {
                optionsPanel.SetActive(false);
            }

            if (languagePanel != null)
            {
                languagePanel.SetActive(false);
            }
        }

        public void OnPlayButtonPressed()
        {
            string targetScene = startInTutorial ? SceneNames.Tutorial : SceneNames.OpenWorld;
            bool canLoadTarget = Application.CanStreamedLevelBeLoaded(targetScene);
            bool canLoadLegacy = Application.CanStreamedLevelBeLoaded(SceneNames.LegacyGame);

            if (!canLoadTarget && !(canLoadLegacy && targetScene == SceneNames.OpenWorld))
            {
                Debug.LogError($"MainMenuUI: cena alvo '{targetScene}' nao esta disponivel no Build Settings.");
                return;
            }

            Debug.Log($"MainMenuUI: carregando cena '{targetScene}'.");

            if (SceneLoader.Instance != null)
            {
                SceneLoader.Instance.LoadMainMenuEntry(startInTutorial);
                return;
            }

            // Fallback de seguranca caso o singleton nao esteja disponivel.
            if (canLoadTarget)
            {
                SceneManager.LoadScene(targetScene);
            }
            else
            {
                SceneManager.LoadScene(SceneNames.LegacyGame);
            }
        }

        public void OnOptionsButtonPressed()
        {
            if (optionsPanel != null)
            {
                optionsPanel.SetActive(true);
            }

            if (languagePanel != null)
            {
                languagePanel.SetActive(false);
            }
        }

        public void OnCloseOptionsButtonPressed()
        {
            if (optionsPanel != null)
            {
                optionsPanel.SetActive(false);
            }
        }

        public void OnLanguageButtonPressed()
        {
            if (languagePanel != null)
            {
                languagePanel.SetActive(true);
            }

            if (optionsPanel != null)
            {
                optionsPanel.SetActive(false);
            }
        }

        public void OnCloseLanguageButtonPressed()
        {
            if (languagePanel != null)
            {
                languagePanel.SetActive(false);
            }
        }

        public void OnQuitButtonPressed()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}

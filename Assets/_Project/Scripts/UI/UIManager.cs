using ArquipelagoPerdidoRPG.Core;
using ArquipelagoPerdidoRPG.Inventory;
using UnityEngine;

namespace ArquipelagoPerdidoRPG.UI
{
    public class UIManager : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject gameplayHudPanel;
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private GameObject inventoryPanel;
        [SerializeField] private GameObject optionsPanel;

        private void Start()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnPauseStateChanged += OnPauseStateChanged;
                GameManager.Instance.OnGameplayInputStateChanged += OnGameplayInputStateChanged;
            }

            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.OnInventoryStateChanged += OnInventoryStateChanged;
            }

            RefreshAll();
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnPauseStateChanged -= OnPauseStateChanged;
                GameManager.Instance.OnGameplayInputStateChanged -= OnGameplayInputStateChanged;
            }

            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.OnInventoryStateChanged -= OnInventoryStateChanged;
            }
        }

        public void OnResumeButtonPressed()
        {
            GameManager.Instance?.SetPause(false);
        }

        public void OnOpenOptionsButtonPressed()
        {
            if (optionsPanel != null)
            {
                optionsPanel.SetActive(true);
            }
        }

        public void OnCloseOptionsButtonPressed()
        {
            if (optionsPanel != null)
            {
                optionsPanel.SetActive(false);
            }
        }

        public void OnBackToMenuButtonPressed()
        {
            GameManager.Instance?.CloseAllUIAndResume();
            SceneLoader.Instance?.LoadMainMenu();
        }

        private void OnPauseStateChanged(bool isPaused)
        {
            if (pausePanel != null)
            {
                pausePanel.SetActive(isPaused);
            }

            if (!isPaused && optionsPanel != null)
            {
                optionsPanel.SetActive(false);
            }

            RefreshHud();
        }

        private void OnInventoryStateChanged(bool isOpen)
        {
            if (inventoryPanel != null)
            {
                inventoryPanel.SetActive(isOpen);
            }

            RefreshHud();
        }

        private void OnGameplayInputStateChanged(bool isEnabled)
        {
            Cursor.lockState = isEnabled ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !isEnabled;
        }

        private void RefreshAll()
        {
            OnPauseStateChanged(GameManager.Instance != null && GameManager.Instance.IsPaused);
            OnInventoryStateChanged(InventoryManager.Instance != null && InventoryManager.Instance.IsOpen);
            OnGameplayInputStateChanged(GameManager.Instance != null && GameManager.Instance.IsGameplayInputEnabled);
        }

        private void RefreshHud()
        {
            if (gameplayHudPanel == null)
            {
                return;
            }

            bool pause = GameManager.Instance != null && GameManager.Instance.IsPaused;
            bool inventory = InventoryManager.Instance != null && InventoryManager.Instance.IsOpen;
            gameplayHudPanel.SetActive(!pause && !inventory);
        }
    }
}

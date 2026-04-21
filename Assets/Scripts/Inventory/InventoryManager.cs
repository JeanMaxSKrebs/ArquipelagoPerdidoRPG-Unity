using System;
using System.Collections.Generic;
using ArquipelagoPerdidoRPG.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace ArquipelagoPerdidoRPG.Inventory
{
    public class InventoryManager : SingletonBehaviour<InventoryManager>
    {
        [Header("Runtime")]
        [SerializeField] private ItemCategory selectedCategory = ItemCategory.Consumables;
        [SerializeField] private List<InventoryItem> items = new List<InventoryItem>();

        [Header("Access Control")]
        [Tooltip("Quando false, o inventario nao pode ser aberto. Controlado pelo TutorialManager durante o tutorial.")]
        [SerializeField] private bool _allowOpen = true;

        [Header("Debug (Runtime)")]
        [SerializeField] private bool _debugIsOpen;
        [SerializeField] private bool _debugCanOpen;
        [SerializeField] private bool _debugSceneAllowsInventory;
        [SerializeField] private bool _autoCreateInventoryUiIfMissing = true;

        private bool _sceneAllowsInventory = true;

        public bool IsOpen { get; private set; }
        public bool CanOpen => _allowOpen && _sceneAllowsInventory;
        public ItemCategory SelectedCategory => selectedCategory;
        public InventoryItem SelectedItem { get; private set; }

        public event Action<bool> OnInventoryStateChanged;
        public event Action<ItemCategory> OnCategoryChanged;
        public event Action<InventoryItem> OnItemSelected;
        public event Action OnInventoryChanged;

        protected override void Awake()
        {
            base.Awake();
            IsOpen = false;
            EnsureDefaultItems();
            SelectCategory(selectedCategory);
            SceneManager.sceneLoaded += OnSceneLoaded;
            EnsureInventoryUiRoot();
            ApplySceneRules(SceneManager.GetActiveScene());
            SyncDebugState();
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
            if (_autoCreateInventoryUiIfMissing)
            {
                EnsureInventoryUiRoot();
            }

            // No Tutorial, bloqueia completamente a abertura do inventário via tecla
            Scene currentScene = SceneManager.GetActiveScene();
            if (currentScene.name == SceneNames.Tutorial)
            {
                if (IsToggleKeyPressed())
                {
                    // Tecla I pressionada mas inventário bloqueado no Tutorial
                    // Silenciosamente ignora
                }
            }
            else if (IsToggleKeyPressed())
            {
                ToggleInventoryFromInput();
            }

            SyncDebugState();
        }

        public void ToggleInventory()
        {
            SetInventoryState(!IsOpen);
        }

        public void OpenInventory()
        {
            SetInventoryState(true);
        }

        public void CloseInventory()
        {
            SetInventoryState(false);
        }

        public void ToggleInventoryFromInput()
        {
            // Regra de UX: tecla I sempre fecha quando aberto.
            if (IsOpen)
            {
                CloseInventory();
                return;
            }

            if (CanOpen)
            {
                OpenInventory();
            }
        }

        public IReadOnlyList<InventoryItem> GetItemsByCategory(ItemCategory category)
        {
            var filtered = new List<InventoryItem>();
            for (int i = 0; i < items.Count; i++)
            {
                InventoryItem item = items[i];
                if (item == null || item.data == null)
                {
                    continue;
                }

                if (item.Category == category)
                {
                    filtered.Add(item);
                }
            }

            return filtered;
        }

        public void SelectCategory(ItemCategory category)
        {
            selectedCategory = category;
            SelectedItem = null;
            OnCategoryChanged?.Invoke(selectedCategory);
            OnItemSelected?.Invoke(SelectedItem);
        }

        public void SelectItem(InventoryItem item)
        {
            if (item == null)
            {
                return;
            }

            SelectedItem = item;
            OnItemSelected?.Invoke(SelectedItem);
        }

        public void TryPerformPrimaryActionOnSelectedItem()
        {
            if (SelectedItem == null || SelectedItem.data == null)
            {
                return;
            }

            switch (SelectedItem.Category)
            {
                case ItemCategory.Consumables:
                    ConsumeSelected();
                    break;
                case ItemCategory.Weapons:
                    Debug.Log($"Inventory: '{SelectedItem.DisplayName}' preparado para equipar.");
                    break;
                case ItemCategory.Materials:
                    Debug.Log($"Inventory: '{SelectedItem.DisplayName}' disponivel para crafting futuro.");
                    break;
                case ItemCategory.Tools:
                    Debug.Log($"Inventory: '{SelectedItem.DisplayName}' preparado para uso em coleta/sistemas.");
                    break;
                case ItemCategory.SpecialItems:
                    Debug.Log($"Inventory: '{SelectedItem.DisplayName}' marcado como item especial.");
                    break;
                case ItemCategory.QuestItems:
                    Debug.Log($"Inventory: '{SelectedItem.DisplayName}' e um item de missao (somente visualizacao).");
                    break;
            }
        }

        public void AddItem(InventoryItemData data, int quantity)
        {
            if (data == null || quantity <= 0)
            {
                return;
            }

            for (int i = 0; i < items.Count; i++)
            {
                InventoryItem entry = items[i];
                if (entry == null)
                {
                    continue;
                }

                if (entry.CanStackWith(data))
                {
                    entry.quantity = Mathf.Min(entry.quantity + quantity, Mathf.Max(1, data.maxStack));
                    OnInventoryChanged?.Invoke();
                    return;
                }
            }

            items.Add(new InventoryItem
            {
                data = data,
                quantity = quantity
            });

            OnInventoryChanged?.Invoke();
        }

        private void SetInventoryState(bool value)
        {
            if (IsOpen == value)
            {
                return;
            }

            if (value && !_allowOpen)
            {
                return;
            }

            IsOpen = value;
            if (!IsOpen)
            {
                SelectedItem = null;
                OnItemSelected?.Invoke(SelectedItem);
            }

            OnInventoryStateChanged?.Invoke(IsOpen);
            SyncDebugState();
        }

        /// <summary>
        /// Libera ou bloqueia a abertura do inventario. Usado pelo TutorialManager para controle de etapa.
        /// </summary>
        public void SetAllowOpen(bool allow)
        {
            _allowOpen = allow;
            if (!allow && IsOpen)
            {
                CloseInventory();
            }

            SyncDebugState();
        }

        public void SetSceneInventoryAccess(bool allow)
        {
            _sceneAllowsInventory = allow;
            if (!allow)
            {
                CloseInventory();
            }

            SyncDebugState();
        }

        public void ForceFullReset()
        {
            IsOpen = false;
            _allowOpen = false;
            _sceneAllowsInventory = true;
            SelectCategory(ItemCategory.Consumables);
            SelectedItem = null;
            OnInventoryStateChanged?.Invoke(false);
            SyncDebugState();
        }

        private void ConsumeSelected()
        {
            if (SelectedItem == null)
            {
                return;
            }

            SelectedItem.quantity = Mathf.Max(SelectedItem.quantity - 1, 0);
            Debug.Log($"Inventory: consumiu '{SelectedItem.DisplayName}'. Valor base: {SelectedItem.data.primaryValue}");

            if (SelectedItem.quantity <= 0)
            {
                items.Remove(SelectedItem);
                SelectedItem = null;
            }

            OnItemSelected?.Invoke(SelectedItem);
            OnInventoryChanged?.Invoke();
        }

        private void EnsureDefaultItems()
        {
            if (items != null && items.Count > 0)
            {
                return;
            }

            items = new List<InventoryItem>
            {
                CreateItem("item_apple", "Maca", "Fruta simples para recuperar energia.", ItemCategory.Consumables, ItemType.Fruit, 5, 10f),
                CreateItem("item_water", "Agua", "Bebida para restaurar hidratacao.", ItemCategory.Consumables, ItemType.Drink, 3, 15f),
                CreateItem("item_potion_small", "Pocao Pequena", "Pocao basica de recuperacao.", ItemCategory.Consumables, ItemType.Potion, 2, 25f),
                CreateItem("item_sword", "Espada Simples", "Arma inicial para combate corpo a corpo.", ItemCategory.Weapons, ItemType.Weapon, 1, 8f, false),
                CreateItem("item_wood", "Madeira", "Material comum de construcao e crafting.", ItemCategory.Materials, ItemType.Material, 12, 0f),
                CreateItem("item_stone", "Pedra", "Material base para ferramentas e estrutura.", ItemCategory.Materials, ItemType.Material, 10, 0f)
            };
        }

        private static InventoryItem CreateItem(
            string id,
            string name,
            string description,
            ItemCategory category,
            ItemType type,
            int quantity,
            float primaryValue,
            bool stackable = true)
        {
            return new InventoryItem
            {
                data = new InventoryItemData
                {
                    id = id,
                    displayName = name,
                    description = description,
                    category = category,
                    type = type,
                    primaryValue = primaryValue,
                    stackable = stackable,
                    maxStack = stackable ? 99 : 1
                },
                quantity = Mathf.Max(1, quantity)
            };
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            ApplySceneRules(scene);
        }

        private void ApplySceneRules(Scene scene)
        {
            bool allowInScene = EvaluateInventoryAccessFromLoadedScenes();

            SetSceneInventoryAccess(allowInScene);

            if (!allowInScene)
            {
                SetAllowOpen(false);
                SelectCategory(ItemCategory.Consumables);
                return;
            }

            if (scene.name == SceneNames.Tutorial)
            {
                // Tutorial inicia bloqueado e libera por etapa via TutorialManager.
                SetAllowOpen(false);
                CloseInventory();
                SelectCategory(ItemCategory.Consumables);
            }
            else
            {
                // Cenas de gameplay fora do tutorial usam inventario normalmente.
                SetAllowOpen(true);
            }

            SyncDebugState();
        }

        private static bool EvaluateInventoryAccessFromLoadedScenes()
        {
            bool hasGameplaySceneLoaded = false;

            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene loadedScene = SceneManager.GetSceneAt(i);
                if (!loadedScene.isLoaded)
                {
                    continue;
                }

                if (loadedScene.name == SceneNames.PersistentSystems)
                {
                    continue;
                }

                if (loadedScene.name == SceneNames.MainMenu)
                {
                    return false;
                }

                if (loadedScene.buildIndex > 1)
                {
                    hasGameplaySceneLoaded = true;
                }
            }

            return hasGameplaySceneLoaded;
        }

            private static bool IsToggleKeyPressed()
            {
        #if ENABLE_INPUT_SYSTEM
                Keyboard keyboard = Keyboard.current;
                return keyboard != null && keyboard.iKey.wasPressedThisFrame;
        #else
                return Input.GetKeyDown(KeyCode.I);
        #endif
            }

                private void SyncDebugState()
                {
                    _debugIsOpen = IsOpen;
                    _debugCanOpen = CanOpen;
                    _debugSceneAllowsInventory = _sceneAllowsInventory;
                }

                private static void EnsureInventoryUiRoot()
                {
                    if (UnityEngine.Object.FindAnyObjectByType<InventoryUI>() != null)
                    {
                        return;
                    }

                    GameObject canvasObj = GameObject.Find("Canvas_Inventory");
                    if (canvasObj == null)
                    {
                        canvasObj = new GameObject("Canvas_Inventory", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                    }

                    Canvas canvas = canvasObj.GetComponent<Canvas>();
                    if (canvas == null)
                    {
                        canvas = canvasObj.AddComponent<Canvas>();
                    }
                    canvas.renderMode = RenderMode.ScreenSpaceOverlay;

                    CanvasScaler scaler = canvasObj.GetComponent<CanvasScaler>();
                    if (scaler == null)
                    {
                        scaler = canvasObj.AddComponent<CanvasScaler>();
                    }
                    scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                    scaler.referenceResolution = new Vector2(1920f, 1080f);
                    scaler.matchWidthOrHeight = 0.5f;

                    if (canvasObj.GetComponent<GraphicRaycaster>() == null)
                    {
                        canvasObj.AddComponent<GraphicRaycaster>();
                    }

                    if (canvasObj.GetComponent<InventoryUI>() == null)
                    {
                        canvasObj.AddComponent<InventoryUI>();
                    }
                }
    }
}

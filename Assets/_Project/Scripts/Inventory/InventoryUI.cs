using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace ArquipelagoPerdidoRPG.Inventory
{
    public class InventoryUI : MonoBehaviour
    {
        [Header("Root")]
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Transform categoriesRoot;
        [SerializeField] private Transform slotsRoot;
        [SerializeField] private GameObject slotTemplate;

        [Header("Details")]
        [SerializeField] private Text itemNameText;
        [SerializeField] private Text itemDescriptionText;
        [SerializeField] private Text itemQuantityText;
        [SerializeField] private Text itemTypeText;
        [SerializeField] private Text categoryTitleText;

        [Header("Buttons")]
        [SerializeField] private Button closeButton;
        [SerializeField] private Button consumeButton;

        private readonly Dictionary<ItemCategory, Button> _categoryButtons = new Dictionary<ItemCategory, Button>();
        private readonly List<InventorySlotUI> _activeSlots = new List<InventorySlotUI>();
        private Canvas _canvas;

        private void Awake()
        {
            // Force reset completo
            if (panelRoot != null)
            {
                panelRoot.SetActive(false);
            }
            
            BuildRuntimeLayoutIfMissing();
            AutoResolveReferences();
            WireCategoryButtons();
            _canvas = GetComponent<Canvas>();
            
            // Desativa canvas se for Tutorial (bloqueia completamente)
            Scene activeScene = SceneManager.GetActiveScene();
            if (activeScene.name == Core.SceneNames.Tutorial && _canvas != null)
            {
                _canvas.enabled = false;
            }
            
            SetPanelActive(false);
            ApplySceneVisibility();
        }

        private void OnEnable()
        {
            // Force reset se Tutorial
            Scene activeScene = SceneManager.GetActiveScene();
            if (activeScene.name == Core.SceneNames.Tutorial)
            {
                if (_canvas != null)
                {
                    _canvas.enabled = false;
                }
                if (panelRoot != null)
                {
                    panelRoot.SetActive(false);
                }
                
                // Desabilita listeners para não reagir a eventos
                if (InventoryManager.Instance != null)
                {
                    InventoryManager.Instance.OnInventoryStateChanged -= OnInventoryStateChanged;
                    InventoryManager.Instance.OnCategoryChanged -= OnCategoryChanged;
                    InventoryManager.Instance.OnItemSelected -= OnItemSelected;
                    InventoryManager.Instance.OnInventoryChanged -= OnInventoryChanged;
                }
                return; // Sai daqui, não registra listeners
            }
            
            SceneManager.sceneLoaded += OnSceneLoaded;

            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.OnInventoryStateChanged += OnInventoryStateChanged;
                InventoryManager.Instance.OnCategoryChanged += OnCategoryChanged;
                InventoryManager.Instance.OnItemSelected += OnItemSelected;
                InventoryManager.Instance.OnInventoryChanged += OnInventoryChanged;
            }

            if (closeButton != null)
            {
                closeButton.onClick.AddListener(OnClosePressed);
            }

            if (consumeButton != null)
            {
                consumeButton.onClick.AddListener(OnConsumePressed);
            }

            ApplySceneVisibility();
            RefreshAll();
        }

        private void Update()
        {
            // Force painel fechado no Tutorial (estado residual failsafe)
            Scene activeScene = SceneManager.GetActiveScene();
            if (activeScene.name == Core.SceneNames.Tutorial && panelRoot != null && panelRoot.activeSelf)
            {
                panelRoot.SetActive(false);
            }
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;

            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.OnInventoryStateChanged -= OnInventoryStateChanged;
                InventoryManager.Instance.OnCategoryChanged -= OnCategoryChanged;
                InventoryManager.Instance.OnItemSelected -= OnItemSelected;
                InventoryManager.Instance.OnInventoryChanged -= OnInventoryChanged;
            }

            if (closeButton != null)
            {
                closeButton.onClick.RemoveListener(OnClosePressed);
            }

            if (consumeButton != null)
            {
                consumeButton.onClick.RemoveListener(OnConsumePressed);
            }
        }

        private void RefreshAll()
        {
            ApplySceneVisibility();

            InventoryManager manager = InventoryManager.Instance;
            if (manager == null)
            {
                SetPanelActive(false);
                return;
            }

            // No Tutorial, força painel fechado
            Scene activeScene = SceneManager.GetActiveScene();
            if (activeScene.name == Core.SceneNames.Tutorial)
            {
                SetPanelActive(false);
            }
            else
            {
                SetPanelActive(manager.IsOpen && IsInventoryVisibleInCurrentContext());
            }
            
            RebuildSlots(manager);
            RefreshDetails(manager.SelectedItem);
            RefreshCategoryVisual(manager.SelectedCategory);
        }

        private void RebuildSlots(InventoryManager manager)
        {
            if (slotsRoot == null || slotTemplate == null)
            {
                return;
            }

            for (int i = 0; i < _activeSlots.Count; i++)
            {
                if (_activeSlots[i] != null)
                {
                    Destroy(_activeSlots[i].gameObject);
                }
            }

            _activeSlots.Clear();

            IReadOnlyList<InventoryItem> items = manager.GetItemsByCategory(manager.SelectedCategory);
            for (int i = 0; i < items.Count; i++)
            {
                GameObject slotObj = Instantiate(slotTemplate, slotsRoot);
                slotObj.SetActive(true);
                InventorySlotUI slot = slotObj.GetComponent<InventorySlotUI>();
                if (slot == null)
                {
                    slot = slotObj.AddComponent<InventorySlotUI>();
                }

                InventoryItem item = items[i];
                slot.Bind(item, item == manager.SelectedItem, OnSlotSelected);
                _activeSlots.Add(slot);
            }

            if (categoryTitleText != null)
            {
                categoryTitleText.text = CategoryToLabel(manager.SelectedCategory);
            }
        }

        private void RefreshDetails(InventoryItem item)
        {
            if (itemNameText != null)
            {
                itemNameText.text = item != null ? item.DisplayName : "Selecione um item";
            }

            if (itemDescriptionText != null)
            {
                itemDescriptionText.text = item != null ? item.Description : "Escolha um item para ver os detalhes.";
            }

            if (itemQuantityText != null)
            {
                itemQuantityText.text = item != null ? $"Quantidade: {Mathf.Max(1, item.quantity)}" : "Quantidade: -";
            }

            if (itemTypeText != null)
            {
                itemTypeText.text = item != null ? $"Tipo: {item.Type}" : "Tipo: -";
            }

            if (consumeButton != null)
            {
                consumeButton.interactable = item != null;
            }
        }

        private void RefreshCategoryVisual(ItemCategory selectedCategory)
        {
            foreach (var pair in _categoryButtons)
            {
                if (pair.Value == null)
                {
                    continue;
                }

                ColorBlock colors = pair.Value.colors;
                colors.normalColor = pair.Key == selectedCategory
                    ? new Color(0.11f, 0.45f, 0.66f, 1f)
                    : new Color(0.07f, 0.30f, 0.44f, 0.96f);
                pair.Value.colors = colors;
            }
        }

        private void OnInventoryStateChanged(bool isOpen)
        {
            bool shouldShow = isOpen && IsInventoryVisibleInCurrentContext();
            SetPanelActive(shouldShow);
            Cursor.lockState = isOpen ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = isOpen;

            if (shouldShow)
            {
                RefreshAll();
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            ApplySceneVisibility();
            
            // Force fechar no Tutorial
            if (scene.name == Core.SceneNames.Tutorial && panelRoot != null)
            {
                panelRoot.SetActive(false);
            }
            
            RefreshAll();
        }

        private void OnCategoryChanged(ItemCategory category)
        {
            InventoryManager manager = InventoryManager.Instance;
            if (manager == null)
            {
                return;
            }

            RebuildSlots(manager);
            RefreshCategoryVisual(category);
            RefreshDetails(manager.SelectedItem);
        }

        private void OnItemSelected(InventoryItem item)
        {
            InventoryManager manager = InventoryManager.Instance;
            if (manager != null)
            {
                RebuildSlots(manager);
            }

            RefreshDetails(item);
        }

        private void OnInventoryChanged()
        {
            InventoryManager manager = InventoryManager.Instance;
            if (manager == null)
            {
                return;
            }

            RebuildSlots(manager);
            RefreshDetails(manager.SelectedItem);
        }

        private void OnClosePressed()
        {
            InventoryManager.Instance?.CloseInventory();
        }

        private void OnConsumePressed()
        {
            InventoryManager manager = InventoryManager.Instance;
            if (manager == null)
            {
                return;
            }

            manager.TryPerformPrimaryActionOnSelectedItem();
            RefreshAll();
        }

        private void OnSlotSelected(InventoryItem item)
        {
            InventoryManager.Instance?.SelectItem(item);
        }

        private void WireCategoryButtons()
        {
            _categoryButtons.Clear();
            if (categoriesRoot == null)
            {
                return;
            }

            RegisterCategoryButton("Button_Consumables", ItemCategory.Consumables);
            RegisterCategoryButton("Button_Weapons", ItemCategory.Weapons);
            RegisterCategoryButton("Button_Materials", ItemCategory.Materials);
            RegisterCategoryButton("Button_Tools", ItemCategory.Tools);
            RegisterCategoryButton("Button_Special", ItemCategory.SpecialItems);
            RegisterCategoryButton("Button_Quest", ItemCategory.QuestItems);
        }

        private void RegisterCategoryButton(string childName, ItemCategory category)
        {
            Transform buttonTransform = categoriesRoot.Find(childName);
            if (buttonTransform == null)
            {
                return;
            }

            Button button = buttonTransform.GetComponent<Button>();
            if (button == null)
            {
                return;
            }

            _categoryButtons[category] = button;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => InventoryManager.Instance?.SelectCategory(category));
        }

        private void SetPanelActive(bool active)
        {
            if (panelRoot != null)
            {
                panelRoot.SetActive(active);
            }
        }

        public void EnableForInventoryStep()
        {
            if (_canvas != null)
            {
                _canvas.enabled = true;
            }

            SceneManager.sceneLoaded += OnSceneLoaded;

            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.OnInventoryStateChanged += OnInventoryStateChanged;
                InventoryManager.Instance.OnCategoryChanged += OnCategoryChanged;
                InventoryManager.Instance.OnItemSelected += OnItemSelected;
                InventoryManager.Instance.OnInventoryChanged += OnInventoryChanged;
            }

            if (closeButton != null)
            {
                closeButton.onClick.AddListener(OnClosePressed);
            }

            if (consumeButton != null)
            {
                consumeButton.onClick.AddListener(OnConsumePressed);
            }

            ApplySceneVisibility();
            RefreshAll();
        }

        public void DisableForTutorial()
        {
            SetPanelActive(false);
        }

        private void ApplySceneVisibility()
        {
            if (_canvas != null)
            {
                _canvas.enabled = IsInventoryVisibleInCurrentContext();
            }

            if (!IsInventoryVisibleInCurrentContext())
            {
                SetPanelActive(false);
            }
        }

        private static bool IsInventoryVisibleInCurrentContext()
        {
            bool hasGameplaySceneLoaded = false;

            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene loadedScene = SceneManager.GetSceneAt(i);
                if (!loadedScene.isLoaded)
                {
                    continue;
                }

                if (loadedScene.name == Core.SceneNames.PersistentSystems)
                {
                    continue;
                }

                if (loadedScene.name == Core.SceneNames.MainMenu)
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

        private static string CategoryToLabel(ItemCategory category)
        {
            switch (category)
            {
                case ItemCategory.Consumables:
                    return "Consumiveis";
                case ItemCategory.Weapons:
                    return "Armas";
                case ItemCategory.Materials:
                    return "Materiais";
                case ItemCategory.Tools:
                    return "Ferramentas";
                case ItemCategory.SpecialItems:
                    return "Itens Especiais";
                case ItemCategory.QuestItems:
                    return "Itens de Missao";
                default:
                    return "Categoria";
            }
        }

        private void AutoResolveReferences()
        {
            Transform root = transform;
            panelRoot ??= FindByPath(root, "Panel_Inventory")?.gameObject;
            categoriesRoot ??= FindByPath(root, "Panel_Inventory/Categories");
            slotsRoot ??= FindByPath(root, "Panel_Inventory/Body/ItemsScroll/Viewport/Content");
            slotTemplate ??= FindByPath(root, "Panel_Inventory/Body/ItemsScroll/Viewport/Content/Slot_Template")?.gameObject;
            itemNameText ??= FindText(root, "Panel_Inventory/Body/Details/Text_ItemName");
            itemDescriptionText ??= FindText(root, "Panel_Inventory/Body/Details/Text_ItemDescription");
            itemQuantityText ??= FindText(root, "Panel_Inventory/Body/Details/Text_ItemQuantity");
            itemTypeText ??= FindText(root, "Panel_Inventory/Body/Details/Text_ItemType");
            categoryTitleText ??= FindText(root, "Panel_Inventory/Categories/Text_CategoryTitle");
            closeButton ??= FindByPath(root, "Panel_Inventory/Button_Close")?.GetComponent<Button>();
            consumeButton ??= FindByPath(root, "Panel_Inventory/Body/Details/Button_PrimaryAction")?.GetComponent<Button>();
        }

        private void BuildRuntimeLayoutIfMissing()
        {
            if (transform.Find("Panel_Inventory") != null)
            {
                return;
            }

            GameObject panel = new GameObject("Panel_Inventory", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(transform, false);
            Image panelImage = panel.GetComponent<Image>();
            panelImage.color = new Color(0.02f, 0.08f, 0.13f, 0.95f);

            RectTransform panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.08f, 0.08f);
            panelRect.anchorMax = new Vector2(0.92f, 0.92f);
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            GameObject categories = CreateUiObject("Categories", panel.transform, true);
            RectTransform categoriesRect = categories.GetComponent<RectTransform>();
            categoriesRect.anchorMin = new Vector2(0f, 1f);
            categoriesRect.anchorMax = new Vector2(1f, 1f);
            categoriesRect.pivot = new Vector2(0.5f, 1f);
            categoriesRect.anchoredPosition = new Vector2(0f, -20f);
            categoriesRect.sizeDelta = new Vector2(-32f, 88f);

            Text categoryTitle = CreateText("Text_CategoryTitle", categories.transform, "Consumiveis", 24, TextAnchor.MiddleLeft);
            RectTransform titleRect = categoryTitle.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0f, 0.5f);
            titleRect.anchorMax = new Vector2(0f, 0.5f);
            titleRect.pivot = new Vector2(0f, 0.5f);
            titleRect.anchoredPosition = new Vector2(14f, 0f);
            titleRect.sizeDelta = new Vector2(220f, 44f);

            CreateCategoryButton("Button_Consumables", categories.transform, "Consumiveis", new Vector2(280f, 0f));
            CreateCategoryButton("Button_Weapons", categories.transform, "Armas", new Vector2(430f, 0f));
            CreateCategoryButton("Button_Materials", categories.transform, "Materiais", new Vector2(580f, 0f));
            CreateCategoryButton("Button_Tools", categories.transform, "Ferramentas", new Vector2(730f, 0f));
            CreateCategoryButton("Button_Special", categories.transform, "Especiais", new Vector2(900f, 0f));
            CreateCategoryButton("Button_Quest", categories.transform, "Missao", new Vector2(1040f, 0f));

            GameObject body = CreateUiObject("Body", panel.transform, false);
            RectTransform bodyRect = body.GetComponent<RectTransform>();
            bodyRect.anchorMin = new Vector2(0f, 0f);
            bodyRect.anchorMax = new Vector2(1f, 1f);
            bodyRect.offsetMin = new Vector2(16f, 16f);
            bodyRect.offsetMax = new Vector2(-16f, -120f);

            GameObject itemsScroll = CreateUiObject("ItemsScroll", body.transform, true);
            Image itemsScrollImage = itemsScroll.GetComponent<Image>();
            itemsScrollImage.color = new Color(0.03f, 0.12f, 0.18f, 1f);
            ScrollRect scrollRect = itemsScroll.AddComponent<ScrollRect>();
            RectTransform itemsScrollRect = itemsScroll.GetComponent<RectTransform>();
            itemsScrollRect.anchorMin = new Vector2(0f, 0f);
            itemsScrollRect.anchorMax = new Vector2(0.56f, 1f);
            itemsScrollRect.offsetMin = Vector2.zero;
            itemsScrollRect.offsetMax = new Vector2(-12f, 0f);

            GameObject viewport = CreateUiObject("Viewport", itemsScroll.transform, true);
            Mask mask = viewport.AddComponent<Mask>();
            mask.showMaskGraphic = false;
            Image viewportImage = viewport.GetComponent<Image>();
            viewportImage.color = new Color(1f, 1f, 1f, 0.02f);
            RectTransform viewportRect = viewport.GetComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.offsetMin = Vector2.zero;
            viewportRect.offsetMax = Vector2.zero;

            GameObject content = CreateUiObject("Content", viewport.transform, false);
            RectTransform contentRect = content.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot = new Vector2(0.5f, 1f);
            contentRect.anchoredPosition = Vector2.zero;
            contentRect.sizeDelta = new Vector2(0f, 400f);

            VerticalLayoutGroup layout = content.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(10, 10, 10, 10);
            layout.spacing = 8f;
            layout.childControlHeight = false;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;

            ContentSizeFitter fitter = content.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

            scrollRect.viewport = viewportRect;
            scrollRect.content = contentRect;
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;

            GameObject slotTemplateGo = CreateUiObject("Slot_Template", content.transform, true);
            slotTemplateGo.AddComponent<InventorySlotUI>();
            Button slotButton = slotTemplateGo.AddComponent<Button>();
            Image slotImage = slotTemplateGo.GetComponent<Image>();
            slotImage.color = new Color(0.05f, 0.15f, 0.24f, 0.88f);
            ColorBlock slotColors = slotButton.colors;
            slotColors.normalColor = slotImage.color;
            slotColors.highlightedColor = new Color(0.08f, 0.22f, 0.34f, 0.96f);
            slotColors.pressedColor = new Color(0.10f, 0.32f, 0.48f, 1f);
            slotButton.colors = slotColors;

            RectTransform slotRect = slotTemplateGo.GetComponent<RectTransform>();
            slotRect.sizeDelta = new Vector2(0f, 54f);
            LayoutElement slotLayout = slotTemplateGo.AddComponent<LayoutElement>();
            slotLayout.preferredHeight = 54f;
            slotLayout.minHeight = 54f;

            Text slotName = CreateText("Text_Name", slotTemplateGo.transform, "Item", 18, TextAnchor.MiddleLeft);
            RectTransform slotNameRect = slotName.GetComponent<RectTransform>();
            slotNameRect.anchorMin = new Vector2(0f, 0f);
            slotNameRect.anchorMax = new Vector2(0.7f, 1f);
            slotNameRect.offsetMin = new Vector2(14f, 0f);
            slotNameRect.offsetMax = new Vector2(-8f, 0f);

            Text slotQty = CreateText("Text_Quantity", slotTemplateGo.transform, "x1", 16, TextAnchor.MiddleRight);
            RectTransform slotQtyRect = slotQty.GetComponent<RectTransform>();
            slotQtyRect.anchorMin = new Vector2(0.7f, 0f);
            slotQtyRect.anchorMax = new Vector2(1f, 1f);
            slotQtyRect.offsetMin = new Vector2(0f, 0f);
            slotQtyRect.offsetMax = new Vector2(-12f, 0f);
            slotTemplateGo.SetActive(false);

            GameObject details = CreateUiObject("Details", body.transform, true);
            details.GetComponent<Image>().color = new Color(0.03f, 0.12f, 0.18f, 1f);
            RectTransform detailsRect = details.GetComponent<RectTransform>();
            detailsRect.anchorMin = new Vector2(0.56f, 0f);
            detailsRect.anchorMax = new Vector2(1f, 1f);
            detailsRect.offsetMin = new Vector2(12f, 0f);
            detailsRect.offsetMax = Vector2.zero;

            Text itemName = CreateText("Text_ItemName", details.transform, "Selecione um item", 28, TextAnchor.UpperLeft);
            SetBlockRect(itemName.rectTransform, new Vector2(14f, -14f), new Vector2(-14f, -74f));

            Text itemDesc = CreateText("Text_ItemDescription", details.transform, "Escolha um item para ver os detalhes.", 18, TextAnchor.UpperLeft);
            SetBlockRect(itemDesc.rectTransform, new Vector2(14f, -84f), new Vector2(-14f, -210f));

            Text itemQty = CreateText("Text_ItemQuantity", details.transform, "Quantidade: -", 18, TextAnchor.UpperLeft);
            SetBlockRect(itemQty.rectTransform, new Vector2(14f, -220f), new Vector2(-14f, -256f));

            Text itemType = CreateText("Text_ItemType", details.transform, "Tipo: -", 18, TextAnchor.UpperLeft);
            SetBlockRect(itemType.rectTransform, new Vector2(14f, -262f), new Vector2(-14f, -298f));

            Button actionButton = CreateButton("Button_PrimaryAction", details.transform, "Acao", new Vector2(170f, 50f), new Vector2(14f, 18f));
            RectTransform actionRect = actionButton.GetComponent<RectTransform>();
            actionRect.anchorMin = new Vector2(0f, 0f);
            actionRect.anchorMax = new Vector2(0f, 0f);
            actionRect.pivot = new Vector2(0f, 0f);

            Button close = CreateButton("Button_Close", panel.transform, "Fechar", new Vector2(180f, 52f), new Vector2(-16f, 16f));
            RectTransform closeRect = close.GetComponent<RectTransform>();
            closeRect.anchorMin = new Vector2(1f, 0f);
            closeRect.anchorMax = new Vector2(1f, 0f);
            closeRect.pivot = new Vector2(1f, 0f);
        }

        private static void SetBlockRect(RectTransform rect, Vector2 topLeft, Vector2 bottomRight)
        {
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0f, topLeft.y);
            rect.offsetMin = new Vector2(topLeft.x, bottomRight.y);
            rect.offsetMax = new Vector2(bottomRight.x, topLeft.y);
        }

        private Button CreateCategoryButton(string name, Transform parent, string label, Vector2 anchoredPosition)
        {
            Button button = CreateButton(name, parent, label, new Vector2(136f, 46f), anchoredPosition);
            RectTransform rect = button.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 0.5f);
            rect.anchorMax = new Vector2(0f, 0.5f);
            rect.pivot = new Vector2(0f, 0.5f);
            return button;
        }

        private static Button CreateButton(string name, Transform parent, string label, Vector2 size, Vector2 anchoredPos)
        {
            GameObject buttonObj = CreateUiObject(name, parent, true);
            Image image = buttonObj.GetComponent<Image>();
            image.color = new Color(0.07f, 0.30f, 0.44f, 0.96f);

            Button button = buttonObj.AddComponent<Button>();
            ColorBlock colors = button.colors;
            colors.normalColor = image.color;
            colors.highlightedColor = new Color(0.10f, 0.38f, 0.55f, 1f);
            colors.pressedColor = new Color(0.14f, 0.48f, 0.68f, 1f);
            button.colors = colors;

            RectTransform rect = buttonObj.GetComponent<RectTransform>();
            rect.sizeDelta = size;
            rect.anchoredPosition = anchoredPos;

            Text text = CreateText("Text", buttonObj.transform, label, 18, TextAnchor.MiddleCenter);
            RectTransform textRect = text.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            return button;
        }

        private static Text CreateText(string name, Transform parent, string value, int size, TextAnchor alignment)
        {
            GameObject textObj = CreateUiObject(name, parent, false);
            Text text = textObj.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = size;
            text.alignment = alignment;
            text.color = Color.white;
            text.text = value;
            return text;
        }

        private static GameObject CreateUiObject(string name, Transform parent, bool withImage)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            if (withImage)
            {
                go.AddComponent<Image>();
            }

            go.transform.SetParent(parent, false);
            return go;
        }

        private static Transform FindByPath(Transform root, string path)
        {
            return root != null ? root.Find(path) : null;
        }

        private static Text FindText(Transform root, string path)
        {
            Transform t = FindByPath(root, path);
            return t != null ? t.GetComponent<Text>() : null;
        }
    }
}

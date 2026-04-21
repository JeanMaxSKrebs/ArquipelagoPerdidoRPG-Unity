using System;
using UnityEngine;
using UnityEngine.UI;

namespace ArquipelagoPerdidoRPG.Inventory
{
    public class InventorySlotUI : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private Text nameText;
        [SerializeField] private Text quantityText;
        [SerializeField] private Image background;

        private InventoryItem _item;
        private Action<InventoryItem> _onClick;

        public void Bind(InventoryItem item, bool selected, Action<InventoryItem> onClick)
        {
            _item = item;
            _onClick = onClick;

            EnsureReferences();

            if (nameText != null)
            {
                nameText.text = item != null ? item.DisplayName : "-";
            }

            if (quantityText != null)
            {
                quantityText.text = item != null ? $"x{Mathf.Max(1, item.quantity)}" : string.Empty;
            }

            if (background != null)
            {
                background.color = selected ? new Color(0.10f, 0.42f, 0.60f, 0.95f) : new Color(0.05f, 0.15f, 0.24f, 0.88f);
            }
        }

        private void Awake()
        {
            EnsureReferences();
        }

        private void OnEnable()
        {
            if (button != null)
            {
                button.onClick.AddListener(HandleClick);
            }
        }

        private void OnDisable()
        {
            if (button != null)
            {
                button.onClick.RemoveListener(HandleClick);
            }
        }

        private void HandleClick()
        {
            _onClick?.Invoke(_item);
        }

        private void EnsureReferences()
        {
            button ??= GetComponent<Button>();
            background ??= GetComponent<Image>();
            nameText ??= FindByPath("Text_Name");
            quantityText ??= FindByPath("Text_Quantity");
        }

        private Text FindByPath(string path)
        {
            Transform node = transform.Find(path);
            if (node == null)
            {
                return null;
            }

            return node.GetComponent<Text>();
        }
    }
}

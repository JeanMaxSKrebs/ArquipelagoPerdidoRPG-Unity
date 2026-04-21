using System;

namespace ArquipelagoPerdidoRPG.Inventory
{
    [Serializable]
    public class InventoryItem
    {
        public InventoryItemData data;
        public int quantity = 1;

        public string DisplayName => data != null ? data.displayName : "Item";
        public string Description => data != null ? data.description : string.Empty;
        public ItemCategory Category => data != null ? data.category : ItemCategory.SpecialItems;
        public ItemType Type => data != null ? data.type : ItemType.Special;

        public bool CanStackWith(InventoryItemData candidate)
        {
            return data != null && candidate != null && data.id == candidate.id && data.stackable;
        }

        public string GetPrimaryActionLabel()
        {
            switch (Category)
            {
                case ItemCategory.Consumables:
                    return "Usar";
                case ItemCategory.Weapons:
                    return "Equipar";
                case ItemCategory.Materials:
                    return "Inspecionar";
                case ItemCategory.Tools:
                    return "Preparar";
                case ItemCategory.SpecialItems:
                    return "Ativar";
                case ItemCategory.QuestItems:
                    return "Visualizar";
                default:
                    return "Selecionar";
            }
        }
    }
}

using System;

namespace ArquipelagoPerdidoRPG.Inventory
{
    [Serializable]
    public class InventoryItemData
    {
        public string id;
        public string displayName;
        public string description;
        public ItemCategory category;
        public ItemType type;
        public bool stackable = true;
        public int maxStack = 99;
        public float primaryValue;
    }
}

using System.Collections.Generic;

[System.Serializable]
public class InventorySaveData
{
    public List<ItemData> items = new List<ItemData>();
    public List<ConsumableData> consumables = new List<ConsumableData>();
    public ItemData equippedWeapon;
    public ItemData equippedChest;
    public ItemData equippedLegs;
    public ItemData equippedHelm;
    public ItemData equippedGloves;
    public ItemData equippedBoots;
}

[System.Serializable]
public class WarehouseSaveData
{
    public List<ItemData> stored = new List<ItemData>();
}
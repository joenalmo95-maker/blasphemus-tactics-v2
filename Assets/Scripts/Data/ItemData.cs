[System.Serializable]
public class ItemData
{
    public string itemName = "Objeto";
    public ItemSlot slot = ItemSlot.Weapon;
    public Rarity rarity = Rarity.Common;
    public string requiredClass = "";
    public StatBlock stats = new StatBlock();
}

public enum ItemSlot { Weapon, Chest, Legs, Helm, Gloves }
public enum Rarity { Common, Rare, Epic, Legendary }
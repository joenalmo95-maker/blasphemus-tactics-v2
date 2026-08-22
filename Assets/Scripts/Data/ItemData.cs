[System.Serializable]
public class ItemData
{
    public string itemName = "Objeto";
    public ItemSlot slot = ItemSlot.Weapon;
    public Rarity rarity = Rarity.Common;
    public string requiredClass = "";
    // 4.3: tipo de armadura (Ninguna para armas y items legacy)
    public ArmorType armorType = ArmorType.Ninguna;
    public StatBlock stats = new StatBlock();
}

public enum ItemSlot { Weapon, Chest, Legs, Helm, Gloves }
public enum Rarity { Common, Rare, Epic, Legendary, Reliquia }
public enum ArmorType { Ninguna, Placas, Cuero, Ropa }
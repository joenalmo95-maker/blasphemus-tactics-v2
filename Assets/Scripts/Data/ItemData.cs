[System.Serializable]
public class ItemData
{
    public string itemName = "Objeto";
    public ItemSlot slot = ItemSlot.Weapon;
    public Rarity rarity = Rarity.Common;
    public string requiredClass = "";
    // 4.3: tipo de armadura (Ninguna para armas y items legacy)
    public ArmorType armorType = ArmorType.Ninguna;
    // 0.7-E: pertenencia a set de armadura
    public SetType setId = SetType.Ninguno;
    public SetPieceType setPiece = SetPieceType.Ninguna;
    public StatBlock stats = new StatBlock();
}

public enum ItemSlot { Weapon, Chest, Legs, Helm, Gloves, Boots }
public enum Rarity { Common, Rare, Epic, Legendary, Reliquia }
public enum ArmorType { Ninguna, Placas, Cuero, Ropa }
// 0.7-E: sets de armadura (3 sets, 4 piezas cada uno)
public enum SetType { Ninguno, Rojo, Amarillo, Verde }
public enum SetPieceType { Ninguna, Casco, Peto, Pantalon, Guantes }
using UnityEngine;
using System.Collections.Generic;

public class InventorySystem : MonoBehaviour
{
    public static InventorySystem Instance { get; private set; }

    public List<ItemData> items = new List<ItemData>();
    private Dictionary<ItemSlot, ItemData> equipped = new Dictionary<ItemSlot, ItemData>();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public ItemData GetEquipped(ItemSlot slot)
    {
        return equipped.TryGetValue(slot, out ItemData item) ? item : null;
    }

    public void AddItem(ItemData item)
    {
        items.Add(item);
        Debug.Log("Objeto obtenido: " + item.itemName + " [" + item.rarity + "]");
    }

    public void Equip(int index)
    {
        if (index < 0 || index >= items.Count) return;
        ItemData item = items[index];

        if (CharacterData.Instance != null && CharacterData.Instance.classData != null
            && item.requiredClass != "" && item.requiredClass != CharacterData.Instance.classData.className)
        {
            Debug.Log("Tu clase no puede equipar " + item.itemName);
            return;
        }

        items.RemoveAt(index);
        if (equipped.ContainsKey(item.slot))
        {
            items.Add(equipped[item.slot]);
        }
        equipped[item.slot] = item;
        Debug.Log("Equipado: " + item.itemName);
        ApplyToUnit();
    }

    public void Unequip(ItemSlot slot)
    {
        if (!equipped.ContainsKey(slot)) return;
        items.Add(equipped[slot]);
        equipped.Remove(slot);
        Debug.Log("Desequipado slot: " + slot);
        ApplyToUnit();
    }

    public StatBlock GetEquippedStats()
    {
        StatBlock s = StatBlock.Zero();
        foreach (var kv in equipped)
        {
            s.Add(kv.Value.stats);
        }
        return s;
    }

    public void ApplyToUnit()
    {
        if (CharacterData.Instance == null) return;
        StatBlock total = CharacterData.Instance.GetTotalStats();

        Unit[] units = FindObjectsByType<Unit>(FindObjectsInactive.Exclude);
        foreach (Unit u in units)
        {
            if (!u.isEnemy)
            {
                u.maxHealth = total.maxHP;
                if (u.currentHealth > u.maxHealth) u.currentHealth = u.maxHealth;
                u.maxAP = total.apMove;
                if (u.currentAP > u.maxAP) u.currentAP = u.maxAP;
                u.stats = total.Clone();
                Debug.Log("Stats aplicados al Renacido. HP max: " + u.maxHealth + " | AP: " + u.maxAP);
                break;
            }
        }
    }
}
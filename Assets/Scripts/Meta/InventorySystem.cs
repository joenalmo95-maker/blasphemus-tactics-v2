using UnityEngine;
using System.Collections.Generic;

public class InventorySystem : MonoBehaviour
{
    public static InventorySystem Instance { get; private set; }

    public List<ItemData> items = new List<ItemData>();
    public List<ConsumableData> consumables = new List<ConsumableData>();
    private Dictionary<ItemSlot, ItemData> equipped = new Dictionary<ItemSlot, ItemData>();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public ItemData GetEquipped(ItemSlot slot)
    {
        return equipped.TryGetValue(slot, out ItemData item) ? item : null;
    }

    public List<ItemData> GetAllEquipped()
    {
        return new List<ItemData>(equipped.Values);
    }

    public void LoadFrom(SaveData data)
    {
        items = data.items != null ? data.items : new List<ItemData>();
        consumables = data.consumables != null ? data.consumables : new List<ConsumableData>();
        equipped.Clear();
        if (data.equipped != null)
        {
            foreach (ItemData it in data.equipped)
            {
                equipped[it.slot] = it;
            }
        }
        ApplyToUnit();
    }

    public void AddItem(ItemData item)
    {
        items.Add(item);
        Debug.Log("Objeto obtenido: " + item.itemName + " [" + item.rarity + "]");
    }

    public void SellItem(int index)
    {
        if (index < 0 || index >= items.Count) return;
        ItemData item = items[index];
        int price = ItemGenerator.SellPrice(item);
        items.RemoveAt(index);

        if (CharacterData.Instance != null)
        {
            CharacterData.Instance.gold += price;
            Debug.Log("Vendido: " + item.itemName + " (+" + price + " oro)");
        }
    }    

    public int GetConsumableCount(ConsumableType t)
    {
        foreach (ConsumableData c in consumables)
        {
            if (c.type == t) return c.count;
        }
        return 0;
    }

    public void AddConsumable(ConsumableType t, int count = 1)
    {
        foreach (ConsumableData c in consumables)
        {
            if (c.type == t)
            {
                c.count += count;
                return;
            }
        }
        consumables.Add(new ConsumableData { type = t, count = count });
    }

    // 1.4-fix: Poción de Vida cuesta 3 AP (o se cura o se mueve)
    // 1.4-fix: Comidas con cooldown global de 10 minutos (tiempo real)
    private static long lastFoodUseTimestamp = 0;
    private const long FOOD_COOLDOWN_TICKS = 10L * 60L * 10000000L; // 10 minutos en ticks

    public bool UseConsumable(ConsumableType t)
    {
        if (TurnManager.Instance != null && !TurnManager.Instance.IsPlayerTurn())
        {
            Debug.Log("Solo puedes usar consumibles en tu turno.");
            return false;
        }

        ConsumableData entry = null;
        foreach (ConsumableData c in consumables)
        {
            if (c.type == t && c.count > 0) { entry = c; break; }
        }

        if (entry == null)
        {
            Debug.Log("No tienes " + ConsumableCatalog.Name(t) + ".");
            return false;
        }

        Unit player = null;
        Unit[] units = FindObjectsByType<Unit>(FindObjectsInactive.Exclude);
        foreach (Unit u in units)
        {
            if (!u.isEnemy) { player = u; break; }
        }
        if (player == null) return false;

        // 1.4-B: Poción de Vida requiere 3 AP y cura 30% HP máx + 30
        if (t == ConsumableType.PocionHP)
        {
            if (player.currentAP < 3)
            {
                Debug.Log("Poción de Vida requiere 3 AP. Tienes: " + player.currentAP);
                return false;
            }
            player.currentAP -= 3;
            int healAmount = Mathf.RoundToInt(player.maxHealth * 0.3f) + 30;
            player.Heal(healAmount);
            Debug.Log("Poción de Vida usada (-3 AP). HP curado: " + healAmount + ". AP restantes: " + player.currentAP);
        }
        // 1.4-fix: Comidas con cooldown global de 10 minutos
        else if (t == ConsumableType.ComidaDano || t == ConsumableType.ComidaDefensa)
        {
            long now = System.DateTime.UtcNow.Ticks;
            long elapsed = now - lastFoodUseTimestamp;
            if (elapsed < FOOD_COOLDOWN_TICKS)
            {
                long remainingTicks = FOOD_COOLDOWN_TICKS - elapsed;
                int remainingMinutes = (int)(remainingTicks / 10000000L / 60L);
                int remainingSeconds = (int)((remainingTicks / 10000000L) % 60L);
                Debug.Log("Comida en cooldown: " + remainingMinutes + "m " + remainingSeconds + "s restantes.");
                CombatFeedback.SpawnText(player.transform.position, "COMIDA EN CD: " + remainingMinutes + "m", Color.red);
                return false;
            }

            // Aplicar buff de comida (dura 10 minutos en tiempo real, no turnos)
            if (t == ConsumableType.ComidaDano)
            {
                player.worldBuffDamage = 2;
                player.worldBuffDamageExpiry = now + FOOD_COOLDOWN_TICKS;
                Debug.Log("Comida Picante usada: +2 daño por 10 minutos.");
            }
            else
            {
                player.worldBuffDefense = 2;
                player.worldBuffDefenseExpiry = now + FOOD_COOLDOWN_TICKS;
                Debug.Log("Comida de Hierro usada: +2 defensa por 10 minutos.");
            }

            lastFoodUseTimestamp = now;
        }

        entry.count--;
        if (entry.count <= 0) consumables.Remove(entry);
        Debug.Log("Usado: " + ConsumableCatalog.Name(t) + ". Restantes: " + GetConsumableCount(t));
        return true;
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
        // FIX: Si ya hay algo equipado en ese slot, devolverlo al inventario
        if (equipped.ContainsKey(item.slot))
        {
            items.Add(equipped[item.slot]);
        }
        // FIX CRÍTICO: Agregar el nuevo item al diccionario equipped
        equipped[item.slot] = item;
        Debug.Log("Equipado: " + item.itemName + " en slot " + item.slot);
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
        
        // Fix: Cap de AP máximo (base 3 + máximo 3 de items = 6 razonable)
        total.apMove = Mathf.Clamp(total.apMove, 1, 6);

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

    // 0.3: Método Serialize para SaveSystem
    public InventorySaveData Serialize()
    {
        InventorySaveData data = new InventorySaveData();
        data.items = items;
        data.consumables = consumables;
        data.equippedWeapon = GetEquipped(ItemSlot.Weapon);
        data.equippedChest = GetEquipped(ItemSlot.Chest);
        data.equippedLegs = GetEquipped(ItemSlot.Legs);
        data.equippedHelm = GetEquipped(ItemSlot.Helm);
        data.equippedGloves = GetEquipped(ItemSlot.Gloves);
        data.equippedBoots = GetEquipped(ItemSlot.Boots);
        return data;
    }

    // 0.3: Método Deserialize para SaveSystem
    public void Deserialize(InventorySaveData data)
    {
        if (data == null) return;
        items = data.items != null ? data.items : new List<ItemData>();
        consumables = data.consumables != null ? data.consumables : new List<ConsumableData>();
        equipped.Clear();
        
        // Fix: Filtrar items dummy/corruptos (nombre "Objeto" o slot incorrecto)
        if (IsValidItem(data.equippedWeapon)) equipped[ItemSlot.Weapon] = data.equippedWeapon;
        if (IsValidItem(data.equippedChest))  equipped[ItemSlot.Chest]  = data.equippedChest;
        if (IsValidItem(data.equippedLegs))   equipped[ItemSlot.Legs]   = data.equippedLegs;
        if (IsValidItem(data.equippedHelm))   equipped[ItemSlot.Helm]   = data.equippedHelm;
        if (IsValidItem(data.equippedGloves)) equipped[ItemSlot.Gloves] = data.equippedGloves;
        if (IsValidItem(data.equippedBoots)) equipped[ItemSlot.Boots] = data.equippedBoots;
        
        // Limpiar items dummy del inventario normal también
        items.RemoveAll(i => !IsValidItem(i));
        
        ApplyToUnit();
    }
    
    // Validar que un item no sea dummy/corrupto
    bool IsValidItem(ItemData item)
    {
        if (item == null) return false;
        if (string.IsNullOrEmpty(item.itemName)) return false;
        if (item.itemName == "Objeto") return false;
        if (item.stats == null) return false;
        return true;
    }
    // Reset para NUEVA PARTIDA - quita todo el equipamiento
    public void UnequipAll()
    {
        equipped.Clear();
        Debug.Log("[InventorySystem] Todo el equipamiento ha sido removido.");
    }

}
  
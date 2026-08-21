using UnityEngine;
using System.Collections.Generic;

// Sistema centralizado de reset para "NUEVA PARTIDA"
public static class GameReset
{
    public static void ResetAll()
    {
        Debug.Log("[GameReset] Iniciando reset completo para NUEVA PARTIDA...");
        
        // 1. Resetear flag de sesión de SaveSystem
        SaveSystem.HasLoadedThisSession = false;
        
        // 2. Resetear CharacterData (nivel, XP, oro, stats)
        if (CharacterData.Instance != null)
        {
            CharacterData.Instance.level = 1;
            CharacterData.Instance.xp = 0;
            CharacterData.Instance.gold = 100;
            CharacterData.Instance.playerName = "Valerius";
            Debug.Log("[GameReset] CharacterData reseteado a nivel 1.");
        }
        
        // 3. Resetear Inventario y Almacén
        if (InventorySystem.Instance != null)
        {
            InventorySystem.Instance.items.Clear();
            if (InventorySystem.Instance.consumables != null)
                InventorySystem.Instance.consumables.Clear();
            InventorySystem.Instance.UnequipAll();
            Debug.Log("[GameReset] Inventario y equipamiento vaciados.");
        }
        
        if (WarehouseSystem.Instance != null)
        {
            WarehouseSystem.Instance.stored.Clear();
            Debug.Log("[GameReset] Almacén vaciado.");
        }
        
        // 4. Resetear Loadout (skills aprendidas) - solo golpe_basico
        LoadoutSystem.ResetToStarter();
        
        // 5. Resetear QuestSystem
        QuestSystem.ResetForNewGame();
        
        // 6. Resetear DungeonDaily
        DungeonDaily.ResetToday();
        
        // 7. Limpiar buffs/debuffs del jugador si existe una Unit viva
        Unit[] units = Object.FindObjectsByType<Unit>(FindObjectsInactive.Exclude);
        foreach (Unit u in units)
        {
            if (!u.isEnemy)
            {
                u.maxAP = 3;
                u.currentAP = 3;
                u.buffDamage = 0;
                u.buffDefense = 0;
                u.buffCrit = 0;
                u.buffTurns = 0;
                u.debuffAttack = 0;
                u.debuffTurns = 0;
                u.pendingApPenalty = 0;
                
                // Restaurar HP a máximo base
                StatBlock derived = CharacterData.Instance != null 
                    ? CharacterData.Instance.GetDerivedStats() 
                    : new StatBlock();
                u.maxHealth = derived.maxHP > 0 ? derived.maxHP : 250;
                u.currentHealth = u.maxHealth;
                u.stats = derived;
                
                Debug.Log("[GameReset] Unit del jugador reseteada. AP: 3/3, HP: " + u.maxHealth);
            }
        }
        
        Debug.Log("[GameReset] Reset completo finalizado.");
    }
}
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class WorldDungeonMarkers : MonoBehaviour
{
    private Text promptText;
    private WorldBootstrap.ZoneDef nearZone;

    void Awake()
    {
        BuildPrompt();
        SpawnMarkers();
    }

    void BuildPrompt()
    {
        GameObject canvas = UIFactory.CreateCanvas("DungeonPromptCanvas", 43);
        promptText = UIFactory.CreateText(canvas.transform, "DungeonPrompt", "", 18, TextAnchor.MiddleCenter,
            new Color(0.7f, 0.3f, 0.9f),
            new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0),
            new Vector2(0, 240), new Vector2(800, 40));
    }

    void SpawnMarkers()
    {
        foreach (WorldBootstrap.ZoneDef zone in WorldBootstrap.Zones)
        {
            GameObject marker = new GameObject("DungeonMarker_" + zone.name);
            marker.transform.position = new Vector3(zone.center.x, zone.center.y, 0);
            
            SpriteRenderer sr = marker.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteFactory.Square();
            sr.color = GetTierColor(zone.tier);
            sr.sortingOrder = 1;
            marker.transform.localScale = new Vector3(1.2f, 1.2f, 1f);

            // Borde del marcador
            GameObject border = new GameObject("Border");
            border.transform.SetParent(marker.transform);
            border.transform.localPosition = Vector3.zero;
            SpriteRenderer brdSr = border.AddComponent<SpriteRenderer>();
            brdSr.sprite = SpriteFactory.Square();
            brdSr.color = Color.black;
            brdSr.sortingOrder = 0;
            border.transform.localScale = new Vector3(1.4f, 1.4f, 1f);
        }
        Debug.Log("[DungeonMarkers] " + WorldBootstrap.Zones.Count + " marcadores de mazmorra creados.");
    }

    Color GetTierColor(EnemyTier tier)
    {
        switch (tier)
        {
            case EnemyTier.Basico: return new Color(0.4f, 0.8f, 0.4f, 0.7f); // verde
            case EnemyTier.Medio: return new Color(0.3f, 0.7f, 0.9f, 0.7f); // azul
            case EnemyTier.Elite: return new Color(0.9f, 0.7f, 0.2f, 0.7f); // dorado
            case EnemyTier.EliteFuerte: return new Color(0.9f, 0.4f, 0.2f, 0.7f); // naranja
            case EnemyTier.Jefe: return new Color(0.9f, 0.2f, 0.2f, 0.7f); // rojo
            default: return Color.gray;
        }
    }

    void Update()
    {
        WorldPlayerController pc = Object.FindAnyObjectByType<WorldPlayerController>();
        if (pc == null) { promptText.text = ""; return; }

        Vector2Int myCell = new Vector2Int(Mathf.RoundToInt(pc.transform.position.x), Mathf.RoundToInt(pc.transform.position.y));

        nearZone = null;
        foreach (WorldBootstrap.ZoneDef zone in WorldBootstrap.Zones)
        {
            if (Mathf.Abs(zone.center.x - myCell.x) <= 2 && Mathf.Abs(zone.center.y - myCell.y) <= 2)
            {
                nearZone = zone;
                break;
            }
        }

        if (nearZone != null)
        {
            promptText.text = "Pulsa E: " + nearZone.name + " (" + nearZone.tier + ")";
            if (Input.GetKeyDown(KeyCode.E)) EnterDungeon(nearZone);
        }
        else
        {
            promptText.text = "";
        }
    }

    void EnterDungeon(WorldBootstrap.ZoneDef zone)
    {
        // Validar límite diario ANTES de mostrar la tarjeta
        if (!DungeonDaily.CanEnter())
        {
            Debug.Log("[DungeonMarkers] Límite diario alcanzado (" + DungeonDaily.MaxPerDay + " mazmorras).");
            // Mostrar feedback visual temporal
            if (promptText != null)
            {
                promptText.text = "LÍMITE DIARIO ALCANZADO (" + DungeonDaily.MaxPerDay + "/5). Vuelve mañana.";
            }
            return;
        }

        // Capturar la zona para el callback
        WorldBootstrap.ZoneDef capturedZone = zone;

        // Mostrar la tarjeta de información con callback de confirmación
        DungeonCardUI.Show(zone, () =>
        {
            // Este callback se ejecuta solo si el jugador presiona "ENTRAR" en la tarjeta
            Debug.Log("[DungeonMarkers] Entrada confirmada a: " + capturedZone.name);
            // 0.7-E.4: guardar la zona para el sistema de drops de set al ganar
            GameFlow.pendingZone = capturedZone;
            
            // Consumir la entrada diaria
            DungeonDaily.Consume();
            
            // 0.7-E.4: guardar la zona para el sistema de drops de set al ganar
            GameFlow.pendingZone = capturedZone;

            // Convertir List<WorldBootstrap.WaveDef> a List<WaveDef> (clase global de GameFlow)
            List<WaveDef> convertedWaves = new List<WaveDef>();
            foreach (WorldBootstrap.WaveDef wbWave in capturedZone.dungeon)
            {
                WaveDef globalWave = new WaveDef();
                foreach (WorldBootstrap.SpawnDef wbSpawn in wbWave.spawns)
                {
                    globalWave.spawns.Add(new SpawnDef
                    {
                        archetype = wbSpawn.archetype,
                        tier = wbSpawn.tier,
                        cell = wbSpawn.cell
                    });
                }
                convertedWaves.Add(globalWave);
            }
            
            GameFlow.pendingIsWorld = false;
            GameFlow.EnterCombat(capturedZone.tier, convertedWaves);
        });
    }
}
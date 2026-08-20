using UnityEngine;
using UnityEngine.UI;

public class WorldPlayerController : MonoBehaviour
{
    public float speed = 5f;
    private Text promptText;

    void Awake()
    {
        GameObject canvas = UIFactory.CreateCanvas("WorldPromptCanvas", 44);
        promptText = UIFactory.CreateText(canvas.transform, "WorldPrompt", "", 16, TextAnchor.MiddleCenter,
            new Color(1f, 0.9f, 0.4f),
            new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0),
            new Vector2(0, 8), new Vector2(900, 24));
    }

    void Update()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");
        Vector3 dir = new Vector3(x, y, 0).normalized;
        if (dir != Vector3.zero)
        {
            Vector3 newPos = transform.position + dir * speed * Time.deltaTime;
            Vector2Int targetCell = new Vector2Int(Mathf.RoundToInt(newPos.x), Mathf.RoundToInt(newPos.y));
            if (TerrainMap.IsWalkable(targetCell))
                transform.position = newPos;
        }

        // Límites del mundo expandido 120x80
        transform.position = new Vector3(
            Mathf.Clamp(transform.position.x, 0, WorldBootstrap.WorldWidth - 1),
            Mathf.Clamp(transform.position.y, 0, WorldBootstrap.WorldHeight - 1),
            0);

        // Actualizar posición conocida
        WorldBootstrap.LastKnownPosition = new Vector2Int(
            Mathf.RoundToInt(transform.position.x),
            Mathf.RoundToInt(transform.position.y));

        // Cámara sigue al jugador
        if (Camera.main != null)
            Camera.main.transform.position = new Vector3(transform.position.x, transform.position.y, -10);

        UpdatePrompt();
    }

    void UpdatePrompt()
    {
        Vector2Int myCell = new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y));

        // Portal a la ciudad
        if (Mathf.Abs(myCell.x - WorldBootstrap.CityPortal.x) <= 1 &&
            Mathf.Abs(myCell.y - WorldBootstrap.CityPortal.y) <= 1)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                PlayerPrefs.SetInt("LastWorldX", myCell.x);
                PlayerPrefs.SetInt("LastWorldY", myCell.y);
                GameFlow.EnterCity();
            }
            promptText.text = "Pulsa E para entrar al Bastión de San Veritas";
        }
        else
        {
            promptText.text = "VALLE DE LA LUZ ETERNA (WASD mover · M mapa · I inventario)";
        }
    }

}
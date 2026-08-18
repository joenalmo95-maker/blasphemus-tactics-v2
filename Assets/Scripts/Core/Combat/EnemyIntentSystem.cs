using UnityEngine;

public enum IntentType { Ninguna, Atacar, Mover, Embestir, Maldecir }

// 1.1-E: predicción telegrafiada de la próxima acción enemiga (icono sobre la cabeza)
public static class EnemyIntentSystem
{
    public static void DecideAll()
    {
        Unit player = null;
        Unit[] all = Object.FindObjectsByType<Unit>(FindObjectsInactive.Exclude);
        foreach (Unit u in all)
        {
            if (!u.isEnemy) { player = u; break; }
        }
        if (player == null) return;

        foreach (Unit e in all)
        {
            if (!e.isEnemy || e.currentHealth <= 0) continue;

            int dist = Mathf.Abs(e.currentGridPos.x - player.currentGridPos.x) +
                       Mathf.Abs(e.currentGridPos.y - player.currentGridPos.y);

            EnemyAI ai = e.GetComponent<EnemyAI>();
            int range = ai != null ? ai.attackRange : 1;

            IntentType t;
            if (ai != null && ai.applyCurse && dist <= range) t = IntentType.Maldecir;
            else if (dist <= range) t = IntentType.Atacar;
            else if (ai != null && ai.canCharge) t = IntentType.Embestir;
            else t = IntentType.Mover;

            e.intent = t;
            e.UpdateFacing(new Vector2(player.currentGridPos.x - e.currentGridPos.x,
                                       player.currentGridPos.y - e.currentGridPos.y).normalized);
            ShowIcon(e);
        }
    }

    static void ShowIcon(Unit e)
    {
        Transform old = e.transform.Find("IntentIcon");
        if (old != null) Object.Destroy(old.gameObject);

        GameObject go = new GameObject("IntentIcon");
        go.transform.SetParent(e.transform, false);
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteFactory.Circle();
        sr.sortingOrder = 6;
        go.transform.localScale = Vector3.one * 0.22f;
        go.transform.localPosition = new Vector3(0, 0.75f, -0.2f);

        switch (e.intent)
        {
            case IntentType.Atacar: sr.color = Color.red; break;
            case IntentType.Mover: sr.color = Color.blue; break;
            case IntentType.Embestir: sr.color = Color.yellow; break;
            case IntentType.Maldecir: sr.color = Color.magenta; break;
            default: sr.color = Color.gray; break;
        }
    }
}
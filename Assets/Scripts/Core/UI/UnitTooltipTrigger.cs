using UnityEngine;
using UnityEngine.EventSystems;

public class UnitTooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Unit targetUnit;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (targetUnit != null && TooltipUI.Instance != null)
        {
            TooltipUI.Instance.ShowUnitTooltip(targetUnit);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (TooltipUI.Instance != null)
        {
            TooltipUI.Instance.Hide();
        }
    }

    // FIX: misma protección si la unidad muere con el cursor sobre su barra de vida.
    void OnDestroy()
    {
        if (TooltipUI.Instance != null) TooltipUI.Instance.Hide();
    }
}
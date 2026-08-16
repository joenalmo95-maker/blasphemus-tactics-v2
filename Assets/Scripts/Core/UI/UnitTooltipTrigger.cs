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
}
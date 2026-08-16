using UnityEngine;
using UnityEngine.EventSystems;

public class ItemTooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public ItemData item;
    public bool compareWithEquipped = false;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (item == null || TooltipUI.Instance == null) return;

        ItemData equipped = null;
        if (compareWithEquipped && InventorySystem.Instance != null)
            equipped = InventorySystem.Instance.GetEquipped(item.slot);

        TooltipUI.Instance.ShowItemTooltip(item, equipped);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (TooltipUI.Instance != null) TooltipUI.Instance.Hide();
    }
}
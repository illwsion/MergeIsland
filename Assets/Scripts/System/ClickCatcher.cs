using UnityEngine;
using UnityEngine.EventSystems;

public class ClickCatcher : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        if (ItemSelectorManager.Instance.HasSelection())
        {
            ItemSelectorManager.Instance.ClearSelection();
        }
    }
}

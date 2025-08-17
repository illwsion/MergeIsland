// EmptyCellCatcher.cs
using UnityEngine;
using UnityEngine.EventSystems;

public class EmptyCellCatcher : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        // 자식에 ItemView가 없으면 빈 칸으로 간주
        ItemView itemView = GetComponentInChildren<ItemView>();
        if (itemView == null)
        {
            ItemSelectorManager.Instance.ClearSelection();
        }
    }
}
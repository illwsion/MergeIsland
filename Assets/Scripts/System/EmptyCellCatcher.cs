// EmptyCellCatcher.cs
using UnityEngine;
using UnityEngine.EventSystems;

public class EmptyCellCatcher : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        // �ڽĿ� ItemView�� ������ �� ĭ���� ����
        ItemView itemView = GetComponentInChildren<ItemView>();
        if (itemView == null)
        {
            Debug.Log("[EmptyCellCatcher] �� �� Ŭ�� �� ���� ����");
            ItemSelectorManager.Instance.ClearSelection();
        }
    }
}
// DraggableItem.cs
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Canvas canvas;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;

    private Transform originalParent;
    private Vector2 originalPosition;
    private Vector2Int fromPos; // �巡�� ���� ��ġ

    public MergeItem mergeItem; // ����� ������ ����

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        canvas = GetComponentInParent<Canvas>();
    }

    public void SetOrigin(Vector2Int position)
    {
        fromPos = position;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalParent = transform.parent;
        originalPosition = rectTransform.anchoredPosition;

        transform.SetParent(canvas.transform);
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        transform.SetParent(originalParent);
        canvasGroup.blocksRaycasts = true;

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (var result in results)
        {
            DropTarget dropTarget = result.gameObject.GetComponent<DropTarget>();
            if (dropTarget != null)
            {
                Debug.Log($"����� ��: ({dropTarget.x}, {dropTarget.y})");
                Vector2Int toPos = new Vector2Int(dropTarget.x, dropTarget.y);
                BoardManager.Instance.HandleDrop(mergeItem, fromPos, toPos);
                return;
            }
        }

        // ���� �� �ִ� ���� �ƴϸ� ����ġ ����
        rectTransform.anchoredPosition = originalPosition;
    }
}
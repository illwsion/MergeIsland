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
    private Vector2Int fromPos; // 드래그 시작 위치

    public MergeItem mergeItem; // 연결된 아이템 정보

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
        if (!mergeItem.Data.canMove)
        {
            eventData.pointerDrag = null;
            return;
        }

        // 애니메이션 중인 아이템은 드래그 방지
        if (ItemAnimationManager.Instance != null && 
            ItemAnimationManager.Instance.HasActiveAnimation(mergeItem))
        {
            eventData.pointerDrag = null;
            return;
        }

        // 진행 중인 모든 애니메이션 종료 (UI 초기화 없이)
        if (ItemAnimationManager.Instance != null)
        {
            ItemAnimationManager.Instance.StopAllAnimations();
        }

        DragManager.Instance.StartDrag(mergeItem, fromPos);

        originalParent = transform.parent;
        originalPosition = rectTransform.anchoredPosition;

        transform.SetParent(canvas.transform);
        canvasGroup.blocksRaycasts = false;

        // 드래그 시작 시, 아직 선택되지 않았다면 자동 선택
        if (!ItemSelectorManager.Instance.HasSelection() ||
            ItemSelectorManager.Instance.GetSelectedItem() != mergeItem)
        {
            var view = GetComponent<ItemView>();
            if (view != null)
            {
                ItemSelectorManager.Instance.SelectWithoutBoardUpdate(view);
            }
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        // 마우스 위치를 월드 좌표로 변환
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(eventData.position);
        mouseWorldPos.z = rectTransform.position.z; // Z 좌표 유지
        
        // 아이템의 중앙이 마우스 위치에 오도록 설정
        rectTransform.position = mouseWorldPos;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        DragManager.Instance.EndDrag();

        transform.SetParent(originalParent);
        canvasGroup.blocksRaycasts = true;

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (var result in results)
        {
            BoardGate gate = result.gameObject.GetComponent<BoardGate>();
            if (gate != null)
            {
                BoardManager.Instance.HandleGateDrop(mergeItem, gate, fromPos);
                return;
            }

            DropTarget dropTarget = result.gameObject.GetComponent<DropTarget>();
            if (dropTarget != null)
            {
                Vector2Int toPos = new Vector2Int(dropTarget.x, dropTarget.y);
                BoardManager.Instance.HandleDrop(mergeItem, fromPos, toPos, GetComponent<ItemView>());
                return;
            }
        }

        // 드래그 실패 -> 제자리 처리로 선택
        BoardManager.Instance.HandleDrop(mergeItem, fromPos, fromPos, GetComponent<ItemView>());
    }
}
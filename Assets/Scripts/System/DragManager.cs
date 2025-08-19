// DragManager.cs
using UnityEngine;

public class DragManager : MonoBehaviour
{
    public static DragManager Instance { get; private set; }
    public bool IsDragging { get; private set; }
    public GameObject inputBlocker;
    
    // 드래그 중인 아이템의 원래 위치 정보
    private Vector2Int? draggingItemPosition = null;
    private MergeItem draggingItem = null;

    void Update()
    {
        inputBlocker.SetActive(DragManager.Instance.IsDragging);
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void StartDrag(MergeItem item, Vector2Int position)
    {
        IsDragging = true;
        draggingItem = item;
        draggingItemPosition = position;
    }
    
    public void EndDrag()
    {
        IsDragging = false;
        draggingItem = null;
        draggingItemPosition = null;
    }
    
    // 드래그 중인 아이템의 원래 위치 반환
    public Vector2Int? GetDraggingItemPosition()
    {
        return draggingItemPosition;
    }
    
    // 드래그 중인 아이템 반환
    public MergeItem GetDraggingItem()
    {
        return draggingItem;
    }
    
    // 특정 위치에 드래그 중인 아이템이 있는지 확인
    public bool IsPositionBeingDragged(Vector2Int position)
    {
        return IsDragging && draggingItemPosition.HasValue && draggingItemPosition.Value == position;
    }
}

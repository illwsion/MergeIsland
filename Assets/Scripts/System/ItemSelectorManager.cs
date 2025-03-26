// ItemSelectorManager.cs
using UnityEngine;

public class ItemSelectorManager : MonoBehaviour
{
    public static ItemSelectorManager Instance;
    public ItemInfoUI itemInfoUI; // 상단 UI 참조

    private MergeItem selectedItem;
    private Vector2Int selectedCoord;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        this.ClearSelection();
    }

    public void Select(MergeItem item, Vector2Int coord)
    {
        selectedItem = item;
        selectedCoord = coord;

        Debug.Log($"아이템 선택됨: {item.type} {item.level} at {coord}");
        itemInfoUI.Show(item);
    }

    public void ClearSelection()
    {
        selectedItem = null;
        itemInfoUI.ShowEmpty();
        Debug.Log("선택 해제됨");
    }

    public bool HasSelection()
    {
        return selectedItem != null;
    }

    public MergeItem GetSelectedItem()
    {
        return selectedItem;
    }

    public Vector2Int GetSelectedCoord()
    {
        return selectedCoord;
    }
}
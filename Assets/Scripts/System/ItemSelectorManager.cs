// ItemSelectorManager.cs
using UnityEngine;

public class ItemSelectorManager : MonoBehaviour
{
    public static ItemSelectorManager Instance;
    public ItemInfoUI itemInfoUI; // ��� UI ����

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

        Debug.Log($"������ ���õ�: {item.type} {item.level} at {coord}");
        itemInfoUI.Show(item);
    }

    public void ClearSelection()
    {
        selectedItem = null;
        itemInfoUI.ShowEmpty();
        Debug.Log("���� ������");
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
// ItemSelectorManager.cs
using UnityEngine;

public class ItemSelectorManager : MonoBehaviour
{
    public static ItemSelectorManager Instance;
    public ItemInfoUI itemInfoUI; // ��� UI ����

    private MergeItem selectedItem;
    public Vector2Int selectedCoord;
    private ItemView selectedItemView;

    private void Start()
    {
        ClearSelection();
    }
    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Update()
    {
        if (selectedItem != null && itemInfoUI != null)
        {
            itemInfoUI.Refresh(selectedItem);
        }
    }

    public void Select(ItemView view)
    {
        if (selectedItemView == view)
            return;

        ClearSelection();

        selectedItemView = view;
        selectedItem = view.mergeItem;
        selectedCoord = view.coord;
        
        itemInfoUI.Show(selectedItem);

        BoardManager.Instance.RefreshBoard();
    }
    
    public void ClearSelection()
    {
        // ���� ���� ����
        selectedItem = null;
        selectedItemView = null;

        if (itemInfoUI != null)
            itemInfoUI.ShowEmpty();

        if (BoardManager.Instance != null)
            BoardManager.Instance.RefreshBoard();
    }
    
    public void ClearSelectionOnEmptyCell()
    {
        // �� �� Ŭ�� �� ���� ����
        if (selectedItemView != null)
        {
            ClearSelection();
        }
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

    public ItemView GetSelectedItemView()
    {
        return selectedItemView;
    }

    public void SetSelectedCoord(Vector2Int coord)
    {
        selectedCoord = coord;
    }
}
// ItemSelectorManager.cs
using UnityEngine;

public class ItemSelectorManager : MonoBehaviour
{
    public static ItemSelectorManager Instance;
    public ItemInfoUI itemInfoUI; // 상단 UI 참조

    private MergeItem selectedItem;
    public Vector2Int selectedCoord;
    private ItemView selectedItemView;

    public string selectedGate;

    private void Start()
    {
        if (BoardManager.Instance?.GetCurrentBoard() == null)
        {
            Debug.LogWarning("[ItemSelectorManager] BoardManager가 아직 초기화되지 않았습니다.");
            return;
        }

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
        selectedItem = null;
        selectedItemView = null;
        selectedGate = null;

        if (itemInfoUI != null)
            itemInfoUI.ShowEmpty();

        if (BoardManager.Instance != null)
            BoardManager.Instance.RefreshBoard();
    }

    public void ClearSelectedItemOnly()
    {
        selectedItem = null;
        selectedItemView = null;
    }

    public void ClearSelectionOnEmptyCell()
    {
        if (selectedItemView != null)
        {
            ClearSelection();
        }
    }

    public void SelectGate(BoardGate gate)
    {
        if (selectedGate == gate.gateData.GetUniqueID()) return;

        ClearSelectedItemOnly(); // 아이템 선택 해제
        selectedGate = gate.gateData.GetUniqueID();
        itemInfoUI.ShowGate(gate);
        BoardManager.Instance.RefreshBoard();
    }

    public void ClearSelectedGateOnly()
    {
        selectedGate = null;
        BoardManager.Instance.RefreshBoard();
    }

    public bool HasSelection()
    {
        return selectedItem != null;
    }

    public bool HasGateSelection()
    {
        return selectedGate != null;
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

    // 현재 선택된 게이트와 비교
    public bool IsGateSelected(BoardGate gate)
    {
        if (selectedGate == null || gate == null)
            return false;

        if (selectedGate.Equals(null) || gate.Equals(null))
            return false;

        return selectedGate == gate.gateData.GetUniqueID();
    }
}
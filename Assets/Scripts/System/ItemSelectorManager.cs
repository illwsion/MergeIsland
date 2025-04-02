// ItemSelectorManager.cs
using UnityEngine;

public class ItemSelectorManager : MonoBehaviour
{
    public static ItemSelectorManager Instance;
    public ItemInfoUI itemInfoUI; // 상단 UI 참조

    private MergeItem selectedItem;
    private Vector2Int selectedCoord;
    private ItemView selectedItemView;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        this.ClearSelection();
    }

    public void Select(ItemView view)
    {
        ClearSelection();

        selectedItemView = view;
        selectedItem = view.mergeItem;
        selectedCoord = view.coord;

        // 선택된 셀의 Outline 활성화
        GameObject cell = view.transform.parent.gameObject;
        Transform outline = cell.transform.Find("SelectionOutline");
        Debug.Log(outline);
        if (outline != null) outline.gameObject.SetActive(true);

        itemInfoUI.Show(selectedItem);
    }

    public void ClearSelection()
    {
        // 이전 선택 해제
        if (selectedItemView != null)
        {
            GameObject cell = selectedItemView.transform.parent.gameObject;
            Transform outline = cell.transform.Find("SelectionOutline");
            if (outline != null) outline.gameObject.SetActive(false);
        }
        selectedItem = null;
        selectedItemView = null;
        itemInfoUI.ShowEmpty();
    }

    public void ClearSelectionOnEmptyCell()
    {
        // 빈 셀 클릭 시 선택 해제
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
}
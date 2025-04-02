// ItemSelectorManager.cs
using UnityEngine;

public class ItemSelectorManager : MonoBehaviour
{
    public static ItemSelectorManager Instance;
    public ItemInfoUI itemInfoUI; // ��� UI ����

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

        // ���õ� ���� Outline Ȱ��ȭ
        GameObject cell = view.transform.parent.gameObject;
        Transform outline = cell.transform.Find("SelectionOutline");
        Debug.Log(outline);
        if (outline != null) outline.gameObject.SetActive(true);

        itemInfoUI.Show(selectedItem);
    }

    public void ClearSelection()
    {
        // ���� ���� ����
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
}
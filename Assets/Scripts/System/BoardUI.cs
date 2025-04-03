// BoardUI.cs
using System.Drawing;
using UnityEngine;
using UnityEngine.UI;

public class BoardUI : MonoBehaviour
{
    public GridLayoutGroup gridLayout;
    public RectTransform boardArea; // ��ü ���� ǥ�� ����
    public GameObject cellPrefab;
    public GameObject itemViewPrefab;

    private const float padding = 20f;
    private MergeBoard currentBoard;

    public void DisplayBoard(MergeBoard board)
    {
        currentBoard = board;
        int width = board.width;
        int height = board.height;

        float areaWidth = boardArea.rect.width - padding;
        float areaHeight = boardArea.rect.height - padding;

        float cellSize = areaWidth / width;
        if (cellSize * height > areaHeight)
        {
            cellSize = areaHeight / height;
        }

        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = width;
        gridLayout.cellSize = new Vector2(cellSize, cellSize);

        int totalCellCount = width * height;

        while (gridLayout.transform.childCount > totalCellCount)
        {
            DestroyImmediate(gridLayout.transform.GetChild(gridLayout.transform.childCount - 1).gameObject);
        }

        while (gridLayout.transform.childCount < totalCellCount)
        {
            Instantiate(cellPrefab, gridLayout.transform);
        }

        int index = 0;

        // ��ǥ ������� ���� ���� ��������
        Vector2Int? selectedCoord = null;
        if (ItemSelectorManager.Instance.HasSelection())
        {
            selectedCoord = ItemSelectorManager.Instance.GetSelectedCoord();
        }

        for (int y = height - 1; y >= 0; y--)
        {
            for (int x = 0; x < width; x++)
            {
                if (index >= gridLayout.transform.childCount)
                {
                    Debug.LogError($"[BoardUI] index �ʰ�! index={index}, count={gridLayout.transform.childCount}");
                    return;
                }

                Transform cell = gridLayout.transform.GetChild(index++);
                Vector2Int coord = new Vector2Int(x, y);

                DropTarget dropTarget = cell.GetComponent<DropTarget>();
                if (dropTarget != null)
                {
                    dropTarget.x = x;
                    dropTarget.y = y;
                }

                // ���� ������ ����
                foreach (Transform child in cell)
                {
                    if (child.name != "SelectionOutline")
                    {
                        Destroy(child.gameObject);
                    }
                }
                // �������� ������ ���� ����
                MergeItem item = board.GetItem(x, y);
                if (item != null)
                {
                    GameObject viewObj = Instantiate(itemViewPrefab, cell);
                    ItemView view = viewObj.GetComponent<ItemView>();
                    view.SetItem(item);
                    view.SetCoord(coord);

                    DraggableItem drag = viewObj.GetComponent<DraggableItem>();
                    if (drag != null)
                    {
                        drag.mergeItem = item;
                        drag.SetOrigin(coord);
                    }
                }

                // SelectionOutline Ȱ��ȭ ����: ���õ� ��ǥ�� ��ġ�ϴ� ��
                Transform outline = cell.transform.Find("SelectionOutline");
                if (outline != null)
                {
                    bool isSelected = selectedCoord.HasValue && selectedCoord.Value == coord;
                    outline.gameObject.SetActive(isSelected);
                }
            }
        }
    }
    public ItemView GetItemViewAt(Vector2Int coord)
    {
        if (currentBoard == null) return null;
        int width = currentBoard.width;
        int height = currentBoard.height;

        int index = (currentBoard.height - 1 - coord.y) * width + coord.x;
        if (index < 0 || index >= gridLayout.transform.childCount)
        {
            Debug.LogWarning("�ε��� ���� �ʰ�!");
            return null;
        }
        Transform cell = gridLayout.transform.GetChild(index);
        var view = cell.GetComponentInChildren<ItemView>(true);
        return view;
    }
}

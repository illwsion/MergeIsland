// BoardUI.cs
using UnityEngine;
using UnityEngine.UI;

public class BoardUI : MonoBehaviour
{
    public GridLayoutGroup gridLayout;
    public RectTransform boardArea; // ��ü ���� ǥ�� ����
    public GameObject cellPrefab;
    public GameObject itemViewPrefab;

    private const float padding = 20f;

    public void DisplayBoard(MergeBoard board)
    {
        // ���� �� ����
        foreach (Transform child in gridLayout.transform)
        {
            Destroy(child.gameObject);
        }

        int width = board.width;
        int height = board.height;

        // ���� ���� ũ�� ���ϱ�
        float areaWidth = boardArea.rect.width - padding;
        float areaHeight = boardArea.rect.height - padding;

        // �� ũ�� ��� (���� ����)
        float cellSize = areaWidth / width;

        // ���� �������ε� Ȯ�� (���ΰ� �ʹ� ������ ���� �������� ����)
        if (cellSize * height > areaHeight)
        {
            cellSize = areaHeight / height;
        }

        // Grid ����
        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = width;
        gridLayout.cellSize = new Vector2(cellSize, cellSize);

        for (int y = height - 1; y >= 0; y--) // �� ����
        {
            for (int x = 0; x < width; x++)
            {
                GameObject cell = Instantiate(cellPrefab, gridLayout.transform);

                // �� ��ǥ ����
                DropTarget dropTarget = cell.GetComponent<DropTarget>();
                if (dropTarget != null)
                {
                    dropTarget.x = x;
                    dropTarget.y = y;
                }
                // ������ ��ġ
                MergeItem item = board.GetItem(x, y);
                if (item != null)
                {
                    GameObject viewObj = Instantiate(itemViewPrefab, cell.transform);
                    ItemView view = viewObj.GetComponent<ItemView>();
                    view.SetItem(item);
                    view.SetCoord(new Vector2Int(x, y));
                    DraggableItem drag = viewObj.GetComponent<DraggableItem>();
                    if (drag != null)
                    {
                        drag.mergeItem = item;
                        drag.SetOrigin(new Vector2Int(x, y));
                    }

                    // ������ ���� ��ư ��Ȱ��ȭ
                    Button btn = cell.GetComponent<Button>();
                    if (btn != null)
                    {
                        btn.enabled = false;
                    }
                }
                else
                {
                    // �� ���� ���� ������ ���� ��ư Ȱ��ȭ
                    Button btn = cell.GetComponent<Button>();
                    if (btn != null)
                    {
                        btn.enabled = true;
                        btn.transition = Selectable.Transition.None;
                        btn.onClick.AddListener(() =>
                        {
                            ItemSelectorManager.Instance.ClearSelection();
                        });
                    }
                }
            }
        }
    }
}

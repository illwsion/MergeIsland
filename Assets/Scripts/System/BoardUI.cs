// BoardUI.cs
using UnityEngine;
using UnityEngine.UI;

public class BoardUI : MonoBehaviour
{
    public GridLayoutGroup gridLayout;
    public RectTransform boardArea; // 전체 보드 표시 영역
    public GameObject cellPrefab;
    public GameObject itemViewPrefab;

    private const float padding = 20f;

    public void DisplayBoard(MergeBoard board)
    {
        // 기존 셀 제거
        foreach (Transform child in gridLayout.transform)
        {
            Destroy(child.gameObject);
        }

        int width = board.width;
        int height = board.height;

        // 보드 영역 크기 구하기
        float areaWidth = boardArea.rect.width - padding;
        float areaHeight = boardArea.rect.height - padding;

        // 셀 크기 계산 (가로 기준)
        float cellSize = areaWidth / width;

        // 세로 기준으로도 확인 (세로가 너무 많으면 세로 기준으로 조정)
        if (cellSize * height > areaHeight)
        {
            cellSize = areaHeight / height;
        }

        // Grid 설정
        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = width;
        gridLayout.cellSize = new Vector2(cellSize, cellSize);

        for (int y = height - 1; y >= 0; y--) // 셀 생성
        {
            for (int x = 0; x < width; x++)
            {
                GameObject cell = Instantiate(cellPrefab, gridLayout.transform);

                // 셀 좌표 설정
                DropTarget dropTarget = cell.GetComponent<DropTarget>();
                if (dropTarget != null)
                {
                    dropTarget.x = x;
                    dropTarget.y = y;
                }
                // 아이템 배치
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

                    // 아이템 선택 버튼 비활성화
                    Button btn = cell.GetComponent<Button>();
                    if (btn != null)
                    {
                        btn.enabled = false;
                    }
                }
                else
                {
                    // 빈 셀일 때만 아이템 선택 버튼 활성화
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

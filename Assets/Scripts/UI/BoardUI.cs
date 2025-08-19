// BoardUI.cs
using System.Drawing;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class BoardUI : MonoBehaviour
{
    public GridLayoutGroup gridLayout;
    public RectTransform boardArea; // 전체 보드 표시 영역
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

        // 좌표 기반으로 선택 상태 가져오기
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
                    Debug.LogError($"[BoardUI] index 초과! index={index}, count={gridLayout.transform.childCount}");
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

                // 기존 아이템 제거
                foreach (Transform child in cell)
                {
                    if (child.name != "SelectionOutline")
                    {
                        Destroy(child.gameObject);
                    }
                }
                // 아이템이 있으면 새로 생성
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

                // SelectionOutline 활성화 조건: 선택된 좌표와 일치하는 셀
                Transform outline = cell.transform.Find("SelectionOutline");
                if (outline != null)
                {
                    bool isSelected = selectedCoord.HasValue && selectedCoord.Value == coord;
                    outline.gameObject.SetActive(isSelected);
                }
            }
        }
    }

    /// <summary>
    /// 같은 보드 내에서 아이템만 업데이트 (셀은 재생성하지 않음)
    /// 드래그 앤 드롭, 머지, 공격 등에 사용
    /// </summary>
    /// <param name="board">업데이트할 보드</param>
    public void UpdateBoardItems(MergeBoard board)
    {
        if (board == null || gridLayout == null) return;

        // 현재 보드와 크기가 다르면 DisplayBoard 사용
        if (currentBoard == null || 
            currentBoard.width != board.width || 
            currentBoard.height != board.height)
        {
            DisplayBoard(board);
            return;
        }

        currentBoard = board;
        int width = board.width;
        int height = board.height;

        // 좌표 기반으로 선택 상태 가져오기
        Vector2Int? selectedCoord = null;
        if (ItemSelectorManager.Instance.HasSelection())
        {
            selectedCoord = ItemSelectorManager.Instance.GetSelectedCoord();
        }

        int index = 0;
        for (int y = height - 1; y >= 0; y--)
        {
            for (int x = 0; x < width; x++)
            {
                if (index >= gridLayout.transform.childCount)
                {
                    Debug.LogError($"[BoardUI] UpdateBoardItems: index 초과! index={index}, count={gridLayout.transform.childCount}");
                    return;
                }

                Transform cell = gridLayout.transform.GetChild(index++);
                Vector2Int coord = new Vector2Int(x, y);

                // 기존 아이템 비활성화 (SelectionOutline 제외)
                ItemView existingView = null;
                foreach (Transform child in cell)
                {
                    if (child.name != "SelectionOutline")
                    {
                        existingView = child.GetComponent<ItemView>();
                        child.gameObject.SetActive(false);
                    }
                }

                // 아이템이 있으면 기존 것을 재활용하거나 새로 생성
                MergeItem item = board.GetItem(x, y);
                
                // 드래그 중인 아이템이 있는 위치는 건드리지 않음
                if (DragManager.Instance.IsPositionBeingDragged(coord))
                {
                    Debug.Log($"[BoardUI] 드래그 중인 위치 {coord} 건드리지 않음");
                    continue;
                }
                
                if (item != null)
                {
                    // 기존 ItemView가 있고 같은 아이템이면 재활용
                    if (existingView != null && existingView.mergeItem == item)
                    {
                        existingView.gameObject.SetActive(true);
                        existingView.SetCoord(coord);
                        
                        DraggableItem drag = existingView.GetComponent<DraggableItem>();
                        if (drag != null)
                        {
                            drag.SetOrigin(coord);
                        }
                    }
                    else
                    {
                        // 새로 생성
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
                }

                // SelectionOutline 활성화 조건: 선택된 좌표와 일치하는 셀
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
            Debug.LogWarning("인덱스 범위 초과!");
            return null;
        }
        Transform cell = gridLayout.transform.GetChild(index);
        var view = cell.GetComponentInChildren<ItemView>(true);
        return view;
    }

    public Vector3 GetGridCenterWorldPosition()
    {
        if (BoardGateSpawner.Instance == null)
        {
            Debug.LogError("[BoardManager] BoardGateSpawner.Instance가 null입니다. 초기화 순서를 확인하세요.");
        }
        return gridLayout.transform.position;
    }
}
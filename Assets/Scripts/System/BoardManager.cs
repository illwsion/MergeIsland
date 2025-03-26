// BoardManager.cs
using UnityEngine;
using System.Collections.Generic;

public class BoardManager : MonoBehaviour
{
    // 좌표 기반으로 보드 연결 (x,y) → MergeBoard
    private Dictionary<Vector2Int, MergeBoard> boardMap = new Dictionary<Vector2Int, MergeBoard>();
    private Vector2Int currentBoardPos = Vector2Int.zero;
    public BoardUI boardUI;
    public static BoardManager Instance;

    // 타입별 최대 레벨 정의
    private Dictionary<string, int> maxLevels = new Dictionary<string, int>
    {
        {"tree", 12},
        {"log", 8}
    };

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void Start()
    {
        // 보드 위치: (0,0), (1,0), (0,1) 식으로 설정
        boardMap[new Vector2Int(0, 0)] = new MergeBoard(3, 2); // 1스테이지
        boardMap[new Vector2Int(1, 0)] = new MergeBoard(5, 5); // 오른쪽 보드
        boardMap[new Vector2Int(0, 1)] = new MergeBoard(6, 4); // 아래 보드
        boardMap[new Vector2Int(0, 0)].PlaceItem(2, 1, new MergeItem(1, 3, "Tree")); // 셀 (2,1)에 레벨 3 나무 배치
        boardMap[new Vector2Int(0, 0)].PlaceItem(1, 1, new MergeItem(1, 3, "Tree")); // 셀 (2,1)에 레벨 3 나무 배치
        boardMap[new Vector2Int(0, 0)].PlaceItem(0, 1, new MergeItem(1, 4, "Tree")); // 셀 (2,1)에 레벨 3 나무 배치
        boardMap[new Vector2Int(0, 1)].PlaceItem(3, 3, new MergeItem(1, 3, "Tree")); // 셀 (2,1)에 레벨 3 나무 배치

        currentBoardPos = new Vector2Int(0, 0);
        Debug.Log("시작 보드: (0, 0)");
        MergeBoard currentBoard = boardMap[currentBoardPos];
        boardUI.DisplayBoard(currentBoard);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            MoveBoard(Vector2Int.right); // 오른쪽 이동
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            MoveBoard(Vector2Int.left); // 왼쪽 이동
        }
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            MoveBoard(Vector2Int.up); // 위로 이동
        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            MoveBoard(Vector2Int.down); // 아래로 이동
        }
    }

    public void MoveBoard(Vector2Int direction)
    {
        Vector2Int nextPos = currentBoardPos + direction;
        if (boardMap.ContainsKey(nextPos))
        {
            currentBoardPos = nextPos;
            Debug.Log("보드 이동: " + currentBoardPos);
            MergeBoard currentBoard = boardMap[currentBoardPos];
            boardUI.DisplayBoard(currentBoard);
            // 여기에 UI 및 오브젝트 업데이트 추가 가능
        }
        else
        {
            Debug.Log("해당 방향에는 보드가 없습니다!");
        }
    }

   

    public void HandleDrop(MergeItem draggedItem, Vector2Int fromPos, Vector2Int toPos)
    {
        MergeBoard board = boardMap[currentBoardPos];

        if (!IsValidCell(board, fromPos) || !IsValidCell(board, toPos)) return;

        MergeItem targetItem = board.GetItem(toPos.x, toPos.y);

        if (targetItem == null) // 빈칸에 드래그
        {
            board.PlaceItem(toPos.x, toPos.y, draggedItem);
            board.grid[fromPos.x, fromPos.y] = null;
            Debug.Log($"아이템 이동: {fromPos} → {toPos}");
        }
        else if (targetItem.level == draggedItem.level && targetItem.type == draggedItem.type)
        {
            int maxLevel = maxLevels.ContainsKey(draggedItem.type.ToLower()) ? maxLevels[draggedItem.type.ToLower()] : int.MaxValue;
            if (draggedItem.level >= maxLevel)
            {
                // 머지는 불가능하지만 위치는 교환
                board.grid[fromPos.x, fromPos.y] = targetItem;
                board.grid[toPos.x, toPos.y] = draggedItem;
                Debug.Log($"최대 레벨 도달로 머지 불가 → 위치 교환: {fromPos} ↔ {toPos}");
                boardUI.DisplayBoard(board);
                return;
            }

            int newLevel = draggedItem.level + 1;
            MergeItem newItem = new MergeItem(draggedItem.id, newLevel, draggedItem.type);
            board.PlaceItem(toPos.x, toPos.y, newItem, true);
            board.grid[fromPos.x, fromPos.y] = null;
            Debug.Log($"머지됨: {draggedItem.level} + {targetItem.level} → {newLevel}");
        }
        else // 다른 아이템
        {
            board.grid[fromPos.x, fromPos.y] = targetItem;
            board.grid[toPos.x, toPos.y] = draggedItem;
            Debug.Log($"아이템 위치 교환: {fromPos} ↔ {toPos}");
        }

        boardUI.DisplayBoard(board);
    }

    private bool IsValidCell(MergeBoard board, Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < board.width && pos.y >= 0 && pos.y < board.height;
    }
}


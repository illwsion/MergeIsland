// BoardManager.cs
using UnityEngine;
using System.Collections.Generic;
using UnityEditorInternal.Profiling.Memory.Experimental;
using System.Collections;

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
        if (ItemDataManager.Instance == null) // ItemDataManger 실행 확인
        {
            Debug.LogError("[BoardManager] ItemDataManager 초기화되지 않음!");
            return;
        }

        // 보드 위치: (0,0), (1,0), (0,1) 식으로 설정
        boardMap[new Vector2Int(0, 0)] = new MergeBoard(3, 2); // 1스테이지
        boardMap[new Vector2Int(1, 0)] = new MergeBoard(5, 5); // 오른쪽 보드
        boardMap[new Vector2Int(0, 1)] = new MergeBoard(6, 4); // 아래 보드
        boardMap[new Vector2Int(0, 0)].PlaceItem(2, 1, new MergeItem(1003)); // 셀 (2,1)에 레벨 3 나무 배치
        boardMap[new Vector2Int(0, 0)].PlaceItem(1, 1, new MergeItem(1003)); // 셀 (2,1)에 레벨 3 나무 배치
        boardMap[new Vector2Int(0, 0)].PlaceItem(0, 1, new MergeItem(1004)); // 셀 (2,1)에 레벨 3 나무 배치
        boardMap[new Vector2Int(0, 1)].PlaceItem(3, 3, new MergeItem(1003)); // 셀 (2,1)에 레벨 3 나무 배치

        currentBoardPos = new Vector2Int(0, 0);
        Debug.Log("시작 보드: (0, 0)");
        MergeBoard currentBoard = boardMap[currentBoardPos];
        boardUI.DisplayBoard(currentBoard);
    }

    void Update()
    {
        if (DragManager.Instance.IsDragging)
            return;
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
            ItemSelectorManager.Instance.ClearSelection(); //아이템 선택 해제

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

        if ((targetItem == null) || (fromPos == toPos)) // 빈칸에 드래그 또는 제자리
        {
            board.grid[fromPos.x, fromPos.y] = null;
            board.PlaceItem(toPos.x, toPos.y, draggedItem);

            ItemSelectorManager.Instance.SetSelectedCoord(toPos);
        }

        DropActionType actionType = DetermineDropAction(draggedItem, targetItem);

        switch (actionType)
        {
            case DropActionType.None:
                HandleSwapOrMove(board, draggedItem, fromPos, toPos);
                break;

            case DropActionType.Merge:
                HandleMerge(board, draggedItem, targetItem, fromPos, toPos);
                break;

            case DropActionType.Attack:
                HandleAttack(board, draggedItem, targetItem, fromPos, toPos);
                break;

            case DropActionType.Supply:
                HandleSupply(board, draggedItem, targetItem, fromPos, toPos);
                break;

            case DropActionType.Split:
                HandleSplit(board, draggedItem, targetItem, fromPos, toPos);
                break;

            default:
                Debug.LogWarning("정의되지 않은 액션입니다.");
                HandleSwapOrMove(board, draggedItem, fromPos, fromPos);
                break;
        }

        boardUI.DisplayBoard(board);
    }

    private DropActionType DetermineDropAction(MergeItem draggedItem, MergeItem targetItem)
    {
        if (draggedItem == null) return DropActionType.None;

        // 분할 아이템 (특수 아이템)

        // 공격 (몬스터에게 무기 드랍)
        if (draggedItem.Data.category == ItemData.Category.Weapon && targetItem != null && targetItem.Data.hp > 0)
            return DropActionType.Attack;

        // 생산 (Supply)
        if (draggedItem.Data.produceType == ItemData.ProduceType.Gather && targetItem != null && targetItem.Data.produceType == ItemData.ProduceType.Supply)
        {
            // 사실 서플라이 가능한지 확인해야 함 아래처럼. 머지테이블에 같이 넣는 대신 Supply로 빠져도 될 듯
            //CanMergeWith에 있고 targetItem.Data.produceType == 'Supply' 일 경우?
            return DropActionType.Supply;
        }
            
        // 머지 가능한 경우
        if (targetItem != null && draggedItem.CanMergeWith(targetItem))
            return DropActionType.Merge;

        // 기본 처리 (교환 또는 이동)
        return DropActionType.None;
    }

    void HandleSwapOrMove(MergeBoard board, MergeItem draggedItem, Vector2Int fromPos, Vector2Int toPos)
    {

    }

    void HandleMerge(MergeBoard board, MergeItem draggedItem, MergeItem targetItem, Vector2Int fromPos, Vector2Int toPos)
    {

    }

    void HandleAttack(MergeBoard board, MergeItem weapon, MergeItem monster, Vector2Int fromPos, Vector2Int toPos)
    {
        monster.TakeDamage(weapon.Data.attackPower);

        // 몬스터 사망 처리
        if (monster.hp <= 0)
        {
            board.grid[toPos.x, toPos.y] = null; // 몬스터 삭제
            // 보상 드랍 로직 추가 가능
        }

        // 무기 아이템 소모 처리
        board.grid[fromPos.x, fromPos.y] = null;

        ItemSelectorManager.Instance.ClearSelection();
    }

    void HandleSupply(MergeBoard board, MergeItem food, MergeItem producer, Vector2Int fromPos, Vector2Int toPos)
    {
        // 생산 시작 로직
       // producer.StartProduction(food.id);

        // 먹이 아이템 소모 처리
        //board.RemoveItem(fromPos.x, fromPos.y);

        ItemSelectorManager.Instance.ClearSelection();
    }

    void HandleSplit(MergeBoard board, MergeItem draggedItem, MergeItem targetItem, Vector2Int fromPos, Vector2Int toPos)
    {

    }


    private bool IsValidCell(MergeBoard board, Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < board.width && pos.y >= 0 && pos.y < board.height;
    }

    public bool IsCellEmpty(MergeBoard board, Vector2Int pos)
    {
        return board.GetItem(pos.x, pos.y) == null; // 아이템이 없으면 빈 칸
    }

    public Vector2Int? FindNearestEmptyCell(Vector2Int origin) //빈칸 탐색 함수
    {
        MergeBoard board = boardMap[currentBoardPos];
        int maxDistance = 10; // 검색 범위 제한 (보드 크기에 맞게 조절)
        for (int distance = 0; distance <= maxDistance; distance++)
        {
            for (int dx = -distance; dx <= distance; dx++)
            {
                for (int dy = -distance; dy <= distance; dy++)
                {
                    // 외곽만 검사 (정사각형 셸 방식)
                    if (Mathf.Abs(dx) != distance && Mathf.Abs(dy) != distance)
                        continue;

                    Vector2Int checkPos = new Vector2Int(origin.x + dx, origin.y + dy);

                    if (IsValidCell(board, checkPos) && IsCellEmpty(board,checkPos))
                    {
                        return checkPos;
                    }
                }
            }
        }

        return null; // 빈칸 없음
    }

    public void SpawnItem(int itemID, Vector2Int position)
    {
        MergeBoard board = boardMap[currentBoardPos];
        if (!IsValidCell(board, position))
        {
            Debug.LogError($"[BoardManager] 유효하지 않은 위치에 아이템 생성 시도: {position}");
            return;
        }

        ItemData data = ItemDataManager.Instance.GetItemData(itemID);
        if (data == null)
        {
            Debug.LogError($"[BoardManager] 유효하지 않은 아이템 ID: {itemID}");
            return;
        }

        board.PlaceItem(position.x, position.y, new MergeItem(itemID));
        boardUI.DisplayBoard(board);
    }

    IEnumerator SelectAfterFrame(Vector2Int pos)
    {
        yield return null; // 한 프레임 뒤에 실행 (DisplayBoard() 이후)

        ItemView targetView = boardUI.GetItemViewAt(pos);
        if (targetView != null)
        {
            ItemSelectorManager.Instance.Select(targetView);
        }
    }

    public void RefreshBoard()
    {
        if (boardMap.ContainsKey(currentBoardPos))
        {
            boardUI.DisplayBoard(boardMap[currentBoardPos]);
        }
        else
        {
            Debug.LogWarning($"[BoardManager] RefreshBoard(): boardMap에 currentBoardPos {currentBoardPos} 없음!");
        }
    }

    public bool HasBoard(Vector2Int pos)
    {
        return boardMap.ContainsKey(pos);
    }
}


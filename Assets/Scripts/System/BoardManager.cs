// BoardManager.cs
using UnityEngine;
using System.Collections.Generic;
using UnityEditorInternal.Profiling.Memory.Experimental;
using System.Collections;
using static UnityEditor.PlayerSettings;

public class BoardManager : MonoBehaviour
{
    // 좌표 기반으로 보드 연결 (x,y) → MergeBoard
    private Dictionary<Vector2Int, MergeBoard> boardMap = new Dictionary<Vector2Int, MergeBoard>();
    private Vector2Int currentBoardPos = Vector2Int.zero;
    public BoardUI boardUI;
    public static BoardManager Instance;

    private List<MergeItem> timeDrivenProducers = new List<MergeItem>();

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
        boardMap[new Vector2Int(0, 0)] = new MergeBoard(4, 4); // 1스테이지
        boardMap[new Vector2Int(1, 0)] = new MergeBoard(5, 5); // 오른쪽 보드
        boardMap[new Vector2Int(0, 1)] = new MergeBoard(6, 4); // 아래 보드
        PlaceInitialItem(new Vector2Int(0, 0), 2, 1, 1003);
        PlaceInitialItem(new Vector2Int(0, 0), 1, 1, 1003);
        PlaceInitialItem(new Vector2Int(0, 0), 0, 1, 1004);
        PlaceInitialItem(new Vector2Int(0, 1), 3, 3, 1004);
        PlaceInitialItem(new Vector2Int(0, 1), 0, 0, 2001);
        PlaceInitialItem(new Vector2Int(0, 1), 0, 1, 2002);
        PlaceInitialItem(new Vector2Int(0, 1), 0, 2, 2003);

        currentBoardPos = new Vector2Int(0, 0);
        Debug.Log("시작 보드: (0, 0)");
        MergeBoard currentBoard = boardMap[currentBoardPos];
        boardUI.DisplayBoard(currentBoard);
    }

    void Update()
    {
        UpdateProductionItems(Time.deltaTime);

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

        if (!board.IsValidCell(fromPos) || !board.IsValidCell(toPos)) return;

        MergeItem targetItem = board.GetItem(toPos.x, toPos.y);

        if ((targetItem == null) || (fromPos == toPos)) // 빈칸에 드래그 또는 제자리
        {
            board.grid[fromPos.x, fromPos.y] = null;
            board.PlaceItem(toPos.x, toPos.y, draggedItem);

            ItemSelectorManager.Instance.SetSelectedCoord(toPos);
        }
        else // 무언가 아이템이 있는 자리로 갔을 때
        {
            DropActionType actionType = DetermineDropAction(draggedItem, targetItem);

            switch (actionType)
            {
                case DropActionType.None:
                    HandleSwap(board, draggedItem, targetItem, fromPos, toPos);
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

                default:
                    Debug.LogWarning("정의되지 않은 액션입니다.");
                    HandleSwap(board, draggedItem, targetItem, fromPos, fromPos);
                    break;
            }
        }

        boardUI.DisplayBoard(board);
    }

    private DropActionType DetermineDropAction(MergeItem draggedItem, MergeItem targetItem)
    {
        // 공격 (체력이 있는 유닛에게 무기 드랍)
        if (draggedItem.Data.attackPower > 0 && targetItem.Data.hp > 0)
            return DropActionType.Attack;

        if (SupplyRuleManager.Instance.GetRule(targetItem.id, draggedItem.id) != null)
        {
            return DropActionType.Supply;
        }
            
        // 머지 (MergeTable에 있을 경우)
        if (draggedItem.CanMergeWith(targetItem))
            return DropActionType.Merge;

        // 기본 처리 (교환)
        return DropActionType.None;
    }

    void HandleSwap(MergeBoard board, MergeItem draggedItem, MergeItem targetItem, Vector2Int fromPos, Vector2Int toPos)
    {
        if (!targetItem.Data.canMove) // 드롭 대상이 이동 불가이면 취소
        {
            Debug.Log("[BoardManager] 해당 위치의 아이템은 교체할 수 없습니다.");
            board.PlaceItem(fromPos.x, fromPos.y, draggedItem);

            ItemSelectorManager.Instance.SetSelectedCoord(fromPos); // 기존 위치
        }
        else
        {
            board.grid[fromPos.x, fromPos.y] = targetItem;
            targetItem.coord = fromPos;
            board.grid[toPos.x, toPos.y] = draggedItem;
            draggedItem.coord = toPos;

            ItemSelectorManager.Instance.SetSelectedCoord(toPos); //새로운 위치
        }

    }

    void HandleMerge(MergeBoard board, MergeItem draggedItem, MergeItem targetItem, Vector2Int fromPos, Vector2Int toPos)
    {
        int? resultId = MergeRuleManager.Instance.GetMergeResult(draggedItem.id, targetItem.id);

        UnregisterProducer(draggedItem);
        UnregisterProducer(targetItem);

        MergeItem newItem = new MergeItem(resultId.Value);
        newItem.board = board;
        board.PlaceItem(toPos.x, toPos.y, newItem, true);
        RegisterProducer(newItem);

        board.grid[fromPos.x, fromPos.y] = null;

        ItemSelectorManager.Instance.SetSelectedCoord(toPos);

        StartCoroutine(SelectAfterFrame(toPos));
    }

    void HandleAttack(MergeBoard board, MergeItem weapon, MergeItem monster, Vector2Int fromPos, Vector2Int toPos)
    {
        //피해 처리
        monster.TakeDamage(weapon.Data.attackPower);

        // 몬스터 사망한 경우
        if (monster.currentHP <= 0)
        {
            // 몬스터 제거
            board.grid[toPos.x, toPos.y] = null;

            // 아이템 드랍
            var dropTable = DropTableManager.Instance.GetTable(monster.Data.dropTableID);
            if (dropTable != null && dropTable.results.Count > 0)
            {
                int dropItemID = GetRandomItemID(dropTable.results); // 확률 기반 선택

                if (dropItemID > 0)
                {
                    BoardManager.Instance.SpawnItem(board, dropItemID, toPos);
                }
            }
            // 추후 : 몬스터 사망 애니메이션 등 추가

            // 선택 상태 해제
            ItemSelectorManager.Instance.ClearSelection();
        }
        else
        {
            // 몬스터 선택
            ItemSelectorManager.Instance.SetSelectedCoord(toPos);
        }

        // 공격 아이템 제거
        UnregisterProducer(weapon);
        board.grid[fromPos.x, fromPos.y] = null;
        StartCoroutine(SelectAfterFrame(toPos));

    }

    void HandleSupply(MergeBoard board, MergeItem suppliedItem, MergeItem receiverItem, Vector2Int fromPos, Vector2Int toPos)
    {
        // 공급 룰 가져옴
        var rule = SupplyRuleManager.Instance.GetRule(receiverItem.id, suppliedItem.id);
        if (rule == null)
        {
            Debug.LogWarning($"[HandleSupply] 공급 룰을 찾을 수 없습니다: A={receiverItem.id}, B={suppliedItem.id}");
            return;
        }

        // 공급 아이템 소모 처리
        UnregisterProducer(suppliedItem);
        board.grid[fromPos.x, fromPos.y] = null;

        //생성 위치 계산
        Vector2Int? spawnPos = board.FindNearestEmptyCell(toPos);
        if (spawnPos == null)
        {
            spawnPos = fromPos;
        }

        // 결과 처리
        switch (rule.resultType)
        {
            case SupplyRule.ResultType.Item:
                BoardManager.Instance.SpawnItem(board, rule.resultItem, spawnPos.Value);
                break;

            case SupplyRule.ResultType.Gold:
                PlayerResourceManager.Instance.Add(ResourceType.Gold, rule.resultValue);
                break;

            case SupplyRule.ResultType.Energy:
                PlayerResourceManager.Instance.Add(ResourceType.Energy, rule.resultValue);
                break;

            case SupplyRule.ResultType.Wood:
                PlayerResourceManager.Instance.Add(ResourceType.Wood, rule.resultValue);
                break;

            default:
                Debug.LogWarning($"[HandleSupply] 알 수 없는 resultType: {rule.resultType}");
                break;
        }

        ItemSelectorManager.Instance.ClearSelection();
    }

    public void SpawnItem(MergeBoard board, int itemID, Vector2Int position)
    {
        if (!board.IsValidCell(position))
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
        MergeItem newItem = new MergeItem(itemID);
        newItem.board = board; // 소속 보드 등록
        
        board.PlaceItem(position.x, position.y, newItem);
        RegisterProducer(board.GetItem(position.x, position.y));
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

    public void RegisterProducer(MergeItem item)
    {
        if (item != null && item.IsTimeDrivenProducer())
        {
            if (!timeDrivenProducers.Contains(item))
            {
                timeDrivenProducers.Add(item);
                Debug.Log($"[등록됨] {item.name} | hash={item.GetHashCode()}");
            }
            else
            {
                Debug.Log($"[이미 등록됨] {item.name} | hash={item.GetHashCode()}");
            }
        }
    }

    public void UnregisterProducer(MergeItem item)
    {
        if (timeDrivenProducers.Contains(item))
        {
            timeDrivenProducers.Remove(item);
        }
    }

    private void UpdateProductionItems(float deltaTime)
    {
        foreach (var item in timeDrivenProducers)
        {
            item.UpdateProductionStorage(deltaTime);
        }
    }
    private void PlaceInitialItem(Vector2Int boardPos, int x, int y, int id)
    {
        MergeBoard board = boardMap[boardPos];

        MergeItem item = new MergeItem(id);
        item.board = board;
        item.coord = new Vector2Int(x, y);
        boardMap[boardPos].PlaceItem(x, y, item);
        RegisterProducer(item);
    }
    public MergeBoard GetCurrentBoard()
    {
        return boardMap[currentBoardPos];
    }

    private int GetRandomItemID(List<DropResult> results)
    {
        int total = 0;
        foreach (var result in results)
            total += result.probability;

        if (total <= 0)
        {
            Debug.LogError("[GetRandomItemID] 확률 총합이 0 이하입니다.");
            return -1;
        }

        int roll = UnityEngine.Random.Range(0, total);
        int accum = 0;

        foreach (var result in results)
        {
            accum += result.probability;
            if (roll < accum)
                return result.itemID;
        }

        return -1;
    }
}


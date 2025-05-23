// BoardManager.cs
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using static ItemData;

public class BoardManager : MonoBehaviour
{
    private Dictionary<string, MergeBoard> boardMap = new(); // boardKey → MergeBoard
    private Dictionary<Vector2Int, string> posToBoardKeyMap = new(); // worldPos → boardKey
    private Dictionary<string, Vector2Int> boardKeyToPosMap = new(); // boardKey → worldPos

    private string currentBoardKey = null;

    public BoardUI boardUI;
    public static BoardManager Instance;

    private List<MergeItem> timeDrivenProducers = new List<MergeItem>();

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

        foreach (var boardInfo in BoardDataManager.Instance.GetAllBoardData())
        {
            MergeBoard board = new MergeBoard(boardInfo.width, boardInfo.height);
            boardMap[boardInfo.key] = board;
            boardKeyToPosMap[boardInfo.key] = boardInfo.worldPos;
            posToBoardKeyMap[boardInfo.worldPos] = boardInfo.key;

            foreach (var itemData in BoardInitialItemManager.Instance.GetInitialItemsForBoard(boardInfo.key))
            {
                SpawnItem(board, itemData.itemKey, itemData.coord);
            }
        }

        currentBoardKey = "BOARD_BEACH_0";
        Debug.Log($"시작 보드: {currentBoardKey}");
        boardUI.DisplayBoard(boardMap[currentBoardKey]);
    }

    void Update()
    {
        UpdateProductionItems(Time.deltaTime);

        if (DragManager.Instance.IsDragging)
            return;
        //임시 이동
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

    public void MoveBoardTo(string boardKey)
    {
        if (!boardMap.ContainsKey(boardKey))
        {
            Debug.LogError($"[BoardManager] 보드 키 '{boardKey}' 를 찾을 수 없습니다.");
            return;
        }

        currentBoardKey = boardKey;
        ItemSelectorManager.Instance.ClearSelection();
        MergeBoard board = boardMap[boardKey];
        boardUI.DisplayBoard(board);
    }

    public void MoveBoard(Vector2Int direction)
    {
        if (!boardKeyToPosMap.ContainsKey(currentBoardKey))
        {
            Debug.LogError("[BoardManager] 현재 보드 키에 해당하는 위치 정보를 찾을 수 없습니다.");
            return;
        }
        Vector2Int currentPos = boardKeyToPosMap[currentBoardKey];
        Vector2Int nextPos = currentPos + direction;

        if (posToBoardKeyMap.TryGetValue(nextPos, out string nextBoardKey))
        {
            MoveBoardTo(nextBoardKey);
        }
        else
        {
            Debug.Log("해당 방향에는 보드가 없습니다!");
        }
    }

    public MergeBoard GetCurrentBoard()
    {
        return boardMap[currentBoardKey];
    }

    public void SpawnItem(MergeBoard board, string itemKey, Vector2Int position)
    {
        if (!board.IsValidCell(position))
        {
            Debug.LogError($"[BoardManager] 유효하지 않은 위치에 아이템 생성 시도: {position}");
            return;
        }

        ItemData data = ItemDataManager.Instance.GetItemData(itemKey);
        if (data == null)
        {
            Debug.LogError($"[BoardManager] 유효하지 않은 아이템 ID: {itemKey}");
            return;
        }
        MergeItem newItem = new MergeItem(itemKey);
        newItem.board = board; // 소속 보드 등록

        board.PlaceItem(position.x, position.y, newItem);
        RegisterProducer(board.GetItem(position.x, position.y));
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
        var producers = timeDrivenProducers.ToArray();
        foreach (var item in producers)
        {
            item.UpdateProductionStorage(deltaTime);
        }
    }

    public void RemoveItem(MergeItem item)
    {
        UnregisterProducer(item);
        MergeBoard board = item.board;
        board.grid[item.coord.x, item.coord.y] = null;
    }

    public bool HasBoard(string boardKey)
    {
        return boardMap.ContainsKey(boardKey);
    }

    public void RefreshBoard()
    {
        if (boardMap.ContainsKey(currentBoardKey))
        {
            boardUI.DisplayBoard(boardMap[currentBoardKey]);
        }
        else
        {
            Debug.LogWarning($"[BoardManager] RefreshBoard(): boardMap에 currentBoardKey {currentBoardKey} 없음!");
        }
    }

    public Vector2Int? GetBoardPosition(string boardKey)
    {
        if (boardKeyToPosMap.TryGetValue(boardKey, out var pos))
            return pos;
        return null;
    }

    public string GetBoardKey(Vector2Int pos)
    {
        if (posToBoardKeyMap.TryGetValue(pos, out var key))
            return key;
        return null;
    }

    public void HandleDrop(MergeItem draggedItem, Vector2Int fromPos, Vector2Int toPos)
    {
        MergeBoard board = boardMap[currentBoardKey];

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
        //공격 가능 판단
        var tool = draggedItem.Data.toolType;
        var target = targetItem.Data.targetMaterial;

        bool isAttackMatch =
            (tool == ToolType.Axe && target == TargetMaterial.Wood) ||
            (tool == ToolType.Pickaxe && (target == TargetMaterial.Stone || target == TargetMaterial.Iron)) ||
            (tool == ToolType.Weapon && target == TargetMaterial.Monster);

        // 공격 (체력이 있는 유닛에게 무기 드랍)
        if (isAttackMatch && draggedItem.Data.attackPower > 0 && targetItem.Data.hp > 0)
            return DropActionType.Attack;

        if (SupplyRuleManager.Instance.GetRule(targetItem.key, draggedItem.key) != null)
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
        string? resultId = MergeRuleManager.Instance.GetMergeResult(draggedItem.key, targetItem.key);

        UnregisterProducer(draggedItem);
        UnregisterProducer(targetItem);

        MergeItem newItem = new MergeItem(resultId);
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
            string dropItemKey = DropTableManager.Instance.GetRandomDropItem(monster);
            if (dropItemKey != null)
            {
                BoardManager.Instance.SpawnItem(board, dropItemKey, toPos);
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
        RemoveItem(weapon);
        StartCoroutine(SelectAfterFrame(toPos));

    }

    void HandleSupply(MergeBoard board, MergeItem suppliedItem, MergeItem receiverItem, Vector2Int fromPos, Vector2Int toPos)
    {
        // 공급 룰 가져옴
        var rule = SupplyRuleManager.Instance.GetRule(receiverItem.key, suppliedItem.key);
        if (rule == null)
        {
            Debug.LogWarning($"[HandleSupply] 공급 룰을 찾을 수 없습니다: A={receiverItem.key}, B={suppliedItem.key}");
            return;
        }

        // 공급 아이템 소모 처리
        RemoveItem(suppliedItem);

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

    

    IEnumerator SelectAfterFrame(Vector2Int pos)
    {
        yield return null; // 한 프레임 뒤에 실행 (DisplayBoard() 이후)

        ItemView targetView = boardUI.GetItemViewAt(pos);
        if (targetView != null)
        {
            ItemSelectorManager.Instance.Select(targetView);
        }
    }
    

    private void PlaceInitialItem(string boardKey, int x, int y, string key)
    {
        MergeBoard board = boardMap[boardKey];

        MergeItem item = new MergeItem(key);
        item.board = board;
        item.coord = new Vector2Int(x, y);
        boardMap[boardKey].PlaceItem(x, y, item);
        RegisterProducer(item);
    }

    private string GetRandomItemKey(List<DropResult> results)
    {
        int total = 0;
        foreach (var result in results)
            total += result.probability;

        if (total <= 0)
        {
            Debug.LogError("[GetRandomItemID] 확률 총합이 0 이하입니다.");
            return "null";
        }

        int roll = UnityEngine.Random.Range(0, total);
        int accum = 0;

        foreach (var result in results)
        {
            accum += result.probability;
            if (roll < accum)
                return result.itemKey;
        }

        return "null";
    }
}


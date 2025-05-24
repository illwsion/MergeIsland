// BoardManager.cs
using UnityEngine;
using System.Collections.Generic;
using UnityEditorInternal.Profiling.Memory.Experimental;
using System.Collections;
using static UnityEditor.PlayerSettings;
using static ItemData;

public class BoardManager : MonoBehaviour
{
    // ��ǥ ������� ���� ���� (x,y) �� MergeBoard
    private Dictionary<Vector2Int, MergeBoard> boardMap = new Dictionary<Vector2Int, MergeBoard>();
    private Vector2Int currentBoardPos = Vector2Int.zero;
    public BoardUI boardUI;
    public static BoardManager Instance;

    private List<MergeItem> timeDrivenProducers = new List<MergeItem>();

    // Ÿ�Ժ� �ִ� ���� ����
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
        if (ItemDataManager.Instance == null) // ItemDataManger ���� Ȯ��
        {
            Debug.LogError("[BoardManager] ItemDataManager �ʱ�ȭ���� ����!");
            return;
        }

        foreach (var boardInfo in BoardDataManager.Instance.GetAllBoardData())
        {
            MergeBoard board = new MergeBoard(boardInfo.width, boardInfo.height);
            boardMap[boardInfo.worldPos] = board;

            foreach (var itemData in BoardInitialItemManager.Instance.GetInitialItemsForBoard(boardInfo.key))
            {
                SpawnItem(board, itemData.itemKey, itemData.coord);
            }
        }

        currentBoardPos = new Vector2Int(0, 0);
        Debug.Log("���� ����: (0, 0)");
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
            MoveBoard(Vector2Int.right); // ������ �̵�
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            MoveBoard(Vector2Int.left); // ���� �̵�
        }
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            MoveBoard(Vector2Int.up); // ���� �̵�
        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            MoveBoard(Vector2Int.down); // �Ʒ��� �̵�
        }


    }

    public void MoveBoard(Vector2Int direction)
    {
        Vector2Int nextPos = currentBoardPos + direction;
        if (boardMap.ContainsKey(nextPos))
        {
            ItemSelectorManager.Instance.ClearSelection(); //������ ���� ����

            currentBoardPos = nextPos;
            Debug.Log("���� �̵�: " + currentBoardPos);
            MergeBoard currentBoard = boardMap[currentBoardPos];
            boardUI.DisplayBoard(currentBoard);
            // ���⿡ UI �� ������Ʈ ������Ʈ �߰� ����
        }
        else
        {
            Debug.Log("�ش� ���⿡�� ���尡 �����ϴ�!");
        }
    }

    public void MoveBoardTo(string boardKey)
    {
        BoardData targetData = BoardDataManager.Instance.GetBoardData(boardKey);
        if (targetData == null)
        {
            Debug.LogError($"[BoardManager] MoveBoardTo: ���� Ű '{boardKey}' �� ã�� �� �����ϴ�.");
            return;
        }

        Vector2Int targetPos = targetData.worldPos;

        if (!boardMap.ContainsKey(targetPos))
        {
            Debug.LogError($"[BoardManager] MoveBoardTo: ���� ��ġ {targetPos} �� boardMap�� �����ϴ�.");
            return;
        }

        currentBoardPos = targetPos;

        ItemSelectorManager.Instance.ClearSelection(); // ���� ����
        Debug.Log($"[BoardManager] ���� �̵�: {boardKey} at {targetPos}");

        MergeBoard currentBoard = boardMap[currentBoardPos];
        boardUI.DisplayBoard(currentBoard);
    }



    public void HandleDrop(MergeItem draggedItem, Vector2Int fromPos, Vector2Int toPos)
    {
        MergeBoard board = boardMap[currentBoardPos];

        if (!board.IsValidCell(fromPos) || !board.IsValidCell(toPos)) return;

        MergeItem targetItem = board.GetItem(toPos.x, toPos.y);

        if ((targetItem == null) || (fromPos == toPos)) // ��ĭ�� �巡�� �Ǵ� ���ڸ�
        {
            board.grid[fromPos.x, fromPos.y] = null;
            board.PlaceItem(toPos.x, toPos.y, draggedItem);

            ItemSelectorManager.Instance.SetSelectedCoord(toPos);
        }
        else // ���� �������� �ִ� �ڸ��� ���� ��
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
                    Debug.LogWarning("���ǵ��� ���� �׼��Դϴ�.");
                    HandleSwap(board, draggedItem, targetItem, fromPos, fromPos);
                    break;
            }
        }

        boardUI.DisplayBoard(board);
    }

    private DropActionType DetermineDropAction(MergeItem draggedItem, MergeItem targetItem)
    {
        //���� ���� �Ǵ�
        var tool = draggedItem.Data.toolType;
        var target = targetItem.Data.targetMaterial;

        bool isAttackMatch =
            (tool == ToolType.Axe && target == TargetMaterial.Wood) ||
            (tool == ToolType.Pickaxe && (target == TargetMaterial.Stone || target == TargetMaterial.Iron)) ||
            (tool == ToolType.Weapon && target == TargetMaterial.Monster);

        // ���� (ü���� �ִ� ���ֿ��� ���� ���)
        if (isAttackMatch && draggedItem.Data.attackPower > 0 && targetItem.Data.hp > 0)
            return DropActionType.Attack;

        if (SupplyRuleManager.Instance.GetRule(targetItem.key, draggedItem.key) != null)
        {
            return DropActionType.Supply;
        }
            
        // ���� (MergeTable�� ���� ���)
        if (draggedItem.CanMergeWith(targetItem))
            return DropActionType.Merge;

        // �⺻ ó�� (��ȯ)
        return DropActionType.None;
    }

    void HandleSwap(MergeBoard board, MergeItem draggedItem, MergeItem targetItem, Vector2Int fromPos, Vector2Int toPos)
    {
        if (!targetItem.Data.canMove) // ��� ����� �̵� �Ұ��̸� ���
        {
            Debug.Log("[BoardManager] �ش� ��ġ�� �������� ��ü�� �� �����ϴ�.");
            board.PlaceItem(fromPos.x, fromPos.y, draggedItem);

            ItemSelectorManager.Instance.SetSelectedCoord(fromPos); // ���� ��ġ
        }
        else
        {
            board.grid[fromPos.x, fromPos.y] = targetItem;
            targetItem.coord = fromPos;
            board.grid[toPos.x, toPos.y] = draggedItem;
            draggedItem.coord = toPos;

            ItemSelectorManager.Instance.SetSelectedCoord(toPos); //���ο� ��ġ
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
        //���� ó��
        monster.TakeDamage(weapon.Data.attackPower);

        // ���� ����� ���
        if (monster.currentHP <= 0)
        {
            // ���� ����
            board.grid[toPos.x, toPos.y] = null;

            // ������ ���
            string dropItemKey = DropTableManager.Instance.GetRandomDropItem(monster);
            if (dropItemKey != null)
            {
                BoardManager.Instance.SpawnItem(board, dropItemKey, toPos);
            }

            // ���� : ���� ��� �ִϸ��̼� �� �߰�

            // ���� ���� ����
            ItemSelectorManager.Instance.ClearSelection();
        }
        else
        {
            // ���� ����
            ItemSelectorManager.Instance.SetSelectedCoord(toPos);
        }

        // ���� ������ ����
        RemoveItem(weapon);
        StartCoroutine(SelectAfterFrame(toPos));

    }

    void HandleSupply(MergeBoard board, MergeItem suppliedItem, MergeItem receiverItem, Vector2Int fromPos, Vector2Int toPos)
    {
        // ���� �� ������
        var rule = SupplyRuleManager.Instance.GetRule(receiverItem.key, suppliedItem.key);
        if (rule == null)
        {
            Debug.LogWarning($"[HandleSupply] ���� ���� ã�� �� �����ϴ�: A={receiverItem.key}, B={suppliedItem.key}");
            return;
        }

        // ���� ������ �Ҹ� ó��
        RemoveItem(suppliedItem);

        //���� ��ġ ���
        Vector2Int? spawnPos = board.FindNearestEmptyCell(toPos);
        if (spawnPos == null)
        {
            spawnPos = fromPos;
        }

        // ��� ó��
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
                Debug.LogWarning($"[HandleSupply] �� �� ���� resultType: {rule.resultType}");
                break;
        }

        ItemSelectorManager.Instance.ClearSelection();
    }

    public void SpawnItem(MergeBoard board, string itemKey, Vector2Int position)
    {
        if (!board.IsValidCell(position))
        {
            Debug.LogError($"[BoardManager] ��ȿ���� ���� ��ġ�� ������ ���� �õ�: {position}");
            return;
        }

        ItemData data = ItemDataManager.Instance.GetItemData(itemKey);
        if (data == null)
        {
            Debug.LogError($"[BoardManager] ��ȿ���� ���� ������ ID: {itemKey}");
            return;
        }
        MergeItem newItem = new MergeItem(itemKey);
        newItem.board = board; // �Ҽ� ���� ���
        
        board.PlaceItem(position.x, position.y, newItem);
        RegisterProducer(board.GetItem(position.x, position.y));
    }

    IEnumerator SelectAfterFrame(Vector2Int pos)
    {
        yield return null; // �� ������ �ڿ� ���� (DisplayBoard() ����)

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
            Debug.LogWarning($"[BoardManager] RefreshBoard(): boardMap�� currentBoardPos {currentBoardPos} ����!");
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
                Debug.Log($"[��ϵ�] {item.name} | hash={item.GetHashCode()}");
            }
            else
            {
                Debug.Log($"[�̹� ��ϵ�] {item.name} | hash={item.GetHashCode()}");
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

    private void PlaceInitialItem(Vector2Int boardPos, int x, int y, string key)
    {
        MergeBoard board = boardMap[boardPos];

        MergeItem item = new MergeItem(key);
        item.board = board;
        item.coord = new Vector2Int(x, y);
        boardMap[boardPos].PlaceItem(x, y, item);
        RegisterProducer(item);
    }

    public MergeBoard GetCurrentBoard()
    {
        return boardMap[currentBoardPos];
    }

    private string GetRandomItemKey(List<DropResult> results)
    {
        int total = 0;
        foreach (var result in results)
            total += result.probability;

        if (total <= 0)
        {
            Debug.LogError("[GetRandomItemID] Ȯ�� ������ 0 �����Դϴ�.");
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


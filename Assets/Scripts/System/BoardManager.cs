// BoardManager.cs
using UnityEngine;
using System.Collections.Generic;
using UnityEditorInternal.Profiling.Memory.Experimental;
using System.Collections;
using static UnityEditor.PlayerSettings;

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

        // ���� ��ġ: (0,0), (1,0), (0,1) ������ ����
        boardMap[new Vector2Int(0, 0)] = new MergeBoard(4, 4); // 1��������
        boardMap[new Vector2Int(1, 0)] = new MergeBoard(5, 5); // ������ ����
        boardMap[new Vector2Int(0, 1)] = new MergeBoard(6, 4); // �Ʒ� ����
        PlaceInitialItem(new Vector2Int(0, 0), 2, 1, 1003);
        PlaceInitialItem(new Vector2Int(0, 0), 1, 1, 1003);
        PlaceInitialItem(new Vector2Int(0, 0), 0, 1, 1004);
        PlaceInitialItem(new Vector2Int(0, 1), 3, 3, 1004);
        PlaceInitialItem(new Vector2Int(0, 1), 0, 0, 2001);
        PlaceInitialItem(new Vector2Int(0, 1), 0, 1, 2002);
        PlaceInitialItem(new Vector2Int(0, 1), 0, 2, 2003);

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
        // ���� (ü���� �ִ� ���ֿ��� ���� ���)
        if (draggedItem.Data.attackPower > 0 && targetItem.Data.hp > 0)
            return DropActionType.Attack;

        if (SupplyRuleManager.Instance.GetRule(targetItem.id, draggedItem.id) != null)
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
        //���� ó��
        monster.TakeDamage(weapon.Data.attackPower);

        // ���� ����� ���
        if (monster.currentHP <= 0)
        {
            // ���� ����
            board.grid[toPos.x, toPos.y] = null;

            // ������ ���
            var dropTable = DropTableManager.Instance.GetTable(monster.Data.dropTableID);
            if (dropTable != null && dropTable.results.Count > 0)
            {
                int dropItemID = GetRandomItemID(dropTable.results); // Ȯ�� ��� ����

                if (dropItemID > 0)
                {
                    BoardManager.Instance.SpawnItem(board, dropItemID, toPos);
                }
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
        UnregisterProducer(weapon);
        board.grid[fromPos.x, fromPos.y] = null;
        StartCoroutine(SelectAfterFrame(toPos));

    }

    void HandleSupply(MergeBoard board, MergeItem suppliedItem, MergeItem receiverItem, Vector2Int fromPos, Vector2Int toPos)
    {
        // ���� �� ������
        var rule = SupplyRuleManager.Instance.GetRule(receiverItem.id, suppliedItem.id);
        if (rule == null)
        {
            Debug.LogWarning($"[HandleSupply] ���� ���� ã�� �� �����ϴ�: A={receiverItem.id}, B={suppliedItem.id}");
            return;
        }

        // ���� ������ �Ҹ� ó��
        UnregisterProducer(suppliedItem);
        board.grid[fromPos.x, fromPos.y] = null;

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

    public void SpawnItem(MergeBoard board, int itemID, Vector2Int position)
    {
        if (!board.IsValidCell(position))
        {
            Debug.LogError($"[BoardManager] ��ȿ���� ���� ��ġ�� ������ ���� �õ�: {position}");
            return;
        }

        ItemData data = ItemDataManager.Instance.GetItemData(itemID);
        if (data == null)
        {
            Debug.LogError($"[BoardManager] ��ȿ���� ���� ������ ID: {itemID}");
            return;
        }
        MergeItem newItem = new MergeItem(itemID);
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
            Debug.LogError("[GetRandomItemID] Ȯ�� ������ 0 �����Դϴ�.");
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


// BoardManager.cs
using UnityEngine;
using System.Collections.Generic;
using UnityEditorInternal.Profiling.Memory.Experimental;
using System.Collections;

public class BoardManager : MonoBehaviour
{
    // ��ǥ ������� ���� ���� (x,y) �� MergeBoard
    private Dictionary<Vector2Int, MergeBoard> boardMap = new Dictionary<Vector2Int, MergeBoard>();
    private Vector2Int currentBoardPos = Vector2Int.zero;
    public BoardUI boardUI;
    public static BoardManager Instance;

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
        boardMap[new Vector2Int(0, 0)] = new MergeBoard(3, 2); // 1��������
        boardMap[new Vector2Int(1, 0)] = new MergeBoard(5, 5); // ������ ����
        boardMap[new Vector2Int(0, 1)] = new MergeBoard(6, 4); // �Ʒ� ����
        boardMap[new Vector2Int(0, 0)].PlaceItem(2, 1, new MergeItem(1003)); // �� (2,1)�� ���� 3 ���� ��ġ
        boardMap[new Vector2Int(0, 0)].PlaceItem(1, 1, new MergeItem(1003)); // �� (2,1)�� ���� 3 ���� ��ġ
        boardMap[new Vector2Int(0, 0)].PlaceItem(0, 1, new MergeItem(1004)); // �� (2,1)�� ���� 3 ���� ��ġ
        boardMap[new Vector2Int(0, 1)].PlaceItem(3, 3, new MergeItem(1003)); // �� (2,1)�� ���� 3 ���� ��ġ

        currentBoardPos = new Vector2Int(0, 0);
        Debug.Log("���� ����: (0, 0)");
        MergeBoard currentBoard = boardMap[currentBoardPos];
        boardUI.DisplayBoard(currentBoard);
    }

    void Update()
    {
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

        if (!IsValidCell(board, fromPos) || !IsValidCell(board, toPos)) return;

        MergeItem targetItem = board.GetItem(toPos.x, toPos.y);

        if ((targetItem == null) || (fromPos == toPos)) // ��ĭ�� �巡�� �Ǵ� ���ڸ�
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
                Debug.LogWarning("���ǵ��� ���� �׼��Դϴ�.");
                HandleSwapOrMove(board, draggedItem, fromPos, fromPos);
                break;
        }

        boardUI.DisplayBoard(board);
    }

    private DropActionType DetermineDropAction(MergeItem draggedItem, MergeItem targetItem)
    {
        if (draggedItem == null) return DropActionType.None;

        // ���� ������ (Ư�� ������)

        // ���� (���Ϳ��� ���� ���)
        if (draggedItem.Data.category == ItemData.Category.Weapon && targetItem != null && targetItem.Data.hp > 0)
            return DropActionType.Attack;

        // ���� (Supply)
        if (draggedItem.Data.produceType == ItemData.ProduceType.Gather && targetItem != null && targetItem.Data.produceType == ItemData.ProduceType.Supply)
        {
            // ��� ���ö��� �������� Ȯ���ؾ� �� �Ʒ�ó��. �������̺� ���� �ִ� ��� Supply�� ������ �� ��
            //CanMergeWith�� �ְ� targetItem.Data.produceType == 'Supply' �� ���?
            return DropActionType.Supply;
        }
            
        // ���� ������ ���
        if (targetItem != null && draggedItem.CanMergeWith(targetItem))
            return DropActionType.Merge;

        // �⺻ ó�� (��ȯ �Ǵ� �̵�)
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

        // ���� ��� ó��
        if (monster.hp <= 0)
        {
            board.grid[toPos.x, toPos.y] = null; // ���� ����
            // ���� ��� ���� �߰� ����
        }

        // ���� ������ �Ҹ� ó��
        board.grid[fromPos.x, fromPos.y] = null;

        ItemSelectorManager.Instance.ClearSelection();
    }

    void HandleSupply(MergeBoard board, MergeItem food, MergeItem producer, Vector2Int fromPos, Vector2Int toPos)
    {
        // ���� ���� ����
       // producer.StartProduction(food.id);

        // ���� ������ �Ҹ� ó��
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
        return board.GetItem(pos.x, pos.y) == null; // �������� ������ �� ĭ
    }

    public Vector2Int? FindNearestEmptyCell(Vector2Int origin) //��ĭ Ž�� �Լ�
    {
        MergeBoard board = boardMap[currentBoardPos];
        int maxDistance = 10; // �˻� ���� ���� (���� ũ�⿡ �°� ����)
        for (int distance = 0; distance <= maxDistance; distance++)
        {
            for (int dx = -distance; dx <= distance; dx++)
            {
                for (int dy = -distance; dy <= distance; dy++)
                {
                    // �ܰ��� �˻� (���簢�� �� ���)
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

        return null; // ��ĭ ����
    }

    public void SpawnItem(int itemID, Vector2Int position)
    {
        MergeBoard board = boardMap[currentBoardPos];
        if (!IsValidCell(board, position))
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

        board.PlaceItem(position.x, position.y, new MergeItem(itemID));
        boardUI.DisplayBoard(board);
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
}


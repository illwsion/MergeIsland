// BoardManager.cs
using UnityEngine;
using System.Collections.Generic;

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
        // ���� ��ġ: (0,0), (1,0), (0,1) ������ ����
        boardMap[new Vector2Int(0, 0)] = new MergeBoard(3, 2); // 1��������
        boardMap[new Vector2Int(1, 0)] = new MergeBoard(5, 5); // ������ ����
        boardMap[new Vector2Int(0, 1)] = new MergeBoard(6, 4); // �Ʒ� ����
        boardMap[new Vector2Int(0, 0)].PlaceItem(2, 1, new MergeItem(1, 3, "Tree")); // �� (2,1)�� ���� 3 ���� ��ġ
        boardMap[new Vector2Int(0, 0)].PlaceItem(1, 1, new MergeItem(1, 3, "Tree")); // �� (2,1)�� ���� 3 ���� ��ġ
        boardMap[new Vector2Int(0, 0)].PlaceItem(0, 1, new MergeItem(1, 4, "Tree")); // �� (2,1)�� ���� 3 ���� ��ġ
        boardMap[new Vector2Int(0, 1)].PlaceItem(3, 3, new MergeItem(1, 3, "Tree")); // �� (2,1)�� ���� 3 ���� ��ġ

        currentBoardPos = new Vector2Int(0, 0);
        Debug.Log("���� ����: (0, 0)");
        MergeBoard currentBoard = boardMap[currentBoardPos];
        boardUI.DisplayBoard(currentBoard);
    }

    void Update()
    {
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

        if (targetItem == null) // ��ĭ�� �巡��
        {
            board.PlaceItem(toPos.x, toPos.y, draggedItem);
            board.grid[fromPos.x, fromPos.y] = null;
            Debug.Log($"������ �̵�: {fromPos} �� {toPos}");
        }
        else if (targetItem.level == draggedItem.level && targetItem.type == draggedItem.type)
        {
            int maxLevel = maxLevels.ContainsKey(draggedItem.type.ToLower()) ? maxLevels[draggedItem.type.ToLower()] : int.MaxValue;
            if (draggedItem.level >= maxLevel)
            {
                // ������ �Ұ��������� ��ġ�� ��ȯ
                board.grid[fromPos.x, fromPos.y] = targetItem;
                board.grid[toPos.x, toPos.y] = draggedItem;
                Debug.Log($"�ִ� ���� ���޷� ���� �Ұ� �� ��ġ ��ȯ: {fromPos} �� {toPos}");
                boardUI.DisplayBoard(board);
                return;
            }

            int newLevel = draggedItem.level + 1;
            MergeItem newItem = new MergeItem(draggedItem.id, newLevel, draggedItem.type);
            board.PlaceItem(toPos.x, toPos.y, newItem, true);
            board.grid[fromPos.x, fromPos.y] = null;
            Debug.Log($"������: {draggedItem.level} + {targetItem.level} �� {newLevel}");
        }
        else // �ٸ� ������
        {
            board.grid[fromPos.x, fromPos.y] = targetItem;
            board.grid[toPos.x, toPos.y] = draggedItem;
            Debug.Log($"������ ��ġ ��ȯ: {fromPos} �� {toPos}");
        }

        boardUI.DisplayBoard(board);
    }

    private bool IsValidCell(MergeBoard board, Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < board.width && pos.y >= 0 && pos.y < board.height;
    }
}


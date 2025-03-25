// BoardUI.cs
using UnityEngine;
using UnityEngine.UI;

public class BoardUI : MonoBehaviour
{
    public GridLayoutGroup gridLayout;
    public RectTransform boardArea; // ��ü ���� ǥ�� ����
    public GameObject cellPrefab;

    private const float padding = 20f;

    public void DisplayBoard(MergeBoard board)
    {
        // ���� �� ����
        foreach (Transform child in gridLayout.transform)
        {
            Destroy(child.gameObject);
        }

        int width = board.width;
        int height = board.height;

        // ���� ���� ũ�� ���ϱ�
        float areaWidth = boardArea.rect.width - padding;
        float areaHeight = boardArea.rect.height - padding;

        // �� ũ�� ��� (���� ����)
        float cellSize = areaWidth / width;

        // ���� �������ε� Ȯ�� (���ΰ� �ʹ� ������ ���� �������� ����)
        if (cellSize * height > areaHeight)
        {
            cellSize = areaHeight / height;
        }

        // Grid ����
        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = width;
        gridLayout.cellSize = new Vector2(cellSize, cellSize);

        // �� ����
        for (int y = height - 1; y >= 0; y--)
        {
            for (int x = 0; x < width; x++)
            {
                GameObject cell = Instantiate(cellPrefab, gridLayout.transform);
                // ���� ������ ������ ���� ����/������ ���� ����
            }
        }
    }
}

// BoardUI.cs
using UnityEngine;
using UnityEngine.UI;

public class BoardUI : MonoBehaviour
{
    public GridLayoutGroup gridLayout;
    public RectTransform boardArea; // 전체 보드 표시 영역
    public GameObject cellPrefab;

    private const float padding = 20f;

    public void DisplayBoard(MergeBoard board)
    {
        // 기존 셀 제거
        foreach (Transform child in gridLayout.transform)
        {
            Destroy(child.gameObject);
        }

        int width = board.width;
        int height = board.height;

        // 보드 영역 크기 구하기
        float areaWidth = boardArea.rect.width - padding;
        float areaHeight = boardArea.rect.height - padding;

        // 셀 크기 계산 (가로 기준)
        float cellSize = areaWidth / width;

        // 세로 기준으로도 확인 (세로가 너무 많으면 세로 기준으로 조정)
        if (cellSize * height > areaHeight)
        {
            cellSize = areaHeight / height;
        }

        // Grid 설정
        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = width;
        gridLayout.cellSize = new Vector2(cellSize, cellSize);

        // 셀 생성
        for (int y = height - 1; y >= 0; y--)
        {
            for (int x = 0; x < width; x++)
            {
                GameObject cell = Instantiate(cellPrefab, gridLayout.transform);
                // 이후 아이템 정보에 따라 색상/아이콘 설정 가능
            }
        }
    }
}

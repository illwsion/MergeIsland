using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[DefaultExecutionOrder(-50)]
public class SkillLinkUI : MonoBehaviour
{
    [Header("화살표 설정")]
    [SerializeField] private GameObject arrowPrefab;
    [SerializeField] private Color arrowColor = Color.white;
    [SerializeField] private float arrowThicknessRatio = 0.1f; // 노드 크기의 10%
    [SerializeField] private float arrowHeadSizeRatio = 0.3f; // 노드 크기의 30%
    [SerializeField] private float minArrowThickness = 2f; // 최소 두께
    [SerializeField] private float maxArrowThickness = 8f; // 최대 두께
    
    [Header("레이어 설정")]
    [SerializeField] private int sortingOrder = 1; // 노드 앞에 그리기
    
    private List<GameObject> activeArrows = new List<GameObject>();
    private RectTransform content;
    
    void Awake()
    {
        // Canvas 하위에 배치되어야 함
        var canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("[SkillLinkUI] Canvas를 찾을 수 없습니다.");
            return;
        }
        
        // content는 외부에서 SetContent로 설정받음
        Debug.Log("[SkillLinkUI] Awake 완료 - content는 SetContent로 설정받아야 합니다.");
    }
    
    public void SetContent(RectTransform contentTransform)
    {
        content = contentTransform;
        if (content != null)
        {
            Debug.Log($"[SkillLinkUI] Content가 설정되었습니다: {content.name}");
        }
        else
        {
            Debug.LogWarning("[SkillLinkUI] Content가 null로 설정되었습니다.");
        }
    }
    
    public void GenerateSkillLinks(string categoryType)
    {
        ClearAllArrows();
        
        if (SkillRequireManager.Instance == null)
        {
            Debug.LogWarning("[SkillLinkUI] SkillRequireManager가 초기화되지 않았습니다.");
            return;
        }
        
        if (content == null)
        {
            Debug.LogWarning("[SkillLinkUI] content가 설정되지 않았습니다. SetContent()를 먼저 호출해주세요.");
            return;
        }
        
        if (arrowPrefab == null)
        {
            Debug.LogError("[SkillLinkUI] arrowPrefab이 설정되지 않았습니다!");
            return;
        }
        
        Debug.Log($"[SkillLinkUI] {categoryType} 카테고리의 스킬 링크를 생성합니다. Content: {content.name}");
        Debug.Log($"[SkillLinkUI] arrowPrefab: {arrowPrefab.name}, 활성화 상태: {arrowPrefab.activeInHierarchy}");
        
        // 현재 카테고리의 모든 스킬 노드 찾기
        var skillNodes = content.GetComponentsInChildren<SkillNodeUI>();
        var nodePositions = new Dictionary<string, Vector2>();
        
        // 각 노드의 위치와 크기 저장
        var nodeSizes = new Dictionary<string, Vector2>();
        foreach (var node in skillNodes)
        {
            var rt = node.GetComponent<RectTransform>();
            nodePositions[node.GetSkillKey()] = rt.anchoredPosition;
            nodeSizes[node.GetSkillKey()] = node.GetNodeSize();
        }
        
        // SkillRequireTable에서 연결 정보 가져와서 화살표 생성
        var allRequires = SkillRequireManager.Instance.GetAllRequireData();
        int arrowCount = 0;
        foreach (var require in allRequires)
        {
            // 두 스킬 모두 현재 카테고리에 있는지 확인
            if (IsSkillInCategory(require.requiredSkillKey, categoryType) && 
                IsSkillInCategory(require.nextSkillKey, categoryType))
            {
                if (nodePositions.ContainsKey(require.requiredSkillKey) && 
                    nodePositions.ContainsKey(require.nextSkillKey))
                {
                    CreateArrow(
                        nodePositions[require.requiredSkillKey],
                        nodePositions[require.nextSkillKey],
                        require.requiredSkillKey,
                        require.nextSkillKey,
                        nodeSizes[require.requiredSkillKey],
                        nodeSizes[require.nextSkillKey]
                    );
                    arrowCount++;
                }
            }
        }
        
        Debug.Log($"[SkillLinkUI] 총 {arrowCount}개의 화살표를 생성했습니다. activeArrows.Count: {activeArrows.Count}");
    }
    
    private bool IsSkillInCategory(string skillKey, string categoryType)
    {
        if (SkillDataManager.Instance == null) return false;
        
        var skill = SkillDataManager.Instance.GetSkillData(skillKey);
        return skill != null && skill.category.ToString() == categoryType;
    }
    
    private void CreateArrow(Vector2 fromPos, Vector2 toPos, string fromSkill, string toSkill, Vector2 fromNodeSize, Vector2 toNodeSize)
    {
        if (arrowPrefab == null)
        {
            Debug.LogWarning("[SkillLinkUI] arrowPrefab이 설정되지 않았습니다.");
            return;
        }
        
        // 화살표를 content의 자식으로 생성 (스킬 노드들과 같은 좌표계 사용)
        var arrow = Instantiate(arrowPrefab, content);
        var arrowRect = arrow.GetComponent<RectTransform>();
        
        // 노드 크기에 따른 화살표 두께 자동 계산
        float avgNodeSize = (fromNodeSize.magnitude + toNodeSize.magnitude) * 0.5f;
        float calculatedThickness = Mathf.Clamp(avgNodeSize * arrowThicknessRatio, minArrowThickness, maxArrowThickness);
        
        // 디버그: 계산된 값 확인
        Debug.Log($"[SkillLinkUI] 계산된 값: 두께={calculatedThickness:F1}");
        
        // Node 바깥 테두리로 위치 조정 (대각선 각도 대응)
        Vector2 fromNodeRadius = fromNodeSize * 0.5f;
        Vector2 toNodeRadius = toNodeSize * 0.5f;
        
        // 방향 벡터 정규화
        Vector2 direction = (toPos - fromPos).normalized;
        
        // 대각선 방향에서 Node 경계 정확히 계산
        Vector2 arrowStart = fromPos + new Vector2(
            direction.x * fromNodeRadius.x,
            direction.y * fromNodeRadius.y
        );
        Vector2 arrowEnd = toPos - new Vector2(
            direction.x * toNodeRadius.x,
            direction.y * toNodeRadius.y
        );
        
        // 전체 거리 계산
        float totalDistance = Vector2.Distance(arrowStart, arrowEnd);
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        
        // 디버그: Node 경계 계산 확인
        Debug.Log($"[SkillLinkUI] Node 경계 계산: fromPos={fromPos}, toPos={toPos}");
        Debug.Log($"[SkillLinkUI] Node 반지름: fromRadius={fromNodeRadius}, toRadius={toNodeRadius}");
        Debug.Log($"[SkillLinkUI] 화살표 시작/끝: start={arrowStart}, end={arrowEnd}");
        Debug.Log($"[SkillLinkUI] 방향: {direction}, 전체거리: {totalDistance:F1}");
        
        // 화살표 설정 (Body와 Head가 하나의 이미지)
        arrowRect.anchoredPosition = arrowStart;
        arrowRect.sizeDelta = new Vector2(totalDistance, calculatedThickness);
        arrowRect.rotation = Quaternion.Euler(0, 0, angle);
        

        
        // 화살표 색상 설정
        var arrowImage = arrow.GetComponent<Image>();
        if (arrowImage != null)
        {
            arrowImage.color = arrowColor;
        }
        
        // 정렬 순서 설정
        var canvasGroup = arrow.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = arrow.AddComponent<CanvasGroup>();
        }
        canvasGroup.blocksRaycasts = false; // 클릭 방해 방지
        
        // 간단한 레이어 관리: transform.SetAsLastSibling()로 맨 앞으로
        arrow.transform.SetAsLastSibling();
        
        // 추가: Canvas Group으로 더 강력한 레이어 제어
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1.0f; // 완전 불투명
            canvasGroup.interactable = false; // 상호작용 비활성화
        }
        
        activeArrows.Add(arrow);
        
        // 디버그 정보
        Debug.Log($"[SkillLinkUI] 화살표 생성: {fromSkill} -> {toSkill} (전체거리: {totalDistance:F1}, 각도: {angle:F1}°)");
        Debug.Log($"[SkillLinkUI] 노드 크기: {fromNodeSize} -> {toNodeSize}, 평균: {avgNodeSize:F1}");
        Debug.Log($"[SkillLinkUI] 계산된 크기: 두께={calculatedThickness:F1}");
        Debug.Log($"[SkillLinkUI] 화살표 위치: {arrowRect.anchoredPosition}, 크기: {arrowRect.sizeDelta}, 회전: {arrowRect.rotation.eulerAngles.z:F1}°");
        Debug.Log($"[SkillLinkUI] 화살표 부모: {arrow.transform.parent.name}, 활성화 상태: {arrow.activeInHierarchy}");
    }
    
    public void ClearAllArrows()
    {
        foreach (var arrow in activeArrows)
        {
            if (arrow != null)
            {
                DestroyImmediate(arrow);
            }
        }
        activeArrows.Clear();
    }
    
    public void RefreshArrows(string categoryType)
    {
        GenerateSkillLinks(categoryType);
    }
}

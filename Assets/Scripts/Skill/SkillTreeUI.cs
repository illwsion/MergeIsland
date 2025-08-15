// SkillTreeUI.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

[DefaultExecutionOrder(0)]
public class SkillTreeUI : MonoBehaviour
{
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private RectTransform content;
    [SerializeField] private GameObject skillNodePrefab;
    [SerializeField] private Vector2 nodeSpacing = new Vector2(150, 150);
    [SerializeField] private TMP_Text skillPointText;
    [SerializeField] private SkillLinkUI skillLinkUI;

    private string currentCategory = "Normal";

    void OnEnable()
    {
        // SkillLinkUI에 content 설정
        if (skillLinkUI != null && content != null)
        {
            skillLinkUI.SetContent(content);
        }
        
        GenerateSkillBoard(currentCategory);
        StartCoroutine(ScrollToCenterNextFrame());
        RefreshSkillPoints();
    }

    public void ChangeTree(string categoryType)
    {
        if (currentCategory != categoryType)
        {
            SkillInfoUI.Instance?.Hide();

            currentCategory = categoryType;

            GenerateSkillBoard(categoryType);
            StartCoroutine(ScrollToCenterNextFrame());
            RefreshVisibleTree();
        }
        
    }

    private IEnumerator ScrollToCenterNextFrame()
    {
        yield return null; // 다음 프레임까지 대기
        Canvas.ForceUpdateCanvases();
        scrollRect.normalizedPosition = new Vector2(0.5f, 0.5f);
    }

    private void RefreshVisibleTree()
    {
        RefreshAllNodes();
    }

    public void RefreshAllNodes()
    {
        foreach (var node in GetComponentsInChildren<SkillNodeUI>())
        {
            node.RefreshVisual();
        }
        RefreshSkillPoints();
        
        // 화살표 새로고침
        if (skillLinkUI != null)
        {
            skillLinkUI.RefreshArrows(currentCategory);
        }
        
        // If info panel is open for some key, re-show to refresh texts when context changed
        if (SkillInfoUI.Instance != null && gameObject.activeInHierarchy)
        {
            var field = typeof(SkillInfoUI).GetField("currentSkillKey", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                var key = field.GetValue(SkillInfoUI.Instance) as string;
                if (!string.IsNullOrEmpty(key))
                {
                    SkillInfoUI.Instance.Show(key);
                }
            }
        }
    }

    private void GenerateSkillBoard(string categoryType)
    {
        foreach (Transform child in content)
        {
            if (child.name != "BackgroundOverlay") // 혹은 tag 비교 등
                Destroy(child.gameObject);
        }

        IEnumerable<SkillData> filteredSkills = SkillDataManager.Instance.GetAllSkills()
            .Where(skill => skill.category.ToString() == categoryType);

        foreach (var skill in filteredSkills)
        {
            var node = Instantiate(skillNodePrefab, content);
            var nodeUI = node.GetComponent<SkillNodeUI>();
            nodeUI.SetSkillKey(skill.key);

            var rt = node.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(
                skill.coordX * nodeSpacing.x,
                skill.coordY * nodeSpacing.y
            );
        }
        
        // 스킬 노드 생성 후 화살표 생성
        if (skillLinkUI != null)
        {
            // 한 프레임 대기 후 화살표 생성 (모든 노드가 완전히 배치된 후)
            StartCoroutine(GenerateLinksAfterFrame(categoryType));
        }
    }

    private void RefreshSkillPoints()
    {
        if (skillPointText == null) return;

        int currentPoints = 0;
        if (SaveController.Instance != null && SaveController.Instance.CurrentSave != null)
        {
            currentPoints = SaveController.Instance.CurrentSave.player.skillPoints;
        }

        skillPointText.text = $"스킬 포인트: {currentPoints}";
    }

    // 스킬 포인트 변경 시 호출할 수 있는 public 메서드
    public void OnSkillPointsChanged()
    {
        RefreshSkillPoints();
    }
    
    private IEnumerator GenerateLinksAfterFrame(string categoryType)
    {
        yield return null; // 다음 프레임까지 대기
        if (skillLinkUI != null)
        {
            skillLinkUI.GenerateSkillLinks(categoryType);
        }
    }
}


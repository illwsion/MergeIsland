// SkillTreeUI.cs
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class SkillTreeUI : MonoBehaviour
{
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private RectTransform content;
    [SerializeField] private GameObject skillNodePrefab;
    [SerializeField] private Vector2 nodeSpacing = new Vector2(150, 150);

    private string currentCategory = "Normal";

    void OnEnable()
    {
        GenerateSkillBoard(currentCategory);
        StartCoroutine(ScrollToCenterNextFrame());
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
    }
}


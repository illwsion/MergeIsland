// SkillTreeUI.cs
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Collections;

public class SkillTreeUI : MonoBehaviour
{
    [SerializeField] private ScrollRect scrollRect;

    [SerializeField] private GameObject productionTree;
    [SerializeField] private GameObject combatTree;
    [SerializeField] private GameObject utilityTree;

    private void Start()
    {
        ChangeTree("Production");
    }

    void OnEnable()
    {
        StartCoroutine(ScrollToBottomNextFrame());
    }

    public void ChangeTree(string treeType)
    {
        productionTree.SetActive(treeType == "Production");
        combatTree.SetActive(treeType == "Combat");
        utilityTree.SetActive(treeType == "Utility");

        StartCoroutine(ScrollToBottomNextFrame());

        RefreshVisibleTree(); // 아래에서 설명
    }

    private IEnumerator ScrollToBottomNextFrame()
    {
        yield return null; // 1 프레임 대기
        Canvas.ForceUpdateCanvases(); // 강제로 Layout 갱신
        scrollRect.verticalNormalizedPosition = 0f;
    }

    private void RefreshVisibleTree()
    {
        var activeTree = GetComponentsInChildren<SkillTreeUI>(true)
            .FirstOrDefault(t => t.gameObject.activeSelf);

        activeTree?.RefreshAllNodes();
    }

    public void RefreshAllNodes()
    {
        foreach (var node in GetComponentsInChildren<SkillNodeUI>())
        {
            node.RefreshVisual();
        }
    }
}


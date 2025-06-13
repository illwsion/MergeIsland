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

        RefreshVisibleTree(); // �Ʒ����� ����
    }

    private IEnumerator ScrollToBottomNextFrame()
    {
        yield return null; // 1 ������ ���
        Canvas.ForceUpdateCanvases(); // ������ Layout ����
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


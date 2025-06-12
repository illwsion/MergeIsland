// SkillPanel.cs
using UnityEngine;

public class SkillPanel : MonoBehaviour
{
    public void ChangeTree(string treeType)
    {
        Debug.Log($"스킬 트리 전환: {treeType}");

        // treeType == "Production" / "Combat" / "Utility"
        // 실제 트리 표시 갱신 처리
    }
}


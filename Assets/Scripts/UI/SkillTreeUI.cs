// SkillTreeDebugUI.cs
using UnityEngine;
using TMPro;
using System.Linq;

public class SkillTreeDebugUI : MonoBehaviour
{
    [SerializeField] private TMP_Text learnedListText;

    private void OnEnable()
    {
        Refresh();
    }

    public void Refresh()
    {
        if (learnedListText == null)
            return;

        var mgr = PlayerSkillManager.Instance;
        if (mgr == null)
        {
            learnedListText.text = "(PlayerSkillManager 없음)";
            return;
        }

        var names = mgr.GetAllLearnedSkills()
            .Select(s => StringTableManager.Instance != null && !string.IsNullOrEmpty(s.skillNameKey)
                ? StringTableManager.Instance.GetLocalized(s.skillNameKey)
                : s.key);

        learnedListText.text = names.Any() ? string.Join(", ", names) : "(배운 스킬 없음)";
    }
}


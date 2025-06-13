// SkillNodeUI.cs
using UnityEngine;
using UnityEngine.UI;

public class SkillNodeUI : MonoBehaviour
{
    [SerializeField] private Image background;
    [SerializeField] private Image icon;
    [SerializeField] private string groupKey; // Inspector에서 직접 입력

    private void Start()
    {
        RefreshVisual();
    }

    public void RefreshVisual()
    {
        int skillLevel = PlayerSkillManager.Instance.GetGroupSkillLevel(groupKey);
        background.color = skillLevel>0 ? Color.green : new Color(0.85f, 0.85f, 0.85f);

        // 아이콘도 데이터 기반으로 불러올 수 있음
        SkillData skillData = PlayerSkillManager.Instance.GetRepresentativeSkill(groupKey);
        if (skillData != null)
            icon.sprite = LoadIcon(skillData.imageName);
    }

    private Sprite LoadIcon(string imageName)
    {
        return Resources.Load<Sprite>($"Icons/{imageName}");
    }
}

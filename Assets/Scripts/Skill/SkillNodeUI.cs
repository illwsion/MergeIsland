// SkillNodeUI.cs
using UnityEngine;
using UnityEngine.UI;

public class SkillNodeUI : MonoBehaviour
{
    [SerializeField] private Image background;
    [SerializeField] private Image icon;
    [SerializeField] private string groupKey; // Inspector에서 직접 입력

    public void RefreshVisual()
    {
        int skillLevel = PlayerSkillManager.Instance.GetGroupSkillLevel(groupKey.Trim());
        background.color = skillLevel>0 ? Color.green : new Color(0.85f, 0.85f, 0.85f);

        // 아이콘도 데이터 기반으로 불러올 수 있음
        SkillData skillData = PlayerSkillManager.Instance.GetRepresentativeSkill(groupKey.Trim());
        if (skillData == null)
        {
            Debug.LogWarning($"[SkillNodeUI] groupKey '{groupKey}'에 해당하는 대표 스킬을 찾지 못했습니다.");
            return;
        }
        if (skillData != null)
            icon.sprite = AtlasManager.Instance.GetSprite(skillData.imageName);
    }
    
    private Sprite LoadIcon(string imageName)
    {
        return Resources.Load<Sprite>($"Icons/{imageName}");
    }
}

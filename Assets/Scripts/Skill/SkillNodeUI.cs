// SkillNodeUI.cs
using UnityEngine;
using UnityEngine.UI;

public class SkillNodeUI : MonoBehaviour
{
    [SerializeField] private Image background;
    [SerializeField] private Image icon;

    private string skillKey;

    public void SetSkillKey(string key)
    {
        skillKey = key;
        RefreshVisual();
    }

    public void RefreshVisual()
    {
        if (string.IsNullOrEmpty(skillKey))
        {
            Debug.LogWarning("[SkillNodeUI] skillKey가 비어있습니다.");
            return;
        }

        int skillLevel = PlayerSkillManager.Instance.GetSkillLevel(skillKey.Trim());
        background.color = skillLevel > 0 ? Color.green : new Color(0.85f, 0.85f, 0.85f);

        SkillData skillData = SkillDataManager.Instance.GetSkillData(skillKey.Trim());
        if (skillData == null)
        {
            Debug.LogWarning($"[SkillNodeUI] skillKey '{skillKey}'에 해당하는 스킬을 찾지 못했습니다.");
            return;
        }

        icon.sprite = AtlasManager.Instance.GetSprite(skillData.imageName);
    }

    private void OnEnable()
    {
        var button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(OnClick);
        }
    }

    private void OnClick()
    {
        SkillInfoUI.Instance?.Show(skillKey);
    }
}

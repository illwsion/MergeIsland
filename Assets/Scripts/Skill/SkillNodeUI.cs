// SkillNodeUI.cs
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class SkillNodeUI : MonoBehaviour
{
    [SerializeField] private Image background;
    [SerializeField] private Image icon;

    private string skillKey;

    public void SetSkillKey(string key)
    {
        skillKey = key?.Trim();
        RefreshVisual();
    }
    
    public string GetSkillKey()
    {
        return skillKey;
    }
    
    public Vector2 GetNodeSize()
    {
        var rt = GetComponent<RectTransform>();
        if (rt != null)
        {
            return rt.sizeDelta;
        }
        return Vector2.one * 100f; // 기본값
    }
    
    private Color GetBackgroundColor()
    {
        if (PlayerSkillManager.Instance == null)
        {
            Debug.LogWarning("[SkillNodeUI] PlayerSkillManager.Instance가 null입니다.");
            return Color.gray; // 기본값
        }
        
        // 1. 스킬을 이미 익혔는지 확인
        bool isSkillLearned = PlayerSkillManager.Instance.IsSkillLearned(skillKey);
        if (isSkillLearned)
        {
            return Color.green; // 초록색: 스킬을 익힘
        }
        
        // 2. 선행 스킬 체크
        var requiredSkills = SkillRequireManager.Instance?.GetRequiredSkills(skillKey);
        if (requiredSkills != null && requiredSkills.Count > 0)
        {
            // 선행 스킬 중 하나라도 익혔는지 확인
            bool hasAnyRequiredSkill = requiredSkills.Any(req => PlayerSkillManager.Instance.IsSkillLearned(req));
            
            if (hasAnyRequiredSkill)
            {
                return new Color(0.7f, 0.7f, 0.7f); // 옅은 회색: 선행 스킬 중 하나라도 익힘
            }
            else
            {
                return new Color(0.3f, 0.3f, 0.3f); // 짙은 회색: 선행 스킬을 모두 익히지 못함
            }
        }
        else
        {
            // 선행 스킬이 없는 경우
            return Color.white; // 하얀색: 선행 스킬 없음
        }
    }

    public void RefreshVisual()
    {
        if (string.IsNullOrEmpty(skillKey))
        {
            Debug.LogWarning("[SkillNodeUI] skillKey가 비어있습니다.");
            return;
        }

        if (background != null)
        {
            Color backgroundColor = GetBackgroundColor();
            background.color = backgroundColor;
        }
        else
        {
            Debug.LogError($"[SkillNodeUI] {skillKey}의 background가 null입니다!");
        }
        
        // 아이콘 시각 효과도 업데이트
        UpdateIconVisual();

        SkillData skillData = SkillDataManager.Instance.GetSkillData(skillKey.Trim());
        if (skillData == null)
        {
            Debug.LogWarning($"[SkillNodeUI] skillKey '{skillKey}'에 해당하는 스킬을 찾지 못했습니다.");
            return;
        }

        if (icon != null && AtlasManager.Instance != null)
        {
            var sprite = AtlasManager.Instance.GetSprite(skillData.imageName);
            if (sprite != null)
            {
                icon.sprite = sprite;
                UpdateIconVisual();
            }
                }
    }
    
    private void UpdateIconVisual()
    {
        if (icon == null) return;
        
        // 스킬 상태에 따른 아이콘 시각 효과 설정
        if (PlayerSkillManager.Instance == null) return;
        
        // 1. 스킬을 이미 익혔는지 확인
        bool isSkillLearned = PlayerSkillManager.Instance.IsSkillLearned(skillKey);
        if (isSkillLearned)
        {
            // 정상 색상 (원본)
            icon.color = Color.white;
            return;
        }
        
        // 2. 선행 스킬 체크
        var requiredSkills = SkillRequireManager.Instance?.GetRequiredSkills(skillKey);
        if (requiredSkills != null && requiredSkills.Count > 0)
        {
            // 선행 스킬 중 하나라도 익혔는지 확인
            bool hasAnyRequiredSkill = requiredSkills.Any(req => PlayerSkillManager.Instance.IsSkillLearned(req));
            
            if (hasAnyRequiredSkill)
            {
                // 회색 효과 (흑백에 가깝게)
                float brightness = 0.6f;
                icon.color = new Color(brightness, brightness, brightness);
            }
            else
            {
                // 실루엣 효과 (검은색)
                icon.color = Color.black;
            }
        }
        else
        {
            // 선행 스킬이 없는 경우 - 회색으로 표시 (배울 수 있지만 아직 안 배움)
            float brightness = 0.6f;
            icon.color = new Color(brightness, brightness, brightness);
        }
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

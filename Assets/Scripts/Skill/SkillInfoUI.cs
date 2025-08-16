// SkillInfoUI.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text;
using System.Linq;
using UnityEngine.EventSystems;

[DefaultExecutionOrder(-100)]
public class SkillInfoUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject root;
    [SerializeField] private Button backgroundCloseButton;

    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text descText;
    [SerializeField] private TMP_Text costText;
    [SerializeField] private TMP_Text conditionText;
    [SerializeField] private Button learnButton;

    private string currentSkillKey;

    public static SkillInfoUI Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        root.SetActive(false);
    }

    private void Start()
    {
        backgroundCloseButton.onClick.AddListener(OnBackgroundClicked);
        learnButton.onClick.AddListener(OnClickLearn);
    }

    public void Show(string skillKey)
    {
        // Normalize key to avoid lookup miss due to whitespace
        skillKey = skillKey?.Trim();
        currentSkillKey = skillKey;
        root.SetActive(true);

        SkillData skill = SkillDataManager.Instance.GetSkillData(skillKey);
        if (skill == null)
        {
            Debug.LogWarning($"[SkillInfoUI] skillKey {skillKey}에 해당하는 스킬을 찾을 수 없습니다.");
            return;
        }

        // 텍스트 및 아이콘 세팅
        iconImage.sprite = AtlasManager.Instance.GetSprite(skill.imageName);
        nameText.text = StringTableManager.Instance.GetLocalized(skill.skillNameKey);
        // 스킬 설명 텍스트 설정
        if (descText != null)
        {
            string baseDesc = StringTableManager.Instance?.GetLocalized(skill.skillDescKey) ?? skill.skillDescKey;
            
            // 스킬 효과 정보 추가
            string effectInfo = BuildEffectInfo(skill);
            if (!string.IsNullOrEmpty(effectInfo))
            {
                baseDesc += "\n\n" + effectInfo;
            }
            
            descText.text = baseDesc;
        }

        // 선/후 계산을 위해 먼저 상태 확인
        bool alreadyLearned = PlayerSkillManager.Instance.GetSkillLevel(skillKey) > 0;

        // 조건 충족 여부 계산: 선행 스킬 / 레벨 / 해금 보드
        bool meetsLevel = true;
        if (skill.unlockLevel > 0)
        {
            int currentLevel = PlayerSkillManager.Instance?.GetCurrentLevel() ?? 1;
            meetsLevel = currentLevel >= skill.unlockLevel;
        }

        bool meetsPrereq = true;
        var requiredSkillKeys = (SkillRequireManager.Instance ?? SkillRequireManager.Ensure())
            ?.GetRequiredSkills(skill.key);
        if (requiredSkillKeys != null && requiredSkillKeys.Count > 0)
        {
            meetsPrereq = requiredSkillKeys.All(k => PlayerSkillManager.Instance.GetSkillLevel(k) > 0);
        }

        bool meetsBoard = true;
        if (!string.IsNullOrWhiteSpace(skill.unlockBoardKey))
        {
            var visited = SaveController.Instance?.CurrentSave?.visitedBoards;
            meetsBoard = visited != null && visited.Contains(skill.unlockBoardKey);
        }

        bool meetsConditions = meetsLevel && meetsPrereq && meetsBoard;
        bool enoughSkillPoint = (SaveController.Instance?.CurrentSave?.player.skillPoints ?? 0) >= skill.costSkillPoint;
        bool canLearn = !alreadyLearned && meetsConditions && enoughSkillPoint;

        // 비용 표시 규칙
        if (alreadyLearned)
        {
            costText.gameObject.SetActive(false);
        }
        else
        {
            costText.gameObject.SetActive(true);
            var sbCost = new StringBuilder();

            // 1) 필요 스킬포인트 라인
            bool enoughSP = SaveController.Instance != null && SaveController.Instance.CurrentSave != null
                ? SaveController.Instance.CurrentSave.player.skillPoints >= skill.costSkillPoint
                : false;
            AppendLineColored(sbCost, "필요 스킬포인트", skill.costSkillPoint.ToString(), enoughSP);

            // 2) 필요 자원 라인 (있을 때만)
            if (skill.costResourceType != ResourceType.None && skill.costResourceValue > 0)
            {
                bool enoughRes = PlayerResourceManager.Instance != null
                    ? PlayerResourceManager.Instance.HasEnough(skill.costResourceType, skill.costResourceValue)
                    : false;
                AppendLineColored(sbCost, "필요 자원", $"{skill.costResourceType} {skill.costResourceValue}", enoughRes);
            }

            costText.text = sbCost.ToString();
        }

        // 조건 텍스트 구성: 선행 스킬 / 요구 레벨 / 요구 해금 보드
        conditionText.text = BuildConditionLines(skill);

        learnButton.gameObject.SetActive(!alreadyLearned);
        learnButton.interactable = canLearn;

        var label = learnButton.GetComponentInChildren<TMP_Text>(true);
        if (!alreadyLearned && label != null)
        {
            string key;
            if (!meetsConditions)
                key = "UI_SKILL_BUTTON_CONDITION";
            else if (!enoughSkillPoint)
                key = "UI_SKILL_BUTTON_SKILLPOINT";
            else
                key = "UI_SKILL_BUTTON_LEARN";

            label.text = StringTableManager.Instance != null
                ? StringTableManager.Instance.GetLocalized(key)
                : key;
        }
        else if (alreadyLearned && label != null)
        {
            string key = "UI_SKILL_BUTTON_LEARNED";
            label.text = StringTableManager.Instance != null
                ? StringTableManager.Instance.GetLocalized(key)
                : key;
        }
    }

    public void Hide()
    {
        root.SetActive(false);
    }

    private void OnClickLearn()
    {
        if (PlayerSkillManager.Instance.LearnSkill(currentSkillKey))
        {
            Show(currentSkillKey); // 다시 새로고침
            SkillTreeUI tree = FindFirstObjectByType<SkillTreeUI>();
            tree?.RefreshAllNodes();
            tree?.OnSkillPointsChanged();
        }
    }

    private void OnBackgroundClicked()
    {
        if (EventSystem.current == null)
        {
            Hide();
            return;
        }

        var pointer = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };
        var results = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(pointer, results);

        foreach (var res in results)
        {
            if (res.gameObject == null) continue;
            // 자기 자신(Info 패널) 계층은 무시
            if (res.gameObject.transform.IsChildOf(root.transform)) continue;

            var node = res.gameObject.GetComponentInParent<SkillNodeUI>();
            var btn = res.gameObject.GetComponentInParent<Button>();
            if (node != null && btn != null)
            {
                // 해당 노드 버튼 클릭을 강제로 실행 → SkillInfoUI.Show 갱신됨
                btn.onClick.Invoke();
                return;
            }
        }

        // 노드가 아니면 패널 닫기
        Hide();
    }

    private string BuildConditionLines(SkillData skill)
    {
        var sb = new StringBuilder();

        // 1) 요구 레벨
        if (skill.unlockLevel > 0)
        {
            int currentLevel = PlayerSkillManager.Instance?.GetCurrentLevel() ?? 1;
            bool ok = currentLevel >= skill.unlockLevel;
            AppendLine(sb, "요구 레벨", skill.unlockLevel.ToString(), ok);
        }
        // 2) 선행 스킬
        var requiredSkillKeys = (SkillRequireManager.Instance ?? SkillRequireManager.Ensure())
            ?.GetRequiredSkills(skill.key);
        if (requiredSkillKeys != null && requiredSkillKeys.Count > 0)
        {
            bool allLearned = requiredSkillKeys.All(k => PlayerSkillManager.Instance.GetSkillLevel(k) > 0);
            string names = string.Join(", ", requiredSkillKeys.Select(k => $"'{GetSkillDisplayName(k)}'"));
            AppendLine(sb, "선행 스킬", names, allLearned);
        }

        

        // 3) 요구 해금 보드
        if (!string.IsNullOrWhiteSpace(skill.unlockBoardKey))
        {
            var visited = (SaveController.Instance != null && SaveController.Instance.CurrentSave != null)
                ? SaveController.Instance.CurrentSave.visitedBoards
                : null;
            bool visitedOk = visited != null && visited.Contains(skill.unlockBoardKey);

            string boardDisplay = skill.unlockBoardKey;
            var boardData = BoardDataManager.Instance != null
                ? BoardDataManager.Instance.GetBoardData(skill.unlockBoardKey)
                : null;
            if (boardData != null && !string.IsNullOrEmpty(boardData.nameKey))
            {
                var localized = StringTableManager.Instance.GetLocalized(boardData.nameKey);
                if (!string.IsNullOrEmpty(localized)) boardDisplay = localized;
            }
            AppendLine(sb, "요구 해금 보드", $"'{boardDisplay}'", visitedOk);
        }

        return sb.ToString();
    }

    private static void AppendLine(StringBuilder sb, string label, string value, bool satisfied)
    {
        if (sb.Length > 0) sb.Append('\n');
        if (satisfied)
        {
            sb.Append(label).Append(" : ").Append(value);
        }
        else
        {
            sb.Append("<color=#FF4D4D>")
              .Append(label).Append(" : ").Append(value)
              .Append("</color>");
        }
    }

    private static void AppendLineColored(StringBuilder sb, string label, string value, bool satisfied)
    {
        AppendLine(sb, label, value, satisfied);
    }

    private static string GetSkillDisplayName(string skillKey)
    {
        var data = SkillDataManager.Instance != null ? SkillDataManager.Instance.GetSkillData(skillKey) : null;
        if (data == null || string.IsNullOrEmpty(data.skillNameKey)) return skillKey;
        var name = StringTableManager.Instance != null ? StringTableManager.Instance.GetLocalized(data.skillNameKey) : null;
        return string.IsNullOrEmpty(name) ? skillKey : name;
    }
    
    private string BuildEffectInfo(SkillData skill)
    {
        if (string.IsNullOrEmpty(skill.targetKey)) return "";
        
        string targetName = GetTargetDisplayName(skill.targetKey);
        string suffix = skill.isPercent ? "%" : "";
        string valueText = skill.skillEffectValue.ToString();
        
        switch (skill.skillEffect)
        {
            case SkillData.SkillEffect.ResourceGain:
                return $"{targetName} 획득량 {valueText}{suffix} 증가";
            case SkillData.SkillEffect.ResourceCap:
                return $"{targetName} 저장량 {valueText}{suffix} 증가";
            case SkillData.SkillEffect.DamageAdd:
                return $"{targetName} 대미지 {valueText}{suffix} 증가";
            case SkillData.SkillEffect.CooldownReduce:
                return $"{targetName} 쿨다운 {valueText}{suffix} 감소";
            case SkillData.SkillEffect.UnlockFeature:
                return $"{targetName} 기능 해금";
            default:
                return $"{targetName} {valueText}{suffix}.";
        }
    }
    
    private string GetTargetDisplayName(string targetKey)
    {
        // 자원 타입인 경우 한글로 표시
        if (System.Enum.TryParse<ResourceType>(targetKey, out var resourceType))
        {
            switch (resourceType)
            {
                case ResourceType.Wood: return "나무";
                case ResourceType.Stone: return "돌";
                case ResourceType.Iron: return "철";
                case ResourceType.Energy: return "에너지";
                case ResourceType.Gold: return "골드";
                case ResourceType.Gem: return "젬";
                default: return targetKey;
            }
        }
        
        // 무기/장비 등 기타 키 매핑
        switch (targetKey.ToLower())
        {
            case "weapon": return "무기";
            case "axe": return "도끼";
            case "pickaxe": return "곡괭이";
            default: return targetKey;
        }
    }
}

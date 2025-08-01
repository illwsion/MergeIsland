// SkillInfoUI.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
        backgroundCloseButton.onClick.AddListener(Hide);
        learnButton.onClick.AddListener(OnClickLearn);
    }

    public void Show(string skillKey)
    {
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
        descText.text = StringTableManager.Instance.GetLocalized(skill.skillDescKey);

        costText.text = $"포인트: {skill.costSkillPoint} / 자원: {skill.costResourceType} {skill.costResourceValue}";
        conditionText.text = $"요구 레벨: {skill.unlockLevel}";

        bool canLearn = PlayerSkillManager.Instance.CanLearnSkill(skillKey);
        bool alreadyLearned = PlayerSkillManager.Instance.GetSkillLevel(skillKey) > 0;

        learnButton.gameObject.SetActive(!alreadyLearned);
        learnButton.interactable = canLearn;

        if (alreadyLearned)
            learnButton.GetComponentInChildren<TMP_Text>().text = "획득 완료";
        else
            learnButton.GetComponentInChildren<TMP_Text>().text = canLearn ? "배우기" : "조건 부족";
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
        }
    }
}

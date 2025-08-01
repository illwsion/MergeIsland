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
            Debug.LogWarning($"[SkillInfoUI] skillKey {skillKey}�� �ش��ϴ� ��ų�� ã�� �� �����ϴ�.");
            return;
        }

        // �ؽ�Ʈ �� ������ ����
        iconImage.sprite = AtlasManager.Instance.GetSprite(skill.imageName);
        nameText.text = StringTableManager.Instance.GetLocalized(skill.skillNameKey);
        descText.text = StringTableManager.Instance.GetLocalized(skill.skillDescKey);

        costText.text = $"����Ʈ: {skill.costSkillPoint} / �ڿ�: {skill.costResourceType} {skill.costResourceValue}";
        conditionText.text = $"�䱸 ����: {skill.unlockLevel}";

        bool canLearn = PlayerSkillManager.Instance.CanLearnSkill(skillKey);
        bool alreadyLearned = PlayerSkillManager.Instance.GetSkillLevel(skillKey) > 0;

        learnButton.gameObject.SetActive(!alreadyLearned);
        learnButton.interactable = canLearn;

        if (alreadyLearned)
            learnButton.GetComponentInChildren<TMP_Text>().text = "ȹ�� �Ϸ�";
        else
            learnButton.GetComponentInChildren<TMP_Text>().text = canLearn ? "����" : "���� ����";
    }

    public void Hide()
    {
        root.SetActive(false);
    }

    private void OnClickLearn()
    {
        if (PlayerSkillManager.Instance.LearnSkill(currentSkillKey))
        {
            Show(currentSkillKey); // �ٽ� ���ΰ�ħ
            SkillTreeUI tree = FindFirstObjectByType<SkillTreeUI>();
            tree?.RefreshAllNodes();
        }
    }
}

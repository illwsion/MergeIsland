// PlayerSkillManager.cs
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PlayerSkillManager : MonoBehaviour
{
    public static PlayerSkillManager Instance { get; private set; }

    private PlayerSaveData saveData;
    private Dictionary<(SkillData.SkillEffect, string), int> cachedSkillEffects = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // �ʿ��ϴٸ� ����
    }

    public void Initialize(PlayerSaveData data)
    {
        saveData = data;
        RecalculateAllEffects();
    }

    public int GetSkillLevel(string skillKey)
    {
        if (string.IsNullOrEmpty(skillKey)) return 0;
        return saveData.learnedSkills.TryGetValue(skillKey, out int level) ? level : 0;
    }

    public bool CanLearnSkill(string skillKey)
    {
        var skill = SkillDataManager.Instance.GetSkillData(skillKey);
        if (skill == null) return false;

        if (GetSkillLevel(skillKey) > 0) return false; // �̹� ��� ���
        if (saveData.skillPoints < skill.costSkillPoint) return false;
        if (skill.unlockLevel > saveData.currentLevel) return false;

        // ���� ��� �� �߰� ������ �ʿ��ϸ� ���⿡ ����

        return true;
    }

    public bool LearnSkill(string skillKey)
    {
        if (!CanLearnSkill(skillKey)) return false;

        saveData.learnedSkills[skillKey] = 1;
        var skill = SkillDataManager.Instance.GetSkillData(skillKey);
        saveData.skillPoints -= skill.costSkillPoint;

        RecalculateAllEffects();
        Debug.Log($"[PlayerSkillManager] ��ų {skillKey} ���� �Ϸ�");
        return true;
    }

    public int GetEffectValue(SkillData.SkillEffect effect, string targetKey)
    {
        return cachedSkillEffects.TryGetValue((effect, targetKey), out int value) ? value : 0;
    }

    private void RecalculateAllEffects()
    {
        cachedSkillEffects.Clear();

        foreach (var pair in saveData.learnedSkills)
        {
            var skill = SkillDataManager.Instance.GetSkillData(pair.Key);
            if (skill == null) continue;

            int totalValue = skill.skillEffectValue; // ������ �� 1���� ��ų�� value��ŭ
            var key = (skill.skillEffect, skill.targetKey);

            if (!cachedSkillEffects.ContainsKey(key))
                cachedSkillEffects[key] = 0;

            cachedSkillEffects[key] += totalValue;
        }
    }

    public IEnumerable<SkillData> GetAllLearnedSkills()
    {
        return saveData.learnedSkills.Keys
            .Select(SkillDataManager.Instance.GetSkillData)
            .Where(skill => skill != null);
    }
}

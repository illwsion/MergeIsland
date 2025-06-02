// PlayerSkillManager.cs
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PlayerSkillManager
{
    private PlayerSaveData saveData;
    private Dictionary<(SkillData.SkillEffect, string), int> cachedSkillEffects = new();

    public PlayerSkillManager(PlayerSaveData data)
    {
        saveData = data;
        RecalculateAllEffects();
    }

    public int GetSkillLevel(string skillKey)
    {
        if (string.IsNullOrEmpty(skillKey))
            return 0;

        return saveData.learnedSkills.TryGetValue(skillKey, out int level) ? level : 0;
    }

    public int GetGroupSkillLevel(string groupKey)
    {
        return saveData.learnedSkills.Keys
            .Select(k => SkillDataManager.Instance.GetSkillData(k))
            .Count(skill => skill != null && skill.group == groupKey);
    }

    public int GetGroupMaxLevel(string groupKey)
    {
        return SkillDataManager.Instance.GetAllSkills()
            .Count(skill => skill.group == groupKey);
    }

    public List<int> GetSkillEffectValues(string groupKey)
    {
        return SkillDataManager.Instance.GetAllSkills()
            .Where(skill => skill.group == groupKey)
            .OrderBy(skill => skill.level)
            .Select(skill => skill.skillEffectValue)
            .ToList();
    }

    public SkillData GetRepresentativeSkill(string groupKey)
    {
        return SkillDataManager.Instance.GetAllSkills()
            .FirstOrDefault(skill => skill.group == groupKey && skill.level == 1);
    }

    public bool CanLearnSkill(string skillKey)
    {
        var skill = SkillDataManager.Instance.GetSkillData(skillKey);
        if (skill == null) return false;

        if (GetSkillLevel(skillKey) >= skill.maxLevel) return false;
        if (saveData.skillPoints < skill.costSkillPoint) return false;
        if (skill.unlockLevel > saveData.currentLevel) return false;

        if (!string.IsNullOrEmpty(skill.prerequisiteSkill1) && GetSkillLevel(skill.prerequisiteSkill1) <= 0)
            return false;
        if (!string.IsNullOrEmpty(skill.prerequisiteSkill2) && GetSkillLevel(skill.prerequisiteSkill2) <= 0)
            return false;

        return true;
    }

    public bool LearnSkill(string skillKey)
    {
        if (!CanLearnSkill(skillKey)) return false;

        saveData.learnedSkills[skillKey] = 1;
        var skill = SkillDataManager.Instance.GetSkillData(skillKey);
        saveData.skillPoints -= skill.costSkillPoint;

        RecalculateAllEffects();
        Debug.Log($"[PlayerSkillManager] 스킬 {skillKey} 습득 완료 (그룹: {skill.group})");
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

            int totalValue = skill.skillEffectValue; // 누적형 → 1레벨 스킬당 value만큼
            var key = (skill.skillEffect, skill.targetKey);

            if (!cachedSkillEffects.ContainsKey(key))
                cachedSkillEffects[key] = 0;

            cachedSkillEffects[key] += totalValue;
        }
    }
}

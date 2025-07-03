// SkillDataManager.cs
using UnityEngine;
using System.Collections.Generic;
using System;

public class SkillDataManager : MonoBehaviour
{
    public static SkillDataManager Instance;

    private Dictionary<string, SkillData> skillDataMap = new Dictionary<string, SkillData>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadSkillData();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public SkillData GetSkillData(string key)
    {
        if (skillDataMap.TryGetValue(key, out var data))
        {
            return data;
        }

        Debug.LogWarning($"[SkillDataManager] Skill ID {key} 를 찾을 수 없습니다.");
        return null;
    }

    private void LoadSkillData()
    {
        TextAsset csvFile = Resources.Load<TextAsset>("SkillTable"); // Resources/SkillTable.csv
        if (csvFile == null)
        {
            Debug.LogError("[SkillDataManager] SkillTable.csv 를 찾을 수 없습니다.");
            return;
        }

        string[] lines = csvFile.text.Split('\n');

        for (int i = 4; i < lines.Length; i++) // 첫 4줄 헤더 스킵
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            string[] values = line.Split(',');
            try
            {
                var skill = ParseSkillData(values);
                skillDataMap[skill.key] = skill;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SkillDataManager] Parse 실패 at line {i + 1}: '{line}'\nException: {e}");
            }
        }
    }

    public IEnumerable<SkillData> GetAllSkills()
    {
        return skillDataMap.Values;
    }

    private SkillData ParseSkillData(string[] values)
    {
        var skill = new SkillData();
        int index = 0;

        skill.key = ParseStringSafe(values[index++], "key");
        skill.category = ParseEnumSafe(values[index++], SkillData.SkillCategory.Normal);
        skill.tag = ParseEnumSafe(values[index++], SkillData.SkillTag.Production);

        skill.coordX = ParseIntSafe(values[index++], "coordX");
        skill.coordY = ParseIntSafe(values[index++], "coordY");

        skill.costSkillPoint = ParseIntSafe(values[index++], "costSkillPoint");
        skill.costResourceType = ParseEnumSafe(values[index++], ResourceType.None);
        skill.costResourceValue = ParseIntSafe(values[index++], "costResourceValue");

        skill.unlockLevel = ParseIntSafe(values[index++], "unlockLevel");
        skill.unlockBoardKey = ParseStringSafe(values[index++], "unlockBoardKey");

        skill.skillEffect = ParseEnumSafe(values[index++], SkillData.SkillEffect.DamageAdd);
        skill.targetKey = ParseStringSafe(values[index++], "targetKey");
        skill.skillEffectValue = ParseIntSafe(values[index++], "skillEffectValue");
        skill.isPercent = ParseBoolSafe(values[index++], "isPercent");

        skill.skillNameKey = ParseStringSafe(values[index++], "skillNameKey");
        skill.skillDescKey = ParseStringSafe(values[index++], "skillDescKey");
        skill.imageName = ParseStringSafe(values[index++], "imageName");

        return skill;
    }

    private int ParseIntSafe(string value, string fieldName)
    {
        value = value.Trim().ToLower();
        if (string.IsNullOrEmpty(value) || value == "null")
            return 0;

        if (int.TryParse(value, out int result))
            return result;

        Debug.LogError($"[SkillDataManager] int 파싱 실패: '{value}' (필드: {fieldName})");
        return 0;
    }

    private T ParseEnumSafe<T>(string value, T defaultValue) where T : struct
    {
        value = value.Trim();
        if (string.IsNullOrEmpty(value) || value.ToLower() == "null")
            return defaultValue;

        if (Enum.TryParse<T>(value, true, out var result))
            return result;

        Debug.LogError($"[SkillDataManager] enum 파싱 실패: '{value}' → 기본값 {defaultValue} 반환");
        return defaultValue;
    }

    private string ParseStringSafe(string value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;

        string trimmed = value.Trim();
        if (trimmed.Equals("null", StringComparison.OrdinalIgnoreCase))
            return null;

        return trimmed;
    }

    private bool ParseBoolSafe(string value, string fieldName)
    {
        value = value.Trim().ToLower();
        if (string.IsNullOrEmpty(value) || value == "null") return false;

        if (bool.TryParse(value, out var result))
            return result;

        // 1/0 또는 yes/no, y/n 지원
        if (value == "1" || value == "yes" || value == "y")
            return true;
        if (value == "0" || value == "no" || value == "n")
            return false;

        Debug.LogError($"[SkillDataManager] bool 파싱 실패: '{value}' (필드: {fieldName})");
        return false;
    }
}

// SkillRequireManager.cs
using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

[DefaultExecutionOrder(-800)]
public class SkillRequireManager : MonoBehaviour
{
    public static SkillRequireManager Instance;

    private List<SkillRequireData> requireList = new List<SkillRequireData>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadRequireData();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public static SkillRequireManager Ensure()
    {
        if (Instance == null)
        {
            var go = new GameObject("SkillRequireManager(Auto)");
            go.AddComponent<SkillRequireManager>();
        }
        return Instance;
    }

    private void LoadRequireData()
    {
        TextAsset csvFile = Resources.Load<TextAsset>("SkillRequireTable"); // Resources/SkillRequireTable.csv
        if (csvFile == null)
        {
            Debug.LogError("[SkillRequireManager] SkillRequireTable.csv 를 찾을 수 없습니다.");
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
                var data = ParseSkillRequireData(values);
                requireList.Add(data);
            }
            catch (Exception e)
            {
                Debug.LogError($"[SkillRequireManager] Parse 실패 at line {i + 1}: '{line}'\nException: {e}");
            }
        }
    }

    private SkillRequireData ParseSkillRequireData(string[] values)
    {
        var data = new SkillRequireData();
        int index = 0;

        data.requiredSkillKey = ParseStringSafe(values[index++], "requiredSkillKey");
        data.nextSkillKey = ParseStringSafe(values[index++], "nextSkillKey");

        return data;
    }

    public List<string> GetNextSkills(string requiredSkillKey)
    {
        return requireList
            .Where(r => r.requiredSkillKey == requiredSkillKey)
            .Select(r => r.nextSkillKey)
            .ToList();
    }

    public List<string> GetRequiredSkills(string nextSkillKey)
    {
        return requireList
            .Where(r => r.nextSkillKey == nextSkillKey)
            .Select(r => r.requiredSkillKey)
            .ToList();
    }

    private string ParseStringSafe(string value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;

        string trimmed = value.Trim();
        if (trimmed.Equals("null", StringComparison.OrdinalIgnoreCase))
            return null;

        return trimmed;
    }
}

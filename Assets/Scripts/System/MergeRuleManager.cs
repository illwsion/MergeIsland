// MergeRuleManager.cs
using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Progress;

public class MergeRuleManager : MonoBehaviour
{
    public static MergeRuleManager Instance;

    private List<MergeRule> rules = new List<MergeRule>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadRulesFromCSV();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void LoadRulesFromCSV()
    {
        TextAsset csvFile = Resources.Load<TextAsset>("MergeTable");
        if (csvFile == null)
        {
            Debug.LogError("[MergeRuleManager] MergeTable.csv �� ã�� �� �����ϴ�.");
            return;
        }

        string[] lines = csvFile.text.Split('\n');

        for (int i = 4; i < lines.Length; i++) // �� ���� ���
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            string[] values = line.Split(',');

            string key = values[0].Trim();
            string note = values[1].Trim();
            string receiverItem = values[2].Trim();
            string suppliedItem = values[3].Trim();
            string result = values[4].Trim();
            bool allowSwap = bool.Parse(values[5].Trim().ToLower());

            rules.Add(new MergeRule(key, note, receiverItem, suppliedItem, result, allowSwap));
        }
    }

    public string? GetMergeResult(string a, string b)
    {
        foreach (var rule in rules)
        {
            if (rule.Matches(a, b))
                return rule.resultItem;
        }
        return null;
    }

    public bool CanMerge(string a, string b)
    {
        return GetMergeResult(a, b) != null;
    }
}

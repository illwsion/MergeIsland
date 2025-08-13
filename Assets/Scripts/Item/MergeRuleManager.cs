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
            Debug.LogError("[MergeRuleManager] MergeTable.csv 를 찾을 수 없습니다.");
            return;
        }

        string[] lines = csvFile.text.Split('\n');

        for (int i = 4; i < lines.Length; i++) // 네 줄은 헤더
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

    public string GetMergeResult(string a, string b)
    {
        foreach (var rule in rules)
        {
            if (rule.Matches(a, b))
                return rule.resultItem;
        }
        return "null";//야매코드긴 하지만...CanMergeWith로 확인하고 왔기 때문에 괜찮음
    }

    public bool CanMerge(string a, string b)
    {
        return GetMergeResult(a, b) != null;
    }
}
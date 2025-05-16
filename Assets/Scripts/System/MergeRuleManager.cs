// MergeRuleManager.cs
using System.Collections.Generic;
using UnityEngine;

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

            int id = int.Parse(values[0]);
            string note = values[1].Trim();
            int receiverItem = int.Parse(values[2]);
            int suppliedItem = int.Parse(values[3]);
            int result = int.Parse(values[4]);
            bool allowSwap = bool.Parse(values[5].Trim().ToLower());

            rules.Add(new MergeRule(id, note, receiverItem, suppliedItem, result, allowSwap));
        }
    }

    public int? GetMergeResult(int a, int b)
    {
        foreach (var rule in rules)
        {
            if (rule.Matches(a, b))
                return rule.resultItem;
        }
        return null;
    }

    public bool CanMerge(int a, int b)
    {
        return GetMergeResult(a, b).HasValue;
    }
}

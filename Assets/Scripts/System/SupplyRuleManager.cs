using System.Collections.Generic;
using UnityEngine;

public class SupplyRuleManager : MonoBehaviour
{
    public static SupplyRuleManager Instance;

    private List<SupplyRule> rules = new List<SupplyRule>();

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
        TextAsset csvFile = Resources.Load<TextAsset>("SupplyTable");
        if (csvFile == null)
        {
            Debug.LogError("[SupplyRuleManager] SupplyTable.csv 를 찾을 수 없습니다.");
            return;
        }

        string[] lines = csvFile.text.Split('\n');

        for (int i = 4; i < lines.Length; i++) // 첫 네 줄은 헤더
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            string[] values = line.Split(',');

            try
            {
                string key = values[0].Trim();
                string note = values[1].Trim();
                string suppliedItem = values[2].Trim();
                string receiverItem = values[3].Trim();
                var resultType = (SupplyRule.ResultType)System.Enum.Parse(typeof(SupplyRule.ResultType), values[4]);
                string resultItem = values[5].Trim();
                int resultValue = int.Parse(values[6]);

                rules.Add(new SupplyRule(key, note, suppliedItem, receiverItem, resultType, resultItem, resultValue));
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[SupplyRuleManager] 파싱 실패: line {i + 1}, {line}\n{e}");
            }
        }
    }

    public SupplyRule GetRule(string a, string b)
    {
        foreach (var rule in rules)
        {
            if (rule.Matches(a, b))
                return rule;
        }
        return null;
    }

    public SupplyRule GetFirstRuleByReceiverItem(string receiverItem)
    {
        Debug.Log(receiverItem);
        foreach (var rule in rules)
        {
            if (rule.receiverItem == receiverItem)
                return rule;
        }
        return null;
    }

    public bool CanSupply(string a, string b)
    {
        return GetRule(a, b) != null;
    }
}

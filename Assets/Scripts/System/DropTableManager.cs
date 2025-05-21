using System.Collections.Generic;
using UnityEngine;

public class DropTableManager : MonoBehaviour
{
    public static DropTableManager Instance;

    private Dictionary<string, DropTableEntry> tableMap = new Dictionary<string, DropTableEntry>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadDropTable();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public DropTableEntry GetTable(string key)
    {
        if (tableMap.TryGetValue(key, out var table))
            return table;

        Debug.LogWarning($"[DropTableManager] ID {key} 테이블을 찾을 수 없습니다.");
        return null;
    }

    private void LoadDropTable()
    {
        TextAsset csvFile = Resources.Load<TextAsset>("DropTable");
        if (csvFile == null)
        {
            Debug.LogError("[DropTableManager] DropTable.csv 를 찾을 수 없습니다.");
            return;
        }

        string[] lines = csvFile.text.Split('\n');

        for (int i = 4; i < lines.Length; i++) // 첫 네 줄은 헤더이므로 스킵
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            string[] values = line.Split(',');

            try
            {
                int index = 0;
                string key = values[index++];

                var results = new List<DropResult>();

                for (int j = 0; j < 5; j++) // item1 ~ item5
                {
                    string itemKey = values[index++].Trim();
                    string chanceStr = values[index++].Trim();

                    if (string.IsNullOrEmpty(itemKey) || itemKey.ToLower() == "null") continue;
                    if (string.IsNullOrEmpty(chanceStr) || chanceStr.ToLower() == "null") continue;

                    int chance = ParseIntSafe(chanceStr, $"item{j + 1}chance");

                    if (itemKey != "null" && chance > 0)
                    {
                        results.Add(new DropResult
                        {
                            itemKey = itemKey,
                            probability = chance
                        });
                    }
                }

                tableMap[key] = new DropTableEntry { key = key, results = results };
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[DropTableManager] 파싱 실패 at line {i + 1}: '{line}'\nException: {e}");
            }
        }
    }

    private int ParseIntSafe(string value, string fieldName)
    {
        value = value.Trim().ToLower();
        if (string.IsNullOrEmpty(value) || value == "null")
            return 0;

        if (int.TryParse(value, out int result))
            return result;

        Debug.LogError($"[DropTableManager] int 파싱 실패: '{value}' (필드: {fieldName})");
        return 0;
    }
}

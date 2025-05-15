using System.Collections.Generic;
using UnityEngine;

public class DropTableManager : MonoBehaviour
{
    public static DropTableManager Instance;

    private Dictionary<int, DropTableEntry> tableMap = new Dictionary<int, DropTableEntry>();

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

    public DropTableEntry GetTable(int id)
    {
        if (tableMap.TryGetValue(id, out var table))
            return table;

        Debug.LogWarning($"[DropTableManager] ID {id} 테이블을 찾을 수 없습니다.");
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
                int id = ParseIntSafe(values[index++], "id");

                var results = new List<DropResult>();

                for (int j = 0; j < 5; j++) // item1 ~ item5
                {
                    string itemStr = values[index++].Trim();
                    string chanceStr = values[index++].Trim();

                    if (string.IsNullOrEmpty(itemStr) || itemStr.ToLower() == "null") continue;
                    if (string.IsNullOrEmpty(chanceStr) || chanceStr.ToLower() == "null") continue;

                    int itemID = ParseIntSafe(itemStr, $"item{j + 1}");
                    int chance = ParseIntSafe(chanceStr, $"item{j + 1}chance");

                    if (itemID != 0 && chance > 0)
                    {
                        results.Add(new DropResult
                        {
                            itemID = itemID,
                            probability = chance
                        });
                    }
                }

                tableMap[id] = new DropTableEntry { id = id, results = results };
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

using System.Collections.Generic;
using UnityEngine;

public class ProduceTableManager : MonoBehaviour
{
    public static ProduceTableManager Instance;

    private Dictionary<string, ProduceTableEntry> tableMap = new Dictionary<string, ProduceTableEntry>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadProduceTable();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public ProduceTableEntry GetTable(string key)
    {
        if (string.IsNullOrEmpty(key))
        {
            Debug.LogWarning("[ProduceTableManager] key�� null �Ǵ� ��� ����");
            return null;
        }

        if (tableMap.TryGetValue(key, out var table))
            return table;

        Debug.LogWarning($"[ProduceTableManager] ID {key} ���̺��� ã�� �� �����ϴ�.");
        return null;
    }

    private void LoadProduceTable()
    {
        TextAsset csvFile = Resources.Load<TextAsset>("ProduceTable");
        if (csvFile == null)
        {
            Debug.LogError("[ProduceTableManager] ProduceTable.csv �� ã�� �� �����ϴ�.");
            return;
        }

        string[] lines = csvFile.text.Split('\n');

        for (int i = 4; i < lines.Length; i++) // ù �� ���� ����̹Ƿ� ��ŵ
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            string[] values = line.Split(',');

            try
            {
                int index = 0;
                string key = values[index++];

                var results = new List<ProduceResult>();

                for (int j = 0; j < 5; j++) // item1 ~ item5
                {
                    string itemKey = values[index++].Trim();
                    string chanceStr = values[index++].Trim();

                    if (string.IsNullOrEmpty(itemKey) || itemKey.ToLower() == "null") continue;
                    if (string.IsNullOrEmpty(chanceStr) || chanceStr.ToLower() == "null") continue;

                    int chance = ParseIntSafe(chanceStr, $"item{j + 1}chance");

                    if (itemKey != null && chance > 0)
                    {
                        results.Add(new ProduceResult
                        {
                            itemKey = itemKey,
                            probability = chance
                        });
                    }
                }

                tableMap[key] = new ProduceTableEntry { key = key, results = results };
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[ProduceTableManager] �Ľ� ���� at line {i + 1}: '{line}'\nException: {e}");
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

        Debug.LogError($"[ProduceTableManager] int �Ľ� ����: '{value}' (�ʵ�: {fieldName})");
        return 0;
    }
}

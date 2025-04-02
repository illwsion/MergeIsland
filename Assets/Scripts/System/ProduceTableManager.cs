using System.Collections.Generic;
using UnityEngine;

public class ProduceTableManager : MonoBehaviour
{
    public static ProduceTableManager Instance;

    private Dictionary<int, ProduceTableEntry> tableMap = new Dictionary<int, ProduceTableEntry>();

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

    public ProduceTableEntry GetTable(int id)
    {
        if (tableMap.TryGetValue(id, out var table))
            return table;

        Debug.LogWarning($"[ProduceTableManager] ID {id} ���̺��� ã�� �� �����ϴ�.");
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
                int id = ParseIntSafe(values[index++], "id");

                var results = new List<ProduceResult>();

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
                        results.Add(new ProduceResult
                        {
                            itemID = itemID,
                            probability = chance
                        });
                    }
                }

                tableMap[id] = new ProduceTableEntry { id = id, results = results };
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[ProduceTableManager] �Ľ� ���� at line {i + 1}: '{line}'\nException: {e}");
            }
        }
        Debug.Log($"[ProduceTableManager] {tableMap.Count}���� �������̺��� �ҷ��Խ��ϴ�.");
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

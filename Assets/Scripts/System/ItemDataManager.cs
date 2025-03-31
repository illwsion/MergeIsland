// ItemDataManager.cs

using UnityEngine;
using System.Collections.Generic;
using System.IO;
using static ItemData;
using System;

public class ItemDataManager : MonoBehaviour
{
    public static ItemDataManager Instance;

    private Dictionary<int, ItemData> itemDataMap = new Dictionary<int, ItemData>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadItemData();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public ItemData GetItemData(int id)
    {
        if (itemDataMap.TryGetValue(id, out var data))
        {
            Debug.Log("아이템 데이터 불러오기");
            return data;
        }
           

        Debug.LogWarning($"[ItemDataManager] ID {id} 아이템 데이터를 찾을 수 없습니다.");
        return null;
    }

    private void LoadItemData()
    {
        TextAsset csvFile = Resources.Load<TextAsset>("ItemTable"); // Resources/ItemTable.csv
        if (csvFile == null)
        {
            Debug.LogError("[ItemDataManager] ItemTable.csv 를 찾을 수 없습니다.");
            return;
        }

        string[] lines = csvFile.text.Split('\n');

        // 첫 줄은 헤더이므로 스킵
        for (int i = 4; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            string[] values = line.Split(',');
            try
            {
                var item = ParseItemData(values);
                itemDataMap[item.id] = item;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[ItemDataManager] Parse 실패 at line {i + 1}: '{line}'\nException: {e}");
            }
        }

        Debug.Log($"[ItemDataManager] {itemDataMap.Count}개의 아이템 데이터를 불러왔습니다.");
    }

    private ItemData ParseItemData(string[] values)
    {
        var item = new ItemData();
        int index = 0;

        item.id = ParseIntSafe(values[index++], "id");
        item.name = values[index++];
        item.category = ParseEnumSafe(values[index++], ItemData.Category.Production);
        item.level = ParseIntSafe(values[index++], "level");
        item.maxLevel = ParseIntSafe(values[index++], "maxLevel");
        item.produceType = ParseEnumSafe(values[index++], ItemData.ProduceType.None);
        item.produceTableID = ParseIntSafe(values[index++], "produceTableID");
        item.costResource = ParseEnumSafe(values[index++], ItemData.CostResource.None);
        item.costValue = ParseIntSafe(values[index++], "costValue");
        item.gatherResource = ParseEnumSafe(values[index++], ItemData.GatherResource.None);
        item.gatherValue = ParseIntSafe(values[index++], "gatherValue");
        item.productionInterval = ParseFloatSafe(values[index++], "productionInterval");
        item.maxProductionAmount = ParseIntSafe(values[index++], "maxProductionAmount");
        item.isSellable = ParseBoolSafe(values[index++], "isSellable");
        item.sellValue = ParseIntSafe(values[index++], "sellValue");
        item.descriptionID = ParseIntSafe(values[index++], "descriptionID");
        item.canMove = ParseBoolSafe(values[index++], "canMove");
        item.canInventoryStore = ParseBoolSafe(values[index++], "canInventoryStore");

        return item;
    }
    private int ParseIntSafe(string value, string fieldName)
    {
        value = value.Trim().ToLower();
        if (string.IsNullOrEmpty(value) || value == "null")
            return 0;

        if (int.TryParse(value, out int result))
            return result;

        Debug.LogError($"[ItemDataManager] int 파싱 실패: '{value}' (필드: {fieldName})");
        return 0;
    }

    private float ParseFloatSafe(string value, string fieldName)
    {
        value = value.Trim().ToLower();
        if (string.IsNullOrEmpty(value) || value == "null")
            return 0f;

        if (float.TryParse(value, out float result))
            return result;

        Debug.LogError($"[ItemDataManager] float 파싱 실패: '{value}' (필드: {fieldName})");
        return 0f;
    }
    private T ParseEnumSafe<T>(string value, T defaultValue) where T : struct
    {
        value = value.Trim();
        if (string.IsNullOrEmpty(value) || value.ToLower() == "null")
            return defaultValue;

        if (System.Enum.TryParse<T>(value, true, out var result))
            return result;

        Debug.LogError($"[ItemDataManager] enum 파싱 실패: '{value}' → 기본값 {defaultValue} 반환");
        return defaultValue;
    }
    private bool ParseBoolSafe(string value, string fieldName)
    {
        value = value.Trim().ToLower();
        if (value == "true") return true;
        if (value == "false") return false;

        Debug.LogError($"[ItemDataManager] bool 파싱 실패: '{value}' (필드: {fieldName})");
        return false; // 기본값
    }
}

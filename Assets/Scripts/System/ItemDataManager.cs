// ItemDataManager.cs

using UnityEngine;
using System.Collections.Generic;
using System.IO;
using static ItemData;
using System;

public class ItemDataManager : MonoBehaviour
{
    public static ItemDataManager Instance;

    private Dictionary<string, ItemData> itemDataMap = new Dictionary<string, ItemData>();

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

    public ItemData GetItemData(string key)
    {
        if (itemDataMap.TryGetValue(key, out var data))
        {
            return data;
        }
           

        Debug.LogWarning($"[ItemDataManager] ID {key} 아이템 데이터를 찾을 수 없습니다.");
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

        // 첫 네 줄은 헤더이므로 스킵
        for (int i = 4; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            string[] values = line.Split(',');
            try
            {
                var item = ParseItemData(values);
                itemDataMap[item.key] = item;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[ItemDataManager] Parse 실패 at line {i + 1}: '{line}'\nException: {e}");
            }
        }
    }

    private ItemData ParseItemData(string[] values)
    {
        var item = new ItemData();
        int index = 0;

        item.key = ParseStringSafe(values[index++], "key");
        item.name = ParseStringSafe(values[index++], "name");
        item.type = ParseStringSafe(values[index++], "type");
        item.category = ParseEnumSafe(values[index++], ItemData.Category.Production);
        item.level = ParseIntSafe(values[index++], "level");
        item.maxLevel = ParseIntSafe(values[index++], "maxLevel");
        item.produceType = ParseEnumSafe(values[index++], ItemData.ProduceType.None);
        item.isProductionLimited = ParseBoolSafe(values[index++], "isProductionLimited");
        item.toolType = ParseEnumSafe(values[index++], ItemData.ToolType.None);
        item.targetMaterial = ParseEnumSafe(values[index++], ItemData.TargetMaterial.None);
        item.produceTableKey = ParseStringSafe(values[index++], "produceTableKey");
        item.dropTableKey = ParseStringSafe(values[index++], "dropTableKey");
        item.costResource = ParseEnumSafe(values[index++], ResourceType.None);
        item.costValue = ParseIntSafe(values[index++], "costValue");
        item.gatherResource = ParseEnumSafe(values[index++], ResourceType.None);
        item.gatherValue = ParseIntSafe(values[index++], "gatherValue");
        item.productionInterval = ParseFloatSafe(values[index++], "productionInterval");
        item.maxProductionAmount = ParseIntSafe(values[index++], "maxProductionAmount");
        item.isSellable = ParseBoolSafe(values[index++], "isSellable");
        item.sellValue = ParseIntSafe(values[index++], "sellValue");
        item.itemNameKey = ParseStringSafe(values[index++], "itemNameKey");
        item.itemDescriptionKey = ParseStringSafe(values[index++], "itemDescriptionKey");
        item.canMove = ParseBoolSafe(values[index++], "canMove");
        item.canInventoryStore = ParseBoolSafe(values[index++], "canInventoryStore");
        item.hp = ParseIntSafe(values[index++], "hp");
        item.attackPower = ParseIntSafe(values[index++], "attackPower");
        item.imageName = ParseStringSafe(values[index++], "imageName");

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

    private string ParseStringSafe(string value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;

        string trimmed = value.Trim();
        if (trimmed.Equals("null", StringComparison.OrdinalIgnoreCase))
            return null;

        return trimmed;
    }
}

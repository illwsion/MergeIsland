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
            Debug.Log("������ ������ �ҷ�����");
            return data;
        }
           

        Debug.LogWarning($"[ItemDataManager] ID {id} ������ �����͸� ã�� �� �����ϴ�.");
        return null;
    }

    private void LoadItemData()
    {
        TextAsset csvFile = Resources.Load<TextAsset>("ItemTable"); // Resources/ItemTable.csv
        if (csvFile == null)
        {
            Debug.LogError("[ItemDataManager] ItemTable.csv �� ã�� �� �����ϴ�.");
            return;
        }

        string[] lines = csvFile.text.Split('\n');

        // ù ���� ����̹Ƿ� ��ŵ
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
                Debug.LogError($"[ItemDataManager] Parse ���� at line {i + 1}: '{line}'\nException: {e}");
            }
        }

        Debug.Log($"[ItemDataManager] {itemDataMap.Count}���� ������ �����͸� �ҷ��Խ��ϴ�.");
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

        Debug.LogError($"[ItemDataManager] int �Ľ� ����: '{value}' (�ʵ�: {fieldName})");
        return 0;
    }

    private float ParseFloatSafe(string value, string fieldName)
    {
        value = value.Trim().ToLower();
        if (string.IsNullOrEmpty(value) || value == "null")
            return 0f;

        if (float.TryParse(value, out float result))
            return result;

        Debug.LogError($"[ItemDataManager] float �Ľ� ����: '{value}' (�ʵ�: {fieldName})");
        return 0f;
    }
    private T ParseEnumSafe<T>(string value, T defaultValue) where T : struct
    {
        value = value.Trim();
        if (string.IsNullOrEmpty(value) || value.ToLower() == "null")
            return defaultValue;

        if (System.Enum.TryParse<T>(value, true, out var result))
            return result;

        Debug.LogError($"[ItemDataManager] enum �Ľ� ����: '{value}' �� �⺻�� {defaultValue} ��ȯ");
        return defaultValue;
    }
    private bool ParseBoolSafe(string value, string fieldName)
    {
        value = value.Trim().ToLower();
        if (value == "true") return true;
        if (value == "false") return false;

        Debug.LogError($"[ItemDataManager] bool �Ľ� ����: '{value}' (�ʵ�: {fieldName})");
        return false; // �⺻��
    }
}

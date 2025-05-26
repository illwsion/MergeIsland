// BoardGateManager.cs
using UnityEngine;
using System.Collections.Generic;
using System;

public class BoardGateManager : MonoBehaviour
{
    public static BoardGateManager Instance;

    private Dictionary<(string, BoardGateData.Direction), BoardGateData> gateDataMap = new();
    private HashSet<(string, BoardGateData.Direction)> unlockedGates = new();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadBoardGateData();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public BoardGateData GetGateData(string boardKey, BoardGateData.Direction direction)
    {
        gateDataMap.TryGetValue((boardKey, direction), out var data);
        return data;
    }

    public List<BoardGateData> GetGatesForBoard(string boardKey)
    {
        List<BoardGateData> result = new();
        foreach (var pair in gateDataMap)
        {
            if (pair.Key.Item1 == boardKey)
            {
                result.Add(pair.Value);
            }
        }
        return result;
    }

    private void LoadBoardGateData()
    {
        TextAsset csvFile = Resources.Load<TextAsset>("BoardGateTable");
        if (csvFile == null)
        {
            Debug.LogError("[BoardGateManager] BoardGateTable.csv 를 찾을 수 없습니다.");
            return;
        }

        string[] lines = csvFile.text.Split('\n');

        for (int i = 4; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            string[] values = line.Split(',');
            try
            {
                var data = ParseBoardGateData(values);
                gateDataMap[(data.boardKey, data.direction)] = data;
            }
            catch (Exception e)
            {
                Debug.LogError($"[BoardGateManager] Parse 실패 at line {i + 1}: '{line}'\nException: {e}");
            }
        }
    }

    private BoardGateData ParseBoardGateData(string[] values)
    {
        int index = 0;
        var data = new BoardGateData
        {
            boardKey = ParseStringSafe(values[index++], "boardKey"),
            direction = ParseEnumSafe(values[index++], BoardGateData.Direction.Top),
            targetBoardKey = ParseStringSafe(values[index++], "targetBoardKey"),
            isLocked = ParseBoolSafe(values[index++], "isLocked"),
            unlockType = ParseEnumSafe(values[index++], BoardGateData.UnlockType.None),
            unlockParam = ParseStringSafe(values[index++], "unlockParam"),
            unlockParamValue = ParseIntSafe(values[index++], "unlockParamValue")
        };

        return data;
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

    private string ParseStringSafe(string value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        string trimmed = value.Trim();
        return trimmed.Equals("null", StringComparison.OrdinalIgnoreCase) ? null : trimmed;
    }

    private bool ParseBoolSafe(string value, string fieldName)
    {
        value = value.Trim().ToLower();
        if (value == "true") return true;
        if (value == "false") return false;

        Debug.LogError($"[BoardGateManager] bool 파싱 실패: '{value}' (필드: {fieldName})");
        return false;
    }

    private T ParseEnumSafe<T>(string value, T defaultValue) where T : struct
    {
        value = value.Trim();
        if (string.IsNullOrEmpty(value) || value.ToLower() == "null")
            return defaultValue;

        if (Enum.TryParse<T>(value, true, out var result))
            return result;

        Debug.LogError($"[BoardGateManager] enum 파싱 실패: '{value}' → 기본값 {defaultValue} 반환");
        return defaultValue;
    }


    public void MarkGateUnlocked(BoardGateData gateData)
    {
        var id = gateData.GetUniqueID();
        var save = SaveController.Instance.CurrentSave;

        if (!save.unlockedGates.Contains(id))
        {
            save.unlockedGates.Add(id);
            Debug.Log($"[BoardGateManager] 게이트 저장: {id} → 해제됨");
        }
    }

    public void LoadUnlockedGates(GameSaveData save)
    {
        foreach (var id in save.unlockedGates)
        {
            foreach (var gate in gateDataMap.Values)
            {
                if ($"{gate.boardKey}_{gate.direction}" == id)
                {
                    gate.isLocked = false;
                }
            }
        }
    }

}

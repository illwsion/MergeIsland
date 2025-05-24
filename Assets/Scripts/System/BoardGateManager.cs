using UnityEngine;
using System.Collections.Generic;
using System;

public class BoardGateManager : MonoBehaviour
{
    public static BoardGateManager Instance;

    private Dictionary<(string, string), BoardGate> gateMap = new();

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

    public BoardGate GetGate(string fromBoard, string direction)
    {
        gateMap.TryGetValue((fromBoard, direction), out var gate);
        return gate;
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
                var gate = ParseBoardGate(values);
                gateMap[(gate.fromBoard, gate.direction)] = gate;
            }
            catch (Exception e)
            {
                Debug.LogError($"[BoardGateManager] Parse 실패 at line {i + 1}: '{line}'\nException: {e}");
            }
        }
    }

    private BoardGate ParseBoardGate(string[] values)
    {
        var gate = new BoardGate();
        int index = 0;

        gate.fromBoard = ParseStringSafe(values[index++], "fromBoard");
        gate.direction = ParseStringSafe(values[index++], "direction");
        gate.toBoard = ParseStringSafe(values[index++], "toBoard");
        gate.isLocked = ParseBoolSafe(values[index++], "isLocked");
        gate.unlockType = ParseEnumSafe(values[index++], UnlockType.None);
        gate.unlockParam = ParseStringSafe(values[index++], "unlockParam");

        return gate;
    }

    private string ParseStringSafe(string value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;

        string trimmed = value.Trim();
        if (trimmed.Equals("null", StringComparison.OrdinalIgnoreCase))
            return null;

        return trimmed;
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
}

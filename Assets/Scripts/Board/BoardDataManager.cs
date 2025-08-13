// BoardDataManager.cs
using UnityEngine;
using System.Collections.Generic;

[DefaultExecutionOrder(-850)]
public class BoardDataManager : MonoBehaviour
{
    public static BoardDataManager Instance;

    private Dictionary<string, BoardData> boardDataMap = new Dictionary<string, BoardData>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadBoardData();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public BoardData GetBoardData(string key)
    {
        if (boardDataMap.TryGetValue(key, out var data))
        {
            return data;
        }

        Debug.LogWarning($"[BoardDataManager] 보드 {key} 를 찾을 수 없습니다.");
        return null;
    }

    public IEnumerable<BoardData> GetAllBoardData()
    {
        return boardDataMap.Values;
    }

    private void LoadBoardData()
    {
        TextAsset csvFile = Resources.Load<TextAsset>("BoardTable"); // Resources/BoardTable.csv
        if (csvFile == null)
        {
            Debug.LogError("[BoardDataManager] BoardTable.csv 를 찾을 수 없습니다.");
            return;
        }

        string[] lines = csvFile.text.Split('\n');

        for (int i = 4; i < lines.Length; i++) // 첫 4줄 헤더 스킵
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            string[] values = line.Split(',');
            try
            {
                var board = ParseBoardData(values);
                boardDataMap[board.key] = board;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[BoardDataManager] Parse 실패 at line {i + 1}: '{line}'\nException: {e}");
            }
        }
    }

    private BoardData ParseBoardData(string[] values)
    {
        int index = 0;
        var data = new BoardData();

        data.key = ParseStringSafe(values[index++], "key");
        int x = ParseIntSafe(values[index++], "worldX");
        int y = ParseIntSafe(values[index++], "worldY");
        data.worldPos = new Vector2Int(x, y);
        data.width = ParseIntSafe(values[index++], "width");
        data.height = ParseIntSafe(values[index++], "height");
        data.theme = ParseEnumSafe(values[index++], BoardData.BoardTheme.None);
        data.exitTop = ParseBoolSafe(values[index++], "exitTop");
        data.exitRight = ParseBoolSafe(values[index++], "exitRight");
        data.exitBottom = ParseBoolSafe(values[index++], "exitBottom");
        data.exitLeft = ParseBoolSafe(values[index++], "exitLeft");
        data.nameKey = ParseStringSafe(values[index++], "nameKey");
        data.descriptionKey = ParseStringSafe(values[index++], "descriptionKey");

        return data;
    }

    private int ParseIntSafe(string value, string fieldName)
    {
        value = value.Trim().ToLower();
        if (string.IsNullOrEmpty(value) || value == "null")
            return 0;

        if (int.TryParse(value, out int result))
            return result;

        Debug.LogError($"[BoardDataManager] int 파싱 실패: '{value}' (필드: {fieldName})");
        return 0;
    }

    private T ParseEnumSafe<T>(string value, T defaultValue) where T : struct
    {
        value = value.Trim();
        if (string.IsNullOrEmpty(value) || value.ToLower() == "null")
            return defaultValue;

        if (System.Enum.TryParse<T>(value, true, out var result))
            return result;

        Debug.LogError($"[BoardDataManager] enum 파싱 실패: '{value}' → 기본값 {defaultValue} 반환");
        return defaultValue;
    }

    private bool ParseBoolSafe(string value, string fieldName)
    {
        value = value.Trim().ToLower();
        if (value == "true") return true;
        if (value == "false") return false;

        Debug.LogError($"[BoardDataManager] bool 파싱 실패: '{value}' (필드: {fieldName})");
        return false;
    }

    private string ParseStringSafe(string value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;

        string trimmed = value.Trim();
        if (trimmed.Equals("null", System.StringComparison.OrdinalIgnoreCase))
            return null;

        return trimmed;
    }
}

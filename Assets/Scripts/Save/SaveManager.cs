// SaveManager.cs
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System;

public static class SaveManager
{
    private static readonly string SavePath = Path.Combine(Application.persistentDataPath, "game_save.json");

    public static void Save(GameSaveData data)
    {
        data.lastSaveTime = DateTime.UtcNow.ToString("o");
        data.boardList = new List<BoardSaveData>(data.boards.Values);

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(SavePath, json);
        Debug.Log($"[SaveManager] ���� �Ϸ�: {SavePath}");
    }

    public static GameSaveData Load()
    {
        if (!File.Exists(SavePath))
        {
            Debug.LogWarning("[SaveManager] ���� ������ �����ϴ�. �� ������ ����.");
            return new GameSaveData();
        }

        string json = File.ReadAllText(SavePath);
        GameSaveData data = JsonUtility.FromJson<GameSaveData>(json);
        Debug.Log($"[SaveManager] �ε� �Ϸ�: {SavePath}");

        // ����Ʈ�� ��ųʸ��� ��ȯ
        data.boards = new Dictionary<string, BoardSaveData>();
        if (data.boardList != null)
        {
            foreach (var b in data.boardList)
            {
                data.boards[b.boardKey] = b;
            }
        }

        return data;
    }
}

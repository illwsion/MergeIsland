// SaveManager.cs
using UnityEngine;
using System.IO;

public static class SaveManager
{
    private static readonly string SavePath = Path.Combine(Application.persistentDataPath, "player_save.json");

    public static void SavePlayer(PlayerData data)
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(SavePath, json);
        Debug.Log($"[SaveManager] 저장 완료: {SavePath}");
    }

    public static PlayerData LoadPlayer()
    {
        if (!File.Exists(SavePath))
        {
            Debug.LogWarning("[SaveManager] 저장 파일이 없습니다. 새 데이터 생성.");
            return new PlayerData();
        }

        string json = File.ReadAllText(SavePath);
        PlayerData data = JsonUtility.FromJson<PlayerData>(json);
        Debug.Log($"[SaveManager] 로드 완료: {SavePath}");
        return data;
    }
}

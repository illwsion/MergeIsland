using System;
using UnityEngine;

public class SaveController : MonoBehaviour
{
    public static SaveController Instance { get; private set; }

    public GameSaveData CurrentSave { get; private set; }

    private float offlineElapsedTime = 0f;

    public float GetOfflineElapsedTime() => offlineElapsedTime;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            CurrentSave = SaveManager.Load();

            // �������� �ð�
            if (DateTime.TryParse(CurrentSave.lastSaveTime, null, System.Globalization.DateTimeStyles.RoundtripKind, out var lastTime))
            {
                offlineElapsedTime = (float)(DateTime.UtcNow - lastTime).TotalSeconds;
                Debug.Log($"[SaveController] �������� �ð�: {offlineElapsedTime}��");
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Save()
    {
        SaveManager.Save(CurrentSave);
    }
}

using System;
using System.Collections;
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
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private IEnumerator Start()
    {
        yield return null;

        if (DateTime.TryParse(CurrentSave.lastSaveTime, null, System.Globalization.DateTimeStyles.RoundtripKind, out var lastTime))
        {
            offlineElapsedTime = (float)(DateTime.UtcNow - lastTime).TotalSeconds;
            Debug.Log($"[SaveController] �������� �ð�: {offlineElapsedTime}��");
        }
    }

    public void Save()
    {
        PlayerResourceManager.Instance.SaveTo(CurrentSave.player);
        Debug.Log($"[SaveController] Save ȣ���. Player Energy: {PlayerResourceManager.Instance.GetAmount(ResourceType.Energy)}");
        SaveManager.Save(CurrentSave);
    }
}

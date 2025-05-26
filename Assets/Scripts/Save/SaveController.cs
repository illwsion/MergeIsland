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
            Debug.Log($"[SaveController] 오프라인 시간: {offlineElapsedTime}초");
        }
    }

    public void Save()
    {
        PlayerResourceManager.Instance.SaveTo(CurrentSave.player);
        Debug.Log($"[SaveController] Save 호출됨. Player Energy: {PlayerResourceManager.Instance.GetAmount(ResourceType.Energy)}");
        SaveManager.Save(CurrentSave);
    }
}

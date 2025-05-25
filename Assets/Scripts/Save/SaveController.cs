using UnityEngine;

public class SaveController : MonoBehaviour
{
    public static SaveController Instance { get; private set; }

    public GameSaveData CurrentSave { get; private set; }

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

    public void Save()
    {
        SaveManager.Save(CurrentSave);
    }
}

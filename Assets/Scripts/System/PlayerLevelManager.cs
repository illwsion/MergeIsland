using UnityEngine;
using System.Collections.Generic;
using System;
using System.IO;

public class PlayerLevelManager : MonoBehaviour
{
    public static PlayerLevelManager Instance;
    [SerializeField] private ExpUIManager expUIManager;

    private Dictionary<int, int> expTable = new Dictionary<int, int>();

    public int CurrentLevel { get; private set; } = 1;
    public int CurrentEXP { get; private set; } = 0;
    public int SkillPoints { get; private set; } = 0;

    public Action<int> OnLevelUp;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadEXPTable();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        UpdateUI();
    }

    private void LoadEXPTable()
    {
        TextAsset csvFile = Resources.Load<TextAsset>("EXPTable");
        if (csvFile == null)
        {
            Debug.LogError("[PlayerLevelManager] EXPTable.csv 파일을 찾을 수 없습니다.");
            return;
        }

        string[] lines = csvFile.text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

        // 첫 네 줄은 헤더이므로 스킵
        for (int i = 3; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            string[] values = line.Split(',');

            if (values.Length < 2) continue;

            try
            {
                int level = ParseIntSafe(values[0], "Level");
                int requiredEXP = ParseIntSafe(values[1], "EXP");

                expTable[level] = requiredEXP;
            }
            catch (Exception e)
            {
                Debug.LogError($"[PlayerLevelManager] EXP 테이블 파싱 실패 at line {i + 1}: '{line}'\n{e}");
            }
        }
    }

    public void AddExperience(int amount)
    {
        CurrentEXP += amount;
        CheckLevelUp();
        UpdateUI();
    }

    private void CheckLevelUp()
    {
        while (expTable.ContainsKey(CurrentLevel) && CurrentEXP >= expTable[CurrentLevel])
        {
            CurrentEXP -= expTable[CurrentLevel];
            CurrentLevel++;
            SkillPoints++;
            OnLevelUp?.Invoke(CurrentLevel);
            Debug.Log($"[PlayerLevelManager] 레벨업! 현재 레벨: {CurrentLevel}");
        }
    }

    public int ExpToNextLevel()
    {
        if (!expTable.ContainsKey(CurrentLevel))
            return int.MaxValue;

        return expTable[CurrentLevel] - CurrentEXP;
    }

    private void UpdateUI()
    {
        int required = ExpToNextLevel();
        expUIManager?.UpdateUI(CurrentLevel, CurrentEXP, required);
    }

    // ------------------ Safe Parsers ------------------

    private int ParseIntSafe(string value, string fieldName)
    {
        value = value.Trim().ToLower();
        if (string.IsNullOrEmpty(value) || value == "null")
            return 0;

        if (int.TryParse(value, out int result))
            return result;

        Debug.LogError($"[PlayerLevelManager] int 파싱 실패: '{value}' (필드: {fieldName})");
        return 0;
    }
}

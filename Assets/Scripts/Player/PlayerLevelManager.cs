using UnityEngine;
using System.Collections.Generic;
using System;
using System.IO;

public class PlayerLevelManager : MonoBehaviour
{
    public static PlayerLevelManager Instance;
    [SerializeField] private ExpUIManager expUIManager;

    private Dictionary<int, int> expTable = new Dictionary<int, int>();
    private Dictionary<int, int> skillPointTable = new Dictionary<int, int>();

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
        // 세이브 데이터에서 초기값 로드
        if (SaveController.Instance != null && SaveController.Instance.CurrentSave != null)
        {
            var save = SaveController.Instance.CurrentSave.player;
            CurrentLevel = Math.Max(1, save.currentLevel); // 최소 1레벨 보장
            CurrentEXP = Math.Max(0, save.currentExp);     // 최소 0 경험치 보장
            SkillPoints = Math.Max(0, save.skillPoints);   // 최소 0 스킬포인트 보장
        }
        
        UpdateUI();
    }

    private void LoadEXPTable()
    {
        TextAsset csvFile = Resources.Load<TextAsset>("EXPTable");
        if (csvFile == null)
        {
            Debug.LogError("[PlayerLevelManager] EXPTable.csv 를 찾을 수 없습니다.");
            return;
        }

        string[] lines = csvFile.text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

        // 첫 3줄은 헤더이므로 스킵
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
                int gainSkillPoint = ParseIntSafe(values[2], "gainSkillPoint");
                expTable[level] = requiredEXP;
                skillPointTable[level] = gainSkillPoint;
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
            
            // 안전하게 skillPointTable에 접근
            int skillPointsGained = 0;
            if (skillPointTable.ContainsKey(CurrentLevel))
            {
                skillPointsGained = skillPointTable[CurrentLevel];
            }
            else
            {
                // 해당 레벨이 없으면 기본값 사용
                skillPointsGained = 1;
                Debug.LogWarning($"[PlayerLevelManager] 레벨 {CurrentLevel}에 대한 스킬포인트 데이터가 없습니다. 기본값 1을 사용합니다.");
            }
            
            SkillPoints += skillPointsGained;
            
            // 세이브 데이터에 스킬 포인트 반영
            SyncSkillPointsToSave();
            
            OnLevelUp?.Invoke(CurrentLevel);
            Debug.Log($"[PlayerLevelManager] 레벨업! 현재 레벨: {CurrentLevel}, 획득 스킬포인트: {skillPointsGained}");
        }
    }

    // 세이브 데이터에 스킬 포인트 동기화
    private void SyncSkillPointsToSave()
    {
        if (SaveController.Instance != null && SaveController.Instance.CurrentSave != null)
        {
            SaveController.Instance.CurrentSave.player.skillPoints = SkillPoints;
            SaveController.Instance.CurrentSave.player.currentLevel = CurrentLevel;
            SaveController.Instance.CurrentSave.player.currentExp = CurrentEXP;
        }
    }

    // 외부에서 스킬 포인트를 차감할 때 호출
    public void SpendSkillPoints(int amount)
    {
        if (SkillPoints >= amount)
        {
            SkillPoints -= amount;
            SyncSkillPointsToSave();
        }
    }

    // 외부에서 스킬 포인트를 추가할 때 호출
    public void AddSkillPoints(int amount)
    {
        SkillPoints += amount;
        SyncSkillPointsToSave();
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
        
        // 안전하게 expTable에 접근
        int currentLevelExp = 0;
        if (expTable.ContainsKey(CurrentLevel))
        {
            currentLevelExp = expTable[CurrentLevel];
        }
        else if (CurrentLevel > 0 && expTable.ContainsKey(CurrentLevel - 1))
        {
            // 현재 레벨이 없으면 이전 레벨의 경험치 사용
            currentLevelExp = expTable[CurrentLevel - 1];
        }
        
        expUIManager?.UpdateUI(CurrentLevel, CurrentEXP, currentLevelExp);
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

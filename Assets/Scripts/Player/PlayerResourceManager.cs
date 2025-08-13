// PlayerResourceManager.cs
using UnityEngine;
using System.Collections.Generic;
using System;
using System.Collections;

public class PlayerResourceManager : MonoBehaviour
{
    public static PlayerResourceManager Instance { get; private set; }

    private Dictionary<ResourceType, int> currentResources = new Dictionary<ResourceType, int>();
    private Dictionary<ResourceType, int> itemBonusResourceCap = new Dictionary<ResourceType, int>();
    private List<MergeItem> maxCapItems = new List<MergeItem>();

    [SerializeField] private ResourceUIManager resourceUIManager;

    [Header("Energy Settings")]
    public int MaxEnergy => GlobalGameConfig.BaseResourceCap.TryGetValue(ResourceType.Energy, out var cap) ? cap : 100;
    public float energyRecoveryInterval => GlobalGameConfig.EnergyRecoveryInterval;
    public int energyRecoveryAmount => GlobalGameConfig.EnergyRecoveryAmount;

    private float recoveryTimer = 0f;

    // 외부에서 남은 시간 확인용 프로퍼티
    public float RecoveryRemainingTime => energyRecoveryInterval - recoveryTimer;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializeResources();
            resourceUIManager.Init();
        }
        else
        {
            Destroy(gameObject);
        }

    }

    private IEnumerator Start()
    {
        yield return null;
        LoadFrom(SaveController.Instance.CurrentSave.player);
        float offline = SaveController.Instance.GetOfflineElapsedTime();
        ApplyOfflineEnergyRecovery(offline);
        foreach (ResourceType type in Enum.GetValues(typeof(ResourceType)))
        {
            if (type != ResourceType.None && type != ResourceType.Exp)
                UpdateUI(type);
        }
    }

    private void Update()
    {
        RecoverEnergyOverTime();
    }

    private void RecoverEnergyOverTime()
    {
        int current = GetAmount(ResourceType.Energy);
        if (current >= GetMax(ResourceType.Energy))
        {
            recoveryTimer = 0f; // 최대치면 타이머 초기화
            return;
        }

        recoveryTimer += Time.deltaTime;

        if (recoveryTimer >= energyRecoveryInterval)
        {
            recoveryTimer = 0f;
            Add(ResourceType.Energy, energyRecoveryAmount);
        }
    }

    public void AddEnergy(int amount)
    {
        Add(ResourceType.Energy, amount);
    }

    public bool HasEnough(ResourceType type, int value)
    {
        return currentResources.ContainsKey(type) && currentResources[type] >= value;
    }

    public bool TrySpend(ResourceType type, int value)
    {
        if (!HasEnough(type, value)) return false;

        currentResources[type] -= value;

        UpdateUI(type);
        return true;
    }

    public void Add(ResourceType type, int value)
    {
        if (type == ResourceType.Exp)
        {
            PlayerLevelManager.Instance.AddExperience(value);
            return;
        }

        if (!currentResources.ContainsKey(type))
        {
            Debug.Log($"[PlayerResourceManager.Add] Add 호출됐지만 존재하지 않아서 초기화 type : {type} value : {value}");
            currentResources[type] = 0;
        }
            
        // 스킬 효과 적용 (자원 획득량)
        int flatFromSkill = 0;
        int percentFromSkill = 0;
        if (PlayerSkillManager.Instance != null)
        {
            flatFromSkill = PlayerSkillManager.Instance.GetEffectFlat(SkillData.SkillEffect.ResourceGain, type.ToString());
            percentFromSkill = PlayerSkillManager.Instance.GetEffectPercent(SkillData.SkillEffect.ResourceGain, type.ToString());
        }
        
        // 고정 보너스를 먼저 더하고, 퍼센트는 합산하여 곱 적용
        int basePlusFlat = value + flatFromSkill;
        float multiplier = 1f + (percentFromSkill / 100f);
        int totalValue = Mathf.FloorToInt(basePlusFlat * multiplier);
        
        int current = currentResources[type];
        int max = GetMax(type);
        int newValue = Mathf.Min(current + totalValue, max);
        currentResources[type] = newValue;
        
        if (flatFromSkill > 0 || percentFromSkill > 0)
        {
            Debug.Log($"[PlayerResourceManager] {type} 획득: 기본 {value} + 고정(+{flatFromSkill}) = {basePlusFlat}, 퍼센트(+{percentFromSkill}%) → 최종 {totalValue}");
        }
        
        UpdateUI(type);
    }

    public int GetAmount(ResourceType type)
    {
        return currentResources.ContainsKey(type) ? currentResources[type] : 0;
    }

    public int GetMax(ResourceType type)
    {
        int baseValue = GlobalGameConfig.BaseResourceCap.TryGetValue(type, out var cap) ? cap : 999999;
        int itemBonus = itemBonusResourceCap.ContainsKey(type) ? itemBonusResourceCap[type] : 0;
        
        int flatFromSkill = 0;
        int percentFromSkill = 0;
        if (PlayerSkillManager.Instance != null)
        {
            flatFromSkill = PlayerSkillManager.Instance.GetEffectFlat(SkillData.SkillEffect.ResourceCap, type.ToString());
            percentFromSkill = PlayerSkillManager.Instance.GetEffectPercent(SkillData.SkillEffect.ResourceCap, type.ToString());
        }
        
        // 고정 보너스를 먼저 더하고, 퍼센트는 base에만 적용 (아이템 보너스에는 미적용)
        int basePlusFlat = baseValue + flatFromSkill;
        float multiplier = 1f + (percentFromSkill / 100f);
        int maxWithPercent = Mathf.FloorToInt(basePlusFlat * multiplier);
        int finalMax = maxWithPercent + itemBonus;
        
        if (flatFromSkill != 0 || percentFromSkill != 0)
        {
            Debug.Log($"[GetMax] {type} 최대치 계산: base {baseValue} + flat {flatFromSkill} = {basePlusFlat}, percent +{percentFromSkill}% → {maxWithPercent}, itemBonus +{itemBonus} → 최종 {finalMax}");
        }
        
        return finalMax;
    }

    public void UpdateUI(ResourceType type)
    {
        int current = GetAmount(type);
        int max = GetMax(type);
        resourceUIManager?.UpdateUI(type, current, max);
    }

    private void UpdateAllResourceUI()
    {
        foreach (ResourceType type in Enum.GetValues(typeof(ResourceType)))
        {
            if (type != ResourceType.None && type != ResourceType.Exp)
            {
                UpdateUI(type);
            }
        }
    }

    private void InitializeResources()
    {
        foreach (ResourceType type in Enum.GetValues(typeof(ResourceType)))
        {
            if (type == ResourceType.None || type == ResourceType.Exp) continue;

            if (!currentResources.ContainsKey(type))
                currentResources[type] = 0;
        }  
    }

    public void ApplyOfflineEnergyRecovery(float offlineSeconds)
    {
        int current = GetAmount(ResourceType.Energy);
        int max = GetMax(ResourceType.Energy);

        if (current >= max) return;

        float totalTime = recoveryTimer + offlineSeconds;
        int recoverable = Mathf.FloorToInt(totalTime / energyRecoveryInterval);
        int actualRecover = Mathf.Min(recoverable, max - current);
        if (actualRecover > 0)
        {
            Add(ResourceType.Energy, actualRecover);
        }

        recoveryTimer = totalTime % energyRecoveryInterval;
    }

    private void RecalculateMaxCaps()
    {
        itemBonusResourceCap.Clear();

        foreach (var item in maxCapItems)
        {
            if (item == null || !item.ProvidesMaxCapBonus()) continue;

            ResourceType type = item.Data.maxCapResource;
            int bonus = item.Data.maxCapValue;

            if (bonus <= 0) continue;

            if (!itemBonusResourceCap.ContainsKey(type))
                itemBonusResourceCap[type] = 0;

            itemBonusResourceCap[type] += bonus;
        }
        UpdateAllResourceUI();
    }

    public void RegisterMaxCapItem(MergeItem item)
    {
        if (item != null)
        {
            if (!maxCapItems.Contains(item))
            {
                maxCapItems.Add(item);
                RecalculateMaxCaps();
            }
        }
        else
        {
            Debug.LogWarning($"[maxCapItem] 조건 불충족: {item?.key}, ProvidesMaxCapBonus: {item?.ProvidesMaxCapBonus()}");
        }
    }

    public void UnregisterMaxCapItem(MergeItem item)
    {
        if (item == null) return;

        if (maxCapItems.Contains(item))
        {
            maxCapItems.Remove(item);
            RecalculateMaxCaps();
        }
    }

    // 저장
    public void SaveTo(PlayerSaveData save)
    {
        save.recoveryTimerSeconds = recoveryTimer;
        save.resourceAmounts.Clear();

        foreach (var pair in currentResources)
        {
            save.resourceAmounts.Add(new ResourceEntry { type = pair.Key.ToString(), amount = pair.Value });
        }
    }

    public void LoadFrom(PlayerSaveData save)
    {
        recoveryTimer = save.recoveryTimerSeconds;
        currentResources.Clear();

        foreach (var entry in save.resourceAmounts)
        {
            if (Enum.TryParse(entry.type, out ResourceType type))
                currentResources[type] = entry.amount;
        }

        // 세이브에 에너지가 없으면 기본치로 설정
        if (!currentResources.ContainsKey(ResourceType.Energy))
        {
            currentResources[ResourceType.Energy] = GetMax(ResourceType.Energy);
        }

        InitializeResources();

        foreach (ResourceType type in Enum.GetValues(typeof(ResourceType)))
        {
            if (type != ResourceType.None && type != ResourceType.Exp)
                UpdateUI(type);
        }
    }

}

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

    // 외부에서 남은 시간 조회용 프로퍼티
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
            

        int current = currentResources[type];
        int max = GetMax(type);
        int newValue = Mathf.Min(current + value, max);
        currentResources[type] = newValue;
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
        return baseValue + itemBonus;
    }

    private void UpdateUI(ResourceType type)
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
        Debug.Log($"[ApplyOfflineEnergyRecovery] recoveryTimer : {recoveryTimer} totalTime : {totalTime}");
        int recoverable = Mathf.FloorToInt(totalTime / energyRecoveryInterval);
        int actualRecover = Mathf.Min(recoverable, max - current);
        Debug.Log($"[ApplyOfflineEnergyRecovery] actualRecover : {actualRecover}");
        if (actualRecover > 0)
        {
            Add(ResourceType.Energy, actualRecover);
        }

        recoveryTimer = totalTime % energyRecoveryInterval;
    }

    private void RecalculateMaxCaps()
    {
        Debug.Log("[RecaculateMaxCaps] 호출");
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
        Debug.Log($"wood cap : {itemBonusResourceCap[ResourceType.Wood]}");
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

    //저장
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

        InitializeResources();

        foreach (ResourceType type in Enum.GetValues(typeof(ResourceType)))
        {
            if (type != ResourceType.None && type != ResourceType.Exp)
                UpdateUI(type);
        }
    }

}

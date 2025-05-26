// PlayerResourceManager.cs
using UnityEngine;
using System.Collections.Generic;
using System;
using System.Collections;

public class PlayerResourceManager : MonoBehaviour
{
    public static PlayerResourceManager Instance { get; private set; }

    private Dictionary<ResourceType, int> resourceTable = new Dictionary<ResourceType, int>();
    private Dictionary<ResourceType, int> maxResourceTable = new Dictionary<ResourceType, int>();
    [SerializeField] private ResourceUIManager resourceUIManager;

    [Header("Energy Settings")]
    public int MaxEnergy = 100;
    public float energyRecoveryInterval = 120f; // 120초
    public int energyRecoveryAmount = 1;

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
        if (current >= MaxEnergy)
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
        return resourceTable.ContainsKey(type) && resourceTable[type] >= value;
    }

    public bool TrySpend(ResourceType type, int value)
    {
        if (!HasEnough(type, value)) return false;

        resourceTable[type] -= value;

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

        if (!resourceTable.ContainsKey(type))
        {
            Debug.Log($"[PlayerResourceManager.Add] Add 호출됐지만 존재하지 않아서 초기화 type : {type} value : {value}");
            resourceTable[type] = 0;
        }
            

        int current = resourceTable[type];
        int max = GetMax(type);
        int newValue = Mathf.Min(current + value, max);
        resourceTable[type] = newValue;
        UpdateUI(type);
    }

    public int GetAmount(ResourceType type)
    {
        return resourceTable.ContainsKey(type) ? resourceTable[type] : 0;
    }

    public int GetMax(ResourceType type)
    {
        return maxResourceTable.ContainsKey(type) ? maxResourceTable[type] : 0;
    }

    public void SetMax(ResourceType type, int value)
    {
        maxResourceTable[type] = value;
        UpdateUI(type);
    }

    public void AddMax(ResourceType type, int value)
    {
        if (!maxResourceTable.ContainsKey(type))
            maxResourceTable[type] = 0;

        maxResourceTable[type] += value;
        UpdateUI(type);
    }

    private void UpdateUI(ResourceType type)
    {
        int current = GetAmount(type);
        int max = GetMax(type);
        resourceUIManager?.UpdateUI(type, current, max);
    }

    private void InitializeResources()
    {
        foreach (ResourceType type in System.Enum.GetValues(typeof(ResourceType)))
        {
            if (type == ResourceType.None || type == ResourceType.Exp) continue;

            if (!resourceTable.ContainsKey(type))
                resourceTable[type] = 0;

            if (!maxResourceTable.ContainsKey(type))
            {
                // 기본 최대치 설정
                switch (type)
                {
                    case ResourceType.Energy:
                        maxResourceTable[type] = MaxEnergy;
                        break;
                    case ResourceType.Wood:
                    case ResourceType.Stone:
                    case ResourceType.Iron:
                        maxResourceTable[type] = 100;
                        break;
                    default:
                        maxResourceTable[type] = 999999; // 사실상 무제한
                        break;
                }
            }
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

    public void SaveTo(PlayerSaveData save)
    {
        save.recoveryTimerSeconds = recoveryTimer;
        save.resourceAmounts.Clear();

        foreach (var pair in resourceTable)
        {
            save.resourceAmounts.Add(new ResourceEntry { type = pair.Key.ToString(), amount = pair.Value });
        }

        save.resourceMaxValues.Clear();
        foreach (var pair in maxResourceTable)
        {
            save.resourceMaxValues.Add(new ResourceEntry { type = pair.Key.ToString(), amount = pair.Value });
        }
    }

    public void LoadFrom(PlayerSaveData save)
    {
        recoveryTimer = save.recoveryTimerSeconds;
        resourceTable.Clear();
        maxResourceTable.Clear();

        foreach (var entry in save.resourceAmounts)
        {
            if (Enum.TryParse(entry.type, out ResourceType type))
                resourceTable[type] = entry.amount;
        }

        foreach (var entry in save.resourceMaxValues)
        {
            if (Enum.TryParse(entry.type, out ResourceType type))
                maxResourceTable[type] = entry.amount;
        }

        InitializeResources();

        foreach (ResourceType type in Enum.GetValues(typeof(ResourceType)))
        {
            if (type != ResourceType.None && type != ResourceType.Exp)
                UpdateUI(type);
        }
    }

}

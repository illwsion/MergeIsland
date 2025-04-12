// PlayerResourceManager.cs
using UnityEngine;
using System.Collections.Generic;

public class PlayerResourceManager : MonoBehaviour
{
    public static PlayerResourceManager Instance { get; private set; }

    private Dictionary<ResourceType, int> resourceTable = new Dictionary<ResourceType, int>();
    [SerializeField] private EnergyUIManager uiManager;

    [Header("Energy Settings")]
    public int MaxEnergy = 100;
    public float energyRecoveryInterval = 120f; // 120초
    public int energyRecoveryAmount = 1;

    private float recoveryTimer = 0f;

    // 외부에서 남은 시간 조회용 프로퍼티
    public float RecoveryRemainingTime => energyRecoveryInterval - recoveryTimer;

    private void Start()
    {
        if (uiManager == null)
            Debug.LogWarning("[PlayerResourceManager] EnergyUIManager가 연결되지 않았습니다.");

        UpdateUI();
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;

            // 초기 자원 세팅 (예시)
            resourceTable[ResourceType.Energy] = 100;
            resourceTable[ResourceType.Gold] = 100;
            resourceTable[ResourceType.Wood] = 50;
        }
        else
        {
            Destroy(gameObject);
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
        //Debug.Log($"[PlayerResource] {type} -{value} → {resourceTable[type]} 남음");

        UpdateUI();
        return true;
    }

    public void Add(ResourceType type, int value)
    {
        if (!resourceTable.ContainsKey(type))
            resourceTable[type] = 0;

        resourceTable[type] += value;
        //Debug.Log($"[PlayerResource] {type} +{value} → {resourceTable[type]}");
        UpdateUI();
    }

    public int GetAmount(ResourceType type)
    {
        return resourceTable.ContainsKey(type) ? resourceTable[type] : 0;
    }

    private void UpdateUI()
    {
        uiManager?.UpdateUI(GetAmount(ResourceType.Energy));
    }
}

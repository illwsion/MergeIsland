// PlayerResourceManager.cs
using UnityEngine;
using System.Collections.Generic;

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

    private void Start()
    {
        resourceUIManager.Init();
        InitializeResources(); // 자원, 최대치 설정

        // UI 초기화는 여기서! (slotTable이 null일 수 없게 됨)
        foreach (ResourceType type in System.Enum.GetValues(typeof(ResourceType)))
        {
            if (type == ResourceType.None) continue;
            UpdateUI(type);
        }
        Add(ResourceType.Wood, 10);
        Add(ResourceType.Energy, 100);
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
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

        UpdateUI(type);
        return true;
    }

    public void Add(ResourceType type, int value)
    {
        if (!resourceTable.ContainsKey(type))
            resourceTable[type] = 0;

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
            if (type == ResourceType.None) continue;

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

            UpdateUI(type);
        }

        
    }
}

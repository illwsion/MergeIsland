// MergeItem.cs
using static ItemData;
using UnityEngine;

public class MergeItem
{
    public int id;
    public ItemData Data => ItemDataManager.Instance.GetItemData(id);

    // 런타임 저장량 관련 상태
    public int maxStorage = 0;
    public int currentStorage = 0;
    public float recoveryTimer = 0f;

    public string name => Data?.name;
    public int level => Data?.level ?? -1;
    public ItemData.Category Category => Data?.category ?? ItemData.Category.Production;
    public ItemData.ProduceType ProduceType => Data?.produceType ?? ItemData.ProduceType.None;
    public int? hp { get; private set; }
    public int? attackPower => Data.attackPower;

    public MergeItem(int id)
    {
        this.id = id;
        hp = Data.hp;  // 여기서 hp 초기화

        if (IsTimeDrivenProducer())
        {
            maxStorage = Data.maxProductionAmount;
            currentStorage = maxStorage; // 초기에는 꽉 찬 상태로 시작
        }
    }

    public void TakeDamage(int damage)
    {
        hp -= damage;
    }

    public bool CanMergeWith(MergeItem other)
    {
        if (other == null || this.Data == null || other.Data == null)
            return false;

        return MergeRuleManager.Instance.GetMergeResult(this.id, other.id).HasValue;
    }

    public bool IsTimeDrivenProducer()
    {
        bool result = Data != null &&
                      Category == ItemData.Category.Production &&
                      (ProduceType == ItemData.ProduceType.Manual || ProduceType == ProduceType.Auto);

        return result;
    }

    public void UpdateProductionStorage(float deltaTime)
    {
        if (!IsTimeDrivenProducer()) return;

        float interval = Data.productionInterval;

        if (currentStorage >= maxStorage) // 최대치 도달 시 회복 중단
        {
            recoveryTimer = 0f;
            return;
        }

        recoveryTimer += deltaTime;
        if (recoveryTimer >= interval)
        {
            int recovered = Mathf.FloorToInt(recoveryTimer / interval);
            currentStorage = Mathf.Min(currentStorage + recovered, maxStorage);
            recoveryTimer = 0f;
        }
    }

    public bool CanProduce()
    {
        return currentStorage > 0;
    }

    public void ConsumeStorage()
    {
        if (currentStorage > 0)
            currentStorage--;
    }

    public float GetRecoveryRemainingTime()
    {
        return Data.productionInterval - recoveryTimer;
    }
}
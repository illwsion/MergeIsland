// MergeItem.cs
using static ItemData;
using UnityEngine;
using System.Collections.Generic;

public class MergeItem
{
    public int id;
    public MergeBoard board;
    public Vector2Int coord;

    public ItemData Data => ItemDataManager.Instance.GetItemData(id);
    public string name => Data?.name;
    public int level => Data?.level ?? -1;
    public ItemData.Category Category => Data?.category ?? ItemData.Category.Production;
    public ItemData.ProduceType ProduceType => Data?.produceType ?? ItemData.ProduceType.None;
    public int? maxHP { get; private set; }
    public int currentHP;
    public int? attackPower => Data.attackPower;

    // ��Ÿ�� ���差 ���� ����
    public int maxStorage = 0;
    public int currentStorage = 0;
    public float recoveryTimer = 0f;
    private float ProductionInterval => Data.productionInterval;

    public MergeItem(int id)
    {
        this.id = id;
        maxHP = Data.hp;  // ���⼭ hp �ʱ�ȭ
        currentHP = Data.hp;

        if (IsTimeDrivenProducer())
        {
            maxStorage = Data.maxProductionAmount;
            currentStorage = maxStorage; // �ʱ⿡�� �� �� ���·� ����
        }
    }

    public void TakeDamage(int damage)
    {
        currentHP -= damage;
    }

    public bool CanMergeWith(MergeItem other)
    {
        if (other == null || this.Data == null || other.Data == null)
            return false;

        return MergeRuleManager.Instance.GetMergeResult(this.id, other.id).HasValue;
    }

    public bool IsTimeDrivenProducer()
    {
        return Data != null &&
               Category == ItemData.Category.Production &&
               (ProduceType == ItemData.ProduceType.Manual || ProduceType == ProduceType.Auto);
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
        return ProductionInterval - recoveryTimer;
    }

    public void UpdateProductionStorage(float deltaTime)
    {
        if (!IsTimeDrivenProducer()) return;

        recoveryTimer += deltaTime;

        switch (ProduceType)
        {
            case ItemData.ProduceType.Manual:
                if (currentStorage >= maxStorage) // �ִ�ġ ���� �� ȸ�� �ߴ�
                {
                    recoveryTimer = 0f;
                    return;
                }

                if (recoveryTimer >= ProductionInterval)
                {
                    int recovered = Mathf.FloorToInt(recoveryTimer / ProductionInterval);
                    currentStorage = Mathf.Min(currentStorage + recovered, maxStorage);
                    recoveryTimer = 0f;
                }
                break;

            case ItemData.ProduceType.Auto:
                if (recoveryTimer >= ProductionInterval)
                {
                    ProduceAuto();
                    // recoveryTimer�� ������ ���� ���� (��ĭ ������ ���)
                }

                break;
            default:
                // ProduceType.Gather, Dialogue ���� ���� ��� �ƴ�
                break;
        }
    }
    // �÷��̾� ��ġ�� ����
    public void ProduceManual()
    {
        if (!CanProduce())
        {
            UIToast.Show("���差�� �����մϴ�!");
            return;
        }

        if (!TryPrepareProduction(out int resultItemID, out Vector2Int spawnPos))
        {
            // ���ο��� ��ĭ ����/��� ���� �α� ���
            return;
        }

        ResourceType costType = Data.costResource.ToResourceType();
        int costValue = Data.costValue;

        if (costType != ResourceType.None)
        {
            if (!PlayerResourceManager.Instance.TrySpend(costType, costValue))
            {
                UIToast.Show("�ڿ��� �����մϴ�!");
                Debug.LogWarning($"[ProduceManual] �ڿ� ����: {costType} {costValue}");
                return;
            }
        }

        ConsumeStorage();
        BoardManager.Instance.SpawnItem(board, resultItemID, spawnPos);
        // ���� �� ���� ���� ����� ������ UI ����
        if (board == BoardManager.Instance.GetCurrentBoard())
        {
            BoardManager.Instance.RefreshBoard();
        }

        Debug.Log($"[ProduceManual] {name} �� {resultItemID} ���� �Ϸ�");
    }

    // �ڵ� ����
    private void ProduceAuto()
    {
        if (!TryPrepareProduction(out int resultItemID, out Vector2Int spawnPos))
        {
            // ���ο��� ��ĭ ����/��� ���� �α� ���
            return;
        }
        BoardManager.Instance.SpawnItem(board, resultItemID, spawnPos);
        Debug.Log($"[ProduceManual] {name} �� {resultItemID} ���� �Ϸ�");
        // ���� �� ���� ���� ����� ������ UI ����
        if (board == BoardManager.Instance.GetCurrentBoard())
        {
            BoardManager.Instance.RefreshBoard();
        }
        recoveryTimer = 0f; // ���� ���� �� ����
    }

    private bool TryPrepareProduction(out int resultItemID, out Vector2Int spawnPos)
    {
        resultItemID = -1;
        spawnPos = Vector2Int.zero;

        if (board == null)
        {
            Debug.LogError($"[TryPrepareProduction] board�� �������: {name}");
            return false;
        }

        Vector2Int? pos = board.FindNearestEmptyCell(coord);
        if (pos == null)
        {
            if (ProduceType == ItemData.ProduceType.Manual)
                UIToast.Show("���忡 �� ĭ�� �����ϴ�!");
            Debug.LogWarning($"[TryPrepareProduction] {name}: �� ĭ ����");
            return false;
        }

        var table = ProduceTableManager.Instance.GetTable(Data.produceTableID);
        if (table == null || table.results.Count == 0)
        {
            Debug.LogWarning("[TryPrepareProduction] ���� ���̺��� ����ֽ��ϴ�.");
            return false;
        }

        int result = GetRandomItemID(table.results);
        if (result == -1)
        {
            Debug.LogError("[TryPrepareProduction] ������ ���� ����");
            return false;
        }

        resultItemID = result;
        spawnPos = pos.Value;
        return true;
    }

    private int GetRandomItemID(List<ProduceResult> results)
    {
        int total = 0;
        foreach (var result in results)
            total += result.probability;

        if (total <= 0)
        {
            Debug.LogError("[GetRandomItemID] Ȯ�� ������ 0 �����Դϴ�.");
            return -1;
        }

        int roll = UnityEngine.Random.Range(0, total);
        int accum = 0;

        foreach (var result in results)
        {
            accum += result.probability;
            if (roll < accum)
                return result.itemID;
        }

        return -1;
    }
}
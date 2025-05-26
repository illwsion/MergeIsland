// MergeItem.cs
using static ItemData;
using UnityEngine;
using System.Collections.Generic;
using System.Resources;
using static UnityEngine.RuleTile.TilingRuleOutput;
using System.Threading;

public class MergeItem
{
    public string key;
    public MergeBoard board;
    public Vector2Int coord;

    public ItemData Data => ItemDataManager.Instance.GetItemData(key);

    //Basic Info
    public string name => Data?.name;
    public string type => Data?.type;
    public int level => Data?.level ?? -1;
    public int maxLevel => Data?.maxLevel ?? -1;
    public string imageName => Data?.imageName;

    //Categories
    public ItemData.Category Category => Data?.category ?? ItemData.Category.Production;

    //Produce
    public ItemData.ProduceType ProduceType => Data?.produceType ?? ItemData.ProduceType.None;
    public bool isProductionLimited => Data?.isProductionLimited ?? false;
    private float productionInterval => Data.productionInterval;
    public int maxProductionAmount => Data?.maxProductionAmount ?? 0;
    public string produceTableKey => Data?.produceTableKey;
    public string dropTableKey => Data?.dropTableKey;

    //Resource
    public ResourceType costResource => Data?.costResource ?? ResourceType.None;
    public int costValue => Data?.costValue ?? 0;
    public ResourceType gatherResource => Data?.gatherResource ?? ResourceType.None;
    public int gatherValue => Data?.gatherValue ?? 0;
    public ResourceType maxCapResource => Data?.maxCapResource ?? ResourceType.None;
    public int maxCapValue => Data?.maxCapValue ?? 0;

    //Selling
    public bool IsSellable => Data?.isSellable ?? false;
    public int sellValue => Data?.sellValue ?? 0;

    //Text
    public string itemNameKey => Data?.itemNameKey;
    public string itemDescriptionKey => Data?.itemDescriptionKey;

    // State Flags
    public bool CanMove => Data?.canMove ?? false;
    public bool CanInventoryStore => Data?.canInventoryStore ?? false;

    // Combat
    public int? attackPower => Data.attackPower;
    public int? maxHP { get; private set; }
    public int currentHP;

    // Runtime Production State
    public int maxStorage = 0;
    public int currentStorage = 0;
    public float recoveryTimer = 0f;
    public bool isProductionBlocked;
    

    public MergeItem(string key)
    {
        this.key = key;

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

        return MergeRuleManager.Instance.GetMergeResult(this.key, other.key) != null;
    }

    public bool IsTimeDrivenProducer()
    {
        return Data != null &&
               Category == ItemData.Category.Production &&
               (ProduceType == ItemData.ProduceType.Manual || ProduceType == ProduceType.Auto);
    }

    public bool ProvidesMaxCapBonus()
    {
        return maxCapResource != ResourceType.None;
    }
    
    public bool CanProduce()
    {
        return currentStorage > 0;
    }

    public void ConsumeStorage()
    {
        currentStorage = Mathf.Max(0, currentStorage - 1);

        if (Data.isProductionLimited && currentStorage <= 0)
        {
            HandleLimitedProductionDepletion();
        }
    }

    private void HandleLimitedProductionDepletion()
    {
        Debug.Log($"[LimitedProduction] {name}�� ���귮 ���� �� ���� ó�� ����");

        BoardManager.Instance.RemoveItem(this);
        if (!string.IsNullOrEmpty(Data.dropTableKey))
        {
            string dropItemKey = DropTableManager.Instance.GetRandomDropItem(this);
            if (!string.IsNullOrEmpty(dropItemKey))
            {
                BoardManager.Instance.SpawnItem(board, dropItemKey, this.coord);
            }
            else
            {
                Debug.Log("������̺��� �������");
            }

        }
        ItemSelectorManager.Instance.ClearSelection();
    }

    public float GetRecoveryRemainingTime()
    {
        return productionInterval - recoveryTimer;
    }

    public void UpdateProductionStorage(float deltaTime)
    {
        if (!IsTimeDrivenProducer()) return;

        recoveryTimer += deltaTime;

        switch (ProduceType)
        {
            case ItemData.ProduceType.Manual:
                if (Data.isProductionLimited)
                {
                    return;
                }

                if (currentStorage >= maxStorage)
                {
                    recoveryTimer = 0f;
                    return;
                }

                if (recoveryTimer >= productionInterval)
                {
                    int recovered = Mathf.FloorToInt(recoveryTimer / productionInterval);
                    currentStorage = Mathf.Min(currentStorage + recovered, maxStorage);
                    recoveryTimer = 0f;
                }
                break;

            case ItemData.ProduceType.Auto:
                while (recoveryTimer >= productionInterval)
                {
                    if (!TryPrepareProduction(out string resultItemKey, out Vector2Int spawnPos))
                    {
                        isProductionBlocked = true;
                        break; // ���� ���� ��ݺ� �ߴ�
                    }

                    isProductionBlocked = false;
                    BoardManager.Instance.SpawnItem(board, resultItemKey, spawnPos);

                    if (Data.isProductionLimited)
                    {
                        ConsumeStorage();
                        if (currentStorage <= 0)
                        {
                            break; // ������ �� �� ���� �Ұ�
                        }
                    }

                    recoveryTimer -= productionInterval; // �ݺ� �����ϵ��� �ð� ����
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

        if (!TryPrepareProduction(out string resultItemKey, out Vector2Int spawnPos))
        {
            // ���ο��� ��ĭ ����/��� ���� �α� ���
            return;
        }

        ResourceType costType = Data.costResource;
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
        BoardManager.Instance.SpawnItem(board, resultItemKey, spawnPos);
        // ���� �� ���� ���� ����� ������ UI ����
        if (board == BoardManager.Instance.GetCurrentBoard())
        {
            BoardManager.Instance.RefreshBoard();
        }

        Debug.Log($"[ProduceManual] {name} �� {resultItemKey} ���� �Ϸ�");
    }

    // �ڵ� ����
    private void ProduceAuto()
    {
        if (!TryPrepareProduction(out string resultItemKey, out Vector2Int spawnPos))
        {
            // ���ο��� ��ĭ ����/��� ���� �α� ���
            isProductionBlocked = true;
            return;
        }
        //���� ���� -> ���� ���� ����
        isProductionBlocked = false;

        BoardManager.Instance.SpawnItem(board, resultItemKey, spawnPos);
        Debug.Log($"[ProduceAuto] {name} �� {resultItemKey} ���� �Ϸ�");

        if (Data.isProductionLimited)
        {
            ConsumeStorage();
        }

        // ���� �� ���� ���� ����� ������ UI ����
        if (board == BoardManager.Instance.GetCurrentBoard())
        {
            BoardManager.Instance.RefreshBoard();
        }

        recoveryTimer = 0f; // ���� ���� �� ����
    }

    public void ProduceGather()
    {
        var data = this.Data;

        // 1. � �ڿ��� �����ϴ���
        var type = data.gatherResource;
        var amount = data.gatherValue;

        if (type == ResourceType.None || amount <= 0)
        {
            Debug.LogWarning("[MergeItem] Gather ����: �߸��� �ڿ� ����");
            return;
        }

        // 2. â�� �� á���� Ȯ��
        int current = PlayerResourceManager.Instance.GetAmount(type);
        int max = PlayerResourceManager.Instance.GetMax(type);

        if (current >= max)
        {
            UIToast.Show("â�� �� á���ϴ�!");
            Debug.LogWarning($"[MergeItem] Gather ����: {type} â�� ���� á���ϴ�.");
            return;
        }

        // 2. �ڿ� �߰�
        PlayerResourceManager.Instance.Add(type, amount);
        Debug.Log($"[MergeItem] {type} +{amount} ȹ��!");

        // 3. ������ ��Ͽ��� ����
        BoardManager.Instance.UnregisterItem(this);

        // 4. ���忡�� ����
        if (board != null)
        {
            Vector2Int pos = coord;
            board.grid[pos.x, pos.y] = null;
        }
        else
        {
            Debug.LogWarning("[MergeItem] ���� ������ ���� ���� ����");
        }
        // ���� �� ���� ���� ����� ������ UI ����
        if (board == BoardManager.Instance.GetCurrentBoard())
        {
            BoardManager.Instance.RefreshBoard();
        }
        ItemSelectorManager.Instance.ClearSelection();
    }

    private bool TryPrepareProduction(out string resultItemKey, out Vector2Int spawnPos)
    {
        resultItemKey = "null";
        spawnPos = Vector2Int.zero;

        if (board == null)
        {
            Debug.LogError($"[TryPrepareProduction] board�� �������: {name}");
            return false;
        }

        // 1. produceTableKey ��ȿ�� �˻�
        if (string.IsNullOrEmpty(Data.produceTableKey) || Data.produceTableKey == "null")
        {
            Debug.LogWarning($"[TryPrepareProduction] {name}�� produceTableKey�� ��� ����");
            return false;
        }

        // 2. �� ĭ ã��
        Vector2Int? pos = board.FindNearestEmptyCell(coord);
        if (pos == null)
        {
            if (ProduceType == ItemData.ProduceType.Manual)
            {
                UIToast.Show("���忡 �� ĭ�� �����ϴ�!");
            }

            Debug.LogWarning($"[TryPrepareProduction] {name}: �� ĭ ����");
            return false;
        }

        // 3. ���̺� ��ȸ
        var table = ProduceTableManager.Instance.GetTable(Data.produceTableKey);
        if (table == null || table.results == null || table.results.Count == 0)
        {
            Debug.LogWarning($"[TryPrepareProduction] {name}: ���� ���̺� '{Data.produceTableKey}'�� ��� ����");
            return false;
        }

        // 4. ��� ������ ����
        string result = GetRandomItemKey(table.results);
        if (string.IsNullOrEmpty(result) || result == "null")
        {
            Debug.LogError($"[TryPrepareProduction] {name}: ������ ���� ���� (���̺� {Data.produceTableKey})");
            return false;
        }

        resultItemKey = result;
        spawnPos = pos.Value;
        return true;
    }

    private string GetRandomItemKey(List<ProduceResult> results)
    {
        int total = 0;
        foreach (var result in results)
            total += result.probability;

        if (total <= 0)
        {
            Debug.LogError("[GetRandomItemKey] Ȯ�� ������ 0 �����Դϴ�.");
            return "null";
        }

        int roll = UnityEngine.Random.Range(0, total);
        int accum = 0;

        foreach (var result in results)
        {
            accum += result.probability;
            if (roll < accum)
                return result.itemKey;
        }

        return "null";
    }

}
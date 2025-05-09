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

    // 런타임 저장량 관련 상태
    public int maxStorage = 0;
    public int currentStorage = 0;
    public float recoveryTimer = 0f;
    private float ProductionInterval => Data.productionInterval;

    public MergeItem(int id)
    {
        this.id = id;
        maxHP = Data.hp;  // 여기서 hp 초기화
        currentHP = Data.hp;

        if (IsTimeDrivenProducer())
        {
            maxStorage = Data.maxProductionAmount;
            currentStorage = maxStorage; // 초기에는 꽉 찬 상태로 시작
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
                if (currentStorage >= maxStorage) // 최대치 도달 시 회복 중단
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
                    // recoveryTimer는 성공할 때만 리셋 (빈칸 없으면 대기)
                }

                break;
            default:
                // ProduceType.Gather, Dialogue 등은 충전 대상 아님
                break;
        }
    }
    // 플레이어 터치로 생산
    public void ProduceManual()
    {
        if (!CanProduce())
        {
            UIToast.Show("저장량이 부족합니다!");
            return;
        }

        if (!TryPrepareProduction(out int resultItemID, out Vector2Int spawnPos))
        {
            // 내부에서 빈칸 없음/결과 없음 로그 출력
            return;
        }

        ResourceType costType = Data.costResource.ToResourceType();
        int costValue = Data.costValue;

        if (costType != ResourceType.None)
        {
            if (!PlayerResourceManager.Instance.TrySpend(costType, costValue))
            {
                UIToast.Show("자원이 부족합니다!");
                Debug.LogWarning($"[ProduceManual] 자원 부족: {costType} {costValue}");
                return;
            }
        }

        ConsumeStorage();
        BoardManager.Instance.SpawnItem(board, resultItemID, spawnPos);
        // 생산 시 현재 보는 보드와 같으면 UI 갱신
        if (board == BoardManager.Instance.GetCurrentBoard())
        {
            BoardManager.Instance.RefreshBoard();
        }

        Debug.Log($"[ProduceManual] {name} → {resultItemID} 생산 완료");
    }

    // 자동 생산
    private void ProduceAuto()
    {
        if (!TryPrepareProduction(out int resultItemID, out Vector2Int spawnPos))
        {
            // 내부에서 빈칸 없음/결과 없음 로그 출력
            return;
        }
        BoardManager.Instance.SpawnItem(board, resultItemID, spawnPos);
        Debug.Log($"[ProduceManual] {name} → {resultItemID} 생산 완료");
        // 생산 시 현재 보는 보드와 같으면 UI 갱신
        if (board == BoardManager.Instance.GetCurrentBoard())
        {
            BoardManager.Instance.RefreshBoard();
        }
        recoveryTimer = 0f; // 생산 성공 시 리셋
    }

    private bool TryPrepareProduction(out int resultItemID, out Vector2Int spawnPos)
    {
        resultItemID = -1;
        spawnPos = Vector2Int.zero;

        if (board == null)
        {
            Debug.LogError($"[TryPrepareProduction] board가 비어있음: {name}");
            return false;
        }

        Vector2Int? pos = board.FindNearestEmptyCell(coord);
        if (pos == null)
        {
            if (ProduceType == ItemData.ProduceType.Manual)
                UIToast.Show("보드에 빈 칸이 없습니다!");
            Debug.LogWarning($"[TryPrepareProduction] {name}: 빈 칸 없음");
            return false;
        }

        var table = ProduceTableManager.Instance.GetTable(Data.produceTableID);
        if (table == null || table.results.Count == 0)
        {
            Debug.LogWarning("[TryPrepareProduction] 생산 테이블이 비어있습니다.");
            return false;
        }

        int result = GetRandomItemID(table.results);
        if (result == -1)
        {
            Debug.LogError("[TryPrepareProduction] 아이템 선택 실패");
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
            Debug.LogError("[GetRandomItemID] 확률 총합이 0 이하입니다.");
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
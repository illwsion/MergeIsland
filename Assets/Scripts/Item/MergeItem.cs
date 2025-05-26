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
        Debug.Log($"[LimitedProduction] {name}의 생산량 소진 → 제거 처리 시작");

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
                Debug.Log("드랍테이블이 비어있음");
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
                        break; // 공간 부족 →반복 중단
                    }

                    isProductionBlocked = false;
                    BoardManager.Instance.SpawnItem(board, resultItemKey, spawnPos);

                    if (Data.isProductionLimited)
                    {
                        ConsumeStorage();
                        if (currentStorage <= 0)
                        {
                            break; // 소진됨 → 더 생산 불가
                        }
                    }

                    recoveryTimer -= productionInterval; // 반복 가능하도록 시간 차감
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

        if (!TryPrepareProduction(out string resultItemKey, out Vector2Int spawnPos))
        {
            // 내부에서 빈칸 없음/결과 없음 로그 출력
            return;
        }

        ResourceType costType = Data.costResource;
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
        BoardManager.Instance.SpawnItem(board, resultItemKey, spawnPos);
        // 생산 시 현재 보는 보드와 같으면 UI 갱신
        if (board == BoardManager.Instance.GetCurrentBoard())
        {
            BoardManager.Instance.RefreshBoard();
        }

        Debug.Log($"[ProduceManual] {name} → {resultItemKey} 생산 완료");
    }

    // 자동 생산
    private void ProduceAuto()
    {
        if (!TryPrepareProduction(out string resultItemKey, out Vector2Int spawnPos))
        {
            // 내부에서 빈칸 없음/결과 없음 로그 출력
            isProductionBlocked = true;
            return;
        }
        //공간 있음 -> 생산 정상 진행
        isProductionBlocked = false;

        BoardManager.Instance.SpawnItem(board, resultItemKey, spawnPos);
        Debug.Log($"[ProduceAuto] {name} → {resultItemKey} 생산 완료");

        if (Data.isProductionLimited)
        {
            ConsumeStorage();
        }

        // 생산 시 현재 보는 보드와 같으면 UI 갱신
        if (board == BoardManager.Instance.GetCurrentBoard())
        {
            BoardManager.Instance.RefreshBoard();
        }

        recoveryTimer = 0f; // 생산 성공 시 리셋
    }

    public void ProduceGather()
    {
        var data = this.Data;

        // 1. 어떤 자원을 생산하는지
        var type = data.gatherResource;
        var amount = data.gatherValue;

        if (type == ResourceType.None || amount <= 0)
        {
            Debug.LogWarning("[MergeItem] Gather 실패: 잘못된 자원 정보");
            return;
        }

        // 2. 창고가 꽉 찼는지 확인
        int current = PlayerResourceManager.Instance.GetAmount(type);
        int max = PlayerResourceManager.Instance.GetMax(type);

        if (current >= max)
        {
            UIToast.Show("창고가 꽉 찼습니다!");
            Debug.LogWarning($"[MergeItem] Gather 차단: {type} 창고가 가득 찼습니다.");
            return;
        }

        // 2. 자원 추가
        PlayerResourceManager.Instance.Add(type, amount);
        Debug.Log($"[MergeItem] {type} +{amount} 획득!");

        // 3. 생산자 목록에서 제거
        BoardManager.Instance.UnregisterItem(this);

        // 4. 보드에서 제거
        if (board != null)
        {
            Vector2Int pos = coord;
            board.grid[pos.x, pos.y] = null;
        }
        else
        {
            Debug.LogWarning("[MergeItem] 보드 정보가 없어 제거 실패");
        }
        // 생산 시 현재 보는 보드와 같으면 UI 갱신
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
            Debug.LogError($"[TryPrepareProduction] board가 비어있음: {name}");
            return false;
        }

        // 1. produceTableKey 유효성 검사
        if (string.IsNullOrEmpty(Data.produceTableKey) || Data.produceTableKey == "null")
        {
            Debug.LogWarning($"[TryPrepareProduction] {name}의 produceTableKey가 비어 있음");
            return false;
        }

        // 2. 빈 칸 찾기
        Vector2Int? pos = board.FindNearestEmptyCell(coord);
        if (pos == null)
        {
            if (ProduceType == ItemData.ProduceType.Manual)
            {
                UIToast.Show("보드에 빈 칸이 없습니다!");
            }

            Debug.LogWarning($"[TryPrepareProduction] {name}: 빈 칸 없음");
            return false;
        }

        // 3. 테이블 조회
        var table = ProduceTableManager.Instance.GetTable(Data.produceTableKey);
        if (table == null || table.results == null || table.results.Count == 0)
        {
            Debug.LogWarning($"[TryPrepareProduction] {name}: 생산 테이블 '{Data.produceTableKey}'이 비어 있음");
            return false;
        }

        // 4. 결과 아이템 선택
        string result = GetRandomItemKey(table.results);
        if (string.IsNullOrEmpty(result) || result == "null")
        {
            Debug.LogError($"[TryPrepareProduction] {name}: 아이템 선택 실패 (테이블 {Data.produceTableKey})");
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
            Debug.LogError("[GetRandomItemKey] 확률 총합이 0 이하입니다.");
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
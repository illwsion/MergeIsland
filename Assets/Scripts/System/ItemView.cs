// ItemView.cs
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.U2D;
using System.Drawing;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class ItemView : MonoBehaviour, IPointerClickHandler
{
    public Image iconImage;       // 셀에 표시할 아이템 이미지
    private int currentLevel;     // 현재 아이템 레벨 저장
    public MergeItem mergeItem;
    public Vector2Int coord;

    public void SetItem(MergeItem item)
    {
        if (item.Data == null)
        {
            Debug.LogWarning($"[ItemView] 아이템 데이터가 없습니다. id: {item.id}");
            return;
        }
        mergeItem = item;
        currentLevel = item.level;
        string spriteName = item.name;
        iconImage.sprite = AtlasManager.Instance.GetSprite(spriteName);
    }

    public void SetCoord(Vector2Int pos)
    {
        coord = pos;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (DragManager.Instance.IsDragging) return;

        var selector = ItemSelectorManager.Instance;

        // 같은 아이템이 이미 선택된 상태면 → 생산
        if (selector.GetSelectedItem() == mergeItem)
        {
            Debug.Log("[ItemView] 같은 아이템 터치 → 생산 실행");
            ProduceItem();
        }
        else
        {
            // 새로운 아이템 선택
            selector.Select(this);
        }
    }

    private void ProduceItem()
    {
        var data = mergeItem.Data;
        ResourceType costType = data.costResource.ToResourceType();
        int costValue = data.costValue;

        // 자원 체크
        if (costType != ResourceType.None)
        {
            if (!PlayerResourceManager.Instance.TrySpend(costType, costValue))
            {
                Debug.LogWarning($"[ProduceItem] 자원이 부족합니다: {costType} {costValue}");
                // TODO: UI 알림
                return;
            }
        }

        // 생산 테이블에서 결과 얻기
        var table = ProduceTableManager.Instance.GetTable(mergeItem.Data.produceTableID);
        if (table == null || table.results.Count == 0)
        {
            Debug.LogWarning("[ProduceItem] 생산 테이블이 비어있습니다.");
            return;
        }

        int resultItemID = GetRandomItemID(table.results);
        if (resultItemID == -1)
        {
            Debug.LogError("[ProduceItem] 아이템 선택 실패");
            return;
        }

        Vector2Int? spawnPos = BoardManager.Instance.FindNearestEmptyCell(coord);
        if (spawnPos == null)
        {
            Debug.Log("빈 칸이 없습니다!");
            return;
        }

        BoardManager.Instance.SpawnItem(resultItemID, spawnPos.Value);
    }

    private int GetRandomItemID(List<ProduceResult> results)
    {
        int total = 0;
        foreach (var result in results)
            total += result.probability;

        if (total <= 0)
        {
            Debug.LogError("[ProduceItem] 확률 총합이 0 이하입니다.");
            return -1;
        }

        int roll = Random.Range(0, total); // 0 ~ total-1
        int accum = 0;

        foreach (var result in results)
        {
            accum += result.probability;
            if (roll < accum)
                return result.itemID;
        }

        return -1; // 실패
    }

    public int GetLevel()
    {
        return currentLevel;
    }
}
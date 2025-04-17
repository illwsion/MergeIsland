// ItemView.cs
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.U2D;
using System.Drawing;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine.WSA;

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

        // 같은 아이템을 또 터치했을 경우
        if (selector.GetSelectedItem() == mergeItem)
        {
            switch (mergeItem.ProduceType)
            {
                case ItemData.ProduceType.Manual:
                    Debug.Log("[ItemView] Manual 아이템 → 생산 실행");
                    ProduceItem();
                    break;

                case ItemData.ProduceType.Gather:
                    Debug.Log("[ItemView] Gather 아이템 → 자원 수확 실행");
                    //CollectResource(); // 이건 이후에 추가 가능
                    break;

                case ItemData.ProduceType.Dialogue:
                    Debug.Log("[ItemView] Dialogue 아이템 → NPC 대화 실행");
                    //TriggerNPCDialogue(); // 이후 확장 시 정의
                    break;

                case ItemData.ProduceType.Auto:
                    Debug.Log("[ItemView] Auto 아이템 → 터치 시 아무 동작 없음");
                    break;

                case ItemData.ProduceType.None:
                default:
                    Debug.Log("[ItemView] 정의되지 않은 ProduceType → 무시");
                    break;
            }

            return;
        }

        // 새로운 아이템 선택
        selector.Select(this);
    }

    private void ProduceItem()
    {
        var data = mergeItem.Data;
        ResourceType costType = data.costResource.ToResourceType();
        int costValue = data.costValue;

        //빈칸 체크
        Vector2Int? spawnPos = BoardManager.Instance.FindNearestEmptyCell(coord);
        if (spawnPos == null)
        {
            UIToast.Show("보드에 빈 칸이 없습니다!");
            Debug.Log("빈 칸이 없습니다!");
            return;
        }

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
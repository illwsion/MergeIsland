// ItemView.cs
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.U2D;
using System.Drawing;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class ItemView : MonoBehaviour, IPointerClickHandler
{
    public Image iconImage;       // ���� ǥ���� ������ �̹���
    private int currentLevel;     // ���� ������ ���� ����
    public MergeItem mergeItem;
    public Vector2Int coord;

    public void SetItem(MergeItem item)
    {
        if (item.Data == null)
        {
            Debug.LogWarning($"[ItemView] ������ �����Ͱ� �����ϴ�. id: {item.id}");
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

        // ���� �������� �̹� ���õ� ���¸� �� ����
        if (selector.GetSelectedItem() == mergeItem)
        {
            Debug.Log("[ItemView] ���� ������ ��ġ �� ���� ����");
            ProduceItem();
        }
        else
        {
            // ���ο� ������ ����
            selector.Select(this);
        }
    }

    private void ProduceItem()
    {
        var data = mergeItem.Data;
        ResourceType costType = data.costResource.ToResourceType();
        int costValue = data.costValue;

        // �ڿ� üũ
        if (costType != ResourceType.None)
        {
            if (!PlayerResourceManager.Instance.TrySpend(costType, costValue))
            {
                Debug.LogWarning($"[ProduceItem] �ڿ��� �����մϴ�: {costType} {costValue}");
                // TODO: UI �˸�
                return;
            }
        }

        // ���� ���̺��� ��� ���
        var table = ProduceTableManager.Instance.GetTable(mergeItem.Data.produceTableID);
        if (table == null || table.results.Count == 0)
        {
            Debug.LogWarning("[ProduceItem] ���� ���̺��� ����ֽ��ϴ�.");
            return;
        }

        int resultItemID = GetRandomItemID(table.results);
        if (resultItemID == -1)
        {
            Debug.LogError("[ProduceItem] ������ ���� ����");
            return;
        }

        Vector2Int? spawnPos = BoardManager.Instance.FindNearestEmptyCell(coord);
        if (spawnPos == null)
        {
            Debug.Log("�� ĭ�� �����ϴ�!");
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
            Debug.LogError("[ProduceItem] Ȯ�� ������ 0 �����Դϴ�.");
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

        return -1; // ����
    }

    public int GetLevel()
    {
        return currentLevel;
    }
}
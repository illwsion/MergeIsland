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

        // ���� �������� �� ��ġ���� ���
        if (selector.GetSelectedItem() == mergeItem)
        {
            switch (mergeItem.ProduceType)
            {
                case ItemData.ProduceType.Manual:
                    Debug.Log("[ItemView] Manual ������ �� ���� ����");
                    ProduceItem();
                    break;

                case ItemData.ProduceType.Gather:
                    Debug.Log("[ItemView] Gather ������ �� �ڿ� ��Ȯ ����");
                    //CollectResource(); // �̰� ���Ŀ� �߰� ����
                    break;

                case ItemData.ProduceType.Dialogue:
                    Debug.Log("[ItemView] Dialogue ������ �� NPC ��ȭ ����");
                    //TriggerNPCDialogue(); // ���� Ȯ�� �� ����
                    break;

                case ItemData.ProduceType.Auto:
                    Debug.Log("[ItemView] Auto ������ �� ��ġ �� �ƹ� ���� ����");
                    break;

                case ItemData.ProduceType.None:
                default:
                    Debug.Log("[ItemView] ���ǵ��� ���� ProduceType �� ����");
                    break;
            }

            return;
        }

        // ���ο� ������ ����
        selector.Select(this);
    }

    private void ProduceItem()
    {
        var data = mergeItem.Data;
        ResourceType costType = data.costResource.ToResourceType();
        int costValue = data.costValue;

        //��ĭ üũ
        Vector2Int? spawnPos = BoardManager.Instance.FindNearestEmptyCell(coord);
        if (spawnPos == null)
        {
            UIToast.Show("���忡 �� ĭ�� �����ϴ�!");
            Debug.Log("�� ĭ�� �����ϴ�!");
            return;
        }

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
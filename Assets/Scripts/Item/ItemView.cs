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
    //����ð��� ���ư��� ���� ���� �ð� ������ �߰�
    public Image iconImage;       // ���� ǥ���� ������ �̹���
    private int currentLevel;     // ���� ������ ���� ����
    public MergeItem mergeItem;
    public Vector2Int coord;

    public void SetItem(MergeItem item)
    {
        if (item.Data == null)
        {
            Debug.LogWarning($"[ItemView] ������ �����Ͱ� �����ϴ�. id: {item.key}");
            return;
        }
        mergeItem = item;
        currentLevel = item.level;
        string spriteName = item.imageName;
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
                    mergeItem.ProduceManual();
                    break;

                case ItemData.ProduceType.Gather:
                    mergeItem.ProduceGather();
                    break;

                case ItemData.ProduceType.Dialogue:
                    //TriggerNPCDialogue(); // ���� Ȯ�� �� ����
                    break;

                case ItemData.ProduceType.Auto:
                    break;

                case ItemData.ProduceType.None:
                default:
                    break;
            }

            return;
        }

        // ���ο� ������ ����
        selector.Select(this);
    }

    public int GetLevel()
    {
        return currentLevel;
    }
}
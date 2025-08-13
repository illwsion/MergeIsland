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
    //생산시간이 돌아가고 있을 때는 시계 아이콘 추가
    public Image iconImage;       // 셀에 표시할 아이템 이미지
    private int currentLevel;     // 현재 아이템 레벨 저장
    public MergeItem mergeItem;
    public Vector2Int coord;

    public void SetItem(MergeItem item)
    {
        if (item.Data == null)
        {
            Debug.LogWarning($"[ItemView] 아이템 데이터가 없습니다. id: {item.key}");
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

        // 같은 아이템을 또 터치했을 경우
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
                    //TriggerNPCDialogue(); // 이후 확장 시 정의
                    break;

                case ItemData.ProduceType.Auto:
                    break;

                case ItemData.ProduceType.None:
                default:
                    break;
            }

            return;
        }

        // 새로운 아이템 선택
        selector.Select(this);
    }

    public int GetLevel()
    {
        return currentLevel;
    }
}
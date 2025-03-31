// ItemView.cs
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.U2D;
using System.Drawing;
using UnityEngine.EventSystems;

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
        Debug.Log(spriteName);
        iconImage.sprite = AtlasManager.Instance.GetSprite(spriteName);
    }
    /*
    public void SetSelected(bool selected)
    {
        if (outlineObject != null)
        {
            outlineObject.SetActive(selected);
            Debug.Log($"[ItemView] SelectionOutline set to {selected} on {gameObject.name}");
        }
    }
    */
    public void SetCoord(Vector2Int pos)
    {
        coord = pos;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        ItemSelectorManager.Instance.Select(this);
    }

    public int GetLevel()
    {
        return currentLevel;
    }
}
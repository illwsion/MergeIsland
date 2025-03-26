// ItemView.cs
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.U2D;

public class ItemView : MonoBehaviour
{
    public Image iconImage;       // 셀에 표시할 아이템 이미지
    private int currentLevel;     // 현재 아이템 레벨 저장

    public void SetItem(MergeItem item)
    {
        currentLevel = item.level;
        string spriteName = $"{item.type}_{item.level}";
        iconImage.sprite = AtlasManager.Instance.GetSprite(spriteName);
        Debug.Log(spriteName);
    }

    public int GetLevel()
    {
        return currentLevel;
    }
}
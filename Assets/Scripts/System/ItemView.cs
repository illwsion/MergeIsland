// ItemView.cs
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.U2D;

public class ItemView : MonoBehaviour
{
    public Image iconImage;       // ���� ǥ���� ������ �̹���
    private int currentLevel;     // ���� ������ ���� ����

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
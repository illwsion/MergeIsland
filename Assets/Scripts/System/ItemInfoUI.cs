// ItemInfoUI.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemInfoUI : MonoBehaviour
{
    public GameObject root;
    public Image iconImage;
    public TMP_Text nameText;
    public TMP_Text descText;
    public TMP_Text levelText;

    public void Show(MergeItem item)
    {
        root.SetActive(true);

        // 아이콘 표시
        string spriteName = item.name;
        iconImage.sprite = AtlasManager.Instance.GetSprite(spriteName);

        // 텍스트 표시
        string localizedName = StringTableManager.Instance.GetLocalized(item.Data.itemNameID);
        nameText.text = localizedName;
        string localizedDesc = StringTableManager.Instance.GetLocalized(item.Data.descriptionID);
        descText.text = localizedDesc;
        levelText.text = $"Lv.{item.level}";
    }

    public void Hide()
    {
        root.SetActive(false);
    }

    public void ShowEmpty()
    {
        root.SetActive(true);
        iconImage.sprite = null;
        nameText.text = "아이템 없음";
        levelText.text = "";
    }
}
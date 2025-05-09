// ItemInfoUI.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemInfoUI : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private GameObject root;

    [Header("Header")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private Button itemInfoDetailButton;

    [Header("Optional Blocks")]
    [SerializeField] private TMP_Text descText;

    [Header("HP Block")]
    [SerializeField] private GameObject hpBlock;
    [SerializeField] private TMP_Text hpText;
    [SerializeField] private RectTransform hpFillBar;

    [Header("Storage Block")]
    [SerializeField] private GameObject storageBlock;
    [SerializeField] private TMP_Text storageText;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private RectTransform timerFillBar;

    [Header("Settings")]
    public float maxBarWidth = 200f; // Fill 바의 최대 길이 (px) 기준

    public void Show(MergeItem item)
    {
        root.SetActive(true);

        // Header
        iconImage.sprite = AtlasManager.Instance.GetSprite(item.name);
        nameText.text = StringTableManager.Instance.GetLocalized(item.Data.itemNameID);
        levelText.text = $"Lv.{item.level}";

        // Description
        descText.text = StringTableManager.Instance.GetLocalized(item.Data.descriptionID);
        
        // HP Block (조건부 표시)
        if (item.maxHP > 0)
        {
            hpBlock.SetActive(true);
            //Fill 길이 계산
            float hpPercent = Mathf.Clamp01((float)item.currentHP / (float)item.maxHP);
            hpFillBar.sizeDelta = new Vector2(hpPercent * maxBarWidth, hpFillBar.sizeDelta.y);
            hpText.text = $"{item.currentHP} / {item.maxHP}";
        }
        else
        {
            hpBlock.SetActive(false);
        }

        // Storage Block (조건부 표시)
        if (item.ProduceType == ItemData.ProduceType.Auto)
        {
            storageBlock.SetActive(true);
            UpdateStorageBlock(item);
        }
        else
        {
            storageBlock.SetActive(false);
        }
    }

    public void Refresh(MergeItem item)
    {
        if (!root.activeSelf) return;

        if (item.IsTimeDrivenProducer())
        {
            UpdateStorageBlock(item);
        }
        if (item.maxHP > 0)
        {
            float hpPercent = Mathf.Clamp01((float)item.currentHP / (float)item.maxHP);
            hpFillBar.sizeDelta = new Vector2(hpPercent * maxBarWidth, hpFillBar.sizeDelta.y);
            hpText.text = $"{item.currentHP} / {item.maxHP}";
        }
    }

    private void UpdateStorageBlock(MergeItem item)
    {
        storageText.text = $"{item.currentStorage} / {item.maxStorage}";

        float remain = item.Data.productionInterval - item.recoveryTimer;
        timerText.text = (item.currentStorage >= item.maxStorage)
            ? "Full"
            : $"{Mathf.FloorToInt(remain / 60f):D2}:{Mathf.FloorToInt(remain % 60f):D2}";

        // Fill 길이 조절 (직접 width 조정 방식)
        if (timerFillBar != null)
        {
            float percent = Mathf.Clamp01(remain / item.Data.productionInterval);
            timerFillBar.sizeDelta = new Vector2(percent * maxBarWidth, timerFillBar.sizeDelta.y);
        }
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
        descText.text = "아이템을 선택하면 정보가 표시됩니다";
        hpBlock.SetActive(false);
        storageBlock.SetActive(false);
        timerText.gameObject.SetActive(false);
    }

    
}
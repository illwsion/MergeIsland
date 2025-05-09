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
    public float maxBarWidth = 200f; // Fill ���� �ִ� ���� (px) ����

    public void Show(MergeItem item)
    {
        root.SetActive(true);

        // Header
        iconImage.sprite = AtlasManager.Instance.GetSprite(item.name);
        nameText.text = StringTableManager.Instance.GetLocalized(item.Data.itemNameID);
        levelText.text = $"Lv.{item.level}";

        // Description
        descText.text = StringTableManager.Instance.GetLocalized(item.Data.descriptionID);
        
        // HP Block (���Ǻ� ǥ��)
        if (item.maxHP > 0)
        {
            hpBlock.SetActive(true);
            //Fill ���� ���
            float hpPercent = Mathf.Clamp01((float)item.currentHP / (float)item.maxHP);
            hpFillBar.sizeDelta = new Vector2(hpPercent * maxBarWidth, hpFillBar.sizeDelta.y);
            hpText.text = $"{item.currentHP} / {item.maxHP}";
        }
        else
        {
            hpBlock.SetActive(false);
        }

        // Storage Block (���Ǻ� ǥ��)
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

        // Fill ���� ���� (���� width ���� ���)
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
        nameText.text = "������ ����";
        levelText.text = "";
        descText.text = "�������� �����ϸ� ������ ǥ�õ˴ϴ�";
        hpBlock.SetActive(false);
        storageBlock.SetActive(false);
        timerText.gameObject.SetActive(false);
    }

    
}
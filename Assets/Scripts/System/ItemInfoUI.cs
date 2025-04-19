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
    public TMP_Text storageText;
    public TMP_Text timerText;

    public void Show(MergeItem item)
    {
        root.SetActive(true);

        // ������ ǥ��
        string spriteName = item.name;
        iconImage.sprite = AtlasManager.Instance.GetSprite(spriteName);

        // �ؽ�Ʈ ǥ��
        string localizedName = StringTableManager.Instance.GetLocalized(item.Data.itemNameID);
        nameText.text = localizedName;
        string localizedDesc = StringTableManager.Instance.GetLocalized(item.Data.descriptionID);
        descText.text = localizedDesc;
        levelText.text = $"Lv.{item.level}";

        // ���差 UI ǥ��
        if (item.IsTimeDrivenProducer())
        {
            storageText.gameObject.SetActive(true);
            timerText.gameObject.SetActive(true);

            storageText.text = $"{item.currentStorage} / {item.maxStorage}";

            if (item.currentStorage >= item.maxStorage)
            {
                timerText.text = "Full";
            }
            else
            {
                float remain = item.Data.productionInterval - item.recoveryTimer;
                int minutes = Mathf.FloorToInt(remain / 60f);
                int seconds = Mathf.FloorToInt(remain % 60f);
                timerText.text = $"{minutes:D2}:{seconds:D2}";
            }
        }
        else
        {
            storageText.gameObject.SetActive(false);
            timerText.gameObject.SetActive(false);
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
        storageText.gameObject.SetActive(false);
        timerText.gameObject.SetActive(false);
    }

    public void Refresh(MergeItem item)
    {
        if (!root.activeSelf) return;

        if (item.IsTimeDrivenProducer())
        {
            storageText.text = $"{item.currentStorage} / {item.maxStorage}";

            if (item.currentStorage >= item.maxStorage)
            {
                timerText.text = "Full";
            }
            else
            {
                float remain = item.Data.productionInterval - item.recoveryTimer;
                int minutes = Mathf.FloorToInt(remain / 60f);
                int seconds = Mathf.FloorToInt(remain % 60f);
                timerText.text = $"{minutes:D2}:{seconds:D2}";
            }
        }
    }
}
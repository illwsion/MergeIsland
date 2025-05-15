// ItemInfoUI.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ItemInfoUI : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private GameObject root;

    [Header("Header")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private Button itemInfoDetailButton;

    [Header("Description")]
    [SerializeField] private TMP_Text descText;

    [Header("HP Block")]
    [SerializeField] private GameObject hpBlock;
    [SerializeField] private TMP_Text hpText;
    [SerializeField] private RectTransform hpFillBar;

    [Header("Manual Block")]
    [SerializeField] private GameObject manualBlock;
    [SerializeField] private TMP_Text manualStorageText;
    [SerializeField] private TMP_Text manualTimerText;
    [SerializeField] private RectTransform manualTimerFillBar;

    [Header("Auto Block")]
    [SerializeField] private GameObject autoBlock;
    [SerializeField] private TMP_Text autoStorageText;
    [SerializeField] private TMP_Text autoTimerText;
    [SerializeField] private RectTransform autoTimerFillBar;

    [SerializeField] private EffectGroup effectGroup;

    public void Show(MergeItem item)
    {
        root.SetActive(true);

        // Header
        iconImage.sprite = AtlasManager.Instance.GetSprite(item.imageName);
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
            float barWidth = hpFillBar.parent.GetComponent<RectTransform>().rect.width;
            hpFillBar.sizeDelta = new Vector2(hpPercent * barWidth, hpFillBar.sizeDelta.y);
            hpText.text = $"{item.currentHP} / {item.maxHP}";
        }
        else
        {
            hpBlock.SetActive(false);
        }

        // Manual Block
        if (item.ProduceType == ItemData.ProduceType.Manual)
        {
            manualBlock.SetActive(true);
            autoBlock.SetActive(false);

            float percent = Mathf.Clamp01((float)item.currentStorage / item.maxStorage);
            manualStorageText.text = $"{item.currentStorage} / {item.maxStorage}";
            manualTimerText.text = item.currentStorage >= item.maxStorage ? "Full" : ""; // Optional timer text
            float barWidth = manualTimerFillBar.parent.GetComponent<RectTransform>().rect.width;
            manualTimerFillBar.sizeDelta = new Vector2(percent * barWidth, manualTimerFillBar.sizeDelta.y);
        }
        // Auto Block
        else if (item.ProduceType == ItemData.ProduceType.Auto)
        {
            manualBlock.SetActive(false);
            autoBlock.SetActive(true);

            float remain = item.Data.productionInterval - item.recoveryTimer;
            float percent = Mathf.Clamp01(item.recoveryTimer / item.Data.productionInterval);

            autoStorageText.text = $"{item.currentStorage} / {item.maxStorage}";
            autoTimerText.text = item.currentStorage >= item.maxStorage
                ? "Full"
                : $"{Mathf.FloorToInt(remain / 60f):D2}:{Mathf.FloorToInt(remain % 60f):D2}";
            float barWidth = autoTimerFillBar.parent.GetComponent<RectTransform>().rect.width;
            autoTimerFillBar.sizeDelta = new Vector2(percent * barWidth, autoTimerFillBar.sizeDelta.y);
        }
        else
        {
            manualBlock.SetActive(false);
            autoBlock.SetActive(false);
        }

        //EffectGroup 
        effectGroup.Clear();
        List<EffectData> effects = CreateEffectDataList(item);
        foreach (var effect in effects)
        {
            effectGroup.AddEffect(effect);
        }
    }

    public void Refresh(MergeItem item)
    {
        if (!root.activeSelf) return;

        if (item.maxHP > 0)
        {
            float hpPercent = Mathf.Clamp01((float)item.currentHP / (float)item.maxHP);
            float barWidth = hpFillBar.parent.GetComponent<RectTransform>().rect.width;
            hpFillBar.sizeDelta = new Vector2(hpPercent * barWidth, hpFillBar.sizeDelta.y);
            hpText.text = $"{item.currentHP} / {item.maxHP}";
        }

        if (item.ProduceType == ItemData.ProduceType.Manual)
        {
            float remain = item.Data.productionInterval - item.recoveryTimer;
            float percent = Mathf.Clamp01((float)item.currentStorage / item.maxStorage);
            manualStorageText.text = $"{item.currentStorage} / {item.maxStorage}";
            manualTimerText.text = item.currentStorage >= item.maxStorage
                ? "Full"
                : $"{Mathf.FloorToInt(remain / 60f):D2}:{Mathf.FloorToInt(remain % 60f):D2}";
            float barWidth = manualTimerFillBar.parent.GetComponent<RectTransform>().rect.width;
            manualTimerFillBar.sizeDelta = new Vector2(percent * barWidth, manualTimerFillBar.sizeDelta.y);
        }
        else if (item.ProduceType == ItemData.ProduceType.Auto)
        {
            float remain = item.Data.productionInterval - item.recoveryTimer;
            float percent = Mathf.Clamp01(item.recoveryTimer / item.Data.productionInterval);

            autoStorageText.text = $"{item.currentStorage} / {item.maxStorage}";
            autoTimerText.text = item.currentStorage >= item.maxStorage
                ? "Full"
                : $"{Mathf.FloorToInt(remain / 60f):D2}:{Mathf.FloorToInt(remain % 60f):D2}";
            float barWidth = autoTimerFillBar.parent.GetComponent<RectTransform>().rect.width;
            autoTimerFillBar.sizeDelta = new Vector2(percent * barWidth, autoTimerFillBar.sizeDelta.y);
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
        manualBlock.SetActive(false);
        autoBlock.SetActive(false);
    }

    private List<EffectData> CreateEffectDataList(MergeItem item)
    {
        var effects = new List<EffectData>();

        // 1. 터치해서 획득
        if (item.ProduceType == ItemData.ProduceType.Gather)
        {
            var gatherType = item.GatherResource;
            var icon = GetGatherIcon(gatherType);
            effects.Add(new EffectData
            {
                type = EffectType.Gather,
                blockSize = EffectBlockSize.Small,
                label = StringTableManager.Instance.GetLocalized("effectLabel_Gather"),
                icon1 = icon,
                value = $"+{item.gatherValue}"
            });
        }

        // 2. 판매
        if (item.sellValue > 0)
        {
            effects.Add(new EffectData
            {
                type = EffectType.Sell,
                blockSize = EffectBlockSize.Small,
                label = StringTableManager.Instance.GetLocalized("effectLabel_Sell"),
                icon1 = AtlasManager.Instance.GetSprite("resourceIcon_gold"),
                value = $"+{item.sellValue}"
            });
        }

        // 3. 대미지
        if (item.attackPower > 0)
        {
            effects.Add(new EffectData
            {
                type = EffectType.Damage,
                blockSize = EffectBlockSize.Small,
                label = StringTableManager.Instance.GetLocalized("effectLabel_Damage"),
                icon1 = AtlasManager.Instance.GetSprite("effectIcon_damage"),
                value = item.attackPower.ToString()
            });
            
        }

        // 4. 공급
        if (item.ProduceType == ItemData.ProduceType.Supply)
        {
            Sprite icon = GetMainProduceIcon(item.produceTableID);

            effects.Add(new EffectData
            {
                type = EffectType.Supply,
                blockSize = EffectBlockSize.Large,
                label = StringTableManager.Instance.GetLocalized("effectLabel_Supply"),
                icon1 = AtlasManager.Instance.GetSprite("supply"),
                icon2 = AtlasManager.Instance.GetSprite("supply"),
                //소모 아이템과 생산 아이템 이미지
                value = null
            });
        }

        // 5. 드랍
        if (item.maxHP > 0)
        {
            effects.Add(new EffectData
            {
                type = EffectType.Drop,
                blockSize = EffectBlockSize.Small,
                label = StringTableManager.Instance.GetLocalized("effectLabel_Drop"),
                icon1 = AtlasManager.Instance.GetSprite("chest"),
                //드랍하는 아이템
                value = null
            });
        }

        // 6. 생산
        if (item.Category == ItemData.Category.Production)
        {
            Sprite icon = GetMainProduceIcon(item.produceTableID);
            //나중에 차례대로 바뀌고 아래에 확률 나와도 좋을 것 같음
            effects.Add(new EffectData
            {
                type = EffectType.Produce,
                blockSize = EffectBlockSize.Small,
                label = StringTableManager.Instance.GetLocalized("effectLabel_Produce"),
                icon1 = icon,
                value = null
            });
        }

        return effects;
    }

    private Sprite GetGatherIcon(ItemData.GatherResource type)
    {
        switch (type)
        {
            case ItemData.GatherResource.Gold:
                return AtlasManager.Instance.GetSprite("resourceIcon_gold");
            case ItemData.GatherResource.Wood:
                return AtlasManager.Instance.GetSprite("resourceIcon_wood");
            case ItemData.GatherResource.Stone:
                return AtlasManager.Instance.GetSprite("resourceIcon_stone");
            case ItemData.GatherResource.Iron:
                return AtlasManager.Instance.GetSprite("resourceIcon_iron");
            case ItemData.GatherResource.Energy:
                return AtlasManager.Instance.GetSprite("resourceIcon_energy");
            case ItemData.GatherResource.Gem:
                return AtlasManager.Instance.GetSprite("resourceIcon_gem");
            default:
                return AtlasManager.Instance.GetSprite("resourceIcon_default");
        }
    }

    public Sprite GetMainProduceIcon(int tableID)
    {
        var table = ProduceTableManager.Instance.GetTable(tableID);

        if (table == null || table.results == null || table.results.Count == 0)
            return AtlasManager.Instance.GetSprite("effectIcon_unknown");

        // 확률이 가장 높은 결과 선택
        ProduceResult best = table.results[0];

        foreach (var r in table.results)
        {
            if (r.probability > best.probability)
                best = r;
        }

        var bestItemData = ItemDataManager.Instance.GetItemData(best.itemID);
        if (bestItemData == null)
            return AtlasManager.Instance.GetSprite("effectIcon_unknown");

        return AtlasManager.Instance.GetSprite(bestItemData.imageName); // name은 스프라이트 키
    }
}
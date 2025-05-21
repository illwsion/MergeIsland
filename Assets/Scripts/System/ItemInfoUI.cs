// ItemInfoUI.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ItemInfoUI : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private GameObject root;
    [SerializeField] private GameObject unselectedOverlay;
    [SerializeField] private TMP_Text unselectedText;

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

    void Start()
    {
        if (unselectedText == null)
            Debug.LogError("[ItemInfoUI] unselectedText�� ������� �ʾҽ��ϴ�.");
        unselectedText.text = StringTableManager.Instance.GetLocalized("UITEXT_UNSELECTEDTEXT");
    }

    public void Show(MergeItem item)
    {
        root.SetActive(true);
        unselectedOverlay.SetActive(false);
        iconImage.gameObject.SetActive(true);
        // Header
        iconImage.sprite = AtlasManager.Instance.GetSprite(item.imageName);
        nameText.text = StringTableManager.Instance.GetLocalized(item.Data.itemNameKey);
        if (item.level > 0)
        {
            levelText.text = $"Lv.{item.level}";
        }
        else
        {
            levelText.text = "";
        }


        // Description
        descText.text = StringTableManager.Instance.GetLocalized(item.Data.itemDescriptionKey);
        
        // HP Block (���Ǻ� ǥ��)
        if (item.maxHP > 0)
        {
            hpBlock.SetActive(true);
            //Fill ���� ���
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

            if (!item.Data.isProductionLimited)
            {
                manualTimerText.text = item.currentStorage >= item.maxStorage
                    ? "Full"
                    : $"{Mathf.FloorToInt(item.GetRecoveryRemainingTime() / 60f):D2}:{Mathf.FloorToInt(item.GetRecoveryRemainingTime() % 60f):D2}";
            }
            else
            {
                Debug.Log("��ǥ��");
                manualTimerText.text = ""; // �ð� ��ǥ��
            }
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

            autoStorageText.text = item.Data.isProductionLimited 
                ? $"{item.currentStorage} / {item.maxStorage}" 
                : ""; // ��ǥ��


            autoTimerText.text = (item.isProductionBlocked)
                ? "Blocked"
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
            float percent = Mathf.Clamp01((float)item.currentStorage / item.maxStorage);
            manualStorageText.text = $"{item.currentStorage} / {item.maxStorage}";
            if (!item.Data.isProductionLimited)
            {
                float remain = item.Data.productionInterval - item.recoveryTimer;
                manualTimerText.text = item.currentStorage >= item.maxStorage
                    ? "Full"
                    : $"{Mathf.FloorToInt(remain / 60f):D2}:{Mathf.FloorToInt(remain % 60f):D2}";
            }
            else
            {
                manualTimerText.text = ""; // �ð� ��ǥ��
            }
            float barWidth = manualTimerFillBar.parent.GetComponent<RectTransform>().rect.width;
            manualTimerFillBar.sizeDelta = new Vector2(percent * barWidth, manualTimerFillBar.sizeDelta.y);
        }
        else if (item.ProduceType == ItemData.ProduceType.Auto)
        {
            float remain = item.Data.productionInterval - item.recoveryTimer;
            float percent = Mathf.Clamp01(item.recoveryTimer / item.Data.productionInterval);

            autoStorageText.text = item.Data.isProductionLimited 
                ? $"{item.currentStorage} / {item.maxStorage}" 
                : ""; // ��ǥ��

            autoTimerText.text = (item.isProductionBlocked)
                ? "Blocked"
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
        unselectedOverlay.SetActive(true);

        iconImage.sprite = null;
        nameText.text = "������ ����";
        levelText.text = "";
        descText.text = "�������� �����ϸ� ������ ǥ�õ˴ϴ�";

        effectGroup.Clear();
        iconImage.gameObject.SetActive(false);
        hpBlock.SetActive(false);
        manualBlock.SetActive(false);
        autoBlock.SetActive(false);
    }

    private List<EffectData> CreateEffectDataList(MergeItem item)
    {
        var effects = new List<EffectData>();

        // 1. ��ġ�ؼ� ȹ��
        if (item.ProduceType == ItemData.ProduceType.Gather)
        {
            var gatherType = item.GatherResource;
            var icon = GetGatherIcon(gatherType);
            effects.Add(new EffectData
            {
                type = EffectType.Gather,
                blockSize = EffectBlockSize.Small,
                label = StringTableManager.Instance.GetLocalized("EFFECTLABEL_GATHER"),
                icon1 = icon,
                value = $"+{item.gatherValue}"
            });
        }

        // 2. �Ǹ�
        if (item.sellValue > 0)
        {
            effects.Add(new EffectData
            {
                type = EffectType.Sell,
                blockSize = EffectBlockSize.Small,
                label = StringTableManager.Instance.GetLocalized("EFFECTLABEL_SELL"),
                icon1 = AtlasManager.Instance.GetSprite("resourceIcon_gold"),
                value = $"+{item.sellValue}"
            });
        }

        // 3. �����
        if (item.attackPower > 0)
        {
            effects.Add(new EffectData
            {
                type = EffectType.Damage,
                blockSize = EffectBlockSize.Small,
                label = StringTableManager.Instance.GetLocalized("EFFECTLABEL_DAMAGE"),
                icon1 = AtlasManager.Instance.GetSprite("effectIcon_damage"),
                value = item.attackPower.ToString()
            });
            
        }

        // 4. ����
        if (SupplyRuleManager.Instance.GetFirstRuleByReceiverItem(item.key) != null)
        {
            // receiverItem�� �����ϴ� rule �� ù ��°�� ������
            var rule = SupplyRuleManager.Instance.GetFirstRuleByReceiverItem(item.key);
            if (rule == null)
            {
                Debug.Log($"[CreateEffectDataList] SupplyRule�� �������� ����: itemID={item.key}");
            }
            if (rule != null)
            {
                var suppliedItemData = ItemDataManager.Instance.GetItemData(rule.suppliedItem);
                Sprite icon1 = AtlasManager.Instance.GetSprite(suppliedItemData?.imageName);

                // icon2 = ���� ���
                Sprite icon2;
                string value = null;

                if (rule.resultType == SupplyRule.ResultType.Item)
                {
                    var resultData = ItemDataManager.Instance.GetItemData(rule.resultItem);
                    icon2 = AtlasManager.Instance.GetSprite(resultData?.imageName);
                }
                else
                {
                    //���߿� ������ �ٲ�� �� (�ڿ��� �����ϰ� �ȴٸ�)
                    icon2 = AtlasManager.Instance.GetSprite(rule.resultType.ToString().ToLower()); // ��: gold �� gold ������
                    value = rule.resultValue.ToString();
                }

                effects.Add(new EffectData
                {
                    type = EffectType.Supply,
                    blockSize = EffectBlockSize.Large,
                    label = StringTableManager.Instance.GetLocalized("EFFECTLABEL_SUPPLY"),
                    icon1 = icon1,
                    icon2 = icon2,
                    value = value
                });
            }
        }

        // 5. ���
        if (!string.IsNullOrEmpty(item.dropTableKey))
        {
            Sprite icon = GetMainDropIcon(item.dropTableKey);
            //���߿� ���ʴ�� �ٲ�� �Ʒ��� Ȯ�� ���͵� ���� �� ����
            effects.Add(new EffectData
            {
                type = EffectType.Drop,
                blockSize = EffectBlockSize.Small,
                label = StringTableManager.Instance.GetLocalized("EFFECTLABEL_DROP"),
                icon1 = icon,
                value = null
            });
        }

        // 6. ����
        if (!string.IsNullOrEmpty(item.produceTableKey))
        {
            Sprite icon = GetMainProduceIcon(item.produceTableKey);
            //���߿� ���ʴ�� �ٲ�� �Ʒ��� Ȯ�� ���͵� ���� �� ����
            effects.Add(new EffectData
            {
                type = EffectType.Produce,
                blockSize = EffectBlockSize.Small,
                label = StringTableManager.Instance.GetLocalized("EFFECTLABEL_PRODUCE"),
                icon1 = icon,
                value = null
            });
        }

        return effects;
    }

    private Sprite GetGatherIcon(ResourceType type)
    {
        switch (type)
        {
            case ResourceType.Gold:
                return AtlasManager.Instance.GetSprite("resourceIcon_gold");
            case ResourceType.Wood:
                return AtlasManager.Instance.GetSprite("resourceIcon_wood");
            case ResourceType.Stone:
                return AtlasManager.Instance.GetSprite("resourceIcon_stone");
            case ResourceType.Iron:
                return AtlasManager.Instance.GetSprite("resourceIcon_iron");
            case ResourceType.Energy:
                return AtlasManager.Instance.GetSprite("resourceIcon_energy");
            case ResourceType.Gem:
                return AtlasManager.Instance.GetSprite("resourceIcon_gem");
            default:
                return AtlasManager.Instance.GetSprite("resourceIcon_default");
        }
    }

    public Sprite GetMainProduceIcon(string tableKey)
    {
        var table = ProduceTableManager.Instance.GetTable(tableKey);

        if (table == null || table.results == null || table.results.Count == 0)
            return AtlasManager.Instance.GetSprite("effectIcon_unknown");

        // Ȯ���� ���� ���� ��� ����
        ProduceResult best = table.results[0];

        foreach (var r in table.results)
        {
            if (r.probability > best.probability)
                best = r;
        }

        var bestItemData = ItemDataManager.Instance.GetItemData(best.itemKey);
        if (bestItemData == null)
            return AtlasManager.Instance.GetSprite("effectIcon_unknown");

        return AtlasManager.Instance.GetSprite(bestItemData.imageName); // name�� ��������Ʈ Ű
    }

    public Sprite GetMainDropIcon(string tableKey)
    {
        var table = DropTableManager.Instance.GetTable(tableKey);

        if (table == null || table.results == null || table.results.Count == 0)
            return AtlasManager.Instance.GetSprite("effectIcon_unknown");

        // Ȯ���� ���� ���� ��� ����
        DropResult best = table.results[0];

        foreach (var r in table.results)
        {
            if (r.probability > best.probability)
                best = r;
        }

        var bestItemData = ItemDataManager.Instance.GetItemData(best.itemKey);
        if (bestItemData == null)
            return AtlasManager.Instance.GetSprite("effectIcon_unknown");

        return AtlasManager.Instance.GetSprite(bestItemData.imageName); // name�� ��������Ʈ Ű
    }
}
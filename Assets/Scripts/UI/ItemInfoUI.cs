// ItemInfoUI.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using static ItemData;
using static BoardGateData;
using static UnityEditor.Progress;
using UnityEngine.Video;
using System.Data;
using static Unity.Collections.Unicode;

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

    public static ItemInfoUI Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }
    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
    void Start()
    {
        if (unselectedText == null)
            Debug.LogError("[ItemInfoUI] unselectedText가 연결되지 않았습니다.");
        unselectedText.text = StringTableManager.Instance.GetLocalized("UI_UNSELECTEDTEXT");
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

            if (!item.Data.isProductionLimited)
            {
                manualTimerText.text = item.currentStorage >= item.maxStorage
                    ? "Full"
                    : $"{Mathf.FloorToInt(item.GetRecoveryRemainingTime() / 60f):D2}:{Mathf.FloorToInt(item.GetRecoveryRemainingTime() % 60f):D2}";
            }
            else
            {
                Debug.Log("비표시");
                manualTimerText.text = ""; // 시간 비표시
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
                : ""; // 비표시


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
                manualTimerText.text = ""; // 시간 비표시
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
                : ""; // 비표시

            autoTimerText.text = (item.isProductionBlocked)
                ? "Blocked"
                : $"{Mathf.FloorToInt(remain / 60f):D2}:{Mathf.FloorToInt(remain % 60f):D2}";

            float barWidth = autoTimerFillBar.parent.GetComponent<RectTransform>().rect.width;
            autoTimerFillBar.sizeDelta = new Vector2(percent * barWidth, autoTimerFillBar.sizeDelta.y);
        }
    }

    public void ShowGate(BoardGate gate)
    {
        root.SetActive(true);
        unselectedOverlay.SetActive(false);
        iconImage.gameObject.SetActive(true);

        // Header
        iconImage.sprite = AtlasManager.Instance.GetSprite("boardGate_beach"); // iconName이 있다면
        nameText.text = StringTableManager.Instance.GetLocalized("UI_LOCKEDGATE_NAME");
        levelText.text = ""; // 게이트는 레벨 없음

        // Description
        string desc = StringTableManager.Instance.GetLocalized("UI_LOCKEDGATE_DESC");

        switch (gate.gateData.unlockType)
        {
            case UnlockType.None:
                desc = "열려있는 게이트입니다.";
                break;

            case UnlockType.Item:
                string itemName = StringTableManager.Instance.GetLocalized(ItemDataManager.Instance.GetItemData(gate.gateData.unlockParam).itemNameKey);
                desc = StringTableManager.Instance.GetLocalized("UI_LOCKEDGATE_ITEM", itemName);
                break;

            case UnlockType.Level:
                desc = StringTableManager.Instance.GetLocalized("UI_LOCKEDGATE_LEVEL", gate.gateData.unlockParamValue);
                break;

            case UnlockType.Quest:
                desc = StringTableManager.Instance.GetLocalized("UI_LOCKEDGATE_QUEST", gate.gateData.unlockParam);
                break;

            case UnlockType.Resource:
                desc = StringTableManager.Instance.GetLocalized("UI_LOCKEDGATE_RESOURCE", gate.gateData.unlockParam);
                break;
        }

        descText.text = desc;

        // 상태 관련 블록 숨김
        hpBlock.SetActive(false);
        manualBlock.SetActive(false);
        autoBlock.SetActive(false);

        //EffectGroup 
        effectGroup.Clear();
        EffectData effect = CreateGateEffectData(gate);
        effectGroup.AddEffect(effect);

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
        nameText.text = "아이템 없음";
        levelText.text = "";
        descText.text = "아이템을 선택하면 정보가 표시됩니다";

        effectGroup.Clear();
        iconImage.gameObject.SetActive(false);
        hpBlock.SetActive(false);
        manualBlock.SetActive(false);
        autoBlock.SetActive(false);
    }

    private EffectData CreateGateEffectData(BoardGate gate)
    {
        var effect = new EffectData();
        effect.sourceGate = gate;

        switch (gate.gateData.unlockType)
        {
            case UnlockType.None:
                break;

            case UnlockType.Item:
                effect.type = EffectType.Gate_Supply;
                effect.blockSize = EffectBlockSize.Large;
                effect.label = StringTableManager.Instance.GetLocalized("EFFECTLABEL_GATE_SUPPLY");
                effect.icon1 = AtlasManager.Instance.GetSprite(ItemDataManager.Instance.GetItemData(gate.gateData.unlockParam).imageName);
                effect.icon2 = AtlasManager.Instance.GetSprite("boardGate_lock");
                effect.value = "";
                //effect.value = gate.gateData.unlockParamValue.ToString();
                break;

            case UnlockType.Level:
                effect.type = EffectType.Gate_Supply;
                effect.blockSize = EffectBlockSize.Small;
                effect.label = StringTableManager.Instance.GetLocalized("EFFECTLABEL_GATE_LEVEL");
                effect.icon1 = AtlasManager.Instance.GetSprite("boardGate_level");
                effect.value = gate.gateData.unlockParamValue.ToString();
                break;

            case UnlockType.Quest:
                effect.type = EffectType.Gate_Quest;
                effect.blockSize = EffectBlockSize.Small;
                effect.label = StringTableManager.Instance.GetLocalized("EFFECTLABEL_GATE_LEVEL");
                effect.icon1 = AtlasManager.Instance.GetSprite("boardGate_quest");
                effect.value = "";
                //effect.value = gate.gateData.unlockParamValue.ToString(); 퀘스트 조건 어떻게 나타낼지? 퀘스트 시스템 구현 이후
                break;

            case UnlockType.Resource:
                effect.type = EffectType.Gate_Resource;
                effect.blockSize = EffectBlockSize.Small;
                effect.label = StringTableManager.Instance.GetLocalized("EFFECTLABEL_GATE_RESOURCE");
                effect.icon1 = GetGatherIcon(ResourceTypeUtil.StringToResourceType(gate.gateData.unlockParam));
                effect.value = gate.gateData.unlockParamValue.ToString();
                break;
        }

        return effect;
    }

    private List<EffectData> CreateEffectDataList(MergeItem item)
    {
        var effects = new List<EffectData>();

        // 1. 터치해서 획득
        if (item.ProduceType == ItemData.ProduceType.Gather)
        {
            var gatherType = item.gatherResource;
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

        // 2. 판매
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

        // 3. 대미지
        if (item.attackPower > 0)
        {
            // ToolType에 따라 아이콘 이름 설정
            string iconName = item.Data.toolType switch
            {
                ToolType.Weapon => "effectIcon_damage_weapon",
                ToolType.Axe => "effectIcon_damage_axe",
                ToolType.Pickaxe => "effectIcon_damage_pickaxe",
                _ => "resourceIcon_default" // fallback
            };

            effects.Add(new EffectData
            {
                type = EffectType.Damage,
                blockSize = EffectBlockSize.Small,
                label = StringTableManager.Instance.GetLocalized("EFFECTLABEL_DAMAGE"),
                icon1 = AtlasManager.Instance.GetSprite(iconName),
                value = item.attackPower.ToString()
            });
            
        }

        // 4. 공급
        if (SupplyRuleManager.Instance.GetFirstRuleByReceiverItem(item.key) != null)
        {
            // receiverItem로 시작하는 rule 중 첫 번째를 가져옴
            var rule = SupplyRuleManager.Instance.GetFirstRuleByReceiverItem(item.key);
            if (rule == null)
            {
                Debug.Log($"[CreateEffectDataList] SupplyRule이 존재하지 않음: itemID={item.key}");
            }
            if (rule != null)
            {
                var suppliedItemData = ItemDataManager.Instance.GetItemData(rule.suppliedItem);
                Sprite icon1 = AtlasManager.Instance.GetSprite(suppliedItemData?.imageName);

                // icon2 = 생산 결과
                Sprite icon2;
                string value = null;

                if (rule.resultType == SupplyRule.ResultType.Item)
                {
                    var resultData = ItemDataManager.Instance.GetItemData(rule.resultItem);
                    icon2 = AtlasManager.Instance.GetSprite(resultData?.imageName);
                }
                else
                {
                    //나중에 아이콘 바꿔야 함 (자원도 생산하게 된다면)
                    icon2 = AtlasManager.Instance.GetSprite(rule.resultType.ToString().ToLower()); // 예: gold → gold 아이콘
                    value = rule.resultValue.ToString();
                }
                //조건에 따라 label을 "먹이주기" 등으로 바꾸면 될듯. Animal일 때 먹이주기, Crop일 때 작물에 주는 느낌같이
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

        // 5. 드랍
        if (!string.IsNullOrEmpty(item.dropTableKey))
        {
            Sprite icon = GetMainDropIcon(item.dropTableKey);
            //나중에 차례대로 바뀌고 아래에 확률 나와도 좋을 것 같음
            effects.Add(new EffectData
            {
                type = EffectType.Drop,
                blockSize = EffectBlockSize.Small,
                label = StringTableManager.Instance.GetLocalized("EFFECTLABEL_DROP"),
                icon1 = icon,
                value = null
            });
        }

        // 6. 생산
        if (!string.IsNullOrEmpty(item.produceTableKey))
        {
            Sprite icon = GetMainProduceIcon(item.produceTableKey);
            //나중에 차례대로 바뀌고 아래에 확률 나와도 좋을 것 같음
            effects.Add(new EffectData
            {
                type = EffectType.Produce,
                blockSize = EffectBlockSize.Small,
                label = StringTableManager.Instance.GetLocalized("EFFECTLABEL_PRODUCE"),
                icon1 = icon,
                value = null
            });
        }

        // 7. 최대 저장량
        if (item.maxCapResource != ResourceType.None)
        {
            var resourceType = item.maxCapResource;
            var icon = GetGatherIcon(resourceType);
            effects.Add(new EffectData
            {
                type = EffectType.Produce,
                blockSize = EffectBlockSize.Small,
                label = StringTableManager.Instance.GetLocalized("EFFECTLABEL_MAXCAP"),
                icon1 = icon,
                value = item.maxCapValue.ToString()
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
            case ResourceType.Exp:
                return AtlasManager.Instance.GetSprite("exp_0");
            default:
                return AtlasManager.Instance.GetSprite("resourceIcon_default");
        }
    }

    public Sprite GetMainProduceIcon(string tableKey)
    {
        var table = ProduceTableManager.Instance.GetTable(tableKey);

        if (table == null || table.results == null || table.results.Count == 0)
            return AtlasManager.Instance.GetSprite("effectIcon_unknown");

        // 확률이 가장 높은 결과 선택
        ProduceResult best = table.results[0];

        foreach (var r in table.results)
        {
            if (r.probability > best.probability)
                best = r;
        }

        var bestItemData = ItemDataManager.Instance.GetItemData(best.itemKey);
        if (bestItemData == null)
            return AtlasManager.Instance.GetSprite("effectIcon_unknown");

        return AtlasManager.Instance.GetSprite(bestItemData.imageName); // name은 스프라이트 키
    }

    public Sprite GetMainDropIcon(string tableKey)
    {
        var table = DropTableManager.Instance.GetTable(tableKey);

        if (table == null || table.results == null || table.results.Count == 0)
            return AtlasManager.Instance.GetSprite("effectIcon_unknown");

        // 확률이 가장 높은 결과 선택
        DropResult best = table.results[0];

        foreach (var r in table.results)
        {
            if (r.probability > best.probability)
                best = r;
        }

        var bestItemData = ItemDataManager.Instance.GetItemData(best.itemKey);
        if (bestItemData == null)
            return AtlasManager.Instance.GetSprite("effectIcon_unknown");

        return AtlasManager.Instance.GetSprite(bestItemData.imageName); // name은 스프라이트 키
    }
}
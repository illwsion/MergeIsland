// ResourceUIManager.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Resources;
using System.Collections.Generic;

public class ResourceUIManager : MonoBehaviour
{
    [Header("Bar UI")]
    public ResourceSlotUI energySlot;
    public ResourceSlotUI expSlot;
    public ResourceSlotUI woodSlot;
    public ResourceSlotUI stoneSlot;
    public ResourceSlotUI ironSlot;

    [Header("Simple UI")]
    public ResourceSlotUI goldSlot;
    public ResourceSlotUI gemSlot;

    private Dictionary<ResourceType, ResourceSlotUI> slotTable;

    public void Init()
    {
        slotTable = new Dictionary<ResourceType, ResourceSlotUI>
    {
        { ResourceType.Energy, energySlot },
        { ResourceType.Exp, expSlot },
        { ResourceType.Wood, woodSlot },
        { ResourceType.Stone, stoneSlot },
        { ResourceType.Iron, ironSlot },
        { ResourceType.Gold, goldSlot },
        { ResourceType.Gem, gemSlot }
    };
    }

    public void UpdateUI(ResourceType type, int current, int max)
    {
        if (slotTable == null)
        {
            Debug.LogError("slotTable�� �ʱ�ȭ���� �ʾҽ��ϴ�.");
            return;
        }

        if (!slotTable.TryGetValue(type, out var slot) || slot == null)
        {
            Debug.LogError($"������ �������� �ʰų� null�Դϴ�: {type}");
            return;
        }

        slot.UpdateUI(current, max);
    }

}

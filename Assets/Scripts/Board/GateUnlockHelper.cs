using System.Xml.Linq;
using UnityEngine;
using static BoardGateData;

public static class GateUnlockHelper
{
    public static void TryUnlock(EffectData effectData)
    {
        if (effectData == null)
        {
            Debug.LogWarning("EffectData is null.");
            return;
        }

        switch (effectData.type)
        {
            case EffectType.Gate_Resource:
                ResourceType costType = ResourceTypeUtil.StringToResourceType(effectData.sourceGate.gateData.unlockParam);
                int costValue = effectData.sourceGate.gateData.unlockParamValue;
                if (costType != ResourceType.None)
                {
                    if (!PlayerResourceManager.Instance.TrySpend(costType, costValue))
                    {
                        UIToast.Show("자원이 부족합니다!");
                        Debug.LogWarning($"[ProduceManual] 자원 부족: {costType} {costValue}");
                    }
                    else
                    {
                        effectData.sourceGate.UnlockGate();
                        ItemSelectorManager.Instance.ClearSelection();
                    }
                }
                break;
            case EffectType.Gate_Level:
                if (PlayerLevelManager.Instance.CurrentLevel >= effectData.sourceGate.gateData.unlockParamValue)
                {
                    effectData.sourceGate.UnlockGate();
                    ItemSelectorManager.Instance.ClearSelection();
                }
                else
                {
                    UIToast.Show("레벨이 부족합니다!");
                }
                break;
            case EffectType.Gate_Quest: // 퀘스트 시스템 구현 이후 구현 예정
            case EffectType.Gate_Supply: // 아이템 드래그 해제 전용이므로 클릭 해제 불가
                break;

            default:
                Debug.Log($"[Unlock] Effect type {effectData.type} not supported for unlock.");
                break;
        }
    }

    public static EffectType ConvertToEffectType(UnlockType unlockType)
    {
        switch (unlockType)
        {
            case UnlockType.Resource:
                return EffectType.Gate_Resource;
            case UnlockType.Level:
                return EffectType.Gate_Level;
            case UnlockType.Quest:
                return EffectType.Gate_Quest;
            case UnlockType.Item:
                return EffectType.Gate_Supply;
            default:
                Debug.LogWarning($"[ConvertToEffectType] Unknown unlock type: {unlockType}");
                return EffectType.Gather;
        }
    }
}
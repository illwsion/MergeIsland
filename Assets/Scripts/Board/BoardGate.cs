// BoardGate.cs
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BoardGate : MonoBehaviour, IPointerClickHandler
{
    public BoardGateData gateData; // Inspector 혹은 코드에서 할당됨
    [SerializeField] private GameObject lockIcon;

    public void Initialize(BoardGateData data)
    {
        gateData = data;
        UpdateLockIcon();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (this == null || gateData == null || this.Equals(null))
        {
            Debug.LogWarning("[BoardGate.OnPointerClick] 이 게이트는 이미 파괴됨");
            return;
        }

        if (gateData.isLocked)
        {
            if (ItemSelectorManager.Instance.IsGateSelected(this))
            {
                Debug.Log("이미 선택된 게이트");
                GateUnlockHelper.TryUnlock(new EffectData
                {
                    type = GateUnlockHelper.ConvertToEffectType(gateData.unlockType),
                    sourceGate = this
                });
            }
            else
            {
                Debug.Log("새로운 게이트");
                ItemSelectorManager.Instance.SelectGate(this);
            }
            return;

        }
        else
        {
            // 도착 보드 찾기
            BoardData targetBoard = BoardDataManager.Instance.GetBoardData(gateData.targetBoardKey);
            if (targetBoard != null)
            {
                BoardManager.Instance.MoveBoardTo(targetBoard.key);
            }
            else
            {
                Debug.LogError($"[BoardGate] 도착 보드 '{gateData.targetBoardKey}' 를 찾을 수 없습니다.");
            }
        }
        
    }

    private bool IsUnlocked()
    {
        switch (gateData.unlockType)
        {
            case BoardGateData.UnlockType.None:
                return true;

            case BoardGateData.UnlockType.Item:
                // 아이템 드래그 전용, 직접 해제 불가 → 잠금 상태로 유지
                return false;
                
            case BoardGateData.UnlockType.Level:
                return PlayerLevelManager.Instance.CurrentLevel >= gateData.unlockParamValue;

            case BoardGateData.UnlockType.Quest:
                //return QuestManager.Instance.IsQuestCompleted(gateData.unlockParam);
                return false;
            case BoardGateData.UnlockType.Resource:
                ResourceType costType = ResourceTypeUtil.StringToResourceType(gateData.unlockParam);
                int requiredAmount = gateData.unlockParamValue;
                return PlayerResourceManager.Instance.GetAmount(costType) >= requiredAmount;

            default:
                return false;
        }
    }

    public void UnlockGate()
    {
        gateData.isLocked = false;
        UpdateLockIcon();
    }

    public void UpdateLockIcon()
    {
        if (gateData == null) return;

        if (lockIcon == null)
        {
            Debug.LogWarning("[UpdateLockIcon] lockIcon이 연결되지 않았습니다!");
            return;
        }

        lockIcon.SetActive(gateData.isLocked);
    }
}

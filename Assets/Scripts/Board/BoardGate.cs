// BoardGate.cs
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BoardGate : MonoBehaviour, IPointerClickHandler
{
    public BoardGateData gateData; // Inspector Ȥ�� �ڵ忡�� �Ҵ��
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
            Debug.LogWarning("[BoardGate.OnPointerClick] �� ����Ʈ�� �̹� �ı���");
            return;
        }

        if (gateData.isLocked)
        {
            if (ItemSelectorManager.Instance.IsGateSelected(this))
            {
                Debug.Log("�̹� ���õ� ����Ʈ");
                GateUnlockHelper.TryUnlock(new EffectData
                {
                    type = GateUnlockHelper.ConvertToEffectType(gateData.unlockType),
                    sourceGate = this
                });
            }
            else
            {
                Debug.Log("���ο� ����Ʈ");
                ItemSelectorManager.Instance.SelectGate(this);
            }
            return;

        }
        else
        {
            // ���� ���� ã��
            BoardData targetBoard = BoardDataManager.Instance.GetBoardData(gateData.targetBoardKey);
            if (targetBoard != null)
            {
                BoardManager.Instance.MoveBoardTo(targetBoard.key);
            }
            else
            {
                Debug.LogError($"[BoardGate] ���� ���� '{gateData.targetBoardKey}' �� ã�� �� �����ϴ�.");
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
                // ������ �巡�� ����, ���� ���� �Ұ� �� ��� ���·� ����
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
            Debug.LogWarning("[UpdateLockIcon] lockIcon�� ������� �ʾҽ��ϴ�!");
            return;
        }

        lockIcon.SetActive(gateData.isLocked);
    }
}

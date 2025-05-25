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
        if (gateData == null)
        {
            Debug.LogError("[BoardGate] gateData �� �������� �ʾҽ��ϴ�.");
            return;
        }

        if (gateData.isLocked)
        {
            if (!IsUnlocked())
            {
                //ItemInfoUI�� ������� ���� ������Ʈ?
                return;
            }
        }

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

    private bool IsUnlocked()
    {
        switch (gateData.unlockType)
        {
            case BoardGateData.UnlockType.None:
                return true;
                //���� ���� ���� �ʿ�
            case BoardGateData.UnlockType.Item:
                //return InventoryManager.Instance.HasItem(gateData.unlockParam);

            case BoardGateData.UnlockType.Level:
                if (int.TryParse(gateData.unlockParam, out int requiredLevel))
                {
                    //return PlayerManager.Instance.Level >= requiredLevel;
                }
                return false;

            case BoardGateData.UnlockType.Quest:
                //return QuestManager.Instance.IsQuestCompleted(gateData.unlockParam);

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

// BoardGate.cs
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BoardGate : MonoBehaviour, IPointerClickHandler
{
    public BoardGateData gateData; // Inspector Ȥ�� �ڵ忡�� �Ҵ��

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
                Debug.Log($"[BoardGate] ����Ʈ�� ��� �ֽ��ϴ�. ����: {gateData.unlockType}, ��: {gateData.unlockParam}");
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
}

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
        if (gateData == null)
        {
            Debug.LogError("[BoardGate] gateData 가 설정되지 않았습니다.");
            return;
        }

        if (gateData.isLocked)
        {
            if (!IsUnlocked())
            {
                //ItemInfoUI에 잠금해제 정보 업데이트?
                return;
            }
        }

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

    private bool IsUnlocked()
    {
        switch (gateData.unlockType)
        {
            case BoardGateData.UnlockType.None:
                return true;
                //추후 조건 구현 필요
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
            Debug.LogWarning("[UpdateLockIcon] lockIcon이 연결되지 않았습니다!");
            return;
        }

        lockIcon.SetActive(gateData.isLocked);
    }
}

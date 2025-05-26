public class BoardGateData
{
    public string boardKey; // 출발 보드
    public Direction direction; // 방향
    public string targetBoardKey; // 도착 보드

    public bool isLocked; // 잠금 여부
    public UnlockType unlockType; // 잠금 해제 조건 타입
    public string unlockParam; // 잠금 해제 조건 파라미터
    public int unlockParamValue;

    public enum Direction
    {
        Top,
        Right,
        Bottom,
        Left
    }

    public enum UnlockType
    {
        None,   // 잠금 아님
        Item,   // 특정 아이템 필요
        Level,  // 특정 레벨 필요
        Quest, // 퀘스트 완료 필요
        Resource // 자원 필요
    }

    public string GetUniqueID()
    {
        return $"{boardKey}_{direction}";
    }
}

public class BoardGateData
{
    public string boardKey; // ��� ����
    public Direction direction; // ����
    public string targetBoardKey; // ���� ����

    public bool isLocked; // ��� ����
    public UnlockType unlockType; // ��� ���� ���� Ÿ��
    public string unlockParam; // ��� ���� ���� �Ķ����

    public enum Direction
    {
        Top,
        Right,
        Bottom,
        Left
    }

    public enum UnlockType
    {
        None,   // ��� �ƴ�
        Item,   // Ư�� ������ �ʿ�
        Level,  // Ư�� ���� �ʿ�
        Quest   // ����Ʈ �Ϸ� �ʿ�
    }

    public string GetUniqueID()
    {
        return $"{boardKey}_{direction}";
    }
}

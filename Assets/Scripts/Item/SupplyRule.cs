public class SupplyRule
{
    public string key;
    public string note;
    public string suppliedItem; // ���� �Ǵ� ������ (�����)
    public string receiverItem; // ���� ��� ������ (����)
    public ResultType resultType;
    public string resultItem;
    public int resultValue;

    public SupplyRule(string key, string note, string suppliedItem, string receiverItem, ResultType resultType, string resultItem, int resultValue)
    {
        this.key = key;
        this.note = note;
        this.suppliedItem = suppliedItem;
        this.receiverItem = receiverItem;
        this.resultType = resultType;
        this.resultItem = resultItem;
        this.resultValue = resultValue;
    }

    public bool Matches(string a, string b)
    {
        return a == receiverItem && b == suppliedItem;
    }

    public enum ResultType
    {
        Item,
        Gold,
        Energy,
        Wood,
        // �ʿ� �� Ȯ��
    }
}

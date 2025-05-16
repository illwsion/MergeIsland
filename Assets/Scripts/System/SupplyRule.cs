public class SupplyRule
{
    public int id;
    public string note;
    public int receiverItem; // 공급 대상 아이템 (남음)
    public int suppliedItem; // 공급 되는 아이템 (사라짐)
    public ResultType resultType;
    public int resultItem;
    public int resultValue;

    public SupplyRule(int id, string note, int receiverItem, int suppliedItem, ResultType resultType, int resultItem, int resultValue)
    {
        this.id = id;
        this.note = note;
        this.receiverItem = receiverItem;
        this.suppliedItem = suppliedItem;
        this.resultType = resultType;
        this.resultItem = resultItem;
        this.resultValue = resultValue;
    }

    public bool Matches(int a, int b)
    {
        return a == receiverItem && b == suppliedItem;
    }

    public enum ResultType
    {
        Item,
        Gold,
        Energy,
        Wood,
        // 필요 시 확장
    }
}

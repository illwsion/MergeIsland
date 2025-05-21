public class SupplyRule
{
    public string key;
    public string note;
    public string suppliedItem; // 공급 되는 아이템 (사라짐)
    public string receiverItem; // 공급 대상 아이템 (남음)
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
        // 필요 시 확장
    }
}

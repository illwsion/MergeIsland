// MergeRule.cs
public class MergeRule
{
    public string key;
    public string note;
    public string suppliedItem;
    public string receiverItem;
    public string resultItem;
    public bool allowSwap;

    public MergeRule(string key, string note, string suppliedItem, string receiverItem, string resultItem, bool allowSwap)
    {
        this.key = key;
        this.note = note;
        this.suppliedItem = suppliedItem;
        this.receiverItem = receiverItem;
        this.resultItem = resultItem;
        this.allowSwap = allowSwap;
    }

    public bool Matches(string a, string b)
    {
        if (allowSwap)
            return (a == suppliedItem && b == receiverItem) || (a == receiverItem && b == suppliedItem);
        else
            return a == suppliedItem && b == receiverItem;
    }
}

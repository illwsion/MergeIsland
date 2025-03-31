// MergeRule.cs
public class MergeRule
{
    public int id;
    public string note;
    public int itemA;
    public int itemB;
    public int resultItem;
    public bool allowSwap;

    public MergeRule(int id, string note, int itemA, int itemB, int resultItem, bool allowSwap)
    {
        this.id = id;
        this.note = note;
        this.itemA = itemA;
        this.itemB = itemB;
        this.resultItem = resultItem;
        this.allowSwap = allowSwap;
    }

    public bool Matches(int a, int b)
    {
        if (allowSwap)
            return (a == itemA && b == itemB) || (a == itemB && b == itemA);
        else
            return a == itemA && b == itemB;
    }
}

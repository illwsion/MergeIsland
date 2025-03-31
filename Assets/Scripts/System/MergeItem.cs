// MergeItem.cs
using static ItemData;

public class MergeItem
{
    public int id;

    public ItemData Data => ItemDataManager.Instance.GetItemData(id);

    public string name => Data?.name;
    public string type => Data?.type;
    public int level => Data?.level ?? -1;
    public ItemData.Category Category => Data?.category ?? ItemData.Category.Production;
    public ItemData.ProduceType ProduceType => Data?.produceType ?? ItemData.ProduceType.None;

    /*
    public bool CanMergeWith(MergeItem other)
    {
        if (other == null || this.Data == null || other.Data == null)
            return false;

        return MergeRuleManager.Instance.GetMergeResult(this.id, other.id).HasValue;
    }
    */
    public MergeItem(int id)
    {
        this.id = id;
    }
}
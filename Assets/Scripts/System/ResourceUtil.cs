// ResourceUtil.cs
public static class ResourceUtil
{
    public static ResourceType ToResourceType(this ItemData.CostResource cost)
    {
        return cost switch
        {
            ItemData.CostResource.Energy => ResourceType.Energy,
            ItemData.CostResource.Gold => ResourceType.Gold,
            ItemData.CostResource.Wood => ResourceType.Wood,
            _ => ResourceType.None
        };
    }

    public static ResourceType ToResourceType(this ItemData.GatherResource gather)
    {
        return gather switch
        {
            ItemData.GatherResource.Energy => ResourceType.Energy,
            ItemData.GatherResource.Gold => ResourceType.Gold,
            ItemData.GatherResource.Wood => ResourceType.Wood,
            _ => ResourceType.None
        };
    }
}

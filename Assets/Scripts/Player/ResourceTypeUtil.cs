using UnityEngine;

public enum ResourceCategory
{
    Currency,
    Material,
    Consumable,
    Special
}

public static class ResourceTypeUtil
{
    public static ResourceCategory GetCategory(ResourceType type)
    {
        switch (type)
        {
            case ResourceType.Gold:
            case ResourceType.Gem:
                return ResourceCategory.Currency;

            case ResourceType.Wood:
            case ResourceType.Stone:
            case ResourceType.Iron:
                return ResourceCategory.Material;

            case ResourceType.Energy:
                return ResourceCategory.Consumable;

            case ResourceType.Exp:
                return ResourceCategory.Special;

            default:
                return ResourceCategory.Material;
        }
    }

    public static ResourceType StringToResourceType(string type)
    {
        switch (type)
        {
            case "Gold": return ResourceType.Gold;
            case "Gem": return ResourceType.Gem;
            case "Energy": return ResourceType.Energy;
            case "Wood": return ResourceType.Wood;
            case "Stone": return ResourceType.Stone;
            case "Iron": return ResourceType.Iron;
            case "Exp": return ResourceType.Exp;
            default: return ResourceType.None;

        }
    }
}
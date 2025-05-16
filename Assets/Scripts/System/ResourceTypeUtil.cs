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
            case ResourceType.Food:
            case ResourceType.Water:
                return ResourceCategory.Consumable;

            case ResourceType.Ticket:
            case ResourceType.Exp:
                return ResourceCategory.Special;

            default:
                return ResourceCategory.Material;
        }
    }
}

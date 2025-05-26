// GlobalGameConfig.cs

using System.Collections.Generic;

public static class GlobalGameConfig
{
    public static readonly Dictionary<ResourceType, int> BaseResourceCap = new()
    {
        { ResourceType.Energy, 100 },
        { ResourceType.Wood, 100 },
        { ResourceType.Stone, 100 },
        { ResourceType.Iron, 100 },
    };

    public const float EnergyRecoveryInterval = 120f;
    public const int EnergyRecoveryAmount = 1;
}

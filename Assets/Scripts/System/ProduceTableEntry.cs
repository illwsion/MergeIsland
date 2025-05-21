// ProduceTableEntry.cs

using System.Collections.Generic;
using static NUnit.Framework.Internal.OSPlatform;

[System.Serializable]
public class ProduceTableEntry
{
    public string key;
    public List<ProduceResult> results;
}

[System.Serializable]
public class ProduceResult
{
    public string itemKey;
    public int probability; // 0~1000 ¹üÀ§ (ex. 500 = 50%)
}
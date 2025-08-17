// DropTableEntry.cs

using System.Collections.Generic;
using static NUnit.Framework.Internal.OSPlatform;

[System.Serializable]
public class DropTableEntry
{
    public string key;
    public List<DropResult> results;
}

[System.Serializable]
public class DropResult
{
    public string itemKey;
    public int probability; // 0~1000  (ex. 500 = 50%)
}
// DropTableEntry.cs

using System.Collections.Generic;
using static NUnit.Framework.Internal.OSPlatform;

[System.Serializable]
public class DropTableEntry
{
    public int id;
    public List<DropResult> results;
}

[System.Serializable]
public class DropResult
{
    public int itemID;
    public int probability; // 0~1000 ���� (ex. 500 = 50%)
}
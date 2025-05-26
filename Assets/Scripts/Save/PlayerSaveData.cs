// PlayerSaveData.cs
using System;
using System.Collections.Generic;

[Serializable]
public class ResourceEntry
{
    public string type;
    public int amount;
}

[Serializable]
public class PlayerSaveData
{
    public int currentLevel;
    public int currentExp;
    public int skillPoints;

    public float recoveryTimerSeconds;

    public List<ResourceEntry> resourceAmounts = new();

    public Dictionary<string, int> learnedSkills = new();
}

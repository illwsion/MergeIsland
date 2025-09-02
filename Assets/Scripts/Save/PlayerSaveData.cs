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
    public int currentLevel = 1;
    public int currentExp = 0;
    public int skillPoints = 0;

    public float recoveryTimerSeconds = 0f;

    public List<ResourceEntry> resourceAmounts = new();

    public Dictionary<string, int> learnedSkills = new();
}

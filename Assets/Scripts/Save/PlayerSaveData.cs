// PlayerSaveData.cs
using System;
using System.Collections.Generic;

[Serializable]
public class PlayerSaveData
{
    public int currentLevel;
    public int currentExp;
    public int skillPoints;

    public Dictionary<string, int> learnedSkills = new Dictionary<string, int>();
}

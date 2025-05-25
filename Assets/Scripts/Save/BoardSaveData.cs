using System.Collections.Generic;

[System.Serializable]
public class BoardSaveData
{
    public string boardKey;
    public List<SavedItemEntry> items = new();
}

[System.Serializable]
public class SavedItemEntry
{
    public string itemKey;
    public int x;
    public int y;
    public int currentStorage;
    public float recoveryTimer;
    public int currentHP;
}

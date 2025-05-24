// BoardGate.cs
using System;

public enum UnlockType
{
    None,
    Level,
    Item,
    Quest,
    Flag
}

[Serializable]
public class BoardGate
{
    public string fromBoard;
    public string toBoard;
    public string direction;       // "Top", "Right", etc.
    public bool isLocked;
    public UnlockType unlockType;
    public string unlockParam;
}

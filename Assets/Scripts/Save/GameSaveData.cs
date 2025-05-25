using System;
using System.Collections.Generic;

[System.Serializable]
public class GameSaveData
{
    public string lastSaveTime;
    public PlayerSaveData player = new PlayerSaveData();

    public List<BoardSaveData> boardList = new();
    public List<string> visitedBoards = new();

    // 저장 후 내부에서 조립용
    [NonSerialized] public Dictionary<string, BoardSaveData> boards = new();
}

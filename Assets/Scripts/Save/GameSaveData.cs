using System;
using System.Collections.Generic;

[System.Serializable]
public class GameSaveData
{
    public string lastSaveTime;
    public PlayerSaveData player = new PlayerSaveData();

    public List<BoardSaveData> boardList = new();
    public List<string> visitedBoards = new();
    public List<string> unlockedGates = new();

    // 저장된 보드 데이터
    [NonSerialized] public Dictionary<string, BoardSaveData> boards = new();
}

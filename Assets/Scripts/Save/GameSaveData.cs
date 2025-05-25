using System;
using System.Collections.Generic;

[System.Serializable]
public class GameSaveData
{
    public string lastSaveTime;
    public PlayerSaveData player = new PlayerSaveData();

    public List<BoardSaveData> boardList = new();
    public List<string> visitedBoards = new();

    // ���� �� ���ο��� ������
    [NonSerialized] public Dictionary<string, BoardSaveData> boards = new();
}

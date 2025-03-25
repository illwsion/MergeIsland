// BoardMap.cs
using UnityEngine;

public class BoardMap
{
    public int x;
    public int y;
    public MergeBoard board;

    public BoardMap(int x, int y, MergeBoard board)
    {
        this.x = x;
        this.y = y;
        this.board = board;
    }
}


// MergeBoard.cs
using UnityEngine;

public class MergeBoard
{
    public int width;
    public int height;
    public MergeItem[,] grid;

    public MergeBoard(int w, int h)
    {
        width = w;
        height = h;
        grid = new MergeItem[w, h];
    }

    public void PlaceItem(int x, int y, MergeItem item, bool overwrite = false)
    {
        if (x >= 0 && x < width && y >= 0 && y < height)
        {
            if (grid[x, y] == null || overwrite)
            {
                grid[x, y] = item;
            }
            else
            {
                //Debug.Log("이미 아이템이 있음!");
            }
        }
    }

    public MergeItem GetItem(int x, int y)
    {
        if (x >= 0 && x < width && y >= 0 && y < height)
        {
            return grid[x, y];
        }
        return null;
    }
}

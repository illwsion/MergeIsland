// MergeBoard.cs
using UnityEditorInternal.Profiling.Memory.Experimental;
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
                item.coord = new Vector2Int(x, y);
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

    public Vector2Int? FindNearestEmptyCell(Vector2Int origin)
    {
        int maxDistance = 10; // 검색 범위 제한
        for (int distance = 0; distance <= maxDistance; distance++)
        {
            for (int dx = -distance; dx <= distance; dx++)
            {
                for (int dy = -distance; dy <= distance; dy++)
                {
                    // 외곽만 검사
                    if (Mathf.Abs(dx) != distance && Mathf.Abs(dy) != distance)
                        continue;

                    Vector2Int checkPos = new Vector2Int(origin.x + dx, origin.y + dy);

                    if (IsValidCell(checkPos) && IsCellEmpty(checkPos))
                    {
                        return checkPos;
                    }
                }
            }
        }

        return null; // 빈칸 없음
    }

    public bool IsValidCell(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < width && pos.y >= 0 && pos.y < height;
    }

    public bool IsCellEmpty(Vector2Int pos)
    {
        return grid[pos.x, pos.y] == null;
    }
}

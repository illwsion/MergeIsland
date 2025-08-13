// BoardData.cs
using UnityEngine;

public class BoardData
{
    public string key;
    public Vector2Int worldPos;
    public int width;
    public int height;
    public BoardTheme theme;

    public bool exitTop;
    public bool exitRight;
    public bool exitBottom;
    public bool exitLeft;

    public string nameKey;
    public string descriptionKey;

    public enum BoardTheme
    {
        None,
        Beach, //해변
        Plain, //평원
        Forest // 숲
    };
}

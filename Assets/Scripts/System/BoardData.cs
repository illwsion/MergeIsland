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
        Beach, //ÇØº¯
        Plain, //Æò¿ø
        Forest // ½£
    };
}

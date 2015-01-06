using System.Collections.Generic;
using UnityEngine;

public static class Helper {

    public static bool Between(this float num, float lower, float upper, bool inclusive = false)
    {
        return inclusive
            ? lower <= num && num <= upper
            : lower < num && num < upper;
    }
}
struct TileCol
{
    public float XMin;
    public float XMax;

    public List<Tile> Tiles;
}

struct Tile
{
    public Rect Bounds;
    public Rect TriggerBounds;
	public bool Active;
    public GameObject TileGo;
}

public struct Song
{
    public string Titel;
    public float Bpm;
    public bool DoubleTiles;
    public List<int> Tileset;
    public string SoundFilePath;
    public List<string> TileSoundFilePaths;
}

public enum Highlighttype
{
    None,
    Preview,
    Time,
    Hit
}

public enum Gamemode
{
    Challenge,
    Freestyle,
    InviteLoop
}

public enum Songtype
{
    Freestyle,
    Challenge
}
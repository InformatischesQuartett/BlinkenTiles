using System.Collections.Generic;
using UnityEngine;

public static class BlinkHelper {

    public static bool Between(this float num, float lower, float upper, bool inclusive = false)
    {
        return inclusive
            ? lower <= num && num <= upper
            : lower < num && num < upper;
    }
}
struct TileCol
{
    public List<int> ChallengeIndex;
    public List<Tile> Tiles;
}

struct Tile
{
    public TileCol parentCol;
	public bool Active;
    public GameObject TileGo;
    public int soundIndex;
}

public struct Song
{
    public string Titel;
    public float Bpm;
    public List<int> Tileset;
    public string SoundFilePath;
    public List<string> TileSoundFilePaths;
    public List<string> TileFailSoundFilePaths;
    public int[] LightColor;
}

struct ConfigSet
{
    public float CameraOffsetX;
    public float CameraOffsetY;
    public int Cols;
    public int Rows;
    public float TileWidth;
    public float TileHeight;
    public float TileSpaceing;
    public float BPM;
    public int[] LightColor;
    public int MinDepth;
    public int MaxDepth;
    public int MinThreshold;
    public int MaxThreshold;
	public int RenderImageType;
    public float GridLocX;
    public float GridLocY;
    public float FieldSizeX;
    public float FieldSizeY;
    public float FieldToleranceX;
    public float FieldToleranceY; 
    public float IdleDelay;
    public float[] ColorDefault;
    public float[] ColorHit;
    public float[] ColorTime;
    public float[] ColorOccupied;
    public float[] ColorPreview;
    public float[] ColorFail;
}

public struct NetworkSet
{
    public bool ChallengeMode
}

public enum Highlighttype
{
    None,
    Occupied,
    Preview,
    Time,
    Hit,
    Fail
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

public enum AudioPlayMode
{
    Loop,
    Once
}

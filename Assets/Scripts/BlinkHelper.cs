using System.Collections.Generic;
using UnityEngine;

public static class BlinkHelper
{
    public static bool Between(this float num, float lower, float upper, bool inclusive = false)
    {
        return inclusive
            ? lower <= num && num <= upper
            : lower < num && num < upper;
    }
}

internal struct TileCol
{
    public List<int> ChallengeIndex;
    public List<Tile> Tiles;
}

internal struct Tile
{
    public bool Active;
    public GameObject TileGo;
    public TileCol parentCol;
    public int soundIndex;
}

public struct Song
{
    public float Bpm;
    public int[] LightColor;
    public int PreheatDuration;
    public int PreheatShowAt;
    public string SoundFilePath;
    public List<string> TileFailSoundFilePaths;
    public List<string> TileSoundFilePaths;
    public List<int> Tileset;
    public string Titel;
}

internal struct ConfigSet
{
    public float BPM;
    public float CameraOffsetX;
    public float CameraOffsetY;
    public float[] ColorDefault;
    public float[] ColorFail;
    public float[] ColorHit;
    public float[] ColorOccupied;
    public float[] ColorPreview;
    public float[] ColorTime;
    public int Cols;
    public float FieldSizeX;
    public float FieldSizeY;
    public float FieldToleranceX;
    public float FieldToleranceY;
    public float GridLocX;
    public float GridLocY;
    public float IdleDelay;
    public float IdleResetDelay;
    public int ImageFileUpdate;
    public int[] LightColor;
    public int MaxDepth;
    public int MaxThreshold;
    public int MinDepth;
    public int MinThreshold;
    public int RenderImageType;
    public int Rows;
    public float TileHeight;
    public float TileSpaceing;
    public float TileWidth;
}


public struct NetworkSet
{
    public bool ChallengeMode;
    public float DemoTime;
    public NetworkSong Song;

    public struct NetworkSong
    {
        public float Length; //length of the Song in seconds
        public int Points;
        public string Title; //name of the Song
    }
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
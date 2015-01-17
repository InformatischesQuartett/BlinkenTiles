using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using ZedGraph;

public static class Config
{
    private static string _freestylePath = Application.streamingAssetsPath + @"\Songs\Freestyle";
    private static string _challengePath = Application.streamingAssetsPath + @"\Songs\Challenge";
    private static string _configPath = Application.streamingAssetsPath;

    public static string ShaderType { get; private set; }

    public static Vector3 CamPosition { get; private set; }
    public static int Cols { get; private set; }
    public static int Rows { get; private set; }
    public static float TileWidth { get; private set; }
    public static float TileHeight { get; private set; }
    public static float TileSpaceing { get; private set; }
    public static float BPM { get; set; }
    public static Gamemode CurrentGamemode { get; set; }

    public static List<Song> FreestyleSongs { get; private set; }
    public static List<Song> ChallengeSongs { get; private set; }

    public static Color ColorDefault { get; private set; }
    public static Color ColorTime { get; private set; }
    public static Color ColorHit { get; private set; }
    public static Color ColorPreview { get; private set; }
    public static Color ColorOccupied { get; private set; }
    public static Color ColorFail { get; private set; }

  //  public static int FreestyleAmmount { }

    public static byte[] LightColor { get; set; }

    // blob detection
    public static int MinDepth { get; set; }
    public static int MaxDepth { get; set; }

    public static int MinThreshold { get; set; }
    public static int MaxThreshold { get; set; }

	public static int RenderImageType { get; set; }

    public static Vector2 GridLoc { get; set; }
    public static Vector2 FieldSize { get; set; }
    public static Vector2 FieldTolerance { get; set; }

	public static bool MatrixEmpty { get; set; }


    static Config()
    {
        string configContent = File.ReadAllText(_configPath + "/config.json");
        var conf = JsonConvert.DeserializeObject<ConfigSet>(configContent);

        CamPosition = new Vector3(conf.CameraOffsetX, conf.CameraOffsetY, -10);

        ShaderType = @"Self-Illumin/Diffuse";

        Cols = conf.Cols;
        Rows = conf.Rows;

        TileWidth = conf.TileWidth;
        TileHeight = conf.TileHeight;
        TileSpaceing = conf.TileSpaceing;

        LightColor =  new byte[4];
        LightColor[0] = (byte) conf.LightColor[0];
        LightColor[1] = (byte) conf.LightColor[1];
        LightColor[2] = (byte) conf.LightColor[2];
        LightColor[3] = (byte) conf.LightColor[3];

        BPM = conf.BPM;

        // blob detection
        MinDepth = conf.MinDepth;
        MaxDepth = conf.MaxDepth;

        MinThreshold = conf.MinThreshold;
        MaxThreshold = conf.MaxThreshold;

		RenderImageType = conf.RenderImageType;

        GridLoc = new Vector2(conf.GridLocX, conf.GridLocY);
        FieldSize = new Vector2(conf.FieldSizeX, conf.FieldSizeY);
        FieldTolerance = new Vector2(conf.FieldToleranceX, conf.FieldToleranceY);

        // prepare game
        CurrentGamemode = Gamemode.Freestyle;

        ColorDefault = new Color(conf.ColorDefault[0], conf.ColorDefault[1], conf.ColorDefault[2]);
        ColorHit = new Color(conf.ColorHit[0], conf.ColorHit[1], conf.ColorHit[2]);
        ColorTime = new Color(conf.ColorTime[0], conf.ColorTime[1], conf.ColorTime[2]);
        ColorOccupied = new Color(conf.ColorOccupied[0], conf.ColorOccupied[1], conf.ColorOccupied[2]);
        ColorPreview = new Color(conf.ColorPreview[0], conf.ColorPreview[1], conf.ColorPreview[2]);
        ColorFail = new Color(conf.ColorFail[0], conf.ColorFail[1], conf.ColorFail[2]);
 
        FreestyleSongs = new List<Song>();
        ChallengeSongs = new List<Song>();

        foreach (var file in Directory.GetFiles(_freestylePath))
        {
            if (file.EndsWith(".json"))
            {
                string filecontent = File.ReadAllText(file);
                var song = JsonConvert.DeserializeObject<Song>(filecontent);
                FreestyleSongs.Add(song);
            }
        }

        foreach (var file in Directory.GetFiles(_challengePath))
        {
            if (file.EndsWith(".json"))
            {
                string filecontent = File.ReadAllText(file);
                var song = JsonConvert.DeserializeObject<Song>(filecontent);
                ChallengeSongs.Add(song);
            }
        }
    }
}

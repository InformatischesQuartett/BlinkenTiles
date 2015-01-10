using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;

public static class Config
{
    private static string _freestylePath = Application.streamingAssetsPath + @"\Songs\Freestyle";
    private static string _challengePath = Application.streamingAssetsPath + @"\Songs\Challenge";
    private static string _configPath = Application.streamingAssetsPath;

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

    public static Material MaterialWhite { get; private set; }
    public static Material MaterialYellow { get; private set; }
    public static Material MaterialRed { get; private set; }
    public static Material MaterialBlue { get; private set; }
    public static Material MaterialGreen { get; private set; }
    public static Material MaterialOrange { get; private set; }

    public static byte[] LightColor { get; set; }


    static Config()
    {
        string configcontent = File.ReadAllText(_configPath + "/config.json");
        var conf = JsonConvert.DeserializeObject<ConfigSet>(configcontent);

        CamPosition = new Vector3(conf.CameraOffsetX, conf.CameraOffsetY, -10);

        Cols = conf.Cols;
        Rows = conf.Rows;

        TileWidth = conf.TileWidth;
        TileHeight = conf.TileHeight;
        TileSpaceing = conf.TileSpaceing;

        LightColor =  new byte[4];
        for(int i = 0; i < conf.LightColor.Length; i++)
        {
            LightColor[i] = (byte) conf.LightColor[i];
        }
        
        BPM = conf.BPM;

        CurrentGamemode = Gamemode.Freestyle;

        MaterialWhite = new Material(Shader.Find("Sprites/Default"));
        MaterialWhite.SetColor("_Color", new Color(1, 1, 1));
        MaterialYellow = new Material(Shader.Find("Sprites/Default"));
        MaterialYellow.SetColor("_Color", new Color(1, 1, 0));
        MaterialRed = new Material(Shader.Find("Sprites/Default"));
        MaterialRed.SetColor("_Color", new Color(1, 0, 0));
        MaterialBlue = new Material(Shader.Find("Sprites/Default"));
        MaterialBlue.SetColor("_Color", new Color(0, 0, 1));
        MaterialGreen = new Material(Shader.Find("Sprites/Default"));
        MaterialGreen.SetColor("_Color", new Color(0, 1, 0));
        MaterialOrange = new Material(Shader.Find("Sprites/Default"));
        MaterialOrange.SetColor("_Color", new Color(1, 0.5f, 0,5f));

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

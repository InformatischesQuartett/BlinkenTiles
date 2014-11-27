using System.Collections;
using UnityEngine;

public static class Config
{
    public static int Cols { get; private set; }
    public static int Rows { get; private set; }
    public static float TileWidth { get; private set; }
    public static float TileHeight { get; private set; }
    public static float TileSpaceing { get; private set; }

    public static Material MaterialWhite { get; private set; }
    public static Material MaterialYellow { get; private set; }
    public static Material MaterialRed { get; private set; }
    public static Material MaterialBlue { get; private set; }
    public static Material MaterialGreen { get; private set; }
    public static Material MaterialOrange { get; private set; }
    public static ArrayList Sound { get; private set; }

    static Config()
    {
        Cols = 8;
        Rows = 4;
        TileWidth = 270;
        TileHeight = 90;
        TileSpaceing = 10;

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

        /*
         * Sound[0] = GameObject.Find("Sound1");
        Sound[1] = GameObject.Find("Sound2");
        Sound[2] = GameObject.Find("Sound3");
        Sound[3] = GameObject.Find("Sound4");
        */
    }
}

using System;
using System.Threading;
using UnityEngine;

public delegate void RenderDataCallback(byte[] data);

public class BlobDetectionSettings
{
    public int RenderImgType { get; set; }

    public int MinDepth { get; set; }
    public int MaxDepth { get; set; }

    public int MinThreshold { get; set; }
    public int MaxThreshold { get; set; }

    public Vector2 GridSize { get; set; }
    public Vector2 GridLoc { get; set; }
    public Vector2 FieldSize { get; set; }
    public Vector2 FieldTolerance { get; set; }
}

public class BlobDetection : MonoBehaviour
{
    private const float FPSUpdateRate = 4.0f;
    public Vector2 FieldSize;
    public Vector2 FieldTolerance;
    public Vector2 GridLoc;
    private BlobDetectionSettings _dSettings;
    private float _deltaTime;
    private KinectManager _kinectManager;

    private byte[] _lastRenderData;
    private Texture2D _lastRenderImage;
    private float _mainFPS;
    private int _mainFrameCount;
    private bool _renderDataUpdate;
    private float _threadFPS;
    private int _threadFrameCount;

    private TileController _tileController;
    private Texture2D _whiteTex;

    private BlobDetectionThread _workerObject;
    private Thread _workerThread;

    // fps calculations

    private void Start()
    {
        _dSettings = new BlobDetectionSettings
        {
            RenderImgType = Config.RenderImageType,
            MinDepth = Config.MinDepth,
            MaxDepth = Config.MaxDepth,
            MinThreshold = Config.MinThreshold,
            MaxThreshold = Config.MaxThreshold,
            GridSize = new Vector2(Config.Cols, Config.Rows),
            GridLoc = Config.GridLoc,
            FieldSize = Config.FieldSize,
            FieldTolerance = Config.FieldTolerance
        };

        _kinectManager = new KinectManager(_dSettings);
        _tileController = GameObject.Find("Tiles").GetComponent<TileController>();

        _workerObject = new BlobDetectionThread(_kinectManager, _tileController,
            _dSettings, SetRenderImage);

        _workerThread = new Thread(_workerObject.ProcessImg);
        _workerThread.Start();

        _kinectManager.WorkerThread = _workerObject;

        // render data
        int imgWidth = _kinectManager.DepthWidth;
        int imgHeight = _kinectManager.DepthHeight;

        _lastRenderImage = new Texture2D(imgWidth, imgHeight, TextureFormat.RGB24, false);
        _lastRenderData = new byte[4*imgWidth*imgHeight];
        _renderDataUpdate = false;

        // fps caluclations
        _deltaTime = 0.0f;

        _mainFrameCount = 0;
        _mainFPS = 0.0f;

        _threadFrameCount = 0;
        _threadFPS = 0.0f;

        // gui
        _whiteTex = new Texture2D(1, 1);
        _whiteTex.SetPixel(0, 0, Color.white);
        _whiteTex.Apply();
    }

    private void Update()
    {
        _kinectManager.Update();

        if (_renderDataUpdate)
        {
            _lastRenderImage.LoadRawTextureData(_lastRenderData);
            _lastRenderImage.Apply();

            _renderDataUpdate = false;
        }

        // fps calculations
        _deltaTime += Time.deltaTime;

        if (_deltaTime > 1.0f/FPSUpdateRate)
        {
            int mainDiff = Time.frameCount - _mainFrameCount;
            _mainFPS = mainDiff/_deltaTime;
            _mainFrameCount = Time.frameCount;

            int threadDiff = _workerObject.GetRunCount() - _threadFrameCount;
            _threadFPS = threadDiff/_deltaTime;
            _threadFrameCount = _workerObject.GetRunCount();

            _deltaTime -= 1.0f/FPSUpdateRate;
        }
    }

    private void SetRenderImage(byte[] data)
    {
        _lastRenderData = data;
        _renderDataUpdate = true;
    }

    private int SettingSlider(int x, int y, string text, int val, int low, int high)
    {
        GUI.BeginGroup(new Rect(x, y, 500, 55));
        GUI.Label(new Rect(0, 0, 200, 25), text + ": " + val);
        val = (int) GUI.HorizontalSlider(new Rect(0, 25, 420, 10), val, low, high);
        if (GUI.Button(new Rect(428, 20, 20, 20), "+")) val = Mathf.Min(high, val + 1);
        if (GUI.Button(new Rect(450, 20, 20, 20), "-")) val = Mathf.Max(low, val - 1);
        GUI.EndGroup();

        return val;
    }

    private void OnGUI()
    {
        int imgHeight = Screen.height;
        int imgWidth = imgHeight*(_kinectManager.DepthWidth/_kinectManager.DepthHeight);

        GUI.DrawTexture(new Rect(0, imgHeight, imgWidth, -imgHeight), _lastRenderImage);

        GUI.skin.label.fontSize = 16;
        GUI.Label(new Rect(5, imgHeight - 40, 200, 20), "RenderImageType " + _dSettings.RenderImgType);
        GUI.Label(new Rect(5, imgHeight - 20, 200, 20), "20.01.2014 / " + DateTime.Now.ToString("HH:mm:ss"));

        GUI.Label(new Rect(5, 0, 250, 25), "Performance: " + _mainFPS.ToString("F1") +
                                           " fps / " + _threadFPS.ToString("F1") + " fps");

        GUI.Label(new Rect(5, 25, 250, 25), "Song: " + _tileController.GetSongTitle());

        GUI.backgroundColor = new Color(1, 1, 1, 0.4f);
        GUI.skin.box.normal.background = _whiteTex;
        GUI.skin.box.fontSize = 16;

        GUI.BeginGroup(new Rect(Screen.width/2 - 600, 50, 500, Screen.height - 100));
        GUI.Box(new Rect(0, 0, 500, Screen.height - 150), "Blob Detection Settings");

        GUI.backgroundColor = new Color(0, 0, 0, 1);

        _dSettings.RenderImgType = SettingSlider(15, 80, "RenderImageType", _dSettings.RenderImgType, 1, 6);
        _dSettings.MinDepth = SettingSlider(15, 180, "Minimum Depth", _dSettings.MinDepth, 0, 8000);
        _dSettings.MaxDepth = SettingSlider(15, 240, "Maximum Depth", _dSettings.MaxDepth, 0, 8000);
        _dSettings.MinThreshold = SettingSlider(15, 300, "Minimum Threshold", _dSettings.MinThreshold, 0, 255);
        _dSettings.MaxThreshold = SettingSlider(15, 360, "Maximum Threshold", _dSettings.MaxThreshold, 0, 255);

        int gridLocX = SettingSlider(15, 460, "Grid Location (X)", (int) _dSettings.GridLoc.x, 0, 200);
        int gridLocY = SettingSlider(15, 520, "Grid Location (Y)", (int) _dSettings.GridLoc.y, 0, 200);
        _dSettings.GridLoc = new Vector2(gridLocX, gridLocY);

        int fieldSizeX = SettingSlider(15, 620, "Field Size (X)", (int) _dSettings.FieldSize.x, 0, 200);
        int fieldSizeY = SettingSlider(15, 680, "Field Size (Y)", (int) _dSettings.FieldSize.y, 0, 200);
        _dSettings.FieldSize = new Vector2(fieldSizeX, fieldSizeY);

        int fieldTolX = SettingSlider(15, 780, "Field Tolerance (X)", (int) _dSettings.FieldTolerance.x, 0, 200);
        int fieldTolY = SettingSlider(15, 840, "Field Tolerance (Y)", (int) _dSettings.FieldTolerance.y, 0, 200);
        _dSettings.FieldTolerance = new Vector2(fieldTolX, fieldTolY);

        GUI.skin.label.fontSize = 12;
        GUI.EndGroup();

        /* if (GUI.Button(new Rect(0, 0, 100, 100), "Save"))
			_kinectManager.SaveDepthToFile(); */
    }

    private void OnApplicationQuit()
    {
        _workerObject.RequestStop();
        _workerThread.Join();

        _kinectManager.OnApplicationQuit();
    }
}
using System;
using UnityEngine;
using System.Threading;
using Emgu.CV.CvEnum;
using UnityEditor;

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
    public Vector2 GridLoc;
    public Vector2 FieldSize;
    public Vector2 FieldTolerance;

    private byte[] _lastRenderData;
    private Texture2D _lastRenderImage;
    private bool _renderDataUpdate;

    private KinectManager _kinectManager;
    private TileController _tileController;
    private BlobDetectionSettings _dSettings;

    private BlobDetectionThread _workerObject;
    private Thread _workerThread;

    // fps calculations
    private const float FPSUpdateRate = 4.0f;
    private float _deltaTime;

    private int _mainFrameCount;
    private float _mainFPS;

    private int _threadFrameCount;
    private float _threadFPS;

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
        var imgWidth = _kinectManager.DepthWidth;
        var imgHeight = _kinectManager.DepthHeight;

        _lastRenderImage = new Texture2D(imgWidth, imgHeight, TextureFormat.RGB24,false);
        _lastRenderData = new byte[4 * imgWidth * imgHeight];
        _renderDataUpdate = false;

        // fps caluclations
        _deltaTime = 0.0f;

        _mainFrameCount = 0;
        _mainFPS = 0.0f;

        _threadFrameCount = 0;
        _threadFPS = 0.0f;
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
            var mainDiff = Time.frameCount - _mainFrameCount;
            _mainFPS = mainDiff / _deltaTime;
            _mainFrameCount = Time.frameCount;

            var threadDiff = _workerObject.GetRunCount() - _threadFrameCount;
            _threadFPS = threadDiff / _deltaTime;
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
        GUI.BeginGroup(new Rect(x, y, 250, 40));
        GUI.Label(new Rect(0, 0, 200, 20), text + ": " + val);
        val = (int)GUI.HorizontalSlider(new Rect(0, 20, 185, 10), val, low, high);
        if (GUI.Button(new Rect(193, 15, 20, 20), "+")) val = Mathf.Min(high, val + 1);
        if (GUI.Button(new Rect(215, 15, 20, 20), "-")) val = Mathf.Max(low, val - 1);
        GUI.EndGroup();

        return val;
    }

    private void OnGUI()
    {
        var imgHeight = Screen.height;
        var imgWidth = imgHeight*(_kinectManager.DepthWidth/_kinectManager.DepthHeight);

        GUI.DrawTexture(new Rect(0, imgHeight, imgWidth, -imgHeight), _lastRenderImage);
        GUI.Label(new Rect(5, imgHeight - 40, 200, 20), "RenderImageType " + _dSettings.RenderImgType);
        GUI.Label(new Rect(5, imgHeight - 20, 200, 20), "20.01.2014 / " + DateTime.Now.ToString("HH:mm:ss"));

        GUI.Label(new Rect(5, 0, 250, 20), "Performance: " + _mainFPS.ToString("F1") +
                                           " fps / " + _threadFPS.ToString("F1") + " fps");

        GUI.backgroundColor = new Color(1, 1, 1, 0.4f);
        GUI.skin.box.normal.background = EditorGUIUtility.whiteTexture;

        GUI.BeginGroup(new Rect(imgWidth + 100, 20, 265, 500));
        GUI.Box(new Rect(0, 0, 265, 500), "Blob Detection Settings");

        GUI.backgroundColor = new Color(0, 0, 0, 1);

        _dSettings.RenderImgType = SettingSlider(15, 40, "RenderImageType", _dSettings.RenderImgType, 1, 6);
        _dSettings.MinDepth = SettingSlider(15, 100, "Minimum Depth", _dSettings.MinDepth, 0, 8000);
        _dSettings.MaxDepth = SettingSlider(15, 140, "Maximum Depth", _dSettings.MaxDepth, 0, 8000);
        _dSettings.MinThreshold = SettingSlider(15, 180, "Minimum Threshold", _dSettings.MinThreshold, 0, 255);
        _dSettings.MaxThreshold = SettingSlider(15, 220, "Maximum Threshold", _dSettings.MaxThreshold, 0, 255);

        var gridLocX = SettingSlider(15, 280, "Grid Location (X)", (int) _dSettings.GridLoc.x, 0, 200);
        var gridLocY = SettingSlider(15, 320, "Grid Location (Y)", (int) _dSettings.GridLoc.y, 0, 200);
        _dSettings.GridLoc = new Vector2(gridLocX, gridLocY);

        var fieldSizeX = SettingSlider(15, 360, "Field Size (X)", (int)_dSettings.FieldSize.x, 0, 200);
        var fieldSizeY = SettingSlider(15, 400, "Field Size (Y)", (int)_dSettings.FieldSize.y, 0, 200);
        _dSettings.FieldSize = new Vector2(fieldSizeX, fieldSizeY);

        var fieldTolX = SettingSlider(15, 440, "Field Tolerance (X)", (int)_dSettings.FieldTolerance.x, 0, 200);
        var fieldTolY = SettingSlider(15, 480, "Field Tolerance (Y)", (int)_dSettings.FieldTolerance.y, 0, 200);
        _dSettings.FieldTolerance = new Vector2(fieldTolX, fieldTolY);

        GUI.EndGroup();
    }

    private void OnApplicationQuit()
    {
        _workerObject.RequestStop();
        _workerThread.Join();

        _kinectManager.OnApplicationQuit();
    }
}
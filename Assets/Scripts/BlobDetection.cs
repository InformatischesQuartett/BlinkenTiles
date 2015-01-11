using UnityEngine;
using System.Threading;

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
    [Range(0, 255)]
    public int MinThr;

    [Range(0, 255)]
    public int MaxThr;

    [Range(0, 7999)]
    public int MinDepth;

    [Range(0, 7999)]
    public int MaxDepth;

    [Range(1, 6)]
    public int DebugImg;

    public Vector2 GridLoc;
    public Vector2 FieldSize;
    public Vector2 FieldTolerance;

    private byte[] _lastRenderData;
    private Texture2D _lastRenderImage;
    private bool _renderDataUpdate;

    private KinectManager _kinectManager;
    private TileController _tileController;
    private BlobDetectionSettings _detectionSettings;

    private BlobDetectionThread _workerObject;
    private Thread _workerThread;

    // fps calculations
    private const float MainFPSUpdateRate = 4.0f;
    private const float ThreadFPSUpdateRate = 1.0f;

    private float _mainDeltaTime;
    private int _mainFrameCount;
    private float _mainFPS;

    private float _threadDeltaTime;
    private int _threadFrameCount;
    private float _threadFPS;

    private void Start()
    {
        _detectionSettings = new BlobDetectionSettings
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

        _kinectManager = new KinectManager(_detectionSettings);
        _tileController = GameObject.Find("Tiles").GetComponent<TileController>();

        _workerObject = new BlobDetectionThread(_kinectManager, _tileController,
            _detectionSettings, SetRenderImage);

        _workerThread = new Thread(_workerObject.ProcessImg);
        _workerThread.Start();

        _kinectManager.WorkerThread = _workerObject;

        // render data
        var imgWidth = _kinectManager.DepthWidth;
        var imgHeight = _kinectManager.DepthHeight;

        _lastRenderImage = new Texture2D(imgWidth, imgHeight, TextureFormat.RGBA32,false);
        _lastRenderData = new byte[4 * imgWidth * imgHeight];
        _renderDataUpdate = false;

        // fps caluclations
        _mainDeltaTime = 0.0f;
        _mainFrameCount = 0;
        _mainFPS = 0.0f;

        _threadDeltaTime = 0.0f;
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
        _mainDeltaTime += Time.deltaTime;

        if (_mainDeltaTime > 1.0f/MainFPSUpdateRate)
        {
            var mainDiff = Time.frameCount - _mainFrameCount;
            _mainFPS = mainDiff / _mainDeltaTime;
            _mainFrameCount = Time.frameCount;

            _mainDeltaTime -= 1.0f/MainFPSUpdateRate;
        }

        _threadDeltaTime += Time.deltaTime;

        if (_threadDeltaTime > 1.0f / ThreadFPSUpdateRate)
        {
            var threadDiff = _workerObject.GetRunCount() - _threadFrameCount;
            _threadFPS = threadDiff / _threadDeltaTime;
            _threadFrameCount = _workerObject.GetRunCount();

            _threadDeltaTime -= 1.0f / ThreadFPSUpdateRate;
        }
    }

    private void SetRenderImage(byte[] data)
    {
        _lastRenderData = data;
        _renderDataUpdate = true;
    }

    private void OnGUI()
    {
        var imgHeight = Screen.height;
        var imgWidth = imgHeight*(_kinectManager.DepthWidth/_kinectManager.DepthHeight);

        GUI.DrawTexture(new Rect(0, imgHeight, imgWidth, -imgHeight), _lastRenderImage);
        GUI.Label(new Rect(5, _lastRenderImage.height - 20, 100, 20), "Schritt " + DebugImg);

        GUI.Label(new Rect(5, 0, 250, 20), "Performance: " + _mainFPS.ToString("F1") +
            " fps / " + _threadFPS.ToString("F1") + " fps");

        //if (GUI.Button(new Rect(0, 0, 100, 20), "Save"))
        //    _kinectManager.SaveDepthToFile();

        //if (GUI.Button(new Rect(220, 0, 100, 20), "Update"))
        //    _workerObject.SetDetectionSettings(_detectionSettings);
    }

    private void OnApplicationQuit()
    {
        _workerObject.RequestStop();
        _workerThread.Join();

        _kinectManager.OnApplicationQuit();
    }
}
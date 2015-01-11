using UnityEngine;
using System.Threading;

public delegate void RenderDataCallback(byte[, ,] data);

public class BlobDetectionSettings
{
    public int RenderImgType { get; set; }
    public Vector2 RenderImgSize { get; set; }

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

    private Color32[] _lastRenderData;
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
        var imgWidth = (int)(Screen.width / 2.0f);
        var imgHeight = (int)(Screen.width / 2.416f);

        _lastRenderImage = new Texture2D(imgWidth, imgHeight);
        _lastRenderData = new Color32[imgWidth * imgHeight];
        _renderDataUpdate = false;

        _detectionSettings = new BlobDetectionSettings
        {
            RenderImgType = Config.RenderImageType,
            RenderImgSize = new Vector2(imgWidth, imgHeight),

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
            _lastRenderImage.SetPixels32(_lastRenderData);
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

    private void SetRenderImage(byte[,,] data)
    {
        lock (_lastRenderData)
        {
            var imgWidth = (int) _detectionSettings.RenderImgSize.x;
            var imgHeight = (int) _detectionSettings.RenderImgSize.y;
           
            var pixct = 0;

            for (int y = imgHeight - 1; y >= 0; y--)
            {
                for (int x = 0; x < imgWidth; x++)
                {
                   
                    if (x > data.GetLength(1) - 1 || y > data.GetLength(0) - 1)
                        _lastRenderData[pixct++] = new Color32(0, 0, 0, 1);
                    else if (data.GetLength(2) == 1)
                        _lastRenderData[pixct++] = new Color32(data[y, x, 0], data[y, x, 0], data[y, x, 0], 255);
                    else
                        _lastRenderData[pixct++] = new Color32(data[y, x, 2], data[y, x, 1], data[y, x, 0], 255);
                }
            }

            _renderDataUpdate = true;
        }
    }

    private void OnGUI()
    {
        GUI.DrawTexture(new Rect(0, 0, _lastRenderImage.width, _lastRenderImage.height), _lastRenderImage);
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
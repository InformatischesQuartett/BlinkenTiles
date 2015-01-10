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

    private void Start()
    {
        var imgWidth = (int)(Screen.width / 2.0f);
        var imgHeight = (int)(Screen.width / 2.416f);

        _lastRenderImage = new Texture2D(imgWidth, imgHeight);
        _lastRenderData = new Color32[imgWidth * imgHeight];
        _renderDataUpdate = false;

        _detectionSettings = new BlobDetectionSettings
        {
            RenderImgType = 5,
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
                    if (x > data.GetLength(1) - 1 || y > data.GetLength(0))
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

        if (GUI.Button(new Rect(0, 0, 100, 20), "Save"))
            _kinectManager.SaveDepthToFile();

        if (GUI.Button(new Rect(220, 0, 100, 20), "Update"))
            _workerObject.SetDetectionSettings(_detectionSettings);
    }

    private void OnApplicationQuit()
    {
        _workerObject.RequestStop();
        _workerThread.Join();

        _kinectManager.OnApplicationQuit();
    }
}
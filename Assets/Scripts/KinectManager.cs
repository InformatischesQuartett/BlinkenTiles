using System;
using System.IO;
using Windows.Kinect;
using UnityEngine;
using Random = System.Random;

public class KinectManager
{
    private const int ColorWidth = 1920;
    private const int ColorHeight = 1080;
    public readonly int DepthHeight = 424;
    public readonly int DepthWidth = 512;
    private readonly byte[] _colorData;
    private readonly ColorSpacePoint[] _colorSpacePoints;
    private readonly bool _sampleMode;

    private ColorFrameReader _colorReader;
    private DepthFrameReader _depthReader;
    private KinectSensor _kinectSensor;

    public KinectManager(BlobDetectionSettings detectionSettings)
    {
        DetectionSettings = detectionSettings;

        if (SystemInfo.operatingSystem.Contains("Windows 8") || SystemInfo.operatingSystem.Contains("Windows 10"))
        {
            _kinectSensor = KinectSensor.GetDefault();
        }
        else
        {
            Debug.LogError("Unsupported Operating System: " + SystemInfo.operatingSystem);
        }

        _sampleMode = (_kinectSensor == null);

        ColorImage = new byte[DepthHeight, DepthWidth, 3];

        if (_kinectSensor != null)
        {
            _depthReader = _kinectSensor.DepthFrameSource.OpenReader();
            _depthReader.FrameArrived += DepthFrameArrived;

            _colorReader = _kinectSensor.ColorFrameSource.OpenReader();
            _colorReader.FrameArrived += ColorFrameArrived;

            // color frame
            FrameDescription colorFrameDesc =
                _kinectSensor.ColorFrameSource.CreateFrameDescription(ColorImageFormat.Rgba);
            _colorData = new byte[colorFrameDesc.BytesPerPixel*colorFrameDesc.LengthInPixels];

            // depth frame
            FrameDescription depthFrameDesc = _kinectSensor.DepthFrameSource.FrameDescription;
            DepthData = new ushort[depthFrameDesc.LengthInPixels];
            _colorSpacePoints = new ColorSpacePoint[depthFrameDesc.LengthInPixels];

            if (!_kinectSensor.IsOpen)
                _kinectSensor.Open();
        }
        else
            ReadDepthFromFile();
    }

    public BlobDetectionSettings DetectionSettings { get; set; }
    public BlobDetectionThread WorkerThread { get; set; }

    public ushort[] DepthData { get; private set; }

    public byte[,,] ColorImage { get; private set; }

    public void Update()
    {
        if (_sampleMode)
        {
            if (Time.frameCount%600 == 0)
                ReadDepthFromFile();

            if (!WorkerThread.GetUpdatedData())
                WorkerThread.SetUpdatedData();
        }
    }

    public void DepthFrameArrived(object sender, DepthFrameArrivedEventArgs evt)
    {
        if (WorkerThread.GetUpdatedData())
            return;

        using (DepthFrame depthFrame = evt.FrameReference.AcquireFrame())
        {
            if (depthFrame == null) return;

            depthFrame.CopyFrameDataToArray(DepthData);
            WorkerThread.SetUpdatedData();
        }
    }

    public void ColorFrameArrived(object sender, ColorFrameArrivedEventArgs evt)
    {
        if (WorkerThread.GetUpdatedData())
            return;

        using (ColorFrame colorFrame = evt.FrameReference.AcquireFrame())
        {
            if (colorFrame == null) return;

            colorFrame.CopyConvertedFrameDataToArray(_colorData, ColorImageFormat.Rgba);

            if (DetectionSettings.RenderImgType == 6)
                CreateColorImage();
        }
    }

    private void CreateColorImage()
    {
        _kinectSensor.CoordinateMapper.MapDepthFrameToColorSpace(DepthData, _colorSpacePoints);

        for (int y = 0; y < DepthHeight; y++)
        {
            for (int x = 0; x < DepthWidth; x++)
            {
                int index = y*DepthWidth + x;

                ColorSpacePoint point = _colorSpacePoints[index];
                var colorX = (int) Math.Floor(point.X + 0.5f);
                var colorY = (int) Math.Floor(point.Y + 0.5f);

                bool inWidth = (colorX >= 0) && (colorX < ColorWidth);
                bool inHeight = (colorY >= 0) && (colorY < ColorHeight);
                if (!inWidth || !inHeight) continue;

                int colorIndex = ((ColorWidth*colorY) + colorX)*4;

                ColorImage[y, x, 0] = _colorData[colorIndex + 0];
                ColorImage[y, x, 1] = _colorData[colorIndex + 1];
                ColorImage[y, x, 2] = _colorData[colorIndex + 2];
            }
        }
    }

    public void SaveDepthToFile()
    {
        if (DepthData == null)
            return;

        var randObj = new Random();
        int name = randObj.Next(10000, 99999);
        string path = Application.streamingAssetsPath + "/Samples/DepthSample" + name;
        FileStream file = File.Open(path, FileMode.Create);

        using (var bw = new BinaryWriter(file))
            foreach (ushort value in DepthData)
                bw.Write(value);

        Debug.Log("Depth sample saved to: " + path);
    }

    private void ReadDepthFromFile()
    {
        var randObj = new Random();
        int name = randObj.Next(1, 15);
        string path = Application.streamingAssetsPath + "/Samples/DepthSample" + name;
        FileStream file = File.Open(path, FileMode.Open);

        using (var br = new BinaryReader(file))
        {
            long valueCt = br.BaseStream.Length/sizeof (ushort);
            var readArr = new ushort[valueCt];

            for (int x = 0; x < valueCt; x++)
                readArr[x] = br.ReadUInt16();

            DepthData = readArr;
        }
    }

    public void OnApplicationQuit()
    {
        if (_depthReader != null)
        {
            _depthReader.Dispose();
            _depthReader = null;
        }

        if (_colorReader != null)
        {
            _colorReader.Dispose();
            _colorReader = null;
        }

        if (_kinectSensor != null)
        {
            if (_kinectSensor.IsOpen)
                _kinectSensor.Close();

            _kinectSensor = null;
        }
    }
}
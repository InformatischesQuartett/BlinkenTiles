using System;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using Windows.Kinect;

public class KinectManager
{
    private const int DepthWidth = 512;
    private const int DepthHeight = 424;

    private const int ColorWidth = 1920;
    private const int ColorHeight = 1080;

    private KinectSensor _sensor;
    private MultiSourceFrameReader _reader;

    public BlobDetectionSettings DetectionSettings { get; set; }
    public BlobDetectionThread WorkerThread { get; set; }

    private ushort[] _depthData;
    public byte[, ,] DepthImage { get; private set; }
    public byte[, ,] DepthFilteredImage { get; private set; }

    private byte[] _colorData;
    private Texture2D _colorImage;

    private readonly bool _sampleMode;

    public KinectManager(BlobDetectionSettings detectionSettings)
    {
        DetectionSettings = detectionSettings;

        if (SystemInfo.operatingSystem.Contains("Windows 8"))
            _sensor = KinectSensor.GetDefault();

        _sampleMode = (_sensor == null);

        _colorImage = new Texture2D(ColorWidth, ColorHeight, TextureFormat.RGBA32, false);

        DepthImage = new byte[DepthHeight, DepthWidth, 1];
        DepthFilteredImage = new byte[DepthHeight, DepthWidth, 1];

        if (_sensor != null)
        {
            _reader = _sensor.OpenMultiSourceFrameReader(FrameSourceTypes.Color | FrameSourceTypes.Depth);

            // color frame
            var colorFrameDesc = _sensor.ColorFrameSource.CreateFrameDescription(ColorImageFormat.Rgba);
            _colorData = new byte[colorFrameDesc.BytesPerPixel * colorFrameDesc.LengthInPixels];

            // depth frame
            var depthFrameDesc = _sensor.DepthFrameSource.FrameDescription;
            _depthData = new ushort[depthFrameDesc.LengthInPixels];

            if (!_sensor.IsOpen)
                _sensor.Open();
        }
        else
            ReadDepthFromFile();
    }

    public void Update()
    {
        if (_sampleMode)
        {
            if (Time.frameCount % 600 == 0)
               ReadDepthFromFile();

            if (!WorkerThread.GetUpdatedData())
                WorkerThread.SetUpdatedData();

            return;
        }

        if (_reader == null || WorkerThread.GetUpdatedData())
            return;

        // get latest images
        var frame = _reader.AcquireLatestFrame();
        if (frame == null) return;

        var colorFrame = frame.ColorFrameReference.AcquireFrame();

        if (colorFrame != null)
        {
            colorFrame.CopyConvertedFrameDataToArray(_colorData, ColorImageFormat.Rgba);
            colorFrame.Dispose();
        }

        var depthFrame = frame.DepthFrameReference.AcquireFrame();
        if (depthFrame == null) return;

        depthFrame.CopyFrameDataToArray(_depthData);
        CreateDepthImage(DetectionSettings.MinDepth, DetectionSettings.MaxDepth);

        depthFrame.Dispose();
    }

    public Texture2D GetColorImg()
    {
        _colorImage.LoadRawTextureData(_colorData);
        _colorImage.Apply();

        return _colorImage;
    }

    public void SaveDepthToFile()
    {
        if (_depthData == null)
            return;

        var randObj = new System.Random();
        var name = randObj.Next(10000, 99999);
        var path = "Assets/Samples/DepthSample" + name;
        var file = File.Open(path, FileMode.Create);

        using (var bw = new BinaryWriter(file))
            foreach (var value in _depthData)
                bw.Write(value);

        Debug.Log("Depth sample saved to: " + path);
    }

    private void ReadDepthFromFile()
    {
        var randObj = new System.Random();
        var name = randObj.Next(1, 15);
        var path = "Assets/Samples/DepthSample" + name;
        var file = File.Open(path, FileMode.Open);

        using (var br = new BinaryReader(file))
        {
            var valueCt = br.BaseStream.Length/sizeof (ushort);
            var readArr = new ushort[valueCt];

            for (int x = 0; x < valueCt; x++)
                readArr[x] = br.ReadUInt16();

            _depthData = readArr;
        }

        CreateDepthImage(DetectionSettings.MinDepth, DetectionSettings.MaxDepth);
    }

    private void CreateDepthImage(int low, int high)
    {
        // find min and max value
        var minDepth = Mathf.Infinity;
        var maxDepth = Mathf.NegativeInfinity;

        foreach (var val in _depthData)
        {
            minDepth = Mathf.Min(minDepth, val);
            maxDepth = Mathf.Max(maxDepth, val);
        }

        minDepth = Mathf.Max(minDepth, low);
        maxDepth = Mathf.Min(maxDepth, high);

        var distDepth = maxDepth - minDepth;

        // convert to depth image
        for (int y = 0; y < DepthHeight; y++)
        {
            for (int x = 0; x < DepthWidth; x++)
            {
                var index = y*DepthWidth + x;
                var val = (_depthData[index] - minDepth)/distDepth;

                DepthImage[y, x, 0] = (byte) (val*255);

                if (_depthData[index] < low || _depthData[index] > high)
                    DepthFilteredImage[y, x, 0] = 0;
                else
                    DepthFilteredImage[y, x, 0] = (byte) (val*255);
            }
        }
    }

    private ushort[] ScaleDepthMap(ushort[] arr, float scale)
    {
        var tmpArr = new List<ushort>();

        const float midHeight = DepthHeight/2.0f;
        const float midWidth = DepthWidth/2.0f;

        var startValY = Mathf.FloorToInt(midHeight - midHeight/2.0f);
        var startValX = Mathf.FloorToInt(midWidth - midWidth/2.0f);

        // output array 
        var scArr = new ushort[arr.Length];

        for (int i = 0; i < arr.Length; i++)
            scArr[i] = 0;

        // catch elements
        for (int y = 0; y < DepthHeight; y += 2)
            for (int x = 0; x < DepthWidth; x += 2)
                tmpArr.Add(arr[y*DepthWidth + x]);

        // write elements
        for (int y = startValY; y < DepthHeight - startValY; y++)
        {
            for (int x = startValX; x < DepthWidth - startValX; x++)
            {
                var index = y*DepthWidth + x;

                scArr[index] = tmpArr[0];
                tmpArr.RemoveAt(0);
            }
        }

        return scArr;
    }

    public void OnApplicationQuit()
    {
        if (_reader != null)
        {
            _reader.Dispose();
            _reader = null;
        }

        if (_sensor != null)
        {
            if (_sensor.IsOpen)
                _sensor.Close();

            _sensor = null;
        }
    }
}
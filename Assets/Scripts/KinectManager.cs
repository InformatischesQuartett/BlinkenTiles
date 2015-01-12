using System;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using Windows.Kinect;

public class KinectManager
{
    public readonly int DepthWidth = 512;
    public readonly int DepthHeight = 424;

    private const int ColorWidth = 1920;
    private const int ColorHeight = 1080;

    private KinectSensor _kinectSensor;
    private MultiSourceFrameReader _frameReader;

    public BlobDetectionSettings DetectionSettings { get; set; }
    public BlobDetectionThread WorkerThread { get; set; }

    public ushort[] DepthData { get; private set; }

    private float _lastColorData;
    private readonly byte[] _colorData;
    private readonly ColorSpacePoint[] _colorSpacePoints;
	public byte[, ,] ColorImage { get; private set; }

    private readonly bool _sampleMode;

    public KinectManager(BlobDetectionSettings detectionSettings)
    {
        DetectionSettings = detectionSettings;

        if (SystemInfo.operatingSystem.Contains("Windows 8"))
            _kinectSensor = KinectSensor.GetDefault();

        _sampleMode = (_kinectSensor == null);

		_lastColorData = 0;
        ColorImage = new byte[DepthHeight, DepthWidth, 3];

        if (_kinectSensor != null)
        {
            _frameReader = _kinectSensor.OpenMultiSourceFrameReader(FrameSourceTypes.Color | FrameSourceTypes.Depth);

            // color frame
            var colorFrameDesc = _kinectSensor.ColorFrameSource.CreateFrameDescription(ColorImageFormat.Rgba);
            _colorData = new byte[colorFrameDesc.BytesPerPixel * colorFrameDesc.LengthInPixels];

            // depth frame
            var depthFrameDesc = _kinectSensor.DepthFrameSource.FrameDescription;
            DepthData = new ushort[depthFrameDesc.LengthInPixels];
            _colorSpacePoints = new ColorSpacePoint[depthFrameDesc.LengthInPixels];

            if (!_kinectSensor.IsOpen)
                _kinectSensor.Open();
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

        if (_frameReader == null || WorkerThread.GetUpdatedData())
            return;

		if (DetectionSettings.RenderImgType == 7) {
			if (Time.time - _lastColorData < 1)
				return;
			else
				_lastColorData = Time.time;
		}
		
		// get latest images
        var frame = _frameReader.AcquireLatestFrame();
        if (frame == null) return;

        // color frame
		var colorFrame = frame.ColorFrameReference.AcquireFrame();
		
		if (colorFrame != null)
		{
			colorFrame.CopyConvertedFrameDataToArray(_colorData, ColorImageFormat.Rgba);
			colorFrame.Dispose();
		}

        // depth frame
        var depthFrame = frame.DepthFrameReference.AcquireFrame();
        if (depthFrame == null) return;

        depthFrame.CopyFrameDataToArray(DepthData);
		depthFrame.Dispose();

        if (DetectionSettings.RenderImgType == 7)
            CreateColorImage();

		WorkerThread.SetUpdatedData();
    }

	private void CreateColorImage()
	{
		_kinectSensor.CoordinateMapper.MapDepthFrameToColorSpace(DepthData, _colorSpacePoints);
		
		for (int y = 0; y < DepthHeight; y++)
		{
			for (int x = 0; x < DepthWidth; x++)
			{
				var index = y * DepthWidth + x;
				
				var point = _colorSpacePoints[index];
				var colorX = (int) Math.Floor(point.X + 0.5f);
				var colorY = (int) Math.Floor(point.Y + 0.5f);
				
				var inWidth = (colorX >= 0) && (colorX < ColorWidth);
				var inHeight = (colorY >= 0) && (colorY < ColorHeight);
				if (!inWidth || !inHeight) continue;
				
				int colorIndex = ((ColorWidth * colorY) + colorX) * 4;
				
				ColorImage[y, x, 0] = _colorData[colorIndex+0];
				ColorImage[y, x, 1] = _colorData[colorIndex+1];
				ColorImage[y, x, 2] = _colorData[colorIndex+2];
			}
		}
	}

    public void SaveDepthToFile()
    {
        if (DepthData == null)
            return;

        var randObj = new System.Random();
        var name = randObj.Next(10000, 99999);
        var path = Application.streamingAssetsPath + "Samples/DepthSample" + name;
        var file = File.Open(path, FileMode.Create);

        using (var bw = new BinaryWriter(file))
            foreach (var value in DepthData)
                bw.Write(value);

        Debug.Log("Depth sample saved to: " + path);
    }

    private void ReadDepthFromFile()
    {
        var randObj = new System.Random();
        var name = randObj.Next(1, 15);
        var path = Application.streamingAssetsPath + "/Samples/DepthSample" + name;
        var file = File.Open(path, FileMode.Open);

        using (var br = new BinaryReader(file))
        {
            var valueCt = br.BaseStream.Length / sizeof(ushort);
            var readArr = new ushort[valueCt];

            for (int x = 0; x < valueCt; x++)
                readArr[x] = br.ReadUInt16();

            DepthData = readArr;
        }
    }

    public void OnApplicationQuit()
    {
        if (_frameReader != null)
        {
            _frameReader.Dispose();
            _frameReader = null;
        }

        if (_kinectSensor != null)
        {
            if (_kinectSensor.IsOpen)
                _kinectSensor.Close();

            _kinectSensor = null;
        }
    }
}
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Windows.Kinect;

public class DepthSourceManager : MonoBehaviour
{
	public int ColorWidth { get; private set; }
	public int ColorHeight { get; private set; }

	public int DepthWidth { get; private set; }
	public int DepthHeight { get; private set; }

	private KinectSensor _Sensor;
	private MultiSourceFrameReader _Reader;

	private ushort[] _DepthData;
	private byte[,,] _DepthImage;

	private byte[] _ColorData;
	private Texture2D _ColorImage;

	public Texture2D GetColorImg()
	{
		_ColorImage.LoadRawTextureData(_ColorData);
		_ColorImage.Apply();
		
		return _ColorImage;
	}
	
	public byte[,,] GetDepthImg()
	{
		if (_DepthData != null)
			CreateDepthImage();
		return _DepthImage;
	}

	public void SaveDepthToFile()
	{
		if (_DepthData == null)
			return;

		var randObj = new System.Random();
		var name = randObj.Next(10000, 99999);
		var path = "Assets/Samples/DepthFile" + name;

		using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write))
		{
			using (BinaryWriter bw = new BinaryWriter(fs))
			{
				foreach (short value in _DepthData)
				{
					bw.Write(value);
				}
			}
		}

		Debug.Log("Saved to: " + path);
	}
	
	void Start () 
	{
		_Sensor = KinectSensor.GetDefault();
		
		if (_Sensor != null) 
		{
			_Reader = _Sensor.OpenMultiSourceFrameReader(FrameSourceTypes.Color | FrameSourceTypes.Depth);

			// color frame
			var colorFrameDesc = _Sensor.ColorFrameSource.CreateFrameDescription(ColorImageFormat.Rgba);

			ColorWidth = colorFrameDesc.Width;
			ColorHeight = colorFrameDesc.Height;

			_ColorImage = new Texture2D(ColorWidth, ColorHeight, TextureFormat.RGBA32, false);
			_ColorData = new byte[colorFrameDesc.BytesPerPixel * colorFrameDesc.LengthInPixels];

			// depth frame
			var depthFrameDesc = _Sensor.DepthFrameSource.FrameDescription;

			DepthWidth = depthFrameDesc.Width;
			DepthHeight = depthFrameDesc.Height;

			_DepthImage = new byte[DepthHeight, DepthWidth, 1];
			_DepthData = new ushort[depthFrameDesc.LengthInPixels];
			
			if (!_Sensor.IsOpen)
			{
				_Sensor.Open();
			}
		}
	}

	void Update () 
	{
		if (_Reader != null) 
		{
			var frame = _Reader.AcquireLatestFrame();

			if (frame != null)
			{
				var colorFrame = frame.ColorFrameReference.AcquireFrame();
				if (colorFrame != null)
				{
					var depthFrame = frame.DepthFrameReference.AcquireFrame();

					if (depthFrame != null)
					{
						colorFrame.CopyConvertedFrameDataToArray(_ColorData, ColorImageFormat.Rgba);

						depthFrame.CopyFrameDataToArray(_DepthData);
						depthFrame.Dispose();
						depthFrame = null;
					}
					
					colorFrame.Dispose();
					colorFrame = null;
				}
				
				frame = null;
			}
		}
	}

	private void CreateDepthImage()
	{
		// find min and max value
		var minDepth = Mathf.Infinity;
		var maxDepth = Mathf.NegativeInfinity;

		foreach (var val in _DepthData) {
			minDepth = Mathf.Min(minDepth, val);
			maxDepth = Mathf.Max(maxDepth, val);
		}

		var distDepth = maxDepth - minDepth;

		// convert to depth image
		for (int y = 0; y < DepthHeight; y++) {
			for (int x = 0; x < DepthWidth; x++) {
				var index = y * DepthWidth + x;
				var val = (_DepthData [index] - minDepth) / distDepth;

				_DepthImage[y, x, 0] = (byte) (val * 255);
			}
		}
	}

	private ushort[] ScaleDepthMap(ushort[] arr, float scale)
	{
		var tmpArr = new List<ushort>();

		var midHeight = DepthHeight/2.0f;
		var midWidth = DepthWidth/2.0f;
		
		var startValY = Mathf.FloorToInt(midHeight - midHeight/2.0f);
		var startValX = Mathf.FloorToInt(midWidth - midWidth/2.0f);

		// output array 
		var scArr = new ushort[arr.Length];
		
		for (int i = 0; i < arr.Length; i++)
			scArr[i] = 0;

		// catch elements
		for (int y = 0; y < DepthHeight; y += 2)
			for (int x = 0; x < DepthWidth; x += 2)
				tmpArr.Add(arr[y * DepthWidth + x]);

		// write elements
		for (int y = startValY; y < DepthHeight - startValY; y++)
		{
			for (int x = startValX; x < DepthWidth - startValX; x++)
			{
				var index = y * DepthWidth + x;
			
				scArr[index] = tmpArr[0];
				tmpArr.RemoveAt(0);
			}
		}
			
		return scArr;
	}

	void OnApplicationQuit()
	{
		if (_Reader != null)
		{
			_Reader.Dispose();
			_Reader = null;
		}
		
		if (_Sensor != null)
		{
			if (_Sensor.IsOpen)
			{
				_Sensor.Close();
			}
			
			_Sensor = null;
		}
	}
}

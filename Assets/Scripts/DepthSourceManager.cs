using UnityEngine;
using System.Collections;
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
		CreateDepthImage();
		return _DepthImage;
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

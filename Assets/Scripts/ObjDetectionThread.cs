using UnityEngine;
using System.Threading;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;
using System.Drawing;
using Color = UnityEngine.Color;
using CvEnum = Emgu.CV.CvEnum;

public class ObjDetectionThread {
	private AutoResetEvent autoEvent;

	private volatile int _minThr;
	private volatile int _maxThr;

	private volatile int _minDepth;
	private volatile int _maxDepth;

	private volatile int _debugImg;
	private volatile int _debugImgSize;
	
	private volatile float _gridLocX;
	private volatile float _gridLocY;

	private volatile float _fieldSizeX;
	private volatile float _fieldSizeY;
	
	private volatile int _gridTolerance;
	
	private volatile Texture2D _lastTex;
	private volatile Texture2D _lastColorImg;

	private DepthSourceManager _depthManager;
	private TileController _tileCtrl;

	private int _cols;
	private int _rows;

	private volatile bool _updatedData;

	public ObjDetectionThread(DepthSourceManager depthManager, TileController tileCtrl, int cols, int rows)
	{
		_depthManager = depthManager;
		_tileCtrl = tileCtrl;

		_cols = cols;
		_rows = rows;

		_updatedData = false;
	}

	public void SetUpdatedData()
	{
		_updatedData = true;
	}

	public void GetUpdatedData()
	{
		return _updatedData;
	}

	private byte[,,] ConvertToImage(Texture2D tex) {
		var arr = new byte[tex.width, tex.height, 3];
		var cols = tex.GetPixels32();
		
		var pixct = 0;
		
		for (int y = tex.height - 1; y >= 0; y--)
		{
			for (int x = 0; x < tex.width; x++)
			{
				arr[y, x, 0] = cols[pixct].r;
				arr[y, x, 1] = cols[pixct].g;
				arr[y, x, 2] = cols[pixct].b;
				
				pixct++;
			}
		}
		
		return arr;
	}
	
	private Texture2D ConvertToTexture(byte[,,] img, int width, int height)
	{
		var tex = new Texture2D(width, height);
		var cols = new Color32[width * height];
		
		var pixct = 0;
		
		for (int y = tex.height - 1; y >= 0; y--)
		{
			for (int x = 0; x < tex.width; x++)
			{
				if (img.GetLength(2) == 1)
					cols[pixct] = new Color32(img[y, x, 0], img[y, x, 0], img[y, x, 0], 255);
				else
					cols[pixct] = new Color32(img[y, x, 2], img[y, x, 1], img[y, x, 0], 255);
				
				pixct++;
			}
		}
		
		tex.SetPixels32(cols);
		tex.Apply();
		
		return tex;
	}

	public void ProcessImg () {
		while (true) {
			while (!_updatedData);

			var depthImg = _depthManager.GetDepthImg();
			var depthFilteredImg = _depthManager.GetDepthFilteredImg();

			if (depthImg == null || depthFilteredImg != null)
				return;
			
			var imgGray = new Image<Gray, byte> (depthFilteredImg);

			var imgOrgGray = new Image<Gray, byte> (depthImg);
			var imgOrg = imgOrgGray.Convert<Bgr, byte> ();
			
			if (_debugImg == 1)
				_lastTex = ConvertToTexture (imgGray.Data, imgGray.Width, imgGray.Height);
			
			// noise reduction
			var imgSm = imgGray.PyrDown ().PyrUp ().SmoothGaussian (3);
			
			var element = new StructuringElementEx (5, 5, 2, 2, Emgu.CV.CvEnum.CV_ELEMENT_SHAPE.CV_SHAPE_ELLIPSE);
			CvInvoke.cvErode (imgSm, imgSm, element, 2);
			CvInvoke.cvDilate (imgSm, imgSm, element, 2);
			
			if (_debugImg == 2)
				_lastTex = ConvertToTexture (imgSm.Data, imgSm.Width, imgSm.Height);
			
			// filtering
			var imgThr = imgSm.InRange (new Gray (_minThr), new Gray (_minThr));
			
			if (_debugImg == 3)
				_lastTex = ConvertToTexture (imgThr.Data, imgThr.Width, imgThr.Height);
			
			// create grid
			var objGrid = new bool[_cols, _rows];
			
			for (int x = 0; x < _cols; x++)
				for (int y = 0; y < _rows; y++)
					objGrid[x, y] = false;
			
			// find contur
			using (MemStorage storage = new MemStorage()) {
				for (Contour<Point> contours = imgThr.FindContours(CvEnum.CHAIN_APPROX_METHOD.CV_CHAIN_APPROX_SIMPLE, CvEnum.RETR_TYPE.CV_RETR_TREE, storage); contours != null; contours = contours.HNext) {
					Contour<Point> currentContour = contours.ApproxPoly (contours.Perimeter * 0.015, storage);
					
					var bdRect = currentContour.BoundingRectangle;
					if (bdRect.Width < 20 || bdRect.Height < 20)
						continue;
					
					// check against grid
					for (int x = 0; x < _cols; x++) {
						for (int y = 0; y < _rows; y++) {
							var grRect = new Rectangle ((int)(_gridLoc.x + x * _fieldSize.x + _gridTolerance),
							                            (int)(_gridLoc.y + y * _fieldSize.y + _gridTolerance),
							                            (int)_fieldSize.x - 2 * _gridTolerance,
							                            (int)_fieldSize.y - 2 * _gridTolerance);
							
							imgOrg.Draw (grRect, new Bgr (200, 0, 0), 2);
							
							if (bdRect.IntersectsWith (grRect))
								objGrid [x, y] = true;
						}
					}
					
					imgOrg.Draw (currentContour.BoundingRectangle, new Bgr (255, 255, 0), 2);
				}
			}
			
			// set tile status
			for (int x = 0; x < _cols; x++)
				for (int y = 0; y < _rows; y++)
					_tileCtrl.SetTileStatus (_cols - x - 1, _rows - y - 1, objGrid [x, y]);
			
			if (_debugImg == 4)
				_lastTex = ConvertToTexture (imgOrg.Data, imgOrg.Width, imgOrg.Height);
			
			// draw grid
			var blendImg = new Image<Bgr, byte> (imgOrg.Width, imgOrg.Height);
			
			for (int x = 0; x < _cols; x++) {
				for (int y = 0; y < _rows; y++) {
					var grRect = new Rectangle ((int)(_gridLoc.x + x * _fieldSize.x),
					                            (int)(_gridLoc.y + y * _fieldSize.y),
					                            (int)_fieldSize.x, (int)_fieldSize.y);
					
					blendImg.Draw (grRect, new Bgr (0, 255, 0), objGrid [x, y] ? -1 : 2);
					imgOrg.Draw (grRect, new Bgr (200, 0, 0), 2);
				}
			}
			
			imgOrg = imgOrg.AddWeighted (blendImg, 0.7f, 0.3f, 0);
			
			if (_debugImg == 5)
				_lastTex = ConvertToTexture (imgOrg.Data, imgOrg.Width, imgOrg.Height);
			
			if (_debugImg == 6) {
				if (_lastColorImg != null) {
					var colorImg = new Image<Bgr, byte> (ConvertToImage (_lastColorImg));
					imgOrg = imgOrg.AddWeighted (colorImg, 0.5f, 0.5f, 0);
				}
			}

			// wait for new data
			_updatedData = false;
		}
	}
}

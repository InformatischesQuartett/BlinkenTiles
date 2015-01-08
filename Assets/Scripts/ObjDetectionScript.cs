using UnityEngine;
using System.Collections;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;
using System.Drawing;
using Color = UnityEngine.Color;
using CvEnum = Emgu.CV.CvEnum;

public class ObjDetectionScript : MonoBehaviour {

	public TileController tileCtrl;

	[Range(0,255)]
	public int MinThr;

	[Range(0, 255)]
	public int MaxThr;

	[Range(0, 5)]
	public int DebugImg;

	[Range(10, 512)]
	public int DebugImgSize;

	public Vector2 GridLoc;
	public Vector2 FieldSize;

	public int GridTolerance;
	
	private Texture2D lastTex;
	private bool[,] ObjGrid;

	private DepthSourceManager DepthManager;

	void Start () {
		lastTex = new Texture2D(512, 512);
		DepthManager = GetComponent<DepthSourceManager>();
	}

	void Update()
	{
		if (Time.frameCount % 20 == 0)
			ProcessImg();
	}

	void OnGUI()
	{
		GUI.DrawTexture(new Rect(0, 0, DebugImgSize, DebugImgSize), lastTex);
		GUI.Label(new Rect(5, DebugImgSize - 20, 100, 20), "Schritt " + DebugImg);
		
		if (GUI.Button(new Rect(0, 0, 100, 20), "Process"))
			ProcessImg();
	}

	byte[,,] ConvertToImage(Texture2D tex) {
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

	Texture2D ConvertToTexture(byte[,,] img, int width, int height)
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
	
	void ProcessImg () {
		var depthImg = DepthManager.GetDepthImg ();

		if (depthImg == null)
			return;

		var imgGray = new Image<Gray, byte> (depthImg);
		var imgOrg = imgGray.Convert<Bgr, byte> ();

		if (DebugImg == 1)
			lastTex = ConvertToTexture(imgGray.Data, imgGray.Width, imgGray.Height);
		
		// noise reduction
		var imgSm = imgGray.PyrDown().PyrUp().SmoothGaussian(3);
		
		var element = new StructuringElementEx(5, 5, 2, 2, Emgu.CV.CvEnum.CV_ELEMENT_SHAPE.CV_SHAPE_ELLIPSE);
		CvInvoke.cvErode(imgSm, imgSm, element, 2);
		CvInvoke.cvDilate(imgSm, imgSm, element, 2);

		if (DebugImg == 2)
			lastTex = ConvertToTexture(imgSm.Data, imgSm.Width, imgSm.Height);
		
		// filtering
		var imgThr = imgSm.InRange(new Gray(MinThr), new Gray(MaxThr));
		CvInvoke.cvDilate(imgThr, imgThr, element, 3);
		
		if (DebugImg == 3)
			lastTex = ConvertToTexture(imgThr.Data, imgThr.Width, imgThr.Height);

		// create grid
		ObjGrid = new bool[(int) Config.Cols, (int) Config.Rows];
		
		for (int x = 0; x < Config.Cols; x++)
			for (int y = 0; y < Config.Rows; y++)
				ObjGrid[x, y] = false;

		// find contur
		using (MemStorage storage = new MemStorage())
		{
			for (Contour<Point> contours = imgThr.FindContours(CvEnum.CHAIN_APPROX_METHOD.CV_CHAIN_APPROX_SIMPLE, CvEnum.RETR_TYPE.CV_RETR_TREE, storage); contours != null; contours = contours.HNext)
			{
				Contour<Point> currentContour = contours.ApproxPoly(contours.Perimeter * 0.015, storage);

				var bdRect = currentContour.BoundingRectangle;
				if (bdRect.Width < 20 || bdRect.Height < 20)
					continue;

				// check against grid
				for (int x = 0; x < Config.Cols; x++) {
					for (int y = 0; y < Config.Rows; y++)
					{
						var grRect = new Rectangle((int) (GridLoc.x + x * FieldSize.x + GridTolerance),
					                         	   (int) (GridLoc.y + y * FieldSize.y + GridTolerance),
						                           (int) FieldSize.x - 2 * GridTolerance,
						                           (int) FieldSize.y - 2 * GridTolerance);

						imgOrg.Draw(grRect, new Bgr(200, 0, 0), 2);

						if (bdRect.IntersectsWith(grRect))
							ObjGrid[x, y] = true;
					}
				}

				imgOrg.Draw(currentContour.BoundingRectangle, new Bgr(255, 255, 0), 2);
			}
		}

		// set tile status
		for (int x = 0; x < Config.Cols; x++)
			for (int y = 0; y < Config.Rows; y++)
				tileCtrl.SetTileStatus(x, y, ObjGrid[x, y]);

		if (DebugImg == 4)
			lastTex = ConvertToTexture(imgOrg.Data, imgOrg.Width, imgOrg.Height);

		// draw grid
		var blendImg = new Image<Bgr, byte>(imgOrg.Width, imgOrg.Height);

		for (int x = 0; x < Config.Cols; x++)
		{
			for (int y = 0; y < Config.Rows; y++)
			{
				var grRect = new Rectangle((int) (GridLoc.x + x * FieldSize.x),
				                          (int) (GridLoc.y + y * FieldSize.y),
				                          (int) FieldSize.x, (int) FieldSize.y);

				blendImg.Draw(grRect, new Bgr(0, 255, 0), ObjGrid[x, y] ? -1 : 2);
				imgOrg.Draw(grRect, new Bgr(200, 0, 0), 2);
			}
		}

		imgOrg = imgOrg.AddWeighted(blendImg, 0.7f, 0.3f, 0);

	    if (DebugImg == 5)
			lastTex = ConvertToTexture(imgOrg.Data, imgOrg.Width, imgOrg.Height);

		//imgOrg.Save(@"E:\Test.jpg");*/
	}
}

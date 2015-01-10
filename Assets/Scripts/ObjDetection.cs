using UnityEngine;
using System.Collections;
using System.Threading;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;
using System.Drawing;
using Color = UnityEngine.Color;
using CvEnum = Emgu.CV.CvEnum;

public class ObjDetection : MonoBehaviour {

	[Range(0,255)]
	public int MinThr;

	[Range(0, 255)]
	public int MaxThr;

	[Range(0, 7999)]
	public int MinDepth;

	[Range(0, 7999)]
	public int MaxDepth;

	[Range(0, 6)]
	public int DebugImg;

	[Range(10, 512)]
	public int DebugImgSize;

	public Vector2 GridLoc;
	public Vector2 FieldSize;

	public int GridTolerance;
	
	private Texture2D lastTex;
	private Texture2D lastColorImg;

	private bool[,] ObjGrid;

	private DepthSourceManager DepthManager;

	void Start () {
		lastTex = new Texture2D(512, 424);

		DepthManager = GetComponent<DepthSourceManager>();

		var workerObject = new ObjDetectionThread();
		var workerThread = new Thread(workerObject.ProcessImg);
	}

	void OnGUI()
	{
		GUI.DrawTexture(new Rect(0, 0, DebugImgSize, 424 * (DebugImgSize/512.0f)), lastTex);
		GUI.Label(new Rect(5, 424 * (DebugImgSize/512.0f) - 20, 100, 20), "Schritt " + DebugImg);
		
		if (GUI.Button(new Rect(0, 0, 100, 20), "Save"))
			DepthManager.SaveDepthToFile();

		if (GUI.Button(new Rect(110, 0, 100, 20), "ColorImg"))
			lastColorImg = DepthManager.GetColorImg();
	}	
}

using UnityEngine;
using System.Collections;

public class RenderGui : MonoBehaviour {

	//Stupid asshole RenderTextures
	public RenderTexture LeftRT;
	public RenderTexture CenterRT;
	public RenderTexture RightRT;

	//Width and Height of on RenderTexture, just to make things easier
	private int width, height;

	//The rectangles where the single RenderTextures are supposed to be drawn in
	private Rect RectL;
	private Rect RectC;
	private Rect RectR;



	public Camera ScreenCam;

	// Use this for initialization
	void Start () {
		//Width and Height of the RenderTextures are all the same
		width = LeftRT.width;
		height = LeftRT.height;

		//Set up the positions, wehre the textures are zuposed to be drawn in the GUI
		RectL = new Rect(width/2,Screen.height/2-height/2,width, height);
		RectC = new Rect(width+(width/2),Screen.height/2-height/2,width, height);
		RectR = new Rect((2*width)+width/2,Screen.height/2-height/2,width, height);
	}


	void OnGUI()
	{
		//Rotate the Gui for the first RenderRexture (LeftRT)
		GUIUtility.RotateAroundPivot(-90, RectL.center);
		//Draw the RenderTexture
		GUI.DrawTexture(RectL, LeftRT, ScaleMode.ScaleAndCrop);
		//And because GUI in Unity is so awsome rotate the Gui back
		GUIUtility.RotateAroundPivot(90, RectL.center);

		//And Again for the Center....
		GUIUtility.RotateAroundPivot(-90, RectC.center);
		GUI.DrawTexture(RectC, CenterRT, ScaleMode.ScaleAndCrop);
		GUIUtility.RotateAroundPivot(90, RectC.center);

		//And Again for the Right....
		GUIUtility.RotateAroundPivot(-90, RectR.center);
		GUI.DrawTexture(RectR, RightRT, ScaleMode.ScaleAndCrop);
		GUIUtility.RotateAroundPivot(90, RectR.center);
	}
}

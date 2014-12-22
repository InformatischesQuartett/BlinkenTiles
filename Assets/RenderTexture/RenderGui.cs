using System;
using UnityEngine;

public class RenderGui : MonoBehaviour {

	//Stupid RenderTextures
	public RenderTexture LeftRT;
	public RenderTexture CenterRT;
	public RenderTexture RightRT;
    public Texture background;

	//Width and Height of on RenderTexture, just to make things easier
	private float width, height;
    public float d;

	//The rectangles where the single RenderTextures are supposed to be drawn in
	private Rect RectL;
	private Rect RectC;
	private Rect RectR;
    public float maxWidth;
    public float yPos;
    private float  aspectRatio;
	// Use this for initialization
	void Start () {
		//Width and Height of the RenderTextures are all the same
        aspectRatio = camera.aspect;
        Debug.Log(camera.aspect.GetTypeCode());
        height = background.width / 3.0f;
        width = height*aspectRatio;
	    d = (width - height) + 3;//plus3 pixels
        Debug.Log("d1: " +d);
        Debug.Log("aspectRatio: " + aspectRatio);
        
	    yPos = Screen.height/2.0f - height/2.0f;
		Debug.Log("wxh: " + width + " x "+height);
		maxWidth = (Screen.width/3.0f);
		//Set up the positions, wehre the textures are zuposed to be drawn in the GUI
        RectL = new Rect(0, yPos, width, height);
        RectC = new Rect(width - d, yPos, width, height);
        RectR = new Rect((width * 2.0f) - 2.0f * d, yPos, width, height);
	}


	void OnGUI()
	{
        RectL = new Rect(0, yPos, width, height);
        RectC = new Rect(width - d, yPos, width, height);
        RectR = new Rect((width * 2.0f) - 2.0f*d, yPos, width, height);
		//Rotate the Gui for the first RenderRexture (LeftRT)
		//GUIUtility.RotateAroundPivot(-90, RectL.center);
		//Draw the RenderTexture
	    //GUI.Button(RectL, LeftRT);
        GUI.DrawTexture(RectL, LeftRT, ScaleMode.StretchToFill, true);
		//And because GUI in Unity is so awsome rotate the Gui back
		//GUIUtility.RotateAroundPivot(90, RectL.center);
        //GUI.Box(RectC, "");
		//And Again for the Center....
		//GUIUtility.RotateAroundPivot(-90, RectC.center);
        GUI.DrawTexture(RectC, CenterRT, ScaleMode.StretchToFill, true);
		//GUIUtility.RotateAroundPivot(90, RectC.center);
        
		//And Again for the Right....
		//GUIUtility.RotateAroundPivot(-90, RectR.center);
        GUI.DrawTexture(RectR, RightRT, ScaleMode.StretchToFill, true);
		//GUIUtility.RotateAroundPivot(90, RectR.center);
	}
}

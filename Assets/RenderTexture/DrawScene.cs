using UnityEngine;
using System.Collections;

public class DrawScene : MonoBehaviour {

    public Camera virtualCam;
    public RenderTexture renderTexture;
    private Rect RectLeft, RectCenter, RectRight;
    public Texture2D texLeft, texCenter, texRight;
    public GameObject dummy;
    public float scale;
    public float offset;
    private float x;
    private Vector2 texSize;
    public  float delta;
	// Use this for initialization
	void Start () {
        renderTexture = new RenderTexture(Screen.width, Screen.height, 24);
        virtualCam.targetTexture = renderTexture;
        float w = Screen.width;
	    float h = Screen.height;
	    float aspect = w/h;
        
        if (aspect < 1.4f)
	    {
	        virtualCam.orthographicSize = 1151;
	    }
        else if (aspect < 1.8f)
        {
            virtualCam.orthographicSize = 862;
        }
        else
        {
            Debug.Log("none");
        }

     
        
        float wDummy3 = (2*dummy.renderer.bounds.extents.x)/3;
        float hDummy = 2*dummy.renderer.bounds.extents.y;
	    
        x = hDummy/wDummy3;

	    float wS3 = (renderTexture.width)/3f;
	    float hSs = wS3*x;
        texSize = new Vector2(hSs,wS3);
        delta = (renderTexture.height - hSs) / 2;

        texLeft = new Texture2D((int)wS3, (int)hSs, TextureFormat.RGB24, false);
        texCenter = new Texture2D((int)wS3, (int)hSs, TextureFormat.RGB24, false);
        texRight = new Texture2D((int)wS3, (int)hSs, TextureFormat.RGB24, false);

	    offset = 50;
        scale = (w / 3) / (hSs+5);
	}
    
    void OnPostRender()
    {
        virtualCam.targetTexture = renderTexture;
        RenderTexture.active = renderTexture;
        texLeft.ReadPixels(new Rect(0, delta, texSize.y, renderTexture.height-delta), 0, 0);
        texCenter.Apply();

        texCenter.ReadPixels(new Rect(texSize.y, delta, texSize.y, renderTexture.height - delta), 0, 0);
        texLeft.Apply();

        texRight.ReadPixels(new Rect(2 * texSize.y, delta, texSize.y, renderTexture.height - delta), 0, 0);
        texRight.Apply();

        RenderTexture.active = null; // "just in case" 
        //virtualCam.targetTexture = null;
    }

    private void OnGUI()
    {
        RectLeft = new Rect(offset, 0, texLeft.width * scale, texLeft.height * scale);
        RectCenter = new Rect(offset + texSize.x * scale, 0, texLeft.width * scale, texLeft.height * scale);
        RectRight = new Rect(offset + (2 * texSize.x) * scale, 0, texLeft.width * scale, texLeft.height * scale);

        GUIUtility.RotateAroundPivot(-90, RectLeft.center);
        GUI.DrawTexture(RectLeft, texLeft, ScaleMode.StretchToFill);
        GUIUtility.RotateAroundPivot(90, RectLeft.center);

        GUIUtility.RotateAroundPivot(-90, RectCenter.center);
        GUI.DrawTexture(RectCenter, texCenter, ScaleMode.StretchToFill);
        GUIUtility.RotateAroundPivot(90, RectCenter.center);

        GUIUtility.RotateAroundPivot(-90, RectRight.center);
        GUI.DrawTexture(RectRight, texRight, ScaleMode.StretchToFill);
        GUIUtility.RotateAroundPivot(90, RectRight.center);
    }
}

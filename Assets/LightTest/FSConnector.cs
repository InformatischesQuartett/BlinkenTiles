using UnityEngine;
using System.Collections;
using LetThereBeLight;

public class FSConnector: MonoBehaviour {

	private byte brightness;
	private byte[] faderValues = new byte[512]; 
	// Use this for initialization
	void Start () {


		//erstes
		faderValues[1] = 131;
		faderValues[2] = 131;
		faderValues[3] = 131;
		faderValues[4] = brightness;

		//zweites
		faderValues[5] = 10;
		faderValues[6] = 10;
		faderValues[7] = 10;
		faderValues[8] = brightness;

		//drittes
		faderValues[9] = 0;
		faderValues[10] = 0;
		faderValues[11] = 0;
		faderValues[12] = brightness;
		brightness = 0;
		Debug.Log(Talker.FindWindow((string)null, "FS"));
		Talker.MessageFunction(faderValues,1); 
	}

	void OnGUI()
	{
		//este Spalte
		if(GUI.Button(new Rect(((Screen.width/2)-50), (Screen.height/2)-30, 50,30), "Rot"))
		{

			faderValues[0] = 122; //R
			faderValues[1] = 2; //G
			faderValues[2] = 3; //B
			faderValues[3] = brightness; //CW
			Talker.MessageFunction(faderValues,1);
		}
		if(GUI.Button(new Rect(((Screen.width/2)-50), Screen.height/2, 50,30), "Grün"))
		{
			faderValues[4] = 2; //R
			faderValues[1] = 122; //G
			faderValues[2] = 3; //B
			faderValues[3] = brightness; //CW
			//Talker.MessageFunction(faderValues,1);
		}
		if(GUI.Button(new Rect(((Screen.width/2)-50), (Screen.height/2)+30, 50,30), "Blau"))
		{
			faderValues[0] = 2; //R
			faderValues[1] = 2; //G
			faderValues[2] = 122; //B
			faderValues[3] = brightness; //CW
			//Talker.MessageFunction(faderValues,1);
		}

		//zweite Spalte
		if(GUI.Button(new Rect(((Screen.width/2)), (Screen.height/2)-30, 50,30), "Rot"))
		{
			//erstes
			faderValues[4] = 122; //R
			faderValues[5] = 2; //G
			faderValues[6] = 3; //B
			faderValues[7] = brightness; //CW
			//Talker.MessageFunction(faderValues,1);
		}
		if(GUI.Button(new Rect(((Screen.width/2)), Screen.height/2, 50,30), "Grün"))
		{
			//erstes
			faderValues[4] = 2; //R
			faderValues[5] = 122; //G
			faderValues[6] = 3; //B
			faderValues[7] = brightness; //CW
			//Talker.MessageFunction(faderValues,1);
		}
		if(GUI.Button(new Rect(((Screen.width/2)), (Screen.height/2)+30, 50,30), "Blau"))
		{
			//erstes
			faderValues[4] = 2; //R
			faderValues[5] = 2; //G
			faderValues[6] = 122; //B
			faderValues[7] = brightness; //CW
			//Talker.MessageFunction(faderValues,1);
		}

		//dritte Spalte
		if(GUI.Button(new Rect(((Screen.width/2)+50), (Screen.height/2)-30, 50,30), "Rot"))
		{
			//erstes
			faderValues[8] = 122; //R
			faderValues[9] = 2; //G
			faderValues[10] = 3; //B
			faderValues[11] = brightness; //CW
			//Talker.MessageFunction(faderValues,1);
		}
		if(GUI.Button(new Rect(((Screen.width/2)+50), Screen.height/2, 50,30), "Grün"))
		{
			//erstes
			faderValues[8]  = 2; //R
			faderValues[9]  = 122; //G
			faderValues[10] = 3; //B
			faderValues[11] = brightness; //CW
			//Talker.MessageFunction(faderValues,1);
		}
		if(GUI.Button(new Rect(((Screen.width/2)+50), (Screen.height/2)+30, 50,30), "Blau"))
		{
			//erstes
			faderValues[8]  = 2; //R
			faderValues[9]  = 2; //G
			faderValues[10] = brightness; //B
			//Talker.MessageFunction(faderValues,1);

		}
		Talker.MessageFunction(faderValues,1);

		brightness = (byte)GUI.HorizontalSlider(new Rect(25, 25, 100, 30), brightness, 0, 255);
		

	}
	// Update is called once per frame
	void Update () {
	
	}
}

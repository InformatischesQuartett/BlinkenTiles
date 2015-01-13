using System.Collections.Generic;
using LetThereBeLight;
using UnityEngine;

public class LightController : MonoBehaviour
{
    private Spot[] spotList= new Spot[8];
    private int numSpots;
   // private Spot _spot;
    private List<byte> fValues = new List<byte>();
    private byte[] faderValues = new byte[513];

    private int curSpot;
    private int count;
	// Use this for initialization
	void Start ()
	{
	    count = 0;
	    numSpots = 8; // to be set by xml or txt file --> external
	    curSpot = 0;
	    
	    for(int i = 0; i < 513; i++)
	    {
            fValues.Add(0);
	    }
        for (int i = 0; i < 8; i++)
        {
            spotList[i] = new Spot(i * 12, ref fValues);
        }
	    //_spot = new Spot(0,ref fValues);
	    /*for (int i = 0; i < numSpots; i++)
	    {
            spotList[i].UpdateValueList();
	    }*/
        UpdateArray();

	   /* for (int i = 0; i < 24; i++)
	    {
	        faderValues[i] = 0;
	    }
        faderValues[0] = 10;
        faderValues[1] = 0;
        faderValues[2] = 0;
        faderValues[3] = 0;

        faderValues[13] = 10;
        faderValues[14] = 0;
        faderValues[15] = 0;
        faderValues[16] = 0;*/
	    
	    Talker.MessageFunction(faderValues, 1);
	}



    public void UpdateFaderValues(int curCol, float timerCol)
    {
        //Jedes päckle dauert 60/BPM
       // 3*päckle = 1* timerCol
        curSpot = curCol;       
        Next(timerCol);
        Talker.MessageFunction(faderValues, 1);
    }

    private int p;
    //get Active collum from TileController one collum = one Spot 
    private void Next(float timerCol)
    {
      
        if(BlinkHelper.Between(timerCol,0,(20/Config.BPM), true))
        {
            p = 0;
        }
        else if (BlinkHelper.Between(timerCol, (20/Config.BPM), (40/Config.BPM), true))
        {
            p = 1;
        }
        else if (BlinkHelper.Between(timerCol, (40/Config.BPM), (60/Config.BPM), true))
        {
            p = 2;
        }

        spotList[curSpot].LightItUp(p);
        faderValues = fValues.ToArray();
        spotList[curSpot].SetColor(Spot.CurrentCollor.Black);
    }

    private void UpdateArray()
    {
        faderValues = fValues.ToArray();
    }
}

﻿using System.Collections.Generic;
using LetThereBeLight;
using UnityEngine;

public class LightController : MonoBehaviour
{
    private Spot[] spotList= new Spot[8];
    private List<byte> _fValues = new List<byte>();
    private byte[] _faderValues = new byte[513];
    private int _curSpot;
    private int _column;

	// Use this for initialization
	void Start ()
	{
	    _curSpot = 0;
	    
	    for(int i = 0; i < 513; i++)
	    {
            _fValues.Add(0);
	    }
        for (int i = 0; i < 8; i++)
        {
            spotList[i] = new Spot(i * 12, ref _fValues);
        }
        UpdateArray();
        Talker.MessageFunction(_faderValues, 1);
	}



    public void UpdateFaderValues(int curCol, float timerCol)
    {
        _curSpot = curCol;       
        Next(timerCol);
        Talker.MessageFunction(_faderValues, 1);
    }

    
    //get Active collum from TileController one collum = one Spot 
    private void Next(float timerCol)
    {
      
        if(BlinkHelper.Between(timerCol,0,(20/Config.BPM), true))
        {
            _column = 0;
        }
        else if (BlinkHelper.Between(timerCol, (20/Config.BPM), (40/Config.BPM), true))
        {
            _column = 1;
        }
        else if (BlinkHelper.Between(timerCol, (40/Config.BPM), (60/Config.BPM), true))
        {
            _column = 2;
        }

        spotList[_curSpot].LightItUp(_column);
        _faderValues = _fValues.ToArray();
        spotList[_curSpot].SetColor(Spot.CurrentCollor.Black);
    }

    private void UpdateArray()
    {
        _faderValues = _fValues.ToArray();
    }
}

using System.Collections.Generic;
using System.Linq;
using LetThereBeLight;
using UnityEngine;
using System.Collections;

public class LightController : MonoBehaviour
{

    private Spot _spot;
    private List<byte> fValues = new List<byte>();
    private byte[] faderValues = new byte[512];
	// Use this for initialization
	void Start ()
	{
	    for(int i = 0; i < 512; i++)
	    {
	        fValues.Add(0);
	    }
	    _spot = new Spot(0,ref fValues);
        faderValues = fValues.ToArray();
	    Talker.MessageFunction(faderValues, 1);
	}

    public void UpdateFaderValues()
    {
        _spot.ChangeColor();
        faderValues = fValues.ToArray();
        Talker.MessageFunction(faderValues, 1);
    }

    
	// Update is called once per frame
	void Update () {
	
	}
}

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Collections;
using LetThereBeLight;
public class Spot
{
    private int position;
    private byte[] row1;//= new byte[4];
    private byte[] row2 = new byte[4];
    private byte[] row3 = new byte[4];
    private List<byte> valueList;
    private byte red, green, blue;
    private byte intensity;
    private CurrentCollor currentcolor;
    public enum CurrentCollor
    {
        Red,Green,Blue,White
    }
    public Spot(int p, ref List<byte> vList)
    {
        position = p;
        valueList = vList;
        Debug.Log("new Spot");
        red = green= blue = 1;
        intensity = 50;
        currentcolor = CurrentCollor.White;
        //red *= (byte)intensity;
        row1 = new byte[] { red, green, blue, 1 };
        row2 = new byte[] { red, green, blue, 1 };
        row3 = new byte[] { red, green, blue, 1 };
        
        UpdateValueList();
        
    }

    public void ChangeColor()
    {
        switch (currentcolor)
        {
            case CurrentCollor.Red:
                SetRowToColor(2,CurrentCollor.Green);
                currentcolor = CurrentCollor.Green;
                break;
            case CurrentCollor.Green:
                SetRowToColor(2, CurrentCollor.Blue);
                currentcolor = CurrentCollor.Blue;
                break;
            case CurrentCollor.Blue:
                SetRowToColor(2, CurrentCollor.White);
                currentcolor = CurrentCollor.White;
                break;
            case CurrentCollor.White:
                SetRowToColor(2, CurrentCollor.Red);
                currentcolor = CurrentCollor.Red;
                break;
            default:
                break;
        }   
        UpdateValueList();
    }   
 
    public void SetRowToColor(int row, CurrentCollor color)
    {
        switch (currentcolor)
        {
            case CurrentCollor.Red:
                red *= intensity;
                green = blue = 1;
                break;
            case CurrentCollor.Green:
                green *= intensity;
                green = blue = 1;
                break;
            case CurrentCollor.Blue:
                blue *= intensity;
                green = blue = 1;
                break;
            case CurrentCollor.White:
                red = green = blue = intensity;
                break;
            default:
                break;
        }

        switch (row)
        {
            case 1:
                row1 = new byte[] {red, green, blue, 1};
                break;
            case 2:
                row2 = new byte[] { red, green, blue, 1 };
                break;
            case 3:
                row3 = new byte[] { red, green, blue, 1 };
                break;
            default:
                row1 =row2=row3= new byte[] { 1, 1, 1, 1 };
                break;
        }
    }

    public void UpdateValueList()
    {
        for(int i = 0; i < row1.Length;i++)
        {
            valueList[position + i] = row1[i];
            valueList[position + 2*i] = row2[i];
            valueList[position + 3*i] = row3[i];
        }
    }

}

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class Spot
{
    private int position;
    private List<byte> valueList;
    private byte red, green, blue;
    private byte intensity;
    private byte[] _lightColor= new byte[4];
    private CurrentCollor currentcolor;
    public enum CurrentCollor
    {
        Red,Green,Blue,White, Black
    }
    public Spot(int p, ref List<byte> vList)
    {
        position = p;
        valueList = vList;

        
        Debug.Log("new Spot " + position);
        red = blue = green =0;
        intensity = 20;

        SetRowToColor(0,CurrentCollor.Black);
        SetRowToColor(1, CurrentCollor.Black);
        SetRowToColor(2, CurrentCollor.Black);
        

        /*valueList[position] = intensity;
        valueList[position + 1] = 0;
        valueList[position + 2] = 0;
        valueList[position + 3] = 0;

        valueList[position + 4] = intensity;
        valueList[position+5] = 0;
        valueList[position+6] = 0;
        valueList[position+7] = 0;

        valueList[position + 8] = intensity;
        valueList[position + 9] = 0;
        valueList[position + 10] = 0;
        valueList[position + 11] = 0;*/
       
    }

    public void LightItUp(int row)
    {
        switch (row)
        {
            case 0:
                for (int i = 0; i < 3; i++)
                {
                    valueList[position + i] = Config.LightColor[i];
                }
                valueList[position + 3] = 0;
                break;
            case 1:
                for (int i = 4; i < 7; i++)
                {
                    valueList[position + i] = Config.LightColor[i - 4];
                }
                valueList[position + 7] = 0;
                break;
            case 2:
                for (int i = 8; i < 11; i++)
                {
                    valueList[position + i] = Config.LightColor[i - 8];
                }
                valueList[position + 7] = 0;
                break;
            default:
                valueList[position] = 1;
                valueList[position + 1] = 1;
                valueList[position + 2] = 1;
                valueList[position + 3] = 0;

                valueList[position + 4] = 1;
                valueList[position + 5] = 1;
                valueList[position + 6] = 1;
                valueList[position + 7] = 0;

                valueList[position + 8] = 1;
                valueList[position + 9] = 1;
                valueList[position + 10] = 1;
                valueList[position + 11] = 0;
                break;
        }
    }

    public void LightUp(int rowNr)
    {
        SetRowToColor(0, CurrentCollor.Black);
        SetRowToColor(1, CurrentCollor.Black);
        SetRowToColor(2, CurrentCollor.Black);
        SetRowToColor(rowNr, CurrentCollor.Red);
    }

    public void SetColor(CurrentCollor color)
    {
        switch (color)
        {
            case CurrentCollor.Red:
                SetRowToColor(0, CurrentCollor.Red);
                SetRowToColor(1, CurrentCollor.Red);
                SetRowToColor(2, CurrentCollor.Red);
                break;
            case CurrentCollor.Green:
                SetRowToColor(0, CurrentCollor.Green);
                SetRowToColor(1, CurrentCollor.Green);
                SetRowToColor(2, CurrentCollor.Green);
                break;
            case CurrentCollor.Blue:
                SetRowToColor(0, CurrentCollor.Blue);
                SetRowToColor(1, CurrentCollor.Blue);
                SetRowToColor(2, CurrentCollor.Blue);
                break;
            case CurrentCollor.White:
                SetRowToColor(0, CurrentCollor.White);
                SetRowToColor(1, CurrentCollor.White);
                SetRowToColor(2, CurrentCollor.White);
                break;
            case CurrentCollor.Black:
                SetRowToColor(0, CurrentCollor.Black);
                SetRowToColor(1, CurrentCollor.Black);
                SetRowToColor(2, CurrentCollor.Black);
                break;
            default:
                break;
        }   
    }   
 
    public void SetRowToColor(int row, CurrentCollor color)
    {
        switch (color)
        {
            case CurrentCollor.Red:
                red = intensity;
                green = blue = 0;
                break;
            case CurrentCollor.Green:
                green = intensity;
                red = blue = 0;
                break;
            case CurrentCollor.Blue:
                blue = intensity;
                green = red = 0;
                break;
            case CurrentCollor.White:
                red = green = blue = intensity;
                break;
            case CurrentCollor.Black:
                red = green = blue = 0;
                break;
            default:
                break;
        }

        switch (row)
        {
            case 0:
                valueList[position+0] = red;
                valueList[position + 1] = green;
                valueList[position + 2] = blue;
                valueList[position + 3] = 0;
                break;
            case 1:
                valueList[position+4] = red;
                valueList[position+5] = green;
                valueList[position+6] = blue;
                valueList[position + 7] = 0;
                break;
            case 2:
                valueList[position + 8] = red;
                valueList[position + 9] = green;
                valueList[position + 10] = blue;
                valueList[position + 11] = 0;
                break;
            default:
                valueList[position] = 1;
                valueList[position + 1] = 1;
                valueList[position + 2] = 1;
                valueList[position + 3] = 0;

                valueList[position+4] = 1;
                valueList[position+5] = 1;
                valueList[position+6] = 1;
                valueList[position+7] = 0;

                valueList[position + 8] = 1;
                valueList[position + 9] = 1;
                valueList[position + 10] = 1;
                valueList[position + 11] = 0;
                break;
        }
    }
}

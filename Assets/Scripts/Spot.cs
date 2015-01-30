using System.Collections.Generic;

public class Spot
{
    public enum CurrentCollor
    {
        Red,
        Green,
        Blue,
        White,
        Black
    }

    private readonly byte _intensity;

    private readonly int _position;
    private readonly List<byte> _valueList;
    private byte _blue;
    private CurrentCollor _currentcolor;
    private byte _green;
    private byte _red;

    public Spot(int p, ref List<byte> vList)
    {
        _position = p;
        _valueList = vList;

        _red = _blue = _green = 0;
        _intensity = 20;

        SetRowToColor(0, CurrentCollor.Black);
        SetRowToColor(1, CurrentCollor.Black);
        SetRowToColor(2, CurrentCollor.Black);
    }

    public void LightItUp(int row)
    {
        switch (row)
        {
            case 0:
                for (int i = 0; i < 3; i++)
                {
                    _valueList[_position + i] = Config.LightColor[i];
                }
                _valueList[_position + 3] = 0;
                break;
            case 1:
                for (int i = 4; i < 7; i++)
                {
                    _valueList[_position + i] = Config.LightColor[i - 4];
                }
                _valueList[_position + 7] = 0;
                break;
            case 2:
                for (int i = 8; i < 11; i++)
                {
                    _valueList[_position + i] = Config.LightColor[i - 8];
                }
                _valueList[_position + 7] = 0;
                break;
            default:
                for (int i = 0; i < 12; i++)
                {
                    _valueList[_position + i] = 0;
                }
                break;
        }
    }


    //can be used to set spot to one of the predefined Colors
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
                SetRowToColor(0, CurrentCollor.Black);
                SetRowToColor(1, CurrentCollor.Black);
                SetRowToColor(2, CurrentCollor.Black);
                break;
        }
    }

    public void SetRowToColor(int row, CurrentCollor color)
    {
        switch (color)
        {
            case CurrentCollor.Red:
                _red = _intensity;
                _green = _blue = 0;
                break;
            case CurrentCollor.Green:
                _green = _intensity;
                _red = _blue = 0;
                break;
            case CurrentCollor.Blue:
                _blue = _intensity;
                _green = _red = 0;
                break;
            case CurrentCollor.White:
                _red = _green = _blue = _intensity;
                break;
            case CurrentCollor.Black:
                _red = _green = _blue = 0;
                break;
            default:
                _red = _green = _blue = 0;
                break;
        }

        switch (row)
        {
            case 0:
                _valueList[_position + 0] = _red;
                _valueList[_position + 1] = _green;
                _valueList[_position + 2] = _blue;
                _valueList[_position + 3] = 0;
                break;
            case 1:
                _valueList[_position + 4] = _red;
                _valueList[_position + 5] = _green;
                _valueList[_position + 6] = _blue;
                _valueList[_position + 7] = 0;
                break;
            case 2:
                _valueList[_position + 8] = _red;
                _valueList[_position + 9] = _green;
                _valueList[_position + 10] = _blue;
                _valueList[_position + 11] = 0;
                break;
            default:
                for (int i = 0; i < 12; i++)
                {
                    _valueList[_position + i] = 0;
                }
                break;
        }
    }
}
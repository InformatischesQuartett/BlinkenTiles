using UnityEngine;

public class ExitApplication : MonoBehaviour
{
    // Use this for initialization

    private float _buttonHeight;
    private float _buttonWidth;
    private ExitMenu _exitMenu;
    private float _menuHeight;
    private Vector2 _menuPos;
    private float _menuWidth;

    private void Start()
    {
        _exitMenu = null;
        _menuWidth = (Screen.width*0.5f)*0.75f;
        _menuHeight = Screen.height*0.3f;
        _buttonWidth = _menuWidth/3.0f;
        _buttonHeight = _menuHeight/3;
        _menuPos = new Vector2(((Screen.width*0.5f) - _menuWidth)*0.5f, (Screen.height*0.5f) - _menuHeight*0.5f);
    }

    private void OnGUI()
    {
        GUI.depth = -1;
        GUI.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 1);
        if (_exitMenu != null)
            _exitMenu();
    }

    private void ShowExitMenu()
    {
        GUI.BeginGroup(new Rect(_menuPos.x, _menuPos.y, _menuWidth, _menuHeight));
        GUI.Box(new Rect(0, _menuHeight*0.2f, _menuWidth, _menuHeight), "Are you sure sou want to quit the application?");
        if (GUI.Button(new Rect(_buttonWidth*0.5f, (_menuHeight*0.5f), _buttonWidth, _buttonHeight), "Yes"))
        {
            Application.Quit();
        }
        if (
            GUI.Button(
                new Rect((_menuWidth*0.5f) + (_buttonWidth*0.5f)*0.5f, (_menuHeight*0.5f), _buttonWidth, _buttonHeight),
                "No"))
        {
            _exitMenu = null;
        }
        GUI.EndGroup();
    }

    // Update is called once per frame
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && _exitMenu == null)
        {
            _exitMenu = ShowExitMenu;
        }
        else if (Input.GetKeyDown(KeyCode.Escape) && _exitMenu != null)
        {
            _exitMenu = null;
        }
    }

    private delegate void ExitMenu();
}
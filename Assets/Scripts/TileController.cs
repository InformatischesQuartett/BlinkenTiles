using System;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using Random = UnityEngine.Random;

public class TileController : MonoBehaviour
{
    public GameObject TilePrefab;
    private int _activeCol;
    private int _beatCounter;
    private Font _countdownFont;
    private float _countdownTexTimer;
    private MovieTexture _countdownTexture;
    private GUIFunction _currenGuiFunction;

    private Font _defaultFont;

    /*Timer start idle time and set up footprints after this certain time*/
    /*Determines how many footprints are allowed on the field*/
    private int _footprintCount = 4;
    private float _idleResetTimer;
    private float _idleTimer;
    private int _killcounter;
    private LightController _lightController;
    private List<TileCol> _matrix;
    private bool _matrixReady;


    private string _networkPath;
    private NetworkSet _networkSet;
    private int _previousActiveCol;
    private List<GameObject> _tempGameObjects;
    private GameObject _tempParent;
    private GameObject _tileParent;
    private float _timerCol;
    // Use this for initialization

    private int _points;
    public float TimerField { get; private set; }
    public float FieldWidth { get; private set; }

    private void Start()
    {
        _networkPath = Application.streamingAssetsPath + @"\Network\network.xml";
        _networkSet = new NetworkSet();
        _currenGuiFunction = null;
        _countdownTexTimer = 0;

        _defaultFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
        _countdownFont = Resources.Load<Font>("BankGothic");
        _countdownTexture = Resources.Load<MovieTexture>("Textures/ChallengeMode");
        GetComponent<AudioSource>().clip = _countdownTexture.audioClip;

        _lightController = GetComponent<LightController>();
        _tileParent = GameObject.Find("Tiles");
        _tempParent = GameObject.Find("Temp");

        _matrix = new List<TileCol>();
        _tempGameObjects = new List<GameObject>();

        _activeCol = 0;

        BuildTiles();
        LoadSong(Songtype.Freestyle, 2);
    }

    // Update is called once per frame
    private void Update()
    {
        GetInputs();

        if (_matrixReady && _activeCol >= 0)
        {
            if (Config.CurrentGamemode == Gamemode.Freestyle)
            {
                //highlight pass
                for (int i = 0; i < Config.Cols; i++)
                {
                    for (int j = 0; j < Config.Rows; j++)
                    {
                        _matrix[i].Tiles[j].TileGo.GetComponent<TileBehaviour>().Highlight = Highlighttype.None;
                    }
                }

                //timer pass
                for (int i = 0; i < Config.Rows; i++)
                {
                    _matrix[_activeCol].Tiles[i].TileGo.GetComponent<TileBehaviour>().Highlight = Highlighttype.Time;
                }

                //people pass
                for (int i = 0; i < Config.Cols; i++)
                {
                    for (int j = 0; j < Config.Rows; j++)
                    {
                        if (_matrix[i].Tiles[j].Active ||
                            _matrix[i].Tiles[j].TileGo.GetComponent<TileBehaviour>().ForceActive)
                            _matrix[i].Tiles[j].TileGo.GetComponent<TileBehaviour>().Highlight = Highlighttype.Occupied;
                    }
                }

                //Shake 'n' Play pass
                for (int i = 0; i < Config.Rows; i++)
                {
                    if (_matrix[_activeCol].Tiles[i].Active ||
                        _matrix[_activeCol].Tiles[i].TileGo.GetComponent<TileBehaviour>().ForceActive)
                    {
                        _matrix[_activeCol].Tiles[i].TileGo.GetComponent<TileBehaviour>().Highlight = Highlighttype.Hit;
                        if (_activeCol != _previousActiveCol)
                        {
                            _matrix[_activeCol].Tiles[i].TileGo.GetComponent<TileBehaviour>().Shake();

                            GameObject soundGo =
                                GameObject.Find("Temp/TileSounds/" + _matrix[_activeCol].Tiles[i].soundIndex);
                            soundGo.GetComponent<AudioClipLoader>().Play(AudioPlayMode.Once);
                        }
                    }
                }
            }

            if (Config.CurrentGamemode == Gamemode.Challenge)
            {
                if (_matrix[_activeCol].ChallengeIndex.Count == 0)
                {
                    UpdateNeworkXML();
                    LoadSongRandom();
                    return;
                }

                //highlight pass
                for (int i = 0; i < Config.Cols; i++)
                {
                    for (int j = 0; j < Config.Rows; j++)
                    {
                        _matrix[i].Tiles[j].TileGo.GetComponent<TileBehaviour>().Highlight = Highlighttype.None;
                        _matrix[i].Tiles[j].TileGo.GetComponent<TileBehaviour>().ResetTexture();
                    }
                }

                //preview pass
                for (int i = 0; i < Config.Cols; i++)
                {
                    if (_matrix[i].ChallengeIndex.Count > 0)
                    {
                        int previewIndex = _matrix[i].ChallengeIndex[0] - 1;
                        //Debug.Log("Spalte " + i + ": " + _matrix[i].ChallengeIndex.Count + " -> " + previewIndex);

                        if (previewIndex >= Config.Rows)
                            previewIndex -= Config.Rows;

                        if (previewIndex >= 0)
                        {
                            _matrix[i].Tiles[previewIndex].TileGo.GetComponent<TileBehaviour>().Highlight =
                                Highlighttype.Preview;
                            _matrix[i].Tiles[previewIndex].TileGo.GetComponent<TileBehaviour>().SetBorder();
                        }
                    }
                }

                //timer pass
                for (int i = 0; i < Config.Rows; i++)
                {
                    _matrix[_activeCol].Tiles[i].TileGo.GetComponent<TileBehaviour>().Highlight = Highlighttype.Time;
                }

                //people pass
                bool peopleInActiveCol = false;

                for (int i = 0; i < Config.Cols; i++)
                {
                    for (int j = 0; j < Config.Rows; j++)
                    {
                        if (_matrix[i].Tiles[j].Active ||
                            _matrix[i].Tiles[j].TileGo.GetComponent<TileBehaviour>().ForceActive)
                        {
                            _matrix[i].Tiles[j].TileGo.GetComponent<TileBehaviour>().Highlight = Highlighttype.Occupied;

                            if (i == _activeCol)
                            {
                                peopleInActiveCol = true;
                            }
                        }
                    }
                }


                //Shake 'n' Play 'n' change preview pass
                if (_beatCounter <= Config.PreheatDuration)
                    return;

                for (int i = 0; i < Config.Rows; i++)
                {
                    int previewIndex = _matrix[_activeCol].ChallengeIndex[0] - 1;

                    if (previewIndex >= Config.Rows)
                        previewIndex -= Config.Rows;

                    bool active = _matrix[_activeCol].Tiles[i].Active ||
                                  _matrix[_activeCol].Tiles[i].TileGo.GetComponent<TileBehaviour>().ForceActive;

                    if (previewIndex == i && (active || !peopleInActiveCol))
                    {
                        _matrix[_activeCol].Tiles[i].TileGo.GetComponent<TileBehaviour>().Highlight = Highlighttype.Hit;
                        if (_activeCol != _previousActiveCol)
                        {
                            _matrix[_activeCol].Tiles[i].TileGo.GetComponent<TileBehaviour>().Shake();

                            GameObject soundGo =
                                GameObject.Find("Temp/TileSounds/" + _matrix[_activeCol].Tiles[i].soundIndex);
                            soundGo.GetComponent<AudioClipLoader>().Play(AudioPlayMode.Once);
                            if (peopleInActiveCol)
                            {
                                //Count good points
                                _points++;
                                _networkSet.Song.Points = _points;
                                UpdateNeworkXML();
                                Debug.Log(_networkSet.Song.Points);
                            }
                        }
                    }
                    else if (active && previewIndex != i)
                    {
                        _matrix[_activeCol].Tiles[i].TileGo.GetComponent<TileBehaviour>().Highlight = Highlighttype.Fail;
                        if (_activeCol != _previousActiveCol)
                        {
                            _matrix[_activeCol].Tiles[i].TileGo.GetComponent<TileBehaviour>().Shake();

                            GameObject soundGo =
                                GameObject.Find("Temp/TileFailSounds/" + _matrix[_activeCol].Tiles[i].soundIndex);
                            soundGo.GetComponent<AudioClipLoader>().Play(AudioPlayMode.Once);
                        }
                    }
                }

                //play song after countdown
                if (Config.CurrentGamemode == Gamemode.Challenge && _beatCounter == Config.PreheatDuration + 1 &&
                    _activeCol != _previousActiveCol)
                {
                    _networkSet.Song.Points = 0;
                    GameObject.FindGameObjectWithTag("Song").GetComponent<AudioSource>().Play();
                    _networkSet.Song.Length = GameObject.FindGameObjectWithTag("Song").GetComponent<AudioSource>().clip.length;
                    UpdateNeworkXML();
                }
            }
        }
    }

    private void LateUpdate()
    {
        if (_activeCol != _previousActiveCol && _activeCol >= 0)
        {
            if (_matrix[_previousActiveCol].ChallengeIndex.Count > 0)
                _matrix[_previousActiveCol].ChallengeIndex.RemoveAt(0);

            _previousActiveCol = _activeCol;
        }

        /**
		 * Check if IdleMode = true.
		 * If IdleMode is active, set footprintCount tiles footprint textures and set them forceActive.
		 **/
        if (!Config.IdleMode && Config.CurrentGamemode == Gamemode.Freestyle)
        {
            /*set IdleMode true if _idleTimer gets above IdleDelay value - set it to false if it falls below*/
            Config.IdleMode = _idleTimer > Config.IdleDelay;

            /*Set Idle Behaviour if IdleMode is activated --> creates footprints*/
            if (Config.IdleMode)
            {
                SetIdleBehaviour();
                //reset idleResetTimer
                _idleResetTimer = 0.0f;
            }
        }

        if (Config.IdleMode && Config.CurrentGamemode == Gamemode.Freestyle)
        {
            /*set IdleMode true if _idleTimer gets above IdleDelay value - set it to false if it falls below*/
            Config.IdleMode = _idleTimer > Config.IdleDelay;
            if (!Config.IdleMode)
            {
                RemoveIdleBehaviour();
            }
            else if (_idleResetTimer > Config.IdleResetDelay)
            {
                /*Resetting footprint positions after a certain amount of time*/
                RemoveIdleBehaviour();
                SetIdleBehaviour();
                _idleResetTimer = 0.0f;
            }
        }
    }

    private void FixedUpdate()
    {
        //IdleTimer
        _idleTimer += Time.fixedDeltaTime;

        _idleResetTimer += Time.fixedDeltaTime;

        if (_timerCol > 60/Config.BPM)
        {
            _timerCol = (_timerCol - (60/Config.BPM)) + Time.fixedDeltaTime;
            _activeCol++;
            _beatCounter++;

            if (_activeCol >= Config.Cols)
                _activeCol = 0;

            if (_beatCounter > Config.PreheatDuration)
            {
                _currenGuiFunction = null;
            }
        }
        else
        {
            _timerCol += Time.fixedDeltaTime;
        }

        if (_activeCol >= 0)
            _lightController.UpdateFaderValues(_activeCol, _timerCol);
    }

    private void OnGUI()
    {
        if (_currenGuiFunction != null)
            _currenGuiFunction();
    }

    private void CountdownGUI()
    {
        _countdownTexTimer += Time.deltaTime;

        if (_countdownTexTimer > 8)
        {
            _countdownTexture.Pause();
            GetComponent<AudioSource>().Pause();
        }

        GUI.color = new Color(1, 1, 1, Math.Min(0.7f, _countdownTexTimer/8f));
        GUI.DrawTexture(new Rect(Screen.width/2f, 0, Screen.width/2f, Screen.height), _countdownTexture);

        if (_countdownTexTimer > 2)
        {
            GUI.skin.label.alignment = TextAnchor.MiddleCenter;
            GUI.color = new Color(0, 0, 0, Math.Min(0.9f, (_countdownTexTimer - 2)/8f));
            GUI.skin.label.fontSize = 200;
            GUI.skin.label.font = _countdownFont;

            var lbRect = new Rect(Screen.width/2f, Screen.height - 300, Screen.width/2f, 300);

            if (Config.PreheatDuration - _beatCounter == 0)
                GUI.Label(lbRect, "GO!");
            else if (Config.PreheatDuration - _beatCounter < Config.PreheatShowAt)
                GUI.Label(lbRect, (Config.PreheatDuration - _beatCounter).ToString());
            else
            {
                GUI.skin.label.fontSize = 70;
                GUI.Label(lbRect, _networkSet.Song.Title);
            }

            GUI.skin.label.font = _defaultFont;
            GUI.skin.label.fontSize = 12;
            GUI.skin.label.alignment = TextAnchor.MiddleLeft;
        }
    }

    public void BuildTiles()
    {
        float xInc = (Config.TileWidth + Config.TileSpaceing);
        float yInc = (Config.TileHeight + Config.TileSpaceing);
        float xStart = -((Config.Cols/2.0f*xInc) - xInc/2);
        float yStart = -((Config.Rows/2.0f*yInc) - yInc/2);

        for (int i = 0; i < Config.Cols; i++)
        {
            float yStartTmp = yStart;

            var currentTileCol = new TileCol();
            currentTileCol.Tiles = new List<Tile>();
            currentTileCol.ChallengeIndex = new List<int>();

            for (int j = 0; j < Config.Rows; j++)
            {
                var current =
                    Instantiate(TilePrefab, new Vector3(xStart, yStartTmp, 0), Quaternion.Euler(270, 0, 0)) as
                        GameObject;

                if (current != null)
                {
                    current.transform.localScale = new Vector3(Config.TileWidth/10, 1, Config.TileHeight/10);
                    current.transform.parent = _tileParent.transform;

                    current.name = "Tile-" + i + "x" + j;

                    var currentTile = new Tile();
                    currentTile.TileGo = current;
                    currentTile.soundIndex = j;

                    currentTile.Active = false;

                    currentTile.parentCol = currentTileCol;

                    currentTileCol.Tiles.Add(currentTile);
                }
                yStartTmp += yInc;
            }
            _matrix.Add(currentTileCol);

            xStart += xInc;
        }

        _activeCol = -1;
        _timerCol = 0;

        _beatCounter = 0;
        _matrixReady = true;
        //FieldWidth = Config.Cols*(Config.TileWidth + Config.TileSpaceing);
    }

    public void DestroyTiles()
    {
        _matrixReady = false;
        _matrix.Clear();

        for (int i = 0; i < _tileParent.transform.childCount; i++)
        {
            Destroy(_tileParent.transform.GetChild(i).gameObject);
        }
    }

    public void RebuildTiles()
    {
        DestroyTiles();
        //Config.LoadNextSong();
        BuildTiles();
    }

    public void LoadSongRandom(Songtype songType = Songtype.Freestyle)
    {
        var songRepo = new List<Song>();

        if (songType == Songtype.Freestyle)
        {
            songRepo = Config.FreestyleSongs;
        }
        else if (songType == Songtype.Challenge)
        {
            songRepo = Config.ChallengeSongs;
        }

        if (songRepo.Count < 1)
        {
            LoadSong(Songtype.Freestyle, 0);
        }
        else
        {
            LoadSong(songType, Random.Range(0, songRepo.Count - 1));
        }
    }

    private void UpdateNeworkXML()
    {
        var xml = new XmlDocument();
        xml.Load(_networkPath);

        //gets firs 
        XmlNodeList nodes = xml.GetElementsByTagName("NetworkSet");
        foreach (XmlNode element in nodes[0].ChildNodes)
        {
            switch (element.Name)
            {
                case "ChallengeMode":
                    element.InnerText = _networkSet.ChallengeMode.ToString();
                    break;

                case "DemoTime":
                    element.InnerText = _networkSet.DemoTime.ToString();
                    break;

                case "Song":
                    element.InnerText = _networkSet.Song.Title;
                    break;
                case "Length":
                    element.InnerText = _networkSet.Song.Length.ToString();
                    break;
                case "Points":
                    element.InnerText = _networkSet.Song.Points.ToString();
                    break;
                default:
                    Debug.Log("Illegal element");
                    break;
            } //end switch
        } //end foreach


        xml.Save(_networkPath);
    }

    public void LoadSong(Songtype songType = Songtype.Challenge, int num = 0)
    {
        var songRepo = new List<Song>();

        if (songType == Songtype.Freestyle)
        {
            _currenGuiFunction = null;

            for (int i = 0; i < Config.Cols; i++)
                for (int j = 0; j < Config.Rows; j++)
                    _matrix[i].Tiles[j].TileGo.GetComponent<TileBehaviour>().ResetTexture();

            songRepo = Config.FreestyleSongs;

            if (num < songRepo.Count)
                Config.CurrentGamemode = Gamemode.Freestyle;

            _networkSet.ChallengeMode = false;
            float bmp = songRepo[num].Bpm;
            float duration = songRepo[num].PreheatDuration*(60/bmp);
            _networkSet.DemoTime = duration;
            _networkSet.Song.Title = songRepo[num].Titel;

            _networkSet.Song.Length = 0;
            _networkSet.Song.Points = 0;
        }
        else if (songType == Songtype.Challenge)
        {
            _countdownTexTimer = 0;

            _countdownTexture.Stop();
            _countdownTexture.Play();

            GetComponent<AudioSource>().Stop();
            GetComponent<AudioSource>().Play();

            _currenGuiFunction = CountdownGUI;

            songRepo = Config.ChallengeSongs;
            if (num < songRepo.Count)
                Config.CurrentGamemode = Gamemode.Challenge;

            _networkSet.ChallengeMode = true;
            float bmp = songRepo[num].Bpm;
            float duration = songRepo[num].PreheatDuration*(60/bmp);
            Debug.Log(duration);
            _networkSet.DemoTime = duration;
            _networkSet.Song.Title = songRepo[num].Titel;
            _networkSet.Song.Length = 0;
            //Will be replace by the actual length of thesong, once itios loaded (after Coundown is over)
            _networkSet.Song.Points = 0;
        }

        UpdateNeworkXML();

        if (num < songRepo.Count)
        {
            if (_tempGameObjects.Count > 0)
            {
                foreach (GameObject tgo in _tempGameObjects)
                {
                    Destroy(tgo);
                }
            }

            var go = new GameObject();
            go.name = songRepo[num].Titel;
            go.tag = "Song";
            go.transform.parent = _tempParent.transform;
            go.AddComponent<AudioSource>();
            go.AddComponent<AudioClipLoader>().Url = songRepo[num].SoundFilePath;

            _tempGameObjects.Add(go);

            var goTileSounds = new GameObject();
            goTileSounds.name = "TileSounds";
            goTileSounds.transform.parent = _tempParent.transform;
            goTileSounds.transform.position = goTileSounds.transform.parent.position;

            _tempGameObjects.Add(goTileSounds);

            for (int i = 0; i < songRepo[num].TileSoundFilePaths.Count; i++)
            {
                var tilesounds = new GameObject();
                tilesounds.name = i.ToString();
                tilesounds.transform.parent = goTileSounds.transform;
                tilesounds.AddComponent<AudioSource>();
                tilesounds.AddComponent<AudioClipLoader>().Url = songRepo[num].TileSoundFilePaths[i];

                _tempGameObjects.Add(tilesounds);
            }

            Config.PreheatDuration = songRepo[num].PreheatDuration;
            Config.PreheatShowAt = songRepo[num].PreheatShowAt;

            if (songType == Songtype.Freestyle)
            {
                go.GetComponent<AudioClipLoader>().Play(AudioPlayMode.Loop);
            }
            else if (songType == Songtype.Challenge)
            {
                var goTileFailSounds = new GameObject();
                goTileFailSounds.name = "TileFailSounds";
                goTileFailSounds.transform.parent = _tempParent.transform;
                goTileFailSounds.transform.position = goTileFailSounds.transform.parent.position;
                _tempGameObjects.Add(goTileFailSounds);

                for (int i = 0; i < songRepo[num].TileFailSoundFilePaths.Count; i++)
                {
                    var tileFailSounds = new GameObject();
                    tileFailSounds.name = i.ToString();
                    tileFailSounds.transform.parent = goTileFailSounds.transform;
                    tileFailSounds.AddComponent<AudioSource>();
                    tileFailSounds.AddComponent<AudioClipLoader>().Url = songRepo[num].TileFailSoundFilePaths[i];

                    _tempGameObjects.Add(tileFailSounds);
                }

                var tileSet = new int[songRepo[num].Tileset.Count + 2*Config.Cols*(Config.PreheatDuration/8) + 1];
                Array.Clear(tileSet, 0, tileSet.Length);
                songRepo[num].Tileset.CopyTo(tileSet, (Config.Cols*(Config.PreheatDuration/8)));

                foreach (TileCol mat in _matrix)
                    mat.ChallengeIndex.Clear();

                for (int i = 0; i < songRepo[num].Tileset.Count + (Config.PreheatDuration/8); i++)
                    _matrix[i%Config.Cols].ChallengeIndex.Add(tileSet[i]);
            }

            Config.BPM = songRepo[num].Bpm;

            Config.LightColor[0] = (byte) songRepo[num].LightColor[0];
            Config.LightColor[1] = (byte) songRepo[num].LightColor[1];
            Config.LightColor[2] = (byte) songRepo[num].LightColor[2];
            Config.LightColor[3] = (byte) songRepo[num].LightColor[3];

            _activeCol = -1;
            _timerCol = 0;
            _beatCounter = 0;
            _idleTimer = 0;
        }
    }

    public string GetSongTitle()
    {
        return _networkSet.Song.Title;
    }

    public void SetTileStatus(int col, int row, bool status)
    {
        lock (_matrix[col].Tiles)
        {
            Tile tile = _matrix[col].Tiles[row];
            tile.Active = status;
            _matrix[col].Tiles[row] = tile;

            if (status)
                _idleTimer = 0;
        }
    }

    /**
	 * Changes Texture and forceActive when in IdleMode
	 **/

    private void SetIdleBehaviour()
    {
        for (int i = 0; i < _footprintCount; i++)
        {
            int randRow = Random.Range(0, Config.Rows);
            int randColumn = Random.Range(0, Config.Cols);

            _matrix[randColumn].Tiles[randRow].TileGo.GetComponent<TileBehaviour>().SetFootprint();
        }
    }

    /**
	 * Searches for tiles with forceActive = true
	 * Removes Footprint Texture and sets forceActive to false when exiting IdleMode
	 **/

    private void RemoveIdleBehaviour()
    {
        for (int i = 0; i < Config.Cols; i++)
        {
            TileCol tileCol = _matrix[i];
            foreach (Tile tile in tileCol.Tiles)
            {
                if (tile.TileGo.GetComponent<TileBehaviour>().ForceActive)
                {
                    tile.TileGo.GetComponent<TileBehaviour>().ResetTexture();
                }
            } //end foreach
        } //end for
    }

    private void GetInputs()
    {
        if (Input.GetKeyDown(KeyCode.B))
            BuildTiles();

        if (Input.GetKeyDown(KeyCode.D))
            DestroyTiles();

        if (Input.GetKeyDown(KeyCode.T))
            RebuildTiles();

        if (Input.GetKeyDown(KeyCode.Alpha1))
            LoadSong(Songtype.Challenge, 0);

        if (Input.GetKeyDown(KeyCode.Alpha2))
            LoadSong(Songtype.Challenge, 1);

        if (Input.GetKeyDown(KeyCode.Alpha3))
            LoadSong(Songtype.Challenge, 2);

        if (Input.GetKeyDown(KeyCode.Alpha4))
            LoadSong(Songtype.Freestyle, 0);

        if (Input.GetKeyDown(KeyCode.Alpha5))
            LoadSong(Songtype.Freestyle, 1);

        if (Input.GetKeyDown(KeyCode.Alpha6))
            LoadSong(Songtype.Freestyle, 2);

        if (Input.GetKeyDown(KeyCode.Alpha7))
            LoadSong(Songtype.Freestyle, 3);

        if (Input.GetKeyDown(KeyCode.I))
            _idleTimer = 0;
    }

    private delegate void GUIFunction();
}
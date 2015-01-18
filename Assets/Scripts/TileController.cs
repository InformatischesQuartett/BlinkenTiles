using System;
using System.IO;
using System.Xml.Serialization;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class TileController : MonoBehaviour
{
    public GameObject TilePrefab;
    private GameObject _tileParent;
    private GameObject _tempParent;

    private float _timerCol;
    private int _activeCol;
    private int _previousActiveCol;
    private int _beatCounter;
    private int _killcounter;

    public float TimerField { get; private set; }
    public float FieldWidth { get; private set; }

    private bool _matrixReady;
    private List<TileCol> _matrix;

    private List<GameObject> _tempGameObjects;

    private LightController _lightController;

    private Texture2D[] _numbers = new Texture2D[9];

    private delegate void GUIFunction();
    private GUIFunction _currenGuiFunction;

	/*Timer start idle time and set up footprints after this certain time*/
	private float _idleTimer = 0.0f;
	/*Timer to reset idlemode footprints after this certain time*/
	private float _idleResetTimer = 0.0f;
	/*Determines how many footprints are allowed on the field*/
	private int _footprintCount = 4;


    private string _networkPath;
	// Use this for initialization
	void Start ()
	{
        _networkPath = Application.streamingAssetsPath + @"\Network";
        
	    LoadCoundtownTextures();
	    _currenGuiFunction = EmptyGUI;
	    _lightController = this.GetComponent<LightController>();
        _tileParent = GameObject.Find("Tiles");
        _tempParent = GameObject.Find("Temp");

        _matrix = new List<TileCol>();
        _tempGameObjects = new List<GameObject>();

	    _activeCol = 0;

        BuildTiles();
        LoadSong(Songtype.Challenge, 0);
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
                        if (_matrix[i].Tiles[j].Active || _matrix[i].Tiles[j].TileGo.GetComponent<TileBehaviour>().ForceActive)
                            _matrix[i].Tiles[j].TileGo.GetComponent<TileBehaviour>().Highlight = Highlighttype.Occupied;
                    }
                }

                //Shake 'n' Play pass
                for (int i = 0; i < Config.Rows; i++)
                {
                    if (_matrix[_activeCol].Tiles[i].Active || _matrix[_activeCol].Tiles[i].TileGo.GetComponent<TileBehaviour>().ForceActive)
                    {
                        _matrix[_activeCol].Tiles[i].TileGo.GetComponent<TileBehaviour>().Highlight = Highlighttype.Hit;
                        if (_activeCol != _previousActiveCol)
                        {
                            _matrix[_activeCol].Tiles[i].TileGo.GetComponent<TileBehaviour>().Shake();

                            var soundGo = GameObject.Find("Temp/TileSounds/" + _matrix[_activeCol].Tiles[i].soundIndex);
                            soundGo.GetComponent<AudioClipLoader>().Play(AudioPlayMode.Once);
                        }
                    }
                }

            }

            else if (Config.CurrentGamemode == Gamemode.Challenge)
            {
                if (_matrix[_activeCol].ChallengeIndex.Count == 0)
                    LoadSongRandom();

                //highlight pass
                for (int i = 0; i < Config.Cols; i++)
                {
                    for (int j = 0; j < Config.Rows; j++)
                    {
                        _matrix[i].Tiles[j].TileGo.GetComponent<TileBehaviour>().Highlight = Highlighttype.None;
                    }
                }

                //preview pass
                for (int i = 0; i < Config.Cols; i++)
                {
                    if (_matrix[i].ChallengeIndex.Count > 0)
                    {
                        var previewIndex = _matrix[i].ChallengeIndex[0] - 1;

                        if (previewIndex > Config.Rows)
                            previewIndex -= Config.Rows;

                        if (previewIndex >= 0)
                        {
                            _matrix[i].Tiles[previewIndex].TileGo.GetComponent<TileBehaviour>().Highlight = Highlighttype.Preview;
                        }
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
                        if (_matrix[i].Tiles[j].Active || _matrix[i].Tiles[j].TileGo.GetComponent<TileBehaviour>().ForceActive)
                            _matrix[i].Tiles[j].TileGo.GetComponent<TileBehaviour>().Highlight = Highlighttype.Occupied;
                    }
                }

                //Shake 'n' Play 'n' change preview pass
                for (int i = 0; i < Config.Rows; i++)
                {
                    var previewIndex = _matrix[_activeCol].ChallengeIndex[0] -1;

                    if (previewIndex > Config.Rows)
                        previewIndex -= Config.Rows;

                    if (previewIndex >= 0)
                    {
                        if (_matrix[_activeCol].Tiles[i].Active || _matrix[_activeCol].Tiles[i].TileGo.GetComponent<TileBehaviour>().ForceActive)
                        {
                            if (previewIndex == i)
                            {
                                _matrix[_activeCol].Tiles[i].TileGo.GetComponent<TileBehaviour>().Highlight = Highlighttype.Hit;
                                if (_activeCol != _previousActiveCol)
                                {
                                    _matrix[_activeCol].Tiles[i].TileGo.GetComponent<TileBehaviour>().Shake();

                                    var soundGo = GameObject.Find("Temp/TileSounds/" + _matrix[_activeCol].Tiles[i].soundIndex);
                                    soundGo.GetComponent<AudioClipLoader>().Play(AudioPlayMode.Once);
                                }
                            }
                            else
                            {
                                _matrix[_activeCol].Tiles[i].TileGo.GetComponent<TileBehaviour>().Highlight = Highlighttype.Fail;
                                if (_activeCol != _previousActiveCol)
                                {
                                    _matrix[_activeCol].Tiles[i].TileGo.GetComponent<TileBehaviour>().Shake();

                                    var soundGo = GameObject.Find("Temp/TileFailSounds/" + _matrix[_activeCol].Tiles[i].soundIndex);
                                    soundGo.GetComponent<AudioClipLoader>().Play(AudioPlayMode.Once);
                                }
                            }
                        }
                    }
                }

                //play song after countdown
                if (Config.CurrentGamemode == Gamemode.Challenge && _beatCounter == 9 && _activeCol != _previousActiveCol)
                {
                    GameObject.FindGameObjectWithTag("Song").audio.Play();
                }
            }
        }
    }

    void LateUpdate ()
    {
        if (_activeCol != _previousActiveCol && _activeCol >= 0)
        {
            if (_matrix[_activeCol].ChallengeIndex.Count > 0)
            {
                _matrix[_activeCol].ChallengeIndex.RemoveAt(0);
            }
        }
        _previousActiveCol = _activeCol;

    /**
		 * Check if IdleMode = true.
		 * If IdleMode is active, set footprintCount tiles footprint textures and set them forceActive.
		 **/
		if (!Config.IdleMode) {
			/*set IdleMode true if _idleTimer gets above IdleDelay value - set it to false if it falls below*/
			Config.IdleMode = _idleTimer > Config.IdleDelay;

			/*Set Idle Behaviour if IdleMode is activated --> creates footprints*/
			if (Config.IdleMode) {
				SetIdleBehaviour ();
				//reset idleResetTimer
				_idleResetTimer = 0.0f;
			}
		}

		if (Config.IdleMode) {
			/*set IdleMode true if _idleTimer gets above IdleDelay value - set it to false if it falls below*/
			Config.IdleMode = _idleTimer > Config.IdleDelay;
			if (!Config.IdleMode) {
				RemoveIdleBehaviour ();
			} else if (_idleResetTimer > Config.IdleResetDelay) {
				/*Resetting footprint positions after a certain amount of time*/
				RemoveIdleBehaviour ();
				SetIdleBehaviour();
				_idleResetTimer = 0.0f;
			}

		}



	}

    void FixedUpdate()
    {
		//IdleTimer
		_idleTimer+= Time.fixedDeltaTime;
		
		_idleResetTimer+= Time.fixedDeltaTime;

        if (_timerCol > 60 / Config.BPM)
        {
            _timerCol = (_timerCol - (60 / Config.BPM)) + Time.fixedDeltaTime;
            _activeCol++;
            _beatCounter++;
            if (_activeCol >= Config.Cols)
                _activeCol = 0;

            if (_beatCounter > 8)
            {
                _currenGuiFunction = EmptyGUI;
            }
        }
        else
        {
            _timerCol += Time.fixedDeltaTime;
        }

        if (_activeCol >= 0)
            _lightController.UpdateFaderValues(_activeCol, _timerCol);
    }

    private void LoadCoundtownTextures()
    {
        for (int j = 0,  i = 8; i >= 0; j++,i--)
        {
            _numbers[j] = Resources.Load<Texture2D>("Textures/Countdown_" + (i));
        }
        _numbers[8] = Resources.Load<Texture2D>("Textures/Countdown_GO");
    }

    private void OnGUI()
    {
        _currenGuiFunction();
    }

    private void EmptyGUI()
    {
    }

    private void CountdownGUI()
    {
        GUI.backgroundColor = new Color(1, 1, 1, 0.5f);
        GUI.Box(new Rect(Screen.width/2f, 0, Screen.width/2f, Screen.height), _numbers[_beatCounter]);
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

            TileCol currentTileCol = new TileCol();
            currentTileCol.Tiles = new List<Tile>();
            currentTileCol.ChallengeIndex = new List<int>();

            for (int j = 0; j < Config.Rows; j++)
            {
                GameObject current = Instantiate(TilePrefab, new Vector3(xStart, yStartTmp, 0), Quaternion.Euler(270, 0, 0)) as GameObject;

                if (current != null)
                {
                    current.transform.localScale = new Vector3(Config.TileWidth/10, 1, Config.TileHeight/10);
                    current.transform.parent = _tileParent.transform;

                    current.name = "Tile-" + i + "x" + j;

                    Tile currentTile = new Tile();
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
        List<Song> songRepo = new List<Song>();

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

    

    
    public void LoadSong(Songtype songType=Songtype.Challenge, int num=0)
    {

        XmlSerializer _writer = new XmlSerializer(typeof(NetworkSet));
        NetworkSet networkSet = new NetworkSet();

        List<Song> songRepo = new List<Song>();

        if (songType == Songtype.Freestyle)
        {
            networkSet.ChallengeMode = false;
            using (FileStream file = File.OpenWrite(_networkPath + @"\network.xml"))
            {
                _writer.Serialize(file, networkSet);
            }
            songRepo = Config.FreestyleSongs;
            if (num < songRepo.Count)
                Config.CurrentGamemode = Gamemode.Freestyle;
        }
        else if (songType == Songtype.Challenge)
        {
            networkSet.ChallengeMode = true;
            using (FileStream file = File.OpenWrite(_networkPath + @"\network.xml"))
            {
                _writer.Serialize(file, networkSet);
            }

            _currenGuiFunction = CountdownGUI;
            songRepo = Config.ChallengeSongs;
            if (num < songRepo.Count)
                Config.CurrentGamemode = Gamemode.Challenge;
        }

        if (num < songRepo.Count)
        {
            if (_tempGameObjects.Count > 0)
            {

                foreach (var tgo in _tempGameObjects)
                {
                    Destroy(tgo);
                }
            }

            var go = new GameObject();
            go.name = songRepo[num].Titel;
            go.tag = "Song";
            go.transform.parent = _tempParent.transform;
            go.AddComponent<AudioSource>();
            go.AddComponent<AudioClipLoader>().url = songRepo[num].SoundFilePath;

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
                tilesounds.AddComponent<AudioClipLoader>().url = songRepo[num].TileSoundFilePaths[i];

                _tempGameObjects.Add(tilesounds);
            }

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
                    tileFailSounds.AddComponent<AudioClipLoader>().url = songRepo[num].TileFailSoundFilePaths[i];

                    _tempGameObjects.Add(tileFailSounds);
                }

                int[] bla = new int[songRepo[num].Tileset.Count + 2*Config.Cols + 1];
                Array.Clear(bla, 0, bla.Length);
                songRepo[num].Tileset.CopyTo(bla, Config.Cols + 1);

                for (int i = 0; i < songRepo[num].Tileset.Count; i++)
                {
                    _matrix[i % Config.Cols].ChallengeIndex.Add(bla[i]);
                }
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

	public void SetTileStatus(int col, int row, bool status)
	{
	    lock (_matrix[col].Tiles)
	    {
	        var tile = _matrix[col].Tiles[row];
	        tile.Active = status;
	        _matrix[col].Tiles[row] = tile;
	    }
	}

	/**
	 * Changes Texture and forceActive when in IdleMode
	 **/ 
	private void SetIdleBehaviour() {
		for (int i = 0; i < _footprintCount; i++)
		{
		    var randRow = Random.Range(0, Config.Rows);
		    var randColumn = Random.Range(0, Config.Cols);

		    _matrix[randColumn].Tiles[randRow].TileGo.GetComponent<TileBehaviour>().SetFootprint();
		}
	}

	/**
	 * Searches for tiles with forceActive = true
	 * Removes Footprint Texture and sets forceActive to false when exiting IdleMode
	 **/ 
	private void RemoveIdleBehaviour () {
		for (int i = 0; i < Config.Cols; i++) {
			TileCol tileCol = _matrix [i];
			foreach (Tile tile in tileCol.Tiles) {
				if (tile.TileGo.GetComponent<TileBehaviour> ().ForceActive) {
					tile.TileGo.GetComponent<TileBehaviour> ().ResetFootprint ();
				}
			}//end foreach
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

        if (Input.GetKeyDown(KeyCode.Alpha0))
            LoadSong(Songtype.Freestyle, 0);

        if (Input.GetKeyDown(KeyCode.I))
            _idleTimer = 0;
    }
}
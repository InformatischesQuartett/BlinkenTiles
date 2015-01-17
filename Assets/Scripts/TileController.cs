using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
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

    public float TimerField { get; private set; }
    public float FieldWidth { get; private set; }

    private bool _matrixReady;
    private List<TileCol> _matrix;

    private List<GameObject> _tempGameObjects;

    private LightController _lightController;

    private Texture2D[] Numbers = new Texture2D[9];

    private delegate void GUIFunction();

    private GUIFunction _currenGuiFunction;

	private float _idleTimer = 0.0f;
	/*Determines how many footprints are allowed on the field*/
	private int _footprintCount = 4;

	// Use this for initialization
	void Start ()
	{
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
	void Update ()
	{
        GetInputs();
	   
        if (_matrixReady)
        {
            for (int i = 0; i < Config.Cols; i++)
            {
                TileCol tileCol = _matrix[i];
                foreach (Tile tile in tileCol.Tiles)
                {
                    TileBehaviour tileScript = tile.TileGo.GetComponent<TileBehaviour>();
                    if (i == _activeCol)
                    {
                        if (tile.Active || tileScript.ForceActive)
                        {
                            tileScript.Highlight = Highlighttype.Hit;
                            if (_activeCol != _previousActiveCol)
                            {
                                tileScript.Shake();

                                var soundGo = GameObject.Find("Temp/TileSounds/" + tile.soundIndex);
                                soundGo.GetComponent<AudioClipLoader>().Play(AudioPlayMode.Once);
                            }
                        }
                        else
                        {
                            tileScript.Highlight = Highlighttype.Time;
                        }
                    }
                    else
                    {
                        if (tile.Active || tileScript.ForceActive)
                        {
                            tileScript.Highlight = Highlighttype.Occupied;
							if (tile.Active) {
								_idleTimer = 0.0f;
							}
                        }
                        else
                        {
                            tileScript.Highlight = Highlighttype.None;
                        }
                    }

                    if (Config.CurrentGamemode == Gamemode.Challenge && _beatCounter == 9 && _activeCol != _previousActiveCol)
                    {
                        GameObject.FindGameObjectWithTag("Song").audio.Play();
                    }
                }
            }
            _previousActiveCol = _activeCol;
            
        }

		/*set IdleMode true if _idleTimer gets above IdleDelay value - set it to false if it falls below*/
		Config.IdleMode = _idleTimer > Config.IdleDelay;
		
		/**
		 * If IdleMode is active, set footprintCount tiles footprint textures and set them forceActive
		 **/
		if (Config.IdleMode && _footprintCount > 0) {
			SetIdleBehaviour ();
		}
		if (!Config.IdleMode) {
			ResetIdleBehaviour ();
		}

	}

    void FixedUpdate()
    {
		//IdleTimer
		_idleTimer+= Time.fixedDeltaTime;

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
            //int n = i + 1;
            //string tex = "Textures/Countdown_" + n;
            Numbers[j] = Resources.Load<Texture2D>("Textures/Countdown_" + (i));
        }
        Numbers[8] = Resources.Load<Texture2D>("Textures/Countdown_GO");
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
         GUI.Box(new Rect(Screen.width/2, 0, Screen.width/2, Screen.height), Numbers[_beatCounter]);
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

    public void LoadSongRandom(Songtype songtype = Songtype.Freestyle)
    {
            
    }


    public void LoadSong(Songtype songType=Songtype.Challenge, int num=0)
    {
        List<Song> songRepo = new List<Song>();

        if (songType == Songtype.Freestyle)
        {
            songRepo = Config.FreestyleSongs;
            if (num < songRepo.Count)
                Config.CurrentGamemode = Gamemode.Freestyle;
        }
        else if (songType == Songtype.Challenge)
        {
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

                for (int i = 0; i < songRepo[num].Tileset.Count; i++)
                {
                    _matrix[i % Config.Cols].ChallengeIndex.Add(songRepo[num].Tileset[i]);
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
	private void SetIdleBehaviour () {
		Debug.Log ("I am setting a footprint tex");
		var randRow = Random.Range (0,Config.Rows);
		var randColumn = Random.Range(0, Config.Cols);

		//_matrix [2].Tiles [3].TileGo.GetComponent<TileBehaviour> ().handleFootprintTexture ();
		_matrix [randColumn].Tiles [randRow].TileGo.GetComponent<TileBehaviour> ().handleFootprintTexture ();

		_footprintCount--;
	}

	/**
	 * Removes Footprint Texture and sets forceActive to false when exiting IdleMode
	 **/ 
	private void ResetIdleBehaviour () {
		for (int i = 0; i < Config.Cols; i++) {
			TileCol tileCol = _matrix [i];
			foreach (Tile tile in tileCol.Tiles) {
				if (tile.TileGo.GetComponent<TileBehaviour> ().ForceActive) {
					tile.TileGo.GetComponent<TileBehaviour> ().ForceActive = false;
					Debug.Log("I am resetting the textures");
				}
			}//end foreach
		} //end for
		//for each tiles in matrix forceActive
		//forceActive = false;
		//remove texture
		_footprintCount = 4; //todo: make dynamic
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
        {
            LoadSong(Songtype.Freestyle, 0);
        }
    }
}
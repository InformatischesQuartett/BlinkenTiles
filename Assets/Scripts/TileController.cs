﻿using UnityEngine;
using System.Collections.Generic;

public class TileController : MonoBehaviour
{
    public GameObject TilePrefab;
    private GameObject _tileParent;
    private GameObject _tempParent;

    private float _timerCol;
    private int _activeCol;
    private int _previousActiveCol;

    public float TimerField { get; private set; }
    public float FieldWidth { get; private set; }

    private bool _matrixReady;
    private List<TileCol> _matrix;

    private DmxController _dmxControllerScript;

    private List<GameObject> _tempGameObjects; 

	// Use this for initialization
	void Start ()
	{
	    _dmxControllerScript = gameObject.GetComponent<DmxController>();
        _tileParent = GameObject.Find("Tiles");
        _tempParent = GameObject.Find("Temp");

        _matrix = new List<TileCol>();
        _tempGameObjects = new List<GameObject>();

	    _activeCol = 0;

        LoadSong();
        BuildTiles();
    }
	
	// Update is called once per frame
	void Update ()
	{
        if (Input.GetKeyDown(KeyCode.B))
            BuildTiles();

        if (Input.GetKeyDown(KeyCode.D))
            DestroyTiles();

        if (Input.GetKeyDown(KeyCode.T))
            RebuildTiles();

        if (Input.GetKeyDown(KeyCode.Alpha0))
            LoadSong(0);

	    if (_timerCol > 60/Config.BPM)
	    {
	        _timerCol = (_timerCol - (60/Config.BPM)) + Time.deltaTime;
	        _activeCol++;
	        if (_activeCol >= Config.Cols)
	            _activeCol = 0;
	    }
	    else
	    {
	        _timerCol += Time.deltaTime;
	    }

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
                        if (tile.Active)
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
                        if (tile.Active)
                        {
                            tileScript.Highlight = Highlighttype.Occupied;
                        }
                        else
                        {
                            tileScript.Highlight = Highlighttype.None;
                        }
                    }
                }
            }
            _previousActiveCol = _activeCol;
        }

	    TimerField = (_activeCol*60/Config.BPM) + _timerCol;
	    _dmxControllerScript.TimeReference = TimerField;

        _dmxControllerScript.Tick();
	}

    public void BuildTiles()
    {
        float xInc = (Config.TileWidth + Config.TileSpaceing);
        float yInc = (Config.TileHeight + Config.TileSpaceing);
        float xStart = -((Config.Cols/2*xInc) - xInc/2);
        float yStart = -((Config.Rows/2*yInc) - yInc/2);

        for (int i = 0; i < Config.Cols; i++)
        {
            float yStartTmp = yStart;

            TileCol currentTileCol = new TileCol();
            currentTileCol.XMin = xStart - Config.TileWidth/2;
            currentTileCol.XMax = xStart + Config.TileWidth/2;
            currentTileCol.Tiles = new List<Tile>();

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
                    currentTile.Bounds = new Rect(currentTileCol.XMin, yStartTmp - Config.TileHeight/2, Config.TileWidth, Config.TileHeight);

					currentTile.Active = false;
                    currentTileCol.Tiles.Add(currentTile);
                }
                yStartTmp += yInc;
            }

            _matrix.Add(currentTileCol);
            
            xStart += xInc;
        }

        _activeCol = 0;
        _timerCol = 0;
        _matrixReady = true;
        FieldWidth = Config.Cols*(Config.TileWidth + Config.TileSpaceing);
        _dmxControllerScript.FieldSize = FieldWidth;
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

    public void LoadSong(int num=0)
    {
        if (_tempGameObjects.Count > 0)
        {
            foreach (var tgo in _tempGameObjects)
            {
                Destroy(tgo);
            }
        }

        var go = new GameObject();
        go.name = "Song";
        go.transform.parent = _tempParent.transform;
        go.AddComponent<AudioSource>();
        go.AddComponent<AudioClipLoader>().url = Config.ChallengeSongs[0].SoundFilePath;

        _tempGameObjects.Add(go);

        var goTileSounds = new GameObject();
        goTileSounds.name = "TileSounds";
        goTileSounds.transform.parent = _tempParent.transform;

        _tempGameObjects.Add(goTileSounds);

        for (int i = 0; i < Config.ChallengeSongs[num].TileSoundFilePaths.Count; i++)
        {
            var tilesounds = new GameObject();
            tilesounds.name = i.ToString();
            tilesounds.transform.parent = goTileSounds.transform;
            tilesounds.AddComponent<AudioSource>();
            tilesounds.AddComponent<AudioClipLoader>().url = Config.ChallengeSongs[num].TileSoundFilePaths[i];

            _tempGameObjects.Add(tilesounds);
        }

        go.GetComponent<AudioClipLoader>().Play(AudioPlayMode.Loop);

        Config.BPM = Config.ChallengeSongs[num].Bpm;

        _activeCol = 0;
        _timerCol = 0;
    }

	public void SetTileStatus(int col, int row, bool status)
	{
		var tile = _matrix[col].Tiles[row];
		tile.Active = status;
		_matrix[col].Tiles[row] = tile;
	}
}
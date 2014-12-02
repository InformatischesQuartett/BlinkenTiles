using UnityEngine;
using System.Collections.Generic;

public class TileController : MonoBehaviour
{

    public GameObject TilePrefab;
    private GameObject _tileParent;
    public GameObject TileDmxPrefab;
    private GameObject _tileDmxParent;
    public GameObject _referencePrefab;
    private GameObject _reference;

    private List<TileCol> _matrix;

	// Use this for initialization
	void Start ()
	{
        _tileParent = GameObject.Find("Tiles");
        _tileDmxParent = GameObject.Find("DMX");

	    BuildTiles();

	    float bla = Config.Cols/2*(Config.TileWidth + Config.TileSpaceing);
        _reference = Instantiate(_referencePrefab, new Vector3(-bla, 0, 0), Quaternion.identity) as GameObject;
	    _reference.name = "Reference";
        _reference.GetComponent<ReferenceBehaviour>().Init(-bla, bla);
	}
	
	// Update is called once per frame
	void Update ()
	{
	    foreach (TileCol tileCol in _matrix)
	    {
	        foreach (Tile tile in tileCol.tiles)
	        {
	            tile.tile.GetComponent<TileBehaviour>().Highlight = Helper.Between(_reference.transform.position.x, tileCol.xMin, tileCol.xMax);
	        }
	    }
	}

    public void BuildTiles()
    {
        float xInc = (Config.TileWidth + Config.TileSpaceing);
        float yInc = (Config.TileHeight + Config.TileSpaceing);
        float xStart = -((Config.Cols/2*xInc) - xInc/2);
        float yStart = -((Config.Rows/2*yInc) - yInc/2);

        _matrix = new List<TileCol>();

        //Debug.Log(xStart + " : " + yStart + " | " + xInc + " : " + yInc);

        for (int i = 0; i < Config.Cols; i++)
        {
            float yStartTmp = yStart;

            TileCol currentTileCol = new TileCol();
            currentTileCol.xMin = xStart - Config.TileWidth/2;
            currentTileCol.xMax = xStart + Config.TileWidth/2;
            currentTileCol.tiles = new List<Tile>();

            for (int j = 0; j < Config.Rows; j++)
            {
                GameObject current = Instantiate(TilePrefab, new Vector3(xStart, yStartTmp, 0), Quaternion.Euler(270, 0, 0)) as GameObject;

                if (current != null)
                {
                    current.transform.localScale = new Vector3(Config.TileWidth/10, 1, Config.TileHeight/10);
                    current.transform.parent = _tileParent.transform;

                    current.name = "Tile-" + i + "x" + j;

                    Tile currentTile = new Tile();
                    currentTile.tile = current;
                    currentTile.bounds = new Rect(currentTileCol.xMin, yStartTmp + Config.TileHeight/2, Config.TileWidth, Config.TileHeight);
                    currentTile.triggerBounds = new Rect(currentTile.bounds.xMin + Config.TileTriggerOffset, currentTile.bounds.yMin + Config.TileTriggerOffset, Config.TileWidth - Config.TileTriggerOffset*2, Config.TileHeight - Config.TileTriggerOffset*2);

                    currentTileCol.tiles.Add(currentTile);
                }
                yStartTmp += yInc;
            }

            _matrix.Add(currentTileCol);

            GameObject currentDMX = Instantiate(TileDmxPrefab, new Vector3(xStart, yStartTmp + 1, 0), Quaternion.identity) as GameObject;
            currentDMX.transform.parent = _tileDmxParent.transform;
            currentDMX.name = "TileBank-" + i;

            xStart += xInc;
        }
    }
}

struct TileCol
{
    public float xMin;
    public float xMax;

    public List<Tile> tiles;
}

struct Tile
{
    public Rect bounds;
    public Rect triggerBounds;

    public GameObject tile;
}

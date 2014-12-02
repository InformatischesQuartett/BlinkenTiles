using UnityEngine;
using System.Collections.Generic;

public class TileController : MonoBehaviour
{

    public GameObject TilePrefab;
    private GameObject _tileParent;
    public GameObject TileDmxPrefab;
    private GameObject _tileDmxParent;
    public GameObject ReferencePrefab;
    private GameObject _reference;
    private GameObject _peopleParent;

    private List<TileCol> _matrix;

	// Use this for initialization
	void Start ()
	{
        _tileParent = GameObject.Find("Tiles");
        _tileDmxParent = GameObject.Find("DMX");
	    _peopleParent = GameObject.Find("People");

        _reference = Instantiate(ReferencePrefab, new Vector3(Config.Cols / 2 * (Config.TileWidth + Config.TileSpaceing), 0, 0), Quaternion.identity) as GameObject;
	    _reference.name = "Reference";

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


	    foreach (TileCol tileCol in _matrix)
	    {
	        foreach (Tile tile in tileCol.Tiles)
	        {
	            TileBehaviour tileScript = tile.TileGo.GetComponent<TileBehaviour>();
	            if (Helper.Between(_reference.transform.position.x, tileCol.XMin, tileCol.XMax))
	            {
	                for (int i = 0; i < _peopleParent.transform.childCount; i++)
	                {
	                    Transform child = _peopleParent.transform.GetChild(i);
	                    if (tile.TriggerBounds.Contains(child.position))
	                    {
	                        tileScript.Highlight = Highlighttype.Hit;
                            tileScript.Shake();
	                        break;
	                    }
	                    tileScript.Highlight = Highlighttype.Time;
	                }
	            }
	            else
	            {
	                tileScript.Highlight = Highlighttype.None;
	            }
	        }
	    }
	}

    public void BuildTiles()
    {
        float xInc = (Config.TileWidth + Config.TileSpaceing);
        float yInc = (Config.TileHeight + Config.TileSpaceing);
        float xStart = -((Config.Cols/2*xInc) - xInc/2);
        float yStart = -((Config.Rows/2*yInc) - yInc/2);

        _reference.GetComponent<ReferenceBehaviour>().Init(-Config.Cols / 2 * (Config.TileWidth + Config.TileSpaceing), Config.Cols / 2 * (Config.TileWidth + Config.TileSpaceing));

        _matrix = new List<TileCol>();

        //Debug.Log(xStart + " : " + yStart + " | " + xInc + " : " + yInc);

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
                    currentTile.Bounds = new Rect(currentTileCol.XMin, yStartTmp - Config.TileHeight/2, Config.TileWidth, Config.TileHeight);
                    currentTile.TriggerBounds = new Rect(currentTile.Bounds.xMin + Config.TileTriggerOffset, currentTile.Bounds.yMin + Config.TileTriggerOffset, Config.TileWidth - Config.TileTriggerOffset*2, Config.TileHeight - Config.TileTriggerOffset*2);

                    currentTileCol.Tiles.Add(currentTile);
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

    public void DestroyTiles()
    {
        _matrix.Clear();

        for (int i = 0; i < _tileParent.transform.childCount; i++)
        {
            Destroy(_tileParent.transform.GetChild(i).gameObject);
        }
        for (int i = 0; i < _tileDmxParent.transform.childCount; i++)
        {
            Destroy(_tileDmxParent.transform.GetChild(i).gameObject);
        }
    }

    public void RebuildTiles()
    {
        DestroyTiles();
//Config changes here
        BuildTiles();
    }
}
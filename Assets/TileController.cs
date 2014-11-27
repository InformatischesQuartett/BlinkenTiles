using UnityEngine;
using System.Collections;

public class TileController : MonoBehaviour
{

    public GameObject TilePrefab;
    private GameObject TileParent;

	// Use this for initialization
	void Start ()
	{
	    TileParent = GameObject.Find("Tiles");

	    BuildTiles();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void BuildTiles()
    {
        float xInc = (Config.TileWidth + Config.TileSpaceing) / 10;
        float yInc = (Config.TileHeight + Config.TileSpaceing) / 10;
        float xStart = -((Config.Cols/2*xInc) - xInc/2);
        float yStart = -((Config.Rows/2*yInc) - yInc/2);

        Debug.Log(xStart + " : " + yStart + " | " + xInc + " : " + yInc);

        for (int i = 0; i < Config.Cols; i++)
        {
            float yStartTmp = yStart;
            for (int j = 0; j < Config.Rows; j++)
            {
                GameObject current = Instantiate(TilePrefab, new Vector3(xStart, yStartTmp, 0), Quaternion.Euler(270, 0, 0)) as GameObject;
                
                current.transform.localScale = new Vector3(Config.TileWidth / 100, 1, Config.TileHeight / 100);
                current.transform.parent = TileParent.transform;

                current.name = "Tile-" + i + "x" + j;

                current.GetComponent<TileBehaviour>().SetTilePosition(i, j);

                yStartTmp += yInc;
            }
            xStart += xInc;
        }
    }

    public bool RegisterTile(int col, int row)
    {
        if (col >= 0 && col < Config.Cols && row >= 0 && row < Config.Rows)
        {
            Debug.Log("Tile registered.");
            return true;
        }
        Debug.LogError("Tile NOT registered!");
        return false;
    }
}

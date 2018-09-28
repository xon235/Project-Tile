using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TilePreviewScript : MonoBehaviour {

    public GameObject tilePrefab;
    public GameObject tileHolder;
    public float tileScale;
    public float tileOffset;
    public float scrollTime;
    public float scrollMinDiff;

    private float tileWidth;
    private List<GameObject> tiles = new List<GameObject>();
    private Vector3 targetPosition;

    private int currentTileIndex = 0;
    public int CurrentTileIndex
    {
        get
        {
            return currentTileIndex;
        }

        set
        {
            currentTileIndex = value;
            targetPosition.x = -tileWidth * currentTileIndex;
        }
    }

    void Start ()
    {
        targetPosition = transform.position;
        tileWidth = tilePrefab.GetComponent<Renderer>().bounds.size.x;

        InitTiles(1);
    }

    private void InitTiles(int seed)
    {
        Random.InitState(seed);

        int lastlastRandom, lastRandom, currentRandom;
        lastlastRandom = lastRandom = currentRandom = -1;

        for (int i = 0; i < 100; i++)
        {
            GameObject tile = Instantiate(
                tilePrefab,
                transform.position + new Vector3(i * tileOffset, 0, 0),
                Quaternion.identity,
                tileHolder.transform);

            tile.transform.localScale *= tileScale;
            tiles.Add(tile);

            currentRandom = Random.Range(0, (int)ColorName.COUNT - 1);
            while(lastlastRandom == lastRandom && lastRandom == currentRandom)
                currentRandom = Random.Range(0, (int)ColorName.COUNT - 1);

            tile.GetComponent<TileScript>().InitTile(
                GameManagerScript.GetTileColor((ColorName)currentRandom),
                true);

            lastlastRandom = lastRandom;
            lastRandom = currentRandom;
        }
    }
	
	// Update is called once per frame
	void Update () {
        if (Input.GetMouseButtonDown(0))
        {
            CurrentTileIndex += 1;
        } else if (Input.GetMouseButtonDown(2))
        {
            CurrentTileIndex = 0;
        } else if (Input.GetMouseButtonDown(1))
        {
            CurrentTileIndex -= 1;
        }

        if (tileHolder.transform.position != targetPosition)
        {
            float speed = (tileHolder.transform.position - targetPosition).magnitude / scrollTime;
            tileHolder.transform.position = Vector3.MoveTowards(tileHolder.transform.position, targetPosition, speed * Time.deltaTime);
        }

        if((tileHolder.transform.position - targetPosition).magnitude <= scrollMinDiff)
        {
            tileHolder.transform.position = targetPosition;
        }

        for(int i = CurrentTileIndex; i >= 0; i--)
        {
            if (tiles[i].GetComponent<TileScript>().IsTileEnabled == false)
                break;

            tiles[i].GetComponent<TileScript>().IsTileEnabled = false;
        }

        for (int i = CurrentTileIndex; i < 100; i++)
        {
            if (tiles[i].GetComponent<TileScript>().IsTileEnabled == true)
                break;

            tiles[i].GetComponent<TileScript>().IsTileEnabled = true;
        }
    }
}

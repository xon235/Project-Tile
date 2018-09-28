using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TilePreviewScript : MonoBehaviour {

    public GameObject tilePrefab;
    public GameObject tileHolder;
    public float tileScale;
    public float tileOffset;
    public float smoothTime;

    private float tileWidth;
    private List<GameObject> tiles = new List<GameObject>();
    private Vector3 targetPosition;
    private Vector3 tileHolderVelocity;

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
        tileWidth = tilePrefab.GetComponent<Renderer>().bounds.size.x;
        targetPosition = transform.position;
        tileHolderVelocity = Vector3.zero;

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
	
	void Update ()
    {
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
            tileHolder.transform.position = Vector3.SmoothDamp(tileHolder.transform.position, targetPosition, ref tileHolderVelocity, smoothTime);
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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TilePreviewScript : MonoBehaviour {

    public GameObject tilePrefab;
    public GameObject tileHolder;
    public float tileScale;
    public float tileOffset;
    public float smoothTime;
    public float selectScale;
    public float selectOffset;

    private float tileWidth;
    private List<TileScript> tiles = new List<TileScript>();
    private Vector3 targetPosition;
    private Vector3 tileHolderVelocity;
    private int bufferTileIndex = 0;

    private int _currentTileIndex = 0;
    public int CurrentTileIndex
    {
        get
        {
            return _currentTileIndex;
        }

        set
        {
            Vector3 pos = tiles[CurrentTileIndex].transform.localPosition;
            pos.y = 0;
            tiles[CurrentTileIndex].transform.localPosition = pos;
            tiles[CurrentTileIndex].transform.localScale = Vector3.one * 2;

            _currentTileIndex = value;

            pos = tiles[CurrentTileIndex].transform.localPosition;
            pos.y = selectOffset;
            tiles[CurrentTileIndex].transform.localPosition = pos;
            tiles[CurrentTileIndex].transform.localScale = Vector3.one * 2 * selectScale;
            targetPosition.x = -tileWidth * _currentTileIndex;
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
            TileScript tile = Instantiate(
                tilePrefab,
                transform.position + new Vector3(i * tileOffset, 0, 0),
                Quaternion.identity,
                tileHolder.transform).GetComponent<TileScript>();

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

        CurrentTileIndex = 0;
    }
	
	void Update ()
    {

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

    public ColorName GetCurrentTileColor()
    {
        return tiles[CurrentTileIndex].ColorName;
    }

    public void FlushBuffer()
    {
        bufferTileIndex = CurrentTileIndex;
    }

    public void ResetBuffer()
    {
        CurrentTileIndex = bufferTileIndex;
    }
}

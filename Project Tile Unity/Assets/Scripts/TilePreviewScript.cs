using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TilePreviewScript : MonoBehaviour
{
    public GameObject tilePrefab;
    public GameObject tileHolder;
    public float tileScale;
    public float tileOffset;
    public float smoothTime;

    public bool IsFinished { get; private set; }

    private int numOfPreviewTiles;
    private int previewSeed;
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
            _currentTileIndex = value;
            targetPosition.x = transform.position.x -tileWidth * _currentTileIndex;
        }
    }

    public void InitTilePreview(int numOfPreviewTiles, int previewSeed)
    {
        this.numOfPreviewTiles = numOfPreviewTiles;
        this.previewSeed = previewSeed;
    }

    void Start ()
    {
        tileWidth = tilePrefab.GetComponent<Renderer>().bounds.size.x;
        targetPosition =transform.position;
        tileHolderVelocity = Vector3.zero;

        InitTiles(previewSeed);
    }

    private void InitTiles(int seed)
    {
        IsFinished = false;
        Random.InitState(seed);

        int lastlastRandom, lastRandom, currentRandom;
        lastlastRandom = lastRandom = currentRandom = -1;

        for (int i = 0; i < numOfPreviewTiles; i++)
        {
            TileScript tile = Instantiate(
                tilePrefab,
                tileHolder.transform.position + new Vector3(i * tileOffset, 0, 0),
                Quaternion.identity,
                tileHolder.transform).GetComponent<TileScript>();

            tile.transform.localScale *= tileScale;
            tiles.Add(tile);

            currentRandom = Random.Range(0, (int)ColorName.COUNT);
            while(lastlastRandom == lastRandom && lastRandom == currentRandom)
                currentRandom = Random.Range(0, (int)ColorName.COUNT);

            tile.GetComponent<TileScript>().InitTile(
                GameManagerScript.Instance.GetTileColor((ColorName)currentRandom),
                true, 
                GameManagerScript.Instance.GetTilePoint());

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

        for(int i = CurrentTileIndex-1; i >= 0; i--)
        {
            if (tiles[i].GetComponent<TileScript>().IsTileEnabled == false)
                break;

            tiles[i].GetComponent<TileScript>().IsTileEnabled = false;
        }

        for (int i = CurrentTileIndex; i < numOfPreviewTiles; i++)
        {
            if (tiles[i].GetComponent<TileScript>().IsTileEnabled == true)
                break;

            tiles[i].GetComponent<TileScript>().IsTileEnabled = true;
        }
    }

    public ColorName GetCurrentTileColor()
    {
        try
        {
            return tiles[CurrentTileIndex].ColorName;
        }
        catch
        {
            IsFinished = true;
            throw;
        }
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

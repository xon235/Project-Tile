using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardScript : MonoBehaviour
{
    public GameObject tilePrefab;
    public GameObject boardPiecePrefab;
    public GameObject boardPiecesHolder;
    public TilePreviewScript tilePreview;
    public int minClearCount;
    public float tileSpawnDelay;
    public float tileSpawnOffset;
    public float tileMovementTolerance;

    private BoardPieceScript[,] boardPieces = new BoardPieceScript[7,7];
    private List<TileScript> tilesOnBoard = new List<TileScript>();
    private List<BoardPieceScript> boardPiecesWithTilesAbove = new List<BoardPieceScript>();

    private bool _isTilesMoving = false;
    public bool IsTilesMoving
    {
        get
        {
            return _isTilesMoving;
        }
        set
        {
            _isTilesMoving = value;
        }
    }

    void Start ()
    {
        InitBoard();
    }

    private void InitBoard()
    {
        float BoardPieceWidth = boardPiecePrefab.GetComponent<Renderer>().bounds.size.x;
        float BoardPieceHeight = boardPiecePrefab.GetComponent<Renderer>().bounds.size.y;

        for (int i = 0; i < 7; i++)
        {
            for (int j = 0; j < 7; j++)
            {
                GameObject boardPiece = Instantiate(
                    boardPiecePrefab,
                    transform.position + new Vector3(-6 + i * BoardPieceWidth, -6 + j * BoardPieceHeight, 0),
                    Quaternion.identity,
                    boardPiecesHolder.transform);
                boardPiece.name = "Board Piece[" + i + ", " + j + "]";

                boardPieces[i, j] = boardPiece.GetComponent<BoardPieceScript>();
                boardPieces[i, j].InitBoardPiece(i, j);
            }
        }

        IsTilesMoving = false;
    }

    void Update()
    {
        UpdateBoard();
        if (!IsTilesMoving)
        {
            HandleInput();
        }
    }

    private void UpdateBoard()
    {
        bool wasTilesMoving = IsTilesMoving;
        UpdateIsTilesMoving();
        if (!IsTilesMoving)
        {
            if(wasTilesMoving != IsTilesMoving)
            {
                ClearTiles();
                UpdatePlaceableTiles();
            }
            HandleInput();
        } else
        {
            ResetTilesOverBoard();
        }
    }

    private void UpdateIsTilesMoving()
    {
        bool wasTilesMoving = IsTilesMoving;
        foreach (TileScript tS in tilesOnBoard)
        {
            if (tS.GetComponent<Rigidbody2D>().velocity.magnitude > tileMovementTolerance)
            {
                IsTilesMoving = true;
                return;
            }
        }

        IsTilesMoving = false;
    }

    private int ClearTiles()
    {
        HashSet<TileScript> tilesToClear = new HashSet<TileScript>();
        foreach(TileScript tile in tilesOnBoard)
        {
            TileScript[] adjacentSameColorTiles = tile.GetAdjacentSameColorTiles();
            if(adjacentSameColorTiles.Length >= minClearCount-1)
            {
                tilesToClear.Add(tile);
                foreach (TileScript adjacentSameColorTile in adjacentSameColorTiles)
                    tilesToClear.Add(adjacentSameColorTile);
            }
        }

        foreach (TileScript tileToClear in tilesToClear)
        {
            tilesOnBoard.Remove(tileToClear);
            StartCoroutine(tileToClear.Clear());
        }

        return tilesToClear.Count;
    }

    private void UpdatePlaceableTiles()
    {
        for (int i = 0; i < 7; i++)
        {
            for (int j = 0; j < 7; j++)
            {
                RaycastHit2D hit = Physics2D.Raycast(boardPieces[i, j].transform.position, Vector2.zero, 0, LayerMask.GetMask("Tiles"));
                boardPieces[i, j].GetComponent<BoxCollider2D>().enabled = (hit.transform == null);
            }
        }
    }


    private void HandleInput()
    {
        if (Input.touchCount > 0)
        {
            switch (Input.GetTouch(0).phase)
            {
                case TouchPhase.Began:

                    break;
                case TouchPhase.Moved:
                    break;
                case TouchPhase.Ended:
                    break;
            }
            PlaceTileOverBoard(Input.GetTouch(0).position);
        }

        if (Input.GetMouseButton(0))
            PlaceTileOverBoard(Input.mousePosition);
        else if (Input.GetMouseButton(1))
            PlaceTilesOnBoard();
        else if (Input.GetMouseButton(2))
            ResetTilesOverBoard();
    }

    private void PlaceTileOverBoard(Vector3 position)
    {
        RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(position), Vector2.zero, 0, LayerMask.GetMask("Board"));
        if (hit.transform != null)
        {
            TileScript tile = Instantiate(tilePrefab, transform).GetComponent<TileScript>();
            tile.InitTile(GameManagerScript.GetTileColor(tilePreview.GetCurrentTileColor()), false);

            tilePreview.CurrentTileIndex += 1;

            BoardPieceScript boardPieceScript = hit.transform.GetComponent<BoardPieceScript>();
            boardPieceScript.PlaceTileOver(tile, tileSpawnOffset);

            boardPiecesWithTilesAbove.Add(boardPieceScript);
        }
    }

    private void PlaceTilesOnBoard()
    {
        for (int i = 0; i < boardPiecesWithTilesAbove.Count; i++)
        {
            StartCoroutine(boardPiecesWithTilesAbove[i].DropTile(tilesOnBoard, tileSpawnDelay * i));
        }
        boardPiecesWithTilesAbove.Clear();
        tilePreview.FlushBuffer();
    }

    private void ResetTilesOverBoard()
    {
        for (int i = 0; i < boardPiecesWithTilesAbove.Count; i++)
        {
            boardPiecesWithTilesAbove[i].ClearAboveTile();
        }

        boardPiecesWithTilesAbove.Clear();
        tilePreview.ResetBuffer();
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardScript : MonoBehaviour
{
    public int boardWidth;
    public int boardHeight;
    public int numOfPreviewTiles;
    public int previewSeed;
    public GameObject tilePrefab;
    public GameObject boardPiecePrefab;
    public GameObject boardPiecesHolder;
    public TilePreviewScript tilePreview;
    public BoxCollider2D lastTilePlacedOverBox;
    public int minClearCount;
    public float tileSpawnDelay;
    public float tileSpawnOffset;
    public float tileMovementTolerance;

    private BoardPieceScript[,] boardPieces;
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

    private void Awake()
    {
        tilePreview.InitTilePreview(numOfPreviewTiles, previewSeed);
    }

    void Start ()
    {
        InitBoard();
    }

    private void InitBoard()
    {
        boardPieces = new BoardPieceScript[boardWidth, boardHeight];
        float boardPieceWidth = boardPiecePrefab.GetComponent<Renderer>().bounds.size.x;
        float boardPieceHeight = boardPiecePrefab.GetComponent<Renderer>().bounds.size.y;
        float initialXOffset = -boardPieceWidth * boardWidth / 2 + boardPieceWidth/2;
        float initialYOffset = -boardPieceHeight * boardHeight / 2 + boardPieceHeight / 2;

        for (int i = 0; i < boardWidth; i++)
        {
            for (int j = 0; j < boardHeight; j++)
            {
                GameObject boardPiece = Instantiate(
                    boardPiecePrefab,
                    transform.position + new Vector3(initialXOffset + i * boardPieceWidth, initialYOffset + j * boardPieceHeight, 0),
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
        for (int i = 0; i < boardWidth; i++)
        {
            for (int j = 0; j < boardHeight; j++)
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
                case TouchPhase.Moved:
                    Vector3 worldPoint = Camera.main.ScreenToWorldPoint(Input.touches[0].position);
                    if (lastTilePlacedOverBox.OverlapPoint(worldPoint))
                    {
                        ResetLastTileOverBoard();
                    }
                    else
                    {
                        BoardPieceScript boardPiece = GetCurrentBoardPiece(worldPoint);
                        if (boardPiece != null && CheckTilePlacedOverAble(boardPiece))
                            PlaceTileOverBoard(boardPiece);
                    }
                    break;
                case TouchPhase.Ended:
                    if (boardPiecesWithTilesAbove.Count < minClearCount)
                    {
                        ResetTilesOverBoard();
                    }
                    else
                    {
                        PlaceTilesOnBoard();
                    }
                    break;
            }
        }

        if (Input.GetMouseButton(0))
        {
            Vector3 worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            if (lastTilePlacedOverBox.OverlapPoint(worldPoint))
            {
                ResetLastTileOverBoard();
            }
            else
            {
                BoardPieceScript boardPiece = GetCurrentBoardPiece(worldPoint);
                if (boardPiece != null && CheckTilePlacedOverAble(boardPiece))
                    PlaceTileOverBoard(boardPiece);
            }
        }
        else if (Input.GetMouseButton(1))
        {
            if(boardPiecesWithTilesAbove.Count < minClearCount)
            {
                ResetTilesOverBoard();
            } else
            {
                PlaceTilesOnBoard();
            }
        }
        else if (Input.GetMouseButton(2))
        {
            ResetTilesOverBoard();
        }
    }

    private BoardPieceScript GetCurrentBoardPiece(Vector3 position)
    {
        RaycastHit2D hit = Physics2D.Raycast(position, Vector2.zero, 0, LayerMask.GetMask("Board"));
        if (hit.transform != null)
            return hit.transform.GetComponent<BoardPieceScript>();

        return null;
    }

    private bool CheckTilePlacedOverAble(BoardPieceScript boardPiece)
    {
        bool isTileOnOrOver = false;
        if (boardPiece.Y == 0)
            isTileOnOrOver = true;
        else
        {
            BoardPieceScript boardPieceBelow = boardPieces[boardPiece.X, boardPiece.Y - 1];
            isTileOnOrOver = boardPieceBelow.IsTileOnOrOver;
        }

        bool isNextToLastTilePlacedOver = true;
        if (boardPiecesWithTilesAbove.Count > 0)
        {
            BoardPieceScript lastPlacedOverBoardPiece = boardPiecesWithTilesAbove[boardPiecesWithTilesAbove.Count - 1];
            isNextToLastTilePlacedOver =
                (boardPiece.X > 0
                && boardPieces[boardPiece.X - 1, boardPiece.Y] == lastPlacedOverBoardPiece)
                ||
                (boardPiece.X < boardWidth - 1
                && boardPieces[boardPiece.X + 1, boardPiece.Y] == lastPlacedOverBoardPiece)
                ||
                (boardPiece.Y > 0
                && boardPieces[boardPiece.X, boardPiece.Y - 1] == lastPlacedOverBoardPiece)
                ||
                (boardPiece.Y < boardHeight - 1
                && boardPieces[boardPiece.X, boardPiece.Y + 1] == lastPlacedOverBoardPiece);
        }

        return (isTileOnOrOver && isNextToLastTilePlacedOver);
    }

    private void PlaceTileOverBoard(BoardPieceScript boardPiece)
    {
        try
        {
            TileColor colorName = GameManagerScript.GetTileColor(tilePreview.GetCurrentTileColor());
            TileScript tile = Instantiate(tilePrefab, transform).GetComponent<TileScript>();
            tile.InitTile(colorName, false);
            boardPiece.PlaceTileOver(tile, tileSpawnOffset);

            tilePreview.CurrentTileIndex += 1;

            boardPiecesWithTilesAbove.Add(boardPiece);
            UpdateLastTilePlacedOverBox();
        }
        catch
        {
            return;
        }
    }

    private void UpdateLastTilePlacedOverBox()
    {
        if (boardPiecesWithTilesAbove.Count > 1)
        {
            lastTilePlacedOverBox.enabled = true;
            lastTilePlacedOverBox.transform.position = boardPiecesWithTilesAbove[boardPiecesWithTilesAbove.Count - 2].transform.position;
        }
        else
        {
            lastTilePlacedOverBox.enabled = false;
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

    private void ResetLastTileOverBoard()
    {
        BoardPieceScript lastBoardPiecesWithTilesAbove = boardPiecesWithTilesAbove[boardPiecesWithTilesAbove.Count - 1];
        lastBoardPiecesWithTilesAbove.ClearAboveTile();
        boardPiecesWithTilesAbove.Remove(lastBoardPiecesWithTilesAbove);
        tilePreview.CurrentTileIndex -= 1;
        UpdateLastTilePlacedOverBox();
    }

    private void ResetTilesOverBoard()
    {
        for (int i = 0; i < boardPiecesWithTilesAbove.Count; i++)
        {
            boardPiecesWithTilesAbove[i].ClearAboveTile();
        }

        boardPiecesWithTilesAbove.Clear();
        tilePreview.ResetBuffer();
        UpdateLastTilePlacedOverBox();
    }
}
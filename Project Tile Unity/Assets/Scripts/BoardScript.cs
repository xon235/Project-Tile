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
    public GameObject ground;
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

        ground.transform.localPosition = new Vector3(0, -boardPieceHeight * boardHeight / 2, 0);
        IsTilesMoving = false;
    }

    void Update()
    {
        if (!GameManagerScript.Instance.IsGameOver)
        {
            bool wasTilesMoving = IsTilesMoving;
            UpdateIsTilesMoving();
            if (!IsTilesMoving)
            {
                if (wasTilesMoving != IsTilesMoving)
                {
                    ClearTiles();
                    CheckGameOver();
                    UpdatePlaceableTiles();
                }
                HandleInput();
            }
            else
            {
                ResetTilesOverBoard();
            }
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

        int points = 0;
        foreach (TileScript tileToClear in tilesToClear)
        {
            points += tileToClear.Point;
            tilesOnBoard.Remove(tileToClear);
            StartCoroutine(tileToClear.Clear());
        }

        GameManagerScript.Instance.AddPointsToScore(points);
        return tilesToClear.Count;
    }

    private void CheckGameOver()
    {
        if (tilePreview.TilesLeft == 0)
        {
            GameManagerScript.Instance.GameOver(true);
        } else
        {
            for (int i = 0; i < boardWidth; i++)
            {
                for (int j = 0; j < boardHeight; j++)
                {
                    if (!boardPieces[i, j].IsTileOn)
                    {
                        int adjacentBoardPieceWithoutTileOnCount = 0;
                        if (0 < i && !boardPieces[i - 1, j].IsTileOn)
                            adjacentBoardPieceWithoutTileOnCount += 1;

                        if (i < boardWidth - 1 && !boardPieces[i + 1, j].IsTileOn)
                            adjacentBoardPieceWithoutTileOnCount += 1;

                        if (0 < j && !boardPieces[i, j - 1].IsTileOn)
                            adjacentBoardPieceWithoutTileOnCount += 1;

                        if (j < boardHeight - 1 && !boardPieces[i, j + 1].IsTileOn)
                            adjacentBoardPieceWithoutTileOnCount += 1;

                        if (adjacentBoardPieceWithoutTileOnCount >= 2)
                            return;
                    }
                }
            }

            GameManagerScript.Instance.GameOver(false);
        }
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
                        UpdateLastTileOverBoard();
                    }
                    else
                    {
                        BoardPieceScript boardPiece = GetCurrentBoardPiece(worldPoint);
                        if (boardPiece != null && CheckTilePlacedOverAble(boardPiece))
                            PlaceTileOverBoard(boardPiece);
                    }
                    break;
                case TouchPhase.Ended:
                    if (boardPiecesWithTilesAbove.Count >= minClearCount
                        || boardPiecesWithTilesAbove.Count == tilePreview.TilesLeft)
                    {
                        PlaceTilesOnBoard();
                    }
                    else
                    {
                        ResetTilesOverBoard();
                    }
                    break;
            }
        }

        if (Input.GetMouseButton(0))
        {
            Vector3 worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            if (lastTilePlacedOverBox.OverlapPoint(worldPoint))
            {
                UpdateLastTileOverBoard();
            }
            else
            {
                BoardPieceScript boardPiece = GetCurrentBoardPiece(worldPoint);
                if (boardPiece != null && CheckTilePlacedOverAble(boardPiece))
                    PlaceTileOverBoard(boardPiece);
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            if (boardPiecesWithTilesAbove.Count >= minClearCount
                        || tilePreview.TilesLeft == 0)
            {
                PlaceTilesOnBoard();
            }
            else
            {
                ResetTilesOverBoard();
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

        return (isNextToLastTilePlacedOver);
    }

    private void PlaceTileOverBoard(BoardPieceScript boardPiece)
    {
        try
        {
            TileColor colorName = GameManagerScript.Instance.GetTileColor(tilePreview.GetCurrentTileColor());
            TileScript tile = Instantiate(tilePrefab, transform).GetComponent<TileScript>();
            tile.InitTile(colorName, false, GameManagerScript.Instance.GetTilePoint(boardPiecesWithTilesAbove.Count));

            boardPiece.PlaceTileOver(tile, tileSpawnOffset);
            boardPiecesWithTilesAbove.Add(boardPiece);

            UpdateLastTilePlacedOverBox();

            tilePreview.CurrentTileIndex += 1;
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
        if (boardPiecesWithTilesAbove.Count > 0)
        {
            for (int i = 0; i < boardPiecesWithTilesAbove.Count; i++)
            {
                StartCoroutine(boardPiecesWithTilesAbove[i].DropTile(tilesOnBoard, tileSpawnDelay * i));
            }
            boardPiecesWithTilesAbove.Clear();
            tilePreview.FlushBuffer();
            GameManagerScript.Instance.IncrementTurnsTook();
        }
    }

    private void UpdateLastTileOverBoard()
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
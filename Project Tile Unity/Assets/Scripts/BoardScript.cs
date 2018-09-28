using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardScript : MonoBehaviour
{
    public GameObject tilePrefab;
    public GameObject boardPiecePrefab;
    public TilePreviewScript tilePreview;
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
            if(_isTilesMoving != value)
            {
                if (value == true)
                    TurnBoardOff();
                else
                    TurnBoardOn();


                _isTilesMoving = value;
            }
        }
    }

    private void TurnBoardOff()
    {
        for (int i = 0; i < 7; i++)
            for (int j = 0; j < 7; j++)
                boardPieces[i, j].GetComponent<BoxCollider2D>().enabled = false;
    }

    private void TurnBoardOn()
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
                    transform);
                boardPiece.name = "Board Piece[" + i + ", " + j + "]";

                boardPieces[i, j] = boardPiece.GetComponent<BoardPieceScript>();
                boardPieces[i, j].InitBoardPiece(i, j);
            }
        }

        IsTilesMoving = false;
    }

    void Update()
    {
        UpdateIsTilesMoving();

        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved)
        {
            PlaceTileOverBoard(Input.GetTouch(0).position);
        }

        if (Input.GetMouseButton(0))
        {
            PlaceTileOverBoard(Input.mousePosition);
        }

        if (Input.GetMouseButton(1))
        {
            for (int i = 0; i < boardPiecesWithTilesAbove.Count; i++)
            {
                StartCoroutine(boardPiecesWithTilesAbove[i].DropTile(tileSpawnDelay * i));
            }
            boardPiecesWithTilesAbove.Clear();
            tilePreview.FlushBuffer();
        }

        if (Input.GetMouseButton(2))
        {
            for (int i = 0; i < boardPiecesWithTilesAbove.Count; i++)
            {
                boardPiecesWithTilesAbove[i].ClearAboveTile();
            }
            boardPiecesWithTilesAbove.Clear();
            tilePreview.ResetBuffer();
        }
    }

    private void UpdateIsTilesMoving()
    {
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

    private void PlaceTileOverBoard(Vector3 position)
    {
        RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(position), Vector2.zero, 0, LayerMask.GetMask("Board"));
        if (hit.transform != null)
        {
            TileScript tile = Instantiate(tilePrefab, transform).GetComponent<TileScript>();
            tile.InitTile(GameManagerScript.GetTileColor(tilePreview.GetCurrentTileColor()), false);
            tilesOnBoard.Add(tile);

            tilePreview.CurrentTileIndex += 1;

            BoardPieceScript boardPieceScript = hit.transform.GetComponent<BoardPieceScript>();
            boardPieceScript.PlaceTileOver(tile, tileSpawnOffset);

            boardPiecesWithTilesAbove.Add(boardPieceScript);
        }
    }
}
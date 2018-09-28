using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardScript : MonoBehaviour
{

    public GameObject tilePrefab;
    public GameObject boardPiecePrefab;
    public GameObject tilePreview;
    public float tileSpawnDelay;
    public float tileSpawnOffset;

    private BoardPieceScript[,] boardPieces = new BoardPieceScript[7,7];
    private List<BoardPieceScript> boardPiecesWithTilesAbove = new List<BoardPieceScript>();

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
                boardPiece.name = "Board Piece[" + i + "][" + j + "]";

                boardPieces[i, j] = boardPiece.GetComponent<BoardPieceScript>();
                boardPieces[i, j].InitBoardPiece(i, j);
            }
        }
    }

    void Update()
    {
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved)
        {
            PlaceTileOverBoard(Input.GetTouch(0).position);
        }

        if (Input.GetMouseButton(0))
        {
            PlaceTileOverBoard(Input.mousePosition);
        }

        if (Input.GetMouseButton(2))
        {
            for(int i = 0; i < boardPiecesWithTilesAbove.Count; i++)
            {
                StartCoroutine(boardPiecesWithTilesAbove[i].DropTile(tileSpawnDelay * i));
            }
            boardPiecesWithTilesAbove.Clear();
        }
    }

    private void PlaceTileOverBoard(Vector3 position)
    {
        RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(position), Vector2.zero);
        if (hit.transform != null && hit.transform.tag == "Board Piece")
        {
            TileScript tile = Instantiate(tilePrefab, transform).GetComponent<TileScript>();
            tile.InitTile(GameManagerScript.GetTileColor(ColorName.RED), false);

            BoardPieceScript boardPieceScript = hit.transform.GetComponent<BoardPieceScript>();
            boardPieceScript.PlaceTileOver(tile, tileSpawnOffset);

            boardPiecesWithTilesAbove.Add(boardPieceScript);

            Debug.Log(hit.transform.name);
        }
    }
}
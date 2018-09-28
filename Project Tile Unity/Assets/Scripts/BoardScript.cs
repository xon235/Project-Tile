﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardScript : MonoBehaviour
{

    public GameObject tilePrefab;
    public GameObject boardPiecePrefab;
    public GameObject tilePreview;
    public float tileSpawnDelay;
    public float tileSpawnOffset;

    private GameObject[,] boardPieces = new GameObject[7,7];
    private GameObject[,] tilesOnBoard = new GameObject[7, 7];
    private GameObject[,] tilesOverBoard = new GameObject[7, 7];

    //TEST
    private Vector3[] spawnPoints;
    //TEST END

    void Start ()
    {
        InitBoard();
        PlaceTestTiles();
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
                boardPieces[i, j] = boardPiece;
            }
        }
    }

    //TEST
    private void PlaceTestTiles()
    {
        spawnPoints = new Vector3[5];
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            spawnPoints[i] = boardPieces[3, i].transform.position + new Vector3(0, tileSpawnOffset, 0);
        }
        StartCoroutine(SpawnTile(1, tileSpawnDelay, 0));
    }

    private IEnumerator SpawnTile(float satrtDelay, float delay, int index)
    {
        yield return new WaitForSeconds(satrtDelay);
        StartCoroutine(SpawnTile(delay, index));
    }

    private IEnumerator SpawnTile(float delay, int index)
    {
        if (index < 5)
        {
            yield return new WaitForSeconds(delay);
            GameObject tile = Instantiate(tilePrefab, spawnPoints[index], Quaternion.identity);
            tile.GetComponent<TileScript>().InitTile(GameManagerScript.GetTileColor((ColorName)index), false);
            StartCoroutine(SpawnTile(delay, index + 1));
        }
    }
    //TEST END

    void Update()
    {
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved)
        {
            HandleInput(Input.GetTouch(0).position);
        }

        if (Input.GetMouseButton(0))
        {
            HandleInput(Input.mousePosition);
        }
    }

    private void HandleInput(Vector3 position)
    {
        RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(position), Vector2.zero);
        if (hit.transform != null)
        {
            Debug.Log(hit.transform.name);
        }
    }
}
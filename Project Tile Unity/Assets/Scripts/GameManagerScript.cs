﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public enum ColorName
{
    RED,
    ORANGE,
    YELLOW,
    GREEN,
    BLUE,
    COUNT
}

[System.Serializable]
public struct TileColor
{
    public ColorName Name;
    public Color TopColor;
    public Color SideColor;
}

public struct GameRecord
{
    public int Score;
    public int TurnsTook;
    public bool Finished;
}

public class GameManagerScript : MonoBehaviour
{
    public static GameManagerScript Instance = null;
    public float gameToRankSceneTransitionDelay;
    public GameObject gameOverLabel;
    public GameObject finishedLabel;
    public TileColor[] tileColors;
    public Text scoreText;
    public Text turnsTookText;
    public int maxNumOfRecords;

    public GameRecord[] records { get; private set; }

    private int score = 0;
    private int turnsTook = 0;
    private Dictionary<ColorName, TileColor> tileColorDict = new Dictionary<ColorName, TileColor>();

    private const string PLAYER_PREFS_SCORE = "PLAYER_PREFS_SCORE";
    private const string PLAYER_PREFS_TURNS_TOOK = "PLAYER_PREFS_TURNS_TOOK";
    private const string PLAYER_PREFS_FINSHED = "PLAYER_PREFS_FINSHED";

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
            Destroy(gameObject);

        LoadRecords();
        UpdateTexts();

        for (int i = 0; i < tileColors.Length; i++)
        {
            tileColorDict.Add(tileColors[i].Name, tileColors[i]);
        }
    }

    private void UpdateTexts()
    {
        if(scoreText != null & turnsTookText != null)
        {
            scoreText.text = score.ToString();
            turnsTookText.text = turnsTook.ToString();
        }
    }

    public TileColor GetTileColor(ColorName colorName)
    {
        return tileColorDict[colorName];
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if(SceneManager.GetActiveScene().name == "GameScene")
            {
                Application.Quit();
            }
            else
            {
                LoadGameScene();
            }
        }
    }

    public int GetTilePoint(int index = 0)
    {
        if(index > 2)
        {
            return 10 * (index - 1);
        }
        return 10;
    }

    public void AddPointsToScore(int points)
    {
        score += points;
        UpdateTexts();
    }

    public void IncrementTurnsTook()
    {
        turnsTook += 1;
        UpdateTexts();
    }

    public void SaveResult(bool finished)
    {
        records[maxNumOfRecords].Score = score;
        records[maxNumOfRecords].TurnsTook = turnsTook;
        records[maxNumOfRecords].Finished = finished;

        Array.Sort<GameRecord>(records, (a, b) =>
        {
            int result = b.Score.CompareTo(a.Score);
            result = result == 0 ? b.TurnsTook.CompareTo(a.TurnsTook) : result;
            result = result == 0 ? b.Finished.CompareTo(a.Finished) : result;
            return result;
        });

        for (int i = 0; i < maxNumOfRecords; i++)
        {
            PlayerPrefs.SetInt(PLAYER_PREFS_SCORE + i, records[i].Score);
            PlayerPrefs.SetInt(PLAYER_PREFS_TURNS_TOOK + i, records[i].TurnsTook);
            PlayerPrefs.SetInt(PLAYER_PREFS_FINSHED + i, records[i].Finished? 1: 0);
        }

        if (finished)
        {
            finishedLabel.SetActive(true);
        }
        else
        {
            gameOverLabel.SetActive(true);
        }
    }

    public void LoadGameScene()
    {
        SceneManager.LoadScene("GameScene");
    }

    public IEnumerator LoadRankScene()
    {
        yield return new WaitForSeconds(gameToRankSceneTransitionDelay);
        SceneManager.LoadScene("RankScene");
    }

    public void LoadRecords()
    {
        records = new GameRecord[maxNumOfRecords+1];
        for(int i = 0; i < maxNumOfRecords; i++)
        {
            records[i].Score = PlayerPrefs.GetInt(PLAYER_PREFS_SCORE+i, 0);
            records[i].TurnsTook = PlayerPrefs.GetInt(PLAYER_PREFS_TURNS_TOOK + i, 0);
            records[i].Finished = PlayerPrefs.GetInt(PLAYER_PREFS_FINSHED + i, 0) != 0;
        }
    }
}
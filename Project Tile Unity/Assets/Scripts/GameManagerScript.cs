using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

public class GameManagerScript : MonoBehaviour
{
    public static GameManagerScript Instance = null;
    public TileColor[] tileColors;
    public Text scoreText;
    public Text turnsTookText;

    private int score = 0;
    private int turnsTook = 0;
    private Dictionary<ColorName, TileColor> tileColorDict = new Dictionary<ColorName, TileColor>();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
            Destroy(gameObject);

        UpdateTexts();

        for (int i = 0; i < tileColors.Length; i++)
        {
            tileColorDict.Add(tileColors[i].Name, tileColors[i]);
        }
    }

    private void UpdateTexts()
    {
        scoreText.text = score.ToString();
        turnsTookText.text = turnsTook.ToString();
    }

    public TileColor GetTileColor(ColorName colorName)
    {
        return tileColorDict[colorName];
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
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
}

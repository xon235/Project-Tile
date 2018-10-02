using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    public TileColor[] tileColors;

    private static Dictionary<ColorName, TileColor> tileColorDict = new Dictionary<ColorName, TileColor>();

    private void Awake()
    {
        for (int i = 0; i < tileColors.Length; i++)
        {
            tileColorDict.Add(tileColors[i].Name, tileColors[i]);
        }
    }

    public static TileColor GetTileColor(ColorName colorName)
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

    public static int GetTilePoint(int index = 0)
    {
        if(index > 2)
        {
            return 10 * (index - 1);
        }
        return 10;
    }
}

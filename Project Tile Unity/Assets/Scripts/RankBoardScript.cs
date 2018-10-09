using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RankBoardScript : MonoBehaviour {

    public float verticalOffset;
    public GameObject scoreText;
    public GameObject turnsTookText;
    public GameObject finishedText;

	// Use this for initialization
	void Start () {
        GameRecord[] gameRecords = GameManagerScript.Instance.Records;

        for(int i = 0; i < gameRecords.Length - 1; i++)
        {
            GameObject sT = Instantiate(scoreText, transform);
            GameObject tT = Instantiate(turnsTookText, transform);
            GameObject fT = Instantiate(finishedText, transform);

            sT.GetComponent<RectTransform>().anchoredPosition = sT.GetComponent<RectTransform>().anchoredPosition + Vector2.down * verticalOffset * i;
            tT.GetComponent<RectTransform>().anchoredPosition = tT.GetComponent<RectTransform>().anchoredPosition + Vector2.down * verticalOffset * i;
            fT.GetComponent<RectTransform>().anchoredPosition = fT.GetComponent<RectTransform>().anchoredPosition + Vector2.down * verticalOffset * i;

            sT.GetComponent<Text>().text = gameRecords[i].Score.ToString();
            tT.GetComponent<Text>().text = gameRecords[i].TurnsTook.ToString();
            fT.GetComponent<Text>().text = gameRecords[i].Finished? "YES": "NO";
        }

        scoreText.SetActive(false);
        turnsTookText.SetActive(false);
        finishedText.SetActive(false);
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}

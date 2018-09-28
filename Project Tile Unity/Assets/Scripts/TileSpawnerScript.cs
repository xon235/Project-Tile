using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileSpawnerScript : MonoBehaviour {

    public GameObject TilePrefab;
    public Transform[] Markers;
    public float StartDelay;
    public float Delay;

	// Use this for initialization
	void Start () {
        StartCoroutine(SpawnTile(StartDelay, Delay, Markers.Length-1));
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private IEnumerator SpawnTile(float satrtDelay, float delay, int index)
    {
        yield return new WaitForSeconds(satrtDelay);
        StartCoroutine(SpawnTile(Delay, index));
    }

    private IEnumerator SpawnTile(float delay, int index)
    {
        if(index >= 0)
        {
            yield return new WaitForSeconds(delay);
            Instantiate(TilePrefab, Markers[index].position, Markers[index].rotation);
            StartCoroutine(SpawnTile(Delay, index-1));
        }
    }
}

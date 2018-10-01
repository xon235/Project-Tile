using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardPieceScript : MonoBehaviour
{
    public int X { get; private set; }
    public int Y { get; private set; }

    public bool IsTileOnOrOver
    {
        get
        {
            if(tileAbove != null)
                return true;
            else
            {
                RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.zero, 0, LayerMask.GetMask("Tiles"));
                return (hit.transform != null);
            }
        }
    }

    private TileScript tileAbove;

    public void InitBoardPiece(int x, int y)
    {
        X = x;
        Y = y;
    }

	public void PlaceTileOver(TileScript tile, float offset)
    {
        tile.transform.position = transform.position + new Vector3(0, offset, 0);
        GetComponent<BoxCollider2D>().enabled = false;
        tileAbove = tile;
    }

    public IEnumerator DropTile(List<TileScript> tilesOnBoard, float delay)
    {
        if (tileAbove != null)
        {
            yield return new WaitForSeconds(delay);
            if (tileAbove != null)
            {
                tileAbove.IsTileEnabled = true;
                tilesOnBoard.Add(tileAbove);
                tileAbove = null;
            }
        }
    }

    public void ClearAboveTile()
    {
        if (tileAbove != null)
        {
            Destroy(tileAbove.gameObject);
            GetComponent<BoxCollider2D>().enabled = true;
            tileAbove = null;
        }
    }
}

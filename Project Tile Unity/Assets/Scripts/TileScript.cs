using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileScript : MonoBehaviour
{

    public GameObject side;
    public float tileDisabledAlpha;

    public ColorName ColorName { get; private set; }
    public bool IsDummy{ get; private set; }

    private bool _isTileEnabled;
    public bool IsTileEnabled
    {
        get
        {
            return _isTileEnabled;
        }
        set
        {
            Color topColor = GetComponent<SpriteRenderer>().color;
            Color sideColor = side.GetComponent<SpriteRenderer>().color;
            
            topColor.a = sideColor.a = value ? 1 : tileDisabledAlpha;

            GetComponent<SpriteRenderer>().color = topColor;
            side.GetComponent<SpriteRenderer>().color = sideColor;

            GetComponent<Rigidbody2D>().isKinematic = !(value && !IsDummy);
            GetComponent<BoxCollider2D>().enabled = (value && !IsDummy);

            _isTileEnabled = value;
        }
    }

    public void InitTile(TileColor tileColor, bool isDummy)
    {
        ColorName = tileColor.Name;
        GetComponent<SpriteRenderer>().color = tileColor.TopColor;
        side.GetComponent<SpriteRenderer>().color = tileColor.SideColor;

        IsDummy = isDummy;
        IsTileEnabled = isDummy;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.tag == tag 
            && transform.position.y > collision.transform.position.y)
            GetComponent<AudioSource>().Play();
    }

    public TileScript[] GetAdjacentSameColorTiles()
    {
        List<TileScript> adjacentSameColorTiles = new List<TileScript>();
        RaycastHit2D[] hits = new RaycastHit2D[4];
        hits[0] = Physics2D.Raycast(transform.position, Vector2.up, GetComponent<Renderer>().bounds.size.y, LayerMask.GetMask("Tiles"));
        hits[1] = Physics2D.Raycast(transform.position, Vector2.down, GetComponent<Renderer>().bounds.size.y, LayerMask.GetMask("Tiles"));
        hits[2] = Physics2D.Raycast(transform.position, Vector2.left, GetComponent<Renderer>().bounds.size.x, LayerMask.GetMask("Tiles"));
        hits[3] = Physics2D.Raycast(transform.position, Vector2.right, GetComponent<Renderer>().bounds.size.x, LayerMask.GetMask("Tiles"));

        foreach(RaycastHit2D hit in hits)
        {
            if (hit.transform != null)
                adjacentSameColorTiles.Add(hit.transform.GetComponent<TileScript>());
        }

        return adjacentSameColorTiles.ToArray();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawRay(transform.position, Vector2.up * GetComponent<Renderer>().bounds.size.y);
        Gizmos.DrawRay(transform.position, Vector2.down * GetComponent<Renderer>().bounds.size.y);
        Gizmos.DrawRay(transform.position, Vector2.left * GetComponent<Renderer>().bounds.size.x);
        Gizmos.DrawRay(transform.position, Vector2.right * GetComponent<Renderer>().bounds.size.x);
    }
}
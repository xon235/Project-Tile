using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileScript : MonoBehaviour
{

    public GameObject side;
    public float tileDisabledAlpha;
    public float clearTime;
    public float circleCastOffsetScale;
    public float circleCastRadius;

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
        hits[0] = Physics2D.CircleCast(transform.position + Vector3.up * GetComponent<Renderer>().bounds.size.y * circleCastOffsetScale, circleCastRadius, Vector3.zero, 0, LayerMask.GetMask("Tiles"));
        hits[1] = Physics2D.CircleCast(transform.position + Vector3.down * GetComponent<Renderer>().bounds.size.y * circleCastOffsetScale, circleCastRadius, Vector3.zero, 0, LayerMask.GetMask("Tiles"));
        hits[2] = Physics2D.CircleCast(transform.position + Vector3.left * GetComponent<Renderer>().bounds.size.x * circleCastOffsetScale, circleCastRadius, Vector3.zero, 0, LayerMask.GetMask("Tiles"));
        hits[3] = Physics2D.CircleCast(transform.position + Vector3.right * GetComponent<Renderer>().bounds.size.x * circleCastOffsetScale, circleCastRadius, Vector3.zero, 0, LayerMask.GetMask("Tiles"));

        foreach (RaycastHit2D hit in hits)
        {
            if (hit.transform != null)
            {
                if(hit.transform.GetComponent<TileScript>().ColorName == ColorName)
                    adjacentSameColorTiles.Add(hit.transform.GetComponent<TileScript>());
            }
        }

        return adjacentSameColorTiles.ToArray();
    }

    public IEnumerator Clear()
    {
        GetComponent<Rigidbody2D>().isKinematic= true;
        GetComponent<BoxCollider2D>().enabled = false;
        Vector3 targetScale = Vector3.zero;
        Vector3 clearScaleVelocity = Vector3.zero;
        while (transform.localScale != targetScale)
        {
            transform.localScale = Vector3.SmoothDamp(transform.localScale, targetScale, ref clearScaleVelocity, clearTime);
            yield return null;
        }

        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position + Vector3.up * GetComponent<Renderer>().bounds.size.y * circleCastOffsetScale, circleCastRadius);
        Gizmos.DrawWireSphere(transform.position + Vector3.down * GetComponent<Renderer>().bounds.size.y * circleCastOffsetScale, circleCastRadius);
        Gizmos.DrawWireSphere(transform.position + Vector3.left * GetComponent<Renderer>().bounds.size.x * circleCastOffsetScale, circleCastRadius);
        Gizmos.DrawWireSphere(transform.position + Vector3.right * GetComponent<Renderer>().bounds.size.x * circleCastOffsetScale, circleCastRadius);
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileScript : MonoBehaviour
{

    public GameObject side;
    public float tileDisabledAlpha;

    public ColorName ColorName { get; private set; }
    public bool IsDummy{ get; private set; }

    private bool isTileEnabled;
    public bool IsTileEnabled
    {
        get
        {
            return isTileEnabled;
        }
        set
        {
            Color topColor = GetComponent<SpriteRenderer>().color;
            Color sideColor = side.GetComponent<SpriteRenderer>().color;
            
            topColor.a = sideColor.a = value ? 1 : tileDisabledAlpha;

            GetComponent<SpriteRenderer>().color = topColor;
            side.GetComponent<SpriteRenderer>().color = sideColor;

            GetComponent<Rigidbody2D>().isKinematic = !value || IsDummy;
            GetComponent<BoxCollider2D>().enabled = !(!value || IsDummy);

            isTileEnabled = value;
        }
    }

    public void InitTile(TileColor tileColor, bool isDummy)
    {
        ColorName = tileColor.Name;
        GetComponent<SpriteRenderer>().color = tileColor.TopColor;
        side.GetComponent<SpriteRenderer>().color = tileColor.SideColor;

        GetComponent<Rigidbody2D>().isKinematic = isDummy;
        IsDummy = isDummy;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.tag == tag 
            && transform.position.y > collision.transform.position.y)
        {
            GetComponent<AudioSource>().Play();
            Debug.Log("Playing Audio");
        }
    }
}

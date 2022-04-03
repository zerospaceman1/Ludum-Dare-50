using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TilePurchase : MonoBehaviour
{
    public GameObject tileDisplay;
    public GameObject soldOut;

    public GameObject tilePurchase;

    // Start is called before the first frame update
    void Start()
    {
        GameController.Instance.OnUpdatePlayerFunds += togglePurchase;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void togglePurchase(int funds)
    {
        if(funds < 400)
        {
            Color col = tileDisplay.GetComponent<SpriteRenderer>().color;
            tileDisplay.GetComponent<SpriteRenderer>().color = new Color(col.r, col.g, col.b, 0.6f);
        } else
        {
            Color col = tileDisplay.GetComponent<SpriteRenderer>().color;
            tileDisplay.GetComponent<SpriteRenderer>().color = new Color(col.r, col.g, col.b, 1f);
        }
    }

    public void buyTile()
    {
        GameController.Instance.buyTile(tilePurchase);
    }

    void OnMouseUp()
    {
        Debug.Log("clicked on this: " + gameObject.name);
        GameController.Instance.buyTile(tilePurchase);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopPreview : MonoBehaviour
{
    public SpriteRenderer previewOne;
    public SpriteRenderer previewTwo;
    public SpriteRenderer previewThree;

    public List<Tile> previewTiles;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void updatePreviews(List<Tile> tiles)
    {
        previewTiles = tiles;
        previewOne.sprite = GameController.Instance.tileSprites[tiles[0].type];
        previewTwo.sprite = GameController.Instance.tileSprites[tiles[1].type];
        //previewThree.sprite = GameController.Instance.tileSprites[tiles[2].type]; // this one is a secret!
    }
}

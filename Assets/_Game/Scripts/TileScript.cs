using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileScript : MonoBehaviour
{
    public Tile.Type type;
    public SpriteRenderer[] lavaSpriteRenderer_Up = new SpriteRenderer[2];
    public SpriteRenderer[] lavaSpriteRenderer_Down = new SpriteRenderer[2];
    public SpriteRenderer[] lavaSpriteRenderer_Left = new SpriteRenderer[2];
    public SpriteRenderer[] lavaSpriteRenderer_Right = new SpriteRenderer[2];
    public SpriteRenderer iconRenderer;
    public SpriteRenderer gridMarker;

    public 

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void animateLava(Tile tile, int tileSizeMultiplier)
    {
        if((type != Tile.Type.canal_down_left && type != Tile.Type.canal_down_right && type != Tile.Type.canal_up_left && type != Tile.Type.canal_up_right) || isCornerOverflowing(tile))
        {
            // if the tile is NOT a corner, or the corner is overflowing
            SpriteRenderer lavaSr = getLavaSpriteByDirection(tile.flowStartedFromDirection)[0];
            lavaSr.size = new Vector2(lavaSr.size.x, -1 * (tileSizeMultiplier * tile.fillValue));   // -1 * because the stretching of the icon looks better in reverse
        } else
        {
            // corner tiles
            // corners require two animations, one for each 'direction' that the lava flows
            if (tile.fillValue < 0.75)
            {
                SpriteRenderer lavaSr = getLavaSpriteByDirection(tile.flowStartedFromDirection)[0];
                lavaSr.size = new Vector2(lavaSr.size.x, -1 * (tileSizeMultiplier * tile.fillValue));
            } else
            {
                SpriteRenderer lavaSr = getLavaSpriteByDirection(tile.flowStartedFromDirection)[1];
                lavaSr.size = new Vector2(lavaSr.size.x, -1 * (tileSizeMultiplier * (tile.fillValue - 0.75f)));
            }
        }
    }

    private bool isCornerOverflowing(Tile tile)
    {
        if(tile.canConnect[tile.flowStartedFromDirection])
        {
            return false;
        }
        return true;
    }

    public SpriteRenderer[] getLavaSpriteByDirection(string direction)
    {
        switch (direction)
        {
            case "up":
                return lavaSpriteRenderer_Up;
            case "down":
                return lavaSpriteRenderer_Down;
            case "left":
                return lavaSpriteRenderer_Left;
            case "right":
                return lavaSpriteRenderer_Right;
            default:
                return lavaSpriteRenderer_Up;
        }
    }

    private void OnMouseUp()
    {
        if (GameController.Instance.gameRunningAndNotPaused())
        {
            if (GameController.Instance.placingTile)
            {
                Debug.Log("placed on: " + gameObject.name);
                Debug.Log("placing tile: " + GameController.Instance.placingTile.name + " at " + GameController.Instance.tileMap[gameObject].x + "," + GameController.Instance.tileMap[gameObject].y);
                GameController.Instance.placeTile(GameController.Instance.tileMap[gameObject].x, GameController.Instance.tileMap[gameObject].y, GameController.Instance.placingTile.GetComponent<TileScript>().type);
            }
        }
    }
}

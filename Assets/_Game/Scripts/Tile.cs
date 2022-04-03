using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile
{
    public enum Type
    {
        canal,
        canal_horizontal,
        canal_down_left,
        canal_up_left,
        canal_up_right,
        canal_down_right,
        town,
        natural,
        nonPlayable
    }

    public Type type = Type.natural;

    public GameObject go;

    public bool isTown = false;
    public bool isVolcano = false;

    public bool isFlowing = false;
    public string flowStartedFromDirection;     // this is the direction from which the lava entered the tile
    public bool isFull = false;
    public bool isBuildable = true;

    public float fillValue = 0f;

    public float flowStartTime = 0f;

    public Dictionary<string, Tile> connections = new Dictionary<string, Tile>() { { "up", null }, { "down", null }, { "left", null }, { "right", null } };
    public Dictionary<string, bool> canConnect = new Dictionary<string, bool>() { { "up", false }, { "down", false }, { "left", false }, { "right", false } };

    public int x, y;

    public bool checkConnectFrom(string direction)
    {
        // checks that something connecting to this tile is possible with this tiles connections.
        // eg: something connecting from it's "down" connection must check this tiles "up" connection
        switch (direction)
        {
            case "up":
                return canConnect["down"];
            case "down":
                return canConnect["up"];
            case "left":
                return canConnect["right"];
            case "right":
                return canConnect["left"];
            default:
                return false;
        }
    }

    public Tile() { }

    public Tile(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public static Tile createTown(int x, int y)
    {
        Tile t = new Tile(x, y);

        t.setTown();

        return t;
    }

    public void setNonPlayable()
    {
        type = Type.nonPlayable;

        canConnect["up"] = true;
        canConnect["down"] = true;
        canConnect["left"] = true;
        canConnect["right"] = true;
    }

    public void setTown()
    {
        type = Type.town;

        canConnect["up"] = false;
        canConnect["down"] = true;
        canConnect["left"] = true;
        canConnect["right"] = true;

        isTown = true;
    }

    public static Tile createNaturalCanalVertical(int x, int y)
    {
        Tile t = new Tile(x, y);
        t.setNatural();
        return t;
    }

    public void setNatural()
    {
        type = Type.natural;

        canConnect["up"] = false;
        canConnect["down"] = true;
        canConnect["left"] = false;
        canConnect["right"] = false;
    }

    public void setCanal(Type type)
    {
        this.type = type;

        canConnect["up"] = type == Type.canal || type == Type.canal_up_left || type == Type.canal_up_right ? true : false;
        canConnect["down"] = type == Type.canal || type == Type.canal_down_left || type == Type.canal_down_right ? true : false;
        canConnect["left"] = type == Type.canal_horizontal || type == Type.canal_down_left || type == Type.canal_up_left ? true : false;
        canConnect["right"] = type == Type.canal_horizontal || type == Type.canal_down_right || type == Type.canal_up_right ? true : false;
    }

    public void setTileByType(Type type)
    {
        switch (type)
        {
            case Type.nonPlayable:
                setNonPlayable();
                break;
            case Type.town:
                setTown();
                break;
            case Type.natural:
                setNatural();
                break;
            case Type.canal:
            case Type.canal_horizontal:
            case Type.canal_down_left:
            case Type.canal_down_right:
            case Type.canal_up_left:
            case Type.canal_up_right:
                setCanal(type);
                break;
            default:
                setNatural();
                break;
        }
    }

    public static string getOppositeDirection(string direction)
    {
        switch (direction)
        {
            case "up":
                return "down";
            case "down":
                return "up";
            case "left":
                return "right";
            case "right":
                return "left";
            default:
                return "up";
        }
    }

    public string getOutputDirection()
    {
        if(type == Type.canal_down_left)
        {
            if(flowStartedFromDirection == "down")
            {
                return "left";
            } else
            {
                return "down";
            }
        }
        if (type == Type.canal_down_right)
        {
            if (flowStartedFromDirection == "down")
            {
                return "right";
            }
            else
            {
                return "down";
            }
        }
        if (type == Type.canal_up_left)
        {
            if (flowStartedFromDirection == "up")
            {
                return "left";
            }
            else
            {
                return "up";
            }
        }
        if (type == Type.canal_up_right)
        {
            if (flowStartedFromDirection == "up")
            {
                return "right";
            }
            else
            {
                return "up";
            }
        }
        if(type == Type.canal)
        {
            if(flowStartedFromDirection == "up")
            {
                return "down";
            } else
            {
                return "up";
            }
        }
        if(type == Type.canal_horizontal)
        {
            if(flowStartedFromDirection == "right")
            {
                return "left";
            } else
            {
                return "right";
            }
        }
        return "down";
    }

    public void setFlowing(string direction)
    {
        if (!isFlowing)
        {
            flowStartedFromDirection = Tile.getOppositeDirection(direction);
            isFlowing = true;
            flowStartTime = Time.time;
            isBuildable = false;
            Debug.Log("starting flow direction: " + flowStartedFromDirection + "  for: " + type + " [" + x + "," + y + "]");
        }
    }
}

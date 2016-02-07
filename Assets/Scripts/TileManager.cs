using UnityEngine;
using System.Collections;

public class TileManager : MonoBehaviour
{
    public static TileManager instance { get; private set; }

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        this.enabled = false;
    }

    public void CreateTileOnGrid(GameObject tileObj, Vector2 pos)
    {
        int xIndex = (int)pos.x;
        int yIndex = (int)pos.y;

        Cell cell = Grid.instance.GetCellFromPos(pos);

        // already have a tile on grid
        if (cell.tile != null)
        {
            Debug.Log("failed create " + tileObj.name + " grid occupied with " + cell.tile.name);
            return;
        }

        GameObject obj = Instantiate(tileObj, pos, Quaternion.identity) as GameObject;

        cell.tile = obj;
    }

}

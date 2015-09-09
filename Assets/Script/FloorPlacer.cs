using UnityEngine;
using System.Collections;

public class FloorPlacer : MonoBehaviour
{
    public GameObject floor;

    private const int FloorTileCount = 1000;
    private int floorPlaced = 0;

    int lastTileX, lastTileY;



    public void Place()
    {
        // first floor 
        // we dont want floor on the border
        // or player will go through
        int x = Random.Range(1, Grid.instance.MAP_WIDTH - 1);
        int y = Random.Range(1, Grid.instance.MAP_HEIGHT - 1);

        // get the cell at this random index
        Cell cell = Grid.instance.mapGrid[x, y];
        Vector2 pos = Vector2.zero;
        pos.Set(x, y);

        Instantiate(floor, pos, Quaternion.identity);

        cell.isEmpty = false;

        floorPlaced++;

        // save info
        lastTileX = x;
        lastTileY = y;

        StartCoroutine(PlaceFloor());
    }

    private void MoveFloorRandom(ref int x, ref int y)
    {
        x = lastTileX;
        y = lastTileY;

        // goes to a random direction
        int dir = Random.Range(0, 8);
        switch (dir)
        {
            // left
            case 0:
                x -= 1;
                break;

            // up
            case 1:
                y += 1;
                break;

            // right
            case 2:
                x += 1;
                break;

            // down
            case 3:
                y -= 1;
                break;

            default:
                break;
        }
    }

    // Debug 
    //float placeWaitTime = 0.1f;
    public IEnumerator PlaceFloor()
    {
        int x = lastTileX;
        int y = lastTileY;

        // only move to non-border tile
        do
        {
            MoveFloorRandom(ref x, ref y);
        } while (x == 0 || x == Grid.instance.MAP_WIDTH || y == 0 || y == Grid.instance.MAP_HEIGHT);

        Cell cell = Grid.instance.mapGrid[x, y];
        if (cell.isEmpty)
        {
            Vector2 pos = Vector2.zero;
            pos.Set(x, y);

            GameObject obj = Instantiate(floor, pos, Quaternion.identity) as GameObject;
            obj.transform.parent = gameObject.transform;

            floorPlaced++;

            cell.isEmpty = false;
        }

        // save info
        lastTileX = x;
        lastTileY = y;

        //yield return new WaitForSeconds(placeWaitTime);

        if (floorPlaced < FloorTileCount)
        {
            yield return null;
            StartCoroutine(PlaceFloor());
        }
        else
        {
            //gameObject.SetActive(false);
            Debug.Log(floorPlaced);
            Debug.Log(gameObject.transform.childCount);
        }

        yield return null;
    }
}
using UnityEngine;
using System.Collections;

public class WallPlacer : MonoBehaviour
{
    public GameObject wallPrefab;

    public IEnumerator PlaceWall()
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

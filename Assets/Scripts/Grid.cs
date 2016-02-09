using UnityEngine;
using System.Collections;

public class Grid : MonoBehaviour
{
    public static int width = 100;
    public static int height = 100;
    public Cell[,] cells;

    public static Grid instance { get; private set; }

    private float cellSize = 1;
    private Vector2 originPos = Vector2.zero;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        this.enabled = false;

        // init grid
        cells = new Cell[100, 100];
        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                cells[j, i] = new Cell(i, j);
            }
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.white;

        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                Gizmos.DrawWireCube(new Vector2(i, j), Vector2.one);
            }
        }
    }

    public Cell RandCellInRange(int minX, int maxX, int minY, int maxY)
    {
        int x = Random.Range(minX, maxX);
        int y = Random.Range(minY, maxY);

        return cells[x, y];
    }

    public Cell GetCellFromPos(Vector2 pos)
    {
        pos.x -= originPos.x;
        pos.y -= originPos.y;

        int xIndex = Mathf.FloorToInt(pos.x / cellSize);
        int yIndex = Mathf.FloorToInt(pos.y / cellSize);

        //Debug.Log(xIndex + " ++ " + yIndex);

        return cells[xIndex, yIndex];
    }
}

public class Cell
{
    public int xIndex { get; private set; }
    public int yIndex { get; private set; }
    public Vector2 position { get; private set; }

    public GameObject tile;

    public Cell(int x, int y)
    {
        xIndex = x;
        yIndex = y;
        position = new Vector2(xIndex, yIndex);
    }
}

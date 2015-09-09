using UnityEngine;
using System.Collections;

public class Grid : MonoBehaviour
{
    public int MAP_WIDTH = 40;
    public int MAP_HEIGHT = 40;

    public Cell[,] mapGrid { get; private set; }

    public static Grid instance { get; private set; }

    FloorPlacer floorPlacer;
    WallPlacer wallPlacer;

    void Awake()
    {
        instance = this;

        floorPlacer = GetComponent<FloorPlacer>();
        wallPlacer = GetComponent<WallPlacer>();
    }

    void Start()
    {
        // init map grid
        mapGrid = new Cell[MAP_WIDTH, MAP_HEIGHT];

        for (int x = 0; x < MAP_WIDTH; x++)
            for (int y = 0; y < MAP_HEIGHT; y++)
                mapGrid[x, y] = new Cell(x, y);

        // start generating
        floorPlacer.Place();
    }
}

public class Cell
{
    public bool isEmpty;
    public int x, y;

    public Cell(int x, int y)
    {
        this.x = x;
        this.y = y;
        isEmpty = true;
    }
}

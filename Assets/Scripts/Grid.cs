using UnityEngine;
using System.Collections;

public class Grid : MonoBehaviour
{
	public static int width = 50;
	public static int height = 50;
	public Cell[,] cells;

	public static Grid instance{ get; private set; }

	void Awake ()
	{
		instance = this;
	}

	void Start ()
	{
		this.enabled = false;

		// init grid
		cells = new Cell[100, 100];
		for (int i = 0; i < height; i++) {
			for (int j = 0; j < width; j++) {
				cells [j, i] = new Cell (i, j);
			}
		}
	}

	public Cell RandCellInRange (int maxX, int maxY)
	{
		int x = Random.Range (0, maxX);
		int y = Random.Range (0, maxY);
	
		return  cells [x, y];
	}
}

public class Cell
{
	public int xIndex{ get; private set; }

	public int yIndex{ get; private set; }

	public Vector2 position{ get; private set; }

	public Cell (int x, int y)
	{
		xIndex = x;
		yIndex = y;
		position = new Vector2 (xIndex, yIndex);
	}
}

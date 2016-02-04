using UnityEngine;
using System.Collections;

public class Grid : MonoBehaviour
{
	public static int width = 100;
	public static int height = 100;
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

	void OnDrawGizmos ()
	{
		Gizmos.color = Color.white;

		for (int i = 0; i < height; i++) {
			for (int j = 0; j < width; j++) {
				Gizmos.DrawWireCube (new Vector2 (i, j), Vector2.one);
			}
		}
	}

	public Cell RandCellInRange (int minX, int maxX, int minY, int maxY)
	{
		int x = Random.Range (minX, maxX);
		int y = Random.Range (minY, maxY);
	
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

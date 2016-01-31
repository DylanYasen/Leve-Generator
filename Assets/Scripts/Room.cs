using UnityEngine;
using System.Collections;

public class Room : MonoBehaviour
{
	public Vector2 Position { get { return transform.position; } }

	public int Width { get { return _width; } }

	public int Height { get { return _height; } }

	public int ID { get { return _id; } set { _id = value; } }

	public Rect Rect { get { return _rect; } }

	public float rectMinX { get { return Rect.x - Rect.width / 2; } }

	public float rectMaxX { get { return Rect.x + Rect.width / 2; } }

	public float rectMinY { get { return Rect.y - Rect.height / 2; } }

	public float rectMaxY { get { return Rect.y + Rect.height / 2; } }

	public Collider2D Collider { get { return _collider; } }

	private int _width;
	private int _height;
	private int _id;
	private Rect _rect;
	private BoxCollider2D _collider;
	private Rigidbody2D _body2d;

	private Cell[,] cells;

	void Awake ()
	{
		_collider = gameObject.GetComponent<BoxCollider2D> ();
		_body2d = gameObject.GetComponent<Rigidbody2D> ();
	}

	public void SetPosition (Vector2 pos)
	{
		transform.position = pos;
	}

	public void SetSize (int width, int height)
	{
		_width = width;
		_height = height;

		// set collider
		// larger gap
		_collider.size = new Vector2 (width + 3, height + 3);

		// set cells
		cells = new Cell[width, height];
	}

	void OnTriggerStay2D (Collider2D other)
	{
		float xDir = other.transform.position.x - transform.position.x;
		float yDir = other.transform.position.y - transform.position.y;

		float xDiff = Mathf.Abs (xDir);
		float yDiff = Mathf.Abs (yDir);

		// go the larger differ axises
		if (xDiff > yDiff)
			transform.Translate (-Mathf.Sign (xDir) * Vector2.right);
		else
			transform.Translate (-Mathf.Sign (yDir) * Vector2.up);
	}

	void OnDrawGizmos ()
	{
		if (!Application.isPlaying)
			return;

		Gizmos.color = Color.red;

		Gizmos.DrawWireCube (transform.position, new Vector2 (Width, Height));
	}

	public void ResetRect ()
	{
		_rect = new Rect (Position.x, Position.y, _width, _height);
	}

	public void RemovePhysics ()
	{
		_rect = new Rect (Position.x, Position.y, _width, _height);

		DestroyImmediate (gameObject.GetComponent<Rigidbody2D> ());
		DestroyImmediate (gameObject.GetComponent<BoxCollider2D> ());
	}

	public void Fill ()
	{
		Vector2 pos = Vector2.zero;

		for (int i = 0; i < cells.GetLength (0); i++) {
			for (int j = 0; j < cells.GetLength (1); j++) {
				pos.Set (rectMinX + 0.5f + i, rectMinY + 0.5f + j);
				GameObject obj = Instantiate (Resources.Load<GameObject> ("Floor"), pos, Quaternion.identity) as GameObject;
			}
		}
	}
}
using UnityEngine;
using System.Collections;

public class Room : MonoBehaviour
{
	public Vector2 Position{ get { return transform.position; } }
	public int Width{ get { return _width; } }
	public int Height{ get { return _height; } }
	
	private int _width;
	private int _height;
	
	public void SetPosition (Vector2 pos)
	{
		transform.position = pos;
	}
	
	public void SetSize (int width, int height)
	{
		_width = width;
		_height = height;

		gameObject.GetComponent<BoxCollider2D> ().size = new Vector2 (width + 1, height + 1);
	}
	
	void OnDrawGizmos ()
	{
		if (!Application.isPlaying)
			return;
		
		Gizmos.color = Color.red;
		
		Gizmos.DrawWireCube (transform.position, new Vector2 (_width, _height));
	}

	public void RemovePhysics ()
	{
		DestroyImmediate (gameObject.GetComponent<Rigidbody2D> ());
		DestroyImmediate (gameObject.GetComponent<BoxCollider2D> ());
	}
}
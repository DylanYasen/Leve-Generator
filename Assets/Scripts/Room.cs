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

    private int _width;
    private int _height;
    private int _id;
    private Rect _rect;

    public void SetPosition(Vector2 pos)
    {
        transform.position = pos;
    }

    public void SetSize(int width, int height)
    {
        _width = width;
        _height = height;

        gameObject.GetComponent<BoxCollider2D>().size = new Vector2(width + 1, height + 1);

    }

    void OnDrawGizmos()
    {
        if (!Application.isPlaying)
            return;

        Gizmos.color = Color.red;

        Gizmos.DrawWireCube(transform.position, new Vector2(Width, Height));
    }

    public void RemovePhysics()
    {
        _rect = new Rect(Position.x, Position.y, _width, _height);

        DestroyImmediate(gameObject.GetComponent<Rigidbody2D>());
        DestroyImmediate(gameObject.GetComponent<BoxCollider2D>());
    }

}
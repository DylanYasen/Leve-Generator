using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Delaunay;
using Delaunay.Geo;

public class DungeonGenerator : MonoBehaviour
{
    public GameObject roomPrefab;

    private List<Room> rooms;
    private int roomId;

    void Start()
    {
        rooms = new List<Room>();

        StartCoroutine(Create());
    }

    IEnumerator Create()
    {
        #region Create Random Size & Pos Rooms 
        int counter = 0;
        int roomAmount = 50;
        int widthSum = 0;
        int heightSum = 0;
        while (counter < roomAmount)
        {
            Vector2 randPoint = GetRandPointInRadius(6);
            int randWidth = Random.Range(1, 10);
            int randHeight = Random.Range(1, 10);

            Room room = CreateRandRoom(randPoint, randWidth, randHeight);
            rooms.Add(room);
            counter++;

            // sum width/height
            widthSum += randWidth;
            heightSum += randHeight;

            yield return new WaitForSeconds(0.1f);
        }
        #endregion

        #region Remove Physics & Small Rooms
        float widthMean = widthSum / roomAmount;
        float heightMean = heightSum / roomAmount;

        yield return new WaitForSeconds(4);
        for (int i = 0; i < rooms.Count; i++)
        {
            rooms[i].RemovePhysics();

            if (rooms[i].Width < widthMean || rooms[i].Height < heightMean)
            {
                Destroy(rooms[i].gameObject);
                rooms.Remove(rooms[i]);
                i--;
            }
        }
        #endregion

        #region  Create room connection using DelaunayTriangles
        _points = new List<Vector2>();
        for (int i = 0; i < rooms.Count; i++)
            _points.Add(rooms[i].Position);
        rect = GetMinRect(rooms);

        Voronoi v = new Voronoi(_points, null, rect);
        _edges = v.VoronoiDiagram();
        _spanningTree = v.SpanningTree(KruskalType.MINIMUM);
        _delaunayTriangulation = v.DelaunayTriangulation();
        #endregion

        #region Extra Loop Connection
        int maxExtraPathAmt = (int)(_delaunayTriangulation.Count * 0.1f);
        int rand = Random.Range(1, maxExtraPathAmt);
        Debug.Log(rand);

        while (rand > 0)
        {
            int randIndex = Random.Range(0, _delaunayTriangulation.Count);

            if (_spanningTree.Contains(_delaunayTriangulation[randIndex]))
                continue;

            else
            {
                _spanningTree.Add(_delaunayTriangulation[randIndex]);
                rand--;
            }
        }
        #endregion

        #region Create HallWays

        for (int i = 0; i < _spanningTree.Count; i++)
        {
            Room room0 = GetRoomFromPos(_spanningTree[i].p0);
            Room room1 = GetRoomFromPos(_spanningTree[i].p1);

            float xDistance = Mathf.Abs(room0.Position.x - room1.Position.x);
            float yDistance = Mathf.Abs(room0.Position.y - room1.Position.y);

            float xMidPointDistance = xDistance / 2;
            float yMidPointDistance = yDistance / 2;

            // close enough in x axis
            // mid x point is inside of both rects
            if (room0.Position.x + xMidPointDistance <= room0.rectMaxX)
            {
                if (room1.Position.x + xMidPointDistance <= room1.rectMaxX)
                {
                    Vector2 startPoint = Vector2.zero;
                    Vector2 endPoint = Vector2.zero;

                    // room0 above room1
                    if (room0.Position.y > room1.Position.y)
                    {
                        // room0 left to room1
                        if (room0.Position.x <= room1.Position.x)
                        {
                            startPoint.Set(room0.Position.x + xMidPointDistance, room0.rectMinY);
                            endPoint.Set(room1.Position.x - xMidPointDistance, room1.rectMaxY);
                        }
                        // room1 left to room0
                        else if (room0.Position.x > room1.Position.x)
                        {
                            startPoint.Set(room0.Position.x - xMidPointDistance, room0.rectMinY);
                            endPoint.Set(room1.Position.x + xMidPointDistance, room1.rectMaxY);
                        }
                    }

                    // room1 above room0
                    else if (room0.Position.y < room1.Position.y)
                    {
                        // room0 left to room1
                        if (room0.Position.x <= room1.Position.x)
                        {
                            startPoint.Set(room0.Position.x + xMidPointDistance, room0.rectMaxY);
                            endPoint.Set(room1.Position.x - xMidPointDistance, room1.rectMinY);
                        }
                        // room1 left to room0
                        else if (room0.Position.x > room1.Position.x)
                        {
                            startPoint.Set(room0.Position.x - xMidPointDistance, room0.rectMaxY);
                            endPoint.Set(room1.Position.x + xMidPointDistance, room1.rectMinY);
                        }
                    }

                    // create vertical line
                    _hallways.Add(new HallWay(startPoint, endPoint, room0, room1));
                }
            }

            // close enough in y axis
            // mid y point is inside of both rects
            else if (room0.Position.y + yMidPointDistance <= room0.rectMaxY)
            {
                if (room1.Position.y + yMidPointDistance <= room1.rectMaxY)
                {
                    Vector2 startPoint = Vector2.zero;
                    Vector2 endPoint = Vector2.zero;

                    // room0 above room1
                    if (room0.Position.y > room1.Position.y)
                    {
                        // room0 left to room1
                        if (room0.Position.x <= room1.Position.x)
                        {
                            startPoint.Set(room0.rectMaxX, room0.Position.y - yMidPointDistance);
                            endPoint.Set(room1.rectMinX, room1.Position.y + yMidPointDistance);
                        }
                        // room1 left to room0
                        else if (room0.Position.x > room1.Position.x)
                        {
                            startPoint.Set(room0.rectMinX, room0.Position.y - yMidPointDistance);
                            endPoint.Set(room1.rectMaxX, room1.Position.y + yMidPointDistance);
                        }
                    }

                    // room1 above room0
                    else if (room0.Position.y < room1.Position.y)
                    {
                        // room0 left to room1
                        if (room0.Position.x <= room1.Position.x)
                        {
                            startPoint.Set(room0.rectMaxX, room0.Position.y + yMidPointDistance);
                            endPoint.Set(room1.rectMinX, room1.Position.y - yMidPointDistance);
                        }
                        // room1 left to room0
                        else if (room0.Position.x > room1.Position.x)
                        {
                            startPoint.Set(room0.rectMinX, room0.Position.y + yMidPointDistance);
                            endPoint.Set(room1.rectMaxX, room1.Position.y - yMidPointDistance);
                        }
                    }

                    // create vertical line
                    _hallways.Add(new HallWay(startPoint, endPoint, room0, room1));
                }
            }

            // not close in both axis
            else
            {

            }

        }

        #endregion
    }

    Rect rect;

    List<LineSegment> _edges = null;
    List<LineSegment> _spanningTree = null;
    List<LineSegment> _delaunayTriangulation = null;
    List<Vector2> _points = null;
    List<HallWay> _hallways = new List<HallWay>();


    void OnDrawGizmos()
    {
        Gizmos.color = Color.black;

        if (rect != null)
            Gizmos.DrawWireCube(rect.position, new Vector2(rect.width, rect.height));

        Gizmos.color = Color.red;
        if (_points != null)
        {
            for (int i = 0; i < _points.Count; i++)
            {
                Gizmos.DrawSphere(_points[i], 0.2f);
            }
        }
        //if (_spanningTree != null)
        //{
        //    Gizmos.color = Color.green;
        //    for (int i = 0; i < _spanningTree.Count; i++)
        //    {
        //        LineSegment seg = _spanningTree[i];
        //        Vector2 left = (Vector2)seg.p0;
        //        Vector2 right = (Vector2)seg.p1;
        //        Gizmos.DrawLine((Vector3)left, (Vector3)right);
        //    }
        //}

        if (_hallways != null)
        {
            Gizmos.color = Color.yellow;
            for (int i = 0; i < _hallways.Count; i++)
            {
                Gizmos.DrawLine(_hallways[i].startPos, _hallways[i].endPos);
            }
        }

        //if (_edges != null)
        //{
        //    Gizmos.color = Color.white;
        //    for (int i = 0; i < _edges.Count; i++)
        //    {
        //        Vector2 left = (Vector2)_edges[i].p0;
        //        Vector2 right = (Vector2)_edges[i].p1;
        //        Gizmos.DrawLine((Vector3)left, (Vector3)right);
        //    }
        //}

        //Gizmos.color = Color.magenta;
        //if (_delaunayTriangulation != null)
        //{
        //    for (int i = 0; i < _delaunayTriangulation.Count; i++)
        //    {
        //        Vector2 left = (Vector2)_delaunayTriangulation[i].p0;
        //        Vector2 right = (Vector2)_delaunayTriangulation[i].p1;
        //        Gizmos.DrawLine((Vector3)left, (Vector3)right);
        //    }
        //}

    }

    public Vector2 GetRandPointInRadius(float radius)
    {
        return Random.insideUnitCircle * radius;
    }

    public Room CreateRandRoom(Vector2 pos, int width, int height)
    {
        GameObject roomObj = Instantiate(roomPrefab, pos, Quaternion.identity) as GameObject;
        Room room = roomObj.GetComponent<Room>();

        // random pos,size,id
        room.SetSize(width, height);
        room.SetPosition(pos);
        room.ID = roomId++;

        roomObj.name = roomId.ToString();

        return room;
    }

    /*
    void DelaunayTriangles(List<Vector2> points, Rect rect)
    {
        Voronoi v = new Voronoi(points, null, rect);

        m_edges = v.VoronoiDiagram();
        m_spanningTree = v.SpanningTree(KruskalType.MINIMUM);
        m_delaunayTriangulation = v.DelaunayTriangulation();
    }
    */

    Rect GetMinRect(List<Room> rects)
    {
        Vector2 min = Vector2.zero;
        Vector2 max = Vector2.zero;

        for (int i = 0; i < rects.Count; i++)
        {
            min.x = Mathf.Min(min.x, rects[i].rectMinX);
            min.y = Mathf.Min(min.y, rects[i].rectMinY);

            max.x = Mathf.Max(max.x, rects[i].rectMaxX);
            max.y = Mathf.Max(max.y, rects[i].rectMaxY);
        }

        float centerX = min.x + (max.x - min.x) / 2;
        float centerY = min.y + (max.y - min.y) / 2;
        float width = max.x - min.x;
        float height = max.y - min.y;

        return new Rect(centerX, centerY, width, height);
    }

    Rect GetMinRect(Room[] rects)
    {
        Vector2 min = Vector2.zero;
        Vector2 max = Vector2.zero;

        for (int i = 0; i < rects.Length; i++)
        {
            min.x = Mathf.Min(min.x, rects[i].rectMinX);
            min.y = Mathf.Min(min.y, rects[i].rectMinY);

            max.x = Mathf.Max(max.x, rects[i].rectMaxX);
            max.y = Mathf.Max(max.y, rects[i].rectMaxY);
        }

        float centerX = min.x + (max.x - min.x) / 2;
        float centerY = min.y + (max.y - min.y) / 2;
        float width = max.x - min.x;
        float height = max.y - min.y;

        return new Rect(centerX, centerY, width, height);
    }

    public Room GetRoomFromPos(Vector2? pos)
    {
        for (int i = 0; i < rooms.Count; i++)
        {
            if (rooms[i].Position == pos)
                return rooms[i];
        }

        return null;
    }
}

public class HallWay
{
    public Vector2 startPos;
    public Vector2 endPos;
    public Room room0;
    public Room room1;

    public HallWay(Vector2 start, Vector2 end, Room room0, Room room1)
    {
        this.startPos = start;
        this.endPos = end;
        this.room0 = room0;
        this.room1 = room1;
    }
}

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

    // debug
    public bool ShowSpanningTree;
    public bool ShowTriangulation;

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
            Vector2 randPoint = GetRandPointInRadius(20);
            int randWidth = Random.Range(1, 15);
            int randHeight = Random.Range(1, 15);

            Room room = CreateRandRoom(randPoint, randWidth, randHeight);
            room.name = counter.ToString();
            rooms.Add(room);
            counter++;

            // sum width/height
            widthSum += randWidth;
            heightSum += randHeight;

            yield return new WaitForSeconds(0.05f);
        }
        #endregion

        #region Remove Small Rooms
        float widthMean = widthSum / roomAmount;
        float heightMean = heightSum / roomAmount;

        yield return new WaitForSeconds(5);
        for (int i = 0; i < rooms.Count; i++)
        {
            //rooms[i].RemovePhysics();
            rooms[i].Reset();

            if (rooms[i].Width < widthMean || rooms[i].Height < heightMean)
            {
                Destroy(rooms[i].gameObject);
                rooms.Remove(rooms[i]);
                i--;
            }
        }
        yield return new WaitForSeconds(0.5f);
        #endregion

        #region  Create room connection using DelaunayTriangles
        _points = new List<Vector2>();
        for (int i = 0; i < rooms.Count; i++)
            _points.Add(rooms[i].Position);
        Rect rect = GetMinRect(rooms);

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
        yield return new WaitForSeconds(0.5f);
        #endregion

        #region Create HallWays

        for (int i = 0; i < _spanningTree.Count; i++)
        {
            yield return new WaitForSeconds(0.2f);
            Room room0 = GetRoomFromPos(_spanningTree[i].p0);
            Room room1 = GetRoomFromPos(_spanningTree[i].p1);

            Debug.Log("Creating Hallway Between " + room0.name + "  " + room1.name);

            float xDistance = Mathf.Abs(room0.Position.x - room1.Position.x);
            float yDistance = Mathf.Abs(room0.Position.y - room1.Position.y);

            float xMidPointDistance = xDistance / 2;
            float yMidPointDistance = yDistance / 2;

            #region Close in X-Axis
            // mid x point is inside of both rects
            if (room0.Position.x + xMidPointDistance <= room0.rectMaxX
                && room1.Position.x + xMidPointDistance <= room1.rectMaxX)
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

                #endregion

                // create vertical line
                _hallways.Add(new HallWay(startPoint, endPoint, room0, room1));
                Debug.Log("##CloseXAxis created hallway from " + startPoint + " to " + endPoint + " between " + room0.name + "  " + room1.name);
            }

            #region Close In Y-Axis
            // mid y point is inside of both rects
            else if (room0.Position.y + yMidPointDistance <= room0.rectMaxY
                    && room1.Position.y + yMidPointDistance <= room1.rectMaxY)
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
                Debug.Log("##CloseYAxis created hallway from " + startPoint + " to " + endPoint + " between " + room0.name + "  " + room1.name);
            }
            #endregion

            #region Far In Both Axis
            // create L shape hall way
            else
            {
                Vector2 startPoint = Vector2.zero;
                Vector2 turnPoint = Vector2.zero;
                Vector2 endPoint = Vector2.zero;

                // room0 above room1
                if (room0.Position.y >= room1.Position.y)
                {
                    // room0 left to room1
                    if (room0.Position.x <= room1.Position.x)
                    {
                        startPoint.Set(room0.rectMaxX, room0.Position.y);
                        turnPoint.Set(room0.Position.x + xDistance, room0.Position.y);
                        endPoint.Set(room1.Position.x, room1.rectMaxY);

                        #region Check If Line Collider With Other Room
                        if (LineHasCollision(startPoint, turnPoint, room0.Collider)
                           ||
                           LineHasCollision(turnPoint, endPoint, room1.Collider)
                           )
                        {
                            // go other way
                            startPoint.Set(room0.Position.x, room0.rectMinY);
                            turnPoint.Set(room0.Position.x, room0.Position.y - yDistance);
                            endPoint.Set(room1.rectMinX, room1.Position.y);
                            Debug.Log("go other way");
                        }

                        // still has collision, delete this segment from spanning tree
                        if (LineHasCollision(startPoint, turnPoint, room0.Collider)
                            ||
                            LineHasCollision(turnPoint, endPoint, room1.Collider)
                            )
                        {
                            Debug.Log("still collision, remove path");

                            _spanningTree.RemoveAt(i);
                            i--;
                            continue;
                        }
                        #endregion
                    }

                    // room1 left to room0
                    else if (room0.Position.x > room1.Position.x)
                    {
                        startPoint.Set(room0.rectMinX, room0.Position.y);
                        turnPoint.Set(room0.Position.x - xDistance, room0.Position.y);
                        endPoint.Set(room1.Position.x, room1.rectMaxY);

                        #region Check If Line Collider With Other Room
                        if (LineHasCollision(startPoint, turnPoint, room0.Collider)
                           ||
                           LineHasCollision(turnPoint, endPoint, room1.Collider)
                           )
                        {
                            // go other way
                            startPoint.Set(room0.Position.x, room0.rectMinY);
                            turnPoint.Set(room0.Position.x, room0.Position.y - yDistance);
                            endPoint.Set(room1.rectMaxX, room1.Position.y);

                            Debug.Log("go other way");
                        }

                        // still has collision, delete this segment from spanning tree
                        if (LineHasCollision(startPoint, turnPoint, room0.Collider)
                            ||
                            LineHasCollision(turnPoint, endPoint, room1.Collider)
                            )
                        {
                            Debug.Log("still collison, delete path");

                            _spanningTree.RemoveAt(i);
                            i--;
                            continue;
                        }
                        #endregion
                    }
                }

                // room1 above room0
                else if (room0.Position.y < room1.Position.y)
                {
                    // room0 left to room1
                    if (room0.Position.x <= room1.Position.x)
                    {
                        startPoint.Set(room0.Position.x, room0.rectMaxY);
                        turnPoint.Set(room0.Position.x, room0.Position.y + yDistance);
                        endPoint.Set(room1.rectMinX, room1.Position.y);

                        #region Check If Line Collider With Other Room
                        if (LineHasCollision(startPoint, turnPoint, room0.Collider)
                           ||
                           LineHasCollision(turnPoint, endPoint, room1.Collider)
                           )
                        {
                            // go other way
                            startPoint.Set(room0.rectMaxX, room0.Position.y);
                            turnPoint.Set(room0.Position.x + xDistance, room0.Position.y);
                            endPoint.Set(room1.Position.x, room1.rectMinY);

                            Debug.Log("go other way");
                        }

                        // still has collision, delete this segment from spanning tree
                        if (LineHasCollision(startPoint, turnPoint, room0.Collider)
                            ||
                            LineHasCollision(turnPoint, endPoint, room1.Collider)
                            )
                        {
                            Debug.Log("still collision, delete path");
                            _spanningTree.RemoveAt(i);
                            i--;
                            continue;
                        }
                        #endregion
                    }
                    // room1 left to room0
                    else if (room0.Position.x > room1.Position.x)
                    {
                        startPoint.Set(room1.rectMaxX, room1.Position.y);
                        turnPoint.Set(room1.Position.x + xDistance, room1.Position.y);
                        endPoint.Set(room0.Position.x, room0.rectMaxY);

                        #region Check If Line Collider With Other Room
                        if (LineHasCollision(startPoint, turnPoint, room1.Collider)
                           ||
                           LineHasCollision(turnPoint, endPoint, room0.Collider)
                           )
                        {
                            // go other way
                            startPoint.Set(room1.Position.x, room1.rectMinY);
                            turnPoint.Set(room1.Position.x, room1.Position.y - yDistance);
                            endPoint.Set(room0.rectMinX, room0.Position.y);

                            Debug.Log("go other way");
                        }

                        // still has collision, delete this segment from spanning tree
                        if (LineHasCollision(startPoint, turnPoint, room1.Collider)
                            ||
                            LineHasCollision(turnPoint, endPoint, room0.Collider)
                            )
                        {
                            Debug.Log("still collision, delete path");
                            _spanningTree.RemoveAt(i);
                            i--;
                            continue;
                        }
                        #endregion
                    }
                }

                // create vertical line
                _hallways.Add(new LHallWay(startPoint, turnPoint, endPoint, room0, room1));
                Debug.Log("##Lshape created hallway from " + startPoint + " to " + turnPoint + " to " + endPoint + " between " + room0.name + "  " + room1.name);
            }
            #endregion
        }

        #endregion

        #region Remove Physics
        for (int i = 0; i < rooms.Count; i++)
            rooms[i].RemovePhysics();
        #endregion

        //#region Create Tiles
        //for (int i = 0; i < rooms.Count; i++)
        //{
        //    rooms[i].Fill();
        //    yield return new WaitForSeconds(0.2f);
        //}
        //for (int i = 0; i < _hallways.Count; i++)
        //{
        //    _hallways[i].Fill();
        //    yield return new WaitForSeconds(0.2f);
        //}
        //#endregion
    }

    List<LineSegment> _edges = null;
    List<LineSegment> _spanningTree = null;
    List<LineSegment> _delaunayTriangulation = null;
    List<Vector2> _points = null;
    List<HallWay> _hallways = new List<HallWay>();


    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        if (_points != null)
        {
            for (int i = 0; i < _points.Count; i++)
            {
                Gizmos.DrawSphere(_points[i], 0.2f);
            }
        }
        if (_spanningTree != null && ShowSpanningTree)
        {
            Gizmos.color = Color.green;
            for (int i = 0; i < _spanningTree.Count; i++)
            {
                LineSegment seg = _spanningTree[i];
                Vector2 left = (Vector2)seg.p0;
                Vector2 right = (Vector2)seg.p1;
                Gizmos.DrawLine((Vector3)left, (Vector3)right);
            }
        }

        if (_hallways != null)
        {
            Gizmos.color = Color.yellow;
            for (int i = 0; i < _hallways.Count; i++)
            {
                if (_hallways[i] is LHallWay)
                {
                    LHallWay hallway = (LHallWay)_hallways[i];
                    Gizmos.DrawLine(hallway.startPos, hallway.turnPos);
                    Gizmos.DrawLine(hallway.turnPos, hallway.endPos);
                }
                else
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

        Gizmos.color = Color.magenta;
        if (_delaunayTriangulation != null && ShowTriangulation)
        {
            for (int i = 0; i < _delaunayTriangulation.Count; i++)
            {
                Vector2 left = (Vector2)_delaunayTriangulation[i].p0;
                Vector2 right = (Vector2)_delaunayTriangulation[i].p1;
                Gizmos.DrawLine((Vector3)left, (Vector3)right);
            }
        }
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

    public bool LineSegCollision(Vector2 line1Start, Vector2 line1End, Vector2 line2Start, Vector2 line2End)
    {
        /* First line segment */
        float x1 = line1Start.x;
        float y1 = line1Start.y;
        float x2 = line1End.x;
        float y2 = line1End.y;

        /* Second line segment */
        float x3 = line2Start.x;
        float y3 = line2Start.y;
        float x4 = line2End.x;
        float y4 = line2End.y;

        float a1, a2, b1, b2, c1, c2; /* Coefficients of line eqns. */
        float r1, r2, r3, r4;         /* 'Sign' values */
        float denom, offset, num;     /* Intermediate values */

        /* Compute a1, b1, c1, where line joining points 1 and 2
         * is "a1 x  +  b1 y  +  c1  =  0".
         */
        a1 = y2 - y1;
        b1 = x1 - x2;
        c1 = x2 * y1 - x1 * y2;

        /* Compute r3 and r4.
         */

        r3 = a1 * x3 + b1 * y3 + c1;
        r4 = a1 * x4 + b1 * y4 + c1;

        /* Check signs of r3 and r4.  If both point 3 and point 4 lie on
         * same side of line 1, the line segments do not intersect.
         */

        if (r3 != 0 && r4 != 0 && (Mathf.Sign(r3) == Mathf.Sign(r4)))
            return (false);

        /* Compute a2, b2, c2 */

        a2 = y4 - y3;
        b2 = x3 - x4;
        c2 = x4 * y3 - x3 * y4;

        /* Compute r1 and r2 */

        r1 = a2 * x1 + b2 * y1 + c2;
        r2 = a2 * x2 + b2 * y2 + c2;

        /* Check signs of r1 and r2.  If both point 1 and point 2 lie
         * on same side of second line segment, the line segments do
         * not intersect.
         */

        if (r1 != 0 && r2 != 0 && (Mathf.Sign(r3) == Mathf.Sign(r4)))
            return (false);

        /* Line segments intersect: compute intersection point. 
         */

        denom = a1 * b2 - a2 * b1;
        if (denom == 0)
            return (true);
        //offset = denom < 0 ? -denom / 2 : denom / 2;

        ///* The denom/2 is to get rounding instead of truncating.  It
        // * is added or subtracted to the numerator, depending upon the
        // * sign of the numerator.
        // */

        //num = b1 * c2 - b2 * c1;
        //*x = (num < 0 ? num - offset : num + offset) / denom;

        //num = a2 * c1 - a1 * c2;
        //*y = (num < 0 ? num - offset : num + offset) / denom;

        return (false);
    }

    public bool LineHasCollision(Vector2 startPoint, Vector2 endPoint, Collider2D ignoreCollider)
    {
        float dis = Vector2.Distance(startPoint, endPoint);
        Vector2 dir = (endPoint - startPoint).normalized;
        RaycastHit2D hit2d = Physics2D.Raycast(startPoint, dir, dis);

        // colliding with other collider
        if (hit2d.collider != null && hit2d.collider != ignoreCollider)
            return true;

        return false;
    }
}

public class HallWay
{
    public Vector2 startPos;
    public Vector2 endPos;
    public Room room0;
    public Room room1;

    private Cell[,] cells;

    public HallWay(Vector2 start, Vector2 end, Room room0, Room room1)
    {
        this.startPos = start;
        this.endPos = end;
        this.room0 = room0;
        this.room1 = room1;

        int xCount = (int)(Mathf.Abs(endPos.x - startPos.x));
        int yCount = (int)(Mathf.Abs(endPos.y - startPos.y));

        cells = new Cell[xCount + 2, yCount];
    }

    public void Fill()
    {
        Vector2 pos = Vector2.zero;
        for (int i = 0; i < cells.GetLength(0); i++)
        {
            for (int j = 0; j < cells.GetLength(1); j++)
            {
                pos.Set(startPos.x - 1 + 0.5f + i, startPos.y + 0.5f + j);
                GameObject obj = MonoBehaviour.Instantiate(Resources.Load<GameObject>("Floor"), pos, Quaternion.identity) as GameObject;
            }
        }
    }
}

public class LHallWay : HallWay
{
    public Vector2 turnPos;

    public LHallWay(Vector2 start, Vector2 turn, Vector2 end, Room room0, Room room1) : base(start, end, room0, room1)
    {
        this.turnPos = turn;
    }
}

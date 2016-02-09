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

	List<LineSegment> _edges = null;
	List<LineSegment> _spanningTree = null;
	List<LineSegment> _delaunayTriangulation = null;
	List<Vector2> _points = null;
	List<HallWay> _hallways = new List<HallWay> ();

	void Start ()
	{
		rooms = new List<Room> ();

		StartCoroutine (Create ());
	}

	IEnumerator Create ()
	{
		#region Create Random Size & Pos Rooms 
		int counter = 0;
		int roomAmount = 30;
		int widthSum = 0;
		int heightSum = 0;
		while (counter < roomAmount) {
			Room room = CreateRandRoom ();
			room.name = counter.ToString ();
			rooms.Add (room);
			counter++;

			// sum width/height
			widthSum += room.Width;
			heightSum += room.Height;

			yield return new WaitForSeconds (0.05f);
		}
		#endregion

		#region Remove Small Rooms
		float widthMean = widthSum / roomAmount;
		float heightMean = heightSum / roomAmount;

		yield return new WaitForSeconds (5);
		for (int i = 0; i < rooms.Count; i++) {
			//rooms[i].RemovePhysics();
			rooms [i].ResetRect ();

			if (rooms [i].Width < widthMean || rooms [i].Height < heightMean) {
				Destroy (rooms [i].gameObject);
				rooms.Remove (rooms [i]);
				i--;
			}
		}
		yield return new WaitForSeconds (0.5f);
		#endregion

		#region  Create room connection using DelaunayTriangles
		_points = new List<Vector2> ();
		for (int i = 0; i < rooms.Count; i++)
			_points.Add (rooms [i].Position);
		Rect rect = GetMinRect (rooms);

		Voronoi v = new Voronoi (_points, null, rect);
		_edges = v.VoronoiDiagram ();
		_spanningTree = v.SpanningTree (KruskalType.MINIMUM);
		_delaunayTriangulation = v.DelaunayTriangulation ();
		#endregion

		#region Extra Loop Connection
		int maxExtraPathAmt = (int)(_delaunayTriangulation.Count * 0.1f);
		int rand = Random.Range (1, maxExtraPathAmt);
		Debug.Log (rand);

		while (rand > 0) {
			int randIndex = Random.Range (0, _delaunayTriangulation.Count);

			if (_spanningTree.Contains (_delaunayTriangulation [randIndex]))
				continue;
			else {
				_spanningTree.Add (_delaunayTriangulation [randIndex]);
				rand--;
			}
		}
		yield return new WaitForSeconds (0.5f);
		#endregion

		#region Create HallWays
		for (int i = 0; i < _spanningTree.Count; i++) {
			yield return new WaitForSeconds (0.2f);
			Room room0 = GetRoomFromPos (_spanningTree [i].p0);
			Room room1 = GetRoomFromPos (_spanningTree [i].p1);

			Debug.Log ("Creating Hallway Between " + room0.name + "  " + room1.name);

			float xDistance = Mathf.Abs (room0.Position.x - room1.Position.x);
			float yDistance = Mathf.Abs (room0.Position.y - room1.Position.y);

			float xMidPointDistance = xDistance / 2;
			float yMidPointDistance = yDistance / 2;

			#region Close in X-Axis
			// mid x point is inside of both rects
			if (room0.Position.x + xMidPointDistance < room0.rectMaxX// removed equal sign ==> 避免房间刚好快要重叠，防止做路径时麻烦，因为路径瓦片可能会刚好在网格线上
			    && room1.Position.x + xMidPointDistance < room1.rectMaxX) {
				Vector2 startPoint = Vector2.zero;
				Vector2 endPoint = Vector2.zero;

				// room0 above room1
				if (room0.Position.y > room1.Position.y) {
					// room0 left to room1
					if (room0.Position.x <= room1.Position.x) {
						startPoint.Set (room0.Position.x + xMidPointDistance, room0.rectMinY);
						endPoint.Set (room1.Position.x - xMidPointDistance, room1.rectMaxY);
					}
                    // room1 left to room0
                    else if (room0.Position.x > room1.Position.x) {
						startPoint.Set (room0.Position.x - xMidPointDistance, room0.rectMinY);
						endPoint.Set (room1.Position.x + xMidPointDistance, room1.rectMaxY);
					}
				}

                // room1 above room0
                else if (room0.Position.y < room1.Position.y) {
					// room0 left to room1
					if (room0.Position.x <= room1.Position.x) {
						startPoint.Set (room0.Position.x + xMidPointDistance, room0.rectMaxY);
						endPoint.Set (room1.Position.x - xMidPointDistance, room1.rectMinY);
					}
                    // room1 left to room0
                    else if (room0.Position.x > room1.Position.x) {
						startPoint.Set (room0.Position.x - xMidPointDistance, room0.rectMaxY);
						endPoint.Set (room1.Position.x + xMidPointDistance, room1.rectMinY);
					}
				}

				#endregion

				// create vertical line
				HallWay hallway = new HallWay (startPoint, endPoint, room0, room1);
				_hallways.Add (hallway);
				room0.AddHallWay (hallway);
				Debug.Log ("##CloseXAxis created hallway from " + startPoint + " to " + endPoint + " between " + room0.name + "  " + room1.name);
			}

            #region Close In Y-Axis
            // mid y point is inside of both rects
            else if (room0.Position.y + yMidPointDistance < room0.rectMaxY
			                  && room1.Position.y + yMidPointDistance < room1.rectMaxY) {
				Vector2 startPoint = Vector2.zero;
				Vector2 endPoint = Vector2.zero;

				// room0 above room1
				if (room0.Position.y > room1.Position.y) {
					// room0 left to room1
					if (room0.Position.x <= room1.Position.x) {
						startPoint.Set (room0.rectMaxX, room0.Position.y - yMidPointDistance);
						endPoint.Set (room1.rectMinX, room1.Position.y + yMidPointDistance);
					}
                    // room1 left to room0
                    else if (room0.Position.x > room1.Position.x) {
						startPoint.Set (room0.rectMinX, room0.Position.y - yMidPointDistance);
						endPoint.Set (room1.rectMaxX, room1.Position.y + yMidPointDistance);
					}
				}

                // room1 above room0
                else if (room0.Position.y < room1.Position.y) {
					// room0 left to room1
					if (room0.Position.x <= room1.Position.x) {
						startPoint.Set (room0.rectMaxX, room0.Position.y + yMidPointDistance);
						endPoint.Set (room1.rectMinX, room1.Position.y - yMidPointDistance);
					}
                    // room1 left to room0
                    else if (room0.Position.x > room1.Position.x) {
						startPoint.Set (room0.rectMinX, room0.Position.y + yMidPointDistance);
						endPoint.Set (room1.rectMaxX, room1.Position.y - yMidPointDistance);
					}
				}

				// create vertical line
				HallWay hallway = new HallWay (startPoint, endPoint, room0, room1);
				_hallways.Add (hallway);
				room0.AddHallWay (hallway);

				Debug.Log ("##CloseYAxis created hallway from " + startPoint + " to " + endPoint + " between " + room0.name + "  " + room1.name);
			}
            #endregion

            #region Far In Both Axis
            // create L shape hall way
            else {
				Vector2 startPoint = Vector2.zero;
				Vector2 turnPoint = Vector2.zero;
				Vector2 endPoint = Vector2.zero;

				// room0 above room1
				if (room0.Position.y >= room1.Position.y) {
					// room0 left to room1
					if (room0.Position.x <= room1.Position.x) {
						startPoint.Set (room0.rectMaxX, room0.Position.y);
						turnPoint.Set (room0.Position.x + xDistance, room0.Position.y);
						endPoint.Set (room1.Position.x, room1.rectMaxY);

						#region Check If Line Collider With Other Room
						if (LineHasCollision (startPoint, turnPoint, room0.Collider)
						    ||
						    LineHasCollision (turnPoint, endPoint, room1.Collider)) {
							// go other way
							startPoint.Set (room0.Position.x, room0.rectMinY);
							turnPoint.Set (room0.Position.x, room0.Position.y - yDistance);
							endPoint.Set (room1.rectMinX, room1.Position.y);
							Debug.Log ("go other way");
						}

						// still has collision, delete this segment from spanning tree
						if (LineHasCollision (startPoint, turnPoint, room0.Collider)
						    ||
						    LineHasCollision (turnPoint, endPoint, room1.Collider)) {
							Debug.Log ("still collision, remove path");

							_spanningTree.RemoveAt (i);
							i--;
							continue;
						}
						#endregion
					}

                    // room1 left to room0
                    else if (room0.Position.x > room1.Position.x) {
						startPoint.Set (room0.rectMinX, room0.Position.y);
						turnPoint.Set (room0.Position.x - xDistance, room0.Position.y);
						endPoint.Set (room1.Position.x, room1.rectMaxY);

						#region Check If Line Collider With Other Room
						if (LineHasCollision (startPoint, turnPoint, room0.Collider)
						    ||
						    LineHasCollision (turnPoint, endPoint, room1.Collider)) {
							// go other way
							startPoint.Set (room0.Position.x, room0.rectMinY);
							turnPoint.Set (room0.Position.x, room0.Position.y - yDistance);
							endPoint.Set (room1.rectMaxX, room1.Position.y);

							Debug.Log ("go other way");
						}

						// still has collision, delete this segment from spanning tree
						if (LineHasCollision (startPoint, turnPoint, room0.Collider)
						    ||
						    LineHasCollision (turnPoint, endPoint, room1.Collider)) {
							Debug.Log ("still collison, delete path");

							_spanningTree.RemoveAt (i);
							i--;
							continue;
						}
						#endregion
					}
				}

                // room1 above room0
                else if (room0.Position.y < room1.Position.y) {
					// room0 left to room1
					if (room0.Position.x <= room1.Position.x) {
						startPoint.Set (room0.Position.x, room0.rectMaxY);
						turnPoint.Set (room0.Position.x, room0.Position.y + yDistance);
						endPoint.Set (room1.rectMinX, room1.Position.y);

						#region Check If Line Collider With Other Room
						if (LineHasCollision (startPoint, turnPoint, room0.Collider)
						    ||
						    LineHasCollision (turnPoint, endPoint, room1.Collider)) {
							// go other way
							startPoint.Set (room0.rectMaxX, room0.Position.y);
							turnPoint.Set (room0.Position.x + xDistance, room0.Position.y);
							endPoint.Set (room1.Position.x, room1.rectMinY);

							Debug.Log ("go other way");
						}

						// still has collision, delete this segment from spanning tree
						if (LineHasCollision (startPoint, turnPoint, room0.Collider)
						    ||
						    LineHasCollision (turnPoint, endPoint, room1.Collider)) {
							Debug.Log ("still collision, delete path");
							_spanningTree.RemoveAt (i);
							i--;
							continue;
						}
						#endregion
					}
                    // room1 left to room0
                    else if (room0.Position.x > room1.Position.x) {
						startPoint.Set (room1.rectMaxX, room1.Position.y);
						turnPoint.Set (room1.Position.x + xDistance, room1.Position.y);
						endPoint.Set (room0.Position.x, room0.rectMaxY);

						#region Check If Line Collider With Other Room
						if (LineHasCollision (startPoint, turnPoint, room1.Collider)
						    ||
						    LineHasCollision (turnPoint, endPoint, room0.Collider)) {
							// go other way
							startPoint.Set (room1.Position.x, room1.rectMinY);
							turnPoint.Set (room1.Position.x, room1.Position.y - yDistance);
							endPoint.Set (room0.rectMinX, room0.Position.y);

							Debug.Log ("go other way");
						}

						// still has collision, delete this segment from spanning tree
						if (LineHasCollision (startPoint, turnPoint, room1.Collider)
						    ||
						    LineHasCollision (turnPoint, endPoint, room0.Collider)) {
							Debug.Log ("still collision, delete path");
							_spanningTree.RemoveAt (i);
							i--;
							continue;
						}
						#endregion
					}
				}

				// create vertical line
				LHallWay hallway = new LHallWay (startPoint, turnPoint, endPoint, room0, room1);
				_hallways.Add (hallway);
				room0.AddHallWay (hallway);

				Debug.Log ("##Lshape created hallway from " + startPoint + " to " + turnPoint + " to " + endPoint + " between " + room0.name + "  " + room1.name);
			}
			#endregion
		}

		#endregion

		#region Remove Physics
		for (int i = 0; i < rooms.Count; i++)
			rooms [i].RemovePhysics ();
		#endregion

		#region Create Tiles
		for (int i = 0; i < rooms.Count; i++) {
			rooms [i].Fill ();
			yield return new WaitForSeconds (0.2f);
		}
		//for (int i = 0; i < _hallways.Count; i++) {
		//	_hallways [i].Fill ();
		//	yield return new WaitForSeconds (0.2f);
		//}
		#endregion
	}


	void OnDrawGizmos ()
	{
		Gizmos.color = Color.red;
		if (_points != null) {
			for (int i = 0; i < _points.Count; i++) {
				Gizmos.DrawSphere (_points [i], 0.2f);
			}
		}
		if (_spanningTree != null && ShowSpanningTree) {
			Gizmos.color = Color.green;
			for (int i = 0; i < _spanningTree.Count; i++) {
				LineSegment seg = _spanningTree [i];
				Vector2 left = (Vector2)seg.p0;
				Vector2 right = (Vector2)seg.p1;
				Gizmos.DrawLine ((Vector3)left, (Vector3)right);
			}
		}

		if (_hallways != null) {
			Gizmos.color = Color.yellow;
			for (int i = 0; i < _hallways.Count; i++) {
				if (_hallways [i] is LHallWay) {
					LHallWay hallway = (LHallWay)_hallways [i];
					Gizmos.DrawLine (hallway.startPos, hallway.turnPos);
					Gizmos.DrawLine (hallway.turnPos, hallway.endPos);
				} else
					Gizmos.DrawLine (_hallways [i].startPos, _hallways [i].endPos);
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
		if (_delaunayTriangulation != null && ShowTriangulation) {
			for (int i = 0; i < _delaunayTriangulation.Count; i++) {
				Vector2 left = (Vector2)_delaunayTriangulation [i].p0;
				Vector2 right = (Vector2)_delaunayTriangulation [i].p1;
				Gizmos.DrawLine ((Vector3)left, (Vector3)right);
			}
		}
	}

	public Vector2 GetRandPointInRadius (float radius)
	{
		return Random.insideUnitCircle * radius;
	}

	public Room CreateRandRoom ()
	{
		// keep it odd
		int randWidth = Random.Range (2, 7);
		randWidth += randWidth + 1;
		int randHeight = Random.Range (2, 7);
		randHeight += randHeight + 1;

		Vector2 pos = Grid.instance.RandCellInRange (randWidth / 2 + 1, Grid.width - randWidth / 2 - 1, randHeight / 2 + 1, Grid.height - randHeight / 2 - 1).position;
		GameObject roomObj = Instantiate (roomPrefab, pos, Quaternion.identity) as GameObject;
		Room room = roomObj.GetComponent<Room> ();

		// random pos,size,id
		room.SetSize (randWidth, randHeight);
		room.SetPosition (pos);
		room.ID = roomId++;

		roomObj.name = roomId.ToString ();

		return room;
	}

	Rect GetMinRect (List<Room> rects)
	{
		Vector2 min = Vector2.zero;
		Vector2 max = Vector2.zero;

		for (int i = 0; i < rects.Count; i++) {
			min.x = Mathf.Min (min.x, rects [i].rectMinX);
			min.y = Mathf.Min (min.y, rects [i].rectMinY);

			max.x = Mathf.Max (max.x, rects [i].rectMaxX);
			max.y = Mathf.Max (max.y, rects [i].rectMaxY);
		}

		float centerX = min.x + (max.x - min.x) / 2;
		float centerY = min.y + (max.y - min.y) / 2;
		float width = max.x - min.x;
		float height = max.y - min.y;

		return new Rect (centerX, centerY, width, height);
	}

	Rect GetMinRect (Room[] rects)
	{
		Vector2 min = Vector2.zero;
		Vector2 max = Vector2.zero;

		for (int i = 0; i < rects.Length; i++) {
			min.x = Mathf.Min (min.x, rects [i].rectMinX);
			min.y = Mathf.Min (min.y, rects [i].rectMinY);

			max.x = Mathf.Max (max.x, rects [i].rectMaxX);
			max.y = Mathf.Max (max.y, rects [i].rectMaxY);
		}

		float centerX = min.x + (max.x - min.x) / 2;
		float centerY = min.y + (max.y - min.y) / 2;
		float width = max.x - min.x;
		float height = max.y - min.y;

		return new Rect (centerX, centerY, width, height);
	}

	public Room GetRoomFromPos (Vector2? pos)
	{
		for (int i = 0; i < rooms.Count; i++) {
			if (rooms [i].Position == pos)
				return rooms [i];
		}

		return null;
	}


	public bool LineHasCollision (Vector2 startPoint, Vector2 endPoint, Collider2D ignoreCollider)
	{
		float dis = Vector2.Distance (startPoint, endPoint);
		Vector2 dir = (endPoint - startPoint).normalized;
		RaycastHit2D hit2d = Physics2D.Raycast (startPoint, dir, dis);

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

	protected Cell[] cells;

	public HallWay (Vector2 start, Vector2 end, Room room0, Room room1)
	{
		this.startPos = start;
		this.endPos = end;
		this.room0 = room0;
		this.room1 = room1;

		int size = 0;

		// vertical
		if (start.x == end.x)
			size = (int)(Mathf.Abs (endPos.y - startPos.y));

		// horizontal
		else
			size = (int)(Mathf.Abs (endPos.x - startPos.x));
		
		cells = new Cell[size];
	}

	public virtual void Fill ()
	{
		GameObject floorPrefab = Resources.Load<GameObject> ("Floor");
		Vector2 pos = startPos;

		float xDir = endPos.x - startPos.x;
		float yDir = endPos.y - startPos.y;

		//for (int i = 0; i < cells.GetLength (0); i++) {
		for (int j = 0; j < cells.Length - 1; j++) {

			// direction of the hall way
			if (xDir < 0)
				pos.Set (startPos.x - 1 - j, startPos.y);
			else if (xDir > 0)
				pos.Set (startPos.x + 1 + j, startPos.y);
			else if (yDir < 0)
				pos.Set (startPos.x, startPos.y - 1 - j);
			else if (yDir > 0)
				pos.Set (startPos.x, startPos.y + 1 + j);

			GameObject obj = MonoBehaviour.Instantiate (floorPrefab, pos, Quaternion.identity) as GameObject;
		}
		//}
	}
}

public class LHallWay : HallWay
{
	public Vector2 turnPos;

	private Cell[] cells1;
	private Cell[] cells2;

	public LHallWay (Vector2 start, Vector2 turn, Vector2 end, Room room0, Room room1) : base (start, end, room0, room1)
	{
		this.turnPos = turn;

		int size1 = 0;
		int size2 = 0;

		// size 1
		// vertical
		if (startPos.x == turnPos.x)
			size1 = (int)(Mathf.Abs (turnPos.y - startPos.y));

		// horizontal
		else
			size1 = (int)(Mathf.Abs (turnPos.x - startPos.x));
		

		// size 2 
		// vertical
		if (turnPos.x == endPos.x)
			size2 = (int)(Mathf.Abs (endPos.y - turnPos.y));

		// horizontal
		else
			size2 = (int)(Mathf.Abs (endPos.x - turnPos.x));


		cells1 = new Cell[size1];
		cells2 = new Cell[size2];
	}

	public override void Fill ()
	{
		GameObject floorPrefab = Resources.Load<GameObject> ("Floor");
		Vector2 pos = startPos;

		float xDir = turnPos.x - startPos.x;
		float yDir = turnPos.y - startPos.y;

		//for (int i = 0; i < cells.GetLength (0); i++) {
		for (int j = 0; j < cells1.Length - 1; j++) {

			// direction of the hall way
			if (xDir < 0)
				pos.Set (startPos.x - 1 - j, startPos.y);
			else if (xDir > 0)
				pos.Set (startPos.x + 1 + j, startPos.y);
			else if (yDir < 0)
				pos.Set (startPos.x, startPos.y - 1 - j);
			else if (yDir > 0)
				pos.Set (startPos.x, startPos.y + 1 + j);

			GameObject obj = MonoBehaviour.Instantiate (floorPrefab, pos, Quaternion.identity) as GameObject;
		}

		xDir = endPos.x - turnPos.x;
		yDir = endPos.y - turnPos.y;

		for (int j = 0; j < cells2.Length - 1; j++) {

			// direction of the hall way
			if (xDir < 0)
				pos.Set (turnPos.x - 1 - j, turnPos.y);
			else if (xDir > 0)
				pos.Set (turnPos.x + 1 + j, turnPos.y);
			else if (yDir < 0)
				pos.Set (turnPos.x, turnPos.y - 1 - j);
			else if (yDir > 0)
				pos.Set (turnPos.x, turnPos.y + 1 + j);

			GameObject obj = MonoBehaviour.Instantiate (floorPrefab, pos, Quaternion.identity) as GameObject;
		}
	}
}
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DungeonGenerator : MonoBehaviour
{
	public GameObject roomPrefab;

	private List<Room>rooms;

	void Start ()
	{
		rooms = new List<Room> ();

		StartCoroutine (Create ());
	}

	IEnumerator Create ()
	{
		int i = 0;

		while (i < 10) {

			// create random size room at random pos
			Vector2 randPoint = GetRandPointInRadius (5);
			Room room = CreateRandomSizeRoom (randPoint);
			rooms.Add (room);
			i ++;

			Debug.Log ("new room");

			yield return new WaitForSeconds (0.1f);
		}

		foreach (Room room in rooms) {
			room.RemovePhysics ();
		}
	}

	public Vector2 GetRandPointInRadius (float radius)
	{
		return Random.insideUnitCircle * radius;
	}

	public Room CreateRandomSizeRoom (Vector2 pos)
	{
		int randWidth = Random.Range (2, 10);
		int randHeight = Random.Range (2, 10);

		GameObject roomObj = Instantiate (roomPrefab, pos, Quaternion.identity) as GameObject;
		Room room = roomObj.GetComponent<Room> ();
		room.SetSize (randWidth, randHeight);
		room.SetPosition (pos);

		return room;
	}

}


	

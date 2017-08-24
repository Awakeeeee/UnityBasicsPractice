using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralCave : MonoBehaviour 
{
	[Header("Map")]
	public int width;
	public int height;
	public int outerWallThickness = 5;

	[Header("Randomize")]
	public bool autoRandomSeed;
	public string seed;
	public int basicMapSmoothTime;
	public int removeRegionSmoothTime = 1;
	[Range(0, 100)]public int fillPercentage;

	[Header("Region")]
	[Tooltip("Any region that has tile less than this number will be removed - turn 0 to 1 or 1 to 0.")]
	public int smallRegionThreshold = 10;

	private int[,] cave;	//save 1 and 0, 1 indicates fill/wall, 0 indicates empty/passable
	private MarchingSquareMeshGenerator msGenerator;

	void Start()
	{
		msGenerator = GetComponent<MarchingSquareMeshGenerator>();
		GenerateCave();
	}

	void Update()
	{
		if(Input.GetKeyDown(KeyCode.J))
		{
			GenerateCave();
		}
	}

	void GenerateCave()
	{
		//initial fill
		cave = new int[width, height];
		RandomlyFillCave();

		//smooth the cave several times
		for(int i = 0; i < basicMapSmoothTime; i++)
		{
			SmoothCave();
		}
		SmoothRegion();

		//more thickness on outline wall
		int[,] thickCave = new int[width + outerWallThickness * 2, height + outerWallThickness * 2];
		for(int x = 0; x < thickCave.GetLength(0); x++)
		{
			for(int y = 0; y < thickCave.GetLength(1); y++)
			{
				if(x < outerWallThickness || x >= width + outerWallThickness || y < outerWallThickness || y >= height + outerWallThickness)	//note x>=width + outer NOT x>=width. I made mistake here
				{
					thickCave[x,y] = 1;
				}else{
					thickCave[x,y] = cave[x - outerWallThickness, y - outerWallThickness];
				}
			}
		}

		//draw the crazy mesh
		if(msGenerator != null)
		{
			//msGenerator.GenerateMesh(cave, 1);
			msGenerator.GenerateMesh(thickCave, 1);
		}
	}

	void RandomlyFillCave()
	{
		if(autoRandomSeed)
		{
			seed = Random.value.ToString();
		}

		System.Random ranPicker = new System.Random(seed.GetHashCode());

		for(int x = 0; x < width; x++)
		{
			for(int y = 0; y < height; y++)
			{
				if(x == 0 || x == width - 1 || y == 0 || y == height - 1)
				{
					cave[x,y] = 1;
				}else
				{
					cave[x, y] = ranPicker.Next(0, 100) > fillPercentage ? 0 : 1;	
				}
			}
		}
	}

	//loop throught every tile, if most of its surroundings are walls, turn itself a wall, vise versa
	//finally each passable area and walls looks 'grouped'
	void SmoothCave()
	{
		for(int x = 0; x < width; x++)
		{
			for(int y = 0; y < height; y++)
			{
				int wallCount = GetSurroundingWallCount(x,y);
				if(wallCount > 4)
				{
					cave[x,y] = 1;
				}else if(wallCount < 4)
				{
					cave[x,y] = 0;
				}
				//else leave as what it is, this is important for the outcome
			}
		}
	}

	int GetSurroundingWallCount(int cx, int cy)
	{
		int wallCount = 0;

		for(int x = cx - 1; x < cx + 2; x++)
		{
			for(int y = cy - 1; y < cy + 2; y++)
			{
				if(IsInMapBound(x, y))	//check if this is a valid tile index, handle argument out of bound error
				{
					if(x != cx || y != cy)
					{
						wallCount += cave[x, y];
					}	
				}else	//if this is an out of bound tile index, just plus one, encourage the edge to be wall
				{
					wallCount ++;
				}
			}
		}

		return wallCount;
	}

	bool IsInMapBound(int x, int y)
	{
		return x >= 0 && x < width && y >= 0 && y < height;
	}

	//Flood-Fill algorithm, think it as paint bucket tool
	//Ingredients: 1.target tile 2.result list 3.checking queque 4.checked list
	List<Coord> FloodFillGetRegion(Coord startCoord)
	{
		int targetTile = cave[startCoord.x, startCoord.y];	//In case of paint tool bucket, this maybe pixel color, in here it is either wall(1) or pass(0)
		List<Coord> region = new List<Coord>();	//result list
		Queue<Coord> openList = new Queue<Coord>();
		bool[,] mapCheckedMarks = new bool[width, height];	//mark which coordinate has been checked. Checked means it is already in openlist

		//start
		openList.Enqueue(startCoord);
		mapCheckedMarks[startCoord.x, startCoord.y] = true;
		//loop
		while(openList.Count > 0)
		{
			Coord checkCoord = openList.Dequeue();
			region.Add(checkCoord);

			for(int x = checkCoord.x - 1; x < checkCoord.x + 2; x++)
			{
				for(int y = checkCoord.y - 1; y < checkCoord.y + 2; y++)
				{
					if(!IsInMapBound(x, y))
						continue;

					if(x != checkCoord.x && y != checkCoord.y)	//dont check diagonal coord
						continue;

					if(x == checkCoord.x && y == checkCoord.y)	//dont check itself
						continue;

					if(mapCheckedMarks[x,y] == true)
						continue;

					if(cave[x, y] != targetTile)
						continue;

					Coord newCoord = new Coord(x, y);
					openList.Enqueue(newCoord);
					mapCheckedMarks[newCoord.x, newCoord.y] = true;
				}
			}
		}

		return region;
	}

	///Use flood fill get region to find all regions of the map, a region is a list of tiles, all regions is a list of region tile list.
	List<List<Coord>> FindAllRegions()
	{
		List<List<Coord>> regions = new List<List<Coord>>();
		bool[,] checkedCoord = new bool[width, height];

		for(int x = 0; x < width; x++)
		{
			for(int y = 0; y < height; y++)
			{
				Coord targetCoord = new Coord(x, y);

				if(checkedCoord[targetCoord.x, targetCoord.y] == false)	//new region found
				{
					List<Coord> newRegion = FloodFillGetRegion(targetCoord);
					for(int i = 0; i < newRegion.Count; i++)	//mark all coord in this region as checked
					{
						checkedCoord[newRegion[i].x, newRegion[i].y] = true;
					}
					regions.Add(newRegion);
				}
			}
		}

		return regions;
	}

	///Find the 'small' regions and remove them from map. Find room regions and connect them.
	void SmoothRegion()
	{
		List<List<Coord>> regions = new List<List<Coord>>();
		List<Room> allRooms = new List<Room>();

		//remove small regions
		for(int s = 0; s < removeRegionSmoothTime; s++)
		{
			regions = FindAllRegions();

			for(int i = 0; i < regions.Count; i++)
			{
				List<Coord> checkRegion = regions[i];

				if(checkRegion.Count < smallRegionThreshold)
				{
					//decide type
					int regionType = cave[checkRegion[0].x, checkRegion[0].y];
					int targetType = -1;
					if(regionType == 1)
					{
						targetType = 0;
					}else if(regionType == 0)
					{
						targetType = 1;
					}else{
						Debug.LogError("Unexpected region tile type.");
					}

					//remove(turn 1to0 and 0to1)
					for(int c = 0; c < checkRegion.Count; c++)
					{
						cave[checkRegion[c].x, checkRegion[c].y] = targetType;
					}
				}
			}
		}

		//find room and connect them
		for(int i = 0; i < regions.Count; i++)
		{
			List<Coord> checkRegion = regions[i];
			if(checkRegion.Count >= smallRegionThreshold)
			{
				if(cave[checkRegion[0].x, checkRegion[0].y] == 0)	//this is room region
				{
					Room room = new Room(checkRegion, cave);
					allRooms.Add(room);
				}
			}
		}
		ConnectClosestRooms(allRooms);
	}

	//Loop through rooms, for each room, find its Closest room and make a passway to that room
	void ConnectClosestRooms(List<Room> allRooms)
	{
		int passwayDist = 0;
		bool possiblePasswayFound = false;
		Coord passwayCoord1 = new Coord();
		Coord passwayCoord2 = new Coord();
		Room passwayRoom1 = new Room();
		Room passwayRoom2 = new Room();

		allRooms.Sort();	//List.Sort() uses IComparable.CompareTo()
		Room mainRoom = allRooms[allRooms.Count - 1];
		mainRoom.isMainRoom = true;	//find main room is prepared for ConnectInaccessibleRoomsToMainRoom()
		mainRoom.isAccessibleToMainRoom = true;
		Debug.DrawLine(CoordToWorldPosition(mainRoom.roomRegion[0], 1f), CoordToWorldPosition(mainRoom.roomRegion[1], 1f), Color.cyan, 100f);	//distinguish main room

		//pick 2 rooms to check
		for(int r1 = 0; r1 < allRooms.Count; r1++)
		{
			for(int r2 = 0; r2 < allRooms.Count; r2++)
			{
				if(r1 == r2)	//dont check itself to itself
					continue;

				Room room1 = allRooms[r1];
				Room room2 = allRooms[r2];

				if(room1.connectedRooms.Count > 0)	//meaning room1 has already got a passway, so break - check next room1 (think that they are all finally serve room1)
				{
					break;
				}

				//pick 2 coordinates from 2 rooms
				for(int c1 = 0; c1 < room1.roomOutline.Count; c1++)
				{
					for(int c2 = 0; c2 < room2.roomOutline.Count; c2++)
					{
						Coord coord1 = room1.roomOutline[c1];
						Coord coord2 = room2.roomOutline[c2];
						int distance = (int)(Mathf.Pow(coord1.x - coord2.x, 2) + Mathf.Pow(coord1.y - coord2.y, 2));

						if(distance < passwayDist || !possiblePasswayFound)
						{
							passwayDist = distance;
							possiblePasswayFound = true;

							passwayCoord1 = coord1;
							passwayCoord2 = coord2;
							passwayRoom1 = room1;
							passwayRoom2 = room2;
						}
					}
				}
			}

			//for a room1, if it finds its CLOEST ROOM and finds A PASSWAY to that, make that passway
			if(possiblePasswayFound)
			{
				MakePassway(passwayRoom1, passwayRoom2, passwayCoord1, passwayCoord2);
				possiblePasswayFound = false;	//rest for next search
			}
		}

		//after connect closest rooms, several nearby rooms may form 'community', communities may not connect to each other, this method adds extra connections to ensure overal main-room-accessible
		ConnectInaccessibleRoomsToMainRoom(allRooms);
	}

	///Recursively find main-room-accessible rooms list and inaccessible room list. Each iteration find the closest ia-room and a-room, and connect them.
	///Recursively call itself until the inaccessible room list counts 0.
	void ConnectInaccessibleRoomsToMainRoom(List<Room> allRooms)
	{
		int passwayDist = 0;
		bool possiblePasswayFound = false;
		Coord passwayCoordOnInaccessibleRoom = new Coord();
		Coord passwayCoordOnAccessibleRoom = new Coord();
		Room bestInaccessibleRoom = new Room();
		Room bestAccessibleRoom = new Room();

		//The two lists are updated in every iteration of this method itself, when inaccessibleRooms.Count = 0, the recursive call ends
		List<Room> accessibleRooms = new List<Room>();
		List<Room> inaccessibleRooms = new List<Room>();
		for(int i = 0; i < allRooms.Count; i++)
		{
			if(allRooms[i].isAccessibleToMainRoom)
			{
				accessibleRooms.Add(allRooms[i]);
			}else{
				inaccessibleRooms.Add(allRooms[i]);
			}
		}

		//Loop through every ia-rooms and a-rooms, to find a shortest path from an a-room, to an ia-room
		for(int ia = 0; ia < inaccessibleRooms.Count; ia++)
		{
			for(int a = 0; a < accessibleRooms.Count; a++)
			{
				if(ia == a)
					continue;
				
				Room roomIA = inaccessibleRooms[ia];
				Room roomA = accessibleRooms[a];

				if(roomIA.isAccessibleToMainRoom)
					continue;

				for(int c1 = 0; c1 < roomIA.roomOutline.Count; c1++)
				{
					for(int c2 = 0; c2 < roomA.roomOutline.Count; c2++)
					{
						Coord coordIA = roomIA.roomOutline[c1];
						Coord coordA = roomA.roomOutline[c2];
						int distance = (int)(Mathf.Pow(coordIA.x - coordA.x, 2) + Mathf.Pow(coordIA.y - coordA.y, 2));

						if(distance < passwayDist || !possiblePasswayFound)
						{
							passwayDist = distance;
							possiblePasswayFound = true;

							passwayCoordOnInaccessibleRoom = coordIA;
							passwayCoordOnAccessibleRoom = coordA;
							bestInaccessibleRoom = roomIA;
							bestAccessibleRoom = roomA;
						}
					}
				}
			}
		}
		//This is after both loops:
		//Every time call this method, only ONE passway is made, then recall itself to make the next one
		if(possiblePasswayFound)
		{
			MakePassway(bestInaccessibleRoom, bestAccessibleRoom, passwayCoordOnInaccessibleRoom, passwayCoordOnAccessibleRoom);
			ConnectInaccessibleRoomsToMainRoom(allRooms);
		}
	}

	void MakePassway(Room r1, Room r2, Coord c1, Coord c2)
	{
		Room.MarkTwoRoomsConnected(r1, r2);
		Vector3 pos1 = CoordToWorldPosition(c1, 0.5f);
		Vector3 pos2 = CoordToWorldPosition(c2, 0.5f);
		Debug.DrawLine(pos1, pos2, Color.yellow, 100f);
	}

	///return the world position of this coordinate when Size = 1
	Vector3 CoordToWorldPosition(Coord c, float y = 0.5f)
	{
		return new Vector3(-width / 2f + c.x, y, -height / 2f + c.y);	//TODO should I +0.5f on X and Z?
	}

	void OnDrawGizmos()
	{
//---------Display method 1. just use map data----------------
//		if(cave == null)
//			return;
//
//		for(int x = 0; x < width; x++)
//		{
//			for(int y = 0; y < height; y++)
//			{
//				Gizmos.color = cave[x,y] == 1 ? Color.black : Color.white;
//				Vector3 tileCenter = new Vector3(-width/2f + x + 0.5f, 0f, -height/2f + y + 0.5f);	//so that the center of map is 0,0,0
//				Gizmos.DrawCube(tileCenter, Vector3.one);
//			}
//		}
	}
}

public struct Coord
{
	public int x;
	public int y;

	public Coord(int _x, int _y)
	{
		x = _x;
		y = _y;
	}
}

/// A region of passway tiles(0) is a room
public class Room : System.IComparable<Room>
{
	public List<Coord> roomRegion;
	public List<Coord> roomOutline;
	public List<Room> connectedRooms;	//if 2 rooms has a passway between them, they are connected
	public int roomSize;

	public bool isMainRoom = false;	//main room is the biggest room
	public bool isAccessibleToMainRoom = false;

	public Room()
	{}

	public Room(List<Coord> region, int[,] map)
	{
		roomRegion = region;
		roomOutline = new List<Coord>();
		connectedRooms = new List<Room>();
		roomSize = roomRegion.Count;

		for(int i = 0; i < roomRegion.Count; i++)
		{
			Coord checkCoord = roomRegion[i];
			//map points in a room are all 0, to build an outline point list, I must check the surrounding point of each room point
			for(int x = checkCoord.x - 1; x <= checkCoord.x + 1; x++)
			{
				for(int y = checkCoord.y - 1; y <= checkCoord.y + 1; y++)
				{
					if(x == checkCoord.x || y == checkCoord.y)	//dont check diagonal coord
					{
						if(map[x, y] == 1)
							roomOutline.Add(checkCoord);
					}
				}
			}
		}
	}

	//Check if this room is connected with another room
	public bool IsConnectedWith(Room other)
	{
		return this.connectedRooms.Contains(other);
	}

	public static void MarkTwoRoomsConnected(Room a, Room b)
	{
		if(a.isAccessibleToMainRoom)
		{
			b.MarkRoomAccessibleToMainRom();
		}else if(b.isAccessibleToMainRoom)
		{
			a.MarkRoomAccessibleToMainRom();
		}
		a.connectedRooms.Add(b);
		b.connectedRooms.Add(a);
	}

	//Called when 2 rooms are connected
	//If a room is accessible to main room, its connected room is also accessible to main room. 
	//BUT note that connect means 'direct connect': 1 ct(connected to) 2, 2 ct 3, once 1 ct main, then 2 ct main, 3 NOT ct main 
	public void MarkRoomAccessibleToMainRom()
	{
		if(!isAccessibleToMainRoom)
		{
			isAccessibleToMainRoom = true;
			foreach(Room r in connectedRooms)
			{
				r.isAccessibleToMainRoom = true;
			}
		}
	}

	public int CompareTo(Room other)
	{
		return this.roomSize.CompareTo(other.roomSize);
	}
}
